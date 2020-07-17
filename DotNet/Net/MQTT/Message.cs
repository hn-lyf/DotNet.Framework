namespace DotNet.Net.MQTT
{
    /// <summary>
    /// 消息
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// 获取一个值，该值指示为在登录MQTT时的客户端编号 
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 客户端网络地址
        /// </summary>
        public System.Net.EndPoint RemoteEndPoint { get; set; }
    }
}
