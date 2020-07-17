using System;

namespace DotNet
{
    /// <summary>
    /// GPS坐标信息。
    /// </summary>
    public struct GPSPoint
    {
        /// <summary>
        /// 使用经纬度初始化<see cref="GPSPoint"/>坐标。
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <param name="pointType">坐标类型</param>
        public GPSPoint(double longitude, double latitude, PointType pointType = PointType.Gps84)
        {
            Longitude = longitude;
            Latitude = latitude;
            PointType = pointType;
        }
        /// <summary>
        ///  坐标类型
        /// </summary>
        public PointType PointType { get; private set; }
        /// <summary>
        ///  经度
        /// </summary>
        public double Longitude { get; private set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double Latitude { get; private set; }

        /// <summary>
        /// 判断坐标是否处于国内
        /// </summary>
        public bool InChina
        {
            get
            {
                if (Longitude < 72.004 || Longitude > 137.8347)
                    return false;
                if (Latitude < 0.8293 || Latitude > 55.8271)
                    return false;
                return true;
            }
        }
        /// <summary>
        /// 计算两个坐标的距离(单位 米)
        /// </summary>
        /// <param name="point">要计算的第二个<see cref="GPSPoint"/></param>
        /// <returns></returns>
        public double GetLongLantiDistance(GPSPoint point)
        {

            if (point.PointType != PointType)
            {
                point = point.Converter(PointType);
            }
            return 6378137.0 * Math.Acos(Math.Sin(Latitude / 180 * Math.PI) * Math.Sin(point.Latitude / 180 * Math.PI) + Math.Cos(Latitude / 180 * Math.PI) * Math.Cos(point.Latitude / 180 * Math.PI) * Math.Cos((Longitude - point.Longitude) / 180 * Math.PI));
        }
        /// <summary>
        /// 将坐标转换到指定类型的坐标
        /// </summary>
        /// <param name="pointType">坐标类型</param>
        /// <returns></returns>
        public GPSPoint Converter(PointType pointType)
        {
            if (PointType == pointType)
            {
                return new GPSPoint(Latitude, Longitude, PointType);
            }
            double[] latlon = null;
            switch (PointType)
            {
                case PointType.Gps84:
                    switch (pointType)
                    {
                        case PointType.BD09:
                            latlon = GPSConverter.Gps84ToBd09(Latitude, Longitude);
                            break;
                        case PointType.GCJ02:
                            latlon = GPSConverter.Gps84ToGcj02(Latitude, Longitude);
                            break;
                    }
                    break;
                case PointType.BD09:
                    switch (pointType)
                    {
                        case PointType.Gps84:
                            latlon = GPSConverter.Bd09ToGps84(Latitude, Longitude);
                            break;
                        case PointType.GCJ02:
                            latlon = GPSConverter.Bd09ToGcj02(Latitude, Longitude);
                            break;
                    }
                    break;
                case PointType.GCJ02:
                    switch (pointType)
                    {
                        case PointType.Gps84:
                            latlon = GPSConverter.Gcj02ToGps84(Latitude, Longitude);
                            break;
                        case PointType.BD09:
                            latlon = GPSConverter.Gcj02ToBd09(Latitude, Longitude);
                            break;
                    }
                    break;
            }
            if (latlon != null)
            {
                return new GPSPoint(latlon[1], latlon[0], pointType);
            }
            return this;
        }
        /// <summary>
        /// 获取GCJ02坐标
        /// <para>高德、腾讯使用此坐标</para>
        /// </summary>
        public GPSPoint GCJ02
        {
            get
            {
                return Converter(PointType.GCJ02);
            }
        }
        /// <summary>
        /// GPS的原始坐标位
        /// <para>GPS设备获取的原始坐标</para>
        /// </summary>
        public GPSPoint Gps84
        {
            get
            {
                return Converter(PointType.Gps84);
            }
        }
        /// <summary>
        /// BD-09坐标 
        /// <para>百度坐标系</para>
        /// </summary>
        public GPSPoint BD09
        {
            get
            {
                return Converter(PointType.BD09);
            }
        }
        /// <summary>
        /// 转换成字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Longitude},{Latitude}";
        }

    }
    /// <summary>
    /// 坐标类型。
    /// </summary>
    public enum PointType
    {
        /// <summary>
        /// GPS的原始坐标位
        /// <para>GPS设备获取的原始坐标</para>
        /// </summary>
        Gps84 = 0,
        /// <summary>
        /// 高德、腾讯使用GCJ02坐标
        /// </summary>
        GCJ02 = 1,
        /// <summary>
        /// 百度坐标系 (BD-09)
        /// </summary>
        BD09 = 2
    }
    static class GPSConverter
    {
        private static readonly double pi = 3.1415926535897932384626;
        private static readonly double x_pi = 3.14159265358979324 * 3000.0 / 180.0;
        private static readonly double a = 6378245.0;
        private static readonly double ee = 0.00669342162296594323;
        private static double TransformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y
                    + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }
        private static double TransformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1
                    * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0
                    * pi)) * 2.0 / 3.0;
            return ret;
        }
        private static double[] Transform(double lat, double lon)
        {
            if (OutOfChina(lat, lon))
            {
                return new double[] { lat, lon };
            }
            double dLat = TransformLat(lon - 105.0, lat - 35.0);
            double dLon = TransformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double SqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * SqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / SqrtMagic * Math.Cos(radLat) * pi);
            double mgLat = lat + dLat;
            double mgLon = lon + dLon;
            return new double[] { mgLat, mgLon };
        }
        /// <summary>
        /// 判断GPS是否在不在国内
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static bool OutOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }
        /// <summary>
        /// Gps84坐标转换成Gps82坐标
        /// <para>高德、腾讯使用Gps82坐标</para>
        /// <para>GPS的原始坐标位Gps84</para>
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static double[] Gps84ToGcj02(double lat, double lon)
        {
            if (OutOfChina(lat, lon))
            {
                return new double[] { lat, lon };
            }
            double dLat = TransformLat(lon - 105.0, lat - 35.0);
            double dLon = TransformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double SqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * SqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / SqrtMagic * Math.Cos(radLat) * pi);
            double mgLat = lat + dLat;
            double mgLon = lon + dLon;
            return new double[] { Retain6(mgLat), Retain6(mgLon) };
        }
        /// <summary>
        /// Gps82坐标转换成Gps84坐标
        /// <para>高德、腾讯使用Gps82坐标</para>
        /// <para>GPS的原始坐标位Gps84</para>
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static double[] Gcj02ToGps84(double lat, double lon)
        {
            double[] gps = Transform(lat, lon);
            double lontitude = lon * 2 - gps[1];
            double latitude = lat * 2 - gps[0];
            return new double[] { latitude, lontitude };
        }
        /// <summary>
        /// 火星坐标系 (GCJ-02) 与百度坐标系 (BD-09) 的转换算法 将 GCJ-02 坐标转换成 BD-09 坐标 
        /// 高德谷歌转为百度
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static double[] Gcj02ToBd09(double lat, double lon)
        {
            double x = lon, y = lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * x_pi);
            double tempLon = z * Math.Cos(theta) + 0.0065;
            double tempLat = z * Math.Sin(theta) + 0.006;
            double[] gps = { tempLat, tempLon };
            return gps;
        }

        /// <summary>
        /// 火星坐标系 (GCJ-02) 与百度坐标系 (BD-09) 的转换算法 * * 将 BD-09 坐标转换成GCJ-02 坐标
        /// 百度坐标转为高德谷歌坐标
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static double[] Bd09ToGcj02(double lat, double lon)
        {
            double x = lon - 0.0065, y = lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            double tempLon = z * Math.Cos(theta);
            double tempLat = z * Math.Sin(theta);
            double[] gps = { tempLat, tempLon };
            return gps;
        }

        /// <summary>
        /// gps84转为bd09
        /// GPS坐标转为百度坐标
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static double[] Gps84ToBd09(double lat, double lon)
        {
            double[] gcj02 = Gps84ToGcj02(lat, lon);
            double[] bd09 = Gcj02ToBd09(gcj02[0], gcj02[1]);
            return bd09;
        }
        /// <summary>
        /// 百度坐标转成GPS坐标
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static double[] Bd09ToGps84(double lat, double lon)
        {
            double[] gcj02 = Bd09ToGcj02(lat, lon);
            double[] gps84 = Gcj02ToGps84(gcj02[0], gcj02[1]);
            //保留小数点后六位  
            gps84[0] = Retain6(gps84[0]);
            gps84[1] = Retain6(gps84[1]);
            return gps84;
        }
        /// <summary>
        /// 保留小数点后六位 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private static double Retain6(double num)
        {
            return Math.Round(num, 6);
        }
    }
}
