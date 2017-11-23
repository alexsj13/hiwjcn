using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Lib.helper;
using Lib.core;
using System.Collections.Specialized;

namespace Lib.extension
{
    public static class ConvertExtension
    {
        /// <summary>
        /// 转换为整型
        /// </summary>
        public static int ToInt(this string data, int? deft = default(int))
        {
            return ConvertHelper.GetInt(data, deft);
        }

        /// <summary>
        /// 转为长整型
        /// </summary>
        /// <param name="data"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        public static long ToLong(this string data, long? deft = default(long)) =>
            ConvertHelper.GetLong(data, deft);

        /// <summary>
        /// 转为float
        /// </summary>
        public static float ToFloat(this string data, float? deft = default(float))
        {
            return ConvertHelper.GetFloat(data, deft);
        }

        /// <summary>
        /// 转换为双精度浮点数,并按指定的小数位4舍5入
        /// </summary>
        public static double ToDouble(this string data, int? digits = null, double? deft = default(double))
        {
            var db = ConvertHelper.GetDouble(data, deft);
            if (digits != null)
            {
                return Math.Round(db, digits.Value);
            }
            return db;
        }

        /// <summary>
        /// 转换为高精度浮点数,并按指定的小数位4舍5入
        /// </summary>
        public static decimal ToDecimal(this string data, int? digits = null, decimal? deft = default(decimal))
        {
            var dec = ConvertHelper.GetDecimal(data, deft);
            if (digits != null)
            {
                return Math.Round(dec, digits.Value);
            }
            return dec;
        }

        /// <summary>
        /// 转换为日期
        /// </summary>
        public static DateTime ToDateTime(this string data, DateTime? deft)
        {
            return ConvertHelper.GetDateTime(data, deft);
        }

        /// <summary>
        /// 转为日期，默认值为当前时间
        /// </summary>
        public static DateTime ToDateTime(this string data)
        {
            return data.ToDateTime(DateTime.Now);
        }

        /// <summary>
        /// 大于0是true，否则false
        /// </summary>
        public static bool ToBool(this int data) => data > 0;

        /// <summary>
        /// true是1，false是0
        /// </summary>
        public static int ToBoolInt(this bool data) => data ? 1 : 0;

        private static readonly IReadOnlyCollection<string> bool_string_list =
            new List<string>() { "1", "true", "yes", "on", "success", "ok", true.ToString().ToLower() }.AsReadOnly();

        /// <summary>
        /// 转换为布尔值
        /// </summary>
        public static bool ToBool(this string data) => bool_string_list.Contains(data.ToLower());

        /// <summary>
        /// true为1，false为0
        /// </summary>
        public static int ToBoolInt(this string data) => data.ToBool().ToBoolInt();

        /// <summary>
        /// 转为json 
        /// </summary>
        public static string ToJson(this object data) => JsonHelper.ObjectToJson(data);

        /// <summary>
        /// json转为实体
        /// </summary>
        public static T JsonToEntity<T>(this string json, bool throwIfException = true, T deft = default(T))
        {
            try
            {
                return JsonHelper.JsonToEntity<T>(json);
            }
            catch when (!throwIfException)
            {
                return deft;
            }
        }

        /// <summary>
        /// 映射
        /// </summary>
        public static T MapTo<T>(this object data) => MapperHelper.GetMappedEntity<T>(data);

        /// <summary>
        /// 映射
        /// </summary>
        public static void MapTo<T>(this object data, ref T entity, string[] notmap = null)
        {
            MapperHelper.MapEntity(ref entity, data, notmap);
        }

        /// <summary>
        /// 格式化数字，获取xxx xxxk xxxw
        /// </summary>
        public static string SimpleNumber(this Int64 num) => Com.SimpleNumber(num);

    }
}
