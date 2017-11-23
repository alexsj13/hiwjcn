using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;
using Lib.core;

namespace Lib.extension
{
    /// <summary>
    /// StringExtension
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 后面加url的斜杠
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string EnsureTrailingSlash(this string input)
        {
            if (!input.EndsWith("/"))
            {
                return input + "/";
            }

            return input;
        }

        /// <summary>
        /// 中间用*替代
        /// </summary>
        public static string HideForSecurity(this string str,
            int start_count = 1, int end_count = 1, int mark_count = 5)
        {
            var list = str.ToCharArray().ToList();
            if (list.Count < start_count + end_count) { return str; }

            var start = list.Take(start_count).AsString();

            var mid = new int[mark_count].Select(x => '*').AsString();

            var end = list.Reverse_().Take(end_count).Reverse_().AsString();

            return $"{start}{mid}{end}";
        }

        /// <summary>
        /// 用@分割邮件地址
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static (string user_name, string host) SplitEmail(this string email)
        {
            var sp = email.Split('@');
            if (sp.Length != 2 || !ValidateHelper.IsAllPlumpString(sp[0], sp[1]))
            {
                throw new Exception("邮件格式错误");
            }
            return (sp[0], sp[1]);
        }

        /// <summary>
        /// trim string
        /// </summary>
        public static string Trim_(this string str, string tm, bool ignore_case = true)
        {
            return StringHelper.Trim(str, tm, ignore_case);
        }

        /// <summary>
        /// 如果不是有效字符串就转换为null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EmptyAsNull(this string str) => ConvertHelper.EmptyAsNull(str);

        /// <summary>
        /// 找到#标签#
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<string> FindTags(this string s)
        {
            return Com.FindTagsFromStr(s);
        }

        /// <summary>
        /// 找到@的人
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<string> FindAt(this string s)
        {
            return Com.FindAtFromStr(s);
        }

        /// <summary>
        /// 获取sha1
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToSHA1(this string s)
        {
            return SecureHelper.GetSHA1(s);
        }

        /// <summary>
        /// 转为md5
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToMD5(this string data)
        {
            return SecureHelper.GetMD5(data);
        }

        /// <summary>
        /// 获取拼音
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetPinyin(this string s)
        {
            return Com.Pinyin(s);
        }

        /// <summary>
        /// 获取拼音首字母
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetSpell(this string s)
        {
            return Com.GetSpell(s);
        }

        /// <summary>
        /// 提取url后面的参数
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ExtractUrlParams(this string s)
        {
            return Com.ExtractUrlParams(s);
        }

        /// <summary>
        /// 模仿python的join
        /// </summary>
        /// <param name="sep"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string Join<T>(this string sep, IEnumerable<T> list) => sep.Join_(list);

        /// <summary>
        /// 模仿python的join
        /// </summary>
        /// <param name="sep"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string Join_<T>(this string sep, IEnumerable<T> list)
        {
            var arrs = list.Select(x => ConvertHelper.GetString(x)).ToArray();
            return string.Join(sep, arrs);
        }

        /// <summary>
        /// 模仿python中的格式化
        /// </summary>
        /// <param name="s"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Format(this string s, params object[] args) => s.Format_(args);

        /// <summary>
        /// 模仿python中的格式化
        /// </summary>
        /// <param name="s"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Format_(this string s, params object[] args) => string.Format(s, args);

        /// <summary>
        /// base64变string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Base64ToString(this string s) => ConvertHelper.Base64Decode(s);

        /// <summary>
        /// string变base64
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string StringToBase64(this string s) => ConvertHelper.Base64Encode(s);

        /// <summary>
        /// 字节数组到base64
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public static string BytesToBase64(this byte[] bs) => ConvertHelper.BytesToBase64(bs);

        /// <summary>
        /// base64到字节数组
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] Base64ToBytes(this string s) => ConvertHelper.Base64ToBytes(s);

        /// <summary>
        /// 获取字节数组，可以指定编码
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this string s, Encoding encoding = null) =>
            (encoding ?? ConfigHelper.Instance.SystemEncoding).GetBytes(s);
    }
}
