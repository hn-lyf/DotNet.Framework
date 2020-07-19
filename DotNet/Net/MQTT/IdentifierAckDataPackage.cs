using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// 消息编号确认包。
    /// </summary>
    public class IdentifierAckDataPackage : MQTTDataPackage
    {
        /// <summary>
        /// 初始化mqtt连接包
        /// </summary>
        /// <param name="package"></param>
        public IdentifierAckDataPackage(MQTTDataPackage package) : base(package)
        {
            Identifier = (ushort)(Data[0] << 8 | Data[1]);
        }
        /// <summary>
        /// 初始化连接包
        /// </summary>
        /// <param name="message">协议类型</param>
        public IdentifierAckDataPackage(MessageType message) : this(message, 2)
        {

        }
        /// <summary>
        /// 初始化连接包
        /// </summary>
        /// <param name="message">协议类型</param>
        /// <param name="dataLnegth">数据长度</param>
        protected IdentifierAckDataPackage(MessageType message, int dataLnegth)
        {
            this.MessageType = message;
            this.Data = new byte[dataLnegth];
         
        }
        /// <summary>
        /// 组装包
        /// </summary>
        protected override void Packaging()
        {
            Data[0] = (byte)(Identifier >> 8);
            Data[1] = (byte)(Identifier & 255);
        }
    }
}
