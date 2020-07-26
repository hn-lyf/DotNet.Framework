namespace DotNet.Net
{
    /// <summary>
    /// http 简单服务器处理程序
    /// </summary>
    public interface IHttpApiHandle
    {
        /// <summary>
        /// http的请求信息
        /// </summary>
        HttpRequest Request { get; }
        /// <summary>
        /// 开始请求是触发。
        /// </summary>
        /// <param name="request"></param>
        void OnRequest(HttpRequest request);
    }
}
