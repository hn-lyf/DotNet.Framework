using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// 订阅回复包
    /// </summary>
    public class SubscribeAckDataPackage : IdentifierAckDataPackage
    {
        /// <summary>
        /// 初始化mqtt连接包
        /// </summary>
        /// <param name="package"></param>
        public SubscribeAckDataPackage(MQTTDataPackage package) : base(package)
        {

        }
        /// <summary>
        /// 初始化连接包
        /// </summary>
        public SubscribeAckDataPackage() : base(MessageType.SubscribeAck, 3)
        {

        }
        /// <summary>
        /// 是否成功
        /// </summary>
        public virtual bool Success { get { return Data[2] >> 7 == 0; } set { Data[2] |= (byte)(value ? 0 : 0x80); } }
        /// <summary>
        /// 有效qos
        /// </summary>
        public Qos ValidQos { get { return (Qos)(Data[2] & 3); } set { Data[2] |= (byte)(value); } }
    }
}
