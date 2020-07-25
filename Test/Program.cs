using System;
using System.Collections.Generic;
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


            HttpHelper.Default.Get("http://www.qq.com");

            Console.ReadLine();
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }
    }
}
