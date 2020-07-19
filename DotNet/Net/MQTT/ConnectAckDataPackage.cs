using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Net.MQTT
{

    /// <summary>
    /// 连接确认包
    /// </summary>
    public class ConnectAckDataPackage : MQTTDataPackage
    {
        /// <summary>
        /// 初始化连接确认包
        /// </summary>
        public ConnectAckDataPackage()
        {
            this.Data = new byte[2];
            this.MessageType = MessageType.ConnectAck;
        }
        /// <summary>
        /// 初始化连接确认包
        /// </summary>
        /// <param name="dataPackage"></param>
        public ConnectAckDataPackage(MQTTDataPackage dataPackage)
        {
            this.Data = dataPackage.Data;
            this.Header = dataPackage.Header;
        }
        /// <summary>
        /// 当前会话 Session Present
        /// </summary>
        public virtual bool SessionPresent { get { return (Data[0] & 1) == 1; } set { (Data ?? (Data = new byte[2]))[0] = (byte)(value ? 1 : 0); } }
        /// <summary>
        /// 表示是否连接成功
        /// </summary>
        public virtual Result Result
        {
            get
            {
                Result result = new Result();
                if (Data != null && Data.Length == 2)
                {
                    result.Success = Data[1] == 0;
                    result.Code = Data[1];
                    switch (Data[1])
                    {
                        case 0:
                            result.Message = "连接成功";
                            break;
                        case 1:
                            result.Message = "连接已拒绝，不支持的协议版本";
                            break;
                        case 2:
                            result.Message = "连接已拒绝，不合格的客户端标识符";
                            break;
                        case 3:
                            result.Message = "连接已拒绝，服务端不可用";
                            break;
                        case 4:
                            result.Message = "连接已拒绝，无效的用户名或密码";
                            break;
                        case 5:
                            result.Message = "连接已拒绝，未授权";
                            break;
                        default:
                            result.Message = "未知错误";
                            break;
                    }
                }
                return result;
            }
            set
            {
                if (value)
                {
                    (Data ?? (Data = new byte[2]))[1] = 0;
                }
                else
                {
                    (Data ?? (Data = new byte[2]))[1] = (byte)value.Code;
                }

            }
        }
        /// <summary>
        /// 获取包完整的字节
        /// </summary>
        /// <returns></returns>
        public override byte[] ToBytes()
        {
            return base.ToBytes();
        }
    }
}
