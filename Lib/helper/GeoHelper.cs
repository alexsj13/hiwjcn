using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.helper
{
    /// <summary>
    /// 地理位置相关帮助类
    /// </summary>
    public static class GeoHelper
    {
        #region 私有

        /// <summary>
        /// 地球半径
        /// </summary>
        private const double EARTH_RADIUS = 6378.137;

        /// <summary>
        /// 弧度转换
        /// </summary>
        private static double Rad(double d) => d * Math.PI / 180.0;

        /// <summary>
        /// 验证经纬度数值正确性
        /// </summary>
        private static bool IsGeoValid(GeoInfo point) =>
            point != null && Math.Abs(point.Lat) <= 90 && Math.Abs(point.Lon) <= 180;

        #endregion

        /// <summary>
        /// 计算距离
        /// </summary>
        public static double GetDistanceInKm(double lat1, double lon1, double lat2, double lon2, double? defaultValue = -1) =>
            GetDistanceInKm(new GeoInfo() { Lat = lat1, Lon = lon1 }, new GeoInfo() { Lat = lat2, Lon = lon2 }, defaultValue);

        /// <summary>
        /// 计算距离，如返回最大值需要检查数据
        /// </summary>
        public static double GetDistanceInKm(GeoInfo startPoint, GeoInfo endPoint, double? defaultValue)
        {
            if (!IsGeoValid(startPoint) || !IsGeoValid(endPoint))
            {
                return defaultValue ?? throw new Exception("无法计算距离，传入坐标错误");
            }

            var startlatrad = Rad(startPoint.Lat);
            var endlatrad = Rad(endPoint.Lat);
            var a = startlatrad - endlatrad;
            var b = Rad(startPoint.Lon - endPoint.Lon);

            return 2 * EARTH_RADIUS * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
                                                     Math.Cos(startlatrad) *
                                                     Math.Cos(endlatrad) *
                                                     Math.Pow(Math.Sin(b / 2), 2)));
        }

    }

    public class GeoInfo
    {
        /// <summary>
        /// 纬度
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public double Lon { get; set; }
    }
}
