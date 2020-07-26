using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Linq;
using DotNet.Web.Apps.Models;
using DotNet.Web.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace DotNet.Web.Apps.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class SwaggerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly SwaggerOptions _options;

        private readonly TemplateMatcher _requestMatcher;

        public SwaggerMiddleware(RequestDelegate next, SwaggerOptions options)
        {
            _next = next;
            _options = (options ?? new SwaggerOptions());
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(_options.RouteTemplate), new RouteValueDictionary());
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (RequestingSwaggerDocument(httpContext.Request, out string documentName))
            {
                if (!httpContext.GetLogin<LoginUserInfo>().Success)
                {
                    httpContext.Response.ContentType = "application/json;charset=utf-8";
                    httpContext.Response.StatusCode = 401;
                   await httpContext.Response.WriteAsync(LoginAttribute.NoLoginResult.ToJson(), System.Text.Encoding.UTF8);
                    return;
                }

            }
            
           await _next(httpContext);
        }
        private bool RequestingSwaggerDocument(HttpRequest request, out string documentName)
        {
            documentName = null;
            if (request.Method != "GET")
            {
                return false;
            }
            if (request.Path.StartsWithSegments("/help"))
            {
                return true;
            }
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            if (!_requestMatcher.TryMatch(request.Path, routeValueDictionary) || !routeValueDictionary.ContainsKey("documentName"))
            {
                return false;
            }

            documentName = routeValueDictionary["documentName"].ToString();
            return true;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class SwaggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseSwaggerMiddleware(this IApplicationBuilder app, Action<SwaggerOptions> setupAction = null)
        {
            SwaggerOptions swaggerOptions = new SwaggerOptions();
            if (setupAction != null)
            {
                setupAction(swaggerOptions);
            }
            else
            {
                swaggerOptions = app.ApplicationServices.GetRequiredService<IOptions<SwaggerOptions>>().Value;
            }

            app.UseMiddleware<SwaggerMiddleware>(new object[1]
            {
                swaggerOptions
            });
            return app;
        }
    }
}
