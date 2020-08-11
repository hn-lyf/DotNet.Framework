using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Linq;
using DotNet.Net;
using DotNet.Net.MQTT;
using PETEST;

namespace Test
{
    public class Program
    {
        static long TotalCount;
        static void Main(string[] ass)
        {
            var v = new Version("1.8.0");
            var v2 = new Version("1.8.1");
            if (v > v2)
            {

            }
            var bytes1 = new byte[5];
          var dd= bytes1.ToHexString(1, 2);
            PeInfo peInfo = new PeInfo(@"D:\Program Files\Xftp 6\nssftp.dll");
            var tabale = peInfo.GetPETable();
           
            var bytes = System.IO.File.ReadAllBytes(@"F:\OneDrive\工具\端口测试.exe");
            var magic = bytes[1] << 8 | bytes[0];//固定等于5A4D
            var cplp = bytes[3] << 8 | bytes[2];// 文件最后一页的字节数

            for (var i = 13800000000; i < 13800000002; i++)
            {
                //  Console.WriteLine($"{DateTime.Now} 准备输入{i}");
                run(i);
            }
            Console.WriteLine("结束？");
            Console.ReadLine();
        }
        private static async System.Threading.Tasks.Task run(long i)
        {
            await System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                System.Diagnostics.Process.Start(@"D:\Program Files\Tencent\WeChat\WeChat.exe");
                try
                {
                    //  Console.WriteLine($"{DateTime.Now} 输入{i}");
                    var result = HttpHelper.Default.Post($"http://bid.hzsteel.com/front/web/register/checkPhone.jhtml", new { mobile = i });
                    //   Console.WriteLine($"{DateTime.Now} 完成{i}");
                    Console.Title = i.ToString();
                    if (!result.Html.Contains("false"))
                    {


                    }
                }
                catch (Exception ex)
                {

                }
            }, TaskCreationOptions.LongRunning);

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
