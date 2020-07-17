using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNet.Linq
{
    /// <summary>
    /// <see cref="string"/>类控制。
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 将字符串转换成字符串指针。
        /// </summary>
        /// <param name="text">要转换成指针的字符串.</param>
        /// <param name="encoding">要使用的编码，如果为null则使用<see cref="System.Text.Encoding.UTF8"/></param>
        /// <returns></returns>
        public static IntPtr ToIntPtr(this string text, Encoding encoding = null)
        {
            byte[] bytes = text.ToBytes(encoding);
            IntPtr intPtr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, intPtr, bytes.Length);
            Marshal.WriteByte(intPtr, bytes.Length, 0);
            return intPtr;
        }
        /// <summary>
        /// 将字符串指针转换成字符串。
        /// </summary>
        /// <param name="ptr">要转换成字符串的指针.</param>
        /// <param name="encoding">要使用的编码，如果为null则使用<see cref="System.Text.Encoding.UTF8"/></param>
        /// <returns></returns>
        public static string ToIntPtrString(this IntPtr ptr, Encoding encoding = null)
        {
            var data = new List<byte>();
            var off = 0;
            if (ptr != IntPtr.Zero)
            {
                while (true)
                {
                    var ch = Marshal.ReadByte(ptr, off++);
                    if (ch == 0)
                    {
                        break;
                    }
                    data.Add(ch);
                }
            }
            return data.ToArray().GetString(encoding);
        }
        /// <summary>
        /// 将一个十六进制的<see cref="string"/>字符串转换成<see cref="byte"/>字节数组。
        /// </summary>
        /// <param name="value">十六进制字符串。</param>
        /// <returns>转换后的<see cref="byte"/>字节数组。</returns>
        public static byte[] ToHexBytes(this string value)
        {
            byte[] inputByteArray = new byte[value.Length / 2];
            for (int index = 0; index < inputByteArray.Length; index++)
            {
                inputByteArray[index] = (byte)Convert.ToInt32(value.Substring(index * 2, 2), 16);
            }
            return inputByteArray;
        }
        /// <summary>
        /// 将指定的 <see cref="string"/> 中的所有字符编码为<paramref name="encoding"/>一个字节序列。
        /// </summary>
        /// <param name="value">包含要编码的字符的 <see cref="string"/>。</param>
        /// <param name="encoding">要使用的编码，如果为null则使用<see cref="System.Text.Encoding.UTF8"/></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string value, System.Text.Encoding encoding = null)
        {
            return (encoding ?? System.Text.Encoding.UTF8).GetBytes(value);
        }
        /// <summary>
        /// 使用指定的密钥计算其MD5值。
        /// </summary>
        /// <param name="value">要计算的字符串。</param>
        /// <param name="keyValue">密钥</param>
        /// <returns>计算得到的Md5值。</returns>
        public static string ToMD5(this string value, byte[] keyValue)
        {
            return value.ToBytes().ToMD5(keyValue).ToHexString();
        }
        /// <summary>
        /// 使用指定的字符串密钥计算其MD5值。
        /// </summary>
        /// <param name="value">要计算的字符串。</param>
        /// <param name="keyValue">字符串密钥</param>
        /// <returns>计算得到的Md5值。</returns>
        public static string ToMD5(this string value, string keyValue)
        {
            return value.ToBytes().ToMD5(keyValue.ToBytes()).ToHexString();
        }
        /// <summary>
        /// 计算字符串的MD5值。
        /// </summary>
        /// <param name="value">要计算的字符串。</param>
        /// <returns>计算得到的Md5值。</returns>
        public static string ToMD5(this string value)
        {
            return value.ToBytes().ToMD5().ToHexString();
        }

        /// <summary>
        /// 将指定的字符串转换成<see cref="Int32"/>类型，如果转换失败则结果不包含值。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static int? ToInt32(this string value, NumberStyles style = NumberStyles.Integer)
        {
            if (int.TryParse(value, style, null, out int result))
            {
                return result;
            }
            return null;
        }
        /// <summary>
        /// 将指定的字符串转换成<see cref="Int64"/>类型，如果转换失败则结果不包含值。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static long? ToInt64(this string value, NumberStyles style = NumberStyles.Integer)
        {
            if (long.TryParse(value, style, null, out long result))
            {
                return result;
            }
            return null;
        }
        /// <summary>
        /// 将指定的字符串转换成<see cref="Double"/>类型，如果转换失败则结果不包含值。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static double? ToDouble(this string value, NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands)
        {
            if (double.TryParse(value, style, null, out double result))
            {
                return result;
            }
            return null;
        }
        /// <summary>
        /// 将指定的字符串转换成<see cref="Decimal"/>类型，如果转换失败则结果不包含值。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static decimal? ToDecimal(this string value, NumberStyles style = NumberStyles.Number)
        {
            if (decimal.TryParse(value, style, null, out decimal result))
            {
                return result;
            }
            return null;
        }
        /// <summary>
        /// 将指定的字符串转换成<see cref="DateTime"/>类型，如果转换失败则结果不包含值。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="formats">指定字符串的时间格式。</param>
        /// <returns></returns>
        public static DateTime? ToDateTime(this string value, params string[] formats)
        {
            if (formats == null || formats.Length == 0)
            {
                formats = new string[] { "yyyy/MM/dd", "yyyy-MM-dd", "yyyy/MM/dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm", "yyyy-MM-dd HH:mm", "yyyy/MM", "yyyy-MM", "yyyyMMdd", "yyyyMMddHHmmss" };
            }
            if (DateTime.TryParseExact(value, formats, null, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return null;
        }
        /// <summary>
        /// 将字符串进行AES加密
        /// </summary>
        /// <param name="value">要加密的字符串</param>
        /// <param name="keyValue">用于加密和解密的对称密钥。长度必须为32位</param>
        /// <returns></returns>
        public static string ToAESEncrypt(this string value, string keyValue)
        {
            return value.ToAESEncrypt(Convert.FromBase64String(keyValue));
        }
        /// <summary>
        /// 将字符串进行AES加密
        /// </summary>
        /// <param name="value">要加密的字符串</param>
        /// <param name="keyValue">用于加密和解密的对称密钥。长度必须为32位</param>
        /// <returns></returns>
        public static string ToAESEncrypt(this string value, byte[] keyValue)
        {
            return Convert.ToBase64String(value.ToBytes().ToAESEncrypt(keyValue));
        }
        /// <summary>
        /// 将字符串进行<see cref="System.Security.Cryptography.AesCryptoServiceProvider"/> AES解密
        /// </summary>
        /// <param name="value">要解密的字符串</param>
        /// <param name="keyValue">用于加密和解密的对称密钥。长度必须为32位</param>
        /// <returns></returns>
        public static string ToAESDecrypt(this string value, string keyValue)
        {
            return value.ToAESDecrypt(Convert.FromBase64String(keyValue));
        }
        /// <summary>
        /// 将字符串进行<see cref="System.Security.Cryptography.AesCryptoServiceProvider"/> AES解密
        /// </summary>
        /// <param name="value">要解密的字符串</param>
        /// <param name="keyValue">用于加密和解密的对称密钥。长度必须为32位</param>
        /// <returns></returns>
        public static string ToAESDecrypt(this string value, byte[] keyValue)
        {
            return Convert.FromBase64String(value).ToAESDecrypt(keyValue).GetString();
        }
        /// <summary>
        /// 计算指定字符串排序后的SHA1值
        /// </summary>
        /// <param name="separator">连接字符串数组的符号</param>
        /// <param name="args">要排序计算的字符串数组</param>
        /// <returns></returns>
        public static string ToSortSHA1(this string separator, params string[] args)
        {
            return args.ToSortSHA1(separator);
        }
        /// <summary>
        /// <see cref="System.Security.Cryptography.RSA"/> 算法的实现执行不对称加密
        /// </summary>
        /// <param name="value">要加密的字符串。</param>
        /// <param name="publeKey"><see cref="System.Security.Cryptography.RSA"/> 公共密钥。</param>
        /// <returns></returns>
        public static string ToRSAEncrypt(this string value, byte[] publeKey = null)
        {
            return value.ToBytes().ToRSAEncrypt(publeKey).ToHexString();
        }
        /// <summary>
        ///  <see cref="System.Security.Cryptography.RSA"/> 算法的实现执行不对称解密。
        /// </summary>
        /// <param name="value">要解密的字符串。</param>
        /// <param name="privateKey"><see cref="System.Security.Cryptography.RSA"/> 私有密钥。</param>
        /// <returns>已解密的数据，它是加密前的原始纯文本。</returns>
        public static string ToRSAEDecrypt(this string value, string privateKey = null)
        {
            return value.ToHexBytes().ToRSAEDecrypt(privateKey).GetString();
        }
        /// <summary>
        /// 将指定的字符串（它将二进制数据编码为 Base64 数字）转换为等效的 8 位无符号整数数组。
        /// </summary>
        /// <param name="value"> 要转换的字符串。</param>
        /// <returns>与 <paramref name="value"/> 等效的 8 位无符号整数数组。</returns>
        public static byte[] ToBase64Bytes(this string value)
        {
            return Convert.FromBase64String(value);
        }
        /// <summary>
        /// 计算指定字符串排序后的SHA1值
        /// </summary>
        /// <param name="args">要排序计算的字符串数组</param>
        /// <param name="separator">连接字符串数组的符号</param>
        /// <returns></returns>
        public static string ToSortSHA1(this string[] args, string separator)
        {
            Array.Sort(args);
            string newStr = string.Join(separator, args);
            return newStr.ToSHA1();
        }
        /// <summary>
        /// 计算指定字符串排序后的SHA1值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToSHA1(this string value)
        {

            byte[] dataHashed = null;
            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                dataHashed = sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(value));
            }
            return dataHashed.ToHexString();
        }
        /// <summary>
        /// 将<see cref="byte"/>数组进行HMAC-SHA256计算。
        /// </summary>
        /// <param name="value">要机密的数据。</param>
        /// <param name="key"><see cref="System.Security.Cryptography.HMACSHA256"/>加密的机密密钥。密钥的长度不限，但如果超过 64 个字节，就会对其进行哈希计算（使用SHA-1），以派生一个 64 个字节的密钥。因此，建议的密钥大小为 64 个字节。</param>
        /// <param name="encoding">要加密的编码，默认为ASCII</param>
        /// <returns></returns>
        public static string ToHMACSHA256(this string value, string key, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = System.Text.Encoding.ASCII;
            }
            return encoding.GetBytes(value).ToHMACSHA256(key.ToBytes(encoding)).ToHexString();
        }
        /// <summary>
        /// 获取当前文本的拼音。
        /// </summary>
        /// <param name="value">要获取的汉字</param>
        /// <param name="separator">连接拼音的符号</param>
        /// <returns></returns>
        public static string ToPinyin(this string value, string separator = " ")
        {
            return Pinyin.GetPinyin(value, separator);
        }
        /// <summary>
        /// 获取当前文本拼音的首字母。
        /// </summary>
        /// <param name="value">要获取的汉字</param>
        /// <param name="separator">连接拼音的符号</param>
        /// <returns></returns>
        public static string ToPinyinInitials(this string value, string separator = "")
        {
            return Pinyin.GetInitials(value, separator);
        }
        /// <summary>
        /// 将Json字符串转换成指定的类型的对象。
        /// </summary>
        /// <typeparam name="T">要转换的类型</typeparam>
        /// <param name="value">json字符串。</param>
        /// <returns></returns>
        public static T JsonToObject<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
        /// <summary>
        /// 使用正则表达式替换
        /// </summary>
        /// <typeparam name="T">要替换的值的类型</typeparam>
        /// <param name="text">要搜索匹配项的字符串。</param>
        /// <param name="pattern">要匹配的正则表达式模式。</param>
        /// <param name="data">要搜索替换的实体，如<paramref name="text"/>的值为"我的编号是:$Id$",<paramref name="pattern"/>的值是@"\$(\w+)\$",那么<paramref name="data"/>的值是new{Id=1}，即可替换成"我的编号是:1"</param>
        /// <returns></returns>
        public static string ReplaceByRegex<T>(this string text, string pattern, T data)
        {
            var type1 = data.GetType();
            return System.Text.RegularExpressions.Regex.Replace(text, pattern, (m) =>
            {
                var key = m.Groups[1].Value;
                if (data is IDictionary<string, object> dictionary)
                {
                    return dictionary[key]?.ToString();
                }
                else if (data is System.Data.DataRow dataRow)
                {
                    return dataRow[key]?.ToString();
                }
                else if (data is System.Collections.IDictionary dictionary1)
                {
                    return dictionary1[key]?.ToString();//Hashtable
                }
                else if (data is System.Collections.Hashtable hashtable)
                {
                    if (hashtable.ContainsKey(key))
                    {
                        return hashtable[key]?.ToString();
                    }
                    return string.Empty;
                }
                return type1.GetProperty(m.Groups[1].Value).GetValue(data, null).ToString();
            });
        }
        /// <summary>
        /// Json字符串转换成<see cref="JsonData"/>动态访问类。
        /// </summary>
        /// <param name="json">Json字符串。</param>
        /// <returns><see cref="JsonData"/>动态访问类。</returns>
        public static dynamic JsonToClass(this string json)
        {
            return new JsonData(json);
        }
        /// <summary>
        /// 将指定的字符串进行url编码
        /// <para>避坑腾讯AI开发的小写坑</para>
        /// </summary>
        /// <param name="text">要进行编码的字符串。</param>
        ///<param name="encoding">要加密的编码，默认为UTF8</param>
        ///<param name="isUpper">是否采用大写（比如：默认是%e8 如果转会成大写就是%E8 ）主要是腾讯那二逼的坑</param>
        /// <returns></returns>
        public static string UrlEncode(this string text, Encoding encoding = null, bool isUpper = false)
        {
            return text.ToBytes(encoding).UrlEncode(isUpper: isUpper).GetString(encoding);
        }
    }
}
