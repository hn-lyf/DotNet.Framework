using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// 请求连接消息
    /// </summary>
    public class ConnectMessage : Message
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
        public bool HasUserName { get => ((ConnectFlag >> 7) & 1) == 1; }
        /// <summary>
        /// 是否包含密码
        /// </summary>
        public bool HasPassword { get => ((ConnectFlag >> 6) & 1) == 1; }
        /// <summary>
        /// 当客户端意外断开服务器发布其Will Message之后，服务器是否应该继续保存。
        /// </summary>
        public bool WillRetain { get => ((ConnectFlag >> 5) & 1) == 1; }
        /// <summary>
        /// 连接的质量
        /// </summary>
        public bool WillQos { get => ((ConnectFlag >> 4) & 1) == 1; }
        /// <summary>
        /// 定义了客户端（没有主动发送DISCONNECT消息）出现网络异常导致连接中断的情况下，服务器需要做的一些措施。
        /// </summary>
        public bool Will { get => ((ConnectFlag >> 3) & 1) == 1; }
        /// <summary>
        /// 表示如果订阅的客户机断线了，要保存为其要推送的消息（QoS为1和QoS为2），若其重新连接时，需将这些消息推送（若客户端长时间不连接，需要设置一个过期值）。 1，断线服务器即清理相关信息，重新连接上来之后，会再次订阅。
        /// </summary>
        public bool CeanSession { get => ((ConnectFlag >> 1) & 1) == 1; }
        /// <summary>
        /// 保留
        /// </summary>
        public bool Reserved { get => (ConnectFlag & 1) == 1; }
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

    }
}
