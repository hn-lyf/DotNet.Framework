using System.Net;
using System.Text;

namespace DotNet.Net
{
    /// <summary>
    ///  http 请求信息返回的结果。
    /// </summary>
    public class HttpResult : Result
    {
        /// <summary>
        /// 获取或设置一个值， 改值表示HTTP 定义的状态代码的值。
        /// </summary>
        public virtual HttpStatusCode StatusCode { get { return (HttpStatusCode)base.Code; } set { Code = (int)value; } }
        private string m_CharacterSet = "utf-8";
        /// <summary>
        /// 获取或设置一个值，该值表示响应的字符集。
        /// </summary>
        public virtual string CharacterSet { get { return m_CharacterSet; } set { m_CharacterSet = value; m_Html = null; } }
        /// <summary>
        /// 获取或设置一个值，该值表示响应的二进制数据。
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
#if NETCOREAPP3_1 || NETSTANDARD2_1
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        public virtual byte[] Bytes { get; set; }
        /// <summary>
        /// 返回的html内容
        /// </summary>
        private string m_Html;
        /// <summary>
        /// 获取或设置一个值，该值表示响应的html文本。
        /// </summary>
        public virtual string Html
        {
            get
            {
                if (m_Html == null)
                {
                    if (Bytes != null)
                    {
                        m_Html = Encoding.GetEncoding(CharacterSet).GetString(Bytes);
                    }
                }
                return m_Html;
            }
        }
        /// <summary>
        /// 获取或设置 Content-typeHTTP 标头的值。
        /// </summary>
        public virtual string ContentType { get; set; }
        /// <summary>
        /// 获取或设置一个值，该值表示错误的信息。
        /// </summary>
        public virtual string Error { get; set; }
    }
}
