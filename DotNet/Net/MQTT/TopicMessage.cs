namespace DotNet.Net.MQTT
{
    /// <summary>
    /// 主题消息
    /// </summary>
    public class TopicMessage : Message
    {
        /// <summary>
        /// 订阅主题  
        /// </summary>
        public string Topic { get; set; }
    }
}
