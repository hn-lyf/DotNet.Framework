using DotNet.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNet.Web
{
    /// <summary>
    /// 写日志筛选
    /// </summary>
    public abstract class LogAsyncActionFilter : IAsyncActionFilter
    {
        public virtual async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var requestId = DotNet.Snowflake.NewId();
            _ = OnBeginLog(requestId, context.HttpContext.Request.Path.ToString(), context.ActionArguments, context.HttpContext.Connection.RemoteIpAddress.ToString(), context);
            var resultContext = await next();
            object resultObj = null;
            if (resultContext.Exception != null)
            {
                resultContext.ExceptionHandled = true;
                resultContext.Result = new JsonResult(resultObj = new DotNet.Result() { Code = 500, Message = resultContext.Exception.Message });
            }
            if (resultContext.Result is ObjectResult objectResult)
            {
                resultObj = objectResult.Value;
            }
            else if (resultContext.Result is JsonResult jsonResult)
            {
                resultObj = jsonResult.Value;
            }
            if (!resultObj.IsNull())
            {
                _ = OnEndLog(requestId, context.HttpContext.Request.Path.ToString(), context.ActionArguments, resultObj, context.HttpContext.Connection.RemoteIpAddress.ToString(), context);
            }
        }
        /// <summary>
        /// 开始写日志
        /// </summary>
        /// <param name="id">此次请求的唯一编号</param>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="actionArguments">请求的方法参数</param>
        /// <param name="createIP">请求的客户端ip</param>
        /// <param name="context">请求实例</param>
        /// <returns></returns>
        public abstract Task OnBeginLog(long id, string requestUrl, IDictionary<string, object> actionArguments, string createIP, ActionExecutingContext context);
        /// <summary>
        /// 结束写日志
        /// </summary>
        /// <param name="id">此次请求的唯一编号</param>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="actionArguments">请求的方法参数</param>
        /// <param name="resultObj">请求返回的结果</param>
        /// <param name="createIP">请求的客户端ip</param>
        /// <param name="context">请求实例</param>
        /// <returns></returns>
        public abstract Task OnEndLog(long id, string requestUrl, IDictionary<string, object> actionArguments, object resultObj, string createIP, ActionExecutingContext context);

    }
}
