using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Net
{
    /// <summary>
    /// 上载文件消息
    /// </summary>
    public sealed class HttpPostedFile
    {
        /// <summary>
        /// 获取客户端上的文件的完全限定名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 获取客户端发送的文件的 MIME 内容类型。
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// 获取上载文件的大小（以字节为单位）。
        /// </summary>
        public int ContentLength { get; set; }
        /// <summary>
        /// 获取文件内容的byte。
        /// </summary>
        public byte[] Bytes { get; set; }
        /// <summary>
        /// 保存上载文件的内容。
        /// </summary>
        /// <param name="filename">保存的文件的名称。</param>
        public void SaveAs(string filename)
        {
            System.IO.File.WriteAllBytes(filename, Bytes);
        }
    }
}
