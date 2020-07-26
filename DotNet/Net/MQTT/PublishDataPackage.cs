using System;
using System.Collections.Generic;
using System.Text;
using DotNet.Linq;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// 发布消息包。
    /// </summary>
    public class PublishDataPackage : TopicDataPackage
    {
        /// <summary>
        /// 初始化mqtt连接包
        /// </summary>
        /// <param name="package"></param>
        public PublishDataPackage(MQTTDataPackage package) : base(package)
        {

        }
        /// <summary>
        /// 初始化连接包
        /// </summary>
        public PublishDataPackage() : base(MessageType.Publish)
        {

        }
        /// <summary>
        /// 发送的数据
        /// </summary>dss
        public byte[] BodyBytes { get; set; }
        /// <summary>
        /// 消息文本
        /// </summary>
        public string Text { get { return BodyBytes.GetString(); } set { BodyBytes = value.ToBytes(); } }
        /// <summary>
        /// 初始化数据。
        /// </summary>
        protected override void Init()
        {
            var index = 2;
            var topicLength = Data[0] << 8 | Data[1];
            Topic = System.Text.ASCIIEncoding.UTF8.GetString(Data, 2, topicLength);
            index += topicLength;
            if (QoS > 0)
            {
                Identifier = (ushort)(Data[index] << 8 | Data[++index]);
                index++;
            }
            BodyBytes = new byte[Data.Length - index];
            Array.Copy(Data, index, BodyBytes, 0, BodyBytes.Length);
        }
        /// <summary>
        /// 开始组装包。
        /// </summary>
        protected override void Packaging()
        {
            var topicBytes = Topic.ToBytes();//主题数据
            Data = new byte[topicBytes.Length + BodyBytes.Length + (QoS > 0 ? 4 : 2)];
            Data[0] = (byte)(topicBytes.Length >> 8);
            Data[1] = (byte)(topicBytes.Length & 255);
            topicBytes.CopyTo(Data, 2);
            if (QoS > 0)
            {
                Data[topicBytes.Length + 2] = (byte)(Identifier >> 8);
                Data[topicBytes.Length + 3] = (byte)(Identifier & 255);
            }
            BodyBytes.CopyTo(Data, Data.Length - BodyBytes.Length);//复制消息内容
            topicBytes = null;
        }
        /// <summary>
        /// 克隆自己
        /// </summary>
        /// <returns></returns>
        public virtual PublishDataPackage Clone()
        {
            return new PublishDataPackage(this);
        }
    }
}
