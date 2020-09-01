using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DotNet.Linq;
using DotNet.Net;
using DotNet.Net.MQTT;


namespace Test
{
    public class Program
    {
        public class LinkInfo
        {
            public string id { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
            public string idCard { get; set; }

        }
        public static void WriteLine(string text)
        {
            Console.WriteLine($"{DateTime.Now}:{text}");
        }
        static long TotalCount;
        static List<string> links = new List<string>("1026223".Split(','));
        static List<LinkInfo> LinkInfos = new List<LinkInfo>();

        static void Main(string[] ass)
        {
            
            Console.WriteLine("结束？");
            Console.ReadLine();
        }
        
       
    }
    static class E
    {
        public static string ToMD5SignPostText<T>(this T model, string appkey)
        {
            var postText = model.ToEnumerable().Where(k => k.Value != null).ToSortJoin((k) => $"{k.Key}={k.Value?.ToString().UrlEncode(isUpper: true)}", "&");
            var text = $"{postText}&app_key={appkey}";
            return $"{postText}&sign={text.ToMD5()}";
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
