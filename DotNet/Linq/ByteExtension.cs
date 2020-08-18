using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace DotNet.Linq
{
    /// <summary>
    /// <see cref="byte"/>相关控制。
    /// </summary>
    public static class ByteExtension
    {
        private static byte[] m_PublicKey;
        private static string m_PrivateKey;
        /// <summary>
        /// 默认<see cref="System.Security.Cryptography.RSA"/>的私有密钥。
        /// </summary>
        public static string PrivateKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(m_PrivateKey))
                {
                    var path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Extension).Assembly.Location), "PrivateRSAKey.key");
                    if (!System.IO.File.Exists(path))
                    {
                        using (var rsa = RSA.Create())
                        {

                            m_PrivateKey = rsa.ToEXmlString(true);
                            System.IO.File.WriteAllBytes(path, m_PrivateKey.ToBytes().ToHexString().ToBytes());
                            m_PublicKey = rsa.ExportParameters(false).Modulus;
                        }
                    }
                    else
                    {
                        m_PrivateKey = System.IO.File.ReadAllBytes(path).GetString().ToHexBytes().GetString();
                    }
                }
                return m_PrivateKey;
            }
        }
        /// <summary>
        /// 默认<see cref="System.Security.Cryptography.RSA"/>的公共密钥。
        /// </summary>
        public static byte[] PublicKey
        {
            get
            {
                if (m_PublicKey == null)
                {
                    using (RSA rsa = RSA.Create())
                    {
                        rsa.FromExXmlString(PrivateKey);
                        m_PublicKey = rsa.ExportParameters(false).Modulus;
                    }
                }
                return m_PublicKey;
            }
        }
        /// <summary>
        /// <see cref="System.Security.Cryptography.RSA"/> 算法的实现执行不对称加密
        /// </summary>
        /// <param name="bytes">要加密的字节数组。</param>
        /// <param name="publicKey"><see cref="System.Security.Cryptography.RSA"/> 公共密钥。</param>
        /// <returns></returns>
        public static byte[] ToRSAEncrypt(this byte[] bytes, byte[] publicKey = null)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                RSAParameters parameter = new RSAParameters()
                {
                    Exponent = new byte[] { 1, 0, 1 },
                    Modulus = publicKey ?? PublicKey
                };
                rsa.ImportParameters(parameter);

#if NETCOREAPP
                return rsa.Encrypt(bytes, RSAEncryptionPadding.Pkcs1);
#else
                return rsa.Encrypt(bytes, false);
#endif
            }
        }
        /// <summary>
        ///  <see cref="System.Security.Cryptography.RSA"/> 算法的实现执行不对称解密。
        /// </summary>
        /// <param name="bytes">要解密的字节数组。</param>
        /// <param name="privateKey"><see cref="System.Security.Cryptography.RSA"/> 私有密钥。</param>
        /// <returns>已解密的数据，它是加密前的原始纯文本。</returns>
        public static byte[] ToRSAEDecrypt(this byte[] bytes, string privateKey = null)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromExXmlString(privateKey ?? PrivateKey);
#if NETCOREAPP
                return rsa.Decrypt(bytes, RSAEncryptionPadding.Pkcs1);
#else
                return rsa.Decrypt(bytes, false);
