
using System;
using System.Collections.Generic;
using System.Text;
using DotNet.Linq;
#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif
namespace DotNet.Web.Linq
{
    /// <summary>
    /// <see cref="HttpContext"/>扩展信息
    /// </summary>
    public static class HttpContextExtension
    {
        /// <summary>
        /// 获取登录信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static Result<T> GetLogin<T>(this HttpContext httpContext)
        {
#if NETFRAMEWORK
            var obj = httpContext.Session[httpContext.Session.SessionID];
            if (obj != null)
            {
                return obj.ToString().JsonToObject<T>();
            }
#else
            var json = httpContext.Session.GetString(httpContext.Session.Id);
            if (json != null)
            {
                return json.JsonToObject<T>();
            }
#endif
            return new Result<T>() { Code = -9999, Message = "尚未登录或登录超时" };
        }
        /// <summary>
        /// 设置登录信息,并返回Token值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpContext"></param>
        /// <param name="loginVal">要保存的登录信息。</param>
        public static Result<string> SetLogin<T>(this HttpContext httpContext, T loginVal)
        {
#if NETFRAMEWORK
            httpContext.Session[httpContext.Session.SessionID] = loginVal.ToJson();
            return httpContext.Session["SessionKey"]?.ToString();
#else
            httpContext.Session.SetString(httpContext.Session.Id, loginVal.ToJson());
            return httpContext.Session.GetString("SessionKey");
#endif

        }
    }
}
