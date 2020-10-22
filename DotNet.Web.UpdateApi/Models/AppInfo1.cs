using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotNet.Web.UpdateApi.Models
{
    /// <summary>
    /// 应用信息
    /// </summary>
    public class AppInfo1
    {
        /// <summary>
        /// 应用编号
        /// </summary>
        public virtual long Id { get; set; }
        /// <summary>
        /// 应用名称
        /// </summary>
        public virtual long AppName { get; set; }
        /// <summary>
        /// 系统类型
        /// </summary>
        public virtual SystemType SystemType { get; set; }
        /// <summary>
        /// 最新版本号
        /// </summary>
        public virtual Version Version { get; set; }
        /// <summary>
        /// 应用服务器路径
        /// </summary>
        public virtual string Path { get; set; }
        /// <summary>
        /// 更新备注
        /// </summary>
        public virtual string Remarks { get; set; }
    }
    /// <summary>
    /// 系统类型
    /// </summary>
    [Flags]
    public enum SystemType
    {

        Web = 1,
        Win = 2,
        Android = 4,
        IOS = 8,
    }
}
