using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dossier.WebApi.Controllers.Model
{
    public class AppUpdateInfo
    {
        public long Id { get; set; }
        public int AppVersion { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        /// <summary>
        /// 0 提示更新，1静默更新
        /// </summary>
        public int UpdateFlag { get; set; }
        /// <summary>
        /// 下载文件
        /// </summary>
        public FileInfo[] List { get; set; }
    }
    public class FileInfo
    {
        public string FileName { get; set; }
        public string MD5 { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }

        public string Url { get; set; }

    }
}
