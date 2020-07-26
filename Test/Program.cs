using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DotNet.Linq;
using DotNet.Net;
using DotNet.Net.MQTT;


namespace Test
{
    public class Program
    {
        static long TotalCount;
        static void Main(string[] ass)
        {
            HttpServer httpServer = new HttpServer();
            httpServer.Start();
            httpServer.ApiHandles.Add("/HttpApi/", typeof(HttpApiHandle));
            //httpServer.AddStaticFile("/httpapi/html", @"G:\webs\TunelManagementTest");
            httpServer.WorkPath = @"G:\webs\TunelManagementTest";
            Console.WriteLine("服务启动");
            HttpHelper.Default.Get("http://www.qq.com");
            Process.Start("explorer.exe", "http://127.0.0.1:8089");
            Console.ReadLine();
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }
    }
    public class HttpApiHandle : IHttpApiHandle
    {
        public HttpRequest Request { get; protected set; }

        public void OnRequest(HttpRequest request)
        {
            Request = request;
        }
        public DotNet.Result Add(long id)
        {
            return new DotNet.Result<long>(true) { Data = id };
        }
    }
}
