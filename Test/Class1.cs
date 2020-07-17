using System;
using System.Text;
using DotNet.Linq;

namespace Test
{
    public class Class1
    {
        static void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var ffs = DotNet.Net.HttpHelper.Default.Get("https://www.qq.com", new { ws = "我的", ie="utf-8" });
            Console.WriteLine(ffs.Html);
            Console.WriteLine(ffs.Html);

            Console.WriteLine(((DotNet.Result)false).ToJson());
            Console.Read();
        }
    }
}
