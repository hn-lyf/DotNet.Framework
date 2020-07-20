using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DotNet.Linq;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// mqtt 服务器连接过来的客户端。
    /// </summary>
    public class MQTTSocketClient : DotNet.Net.SocketClient<MQTTDataPackage>
    {
        /// <summary>
        /// 表示mqtt服务器。
        /// </summary>
        public virtual MQTTServer TcpServer { get; set; }
        /// <summary>
        /// 获取一个值，该值指示客户端是否发送过了连接协议包。
        /// </summary>
        public virtual bool IsConnect { get; protected set; }
        private readonly List<TopicDataPackage> subscribeTopics = new List<TopicDataPackage>();
        /// <summary>
        /// 订阅主题。
        /// </summary>
        public TopicDataPackage[] SubscribeTopics { get => subscribeTopics.ToArray(); }
        /// <summary>
        /// 当前消息序号
        /// </summary>
        public virtual ushort Identifier { get; set; }
        /// <summary>
        /// 客户端连接编号
        /// </summary>
        public virtual string ClientId { get; set; }
        /// <summary>
        /// 客户端唯一连接id
        /// </summary>
        public override long Id
        {
            get
            {
                if (long.TryParse(ClientId, out long id))
                {
                    base.Id = id;
                }
                else
                {
                    base.Id = ClientId.GetHashCode();
                }
                return base.Id;
            }
            set
            {
                ClientId = value.ToString();
            }
        }
        /// <summary>
        /// 写日志。
        /// </summary>
        /// <param name="text"></param>
        public override void WriteLog(string text)
        {
            if (ClientId != null)
            {
                text = $"客户端编号：{ClientId}：{text}";
            }
            base.WriteLog(text);
        }
        /// <summary>
        /// 使用<see cref="Socket"/>客户端初始化。
        /// </summary>
        /// <param name="socket"></param>
        public MQTTSocketClient(Socket socket) : base(socket)
        {

        }
        /// <summary>
        /// 关闭服务端连接
        /// </summary>
        protected override void OnClose()
        {
             Console.WriteLine($"{ClientId}关闭连接");
        }

        /// <summary>
        /// 处理收到的包
        /// </summary>
        /// <param name="dataPackage"></param>
        protected override void OnHandleDataPackage(MQTTDataPackage dataPackage)
        {

            try
            {

                WriteLog($"收到{dataPackage.MessageType} 包，	QoS level:{dataPackage.QoS}");

                if (IsConnect && dataPackage.MessageType != MessageType.Connect)
                {
                    WriteLog($"收到{dataPackage.MessageType} 包，	QoS level:{dataPackage.QoS} ,但连接尚未登录，被抛弃");
                    this.Close();

                }

                switch (dataPackage.MessageType)
                {
                    case MessageType.Connect:
                        OnConnect(dataPackage);
                        break;
                    case MessageType.Subscribe:
                        OnSubscribe(dataPackage);
                        break;
                    case MessageType.PingRequest:
                        OnPingRequest(dataPackage);
                        break;
                    case MessageType.Publish:
                        OnPublishPackage(dataPackage).Wait();
                        break;
                    case MessageType.UnSubscribe:
                        OnUnSubscribe(dataPackage);
                        break;
                    case MessageType.Disconnect:
                        this.Close();
                        break;
                }
            }
            catch (Exception ex)
            {

            }
            dataPackage = null;

        }
#if NET40
        /// <summary>
        /// 当收到发布消息
        /// </summary>
        /// <param name="dataPackage"></param>
        protected virtual Task OnPublishPackage(MQTTDataPackage dataPackage)
        {
            return Task.Factory.StartNew(() =>
            {
#else
        /// <summary>
        /// 当收到发布消息
        /// </summary>
        /// <param name="dataPackage"></param>
        /// <returns></returns>
        protected virtual async Task OnPublishPackage(MQTTDataPackage dataPackage)
        {
            await Task.Run(() =>
            {
#endif
                try
                {
                    PublishDataPackage publishDataPackage = new PublishDataPackage(dataPackage);
                    var result = OnPublish(publishDataPackage);
                    if (dataPackage.QoS > 0)
                    {
                        var package = new MQTTDataPackage() { MessageType = MessageType.PublishAck, Data = new byte[3] { (byte)(publishDataPackage.Identifier >> 8), (byte)(publishDataPackage.Identifier & 255), 0 } };
                        if (dataPackage.QoS == 1)
                        {
                            if (!result.Success)
                            {
                                package.Data[2] = 1;
                            }
                            SendPackage(package);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            });
        }
        /// <summary>
        /// 当客户发布消息。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual Result OnPublish(PublishDataPackage message)
        {
            WriteLog($"客户端{message.ClientId}发布消息{message.Topic},QoS{message.QoS}。内容：{message.Text}");
            try
            {
                foreach (var client in TcpServer.Clients)
                {
                    foreach (var topic in client.SubscribeTopics)
                    {
                        if (MqttTopicFilterComparer.IsMatch(message.Topic, topic.Topic))
                        {
                            var temp = message.Clone();
                            temp.QoS = 0;// Math.Min(message.QoS, topic.QoS);//mqtt协议规定，取订阅主题和发送主题中最小的qos值。
                            client.Publish(temp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return true;
        }
        /// <summary>
        /// 发布消息。
        /// </summary>
        /// <param name="message">要发布的消息。</param>
        /// <returns></returns>
        public virtual Result Publish(PublishDataPackage message)
        {
            message.Identifier = ++Identifier;
            this.SendPackage(message);//目前不校验，qos 直接发送
            return true;
        }
        /// <summary>
        /// 当客户端发送了ping 请求
        /// </summary>
        /// <param name="dataPackage"></param>
        protected virtual void OnPingRequest(MQTTDataPackage dataPackage)
        {
            var package = new MQTTDataPackage() { MessageType = MessageType.PingResponse };
            SendPackage(package);
        }
        /// <summary>
        /// 发生订阅消息
        /// </summary>
        /// <param name="dataPackage"></param>
        private void OnSubscribe(MQTTDataPackage dataPackage)
        {

            TopicDataPackage topicDataPackage = new TopicDataPackage(dataPackage);
            var result = OnSubscribe(topicDataPackage);
            var package = new SubscribeAckDataPackage() { Identifier = topicDataPackage.Identifier, Success = result.Success };
            if (result.Success)
            {
                if (!subscribeTopics.Contains(topicDataPackage))
                {
                    subscribeTopics.Add(topicDataPackage);
                }
                package.ValidQos = Qos.QoS2;//
            }
            SendPackage(package);
        }
        /// <summary>
        /// 取消订阅消息
        /// </summary>
        /// <param name="dataPackage"></param>
        private void OnUnSubscribe(MQTTDataPackage dataPackage)
        {
            TopicDataPackage topicDataPackage = new TopicDataPackage(dataPackage);
            var result = OnUnSubscribe(topicDataPackage);
            if (result.Success)
            {
                if (subscribeTopics.Contains(topicDataPackage))
                {
                    subscribeTopics.Remove(topicDataPackage);
                }
                var package = new IdentifierAckDataPackage(MessageType.UnSubscribeAck) { Identifier = topicDataPackage.Identifier };
                SendPackage(package);
            }
        }
        /// <summary>
        /// 当收到 取消订阅主题消息时。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual Result OnUnSubscribe(TopicDataPackage message)
        {
            WriteLog($"客户端{message.ClientId} 取消订阅{message.Topic},QoS{message.QoS}");
            return true;
        }
        /// <summary>
        /// 当收到订阅主题消息时。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual Result OnSubscribe(TopicDataPackage message)
        {
            WriteLog($"客户端{message.ClientId}订阅{message.Topic},QoS{message.RequestedQoS}");
            return true;
        }
        /// <summary>
        /// 当客户端发送连接请求时。
        /// </summary>
        /// <param name="dataPackage">连接请求的包</param>
        private void OnConnect(MQTTDataPackage dataPackage)
        {
            ConnectDataPackage connectDataPackage = new ConnectDataPackage(dataPackage);
            var result = OnClientConnect(connectDataPackage);
            var client = TcpServer.GetClientById(connectDataPackage.ClientId);
            if (client.Success)
            {
                client.Data.WriteLog($"新的客户端连接{this.RemoteEndPoint}上线，旧连接关闭");
                client.Data.Close();
            }
            ClientId = connectDataPackage.ClientId;
            Console.WriteLine($"{ClientId}上线");
            this.KeepAlive = Convert.ToInt32(connectDataPackage.KeepAlive * 1000 * 1.5);
            var package = new ConnectAckDataPackage() { Result = result };
            SendPackage(package);

        }
        /// <summary>
        /// 发送一个标准的mqtt包到客户端连接。
        /// </summary>
        /// <param name="package"></param>
        public virtual void SendPackage(MQTTDataPackage package)
        {
            WriteLog($"发送{package.MessageType}包,QOS：{package.QoS}");
            this.SendBytes(package.ToBytes());
        }
        /// <summary>
        /// 当客户端连接到服务验证是否可以连接
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual Result OnClientConnect(ConnectDataPackage message)
        {

            WriteLog($"客户端{message.ProtocolName}连接,客户端编号{message.ClientId},用户名：{message.UserName}，密码：{message.Password},CeanSession:{message.CeanSession}");
            return true;
        }
        /// <summary>
        /// 接收一个完整的包。
        /// </summary>
        /// <returns></returns>
        protected override Result<MQTTDataPackage> ReceivePackage()
        {

            Result<byte[]> result;
            Result<MQTTDataPackage> resultPackage = new Result<MQTTDataPackage>() { Success = false };
            MQTTDataPackage package = new MQTTDataPackage() { ClientId = ClientId, RemoteEndPoint = RemoteEndPoint };
            result = this.Socket.ReceiveBytes(1);
            if (!result.Success)
            {
                WriteLog("获取mqtt 头 首字节失败");
                this.Close();
                return resultPackage;
            }
            package.Header = result.Data[0];
            var msgLengthResult = ReadLength();
            if (!msgLengthResult.Success)
            {
                WriteLog(msgLengthResult.Message);
                return resultPackage;
            }
            result = this.Socket.ReceiveBytes(msgLengthResult.Data);
            if (!result.Success)
            {
                WriteLog($"获取数据长度{msgLengthResult.Data}内容失败");
                return resultPackage;
            }
            package.Data = result.Data;
            resultPackage.Data = package;
            resultPackage.Success = true;
            resultPackage.Message = "获取包成功";
            System.Threading.Interlocked.Increment(ref MQTTServer.count);
            System.Threading.Interlocked.Increment(ref MQTTServer.TotalCount);
            return resultPackage;
        }
        /// <summary>
        /// 获取一个长度数据
        /// </summary>
        /// <returns></returns>
        protected virtual Result<int> ReadLength()
        {
            var result = this.Socket.ReceiveBytes(1);
            if (!result.Success)
            {
                WriteLog("获取mqtt 长度失败");
                return new Result<int>(result);
            }
            var msgType = result.Data[0];
            var msgLength = msgType & 127;//取低7为的值，因为可变长度有效值只有低7位，第8位用来标识下一个字节是否属于长度字节
            var leftBit = 7;
            while (msgType >> 7 == 1)//判断最高位是否为1，如果为1则说明后面的1个字节也是属于长度字节
            {
                result = this.Socket.ReceiveBytes(1);
                if (!result.Success)
                {
                    WriteLog("获取mqtt 长度失败");
                    return new Result<int>(result);
                }
                msgType = result.Data[0];
                msgLength = ((msgType & 127) << leftBit) | msgLength;// 因为mqtt 可变长度的字节是低位在前，所以新取到的长度要左移取到的次数*7位在|原来的长度。
                leftBit += 7;
            }
            return msgLength;
        }
    }
}
