#if NETCOREAPP||NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Text;

namespace DotNet
{
    /// <summary>
    /// 获取默认配置信息。
    /// </summary>
    public static class Configuration
    {
#if NETCOREAPP || NETSTANDARD
        private readonly static IConfigurationRoot m_Config = null;
        static Configuration()
        {
            var builder = new ConfigurationBuilder();

            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "appsettings.json");
            if (System.IO.File.Exists(path))
            {
                builder.AddJsonFile(path, optional: false, reloadOnChange: true);
            }
            else
            {
                path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Configuration).Assembly.Location), "appsettings.json");
                if (System.IO.File.Exists(path))
                {
                    builder.AddJsonFile(path, optional: false, reloadOnChange: true);
                }
            }
            m_Config = builder.Build();
            RegisterDbProviderFactories();
        }
        private static void RegisterDbProviderFactories()
        {
            var dbProviderFactories = Config.GetSection("DbProviderFactories").GetChildren();
            foreach (IConfigurationSection section in dbProviderFactories)
            {
                if (!DbProviderFactories.TryGetFactory(section.Key, out _))
                {
                    DbProviderFactories.RegisterFactory(section.Key, section.Value);
                }
            }
        }
        /// <summary>
        /// 配置根。
        /// </summary>
        public static IConfigurationRoot Config { get { return m_Config; } }
#endif
        /// <summary>
        /// 获取指定名称的数据库连接字符串。
        /// </summary>
        /// <param name="name">连接名称。</param>
        /// <returns></returns>
        public static string GetConnectionString(string name = "default")
        {
#if NETFRAMEWORK //.NET Framework
            if (ConfigurationManager.ConnectionStrings.Count <= 0)
            {
                return "Server=.;Integrated Security=SSPI;Database=master";
            }
            return (ConfigurationManager.ConnectionStrings[name] ?? ConfigurationManager.ConnectionStrings[0]).ConnectionString;
#else
            return Config.GetConnectionString(name);
#endif
        }
        /// <summary>
        /// 获取默认配置的设置
        /// </summary>
        /// <param name="settingName"></param>
        /// <returns></returns>
        public static string GetSetting(string settingName)
        {
#if NETFRAMEWORK //.NET Framework
            return ConfigurationManager.AppSettings[settingName];
#else
            return Config.GetSection(settingName).Value;
#endif
        }
        /// <summary>
        /// 获取数据库类型。
        /// </summary>
        public static string ConnectionProviderName
        {
            get
            {
#if NETFRAMEWORK //.NET Framework
                if (ConfigurationManager.ConnectionStrings.Count <= 0)
                {
                    return "System.Data.SqlClient";
                }
                return (ConfigurationManager.ConnectionStrings["default"] ?? ConfigurationManager.ConnectionStrings[0]).ProviderName;
#else
                return Config.GetSection("ConnectionStrings:ProviderName").Value ?? "System.Data.SqlClient";
#endif
            }
        }
    }
}
