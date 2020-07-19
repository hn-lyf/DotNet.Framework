using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DotNet.Net.MQTT
{
    /// <summary>
    /// 包事件等待。
    /// </summary>
    public class PackageEventWaitHandle : IDisposable
    {
        /// <summary>
        /// 等待需要返回的消息协议
        /// </summary>
        public MessageType MessageType { get; set; }
        /// <summary>
        /// 当前消息序号
        /// </summary>
        public virtual ushort Identifier { get; set; }
        /// <summary>
        /// 等待线程
        /// </summary>
        public EventWaitHandle WaitHandle { get; set; }
        /// <summary>
        /// 接收到的返回数据包
        /// </summary>
        public MQTTDataPackage Data { get; set; }
        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            using (WaitHandle)
            {
                if (WaitHandle != null)
                {
                    if (!(WaitHandle?.SafeWaitHandle?.IsClosed).GetValueOrDefault(true))
                    {
                        WaitHandle?.Close();
                    }
                }
            }
            WaitHandle = null;
        }
    }
}
