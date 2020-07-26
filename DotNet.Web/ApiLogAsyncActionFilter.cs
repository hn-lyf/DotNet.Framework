using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNet.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DotNet.Web
{
    /// <summary>
    /// api 日志记录器。
    /// </summary>
    public abstract class ApiLogAsyncActionFilter : LogAsyncActionFilter
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
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
                if (!(resultObj is DotNet.Result))
                {
                    resultContext.Result = new JsonResult(resultObj);
                }
                _ = OnEndLog(requestId, context.HttpContext.Request.Path.ToString(), context.ActionArguments, resultObj, context.HttpContext.Connection.RemoteIpAddress.ToString(), context);
            }
        }
    }
}
