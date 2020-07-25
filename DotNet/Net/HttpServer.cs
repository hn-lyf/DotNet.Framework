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
        public void Start(string uriPrefix = "http://+:8089/httpapi/")
        {
            listener.Prefixes.Add(uriPrefix);
            listener.Start();
            listener.BeginGetContext(ContextResult, listener);
        }

        public virtual Dictionary<string, string> StaticFiles { get; } = new Dictionary<string, string>();
        public virtual Result AddStaticFile(string uriPrefix, string path)
        {
            if (!uriPrefix.StartsWith('/'))
            {
                return new Result() { Message = $"参数{nameof(uriPrefix)}必须以/开头。" };
            }
            StaticFiles.Add(uriPrefix, path);
            return true;
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
                var request = new HttpRequest(context);
                using (var response = context.Response)
                {
                    var staticFile = StaticFiles.FirstOrDefault((item) => request.Url.AbsolutePath.StartsWith(item.Key, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(staticFile.Value))//静态资源
                    {
                        var path = staticFile.Value;

                    }
                    context.Response.AddHeader("Content-type", "application/json;charset=utf-8");//添加响应头信息
                    context.Response.ContentEncoding = Encoding.UTF8;
                    DotNet.Result result = new DotNet.Result() { Message = "系统错误" };
                    try
                    {
                        result = OnBeginRequest(request);
                    }
                    catch (Exception ex)
                    {
                        result = new DotNet.Result() { Message = "系统错误:" + ex.Message };
                    }
                    finally
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(context.Response.OutputStream))
                        {
                            sw.Write(result.ToJson());
                        }
                    }
                    response.Close();
                }
            }
        }
        /// <summary>
        /// 当有新的请求连接过来时。
        /// </summary>
        /// <param name="request">http请求。</param>
        /// <returns></returns>
        protected virtual Result OnBeginRequest(HttpRequest request)
        {
            return false;
        }

    }
}
