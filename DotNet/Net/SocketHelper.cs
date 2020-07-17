using DotNet.Linq;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Net
{
    /// <summary>
    /// Berkeley 套接字 辅助
    /// </summary>
    public abstract class SocketClient<Package>
        where Package : IDataPackage
    {
        private Socket m_Socket;
        private Timer timerHeartbeat;
        private System.Net.EndPoint remoteEndPoint;
        /// <summary>
        /// 客户端唯一标识
        /// </summary>
        public virtual long Id { get; set; }
        /// <summary>
        ///  Berkeley 套接字。
        /// </summary>
        public virtual Socket Socket { get => m_Socket; protected set { m_Socket = value; remoteEndPoint = m_Socket.RemoteEndPoint; } }
        /// <summary>
        /// 客户端的远程信息。
        /// </summary>
        public virtual System.Net.EndPoint RemoteEndPoint { get => remoteEndPoint; }
        /// <summary>
        /// 心跳时间。
        /// </summary>
        public virtual int KeepAlive { get; set; } = 180000;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="socket"></param>
        protected SocketClient(Socket socket)
        {
            Socket = socket;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        protected SocketClient()
        {

        }
        /// <summary>
        /// 读取一个完整的包。
        /// </summary>
        /// <returns></returns>
        protected abstract DotNet.Result<Package> ReceivePackage();
        /// <summary>
        /// 开始循环读取消息。
        /// </summary>
        public virtual void OnReceive()
        {

            DotNet.Result<Package> bytesResult = new Result<Package>();
            try
            {
                bytesResult = ReceivePackage();
            }
            catch (Exception ex)
            {
                WriteErrorLog($"接收包时异常", ex);
            }
            finally
            {

                if (bytesResult.Success)
                {
                    try
                    {
#if NET40
                        Task.Factory.StartNew(() => { OnHandleDataPackage(bytesResult.Data); });
#else
                        _ = OnHandleDataPackage(bytesResult.Data);
#endif
                    }
                    catch (Exception ex)
                    {
                        WriteErrorLog($"客户端处理包时报错", ex);
                    }
                }
                else
                {
                    WriteLog($"接收包时错误，错误内容：{bytesResult.Message}");
                    if (bytesResult.Code == -1)
                    {
                        this.Close();
                    }
                }
                if (Socket != null && Socket.Connected)
                {
                    OnHeartbeatTimer();
                    OnReceive();
                }
                else
                {
                    Close();
                }
            }

        }
#if NET40
        /// <summary>
        /// 启用异步读取
        /// </summary>
        /// <returns></returns>
        public virtual Task OnReceiveAsync()
        {
            return Task.Factory.StartNew(OnReceive);
        }
#else
        /// <summary>
        /// 启用异步读取
        /// </summary>
        /// <returns></returns>
        public virtual async Task OnReceiveAsync()
        {
            await Task.Run(() =>
            {
                OnReceive();
            });
        }
#endif

        private bool m_IsClose;
        /// <summary>
        /// 是否已经关闭
        /// </summary>
        public virtual bool IsClose => m_IsClose;
        /// <summary>
        /// 关闭连接，并退出当前线程
        /// </summary>
        public virtual void Close(int timeout = 3)
        {
            lock (this)
            {
                if (!IsClose)
                {
                    m_IsClose = true;
                    WriteLog($"关闭连接");
                    OnClose();
                    //真正关闭，避免二次关闭
                }
            }
            Socket?.Dispose();
            Socket?.Close(timeout);
            timerHeartbeat?.Dispose();
        }
        /// <summary>
        /// 关闭连接并退出。
        /// </summary>
        protected abstract void OnClose();
        /// <summary>
        /// 设置心跳计数器
        /// </summary>
        protected virtual void OnHeartbeatTimer()
        {
            if (timerHeartbeat == null)
            {
                timerHeartbeat = new Timer(OnHeartbeatTimerCallback, this, KeepAlive, KeepAlive);
            }
            else
            {
                timerHeartbeat.Change(KeepAlive, KeepAlive);
            }
        }
        /// <summary>
        /// 心跳实际到达后触发，改方法又心跳计数器执行。
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnHeartbeatTimerCallback(object state)
        {
            WriteLog($"客户端{KeepAlive}s未发包，已丢弃");
            Close();
        }
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
        /// <summary>
        /// 写入日志。
        /// </summary>
        /// <param name="text">日志内容</param>
        /// <param name="args"></param>
        public virtual void WriteLog(string text, params object[] args)
        {
            WriteLog(string.Format(text, args));
        }
#if NET40
        /// <summary>
        /// 开始处理接收的包
        /// </summary>
        /// <param name="dataPackage"></param>
        protected abstract Task OnHandleDataPackage(Package dataPackage);
#else
        /// <summary>
        /// 开始处理接收的包
        /// </summary>
        /// <param name="dataPackage"></param>
        protected abstract Task OnHandleDataPackage(Package dataPackage);
#endif
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes"></param>
        public virtual void SendBytes(byte[] bytes)
        {
            lock (this)
            {
                if (!IsClose)
                {
                    try
                    {
                        Socket.Send(bytes);
                    }
                    catch (Exception ex)
                    {
                        WriteErrorLog($"发送数据{bytes.ToBase64String()}", ex);
                        if (!Socket.Connected)
                        {
                            Close();
                        }
                    }

                }
            }
        }
    }
}
