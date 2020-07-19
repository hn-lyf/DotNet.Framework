using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNet;
using DotNet.Linq;
using DotNet.Net;

namespace Test
{
    public class Class1
    {
        static void Main1()
        {


         
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var ffs = DotNet.Net.HttpHelper.Default.Get("https://www.qq.com", new { ws = "我的1", ie = "utf-8" });
            Console.WriteLine(ffs.Html);
            Console.WriteLine(ffs.Html);

            Console.WriteLine(((DotNet.Result)false).ToJson());
            Console.Read();
        }

    }

}
