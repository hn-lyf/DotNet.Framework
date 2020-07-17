using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace DotNet.Net
{
    /// <summary>
    /// HTTP 资源的二次封装
    /// </summary>
    public class HttpRequest
    {
        private NameValueCollection from;
        readonly HttpListenerContext m_Context = null;
        private readonly HttpFileCollection files = new HttpFileCollection();
        /// <summary>
        /// 获取表示客户端对资源的请求的 System.Net.HttpListenerRequest。
        /// </summary>
        protected HttpListenerRequest Request { get { return m_Context.Request; } }
        /// <summary>
        /// 获取文件上传的列表。
        /// </summary>
        public HttpFileCollection Files
        {
            get
            {
                LoadFrom();
                return files;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public HttpListenerContext Context { get { return m_Context; } }
        static byte[] ToByteArray(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }
        static int IndexOf(byte[] searchWithin, byte[] serachFor, int startIndex)
        {
            int index = 0;
            int startPos = Array.IndexOf(searchWithin, serachFor[0], startIndex);

            if (startPos != -1)
            {
                while ((startPos + index) < searchWithin.Length)
                {
                    if (searchWithin[startPos + index] == serachFor[index])
                    {
                        index++;
                        if (index == serachFor.Length)
                        {
                            return startPos;
                        }
                    }
                    else
                    {
                        startPos = Array.IndexOf<byte>(searchWithin, serachFor[0], startPos + index);
                        if (startPos == -1)
                        {
                            return -1;
                        }
                        index = 0;
                    }
                }
            }

            return -1;
        }
        /// <summary>
        /// 加载form内容。
        /// </summary>
        protected virtual void LoadFrom()
        {
            if (from == null)
            {
                from = new NameValueCollection();
                if (Request.HasEntityBody)
                {
                    if (Request.ContentLength64 > 0)
                    {
                        if (Request.ContentType.StartsWith("multipart/form-data"))
                        {
                            byte[] data = ToByteArray(Request.InputStream);
                            string content = Encoding.UTF8.GetString(data);
                            int delimiterEndIndex = content.IndexOf("\r\n");
                            if (delimiterEndIndex > -1)
                            {
                                string delimiter = content.Substring(0, content.IndexOf("\r\n"));
                                string[] sections = content.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string s in sections)
                                {

                                    if (s.Contains("Content-Disposition"))
                                    {
                                        Match nameMatch = new Regex(@"(?<=name\=\"")(.*?)(?=\"")").Match(s);
                                        string name = nameMatch.Value.Trim().ToLower();

                                        Regex re = new Regex(@"(?<=Content\-Type:)(.*?)(?=\r\n\r\n)");
                                        Match contentTypeMatch = re.Match(content);
                                        // Look for filename
                                        re = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")");
                                        Match filenameMatch = re.Match(s);
                                        if (contentTypeMatch.Success && filenameMatch.Success)
                                        {
                                            HttpPostedFile postedFile = new HttpPostedFile
                                            {
                                                ContentType = contentTypeMatch.Value.Trim(),
                                                FileName = filenameMatch.Value.Trim()
                                            };

                                            // Get the start & end indexes of the file contents
                                            int startIndex = contentTypeMatch.Index + contentTypeMatch.Length + "\r\n\r\n".Length;

                                            byte[] delimiterBytes = Encoding.UTF8.GetBytes("\r\n" + delimiter);
                                            int endIndex = IndexOf(data, delimiterBytes, startIndex);

                                            int contentLength = endIndex - startIndex;

                                            // Extract the file contents from the byte array
                                            byte[] fileData = new byte[contentLength];

                                            Buffer.BlockCopy(data, startIndex, fileData, 0, contentLength);

                                            postedFile.Bytes = fileData;
                                            Files[name] = postedFile;
                                        }
                                        else
                                        {
                                            int startIndex = nameMatch.Index + nameMatch.Length + "\r\n\r\n".Length;
                                            from.Add(name, System.Web.HttpUtility.UrlDecode(s.Substring(startIndex).TrimEnd(new char[] { '\r', '\n' }).Trim()));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Request.ContentType.StartsWith("application/x-www-form-urlencoded"))
                            {
                                var bytes = new byte[Request.ContentLength64];
                                int readCount = 0;
                                while (readCount < bytes.Length)
                                {
                                    readCount += Request.InputStream.Read(bytes, readCount, bytes.Length - readCount);
                                }
                                var postTxt = Request.ContentEncoding.GetString(bytes);
                                //按&号分割，然后等号分割
                                var parameters = postTxt.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string parameter in parameters)
                                {
                                    var vals = parameter.Split('=');
                                    from.Add(vals[0], System.Web.HttpUtility.UrlDecode(vals[1]));
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取请求的post资源
        /// </summary>
        public NameValueCollection From
        {
            get
            {
                LoadFrom();
                return from;
            }
        }
        /// <summary>
        /// 初始化对象
        /// </summary>
        /// <param name="context"></param>
        public HttpRequest(HttpListenerContext context)
        {
            m_Context = context;
        }
        /// <summary>
        ///  获取客户端请求的 URL 信息（不包括主机和端口）。
        /// </summary>
        public string RawUrl { get { return Request.RawUrl; } }
        /// <summary>
        ///  获取一个 System.Boolean 值，该值指示客户端是否请求持续型连接。
        /// </summary>
        public bool KeepAlive { get { return Request.KeepAlive; } }
        /// <summary>
        /// 获取一个 System.Boolean 值，该值指示请求是否有关联的正文数据。
        /// </summary>
        public bool HasEntityBody { get { return Request.HasEntityBody; } }
        /// <summary>
        /// 获取请求客户端使用的 HTTP 版本。
        /// </summary>
        public Version ProtocolVersion { get { return Request.ProtocolVersion; } }
        /// <summary>
        /// 获取随请求发送的 Cookie。
        /// </summary>
        public CookieCollection Cookies { get { return Request.Cookies; } }
        /// <summary>
        /// 获取客户端请求的 System.Net.TransportContext。
        /// </summary>
        public TransportContext TransportContext { get { return Request.TransportContext; } }
        /// <summary>
        /// 获取一个错误代码，该代码标识的问题涉及客户端提供的 System.Security.Cryptography.X509Certificates.X509Certificate。
        /// </summary>
        public int ClientCertificateError { get { return Request.ClientCertificateError; } }
        /// <summary>
        ///  获取响应的首选自然语言。
        /// </summary>
        public string[] UserLanguages { get { return Request.UserLanguages; } }
        /// <summary>
        /// 获取由客户端指定的 DNS 名称和端口号（如果提供了端口号）。
        /// </summary>
        public string UserHostName { get { return Request.UserHostName; } }
        /// <summary>
        /// 获取请求被定向到的服务器 IP 地址和端口号。
        /// </summary>
        public string UserHostAddress { get { return Request.UserHostAddress; } }
        /// <summary>
        ///  获取客户端提供的用户代理。
        /// </summary>
        public string UserAgent { get { return Request.UserAgent; } }
        /// <summary>
        /// 获取以下资源的统一资源标识符 (URI)，该资源将使客户端与服务器相关。
        /// </summary>
        public Uri UrlReferrer { get { return Request.UrlReferrer; } }
        /// <summary>
        /// 获取客户端请求的 System.Uri 对象。
        /// </summary>
        public Uri Url { get { return Request.Url; } }
        /// <summary>
        /// 获取客户端通过请求发送的服务提供程序名称 (SPN)。
        /// </summary>
        public string ServiceName { get { return Request.ServiceName; } }
        /// <summary>
        /// 获取发出请求的客户端 IP 地址和端口号。
        /// </summary>
        public IPEndPoint RemoteEndPoint { get { return Request.RemoteEndPoint; } }
        /// <summary>
        /// 获取请求被定向到的服务器 IP 地址和端口号。
        /// </summary>
        public IPEndPoint LocalEndPoint { get { return Request.LocalEndPoint; } }
        /// <summary>
        /// 获取一个 System.Boolean 值，该值指示用来发送请求的 TCP 连接是否使用安全套接字层 (SSL) 协议。
        /// </summary>
        public bool IsSecureConnection { get { return Request.IsSecureConnection; } }
        /// <summary>
        /// 获取 System.Boolean 值，该值指示该请求是否来自本地计算机。
        /// </summary>
        public bool IsLocal { get { return Request.IsLocal; } }
        /// <summary>
        /// 获取一个 System.Boolean 值，该值指示发送此请求的客户端是否经过身份验证。
        /// </summary>
        public bool IsAuthenticated { get { return Request.IsAuthenticated; } }
        /// <summary>
        /// 获取包含正文数据的流，这些数据由客户端发送。
        /// </summary>
        public Stream InputStream { get { return Request.InputStream; } }
        /// <summary>
        ///  获取由客户端指定的 HTTP 方法。
        /// </summary>
        public string HttpMethod { get { return Request.HttpMethod; } }
        /// <summary>
        /// 获取在请求中发送的标头名称/值对的集合。
        /// </summary>
        public NameValueCollection Headers { get { return Request.Headers; } }
        /// <summary>
        /// 获取包含在请求中的正文数据的 MIME 类型。
        /// </summary>
        public string ContentType { get { return Request.ContentType; } }
        /// <summary>
        /// 获取包含在请求中的正文数据的长度。
        /// </summary>
        public long ContentLength64 { get { return Request.ContentLength64; } }
        /// <summary>
        /// 获取可用于随请求发送的数据的内容编码
        /// </summary>
        public Encoding ContentEncoding { get { return Request.ContentEncoding; } }
        /// <summary>
        /// 获取客户端接受的 MIME 类型。
        /// </summary>
        public string[] AcceptTypes { get { return Request.AcceptTypes; } }
        /// <summary>
        /// 获取传入的 HTTP 请求的请求标识符。
        /// </summary>
        public Guid RequestTraceIdentifier { get { return Request.RequestTraceIdentifier; } }
        /// <summary>
        /// 获取包含在请求中的查询字符串。
        /// </summary>
        public NameValueCollection QueryString { get { return Request.QueryString; } }

        /// <summary>
        /// 开始对客户端的 X.509 v.3 证书的异步请求。
        /// </summary>
        /// <param name="requestCallback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IAsyncResult BeginGetClientCertificate(AsyncCallback requestCallback, object state)
        {
            return Request.BeginGetClientCertificate(requestCallback, state);
        }
        /// <summary>
        /// 结束对客户端的 X.509 v.3 证书的异步请求。
        /// </summary>
        /// <param name="asyncResult"> 证书的挂起请求。</param>
        /// <returns></returns>
        public X509Certificate2 EndGetClientCertificate(IAsyncResult asyncResult)
        {
            return Request.EndGetClientCertificate(asyncResult);
        }
        /// <summary>
        /// 检索客户端的 X.509 v.3 证书。
        /// </summary>
        /// <returns></returns>
        public X509Certificate2 GetClientCertificate()
        {
            return Request.GetClientCertificate();
        }
        /// <summary>
        /// 使用指定编码对url的请求参数进行解析
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static NameValueCollection EncodeQueryString(Uri uri)
        {
            var ret = new NameValueCollection();
            var q = uri.Query;
            if (q.Length > 0)
            {
                foreach (var p in q.Substring(1).Split('&'))
                {
                    var s = p.Split(new char[] { '=' }, 2);
                    ret.Add(HttpUtility.UrlDecode(s[0]), HttpUtility.UrlDecode(s[1]));
                }
            }
            return ret;
        }
        /// <summary>
        /// 获取所有的请求参数
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string this[string name]
        {
            get
            {
                if (Request.QueryString[name] != null)
                {
                    return EncodeQueryString(Request.Url)[name];
                }
                if (this.From[name] != null)
                {
                    return From[name];
                }
                if (this.Headers[name] != null)
                {
                    return Headers[name];
                }
                return null;
            }
        }
    }
}
