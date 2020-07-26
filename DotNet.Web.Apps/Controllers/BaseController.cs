using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Web.Apps.Models;
using DotNet.Web.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.Web.Apps.Controllers
{
    /// <summary>
    /// 控制器基类。
    /// </summary>
    public class BaseController : Controller
    {
        /// <summary>
        /// 登录用户信息。
        /// </summary>
        public virtual Result<LoginUserInfo> LoginUser
        {
            get
            {
                return HttpContext.GetLogin<LoginUserInfo>();
            }
        }
    }
}
