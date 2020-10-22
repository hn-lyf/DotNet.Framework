using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Linq;
using DotNet.Web.UpdateApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotNet.Web.UpdateApi.Controllers
{
    [ApiController]
    [Route("[controller]/[Action]")]
    public class AppController : ControllerBase
    {
        private readonly ILogger<AppController> _logger;

        public AppController(ILogger<AppController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// 获取指定应用的信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version">客户端版本号</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:long}")]
        public Result<AppInfo1> Info(long id, Version version)
        {


            if (Request.Headers.ContainsKey("SystemType"))
            {
                //移动端？
                if (Enum.TryParse<SystemType>(Request.Headers["SystemType"], out SystemType type))
                {

                }
            }

            var result = DotNet.Data.DbHelper.Default.QuerySqlToTable("SELECT * FROM S1_AppInfo where Id=@Id ", new { Id = id });
            if (result.Success && result.Data.Rows.Count == 1)
            {
                AppInfo1 app = new AppInfo1();
                result.Data.Rows[0].ToModel(app);
                if (app.SystemType >= SystemType.Android)
                {
                    
                }
                if (app.Version > version)
                {
                    return new Result<AppInfo1>() { Success = true, Code = 1, Data = app, Message = app.Remarks };
                }
                return new Result<AppInfo1>() { Success = true, Code = 0, Data = app, Message = app.Remarks };
            }
            return new Result<AppInfo1>() { Success = false, Code = 404, Message = "应用不存在" };
        }
        /// <summary>
        /// 更新应用信息，返回应用的所以文件下载地址
        /// </summary>
        /// <param name="id"></param>
        /// <param name="versionInt">下载版本信息</param>
        /// <returns></returns>
        [HttpGet]
        public Result Update(long id, Version versionInt)
        {
            return new Result() { };
        }
        public Result Download(long id, int versionInt)
        {
            return new Result() { };
        }
    }
}
