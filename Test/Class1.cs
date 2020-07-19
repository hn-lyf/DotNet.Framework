using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNet;
using DotNet.Linq;
using DotNet.Net;

namespace Test
{
    public class Class1
    {
        static void Main1()
        {


         
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var ffs = DotNet.Net.HttpHelper.Default.Get("https://www.qq.com", new { ws = "我的1", ie = "utf-8" });
            Console.WriteLine(ffs.Html);
            Console.WriteLine(ffs.Html);

            Console.WriteLine(((DotNet.Result)false).ToJson());
            Console.Read();
        }


    }
    public class WaterDepthClient : SocketClient<WaterDepthPackage>
    {
        protected override void OnClose()
        {
            WriteLog($"链接关闭{this.Socket.RemoteEndPoint}");
        }

        protected override void OnHandleDataPackage(WaterDepthPackage dataPackage)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 接收完整的包
        /// </summary>
        /// <returns></returns>
        protected override Result<WaterDepthPackage> ReceivePackage()
        {
            Result<byte[]> result;
            Result<WaterDepthPackage> resultPackage = new Result<WaterDepthPackage>() { Success = false };
            resultPackage.Data = new WaterDepthPackage();
            result = this.Socket.ReceiveBytes(2);
            if (!result.Success)
            {
                WriteLog($"读取头失败,获取到的表头{result.Message}");
                resultPackage = new Result<WaterDepthPackage>(result) { Message = "读取头失败" };
                return resultPackage;
            }
            if (result.Data[0] != 0x1A && result.Data[1] != 0xCF)
            {
                resultPackage = new Result<WaterDepthPackage>() { Message = $"表头不正确,获取到的表头{result.Data.ToHexString()}" };
                WriteLog($"表头不正确,获取到的表头{result.Data.ToHexString()}");
                return resultPackage;

            }
            resultPackage.Data.HeadInt = result.Data[0] << 8 | result.Data[1];

            result = this.Socket.ReceiveBytes(4);//设备编号
            if (!result.Success)
            {
                return new Result<WaterDepthPackage>(result) { Message = "读取设备编号失败" };
            }
            resultPackage.Data.TerminalId = result.Data.ToUInt32();
            result = this.Socket.ReceiveBytes(4);//校验和长度
            if (!result.Success)
            {
                return new Result<WaterDepthPackage>(result) { Message = "读取校验失败" };
            }
            var dataLeng = ((((result.Data[1] << 0x10)) | (result.Data[2] << 8)) | result.Data[3]);
            var ddd = 0x100 - (result.Data[1] + result.Data[2] + result.Data[3]);
            if (0x100 - (result.Data[1] + result.Data[2] + result.Data[3]) != result.Data[0])
            {
                return new Result<WaterDepthPackage>(result) { Message = "校验数据失败" };
            }
            result = this.Socket.ReceiveBytes(1);
            if (!result.Success)
            {
                return new Result<WaterDepthPackage>(result) { Message = "读取ApId失败" };
            }
            resultPackage.Data.ApId = result.Data[0];
            result = this.Socket.ReceiveBytes(dataLeng - 1);
            if (!result.Success)
            {
                return new Result<WaterDepthPackage>(result) { Message = "读取数据失败" };
            }
            resultPackage.Data.Data = result.Data;
            return resultPackage;
        }
    }
    public class WaterDepthPackage : IDataPackage
    {
        public const byte IdFlag = 01;
        public const ushort DeviceType = 0x0101;
        public const byte Version = 0x01;
        /// <summary>
        /// 消息头
        /// </summary>
        public virtual int HeadInt { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public virtual uint TerminalId { get; set; }
        /// <summary>
        /// 设备配置数据上报
        /// </summary>
        public virtual int ApId { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public virtual byte[] Data { get; set; }
        public virtual long DeviceId { get { return (long)IdFlag << 56 | (long)DeviceType << 40 | (long)Version << 32 | ((long)TerminalId & 255) << 24 | ((long)TerminalId >> 8); } }
        public virtual byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>
            {
                0x1A,
                0xCF
            };
            bytes.AddRange(TerminalId.ToBytes());

            var dataLength = Data.Length + 1;

            bytes.Add((byte)(0x100 - dataLength));
            bytes.Add((byte)(dataLength >> 16));
            bytes.Add((byte)(dataLength >> 8 & 255));
            bytes.Add((byte)(dataLength & 255));

            bytes.Add((byte)(ApId));

            bytes.AddRange(Data);
            return bytes.ToArray();
        }
    }
}
