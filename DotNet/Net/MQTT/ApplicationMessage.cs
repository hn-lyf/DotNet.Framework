using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// 应用消息
    /// </summary>
    public class ApplicationMessage : TopicMessage
    {
        /// <summary>
        /// 发送的数据
        /// </summary>dss
        public byte[] Data { get; set; }
        /// <summary>
        /// 消息文本
        /// </summary>
        public string Text { get { return System.Text.Encoding.UTF8.GetString(Data); } }
    }
}
