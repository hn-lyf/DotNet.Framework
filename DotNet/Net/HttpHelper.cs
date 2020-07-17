using DotNet.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace DotNet.Net
{
    /// <summary>
    /// 对<see cref="HttpWebRequest"/>的一系列封装。
    /// </summary>
    public class HttpHelper
    {

        /// <summary>
        /// 使用默认的<see cref="HttpHelper"/>类。
        /// </summary>
        public static HttpHelper Default { get; } = new HttpHelper();
        private WebHeaderCollection defaultHeaders;
        private string m_LastUrl;
        /// <summary>
        /// 获取或设置一个值，该值表示响应的字符集。
        /// </summary>
        public string CharacterSet;
        private string m_BaseUri;
        /// <summary>
        /// 获取或设置 User-agentHTTP 标头的值。
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36";
        /// <summary>
        /// 指定客户端为向服务器验证自身身份而出示的凭据。
        /// </summary>
        public string Authorization { get; set; }
        /// <summary>
        ///  Cookie 管理类。
        /// </summary>
        public CookieContainer Cookies { get; set; }


        private X509Certificate m_ClientCertificate;
        /// <summary>
        /// 获取或设置一个值，该值表示上次访问的url。
        /// </summary>
        public virtual string LastUrl { get { return m_LastUrl; } set { m_LastUrl = value; } }
        /// <summary>
        /// 设置或设置当前请求的客户端证书。
        /// </summary>
        public X509Certificate ClientCertificate { get => m_ClientCertificate; set => m_ClientCertificate = value; }
        /// <summary>
        /// 获取或设置一个值，该值表示基础的url。
        /// </summary>
        public string BaseUri { get => m_BaseUri; set => m_BaseUri = value; }
        /// <summary>
        /// 默认头信息
        /// </summary>
        public WebHeaderCollection DefaultHeaders { get => defaultHeaders; set => defaultHeaders = value; }
        /// <summary>
        /// 添加指定名称和值到默认头信息中。
        /// </summary>
        /// <param name="header">头信息</param>
        /// <param name="value">头值</param>
        public virtual void AddHeader(HttpRequestHeader header, string value)
        {
            DefaultHeaders.Add(header, value);
        }
        /// <summary>
        /// 添加指定名称和值到默认头信息中。
        /// </summary>
        /// <param name="name">头名称</param>
        /// <param name="value">头值</param>
        public virtual void AddHeader(string name, string value)
        {
            DefaultHeaders.Add(name, value);
        }
        /// <summary>
        /// 使用基础的url初始化。
        /// </summary>
        /// <param name="baseUri">基础的url</param>
        public HttpHelper(string baseUri = "")
        {
            this.CharacterSet = "utf-8";
            this.Cookies = new CookieContainer();
            this.m_LastUrl = string.Empty;
            defaultHeaders = new WebHeaderCollection();
            if (!string.IsNullOrEmpty(baseUri) && !baseUri.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) && !baseUri.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
            {
                baseUri = string.Format("{0}{1}", "http://", baseUri);
            }
            this.m_BaseUri = baseUri;
        }
        /// <summary>
        /// 像当前Cookie管理端添加Cookie。
        /// </summary>
        /// <param name="name">Cookie 名称</param>
        /// <param name="value">Cookie 值。</param>
        public virtual void AddCookie(string name, string value)
        {
            this.AddCookie(new Uri(this.BaseUri), name, value);
        }
        /// <summary>
        /// 像指定的url的Cookie管理端添加Cookie。
        /// </summary>
        /// <param name="url">要添加的url。</param>
        /// <param name="name">Cookie 名称</param>
        /// <param name="value">Cookie 值。</param>
        public virtual void AddCookie(Uri url, string name, string value)
        {
            this.Cookies.Add(url, new Cookie(name, value));
        }
        /// <summary>
        /// 获取完整的url。
        /// </summary>
        /// <param name="url">url地址。</param>
        /// <returns></returns>
        public virtual Uri GetUrl(string url = "")
        {
            if (!url.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) && !url.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
            {
                url = string.Format("{0}{1}", this.BaseUri, url);
                if (string.IsNullOrWhiteSpace(this.BaseUri))
                {
                    url = string.Format("{0}{1}", "http://", url);
                }
            }
            return new Uri(url);
        }
        /// <summary>
        /// 根据url创建请求类。
        /// </summary>
        /// <param name="url">url请求地址。</param>
        /// <param name="headers">设置一些请求的头。</param>
        /// <typeparam name="H">请求头类型</typeparam>
        /// <returns></returns>
        protected virtual HttpWebRequest CreateRequest<H>(string url, H headers)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(GetUrl(url));
            //request.Timeout=
            request.UserAgent = UserAgent;
            request.Referer = LastUrl;
            if (!string.IsNullOrEmpty(Authorization))
            {
                request.Headers[HttpRequestHeader.Authorization] = Authorization;
            }
            if (DefaultHeaders != null)
            {
                foreach (string key in DefaultHeaders)
                {
                    request.Headers[key] = DefaultHeaders[key];
                }
            }
            if (headers != null)
            {
                foreach (var key in headers.ToEnumerable())
                {
                    request.Headers[key.Key] = key.Value?.ToString();
                }
            }
            request.CookieContainer = Cookies;
            if (ClientCertificate != null)
            {
                request.ClientCertificates.Add(ClientCertificate);
            }
            return request;
        }

        /// <summary>
        /// 根据参数类型创建参数。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected virtual string GetParameter<T>(T parameter)
        {
            if (parameter is string)
            {
                return (parameter as string);
            }
            if (parameter == null)
            {
                return string.Empty;
            }
            return parameter.ToEnumerable().Join(item => $"{item.Key}={item.Value?.ToString().UrlEncode()}", "&");
        }
        /// <summary>
        /// get请求一个指定的url，并返回<see cref="HttpResult"/>。
        /// </summary>
        /// <param name="url">url请求地址。</param>
        /// <returns></returns>
        public virtual HttpResult Get(string url)
        {
            return Get<string, string>(url, null);
        }
        /// <summary>
        /// get请求一个指定的url，并返回<see cref="HttpResult"/>。
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="url">url请求地址。</param>
        /// <param name="parameters">要请求的参数</param>
        /// <returns></returns>
        public virtual HttpResult Get<P>(string url, P parameters)
        {
            return Get<P, string>(url, parameters);
        }
        /// <summary>
        /// 异步获取,get请求一个指定的url
        /// </summary>
        /// <typeparam name="T">要设置的头</typeparam>
        /// <param name="url">要请求的url地址</param>
        /// <param name="resultAction">请求完成后执行的委托</param>
        ///  <param name="headers">要设置的头</param>
        public virtual void AsyncGet<T>(string url, Action<HttpWebRequest, HttpResult> resultAction, T headers)
        {
            HttpWebRequest request = CreateRequest(url, headers);
            request.ContentType = "text/html";
            request.Method = "GET";
            request.BeginGetResponse(GetResponseCallback, new object[] { request, resultAction });
        }
        /// <summary>
        ///  异步获取,get请求一个指定的url
        /// </summary>
        /// <param name="url">要获取的url</param>
        /// <param name="resultAction">请求完成后返回的方法。</param>
        public virtual void AsyncGet(string url, Action<HttpWebRequest, HttpResult> resultAction)
        {
            AsyncGet<string>(url, resultAction, null);//AsyncHttpResult
        }
        /// <summary>
        /// 异步post请求指定网页。
        /// </summary>
        /// <param name="url">url地址。</param>
        /// <param name="data">要填写到post的数据。</param>
        /// <param name="resultAction">请求完成后的委托。</param>
        public virtual void AsyncPost<T>(string url, T data, Action<HttpWebRequest, HttpResult> resultAction)
        {
            AsyncPost<T, string>(url, data, resultAction, null);
        }
        /// <summary>
        /// 异步post请求指定网页。
        /// </summary>
        /// <typeparam name="T">请求的参数类型</typeparam>
        /// <typeparam name="H">设置头的类型</typeparam>
        /// <param name="url">url地址。</param>
        /// <param name="data">要填写到post的数据。</param>
        /// <param name="resultAction">请求完成后的委托。</param>
        /// <param name="headers">要设置的头</param>
        public virtual void AsyncPost<T, H>(string url, T data, Action<HttpWebRequest, HttpResult> resultAction, H headers)
        {
            HttpWebRequest request = CreateRequest(url, headers);
            request.ContentType = request.ContentType ?? "application/x-www-form-urlencoded;charset=utf-8";
            request.Method = "POST";
            request.BeginGetRequestStream(GetRequestStreamCallback, new object[] { request, data, resultAction });
        }
        /// <summary>
        /// 获取写入流的异步
        /// </summary>
        /// <param name="ar"></param>
        protected virtual void GetRequestStreamCallback(IAsyncResult ar)
        {
            var array = ar.AsyncState as object[];
            var request = array[0] as HttpWebRequest;
            var data = array[1];
            var resultAction = array[2] as Action<HttpWebRequest, HttpResult>;
            HttpResult result = new HttpResult();
            try
            {
                using (Stream requestStream = request.EndGetRequestStream(ar))
                {
                    byte[] postData = null;
                    if (data is byte[] bytes)
                    {
                        postData = bytes;
                    }
                    else
                    {
                        postData = Encoding.UTF8.GetBytes(GetParameter(data));
                    }
                    requestStream.Write(postData, 0, postData.Length);
                    postData = null;
                }
                result = GetResult(request);//偷个懒 这里就不异步执行了
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }
            resultAction?.Invoke(request, result);
        }
        /// <summary>
        /// <see cref="HttpWebRequest.BeginGetResponse"/>的回调函数。
        /// </summary>
        /// <param name="ar"></param>
        protected virtual void GetResponseCallback(IAsyncResult ar)
        {
            var array = ar.AsyncState as object[];
            var request = array[0] as HttpWebRequest;
            var resultAction = array[1] as Action<HttpWebRequest, HttpResult>;
            HttpResult result = new HttpResult();
            try
            {
                using (HttpWebResponse response = request.EndGetResponse(ar) as HttpWebResponse)
                {
                    result = ResponseToResult(response);
                }
            }
            catch (WebException exception)
            {
                if (exception.Response is HttpWebResponse)
                {
                    result.StatusCode = (exception.Response as HttpWebResponse).StatusCode;
                }
                result.Error = exception.Message;
            }
            catch (Exception exception2)
            {
                result.Error = exception2.Message;
            }
            resultAction?.Invoke(request, result);
        }
        /// <summary>
        /// get请求一个指定的url，并返回<see cref="HttpResult"/>。
        /// </summary>
        /// <typeparam name="P">参数类型</typeparam>
        /// <typeparam name="T">设置头的类型</typeparam>
        /// <param name="url">url请求地址。</param>
        /// <param name="parameters">请求参数</param>
        /// <param name="headers">设置一些请求的头。</param>
        /// <returns></returns>
        public virtual HttpResult Get<P, T>(string url, P parameters = default, T headers = default)
        {
            if (!parameters.IsNull())
            {
                if (url.Contains("?"))
                {
                    url += GetParameter(parameters);
                }
                else
                {
                    url += "?" + GetParameter(parameters);
                }
            }
            HttpWebRequest request = CreateRequest(url, headers);
            request.ContentType = "text/html";
            request.Method = "GET";
            return this.GetResult(request);
        }
        /// <summary>
        /// post 请求一个指定的url，并返回<see cref="HttpResult"/>。
        /// </summary>
        /// <typeparam name="T">请求的参数类型</typeparam>
        /// <param name="url">url请求地址。</param>
        /// <param name="value">post提交的数据。</param>
        /// <param name="accept">Accept HTTP 标头的值。</param>
        /// <param name="contentType">设置请求类型</param>
        /// <returns></returns>
        public HttpResult Post<T>(string url, T value, string accept = "application/json", string contentType = "application/x-www-form-urlencoded;charset=utf-8")
        {
            return Post<T, string>(url, value, accept, contentType, null);
        }
        /// <summary>
        /// post 请求一个指定的url，并返回<see cref="HttpResult"/>。
        /// </summary>
        /// <typeparam name="T">请求的参数类型</typeparam>
        /// <typeparam name="H">设置头的类型</typeparam>
        /// <param name="url">url请求地址。</param>
        /// <param name="value">post提交的数据。</param>
        /// <param name="accept">Accept HTTP 标头的值。</param>
        /// <param name="contentType">设置请求类型</param>
        /// <param name="headers">设置一些请求的头。</param>
        /// <returns></returns>
        public HttpResult Post<T, H>(string url, T value, string accept = "application/json", string contentType = "application/x-www-form-urlencoded;charset=utf-8", H headers = default)
        {
            HttpWebRequest request = CreateRequest(url, headers);
            request.Method = "POST";
            request.Accept = request.Accept ?? accept;
            request.ContentType = contentType;
            byte[] postData;
            if (value is byte[])
            {
                postData = value as byte[];
            }
            else
            {
                postData = Encoding.UTF8.GetBytes(GetParameter(value));
            }
            return GetResult(request, postData);
        }
        /// <summary>
        /// 上传文件到指定url
        /// </summary>
        /// <typeparam name="T">请求的参数类型</typeparam>
        /// <typeparam name="H">设置头的类型</typeparam>
        /// <param name="url">url请求地址。</param>
        /// <param name="value">post提交的数据。</param>
        /// <param name="fileName">上传的文件名。</param>
        /// <param name="formName">文件域名称。</param>
        /// <param name="fileBytes">要上传的文件数据</param>
        /// <param name="fileContentType">要上传的文件Content-Type类型</param>
        /// <param name="accept">Accept HTTP 标头的值。</param>
        /// <param name="headers">设置一些请求的头。</param>
        /// <returns></returns>
        public HttpResult UploadFile<T, H>(string url, T value, string fileName, string formName, byte[] fileBytes, string fileContentType, string accept = "application/json", H headers = default)
        {
            HttpWebRequest request = CreateRequest(url, headers);
            string boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.Accept = request.Accept ?? accept;
            boundary = "--" + boundary;
            byte[] postBytes = null;
            if (fileContentType.IsNull())
            {
                fileContentType = MimeMapping.GetMimeMapping(fileName);
            }
            using (System.IO.MemoryStream memoryStream = new MemoryStream())
            {
                foreach (var key in value.ToEnumerable())
                {
                    memoryStream.Write(Encoding.UTF8.GetBytes(string.Format("{0}\r\nContent-Disposition: form-data; name=\"{1}\";\r\n\r\n{2}\r\n", boundary, key.Key, key.Value)));
                }
                memoryStream.Write(Encoding.UTF8.GetBytes(string.Format("{0}\r\nContent-Disposition: form-data; name=\"{1}\";filename=\"{2}\"\r\nContent-Type:{3}\r\n\r\n", boundary, formName, fileName, fileContentType)));
                memoryStream.Write(fileBytes, 0, fileBytes.Length);
                memoryStream.Write(Encoding.UTF8.GetBytes(string.Format("\r\n{0}--\r\n", boundary)));
                postBytes = memoryStream.ToArray();
            }
            return GetResult(request, postBytes);
        }
        /// <summary>
        /// 上传文件到指定url
        /// </summary>
        /// <typeparam name="T">请求的参数类型</typeparam>
        /// <param name="url">url请求地址。</param>
        /// <param name="value">post提交的数据。</param>
        /// <param name="fileName">上传的文件名。</param>
        /// <param name="formName">文件域名称。</param>
        /// <param name="fileBytes">要上传的文件数据</param>
        /// <param name="fileContentType">要上传的文件Content-Type类型</param>
        /// <param name="accept">Accept HTTP 标头的值。</param>
        /// <returns></returns>
        public HttpResult UploadFile<T>(string url, T value, string fileName, string formName, byte[] fileBytes, string fileContentType, string accept = "javascript/json")
        {
            return UploadFile<T, string>(url, value, fileName, formName, fileBytes, fileContentType, accept, null);
        }
        /// <summary>
        /// 根据请求对象<paramref name="httpWebRequest"/>创建请求并返回<see cref="HttpResult"/>。
        /// </summary>
        /// <param name="httpWebRequest">请求对象。</param>
        /// <param name="postBytes">要写入请求的内容</param>
        /// <returns></returns>
        protected virtual HttpResult GetResult(HttpWebRequest httpWebRequest, byte[] postBytes = null)
        {
            HttpResult result = new HttpResult();
            try
            {
                if (postBytes != null)
                {
                    httpWebRequest.ContentLength = postBytes.LongLength;
                    using (Stream reqStream = httpWebRequest.GetRequestStream())
                    {
                        reqStream.Write(postBytes, 0, postBytes.Length);
                        reqStream.Dispose();
                    }
                }
                using (HttpWebResponse response = httpWebRequest.GetResponse() as HttpWebResponse)
                {
                    result = ResponseToResult(response);

                }
                result.Message = "访问成功";
                result.Success = true;
            }
            catch (WebException exception)
            {
                if (exception.Response is HttpWebResponse errorResponse)
                {
                    result.StatusCode = errorResponse.StatusCode;
                    var tempResult = ResponseToResult(errorResponse);
                    result.Error = tempResult.Html;
                }
                else
                {
                    result.Error = exception.Message;
                }

            }
            catch (Exception exception2)
            {
                result.Error = exception2.Message;
            }
            return result;
        }
        /// <summary>
        /// 将<see cref="HttpWebResponse"/>对象转换为<see cref="HttpResult"/>结果。
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        protected virtual HttpResult ResponseToResult(HttpWebResponse response)
        {
            HttpResult result = new HttpResult();
            using (response)
            {
                Cookies.Add(new Uri(string.Format("{0}://{1}", response.ResponseUri.Scheme, response.ResponseUri.Authority)), response.Cookies);
                result.StatusCode = response.StatusCode;

                Cookies.Add(response.Cookies);
                string characterSet = response.CharacterSet;
                result.ContentType = response.ContentType;
                if (!string.IsNullOrEmpty(characterSet))
                {
                    result.CharacterSet = characterSet;
                }
                using (var stream = response.GetResponseStream())
                {
                    if (response.ContentEncoding != null && response.ContentEncoding.StartsWith("gzip"))
                    {
                        using (System.IO.Compression.GZipStream gZipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
                        {
                            result.Bytes = stream.ReadBytes();
                        }
                    }
                    else
                    {
                        result.Bytes = stream.ReadBytes();
                    }
                }
                result.Message = "访问成功";
                result.Success = true;
            }
            return result;
        }
    }
}
