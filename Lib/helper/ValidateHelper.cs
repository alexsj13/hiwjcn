using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using Lib.core;
using System.Globalization;
using Lib.io;
using Lib.data;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Lib.extension;
using Newtonsoft.Json;

namespace Lib.helper
{
    /*
     * https://github.com/arasatasaygin/is.js/blob/master/is.js
     * 
         var regexes = {
        affirmative: /^(?:1|t(?:rue)?|y(?:es)?|ok(?:ay)?)$/,
        alphaNumeric: /^[A-Za-z0-9]+$/,
        caPostalCode: /^(?!.*[DFIOQU])[A-VXY][0-9][A-Z]\s?[0-9][A-Z][0-9]$/,
        creditCard: /^(?:(4[0-9]{12}(?:[0-9]{3})?)|(5[1-5][0-9]{14})|(6(?:011|5[0-9]{2})[0-9]{12})|(3[47][0-9]{13})|(3(?:0[0-5]|[68][0-9])[0-9]{11})|((?:2131|1800|35[0-9]{3})[0-9]{11}))$/,
        dateString: /^(1[0-2]|0?[1-9])([\/-])(3[01]|[12][0-9]|0?[1-9])(?:\2)(?:[0-9]{2})?[0-9]{2}$/,
        email: /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$/i, // eslint-disable-line no-control-regex
        eppPhone: /^\+[0-9]{1,3}\.[0-9]{4,14}(?:x.+)?$/,
        hexadecimal: /^(?:0x)?[0-9a-fA-F]+$/,
        hexColor: /^#?([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$/,
        ipv4: /^(?:(?:\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])\.){3}(?:\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])$/,
        ipv6: /^((?=.*::)(?!.*::.+::)(::)?([\dA-F]{1,4}:(:|\b)|){5}|([\dA-F]{1,4}:){6})((([\dA-F]{1,4}((?!\3)::|:\b|$))|(?!\2\3)){2}|(((2[0-4]|1\d|[1-9])?\d|25[0-5])\.?\b){4})$/i,
        nanpPhone: /^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/,
        socialSecurityNumber: /^(?!000|666)[0-8][0-9]{2}-?(?!00)[0-9]{2}-?(?!0000)[0-9]{4}$/,
        timeString: /^(2[0-3]|[01]?[0-9]):([0-5]?[0-9]):([0-5]?[0-9])$/,
        ukPostCode: /^[A-Z]{1,2}[0-9RCHNQ][0-9A-Z]?\s?[0-9][ABD-HJLNP-UW-Z]{2}$|^[A-Z]{2}-?[0-9]{4}$/,
        url: /^(?:(?:https?|ftp):\/\/)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:\/\S*)?$/i,
        usZipCode: /^[0-9]{5}(?:-[0-9]{4})?$/
    };
    */

    /// <summary>
    /// 验证帮助类
    /// </summary>
    public static class ValidateHelper
    {

        private static readonly Regex RegNumber = new Regex("^[0-9]+$", RegexOptions.Compiled);
        private static readonly Regex RegNumberSign = new Regex("^[+-]?[0-9]+$", RegexOptions.Compiled);
        private static readonly Regex RegDecimal = new Regex("^[0-9]+[.]?[0-9]+$", RegexOptions.Compiled);
        private static readonly Regex RegDecimalSign = new Regex("^[+-]?[0-9]+[.]?[0-9]+$", RegexOptions.Compiled); //等价于^[+-]?\d+[.]?\d+$
        private static readonly Regex RegEmail = new Regex(@"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$", RegexOptions.Compiled);//w 英文字母或数字的字符串，和 [a-zA-Z0-9] 语法一样 
        private static readonly Regex RegCHZN = new Regex("[\u4e00-\u9fa5]", RegexOptions.Compiled);

