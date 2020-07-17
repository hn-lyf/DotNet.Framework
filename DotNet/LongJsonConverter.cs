using DotNet.Linq;
using Newtonsoft.Json;
using System;
#if NETCOREAPP3_1||NETSTANDARD2_1
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
#endif

namespace DotNet
{
    /// <summary>
    /// <see cref="long"/>类型转换。
    /// <para>解决js中不能识别大<see cref="long"/>值的问题。</para>
    /// </summary>
    public class LongJsonConverter : Newtonsoft.Json.JsonConverter
    {
        /// <summary>
        /// 只处理<see cref="long"/>类型。
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(long) || objectType == typeof(long?))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 读取json值
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            return reader.Value.ChangeType(objectType);
        }
        /// <summary>
        /// 将<see cref="long"/>值已字符串形式输出。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value is long v)
            {
                writer.WriteValue(v.ToString());
            }
        }
    }
#if NETCOREAPP3_1||NETSTANDARD2_1
    /// <summary>
    /// 系统默认的json 转换
    /// </summary>
    public class LongToStringConverter : System.Text.Json.Serialization.JsonConverter<long>
    {
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out long number, out int bytesConsumed) && span.Length == bytesConsumed)
                {
                    return number;
                }
                if (long.TryParse(reader.GetString(), out number))
                {
                    return number;
                }
            }
            return reader.GetInt64();
        }
        /// <summary>
        /// 把<see cref="long"/>转换成字符串写入
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
    /// <summary>
    /// 时间转换
    /// </summary>
    public class DateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
    {
        /// <summary>
        /// 获取或设置DateTime格式
        /// <para>默认为: yyyy-MM-dd HH:mm:ss</para>
        /// </summary>
        public string DateTimeFormat { get; set; }
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out DateTime number, out int bytesConsumed) && span.Length == bytesConsumed)
                {
                    return number;
                }
                var date = reader.GetString().ToDateTime(DateTimeFormat);
                if (date.HasValue)
                {
                    return date.Value;
                }
            }
            return reader.GetDateTime();
        }
        /// <summary>
        /// 把<see cref="long"/>转换成字符串写入
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateTimeFormat ?? "yyyy-MM-dd HH:mm:ss"));
        }
    }
#endif
}
