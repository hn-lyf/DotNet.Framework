using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Web.Apps.Models;
using DotNet.Web.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DotNet.Web.Apps.Controllers
{
    /// <summary>
    /// 授权中心。
    /// </summary>
    [Login]
    public class AccountController : ApiController
    {
        private readonly ILogger<AccountController> _logger;

        /// <summary>
        /// 初始化授权中心。
        /// </summary>
        /// <param name="logger"></param>
        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// 用户登录。
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [NotLogin]
        public LoginResult Login(LoginDTO model)
        {
            var user = new LoginUserInfo() { Id = 123 };
            var result = HttpContext.SetLogin(user);
            return new LoginResult() { Data = user, Message = "登录成功", Success = true, Token = result.Data };
        }
        /// <summary>
        /// 获取登录用户信息。
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Result<LoginUserInfo> Info()
        {
            var result = LoginUser;
            result.Message += $",服务器编号:{Startup.Configuration.GetSection("id").Value}，服务器IP:{HttpContext.Connection.LocalIpAddress}-,客户端IP：{HttpContext.Request.Headers["X-Forwarded-For"]}-{Guid.NewGuid()}";
            return result;
        }
    }
}
