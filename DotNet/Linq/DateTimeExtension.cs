using System;

namespace DotNet.Linq
{
    /// <summary>
    /// <see cref="DateTime"/>时间扩展类。
    /// </summary>
    public static class DateTimeExtension
    {
        private const long DatetimeMinTimeTicks = 621355968000000000L;
        /// <summary>
        /// 将<see cref="DateTime"/>时间转换成Unix时间戳。
        /// </summary>
        /// <param name="dateTime"><see cref="DateTime"/>时间。</param>
        /// <returns>Unix时间戳。</returns>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (dateTime.Ticks - DatetimeMinTimeTicks) / 10000000L;
        }
        /// <summary>
        /// 将时间戳转换成<see cref="DateTime"/>时间
        /// </summary>
        /// <param name="value">要转换的时间戳</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long value)
        {
            return new DateTime(value * 10000000L + 621355968000000000L); 
        }
        /// <summary>
        /// 将时间换算成可与<see cref="Snowflake"/>的编号。
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToSnowflakeEqualsId(this DateTime dateTime)
        {
            return DotNet.Snowflake.DateTimeToEqualsId(dateTime);
            
        }

    }
}
