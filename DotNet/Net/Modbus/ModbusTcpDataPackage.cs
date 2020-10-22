using System;
using System.Collections.Generic;
using System.Text;
using DotNet.Linq;

namespace DotNet.Net.Modbus
{
    /// <summary>
    /// modbus 协议包
    /// </summary>
    public class ModbusDataPackage : IDataPackage
    {

        /// <summary>
        /// 功能码
        /// </summary>
        public ModbusFunction Function { get; set; }
        /// <summary>
        /// 寄存器地址
        /// </summary>
        public ushort Address { get; set; }
        /// <summary>
        /// 线圈数量
        /// </summary>
        public ushort Count { get; set; }

        /// <summary>
        /// 从站应答的字节
        /// </summary>
        public byte[] Bytes { get; set; }

        public byte[] ToBytes()
        {
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                memoryStream.WriteByte((byte)Function);
                switch (Function)
                {
                    case ModbusFunction.ReadCoil:
                    case ModbusFunction.ReadHoldingRegister:
                    case ModbusFunction.ReadInputCoil:
                    case ModbusFunction.ReadInputRegister:
                        memoryStream.Write(Address.ToBytes());
                        memoryStream.Write(Count.ToBytes());
                        break;
                    default:
                        memoryStream.WriteByte((byte)Bytes.Length);
                        memoryStream.Write(Bytes);
                        break;
                }
                return memoryStream.ToArray();
            }

        }
    }
    public class ModbusTcpDataPackage : ModbusDataPackage
    {
        /// <summary>
        /// 事务处理标识，服务器从接收的请求中重新复制
        /// </summary>
        public ushort Index { get; set; }
        /// <summary>
        /// modbus协议标识 固定为0
        /// </summary>
        public ushort ModbusFlag { get; set; }

    }
}
