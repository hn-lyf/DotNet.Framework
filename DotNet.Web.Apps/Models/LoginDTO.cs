using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotNet.Web.Apps.Models
{
    /// <summary>
    /// 登录实体。
    /// </summary>
    public class LoginDTO
    {
        /// <summary>
        /// 登录用户名
        /// </summary>
        [Required]
        public virtual string UserName { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        [Required]
        public virtual string Password { get; set; }

    }
}
