using System;
using System.Collections.Generic;
using System.Text;
using DotNet.Linq;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// mqtt连接包消息。  
    /// </summary>
    public class ConnectDataPackage : MQTTDataPackage
    {
        /// <summary>
        /// 协议类型名称
        /// </summary>
        public string ProtocolName { get; set; }
        /// <summary>
        /// mqtt 版本
        /// </summary>
        public byte Version { get; set; }
        /// <summary>
        /// 连接标识
        /// </summary>
        public byte ConnectFlag { get; set; }
        /// <summary>
        /// 是否包含用户名
        /// </summary>
        public bool HasUserName { get => ((ConnectFlag >> 7) & 1) == 1; set { ConnectFlag |= (byte)(value ? 128 : 0); } }
        /// <summary>
        /// 是否包含密码
        /// </summary>
        public bool HasPassword { get => ((ConnectFlag >> 6) & 1) == 1; set { ConnectFlag |= (byte)(value ? 64 : 0); } }
        /// <summary>
        /// 当客户端意外断开服务器发布其Will Message之后，服务器是否应该继续保存。
        /// </summary>
        public bool WillRetain { get => ((ConnectFlag >> 5) & 1) == 1; set { ConnectFlag |= (byte)(value ? 32 : 0); } }
        /// <summary>
        /// 连接的质量
        /// </summary>
        public int WillQos { get => ((ConnectFlag >> 3) & 3); set { ConnectFlag |= (byte)(value << 3); } }
        /// <summary>
        /// 定义了客户端（没有主动发送DISCONNECT消息）出现网络异常导致连接中断的情况下，服务器需要做的一些措施。
        /// </summary>
        public bool Will { get => ((ConnectFlag >> 2) & 1) == 1; set { ConnectFlag |= (byte)(value ? 4 : 0); } }
        /// <summary>
        /// 表示如果订阅的客户机断线了，要保存为其要推送的消息（QoS为1和QoS为2），若其重新连接时，需将这些消息推送（若客户端长时间不连接，需要设置一个过期值）。 1，断线服务器即清理相关信息，重新连接上来之后，会再次订阅。
        /// </summary>
        public bool CeanSession { get => ((ConnectFlag >> 1) & 1) == 1; set { ConnectFlag |= (byte)(value ? 2 : 0); } }
        /// <summary>
        /// 保留,此值必须为false
        /// </summary>
        public bool ConnectReserved { get => (ConnectFlag & 1) == 1; set { ConnectFlag |= (byte)(value ? 1 : 0); } }
        /// <summary>
        /// 以秒为单位，定义服务器端从客户端接收消息的最大时间间隔。一般应用服务会在业务层次检测客户端网络是否连接，不是TCP/IP协议层面的心跳机制(比如开启SOCKET的SO_KEEPALIVE选项)。 一般来讲，在一个心跳间隔内，客户端发送一个PINGREQ消息到服务器，服务器返回PINGRESP消息，完成一次心跳交互，继而等待下一轮。若客户端没有收到心跳反馈，会关闭掉TCP/IP端口连接，离线。 16位两个字节，可看做一个无符号的short类型值。最大值，2^16-1 = 65535秒 = 18小时。最小值可以为0，表示客户端不断开。一般设为几分钟，比如微信心跳周期为300秒
        /// </summary>
        public int KeepAlive { get; set; }
        /// <summary>
        /// 连接的用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 连接密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 初始化mqtt连接包
        /// </summary>
        /// <param name="package"></param>
        public ConnectDataPackage(MQTTDataPackage package) : base(package)
        {
            Init();
        }
        /// <summary>
        /// 初始化连接包
        /// </summary>
        public ConnectDataPackage()
        {
            this.MessageType = MessageType.Connect;
        }
        /// <summary>
        /// 初始化内容。
        /// </summary>
        protected virtual void Init()
        {
            var bytes = Data;
            var protocolNameLength = bytes[0] << 8 | bytes[1];
            var index = protocolNameLength + 2;
            this.ProtocolName = System.Text.ASCIIEncoding.ASCII.GetString(bytes, 2, protocolNameLength);
            this.Version = bytes[index];
            this.ConnectFlag = bytes[++index];
            this.KeepAlive = bytes[++index] << 8 | bytes[++index];
            this.KeepAlive = this.KeepAlive * 1000 * 2;
            var clientIdLength = bytes[++index] << 8 | bytes[++index];
            this.ClientId = System.Text.ASCIIEncoding.ASCII.GetString(bytes, ++index, clientIdLength);
            index += clientIdLength;
            if (this.HasUserName)
            {
                var userNameLength = bytes[index] << 8 | bytes[++index];
                this.UserName = System.Text.ASCIIEncoding.ASCII.GetString(bytes, ++index, userNameLength);
                index += userNameLength;
                var passwordLength = bytes[index] << 8 | bytes[++index];
                this.Password = System.Text.ASCIIEncoding.ASCII.GetString(bytes, ++index, passwordLength);
            }
        }
        /// <summary>
        /// 获取包完整的字节
        /// </summary>
        /// <returns></returns>
        public override byte[] ToBytes()
        {
            var list = new List<byte>();
            var mqttBytes = ProtocolName.ToBytes(Encoding.ASCII);//协议名称：固定位MQTT
            list.Add((byte)(mqttBytes.Length >> 8));
            list.Add((byte)(mqttBytes.Length & 255));
            list.AddRange(mqttBytes);

            list.Add(Version);//协议版本
            list.Add(ConnectFlag);//连接标识

            list.Add((byte)(KeepAlive >> 8));//心跳值
            list.Add((byte)(KeepAlive & 255));

            var clientIdBytes = ClientId.ToBytes(Encoding.ASCII);//客户端编号
            list.Add((byte)(clientIdBytes.Length >> 8));
            list.Add((byte)(clientIdBytes.Length & 255));
            list.AddRange(clientIdBytes);

            if (HasUserName)//是否包含用户名
            {
                var userNameBytes = UserName.ToBytes(Encoding.ASCII);
                list.Add((byte)(userNameBytes.Length >> 8));
                list.Add((byte)(userNameBytes.Length & 255));
                list.AddRange(userNameBytes);
            }
            if (HasPassword)//是否包含用户密码
            {
                var passwordBytes = Password.ToBytes(Encoding.ASCII);
                list.Add((byte)(passwordBytes.Length >> 8));
                list.Add((byte)(passwordBytes.Length & 255));
                list.AddRange(passwordBytes);
            }
            Data = list.ToArray();
            list.Clear();
            return base.ToBytes();
        }

    }
}
