using System;
using System.Collections.Generic;
using System.Text;
using DotNet.Linq;
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
        public string Text { get { return Data.GetString(); } set { Data = value.ToBytes(); } }
        /// <summary>
        /// 克隆一个消息应用
        /// </summary>
        /// <returns></returns>
        public virtual ApplicationMessage Clone()
        {
            return new ApplicationMessage() { ClientId = this.ClientId, Data = Data, Identifier = Identifier, QoS = QoS, RemoteEndPoint = RemoteEndPoint, Topic = Topic };
        }
        /// <summary>
        /// 将消息转换成<see cref="MQTTDataPackage"/>标准协议包。
        /// </summary>
        /// <returns></returns>
        public virtual MQTTDataPackage ToPackage()
        {
            MQTTDataPackage package = new MQTTDataPackage() { MessageType = MessageType.Publish, QoS = QoS };
            List<byte> bytes = new List<byte>();
            var topicBytes = this.Topic.ToBytes(Encoding.ASCII);
            bytes.Add((byte)(topicBytes.Length >> 8));
            bytes.Add((byte)(topicBytes.Length & 255));
            bytes.AddRange(topicBytes);
            if (QoS > 0)
            {
                bytes.Add((byte)(this.Identifier >> 8));
                bytes.Add((byte)(Identifier & 255));
            }
            bytes.AddRange(Data);
            package.Data = bytes.ToArray();
            bytes.Clear();
            return package;
        }
    }
}
