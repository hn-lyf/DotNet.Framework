using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// mqtt 连接信息。
    /// </summary>
    public class MQTTConnectInfo
    {
        /// <summary>
        /// 客户端编号
        /// </summary>
        public virtual string ClientId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public virtual string UserName { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>
        public virtual string Password { get; set; }
        /// <summary>
        /// 遗嘱保留
        /// </summary>
        public virtual bool WillRetain { get; set; }
        /// <summary>
        /// 遗嘱QoS
        /// </summary>
        public virtual Qos WillQos { get; set; }
        /// <summary>
        /// 遗嘱标志 
        /// </summary>
        public virtual bool WillFlag { get; set; }
        /// <summary>
        /// 是否清除对话。
        /// </summary>
        public virtual bool CleanSession { get; set; }
        /// <summary>
        /// 保持连接
        /// <para>警告：这里的单位是秒</para>
        /// </summary>
        public virtual ushort KeepAlive { get; set; } = 10;
    }
    /// <summary>
    /// qos表示
    /// </summary>
    public enum Qos
    {
        /// <summary>
        /// qos 0 不保证消息的到达 ，消息是否到达由底层的tcp协议来保证
        /// </summary>
        QoS0,
        /// <summary>
        /// qos 1 保证至少到达一次，但可能有多次。
        /// </summary>
        QoS1,
        /// <summary>
        /// qos 2 保证至少且只有一次。a-b b 保证只接收一次。
        /// </summary>
        QoS2
    }
}
