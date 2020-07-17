namespace DotNet
{
    /// <summary>
    /// 表示一个结果
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 初始化有数据的结果
        /// </summary>
        /// <param name="result">使用一个结果初始化</param>
        public Result(Result result = default)
        {
            if (result != null)
            {
                Code = result.Code;
                Success = result.Success;
                Message = result.Message;
            }
        }
        /// <summary>
        /// 获取或设置一个值，该值表示此结果的说明。
        /// </summary>
        public virtual string Message { get; set; } = SR.Current.GetString("failed");
        /// <summary>
        /// 获取或设置一个值，该值表示此结果是否成功。
        /// </summary>
        public virtual bool Success { get; set; }
        /// <summary>
        /// 获取或设置一个值，该值表示此结果的数值。
        /// </summary>
        public virtual int Code { get; set; }
        /// <summary>
        /// 将一个<see cref="System.Boolean"/>值转换成<see cref="Result"/>结果。
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Result(bool value)
        {
            return new Result() { Success = value, Message = value ? SR.Current.GetString("success") : SR.Current.GetString("failed") };
        }
        /// <summary>
        /// 将一个<see cref="Result"/>结果转换成<see cref="System.Boolean"/>值。
        /// </summary>
        /// <param name="result"></param>
        public static implicit operator bool(Result result)
        {
            return result != null && result.Success;
        }
    }
    /// <summary>
    /// 表示一个带有数据的结果
    /// </summary>
    /// <typeparam name="T">附加数据类型。</typeparam>
    public class Result<T> : Result
    {
        /// <summary>
        /// 初始化有数据的结果
        /// </summary>
        public Result(Result result = null) : base(result)
        {

        }
        /// <summary>
        /// 附加数据。
        /// </summary>
        public virtual T Data { get; set; }
        /// <summary>
        /// 将一个<typeparamref name="T"/>对象转换成<see cref="Result{T}"/>对象。
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Result<T>(T value)
        {
            return new Result<T>() { Success = value != null, Message = value != null ? SR.Current.GetString("success") : SR.Current.GetString("failed"), Data = value };
        }
        /// <summary>
        /// 将一个<sess name="Result{T}"/>对象转换成<typeparamref name="T"/>对象。
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator T(Result<T> value)
        {
            return value.Data;
        }
    }
}
