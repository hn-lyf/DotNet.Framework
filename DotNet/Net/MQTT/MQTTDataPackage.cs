using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// MQTT数据包
    /// </summary>
    public class MQTTDataPackage : IDataPackage
    {
        /// <summary>
        /// 初始化MQTT数据包
        /// </summary>
        public MQTTDataPackage()
        {

        }
        /// <summary>
        /// 初始化MQTT数据包
        /// </summary>
        /// <param name="dataPackage"></param>
        public MQTTDataPackage(MQTTDataPackage dataPackage)
        {
            this.Data = dataPackage.Data;
            this.Header = dataPackage.Header;
            ClientId = dataPackage.ClientId;
            RemoteEndPoint = dataPackage.RemoteEndPoint;
        }
        /// <summary>
        /// 获取一个值，该值指示为在登录MQTT时的客户端编号 
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 客户端网络地址
        /// </summary>
        public System.Net.EndPoint RemoteEndPoint { get; set; }
        /// <summary>
        /// 头字节
        /// </summary>
        public byte Header { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType MessageType { get => (MessageType)(Header >> 4); set { Header = (byte)((byte)value << 4 | Header & 15); } }
        /// <summary>
        /// 保留bit位
        /// </summary>
        public bool Reserved { get => (Header & 1) == 1; set { Header = (byte)(Header & 254 | (value ? 1 : 0)); } }
        /// <summary>
        /// 保证消息可靠传输，默认为0，只占用一个字节，表示第一次发送。不能用于检测消息重复发送等。只适用于客户端或服务器端尝试重发PUBLISH(发送消息), PUBREL(QoS 2消息流的第二部分，表示消息发布已释放), SUBSCRIBE(客户端订阅某个主题) 或 UNSUBSCRIBE(客户端终止订阅的消息)消息，注意需要满足以下条件：当 QoS > 0时消息需要回复确认，此时，在可变头部需要包含消息ID。当值为1时，表示当前消息先前已经被传送过。
        /// </summary>
        public bool DUPFlag { get => (Header & 0xF >> 3) == 1; set { Header = (byte)(Header & 247 | (value ? 8 : 0)); } }
        /// <summary>
        /// 服务质量  0 最多一次，1 至少一次，2只有一次
        /// </summary>
        public int QoS { get => (Header & 7) >> 1; set { Header = (byte)(Header & 249 | (value << 1)); } }
        /// <summary>
        /// 包数据内容。
        /// </summary>
        public byte[] Data { get; set; } = new byte[0];
        /// <summary>
        /// 当前消息序号
        /// </summary>
        public virtual ushort Identifier { get; set; }
        /// <summary>
        /// 开始组装包。
        /// </summary>
        protected virtual void Packaging()
        {

        }
        /// <summary>
        /// 获取包完整的字节
        /// </summary>
        /// <returns></returns>
        public virtual byte[] ToBytes()
        {
            Packaging();
            List<byte> data = new List<byte>
            {
                Header
            };
            var lenth = Data.Length;
            while (lenth > 127)
            {
                data.Add((byte)(128 | (lenth & 127)));
                lenth >>= 7;
            }
            data.Add((byte)lenth);
            data.AddRange(Data);
            return data.ToArray();
        }
        /// <summary>
        /// 转换为消息。
        /// </summary>
        /// <returns></returns>
        public virtual Message ToMessage()
        {
            switch (MessageType)
            {
                case MessageType.Connect:
                    return OnConnect();
                case MessageType.Subscribe:
                    return OnSubscribe();
                case MessageType.Publish:
                    return OnPublish();
                case MessageType.UnSubscribe:
                    return OnUnSubscribe();
                //case MessageType.ConnectAck:
                //    return new ConnectAckDataPackage(this);
                case MessageType.Disconnect:
                    break;
                case MessageType.PingRequest:
                    break;
                case MessageType.PingResponse:
                    break;
                case MessageType.PublishAck:
                    break;
                case MessageType.PublishComplete:
                    break;
                case MessageType.PublishReceipt:
                    break;
                case MessageType.PublishRelease:
                    break;
                case MessageType.SubscribeAck:
                    break;
                case MessageType.UnSubscribeAck:
                    break;
            }
            return null;
        }
        /// <summary>
        /// 连接消息
        /// </summary>
        /// <returns></returns>
        protected virtual ConnectMessage OnConnect()
        {
            ConnectMessage message = new ConnectMessage() { };
            var bytes = Data;
            var protocolNameLength = bytes[0] << 8 | bytes[1];
            var index = protocolNameLength + 2;
            message.ProtocolName = System.Text.ASCIIEncoding.ASCII.GetString(bytes, 2, protocolNameLength);
            message.Version = bytes[index];
            message.ConnectFlag = bytes[++index];
            message.KeepAlive = bytes[++index] << 8 | bytes[++index];
            message.KeepAlive = message.KeepAlive * 1000 * 2;
            var clientIdLength = bytes[++index] << 8 | bytes[++index];
            message.ClientId = System.Text.ASCIIEncoding.ASCII.GetString(bytes, ++index, clientIdLength);
            index += clientIdLength;
            if (message.HasUserName)
            {
                var userNameLength = bytes[index] << 8 | bytes[++index];
                message.UserName = System.Text.ASCIIEncoding.ASCII.GetString(bytes, ++index, userNameLength);
                index += userNameLength;
                var passwordLength = bytes[index] << 8 | bytes[++index];
                message.Password = System.Text.ASCIIEncoding.ASCII.GetString(bytes, ++index, passwordLength);
            }
            return message;
        }
        /// <summary>
        /// 当收到订阅消息
        /// </summary>
        /// <returns></returns>
        protected virtual TopicMessage OnSubscribe()
        {
            TopicMessage message = new TopicMessage() { ClientId = ClientId, RemoteEndPoint = RemoteEndPoint };
            var index = 0;
            message.Identifier = (ushort)(Data[0] << 8 | Data[1]);
            var topicLength = Data[2] << 8 | Data[3];
            message.Topic = System.Text.ASCIIEncoding.ASCII.GetString(Data, 4, topicLength);
            index += 4 + topicLength;
            message.QoS = Data[index];
            return message;
        }
        /// <summary>
        /// 当收到取消订阅消息
        /// </summary>
        /// <returns></returns>
        protected virtual TopicMessage OnUnSubscribe()
        {
            return OnSubscribe();
        }
        /// <summary>
        /// 当收到发布消息
        /// </summary>
        /// <returns></returns>
        protected virtual ApplicationMessage OnPublish()
        {
            ApplicationMessage subscribeMessage = new ApplicationMessage() { QoS = QoS };
            var index = 2;
            var topicLength = Data[0] << 8 | Data[1];
            subscribeMessage.Topic = System.Text.ASCIIEncoding.ASCII.GetString(Data, 2, topicLength);
            index += topicLength;
            if (QoS > 0)
            {
                subscribeMessage.Identifier = (ushort)(Data[index] << 8 | Data[++index]);
                index++;
            }
            subscribeMessage.Data = new byte[Data.Length - index];
            Array.Copy(Data, index, subscribeMessage.Data, 0, subscribeMessage.Data.Length);
            return subscribeMessage;
        }


    }
}
