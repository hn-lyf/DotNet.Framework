using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DotNet.Data.Linq;

namespace DotNet.Web.Apps
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var r = DotNet.Data.DbHelper.Default.QueryById<UserInfo>("Sys_User", "admin1", "UserName");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
    class UserInfo
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }

    }
}