        /// <summary>
        /// 判断是否是邮件地址，来自nop的方法
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsEmail(string s)
        {
            return IsPlumpString(s) && RegexHelper.IsMatch(s, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        }

        /// <summary>
        /// 判断是不是url
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsURL(string s)
        {
            return IsPlumpString(s) && RegexHelper.IsMatch(s, @"[a-zA-z]+://[^s]*");
        }

        /// <summary>
        /// 是手机号
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsMobilePhone(string s)
        {
            return IsPlumpString(s) && RegexHelper.IsMatch(s, @"^(13[0-9]|14[5|7]|15[0|1|2|3|5|6|7|8|9]|18[0|1|2|3|5|6|7|8|9])\d{8}$");
        }

        /// <summary>
        /// 是否为固话号
        /// </summary>
        public static bool IsPhone(string s)
        {
            return IsPlumpString(s) && RegexHelper.IsMatch(s, @"^(\d{3,4}-?)?\d{7,8}$");
        }

        /// <summary>
        /// 是否是域名
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsDomain(string s)
        {
            return IsPlumpString(s) && RegexHelper.IsMatch(s, @"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+$");
        }

        /// <summary>
        /// 是否是中文
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsChinese(string s)
        {
            return IsPlumpString(s) && RegexHelper.IsMatch(s, @"^[\u4e00-\u9fa5]{0,}$");
        }

        /// <summary>
        /// 是否是身份证号
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsIDCardNo(string s)
        {
            return IsPlumpString(s) && RegexHelper.IsMatch(s, @"^(^\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$");
        }

        /// <summary>
        /// 是否为日期
        /// </summary>
        public static bool IsDate(string s)
        {
            //return IsPlumpString(s) && RegexHelper.IsMatch(s, @"(\d{4})-(\d{1,2})-(\d{1,2})");
            return DateTime.TryParse(s, out var re);
        }

        /// <summary>
        /// 是否是IP
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsIP(string s)
        {
            return IsPlumpString(s) && RegexHelper.IsMatch(s, @"\d+\.\d+\.\d+\.\d+");
        }

        /// <summary>
        /// 是否是时间
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsTime(string s)
        {
            return IsPlumpString(s) && RegexHelper.IsMatch(s, @"^(([0-1]?[0-9])|([2][0-3])):([0-5]?[0-9])(:([0-5]?[0-9]))?$");
        }

        /// <summary>
        /// 数字或者字母
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNUMBER_OR_CHAR(string s)
        {
            return IsPlumpString(s) && RegexHelper.IsMatch(s, @"^[A-Za-z0-9]{4,40}$");
        }

        /// <summary>
        /// 是否是数字
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNumber(string s)
        {
            return IsPlumpString(s) && RegexHelper.IsMatch(s, @"^[0-9]*$");
        }

        /// <summary>
        /// 是否是数值(包括整数和小数)
        /// </summary>
        public static bool IsFloat(string s)
        {
            return float.TryParse(s, out var re);
        }

        /// <summary>
        /// 是否是double
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsDouble(string s)
        {
            return double.TryParse(s, out var re);
        }

        /// <summary>
        /// 判断是数字，空返回false（前面可以带负号）
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsInt(string s)
        {
            return int.TryParse(s, out var re);
        }

        /// <summary>
        /// 判断是字母，空返回false
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsChar(string s)
        {
            if (!IsPlumpString(s)) { return false; }
            return s.ToArray().All(x => (x >= 'a' && x <= 'z') || (x >= 'A' && x <= 'Z'));
        }

        /// <summary>
        /// 判断是否是中文字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsChineaseStr(string str)
        {
            if (!IsPlumpString(str))
            {
                return false;
            }
            return str.ToArray().All(x => x >= 0x4e00 && x <= 0x9fbb);
        }

        /// <summary>
        /// 简单判断是否是json，不一定全部正确
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static bool IsJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject(json) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查颜色值是否为3/6位的合法颜色只支持#ffffff格式，rgb(0,0,0)格式不能验证通过
        /// </summary>
        /// <param name="color">待检查的颜色</param>
        /// <returns></returns>
        public static bool IsColor(string color)
        {
            if (!IsPlumpString(color)) { return false; }
            color = color.Trim();
            if (color.StartsWith("#")) { return false; }
            color = color.Trim('#');

            if (color.Length != 3 && color.Length != 6) { return false; }

            //不包含0-9  a-f以外的字符
            if (!RegexHelper.IsMatch(color, "[^0-9a-f]", RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }

        public static readonly string[] ImageExtesions = new string[] { ".jpg", ".png", ".gif", ".bmp", ".jpeg" };

        /// <summary>
        /// 是否是图片
        /// </summary>
        /// <param name="urlOrPathOrName"></param>
        /// <returns></returns>
        public static bool IsImage(string urlOrPathOrName)
        {
            urlOrPathOrName = ConvertHelper.GetString(urlOrPathOrName).Trim().ToLower();
            foreach (var ext in ImageExtesions)
            {
                if (urlOrPathOrName.EndsWith(ext)) { return true; }
            }
            return false;
        }

        /// <summary>
        /// 检查字节数组是否是可以转换为图片
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsImage(byte[] b)
        {
            try
            {
                using (var ms = new MemoryStream(b))
                {
                    using (var img = Image.FromStream(ms))
                    {
                        return !img.Size.IsEmpty;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断文件流是否为UTF8字符集
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <returns>判断结果</returns>
        private static bool IsUTF8(FileStream stream)
        {
            byte cOctets = 0;  // octets to go in this UTF-8 encoded character 
            byte chr;
            bool bAllAscii = true;
            for (int i = 0; i < stream.Length; ++i)
            {
                chr = (byte)stream.ReadByte();

                if ((chr & 0x80) != 0) { bAllAscii = false; }

                if (cOctets == 0)
                {
                    if (chr >= 0x80)
                    {
                        do
                        {
                            chr <<= 1;
                            cOctets++;
                        }
                        while ((chr & 0x80) != 0);

                        cOctets--;
                        if (cOctets == 0) { return false; }
                    }
                }
                else
                {
                    if ((chr & 0xC0) != 0x80) { return false; }
                    cOctets--;
                }
            }

            if (cOctets > 0)
            {
                return false;
            }

            if (bAllAscii)
            {
                return false;
            }

            return true;
        }

        #region 判断是否是空数据

        /// <summary>
        /// 判断是否是有值的list
        /// </summary>
        public static bool IsPlumpList<T>(IEnumerable<T> list) => list?.Count() > 0;

        /// <summary>
        /// 包含长度大于0的item，并把他们找出来
        /// </summary>
        public static bool IsPlumpListAfterFilterMeaninglessData(IEnumerable<string> list, out List<string> filtered)
        {
            filtered = ConvertHelper.NotNullList(list).Where(x => IsPlumpString(x)).ToList();
            return filtered.Count > 0;
        }

        /// <summary>
        /// 判断是否是有值的字典
        /// </summary>
        public static bool IsPlumpDict<K, V>(IDictionary<K, V> dict) => dict?.Count > 0;

        /// <summary>
        /// 去除两端空格后判断是否是非空字符串
        /// </summary>
        public static bool IsPlumpStringAfterTrim(string str) => str?.Trim()?.Length > 0;

        /// <summary>
        /// 判断是否是非空字符串
        /// </summary>
        public static bool IsPlumpString(string str) => str?.Length > 0;

        /// <summary>
        /// 判断是否都是非空字符串
        /// </summary>
        public static bool IsAllPlumpString(params string[] strs)
        {
            if (!IsPlumpList(strs)) { throw new Exception("至少需要一个参数"); }
            return strs.All(x => IsPlumpString(x));
        }

        /// <summary>
        /// 判断数组里至少有一个非空字符串
        /// </summary>
        public static bool IsAnyPlumpString(params string[] strs)
        {
            if (!IsPlumpList(strs)) { throw new Exception("至少需要一个参数"); }
            return strs.Any(x => IsPlumpString(x));
        }
        #endregion

        /// <summary>
        /// 判断字符串的长度是否在范围之内，str可以为空
        /// </summary>
        public static bool IsLenInRange(string str, int min, int max)
        {
            var len = str?.Length ?? 0;
            return len >= min && len <= max;
        }

        /// <summary>
        /// 判断两个集合是否有交集
        /// </summary>
        public static bool HasInterSection<T>(IEnumerable<T> a, IEnumerable<T> b) => 
            Com.GetInterSection(a, b).Count > 0;

        /// <summary>
        /// 判断一个对象是否是某个类型
        /// </summary>
        public static bool Is<T>(object obj)
        {
            return obj != null && obj is T;
        }

        /// <summary>
        /// 判断是相同引用
        /// </summary>
        public static bool IsReferenceEquals(object obj1, object obj2)
        {
            return object.ReferenceEquals(obj1, obj2);
        }

        /// <summary>
        /// 根据attribute验证model
        /// </summary>
        [Obsolete("使用CheckEntity_")]
        public static List<string> CheckEntity<T>(T model) where T : IDBTable
        {
            var list = new List<string>();
            if (model == null)
            {
                list.Add("实体对象不能为Null");
                return list;
            }

            //checker
            Func<ValidationAttribute, object, bool> CheckProp = (validator, data) =>
            {
                if (validator != null && !validator.IsValid(data))
                {
                    list.Add(validator.ErrorMessage);
                    return false;
                }
                return true;
            };

            foreach (var prop in model.GetType().GetProperties())
            {
                if (prop.GetCustomAttributes<NotMappedAttribute>().Any()) { continue; }
                //自定义
                if (prop.GetCustomAttributes<CustomValidationAttribute>().FirstOrDefault() != null)
                {
                    throw new NotSupportedException("不支持CustomValidationAttribute");
                }

                var value = prop.GetValue(model);

                //是否可为空
                if (!CheckProp(prop.GetCustomAttributes<RequiredAttribute>().FirstOrDefault(), value)) { continue; }
                //字符串长度
                if (!CheckProp(prop.GetCustomAttributes<StringLengthAttribute>().FirstOrDefault(), value)) { continue; }
                //正则表达式
                if (!CheckProp(prop.GetCustomAttributes<RegularExpressionAttribute>().FirstOrDefault(), value)) { continue; }
                //范围
                if (!CheckProp(prop.GetCustomAttributes<RangeAttribute>().FirstOrDefault(), value)) { continue; }
                //最大长度
                if (!CheckProp(prop.GetCustomAttributes<MaxLengthAttribute>().FirstOrDefault(), value)) { continue; }
                //最小长度
                if (!CheckProp(prop.GetCustomAttributes<MinLengthAttribute>().FirstOrDefault(), value)) { continue; }
                //电话
                if (!CheckProp(prop.GetCustomAttributes<PhoneAttribute>().FirstOrDefault(), value)) { continue; }
                //邮件
                if (!CheckProp(prop.GetCustomAttributes<EmailAddressAttribute>().FirstOrDefault(), value)) { continue; }
                //URL
                if (!CheckProp(prop.GetCustomAttributes<UrlAttribute>().FirstOrDefault(), value)) { continue; }
                //信用卡
                if (!CheckProp(prop.GetCustomAttributes<CreditCardAttribute>().FirstOrDefault(), value)) { continue; }
            }

            list = list.Where(x => IsPlumpString(x)).Distinct().ToList();

            if (ValidateHelper.IsPlumpList(list))
            {
                Console.WriteLine(",".Join_(list));
            }

            return list;
        }

        /// <summary>
        /// 根据attribute验证model
        /// </summary>
        public static List<string> CheckEntity_<T>(T model) where T : IDBTable
        {
            var list = new List<string>();
            if (model == null)
            {
                list.Add("实体对象不能为Null");
                return list;
            }

            //checker
            bool CheckProp(IEnumerable<ValidationAttribute> validators, object data, PropertyInfo p)
            {
                foreach (var validator in ConvertHelper.NotNullList(validators))
                {
                    if (!validator.IsValid(data))
                    {
                        var msg = ConvertHelper.GetString(validator.ErrorMessage).Trim();
                        if (!ValidateHelper.IsPlumpString(msg))
                        {
                            msg = $"字段{p.Name}未通过{validator.GetType().Name}标签的验证";
                        }
                        list.Add(msg);
                        return false;
                    }
                }
                return true;
            };

            foreach (var prop in model.GetType().GetProperties())
            {
                if (prop.GetCustomAttributes<NotMappedAttribute>().Any()) { continue; }

                var value = prop.GetValue(model);

                if (!CheckProp(prop.GetCustomAttributes_<ValidationAttribute>(), value, prop)) { continue; }
            }

            list = list.Where(x => IsPlumpString(x)).Distinct().ToList();

            if (ValidateHelper.IsPlumpList(list))
            {
                Console.WriteLine(",".Join_(list));
            }

            return list;
        }

        /// <summary>
        /// 两个json有相同的结构
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        public static bool SameJsonStructure(this (string, string) tuple)
        {
            return JsonHelper.HasSameStructure(tuple.Item1, tuple.Item2);
        }
    }
}
