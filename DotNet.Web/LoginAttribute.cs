#if NETFRAMEWORK
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DotNet.Web
{
    /// <summary>
    /// 登陆标签，设置此标签后没有登陆的将无法登陆context.HttpContext.Session.Get(context.HttpContext.Session.Id) 必须有值
    /// </summary>
    public class LoginAttribute : ActionFilterAttribute
    {
        public static DotNet.Result NoLoginResult = new Result() { Success = false, Message = "您尚未登陆或登陆超时", Code = -999 };
        /// <summary>
        /// 设置返回结果是否为json格式
        /// </summary>
        public virtual bool IsJson { get; set; } = true;
        /// <summary>
        /// 设置重置url的地址
        /// </summary>
        public virtual string RedirectUrl { get; set; } = "/";
        /// <summary>
        /// 开始执行方法
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
#if NET45
            var obj = context.HttpContext.Session[context.HttpContext.Session.SessionID];//获取登录信息
            if (obj == null)
            {
                if (!context.ActionDescriptor.GetFilterAttributes(true).Any(f => f is NotLoginAttribute) && context.ActionDescriptor.GetCustomAttributes(true).Any(f => f is NotLoginAttribute))
                {
                    if (IsJson)
                    {
                        context.Result = new JsonResult() { Data =NoLoginResult};
                    }
                    else
                    {
                        context.Result = new RedirectResult(RedirectUrl);
                    }
                    return;
                }
            }
#else
            var bytes = context.HttpContext.Session.Get(context.HttpContext.Session.Id);
            if (bytes == null)
            {
                if (!context.ActionDescriptor.FilterDescriptors.Any(f => f.Filter is NotLoginAttribute || f.Filter is AllowAnonymousAttribute))
                {
                    if (IsJson)
                    {
                        context.Result = new JsonResult(NoLoginResult);
                    }
                    else
                    {
                        context.Result = new RedirectResult(RedirectUrl);
                    }
                    return;
                }
            }
#endif

            base.OnActionExecuting(context);
        }
    }
    /// <summary>
    /// 不需要登陆
    /// </summary>
    public class NotLoginAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {

            base.OnActionExecuted(context);
        }
    }
}
