using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Net.Modbus
{
    /// <summary>
    /// modbus 功能码
    /// </summary>
    public enum ModbusFunction : byte
    {
        /// <summary>
        /// 读取线圈
        /// </summary>
        ReadCoil = 01,
        /// <summary>
        /// 读取输入线圈
        /// </summary>
        ReadInputCoil = 02,
        /// <summary>
        /// 读寄存器
        /// </summary>
        ReadHoldingRegister = 03,
        /// <summary>
        /// 读取输入寄存器
        /// </summary>
        ReadInputRegister = 04,
        /// <summary>
        /// 写单个线圈
        /// </summary>
        WriteCoil = 05,
        /// <summary>
        /// 写单个寄存器
        /// </summary>
        WriteHoldingRegister = 06,
        /// <summary>
        /// 写多个寄存器
        /// </summary>
        WriteMoreRegister = 0xF,

    }
}
