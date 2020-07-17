using System;

namespace DotNet
{
    /// <summary>
    /// 身份证信息
    /// </summary>
    public class IDCard
    {
        private static readonly int[] Radix = new int[] { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
        private static readonly string CheckCodes = "10X98765432";
        /// <summary>
        /// 省份代码2位
        /// </summary>
        public int ProvinceCode { get; set; }//设置的时候可以判断 比如大于等于100
        /// <summary>
        /// 城市代码2位
        /// </summary>
        public int CityCode { get; set; }//设置的时候可以判断 比如大于等于100
        /// <summary>
        /// 县级代码2位
        /// </summary>
        public int County { get; set; }//设置的时候可以判断 比如大于等于100
        /// <summary>
        /// 生日8位
        /// </summary>
        public DateTime Birthday { get; set; }
        /// <summary>
        /// 性别，男or女
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 序列号3位
        /// </summary>
        public int SerialNumber { get; set; }//设置的时候可以判断 比如大于等于1000
        /// <summary>
        /// 校验码1位
        /// </summary>
        public char CheckCode
        {
            get
            {
                var no = string.Concat(ProvinceCode.ToString("00"), CityCode.ToString("00"), County.ToString("00"), Birthday.ToString("yyyyMMdd"), SerialNumber.ToString("000"));
                int h = 0;
                for (var index = 0; index < 17; index++)
                {
                    h += int.Parse(no[index].ToString()) * Radix[index];
                }
                var j = h % 11;
                return CheckCodes[j];
            }
        }
        /// <summary>
        /// 身份证号码。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(ProvinceCode.ToString("00"), CityCode.ToString("00"), County.ToString("00"), Birthday.ToString("yyyyMMdd"), SerialNumber.ToString("000"), CheckCode);
        }
        /// <summary>
        /// 将身份证字符串转换成<see cref="IDCard"/>的对象。如果身份证字符串不正确则返回null
        /// </summary>
        /// <param name="no">身份证字符串。</param>
        /// <returns></returns>
        public static IDCard ToIDCard(string no)
        {
            if (string.IsNullOrWhiteSpace(no))
            {
                return null;
            }
            if (no.Length != 18)
            {
                return null;
            }

            IDCard card = new IDCard();
            if (!int.TryParse(no.Substring(0, 2), out int provinceCode))
            {
                return null;
            }
            card.ProvinceCode = provinceCode;
            if (!int.TryParse(no.Substring(2, 2), out int cityCode))
            {
                return null;
            }
            card.CityCode = cityCode;
            if (!int.TryParse(no.Substring(4, 2), out int county))
            {
                return null;
            }
            card.County = county;
            if (!DateTime.TryParseExact(no.Substring(6, 8), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime birthday))
            {
                return null;
            }
            card.Birthday = birthday;
            if (!int.TryParse(no.Substring(14, 3), out int serialNumber))
            {
                return null;
            }
            card.SerialNumber = serialNumber;
            card.Sex = int.Parse(no.Substring(16, 1).ToString()) % 2 == 0 ? "女" : "男";
            if (card.CheckCode != no[17])
            {
                return null;
            }
            return card;
        }
        /// <summary>
        /// 隐身转换。
        /// </summary>
        /// <param name="no">身份证字符串。</param>
        public static explicit operator IDCard(string no)
        {
            return ToIDCard(no);
        }
        /// <summary>
        /// 将身份证<see cref="IDCard"/>转换成字符串。
        /// </summary>
        /// <param name="card"></param>
        public static implicit operator string(IDCard card)
        {
            return card.ToString();
        }
    }
}
