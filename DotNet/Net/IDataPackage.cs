namespace DotNet.Net
{
    /// <summary>
    /// 表示一个数据包的接口。
    /// </summary>
    public interface IDataPackage
    {
        /// <summary>
        /// 获取整个包的完整字节。
        /// </summary>
        /// <returns></returns>
        byte[] ToBytes();
    }
}
