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


            ////
            int count = 0;
            var ClientId = DateTime.Now.Ticks.ToString();
            MQTTClient client = new MQTTClient("my.hnlyf.com");
            client.Connect(new MQTTConnectInfo() { UserName = "test", Password = "test", ClientId = ClientId });
            // client.Subscribe("iot/log/#"); ;
            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += (o, e) =>
            {
                Console.Title = ($"{ClientId}--{ass[0]}：当前每秒：{count}个,总个数：{TotalCount}");
                count = 0;
            };

            timer.Start();
            while (true)
            {
                //Console.WriteLine("请随意输入");
                var text = Guid.NewGuid().ToString();
                if (string.IsNullOrEmpty(text))
                {
                    client.UnSubscribe("iot/log/#"); ;
                    client.Disconnect();
                }
                PublishDataPackage applicationMessage = new PublishDataPackage() { Topic = $"iot/log/{ass[0]}", Text = text };
                if (!client.Publish(applicationMessage))
                {
                    Console.WriteLine("断线了？");
                    Console.ReadLine();
                }
                System.Threading.Interlocked.Increment(ref count);
                System.Threading.Interlocked.Increment(ref TotalCount);
                System.Threading.Thread.Sleep(10);
            }



            Console.ReadLine();
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }
    }
}
