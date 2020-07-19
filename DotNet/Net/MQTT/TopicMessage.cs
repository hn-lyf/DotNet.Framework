using System.Collections.Generic;
using System.Text;
using DotNet.Linq;

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
        /// <summary>
        ///服务质量等级
        /// </summary>
        public int QoS { get; set; }
        /// <summary>
        /// 消息序号
        /// </summary>
        public ushort Identifier { get; set; }
        /// <summary>
        /// 判断两个对象的主题和qos是否相等。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var message = obj as TopicMessage;
            return message.Topic == this.Topic && message.QoS == this.QoS;
        }
        /// <summary>
        /// 用作特定类型的哈希函数。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Topic.GetHashCode() + QoS.GetHashCode();
        }
        /// <summary>
        /// 将消息转换成<see cref="MQTTDataPackage"/>标准协议包。
        /// </summary>
        /// <returns></returns>
        public virtual MQTTDataPackage ToPackage(bool subscribe)
        {
            MQTTDataPackage package = new MQTTDataPackage() { MessageType = subscribe ? MessageType.Subscribe : MessageType.UnSubscribe, QoS = QoS };
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)(this.Identifier >> 8));
            bytes.Add((byte)(Identifier & 255));
            var topicBytes = this.Topic.ToBytes(Encoding.ASCII);
            bytes.Add((byte)(topicBytes.Length >> 8));
            bytes.Add((byte)(topicBytes.Length & 255));
            bytes.AddRange(topicBytes);
            bytes.Add((byte)(QoS));
            package.Data = bytes.ToArray();
           
            bytes.Clear();
            return package;
        }
    }
}
