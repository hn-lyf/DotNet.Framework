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


namespace Test
{
    public class Program
    {
        static long TotalCount;
        static void Main(string[] ass)
        {
            for (var i = 13800000000; i < 13900000100; i++)
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
               
                try
                {
                  //  Console.WriteLine($"{DateTime.Now} 输入{i}");
                    var result = HttpHelper.Default.Post($"http://bid.hzsteel.com/front/web/register/checkPhone.jhtml",new { mobile=i });
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
