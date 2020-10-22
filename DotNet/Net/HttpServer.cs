using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using DotNet.Linq;

namespace DotNet.Net
{
    /// <summary>
    /// 一个简单的http服务器。
    /// </summary>
    public class HttpServer
    {
        private System.Net.HttpListener listener = new System.Net.HttpListener();
        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="uriPrefix"></param>
        public void Start(string uriPrefix = "http://+:8089/")
        {
            listener.Prefixes.Add(uriPrefix);
            listener.Start();
            listener.BeginGetContext(ContextResult, listener);
        }
        /// <summary>
        /// 静态资源
        /// </summary>
        public virtual Dictionary<string, string> StaticFiles { get; } = new Dictionary<string, string>();
        /// <summary>
        /// 处理程序
        /// </summary>
        public virtual Dictionary<string, Type> ApiHandles { get; } = new Dictionary<string, Type>();
        /// <summary>
        /// 默认网站工作目录。
        /// </summary>
        public virtual string WorkPath { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "wwwroot");
        /// <summary>
        /// 添加静态资源访问。
        /// </summary>
        /// <param name="uriPrefix">静态资源前缀</param>
        /// <param name="path">物理路径。</param>
        /// <returns></returns>
        public virtual Result AddStaticFile(string uriPrefix, string path)
        {
            if (!uriPrefix.StartsWith("/"))
            {
                return new Result() { Message = $"参数{nameof(uriPrefix)}必须以/开头。" };
            }
            StaticFiles.Add(uriPrefix, path);
            return true;
        }
        /// <summary>
        /// 添加处理程序。
        /// </summary>
        /// <param name="uriPrefix"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual Result AddHandle(string uriPrefix, Type handle)
        {
            if (!uriPrefix.StartsWith("/"))
            {
                return new Result() { Message = $"参数{nameof(uriPrefix)}必须以/开头。" };
            }
            ApiHandles.Add(uriPrefix, handle);
            return true;
        }
        /// <summary>
        /// 添加处理程序
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual Result AddHandle(Type handle)
        {
            return AddHandle($"/{handle.Name.Replace("Handle", "")}/", handle);
        }
        private void ContextResult(IAsyncResult ar)
        {
            System.Net.HttpListener listener = ar.AsyncState as System.Net.HttpListener;
            listener.BeginGetContext(ContextResult, listener);
            HttpListenerContext context = null;
            try
            {
                context = listener.EndGetContext(ar);
            }
            catch
            {

            }
            if (context != null)
            {
                OnRequest(new HttpRequest(context));
            }
        }
        /// <summary>
        /// 当收到请求时发生。
        /// </summary>
        /// <param name="request"></param>
        protected virtual void OnRequest(HttpRequest request)
        {
            using (var response = request.Context.Response)
            {
                var apiHandle = ApiHandles.FirstOrDefault((item) => request.Url.AbsolutePath.StartsWith(item.Key, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(apiHandle.Key))//api处理器
                {
                    response.AddHeader("Content-type", "application/json;charset=utf-8");//添加响应头信息
                    response.ContentEncoding = Encoding.UTF8;
                    DotNet.Result result = new DotNet.Result() { Message = "系统错误" };
                    try
                    {
                        result = OnBeginRequest(request, apiHandle);
                    }
                    catch (Exception ex)
                    {
                        result = new DotNet.Result() { Message = "系统错误:" + ex.Message };
                    }
                    finally
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(response.OutputStream))
                        {
                            sw.Write(result.ToJson());
                        }
                    }
                    response.Close();
                    return;
                }
                var staticFile = StaticFiles.FirstOrDefault((item) => request.Url.AbsolutePath.StartsWith(item.Key, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(staticFile.Value))//静态资源
                {
                    var path = staticFile.Value;
                    var urlPath = request.Url.AbsolutePath.Remove(0, staticFile.Key.Length);
                    if (!string.IsNullOrEmpty(urlPath))
                    {
                        urlPath = urlPath.Remove(0, 1).Replace('/', System.IO.Path.DirectorySeparatorChar);
                    }
                    WriteStaticFile(request, urlPath, staticFile.Value);
                    return;
                }
                else
                {
                    WriteStaticFile(request, request.Url.AbsolutePath.Remove(0, 1), WorkPath);
                    return;
                }

            }
        }
        /// <summary>
        /// 输出静态资源
        /// </summary>
        /// <param name="request"></param>
        /// <param name="urlPath"></param>
        /// <param name="path"></param>
        protected virtual void WriteStaticFile(HttpRequest request, string urlPath, string path)
        {
            var filePath = System.IO.Path.Combine(path, urlPath);
            if (System.IO.Directory.Exists(filePath))
            {
                filePath = System.IO.Path.Combine(filePath, "index.html");
                if (System.IO.File.Exists(filePath))
                {
                    var url = $"{System.IO.Path.Combine(request.Url.AbsolutePath, "index.html")}{request.Url.Query}";
                    request.Context.Response.Redirect(url);
                    request.Context.Response.Close();
                    return;
                }
            }
            if (System.IO.File.Exists(filePath))
            {
                var mimeMapping = MimeMapping.GetMimeMapping(filePath);
                request.Context.Response.AddHeader("Content-type", $"{mimeMapping};charset=utf-8");
                using (System.IO.FileStream fileStream = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    fileStream.CopyTo(request.Context.Response.OutputStream);
                }
                request.Context.Response.Close();
                return;
            }
            request.Context.Response.StatusCode = 404;
            request.Context.Response.Close();
            return;
        }
        /// <summary>
        /// 当有新的请求连接过来时
        /// </summary>
        /// <param name="request"></param>
        /// <param name="apiHandle"></param>
        /// <returns></returns>
        protected virtual Result OnBeginRequest(HttpRequest request, KeyValuePair<string, Type> apiHandle)
        {
            var httpApiHandle = Activator.CreateInstance(apiHandle.Value) as IHttpApiHandle;
            if (httpApiHandle != null)
            {
                httpApiHandle.OnRequest(request);
                var method = apiHandle.Value.GetMethod(request.Url.Segments[request.Url.Segments.Length - 1], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    var parametersObj = new object[parameters.Length];
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        var parameterVal = request[parameter.Name];
                        if (!string.IsNullOrEmpty(parameterVal))
                        {

                            if (parameter.ParameterType == typeof(string))
                            {
                                parametersObj[i] = parameterVal;
                            }
                            else if (parameter.ParameterType.IsPrimitive)
                            {
                                parametersObj[i] = parameterVal.ChangeType(parameter.ParameterType);
                            }
                            else
                            {
                                parametersObj[i] = Newtonsoft.Json.JsonConvert.DeserializeObject(parameterVal, parameter.ParameterType);
                            }
                            
                        }
                        else
                        {
                            parametersObj[i] = parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null;
                        }
                    }
                    var obj = method.Invoke(httpApiHandle, parametersObj);
                    if (obj is Result result1)
                    {
                        return result1;
                    }
                    return new Result<object>() { Data = obj, Success = true };
                }
            }
            return false;
        }

    }
}
