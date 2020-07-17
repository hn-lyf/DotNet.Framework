using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 保留
        /// </summary>
        Reserved = 0,
        /// <summary>
        /// 客户端发起连接
        /// </summary>
        Connect = 1,
        /// <summary>
        /// 服务器回复连接确认
        /// </summary>
        ConnectAck = 2,
        /// <summary>
        /// 发布消息。
        /// </summary>
        Publish = 3,
        /// <summary>
        /// Qos1消息确认。
        /// </summary>
        PublishAck = 4,
        /// <summary>
        /// Qos2消息回执。
        /// </summary>
        PublishReceipt = 5,
        /// <summary>
        /// Qos2消息释放。
        /// </summary>
        PublishRelease = 6,
        /// <summary>
        /// Qos2消息完成。
        /// </summary>
        PublishComplete = 7,
        /// <summary>
        /// 订阅请求
        /// </summary>
        Subscribe = 8,
        /// <summary>
        /// 订阅确定
        /// </summary>
        SubscribeAck = 9,
        /// <summary>
        /// 取消订阅
        /// </summary>
        UnSubscribe = 10,
        /// <summary>
        /// 取消订阅确定
        /// </summary>
        UnSubscribeAck = 11,
        /// <summary>
        /// ping 请求
        /// </summary>
        PingRequest = 12,
        /// <summary>
        /// ping 回复
        /// </summary>
        PingResponse = 13,
        /// <summary>
        /// 断开连接
        /// </summary>
        Disconnect = 14,
    }
}
