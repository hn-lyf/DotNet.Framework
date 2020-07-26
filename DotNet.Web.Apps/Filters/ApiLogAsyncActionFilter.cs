using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace DotNet.Web.Apps.Filters
{
    /// <summary>
    /// 日志记录器。
    /// </summary>
    public class ApiLogAsyncActionFilter : DotNet.Web.ApiLogAsyncActionFilter
    {
        private readonly ILogger<ApiLogAsyncActionFilter> _logger;

        ///// <summary>
        ///// 初始化授权中心。
        ///// </summary>
        ///// <param name="logger"></param>
        //public ApiLogAsyncActionFilter(ILogger<ApiLogAsyncActionFilter> logger)
        //{
        //    _logger = logger;
        //}

        public override Task OnBeginLog(long id, string requestUrl, IDictionary<string, object> actionArguments, string createIP, ActionExecutingContext context)
        {
            return Task.CompletedTask;
        }

        public override Task OnEndLog(long id, string requestUrl, IDictionary<string, object> actionArguments, object resultObj, string createIP, ActionExecutingContext context)
        {
            //_logger.LogInformation($"{requestUrl} {actionArguments.ToJson()}  {resultObj.ToJson()}");
            return Task.CompletedTask;
        }
    }
}
