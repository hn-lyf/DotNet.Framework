using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;

namespace DotNet.Linq
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// 判断是否为null值或为<see cref="DBNull"/>。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">需要要判断的</param>
        /// <returns></returns>
        public static bool IsNull<T>(this T value)
        {
            if (value != null)
            {
                return (value is DBNull);
            }
            return true;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <typeparam name="T">释放资源的类型</typeparam>
        /// <param name="value">待释放的资源。</param>
        public static void Dispose<T>(this T value)
        {
            if (value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        /// <summary>
        /// 将对象或具有指定顶级（根）的对象图形序列化为给定流。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(this T value)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, value);
                return stream.ToArray();
            }
        }
        /// <summary>
        /// 将对象或具有指定顶级（根）的对象图形序列化为给定流。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <param name="stream">图形要序列化为的流。</param>
        public static void Serialize<T>(this T value, Stream stream)
        {
            new BinaryFormatter().Serialize(stream, value);
        }
        /// <summary>
        /// 使用指定的格式格式化当前实例的值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">需要格式化的对象。</param>
        /// <param name="format"> 要使用的格式。</param>
        /// <returns>返回格式化后的字符串。</returns>
        public static string ToString<T>(this T value, string format)
        {
            if (value.IsNull())
            {
                return string.Empty;
            }
            if (value is IFormattable)
            {
                return (value as IFormattable).ToString(format, null);
            }
            return value.ToString();
        }
        /// <summary>
        /// 更改对象类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="conversionType">要转换后的字符串。</param>
        /// <returns></returns>
        public static object ChangeType<T>(this T value, Type conversionType)
        {
            if (conversionType.IsEnum)
            {
                return Enum.Parse(conversionType, value.ToString());
            }
            if (conversionType == typeof(string))
            {
                return value.ToString();
            }
            var vtype = conversionType.GetValueType();
            if (vtype != conversionType)
            {
                try
                {
                    return Convert.ChangeType(value, vtype);
                }
                catch
                {
                    return null;
                }
            }
            return Convert.ChangeType(value, conversionType);
        }
        /// <summary>
        /// 获取值真实的类，避免int? 等。
        /// </summary>
        /// <param name="conversionType">要转换后的字符串。</param>
        /// <returns></returns>
        public static Type GetValueType(this Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                conversionType = conversionType.GetGenericArguments()[0];
            }
            return conversionType;
        }
        /// <summary>
        /// 转换成<see cref="KeyValuePair{String, Object}"/>的<see cref="IEnumerable"/>的迭代。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, object>> ToEnumerable<T>(this T value)
        {
            if (!value.IsNull())
            {
                if (value is IDictionary dictionary)
                {
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        yield return new KeyValuePair<string, object>(entry.Key.ToString(), entry.Value);
                    }
                }
                else if (value is NameValueCollection)
                {
                    var nameObjectCollection = value as NameValueCollection;
                    foreach (string key in nameObjectCollection.Keys)
                    {
                        yield return new KeyValuePair<string, object>(key.ToString(), nameObjectCollection[key]);
                    }
                }
                else if (value is IEnumerable<KeyValuePair<string, object>> enumerable1)
                {
                    foreach (KeyValuePair<string, object> entry in enumerable1)
                    {
                        yield return entry;
                    }
                }
#if !NETFRAMEWORK
                else if (value is IEnumerable<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> enumerable3)
                {
                    foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> entry in enumerable3)
                    {
                        yield return new KeyValuePair<string, object>(entry.Key, entry.Value);
                    }
                }
#endif
                else if (value is DataRow)
                {
                    var row = value as DataRow;
                    foreach (DataColumn column in row.Table.Columns)
                    {
                        yield return new KeyValuePair<string, object>(column.ColumnName, row[column]);
                    }
                }
                else if (value is Hashtable)
                {
                    var hashtable = value as Hashtable;
                    foreach (string key in hashtable.Keys)
                    {
                        yield return new KeyValuePair<string, object>(key, hashtable[key]);
                    }
                }
                else
                {
                    foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(value))
                    {
                        yield return new KeyValuePair<string, object>(descriptor.Name, descriptor.GetValue(value));
                    }
                }
            }
        }
        /// <summary>
        /// 将对象转换成json字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="isLower">是否转换成小写</param>
        /// <returns></returns>
        public static string ToJson<T>(this T value, bool isLower = false)
        {
            JsonSerializerSettings serializerSettings = null;
            if (isLower)
            {
                serializerSettings = new JsonSerializerSettings() { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() };
            }
            if (value is NameValueCollection valueCollection)
            {
                return JsonConvert.SerializeObject(valueCollection.AllKeys.ToDictionary(k => k, k => valueCollection[k]), serializerSettings);
            }
            return JsonConvert.SerializeObject(value, serializerSettings);
        }
        /// <summary>
        /// 串联类型为 System.Collections.Generic.IEnumerable`1 的 System.String 构造集合的成员，其中在每个成员之间使用指定的分隔符。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="func"></param>
        /// <param name="separator">要用作分隔符的字符串。separator 包括在返回的字符串中（只有在 values 具有多个元素时）。</param>
        /// <returns></returns>
        public static string Join<T>(this IEnumerable<T> array, Func<T, string> func, string separator = ",")
        {
            return string.Join(separator, array.Select(t => func(t))); ;
        }
        /// <summary>
        /// 串联类型为 System.Collections.Generic.IEnumerable`1 的 System.String 构造集合的成员并排序，其中在每个成员之间使用指定的分隔符。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="func"></param>
        /// <param name="separator">要用作分隔符的字符串。separator 包括在返回的字符串中（只有在 values 具有多个元素时）。</param>
        /// <returns></returns>
        public static string ToSortJoin<T>(this IEnumerable<T> array, Func<T, string> func, string separator = ",")
        {
            var arrayStr = array.Select(t => func(t)).ToArray();
            Array.Sort(arrayStr);
            return string.Join(separator, arrayStr);
        }
    }
}
