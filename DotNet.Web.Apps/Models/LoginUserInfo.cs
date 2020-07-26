using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNet.Web.Apps.Models
{
    /// <summary>
    /// 用户信息。
    /// </summary>
    public class LoginUserInfo
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public virtual long Id { get; set; }
    }
    /// <summary>
    /// 登录信息返回。
    /// </summary>
    public class LoginResult : Result<LoginUserInfo>
    {
        /// <summary>
        /// 验证的token值。
        /// </summary>
        public virtual string Token { get; set; }
    }
}
