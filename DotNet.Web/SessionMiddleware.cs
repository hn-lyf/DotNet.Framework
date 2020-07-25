using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Web
{
    /// <summary>
    /// 自定义session，支持toket
    /// </summary>
    public class SessionMiddleware
    {
        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();
        private const int SessionKeyLength = 64; // "382c74c3-721d-4f34-80e5-57657b6cbc27"
        private static readonly Func<bool> ReturnTrue = () => true;
        private readonly RequestDelegate _next;
        private readonly SessionOptions _options;
        private readonly ILogger _logger;
        private readonly ISessionStore _sessionStore;
        private readonly IDataProtector _dataProtector;
        /// <summary>
        /// Creates a new <see cref="SessionMiddleware"/>.
        /// </summary>
        /// <param name="next">The <see cref="RequestDelegate"/> representing the next middleware in the pipeline.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> representing the factory that used to create logger instances.</param>
        /// <param name="dataProtectionProvider">The <see cref="IDataProtectionProvider"/> used to protect and verify the cookie.</param>
        /// <param name="sessionStore">The <see cref="ISessionStore"/> representing the session store.</param>
        /// <param name="options">The session configuration options.</param>
        public SessionMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider,
            ISessionStore sessionStore,
            IOptions<SessionOptions> options)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (dataProtectionProvider == null)
            {
                throw new ArgumentNullException(nameof(dataProtectionProvider));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory.CreateLogger<SessionMiddleware>();
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(SessionMiddleware));
            _options = options.Value;
            _sessionStore = sessionStore ?? throw new ArgumentNullException(nameof(sessionStore));
        }

        public async Task Invoke(HttpContext context)
        {
            var isNewSessionKey = false;
            Func<bool> tryEstablishSession = ReturnTrue;
            var headerName = DotNet.Configuration.Config.GetSection("Session:HeaderName").Value ?? _options.Cookie.Name;
            var cookieValue = context.Request.Headers[headerName].ToString();
            
            if (string.IsNullOrWhiteSpace(cookieValue))
            {
                cookieValue = context.Request.Cookies[_options.Cookie.Name];
            }
            var sessionKey = CookieProtection.Unprotect(_dataProtector, cookieValue, _logger);
            if (string.IsNullOrWhiteSpace(sessionKey) || sessionKey.Length != SessionKeyLength)
            {
                // No valid cookie, new session.
                var guidBytes = new byte[16];
                CryptoRandom.GetBytes(guidBytes);
                sessionKey = $"{new Guid(guidBytes):n}{Guid.NewGuid():n}";
                cookieValue = CookieProtection.Protect(_dataProtector, sessionKey);
                var establisher = new SessionEstablisher(context, cookieValue, _options);
                tryEstablishSession = establisher.TryEstablishSession;
                isNewSessionKey = true;
            }

            var feature = new SessionFeature
            {
                Session = _sessionStore.Create(sessionKey, _options.IdleTimeout, _options.IOTimeout, tryEstablishSession, isNewSessionKey)
            };
            feature.Session.SetString("SessionKey", sessionKey);
            context.Features.Set<ISessionFeature>(feature);
            try
            {
                await _next(context);
            }
            finally
            {
                context.Features.Set<ISessionFeature>(null);

                if (feature.Session != null)
                {
                    try
                    {
                        await feature.Session.CommitAsync(context.RequestAborted);
                    }
                    catch (OperationCanceledException)
                    {
                        // _logger.SessionCommitCanceled();
                    }
                    catch (Exception)
                    {
                        //_logger.ErrorClosingTheSession(ex);
                    }
                }
            }
        }
        private class SessionEstablisher
        {
            private readonly HttpContext _context;
            private readonly string _cookieValue;
            private readonly SessionOptions _options;
            private bool _shouldEstablishSession;
            private readonly string HeaderName;

            public SessionEstablisher(HttpContext context, string cookieValue, SessionOptions options)
            {
                _context = context;
                _cookieValue = cookieValue;
                _options = options;
                context.Response.OnStarting(OnStartingCallback, state: this);
                HeaderName = DotNet.Configuration.Config.GetSection("Session:HeaderName").Value ?? _options.Cookie.Name;
            }

            private static Task OnStartingCallback(object state)
            {
                var establisher = (SessionEstablisher)state;
                if (establisher._shouldEstablishSession)
                {
                    establisher.SetCookie();
                }
                return Task.FromResult(0);
            }

            private void SetCookie()
            {
                var cookieOptions = _options.Cookie.Build(_context);
                cookieOptions.IsEssential = true;//始终设置为重要，要不然可能获取不到
                var response = _context.Response;
                response.Cookies.Append(_options.Cookie.Name, _cookieValue, cookieOptions);

                var responseHeaders = response.Headers;
                responseHeaders[HeaderNames.CacheControl] = "no-cache";
                responseHeaders[HeaderNames.Pragma] = "no-cache";
                responseHeaders[HeaderNames.Expires] = "-1";
                responseHeaders[HeaderName] = _cookieValue;
            }

            // Returns true if the session has already been established, or if it still can be because the response has not been sent.
            internal bool TryEstablishSession()
            {
                return (_shouldEstablishSession |= !_context.Response.HasStarted);
            }
        }
    }
    /// <summary>
    /// 自定义Session扩展
    /// </summary>
    public static class DotNetSessionMiddlewareExtensions
    {
        /// <summary>
        /// 启用自定义Session。
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMySession(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionMiddleware>();
        }
    }
    internal static class CookieProtection
    {
        internal static string Protect(IDataProtector protector, string data)
        {
            if (protector == null)
            {
                throw new ArgumentNullException(nameof(protector));
            }
            if (string.IsNullOrEmpty(data))
            {
                return data;
            }

            var userData = Encoding.UTF8.GetBytes(data);

            var protectedData = protector.Protect(userData);
            return Convert.ToBase64String(protectedData).TrimEnd('=');
        }

        internal static string Unprotect(IDataProtector protector, string protectedText, ILogger logger)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            try
            {
                if (string.IsNullOrEmpty(protectedText))
                {
                    return string.Empty;
                }

                var protectedData = Convert.FromBase64String(Pad(protectedText));
                if (protectedData == null)
                {
                    return string.Empty;
                }

                var userData = protector.Unprotect(protectedData);
                if (userData == null)
                {
                    return string.Empty;
                }

                return Encoding.UTF8.GetString(userData);
            }
            catch (Exception)
            {
                // Log the exception, but do not leak other information
                //logger.ErrorUnprotectingSessionCookie(ex);
                return string.Empty;
            }
        }

        private static string Pad(string text)
        {
            var padding = 3 - ((text.Length + 3) % 4);
            if (padding == 0)
            {
                return text;
            }
            return text + new string('=', padding);
        }
    }
}
