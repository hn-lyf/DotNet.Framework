using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dossier.WebApi.Controllers.Model;
using DotNet.Linq;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.Web.UpdateApi.Controllers
{
    [Route("api/[controller]/[action]")]
    public class UpdateController : Controller
    {
        static string GetMD5(string file)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var fs = System.IO.File.Open(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    return md5.ComputeHash(fs).ToHexString();
                }

            }
        }
        [HttpPost]
        public object CheckUpdate([FromForm] AppInfoInput model)
        {
            var path = DotNet.Configuration.Config.GetSection("UpdatePath").Value ?? @"wwwroot\files";
            var appVersion = int.Parse(DotNet.Configuration.Config.GetSection("AppVersion").Value ?? "2310000");
            if (model.AppVersion < appVersion)
            {
                var appPath = System.IO.Path.Combine(path, model.AppId.ToString(), appVersion.ToString());
                if (System.IO.Directory.Exists(appPath))
                {
                    var cache = System.IO.Path.Combine(appPath, "cache.cache");
                    AppUpdateInfo appUpdate = new AppUpdateInfo();
                    if (System.IO.File.Exists(cache))
                    {
                        appUpdate = System.IO.File.ReadAllText(cache).JsonToObject<AppUpdateInfo>();
                        return new DotNet.Result<AppUpdateInfo>()
                        {
                            Code = 200,
                            Success = true,
                            Message = string.Empty,
                            Data = appUpdate
                        };
                    }
                    List<FileInfo> list = new List<FileInfo>();
                    var files = System.IO.Directory.GetFiles(appPath, "*", System.IO.SearchOption.AllDirectories);
                    appUpdate = new AppUpdateInfo() { AppVersion = appVersion, Description = string.Empty, Id = 1, UpdateFlag = 1, UpdateTime = DateTime.Now };
                    foreach (var file in files)
                    {
                        var fileInfo = new System.IO.FileInfo(file);
                        list.Add(new FileInfo
                        {
                            FileName = fileInfo.Name,
                            MD5 = GetMD5(file),
                            Path = file.Remove(0, appPath.Length + 1),
                            Size = fileInfo.Length,
                            Url = $"/files{file.Remove(0, path.Length)}"
                        }); ;
                    }
                    appUpdate.List = list.ToArray();
                    System.IO.File.WriteAllText(cache, appUpdate.ToJson());
                    return new DotNet.Result<AppUpdateInfo>()
                    {
                        Code = 200,
                        Success = true,
                        Message = string.Empty,
                        Data = appUpdate
                    };
                }
            }
            return new DotNet.Result() { Code = 0, Success = false, Message = "没有要更新的" };
        }
        public object FileUpdate(FileInfo model)
        {
            var path = DotNet.Configuration.Config.GetSection("UpdatePath").Value ?? ""; ;
            path = System.IO.Path.Combine(path, "staticfiles");
            var filePath = System.IO.Path.Combine(path, model.FileName);
            if (System.IO.File.Exists(filePath))
            {
                var fileInfo = new System.IO.FileInfo(filePath);
                FileInfo server = null;
                var cache = filePath + ".cache";
                if (System.IO.File.Exists(cache))
                {
                    server = System.IO.File.ReadAllText(cache).JsonToObject<FileInfo>();
                }
                else
                {
                    server = new FileInfo
                    {
                        FileName = fileInfo.Name,
                        MD5 = GetMD5(filePath),
                        Path = model.Path,
                        Size = fileInfo.Length,
                        Url = $"/files/staticfiles/{model.FileName}"
                    };
                    System.IO.File.WriteAllText(cache, server.ToJson());
                }
                return new DotNet.Result<FileInfo>()
                {
                    Code = server.MD5 == model.MD5 ? 0 : 200,
                    Success = server.MD5 == model.MD5,
                    Message = "",
                    Data = server
                };
            }
            return new DotNet.Result() { Code = 0, Success = false, Message = "没有要更新的" };
        }
    }
}
