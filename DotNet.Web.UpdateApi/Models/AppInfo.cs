using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNet.Web.UpdateApi
{
    public class AppInfo
    {
        public long Id { get; set; }
        public int AppVersion { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// 0 提示更新，1静默更新
        /// </summary>
        public int UpdateFlag { get; set; }
    }
    public class AppInfoInput
    {
        public long AppId { get; set; }
        public int AppVersion { get; set; }
    }
}
