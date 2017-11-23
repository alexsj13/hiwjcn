using Lib.data;
using Lib.helper;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lib.extension
{
    /// <summary>
    /// ValidateExtension
    /// </summary>
    public static class ValidateExtension
    {
        /// <summary>
        /// 判断是否都是非空字符串
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static bool IsAllPlumpString(this IEnumerable<string> arr) => 
            ValidateHelper.IsAllPlumpString(arr.ToArray());

        /// <summary>
        /// 判断是否满足数据库约束
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool IsValid<T>(this T model) where T : IDBTable
        {
            return model.IsValid(out var err);
        }

        /// <summary>
        /// 判断是否满足数据库约束
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public static bool IsValid<T>(this T model, out string err) where T : IDBTable
        {
            err = model.GetValidError();
            return !ValidateHelper.IsPlumpString(err);
        }

        /// <summary>
        /// 获取验证错误
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetValidError<T>(this T model) where T : IDBTable
        {
            return model.GetValidErrors().FirstOrDefault();
        }

        /// <summary>
        /// 获取验证错误
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<string> GetValidErrors<T>(this T model) where T : IDBTable
        {
            return ValidateHelper.CheckEntity_(model);
        }

        /// <summary>
        /// 用正则匹配
        /// </summary>
        /// <param name="s"></param>
        /// <param name="pattern"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool IsMatchedByPattern(this string s, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return RegexHelper.IsMatch(s, pattern, options);
        }

        /// <summary>
        /// 是手机号
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsMobilePhone(this string s)
        {
            return ValidateHelper.IsMobilePhone(s);
        }

        /// <summary>
        /// 是否是域名
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsDomain(this string s)
        {
            return ValidateHelper.IsDomain(s);
        }

        /// <summary>
        /// 是否是中文
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsChinese(this string s)
        {
            return ValidateHelper.IsChinese(s);
        }

        /// <summary>
        /// 判断是否是中文字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsChineaseStr(this string s)
        {
            return ValidateHelper.IsChineaseStr(s);
        }

        /// <summary>
        /// 是否是身份证号
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsIDCardNo(this string s)
        {
            return ValidateHelper.IsIDCardNo(s);
        }

        /// <summary>
        /// 是否是IP
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsIP(this string s)
        {
            return ValidateHelper.IsIP(s);
        }

        /// <summary>
        /// 数字或者字母
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNUMBER_OR_CHAR(this string s)
        {
            return ValidateHelper.IsNUMBER_OR_CHAR(s);
        }

        /// <summary>
        /// 是否是数字
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNumber(this string s)
        {
            return ValidateHelper.IsNumber(s);
        }

        /// <summary>
        /// 是否是邮箱
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsEmail(this string s)
        {
            return ValidateHelper.IsEmail(s);
        }
        /// <summary>
        /// 是否是电话
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsPhone(this string data)
        {
            return ValidateHelper.IsPhone(data);
        }
        /// <summary>
        /// 是否是URL
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsURL(this string data)
        {
            return ValidateHelper.IsURL(data);
        }
        /// <summary>
        /// 是否是日期
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsDate(this string data)
        {
            return ValidateHelper.IsDate(data);
        }
        /// <summary>
        /// 是否是时间
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsTime(this string data)
        {
            return ValidateHelper.IsTime(data);
        }
        /// <summary>
        /// 是否是float
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsFloat(this string data)
        {
            return ValidateHelper.IsFloat(data);
        }
        /// <summary>
        /// 是否是int
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsInt(this string data)
        {
            return ValidateHelper.IsInt(data);
        }

        /// <summary>
        /// 是否是Json
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsJson(this string data) => ValidateHelper.IsJson(data);

        /// <summary>
        /// 长度是否在范围内
        /// </summary>
        /// <param name="data"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool LenInRange(this string data, int min, int max)
        {
            return ValidateHelper.IsLenInRange(data, min, max);
        }
    }
}
