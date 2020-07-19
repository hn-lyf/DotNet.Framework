using System;
using System.Collections.Generic;
using System.Text;
using DotNet.Linq;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// 主题包。
    /// </summary>
    public class TopicDataPackage : MQTTDataPackage
    {
        /// <summary>
        /// 初始化mqtt连接包
        /// </summary>
        /// <param name="package"></param>
        public TopicDataPackage(MQTTDataPackage package) : base(package)
        {
            Init();//
        }
        /// <summary>
        /// 初始化连接包
        /// </summary>
        /// <param name="message">协议类型</param>
        public TopicDataPackage(MessageType message)
        {
            this.MessageType = message;
        }
        /// <summary>
        /// 订阅主题  
        /// </summary>
        public string Topic { get; set; }
        /// <summary>
        /// 服务质量要求
        /// </summary>
        public Qos RequestedQoS { get; set; }
        /// <summary>
        /// 初始化内容。
        /// </summary>
        protected virtual void Init()
        {
            var index = 0;
            Identifier = (ushort)(Data[0] << 8 | Data[1]);
            var topicLength = Data[2] << 8 | Data[3];
            Topic = System.Text.ASCIIEncoding.UTF8.GetString(Data, 4, topicLength);
            index += 4 + topicLength;
            if (Data.Length > index)
            {
                RequestedQoS = (Qos)Data[index];
            }
        }
        /// <summary>
        /// 开始组装包。
        /// </summary>
        protected override void Packaging()
        {
            var topicBytes = Topic.ToBytes();

            Data = new byte[topicBytes.Length + (MessageType == MessageType.UnSubscribe ? 4 : 5)];
            Data[0] = (byte)(Identifier >> 8);
            Data[1] = (byte)(Identifier & 255);
            Data[2] = (byte)(topicBytes.Length >> 8);
            Data[3] = (byte)(topicBytes.Length & 255);
            topicBytes.CopyTo(Data, 4);
            if (MessageType != MessageType.UnSubscribe)
            {
                Data[Data.Length - 1] = (byte)RequestedQoS;
            }
            topicBytes = null;
        }
    }
}
