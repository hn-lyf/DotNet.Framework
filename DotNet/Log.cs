using System;
using System.Collections.Generic;
using System.Text;
using DotNet.Linq;

namespace DotNet
{
    /// <summary>
    /// 一个简单的日志记录
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// 默认日志地址
        /// </summary>
        public static string LogPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Log).Assembly.Location), "log");
        /// <summary>
        /// 写入日志。
        /// </summary>
        /// <param name="text">日志内容</param>
        public static void WriteLog(string text)
        {

            WriteLogFile(text, string.Empty);
        }
        private static void WriteLogFile(string text, string fileName)
        {
           
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}：{text}");
           
            var path = System.IO.Path.Combine(LogPath, DateTime.Now.ToString("yyyy-MM"));
            try
            {
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                path = System.IO.Path.Combine(path, $"{DateTime.Now:yyyy-MM-dd}{fileName}.log");
                new Action(() =>
                {
                    System.IO.File.AppendAllText(path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}:{text}\r\n");

                }).ExecuteRetry(3, 500);

            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// 写入日志。
        /// </summary>
        /// <param name="text">日志内容</param>
        /// <param name="args"></param>
        public static void WriteLog(string text, params object[] args)
        {
            WriteLog(string.Format(text, args));
        }
        /// <summary>
        /// 写入错误信息到日志。
        /// </summary>
        /// <param name="text">错误信息描述</param>
        /// <param name="exception">异常信息</param>
        public static void WriteErrorLog(string text, Exception exception = null)
        {
            text = $"{text}。异常信息:\r\n{exception}\r\n";
            WriteLogFile(text, "-error");
        }
    }
}