#endif
            }
        }
        /// <summary>
        ///  将一个<see cref="Byte"/>字节数组解析成类成员。
        /// </summary>
        /// <param name="bytes">要转换的字节数组。</param>
        /// <returns>解析出来的类成员<see cref="object"/>。</returns>
        public static object Deserialize(this byte[] bytes)
        {
            using (var byteStream = new System.IO.MemoryStream(bytes))
            {
                BinaryFormatter binFormat = new BinaryFormatter();//
                return binFormat.Deserialize(byteStream);
            }
        }
        /// <summary>
        /// 将一个<see cref="Byte"/>字节数组转换成一个十六进制字符串。
        /// </summary>
        /// <param name="bytes">要转换的字节数组。</param>
        /// <param name="index">要开始转换的索引。</param>
        /// <param name="count">要转换的个数</param>
        /// <returns>一个十六进制字符串。</returns>
        public static string ToHexString(this byte[] bytes, int index = 0, int count = -1)
        {
            int num = bytes.Length * 2;
            StringBuilder stringBuilder = new StringBuilder();
            while (index < bytes.Length)
            {
                byte num4 = bytes[index++];
                stringBuilder.Append(GetHexValue(num4 / 0x10));
                stringBuilder.Append(GetHexValue(num4 % 0x10));
                if (count != -1 && stringBuilder.Length == count * 2)
                {
                    break;
                }
            }
            return stringBuilder.ToString();
        }
        private static char GetHexValue(int i)
        {
            if (i < 10)
            {
                return (char)(i + 0x30);
            }
            return (char)((i - 10) + 0x41);
        }
        /// <summary>
        /// 计算指定字节数组的MD5值。
        /// </summary>
        /// <param name="value">要计算的字节数组。</param>
        /// <returns>计算所得的哈希代码。</returns>
        public static byte[] ToMD5(this byte[] value)
        {
            using (MD5CryptoServiceProvider hashAlgorithm = new MD5CryptoServiceProvider())
            {
                return hashAlgorithm.ComputeHash(value);
            }
        }
        /// <summary>
        /// 使用提供的密钥计算指定字节数组的MD5值。
        /// </summary>
        /// <param name="value">要计算的字节数组。</param>
        /// <param name="keyValue"><see cref="System.Security.Cryptography.HMACMD5"/> 加密的机密密钥。密钥的长度不限，但如果超过 64 个字节，就会对其进行哈希计算（使用SHA-1），以派生一个 64 个字节的密钥。因此，建议的密钥大小为 64 个字节。</param>
        /// <returns></returns>
        public static byte[] ToMD5(this byte[] value, byte[] keyValue)
        {
            using (HMACMD5 hashAlgorithm = new HMACMD5(keyValue))
            {
                return hashAlgorithm.ComputeHash(value);
            }
        }
        /// <summary>
        /// 使用制定的编码对字节数组编码。
        /// </summary>
        /// <param name="bytes">要制定的字节数组。</param>
        /// <param name="encoding">如果为null则采用utf-8编码。</param>
        /// <returns>包含指定字节序列解码结果的<see cref="string"/></returns>
        public static string GetString(this byte[] bytes, System.Text.Encoding encoding = null)
        {
            return (encoding ?? System.Text.Encoding.UTF8).GetString(bytes);
        }
        /// <summary>
        /// 使用制定的编码对字节数组编码。
        /// </summary>
        /// <param name="bytes">要制定的字节数组。</param>
        /// <param name="index">第一个要解码的字节的索引。</param>
        /// <param name="count">要解码的字节数。</param>
        /// <param name="encoding">如果为null则采用utf-8编码。</param>
        /// <returns></returns>
        public static string GetString(this byte[] bytes, int index, int count, System.Text.Encoding encoding = null)
        {
            return (encoding ?? System.Text.Encoding.UTF8).GetString(bytes, index, count);
        }
        /// <summary>
        /// 将字节进行<see cref="AesCryptoServiceProvider"/> AES加密
        /// </summary>
        /// <param name="value">要加密的字节数组</param>
        /// <param name="keyValue">用于加密和解密的对称密钥。</param>
        /// <returns></returns>
        public static byte[] ToAESEncrypt(this byte[] value, byte[] keyValue)
        {
            #region 自己进行PKCS7补位，用系统自己带的不行
            byte[] msg = new byte[value.Length + 32 - value.Length % 32];
            Array.Copy(value, msg, value.Length);
            byte[] pad = KCS7Encoder(value.Length);
            Array.Copy(pad, 0, msg, value.Length, pad.Length);
            #endregion
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.KeySize = 256;
                //支持的块大小
                aesAlg.BlockSize = 128;
                //填充模式
                //aes.Padding = PaddingMode.PKCS7;
                aesAlg.Padding = PaddingMode.None;
                byte[] Iv = new byte[16];
                Array.Copy(keyValue, Iv, 16);
                aesAlg.Key = keyValue;
                aesAlg.IV = Iv;
                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {

                    return encryptor.TransformFinalBlock(msg, 0, msg.Length);
                }
            }
        }
        private static byte[] KCS7Encoder(int text_length)
        {
            int block_size = 32;
            // 计算需要填充的位数
            int amount_to_pad = block_size - (text_length % block_size);
            if (amount_to_pad == 0)
            {
                amount_to_pad = block_size;
            }
            // 获得补位所用的字符
            char pad_chr = (char)(amount_to_pad & 0xFF);
            string tmp = "";
            for (int index = 0; index < amount_to_pad; index++)
            {
                tmp += pad_chr;
            }
            return Encoding.UTF8.GetBytes(tmp);
        }
        /// <summary>
        /// 将字节进行<see cref="AesCryptoServiceProvider"/> AES解密
        /// </summary>
        /// <param name="value">要解密的字节数组</param>
        /// <param name="keyValue">用于加密和解密的对称密钥。</param>
        /// <returns></returns>
        public static byte[] ToAESDecrypt(this byte[] value, byte[] keyValue)
        {
            byte[] bytes = null;
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.KeySize = 256;
                //支持的块大小
                aesAlg.BlockSize = 128;
                //填充模式
                //aes.Padding = PaddingMode.PKCS7;
                aesAlg.Padding = PaddingMode.None;
                aesAlg.Mode = CipherMode.CBC;
                byte[] Iv = new byte[16];
                Array.Copy(keyValue, Iv, 16);
                aesAlg.Key = keyValue;
                aesAlg.IV = Iv;
                byte[] msg = new byte[value.Length + 32 - value.Length % 32];
                Array.Copy(value, msg, value.Length);
                using (ICryptoTransform encryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    bytes = encryptor.TransformFinalBlock(value, 0, value.Length);
                }
            }
            int pad = bytes[bytes.Length - 1];
            if (pad < 1 || pad > 32)
            {
                pad = 0;
            }
            byte[] res = new byte[bytes.Length - pad];
            Array.Copy(bytes, 0, res, 0, bytes.Length - pad);
            return res;
        }
        /// <summary>
        /// 将<see cref="byte"/>数组进行HMAC-SHA256计算。
        /// </summary>
        /// <param name="value">要机密的数据。</param>
        /// <param name="key"><see cref="System.Security.Cryptography.HMACSHA256"/>加密的机密密钥。密钥的长度不限，但如果超过 64 个字节，就会对其进行哈希计算（使用SHA-1），以派生一个 64 个字节的密钥。因此，建议的密钥大小为 64 个字节。</param>
        /// <returns></returns>
        public static byte[] ToHMACSHA256(this byte[] value, byte[] key)
        {
            byte[] dataHashed = null;
            using (var sha = new HMACSHA256(key))
            {
                dataHashed = sha.ComputeHash(value);
            }
            return dataHashed;
        }
        /// <summary>
        /// 将 8 位无符号整数的数组转换为其用 Base64 数字编码的等效字符串表示形式。
        /// </summary>
        /// <param name="value"> 一个 8 位无符号整数数组。</param>
        /// <returns><paramref name="value"/> 的内容的字符串表示形式，以 Base64 表示。</returns>
        public static string ToBase64String(this byte[] value)
        {
            return Convert.ToBase64String(value);
        }
        /// <summary>
        /// 将4字节<see cref="byte"/>数组转换<see cref="int"/>。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index">从第几个字节开始</param>
        /// <returns></returns>
        public static int ToInt32(this byte[] bytes, int index = 0)
        {
            return ((((bytes[index] << 0x18) | (bytes[index + 1] << 0x10)) | (bytes[index + 2] << 8)) | bytes[index + 3]);
        }
        /// <summary>
        /// 将4字节<see cref="byte"/>数组转换<see cref="uint"/>。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index">从第几个字节开始</param>
        /// <returns></returns>
        public static uint ToUInt32(this byte[] bytes, int index = 0)
        {
            return (uint)((((bytes[index] << 0x18) | (bytes[index + 1] << 0x10)) | (bytes[index + 2] << 8)) | bytes[index + 3]);
        }
        /// <summary>
        /// 将8字节<see cref="byte"/>转换<see cref="long"/>。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index">从第几个字节开始</param>
        /// <returns></returns>
        public static unsafe long ToInt64(this byte[] bytes, int index = 0)
        {
            // return (long)((long)bytes[index] << 56 | (long)bytes[index+1] << 48 | (long)bytes[index+2] << 40 | (long)bytes[index+3] << 32 |
            // (long)bytes[index+4] << 24 | (long)bytes[index+5] << 16 | (long)bytes[index+6] << 8 | (long)bytes[index+7]);
            fixed (byte* ptr = &bytes[index])
            {
                return (long)((long)ptr[0] << 56 | (long)ptr[1] << 48 | (long)ptr[2] << 40 | (long)ptr[3] << 32 |
                    (long)ptr[4] << 24 | (long)ptr[5] << 16 | (long)ptr[6] << 8 | (long)ptr[7]);
            }
        }
        /// <summary>
        /// 将8字节<see cref="byte"/>转换<see cref="ulong"/>。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index">从第几个字节开始</param>
        /// <returns></returns>
        public static unsafe ulong ToUInt64(this byte[] bytes, int index = 0)
        {
            // return (long)((long)bytes[index] << 56 | (long)bytes[index+1] << 48 | (long)bytes[index+2] << 40 | (long)bytes[index+3] << 32 |
            // (long)bytes[index+4] << 24 | (long)bytes[index+5] << 16 | (long)bytes[index+6] << 8 | (long)bytes[index+7]);
            fixed (byte* ptr = &bytes[index])
            {
                return (ulong)((ulong)ptr[0] << 56 | (ulong)ptr[1] << 48 | (ulong)ptr[2] << 40 | (ulong)ptr[3] << 32 |
                    (ulong)ptr[4] << 24 | (ulong)ptr[5] << 16 | (ulong)ptr[6] << 8 | (ulong)ptr[7]);
            }
        }
        /// <summary>
        /// 将1字节<see cref="byte"/>转换<see cref="bool"/>。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index">从第几个字节开始</param>
        /// <returns></returns>
        public static bool ToBoolean(this byte[] bytes, int index = 0)
        {
            return (bytes[index] != 0);
        }
        /// <summary>
        /// 将8字节<see cref="byte"/>转换<see cref="double"/>。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startIndex">从第几个字节开始</param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public static unsafe double ToDouble(this byte[] bytes, int startIndex = 0)
        {
            var val = bytes.ToInt64(startIndex);
            return *(((double*)&val));
        }
        /// <summary>
        /// 将4字节<see cref="byte"/>转换<see cref="float"/>。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startIndex">从第几个字节开始</param>
        public static unsafe float ToFloat(this byte[] bytes, int startIndex = 0)
        {
            int val = bytes.ToInt32(startIndex);
            return *(float*)&val;
        }
        /// <summary>
        /// 将2字节<see cref="byte"/>转换<see cref="short"/>。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startIndex">从第几个字节开始</param>
        public static short ToInt16(this byte[] bytes, int startIndex = 0)
        {
            return ((short)((bytes[startIndex] << 8) | bytes[startIndex + 1]));
        }
        /// <summary>
        /// 将2字节<see cref="byte"/>转换<see cref="ushort"/>。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startIndex">从第几个字节开始</param>
        public static ushort ToUInt16(this byte[] bytes, int startIndex = 0)
        {
            return ((ushort)((bytes[startIndex] << 8) | bytes[startIndex + 1]));
        }
        /// <summary>
        /// 将<see cref="long"/>转换8字节<see cref="byte"/>。
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns></returns>
        public static byte[] ToBytes(this long value)
        {
            byte[] src = new byte[8];
            src[7] = (byte)(value & 0xFF);
            src[6] = (byte)((value >> 8) & 0xFF);
            src[5] = (byte)((value >> 16) & 0xFF);
            src[4] = (byte)((value >> 24) & 0xFF);
            src[3] = (byte)((value >> 32) & 0xFF);
            src[2] = (byte)((value >> 40) & 0xFF);
            src[1] = (byte)((value >> 48) & 0xFF);
            src[0] = (byte)((value >> 56) & 0xFF);
            return src;
        }
        /// <summary>
        /// 将<see cref="ulong"/>转换8字节<see cref="byte"/>。
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns></returns>
        public static byte[] ToBytes(this ulong value)
        {
            byte[] src = new byte[8];
            src[7] = (byte)(value & 0xFF);
            src[6] = (byte)((value >> 8) & 0xFF);
            src[5] = (byte)((value >> 16) & 0xFF);
            src[4] = (byte)((value >> 24) & 0xFF);
            src[3] = (byte)((value >> 32) & 0xFF);
            src[2] = (byte)((value >> 40) & 0xFF);
            src[1] = (byte)((value >> 48) & 0xFF);
            src[0] = (byte)((value >> 56) & 0xFF);
            return src;
        }
        /// <summary>
        /// 将<see cref="int"/>转换4字节<see cref="byte"/>。
        /// </summary>
        /// <param name="i">要转换的值</param>
        /// <returns></returns>
        public static byte[] ToBytes(this int i)
        {
            byte[] src = new byte[4];
            src[3] = (byte)(i & 0xFF);
            src[2] = (byte)((i >> 8) & 0xFF);
            src[1] = (byte)((i >> 16) & 0xFF);
            src[0] = (byte)((i >> 24) & 0xFF);
            return src;
        }
        /// <summary>
        /// 将<see cref="uint"/>转换4字节<see cref="byte"/>。
        /// </summary>
        /// <param name="i">要转换的值</param>
        /// <returns></returns>
        public static byte[] ToBytes(this uint i)
        {
            byte[] src = new byte[4];
            src[3] = (byte)(i & 0xFF);
            src[2] = (byte)((i >> 8) & 0xFF);
            src[1] = (byte)((i >> 16) & 0xFF);
            src[0] = (byte)((i >> 24) & 0xFF);
            return src;
        }
        /// <summary>
        /// 将<see cref="short"/>转换2字节<see cref="byte"/>。
        /// </summary>
        /// <param name="i">要转换的值</param>
        /// <returns></returns>
        public static byte[] ToBytes(this short i)
        {
            byte[] src = new byte[2];
            src[1] = (byte)(i & 0xFF);
            src[0] = (byte)((i >> 8) & 0xFF);
            return src;
        }
        /// <summary>
        /// 将<see cref="ushort"/>转换2字节<see cref="byte"/>。
        /// </summary>
        /// <param name="i">要转换的值</param>
        /// <returns></returns>
        public static byte[] ToBytes(this ushort i)
        {
            byte[] src = new byte[2];
            src[1] = (byte)(i & 0xFF);
            src[0] = (byte)((i >> 8) & 0xFF);
            return src;
        }
        private static bool IsUrlSafeChar(char ch)
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
            {
                return true;
            }

            switch (ch)
            {
                case '-':
                case '_':
                case '.':
                case '!':
                case '*':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }
        private static char IntToHex(int n, bool isUpper = false)
        {
            if (isUpper)
            {
                return n <= 9 ? (char)(n + '0') : (char)(n - 10 + 'A');
            }
            return n <= 9 ? (char)(n + '0') : (char)(n - 10 + 'a');
        }
        /// <summary>
        /// 将指定的bytes字节进行url编码
        /// </summary>
        /// <param name="bytes">要编码的字节</param>
        /// <param name="offset">开始字节</param>
        /// <param name="count">编码个数</param>
        /// <param name="isUpper">是否采用大写（比如：默认是%e8 如果转会成大写就是%E8 ）主要是腾讯那二逼的坑</param>
        /// <returns></returns>
        public static byte[] UrlEncode(this byte[] bytes, int offset = 0, int count = 0, bool isUpper = false)
        {

            int cSpaces = 0;
            int cUnsafe = 0;
            // count them first
            if (count == 0)
            {
                count = bytes.Length;
            }
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];

                if (ch == ' ')
                {
                    cSpaces++;
                }
                else if (!IsUrlSafeChar(ch))
                {
                    cUnsafe++;
                }
            }

            // nothing to expand?
            if (cSpaces == 0 && cUnsafe == 0)
            {
                // DevDiv 912606: respect "offset" and "count"
                if (0 == offset && bytes.Length == count)
                {
                    return bytes;
                }
                else
                {
                    byte[] subarray = new byte[count];
                    Buffer.BlockCopy(bytes, offset, subarray, 0, count);
                    return subarray;
                }
            }

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cUnsafe * 2];
            int pos = 0;

            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];
                char ch = (char)b;

                if (IsUrlSafeChar(ch))
                {
                    expandedBytes[pos++] = b;
                }
                else if (ch == ' ')
                {
                    expandedBytes[pos++] = (byte)'+';
                }
                else
                {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4), isUpper);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f, isUpper);
                }
            }

            return expandedBytes;
        }

        /// <summary>
        /// 添加动态长度字节。最高位为1 则标识需要加上后面的字节，如果最高位为0则结束。
        /// </summary>
        /// <param name="list"></param>
        /// <param name="length">要加上的长度</param>
        public static void AddVariableLengthByte(this List<byte> list, int length)
        {
            while (length > 127)
            {
                list.Add((byte)(128 | (length & 127)));
                length >>= 7;
            }
            list.Add((byte)length);
        }
        /// <summary>
        /// 获取动态长度字节。最高位为1 则标识需要加上后面的字节，如果最高位为0则结束。
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GetVariableLengthBytes(this int length)
        {
            List<byte> list = new List<byte>();
            list.AddVariableLengthByte(length);
            return list.ToArray();
        }
    }
}
