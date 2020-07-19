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
        static void Main()
        {
            ////
            MQTTClient client = new MQTTClient("127.0.0.1");
            client.Connect(new MQTTConnectInfo() { UserName = "test", Password = "test", ClientId = DateTime.Now.Ticks.ToString() });
            client.Subscribe("iot/log/#"); ;

            Console.ReadLine();
            while (true)
            {
                //Console.WriteLine("请随意输入");
                var text = Guid.NewGuid().ToString();
                if (string.IsNullOrEmpty(text))
                {
                    client.UnSubscribe("iot/log/#"); ;
                    client.Disconnect();
                }
                PublishDataPackage applicationMessage = new PublishDataPackage() { Topic = $"iot/log/{client.ClientId}/dsf", Text = text };
                client.Publish(applicationMessage);
                //Console.ReadLine();
                 System.Threading.Thread.Sleep(100);
            }
            MQTTServer tcpServer = new MQTTServer();
            tcpServer.Start(1883);

            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += (o, e) =>
            {
                Console.Title = $"当前在线客户端：{tcpServer.Clients.Length} 个";
            };
            timer.Start();
            Console.ReadLine();
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }
    }
}
