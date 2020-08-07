using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using DotNet.Linq;
using System.Threading.Tasks;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// mqtt 客户端
    /// </summary>
    public class MQTTClient : SocketClient<MQTTDataPackage>
    {
        /// <summary>
        /// 客户端连接编号
        /// </summary>
        public virtual string ClientId { get; set; }
        /// <summary>
        /// 所有等待的线程
        /// </summary>
        private Dictionary<int, PackageEventWaitHandle> sendWaits = new Dictionary<int, PackageEventWaitHandle>();
        /// <summary>
        /// mqtt服务端的地址，可以是域名或者ip
        /// </summary>
        public virtual string HostName { get; set; }
        /// <summary>
        /// mqtt服务器的端口号
        /// </summary>
        public virtual int Port { get; set; }

        /// <summary>
        /// 使用指定的服务器地址和端口号进行mqtt客户端的初始化
        /// </summary>
        /// <param name="hostName">mqtt服务端的地址，可以是域名或者ip</param>
        /// <param name="port">mqtt服务器的端口号</param>
        public MQTTClient(string hostName, int port = 1883)
        {
            HostName = hostName;
            Port = port;
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }
        /// <summary>
        /// 连接到mqtt服务器
        /// </summary>
        /// <param name="connectInfo"> mqtt连接的相关消息</param>
        /// <returns></returns>
        public virtual Result Connect(MQTTConnectInfo connectInfo)
        {
            try
            {
                this.Socket.Connect(HostName, Port);//
                this.KeepAlive = connectInfo.KeepAlive * 1500;
                _ = OnReceiveAsync();

            }
            catch (Exception ex)
            {
                return new Result() { Message = ex.Message, Code = -1 };
            }
            ConnectDataPackage connectDataPackage = new ConnectDataPackage();
            connectDataPackage.CeanSession = connectInfo.CleanSession;
            connectDataPackage.ClientId = connectInfo.ClientId;
            connectDataPackage.ConnectReserved = false;
            connectDataPackage.DUPFlag = connectInfo.WillRetain;
            connectDataPackage.HasPassword = connectInfo.Password != null;
            connectDataPackage.HasUserName = connectInfo.UserName != null;
            connectDataPackage.KeepAlive = connectInfo.KeepAlive;
            connectDataPackage.UserName = connectInfo.UserName;
            connectDataPackage.Password = connectInfo.Password;
            connectDataPackage.ProtocolName = "MQTT";
            connectDataPackage.WillQos = (int)connectInfo.WillQos;
            connectDataPackage.WillRetain = connectInfo.WillRetain;
            connectDataPackage.Will = connectInfo.WillFlag;
            connectDataPackage.Version = 4;
            var resultReceive = SendPackage(connectDataPackage);
            if (!resultReceive.Success)
            {
                return resultReceive;
            }
            this.ClientId = connectInfo.ClientId;
            //发送一个完整的连接包。
            var packageReceive = new ConnectAckDataPackage(resultReceive.Data);
            _ = OnKeepAlive(connectInfo.KeepAlive);
            return packageReceive.Result;
        }
        /// <summary>
        /// 释放并关闭连接。
        /// </summary>
        public virtual void Disconnect()
        {
            SendPackage(new MQTTDataPackage() { MessageType = MessageType.Disconnect });
            this.Close();

        }
        /// <summary>
        /// 取消订阅主题
        /// </summary>
        /// <param name="topic">主题内容</param>
        /// <returns></returns>
        public virtual Result UnSubscribe(string topic)
        {
            return SendPackage(new TopicDataPackage(MessageType.UnSubscribe) { Topic = topic, Identifier = ++Identifier });
        }
        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic">主题内容</param>
        /// <param name="requestedQoS">主题服务等级</param>
        /// <returns></returns>
        public virtual Result Subscribe(string topic, Qos requestedQoS = Qos.QoS0)
        {
            return SendPackage(new TopicDataPackage(MessageType.Subscribe) { Topic = topic, Identifier = ++Identifier, RequestedQoS = requestedQoS });
        }
        /// <summary>
        /// 当心跳连接到达。
        /// </summary>
        /// <param name="keepAlive"></param>
        /// <returns></returns>
        protected virtual Task OnKeepAlive(int keepAlive)
        {
            return Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(keepAlive * 1000);
                    var package = new MQTTDataPackage() { MessageType = MessageType.PingRequest };
                    var result = SendPackage(package);
                    if (!result.Success)
                    {
                        this.Close();
                        return;
                    }
                }
            }, TaskCreationOptions.LongRunning);

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
            var msgLength = msgType & 127;
            var leftBit = 7;
            while (msgType >> 7 == 1)
            {
                result = this.Socket.ReceiveBytes(1);
                if (!result.Success)
                {
                    WriteLog("获取mqtt 长度失败");
                    return new Result<int>(result);
                }
                msgType = result.Data[0];
                msgLength = ((msgType & 127) << leftBit) | msgLength;
                leftBit += 7;
            }
            return msgLength;
        }
        /// <summary>
        /// 接收包
        /// </summary>
        /// <returns></returns>
        protected override Result<MQTTDataPackage> ReceivePackage()
        {
            Result<byte[]> result;
            Result<MQTTDataPackage> resultPackage = new Result<MQTTDataPackage>() { Success = false };
            MQTTDataPackage package = new MQTTDataPackage();
            package.ClientId = ClientId;
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
            return resultPackage;
        }
        /// <summary>
        /// 关闭包
        /// </summary>
        protected override void OnClose()
        {
            //
        }
        /// <summary>
        /// 当前消息序号
        /// </summary>
        public virtual ushort Identifier { get; set; }
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
        /// 订阅主题包。
        /// </summary>
        /// <param name="message">要发布的消息。</param>
        /// <returns></returns>
        public virtual Result Subscribe(TopicDataPackage message)
        {
            message.Identifier = ++Identifier;
            var result = this.SendPackage(message);//目前不校验，qos 直接发送
            if (!result.Success)
            {
                WriteLog($"订阅{message.Topic}失败");
            }
            return result;
        }

        /// <summary>
        /// 处理收到的包
        /// </summary>
        /// <param name="dataPackage"></param>
        protected override void OnHandleDataPackage(MQTTDataPackage dataPackage)
        {
            WriteLog($"收到{dataPackage.MessageType} 包，	QoS level:{dataPackage.QoS}");

            switch (dataPackage.MessageType)
            {
                case MessageType.ConnectAck:
                    //ConnectAckDataPackage
                    break;
                case MessageType.Disconnect:
                    break;
                case MessageType.PingRequest:
                    break;
                case MessageType.PingResponse:
                    break;
                case MessageType.Publish://这里的发布消息就是服务端转中过来的包
                    OnPublish(dataPackage);
                    break;
                case MessageType.PublishAck:
                    // dataPackage = new IdentifierAckDataPackage(dataPackage);
                    break;
                case MessageType.PublishComplete:
                    break;
                case MessageType.PublishReceipt:
                    break;
                case MessageType.PublishRelease:
                    break;
                case MessageType.SubscribeAck:
                    dataPackage = new SubscribeAckDataPackage(dataPackage);
                    break;
                case MessageType.UnSubscribeAck:
                    dataPackage = new IdentifierAckDataPackage(dataPackage);
                    break;
            }
            //必须把序号读取处理，如果没用 就用0表示
            //2 +2.5 字节
            var waitId = dataPackage.Identifier << 4 | (byte)dataPackage.MessageType;
            if (sendWaits.ContainsKey(waitId))
            {
                sendWaits[waitId].Data = dataPackage;
                sendWaits[waitId].WaitHandle.Set();

            }
        }


        /// <summary>
        /// 当收到发布消息
        /// </summary>
        /// <param name="dataPackage"></param>
        protected virtual void OnPublish(MQTTDataPackage dataPackage)
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
        /// <summary>
        /// 当客户发布消息。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual Result OnPublish(PublishDataPackage message)
        {
            WriteLog($"客户端{message.ClientId}发布消息{message.Topic},QoS{message.QoS}。内容：{message.Text}");
            return true;
        }
        /// <summary>
        /// 发送包。
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public virtual Result<MQTTDataPackage> SendPackage(MQTTDataPackage package)
        {
            try
            {
                var waitId = package.Identifier << 4;
                switch (package.MessageType)
                {
                    case MessageType.Connect:
                        waitId |= (byte)MessageType.ConnectAck;
                        break;
                    case MessageType.Disconnect:
                        waitId = -1;
                        break;
                    case MessageType.PingRequest:
                        waitId |= (byte)MessageType.PingResponse;
                        break;
                    case MessageType.Publish:
                        if (package.QoS > 0)
                        {
                            if (package.QoS == 1)
                            {
                                waitId |= (byte)MessageType.PublishAck;
                            }
                            else
                            {
                                waitId |= (byte)MessageType.PublishReceipt;
                            }
                        }
                        else
                        {
                            waitId = -1;
                        }
                        break;
                    case MessageType.PublishComplete:
                        waitId = -1;
                        break;
                    case MessageType.PublishReceipt:
                        waitId |= (byte)MessageType.PublishRelease;
                        break;
                    case MessageType.PublishRelease:
                        waitId |= (byte)MessageType.PublishComplete;
                        break;
                    case MessageType.Subscribe:
                        waitId |= (byte)MessageType.SubscribeAck;
                        break;
                    case MessageType.UnSubscribe:
                        waitId |= (byte)MessageType.UnSubscribeAck;
                        break;
                    default:
                        waitId = -1;
                        break;
                }
                if (waitId != -1)
                {
                    using (PackageEventWaitHandle packageEventWait = new PackageEventWaitHandle()
                    {
                        WaitHandle = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset)
                    })
                    {
                        lock (this)
                        {
                            sendWaits.Add(waitId, packageEventWait);
                        }

                        var result = this.SendBytes(package.ToBytes());
                        WriteLog($"发送{package.MessageType}包 {result.Success}---------{sendWaits.Count}");
                        if (result.Success)
                        {

                            packageEventWait.WaitHandle.WaitOne(KeepAlive);
                        }
                        lock (this)
                        {
                            sendWaits.Remove(waitId);
                        }
                        return packageEventWait.Data;
                    }
                }
                else
                {
                    var result = this.SendBytes(package.ToBytes());
                    WriteLog($"发送{package.MessageType}包 {result.Success}---------{sendWaits.Count}");
                    return new Result<MQTTDataPackage>(true);
                }
            }
            catch (Exception ex)
            {
                return new Result<MQTTDataPackage>(false);
            }
        }
    }
}
