using System;
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
        public static void Write(this Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }
        /// <summary>
        /// 写入字符串到流。
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="text">要写入的文本</param>
        /// <param name="encoding">要写入文本的编码</param>
        public static void WriteText(this Stream stream, string text, Encoding encoding = null)
        {
            stream.Write((encoding ?? Encoding.UTF8).GetBytes(text));
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
        /// <summary>
        /// 读取<see cref="Stream"/>中指定分页大小和页索引的数据。
        /// </summary>
        /// <param name="stream">要读取的流。</param>
        /// <param name="pageSize">页数据大小</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="fullPage">是否必须读取一个完整的包，如果此值为true,则返回的<see cref="byte"/>数组大小都为<paramref name="pageSize"/>大小，哪怕只读取到了1字节。</param>
        /// <returns></returns>
        public static byte[] ReadPageBytes(this Stream stream, int pageSize, int pageIndex, bool fullPage)
        {
            var beginIndex = pageSize * pageIndex;
            var length = Math.Min(pageSize, stream.Length - beginIndex);
            stream.Position = beginIndex;
            var bytes = new byte[fullPage ? pageSize : length];
            var readLength = stream.Read(bytes, 0, (int)bytes.Length);
            while (readLength != length)
            {
                readLength += stream.Read(bytes, readLength, bytes.Length - readLength);
            }
            return bytes;
        }
        /// <summary>
        /// 根据文件路径读取文件<see cref="Stream"/>中指定分页大小和页索引的数据。
        /// </summary>
        /// <param name="filePath">要读取的文件路径。</param>
        /// <param name="pageSize">页数据大小</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="fullPage">是否必须读取一个完整的包，如果此值为true,则返回的<see cref="byte"/>数组大小都为<paramref name="pageSize"/>大小，哪怕只读取到了1字节。</param>
        /// <returns></returns>
        public static byte[] ReadPageBytes(this string filePath, int pageSize, int pageIndex, bool fullPage)
        {
            using (var fs = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return fs.ReadPageBytes(pageSize, pageIndex, fullPage);
            }
        }
    }
}
