using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Linq
{
    /// <summary>
    /// <see cref="Socket"/> 扩展
    /// </summary>
    public static class SocketExtension
    {

        /// <summary>
        /// 将两个<see cref="Socket"/> 进行数据交换。
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        public static void ExchangeData(this Socket server, Socket client)
        {
            var clientStream = new NetworkStream(client, true);
            var serverStream = new NetworkStream(server, true);
#if NET40
            Task.WaitAll(Task.Factory.StartNew(() =>
            {
                try
                {
                    clientStream.CopyTo(serverStream);
                }
                catch
                {
                    client?.Close();
                    server?.Close();
                }
            }), Task.Factory.StartNew(() =>
            {
                try
                {
                    serverStream.CopyTo(clientStream);
                }
                catch
                {
                    client?.Close();
                    server?.Close();
                }
            }));
#else
            Task.WaitAll(Task.Run(() =>
           {
               try
               {
                   clientStream.CopyTo(serverStream);
               }
               catch
               {
                   client?.Close();
                   server?.Close();
               }


           }), Task.Run(() =>
           {
               try
               {
                   serverStream.CopyTo(clientStream);
               }
               catch
               {
                   client?.Close();
                   server?.Close();
               }

           }));
#endif
            client?.Close();
            server?.Close();
        }

        /// <summary>
        /// 同步接收指定长度的<see cref="Socket"/>数据包。
        /// </summary>
        /// <param name="client">要从读取的<see cref="Socket"/>对象。</param>
        /// <param name="length">要接受的数据长度。</param>
        /// <returns></returns>
        public static Result<byte[]> ReceiveBytes(this Socket client, int length)
        {
            Result<byte[]> result = new Result<byte[]>() { Code = 0, Message = "未知错误" };
            var bytes = new byte[length];
            int count = 0;

            while (count < length)
            {
                int tempcount = 0;
                if (client != null && client.Connected)
                {
                    try
                    {
                        tempcount = client.Receive(bytes, count, length - count, SocketFlags.None);
                    }
                    catch (Exception ex)
                    {
                        result.Message = ex.ToString();
                        return result;
                    }
                }
                if (tempcount == 0)
                {
                    result.Code = -1;
                    result.Message = $"读取到长度为0，已读取{count}";
                    return result;
                }
                count += tempcount;
            }
            result.Code = count;
            result.Data = bytes;
            result.Success = true;
            result.Message = "获取成功";
            return result;
        }
    }
}
