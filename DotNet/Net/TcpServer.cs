using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DotNet.Net
{
    /// <summary>
    /// 实现一个高效的异步tcp服务器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="P"></typeparam>
    public class TcpServer<T, P>
        where T : SocketClient<P>
        where P : IDataPackage
    {
        private bool IsStart;
        private readonly List<T> clients = new List<T>();
        /// <summary>
        /// Tcp服务器
        /// </summary>
        private System.Net.Sockets.Socket serverSocket;

        /// <summary>
        /// 写入日志。
        /// </summary>
        /// <param name="text">日志内容</param>
        public virtual void WriteLog(string text)
        {
            Log.WriteLog(text);
        }
        /// <summary>
        /// 写入错误信息到日志。
        /// </summary>
        /// <param name="text">错误信息描述</param>
        /// <param name="exception">异常信息</param>
        public virtual void WriteErrorLog(string text, Exception exception = null)
        {
            Log.WriteErrorLog(text, exception);
        }
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        /// <summary>
        /// 启动监听的端口
        /// </summary>
        /// <param name="port">要监听的端口号</param>
        /// <param name="localaddr">要绑定的ip地址</param>
        /// <param name="reuseAddress">是否运行端口复用</param>
        public virtual void Start(int port, IPAddress localaddr = null, bool reuseAddress = true)
        {
            var serverSocketEP = new IPEndPoint(localaddr ?? IPAddress.Any, port);
            serverSocket = new Socket(serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (reuseAddress)
            {
                //serverSocket.ExclusiveAddressUse = true;
                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            serverSocket.Bind(serverSocketEP);
            serverSocket.Listen(int.MaxValue);
            serverSocket.BeginAccept(AcceptSocketCallback, serverSocket);
            IsStart = true;
            WriteLog($"服务启动，监听IP:{serverSocketEP.Address}，端口：{port}");
          
            timer.Elapsed += (o, e) =>
            {
                Console.Title = ($"当前客户端：{Clients.Length}个，每秒处理包：{count}个,总处理个数：{TotalCount}");
                count = 0;
            };

            timer.Start();
        }
        public static long TotalCount;
        public static long count;
        /// <summary>
        /// 停止端口监听。
        /// </summary>
        public virtual void Stop()
        {
            serverSocket.Close();
            IsStart = false;
        }
        /// <summary>
        /// 接收到请求。
        /// </summary>
        /// <param name="ar"></param>
        protected virtual void AcceptSocketCallback(IAsyncResult ar)
        {
            var serverSocket = ar.AsyncState as Socket;
            Socket client = null;
            try
            {
                client = serverSocket.EndAccept(ar);
            }
            catch (Exception ex)
            {
                WriteErrorLog("EndAcceptSocket异常:{0}", ex);
            }
            finally
            {
                if (IsStart)
                {
                    serverSocket.BeginAccept(AcceptSocketCallback, serverSocket);
                }
            }
            if (client != null)
            {
                using (client)
                {
                    try
                    {
                        WriteLog($"新客户端{client.RemoteEndPoint}连接");
                        T socketClient = Activator.CreateInstance(typeof(T), new object[] { client }) as T;
                        clients.Add(socketClient);
                        OnNewClient(socketClient);
                        clients.Remove(socketClient);

                    }
                    catch (Exception ex)
                    {
                        WriteErrorLog("处理客户端连接时发生致命错误", ex);
                    }
                }
            }
        }
        /// <summary>
        /// 当新的客户端连接到服务器时执行的方法，当此方法结束时会断开连接和删除信息，请注意循环读取。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected virtual void OnNewClient(T client)
        {
            client.OnReceive();
        }
        /// <summary>
        /// 获取一个集合，该集合表示当前所有在线的客户端连接。
        /// </summary>
        public virtual T[] Clients { get => clients.ToArray(); }
        /// <summary>
        /// 根据客户端编号获取客户端连接
        /// </summary>
        /// <param name="clientId">客户端编号。</param>
        /// <returns></returns>
        public virtual Result<T> GetClientById(long clientId)
        {
            foreach (var client in Clients)
            {
                if (client.Id == clientId)
                {
                    return client;
                }
            }
            return new Result<T>(false);
        }
    }
}
