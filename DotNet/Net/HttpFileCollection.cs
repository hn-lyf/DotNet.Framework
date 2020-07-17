using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace DotNet.Net
{
    /// <summary>
    /// Http上传的文件集合。
    /// </summary>
    public class HttpFileCollection : NameObjectCollectionBase
    {
        /// <summary>
        /// 从文件集合中获取具有指定名称的对象。
        /// </summary>
        /// <param name="name">要返回的项名称。</param>
        /// <returns></returns>
        public HttpPostedFile this[string name] { get { return base.BaseGet(name) as HttpPostedFile; } set { base.BaseSet(name, value); } }
        /// <summary>
        /// 从 System.Web.HttpFileCollection 中获取具有指定数字索引的对象。
        /// </summary>
        /// <param name="index">要从文件集合中获取的项索引。</param>
        /// <returns></returns>
        public HttpPostedFile this[int index] { get { return base.BaseGet(index) as HttpPostedFile; } set { base.BaseSet(index, value); } }

        /// <summary>
        /// 获取一个字符串数组，该数组包含文件集合中所有成员的键（名称）。
        /// </summary>
        public string[] AllKeys { get { return base.BaseGetAllKeys(); } }
    }
}
