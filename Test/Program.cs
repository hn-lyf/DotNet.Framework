using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            
            Console.WriteLine("结束？");
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
