using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Install
{
    /// <summary>
    /// Windows 服务
    /// </summary>
    class WinService : ServiceBase
    {
        private static string WinServiceName;
        private static Action<string[]> StartRun;
        public WinService()
        {
            ServiceName = WinServiceName;
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            _ = OnRun(args);
        }
        protected async Task OnRun(string[] args)
        {
            await Task.Run(() => {
                StartRun(args);
            });
        }
        public static void Config(Action<string[]> startRun, string serviceName)
        {
            WinServiceName = serviceName;
            StartRun = startRun;
        }
    }
}
