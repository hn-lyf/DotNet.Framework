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
            var model = new { app_id = "2130481547", time_stamp = 1597219692, nonce_str = "67325e127ac54f54afe252c2808d8100", image = "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" };
            var postText = model.ToEnumerable().Where(k => k.Value != null).ToSortJoin((k) => $"{k.Key}={k.Value?.ToString().UrlEncode(isUpper: true)}", "&");
            var text = $"{postText}&app_key={"UEvT6oeo3PF3eyq9"}";
            var ddd= $"{postText}&sign={text.ToMD5()}";
            string authorization = "oR0je56inghC_choXxHc4kvnkG1g";
            HttpHelper httpHelper = new HttpHelper("https://wx.fjsstlyq.com");
            httpHelper.AddHeader("Wechat-Authorization", authorization);
            var listLinkManResult = httpHelper.Get("/api/index/listLinkMan?currentPage=1&pageSize=10");
            if (listLinkManResult.Success)
            {
                string linkManIds = "1026223,1026224";//1026222
                foreach (var item in listLinkManResult.Html.JsonToClass().data.list)
                {
                    if (links.Contains(item.id.ToString()))
                    {
                        LinkInfos.Add(new LinkInfo() { id = item.id, idCard = item.idCard, name = item.name, phone = item.phone });
                        Program.WriteLine($"常用联系人：Id:{item.id},姓名:{item.name},电话：{item.phone},身份证：{item.idCard}");
                    }


                }//1026220,1026222,1026223,1026224
            }
            var date = "2020-08-16";
            Program.WriteLine($"抢票的日期：{date}");
            var resultGoodsOrderPrice = httpHelper.Get("https://wx.fjsstlyq.com/api/index/goodsOrderPrice/25659");
            var priceRuleId = string.Empty;
            if (resultGoodsOrderPrice.Success)
            {
                foreach (var item in resultGoodsOrderPrice.Html.JsonToClass().data)
                {
                    if (item.played == true && item.date == date)
                    {
                        priceRuleId = item.priceRuleId;
                        break;
                    }
                }
            }

            bool isok = false;
            while (true)
            {
                var result = httpHelper.Get("https://wx.fjsstlyq.com/api/index/goodsDateList/" + priceRuleId);//72878
                if (result.Success)
                {
                    isok = false;
                    foreach (var item in result.Html.JsonToClass().data.productstockInfo)
                    {
                        if (item.surplus > 0)
                        {
                            isok = true;
                            Program.WriteLine($"{DateTime.Now}时间段：{item.describe} 有剩余{item.surplus}张票");
                            for (var i = 0; i < Convert.ToInt32(item.surplus.ToString()) && i < LinkInfos.Count; i++)
                            {
                                QP(authorization, "25659", priceRuleId, item.substockId.ToString(), LinkInfos[i], item.describe.ToString());
                            }

                        }
                    }
                    if (!isok)
                    {
                        Console.Title = $"{DateTime.Now}:无票";
                        //Program.WriteLine($"无票");
                    }
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now}异常：");
                }

                System.Threading.Thread.Sleep(500);
            }
            Console.WriteLine("结束？");
            Console.ReadLine();
        }
        public static void QP(string authorization, string goodsId, string priceRuleId, string dateId, LinkInfo linkManIds, string parkingTime)
        {
            Task.Factory.StartNew(() =>
            {
                HttpHelper httpHelper = new HttpHelper("https://wx.fjsstlyq.com");
                httpHelper.AddHeader("Wechat-Authorization", authorization);
                var restult = httpHelper.Get("/api/order/sendCode");
                string code = "";
                if (restult.Success)
                {
                    string base64 = restult.Html.JsonToClass().data;
                    code = OCR(base64);
                }
                if (code.Length != 4)
                {
                    Program.WriteLine($"识别验证码识别：{code}");
                    QP(authorization, goodsId, priceRuleId, dateId, linkManIds, parkingTime);
                    return;
                }
                var test = httpHelper.Post($"/api/order/subOrder?goodsId={goodsId}&priceRuleId={priceRuleId}&dateId={dateId}&linkManIds={linkManIds.id}&buyUserName={linkManIds.name}&buyPhone={linkManIds.phone}&buyIdCard={linkManIds.idCard}&totelPrice=210&price=210&num=1&parkingTime={parkingTime}&code={code}", new { });
                if (test.Success && test.Html.Contains("OK"))
                {
                    links.Remove(linkManIds.id);
                    LinkInfos.Remove(linkManIds);
                    Program.WriteLine($"{linkManIds.name}购票成功");
                    Beep(500, 3000);
                }
                else
                {
                    Program.WriteLine($"{linkManIds.name}购票是吧{test.Html}");
                }
            }, TaskCreationOptions.LongRunning);
        }
        [DllImport("Kernel32.dll")] //引入命名空间 using System.Runtime.InteropServices;  
        public static extern bool Beep(int frequency, int duration);
        public static string OCR(string imageBase64)
        {
            string str = string.Empty;
            HttpHelper ocr = new HttpHelper("https://api.ai.qq.com");

            var postData = new { app_id = "2130481547", time_stamp = DateTime.Now.ToUniversalTime().ToUnixTimestamp(), nonce_str = Guid.NewGuid().ToString("N"), image = imageBase64 };
            var result = ocr.Post("/fcgi-bin/ocr/ocr_generalocr", postData.ToMD5SignPostText("UEvT6oeo3PF3eyq9"));
            if (result.Success)
            {
                var obj = result.Html.JsonToClass();
                if (obj.msg == "ok")
                {
                    if (obj.data.item_list.Count > 1)
                    {

                    }
                    foreach (var item in obj.data.item_list)
                    {
                        str = item.itemstring;
                        if (GetNumberAlpha(str).Length == 4)
                        {
                            break;
                        }
                    }

                }
            }
            return GetNumberAlpha(str);
        }
        public static string GetNumberAlpha(string source)
        {
            string pattern = "[A-Za-z0-9]";
            string strRet = "";
            System.Text.RegularExpressions.MatchCollection results = System.Text.RegularExpressions.Regex.Matches(source, pattern);
            foreach (var v in results)
            {
                strRet += v.ToString();
            }
            return strRet;
        }
        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

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
