using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotNet.Linq
{
    /// <summary>
    /// <see cref="System.IO.Stream"/>扩展类
    /// </summary>
    public static class StreamExtension
    {
        /// <summary>
        /// 将<see cref="byte"/>写入到流。
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes">要写入的字节</param>
        public static void Write(this Stream stream,byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }
        /// <summary>
        /// 写入字符串到流。
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="text">要写入的文本</param>
        /// <param name="encoding">要写入文本的编码</param>
        public static void WriteText(this Stream stream,string text,Encoding encoding = null)
        {
            stream.Write((encoding??Encoding.UTF8).GetBytes(text));
        }
        /// <summary>
        /// 读取<see cref="Stream"/>中所有的字节。
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ReadBytes(this Stream stream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
