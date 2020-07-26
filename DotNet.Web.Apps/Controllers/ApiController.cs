using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.Web.Apps.Controllers
{
    /// <summary>
    /// Api控制基类。
    /// </summary>
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class ApiController : BaseController
    {
        
    }
}
