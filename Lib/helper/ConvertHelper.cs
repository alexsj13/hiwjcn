using System;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Lib.io;
using Lib.extension;

namespace Lib.helper
{
    /// <summary>
    /// 类型帮助类
    /// </summary>
    public static class ConvertHelper
    {
        #region 字符串转换

        /// <summary>
        /// 删除首尾无意义的字符，比如空格和制表符等
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TrimMeaninglessChars(string str)
        {
            str = GetString(str);
            if (str.Length <= 0) { return str; }
            var trim = new string[] { "\n", "\t", ' '.ToString() };
            while (true)
            {
                var len = str.Length;
                foreach (var t in trim)
                {
                    str = StringHelper.Trim(str, t, ignoreCase: true);
                }

                //break if trim end
                var not_trimed = str.Length == len;
                if (not_trimed)
                {
                    break;
                }
            }
            return str;
        }

        /// <summary>
        /// 获取非空字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetString(object obj, string str = "")
        {
            return (obj == null) ? str : obj.ToString();
        }

        /// <summary>
        /// 如果不是有效字符串就变成null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EmptyAsNull(string str)
        {
            if (!ValidateHelper.IsPlumpStringAfterTrim(str))
            {
                return null;
            }
            return str;
        }
        #endregion

        #region base64
        /// <summary>
        /// 获取base64字符串
        /// </summary>
        public static string GetBase64String(string str, Encoding encoding = null)
        {
            if (encoding == null) { encoding = Encoding.Default; }
            return GetBase64String(encoding.GetBytes(str));
        }

        /// <summary>
        /// 获取base64字符串
        /// </summary>
        public static string GetBase64String(byte[] bs)
        {
            return Convert.ToBase64String(bs);
        }

        /// <summary>
        /// base64字符串转为普通字符串
        /// </summary>
        public static string GetStringFromBase64String(string str, Encoding encoding = null)
        {
            var bs = GetBytesFromBase64String(str);
            return encoding.GetString(bs);
        }
        /// <summary>
        /// base64字符串转为byte[]
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetBytesFromBase64String(string str)
        {
            return Convert.FromBase64String(str);
        }
        #endregion

        #region 数字转换
        /// <summary>
        /// 转换为int类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        public static int GetInt(object obj, int? deft = default(int))
        {
            if (int.TryParse(GetString(obj), out int res))
            {
                return res;
            }
            return deft ?? throw new ArgumentException($"{obj}无法转为{res.GetType()}");
        }
        /// <summary>
        /// 转换为int64类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        public static Int64 GetInt64(object obj, Int64? deft = default(Int64))
        {
            if (Int64.TryParse(GetString(obj), out var res))
            {
                return res;
            }
            return deft ?? throw new ArgumentException($"{obj}无法转为{res.GetType()}");
        }
        /// <summary>
        /// 获取float类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        public static float GetFloat(object obj, float? deft = default(float))
        {
            if (float.TryParse(GetString(obj), out var res))
            {
                return res;
            }
            return deft ?? throw new ArgumentException($"{obj}无法转为{res.GetType()}");
        }
        /// <summary>
        /// 获取long类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        public static long GetLong(object obj, long? deft = default(long))
        {
            if (long.TryParse(GetString(obj), out var res))
            {
                return res;
            }
            return deft ?? throw new ArgumentException($"{obj}无法转为{res.GetType()}");
        }
        /// <summary>
        /// 获取double类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        public static double GetDouble(object obj, double? deft = default(double))
        {
            if (double.TryParse(GetString(obj), out var res))
            {
                return res;
            }
            return deft ?? throw new ArgumentException($"{obj}无法转为{res.GetType()}");
        }
        /// <summary>
        /// 获取decimal类型，常常用于货币
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        public static decimal GetDecimal(object obj, decimal? deft = default(decimal))
        {
            if (decimal.TryParse(GetString(obj), out var res))
            {
                return res;
            }
            return deft ?? throw new ArgumentException($"{obj}无法转为{res.GetType()}");
        }
        #endregion

        #region 日期转换

        /// <summary>
        /// 转换为日期格式
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(object obj, DateTime? deft)
        {
            if (DateTime.TryParse(GetString(obj), out var res))
            {
                return res;
            }
            return deft ?? throw new ArgumentException($"{obj}无法转为{res.GetType()}");
        }

        /// <summary>
        /// 转换为日期格式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DateTime GetDateTime(object obj)
        {
            return GetDateTime(obj, DateTime.Now);
        }

        #endregion

        #region 图片转换
        public static byte[] BitmapToBytes(Bitmap bm, ImageFormat format = null)
        {
            if (bm == null) { throw new Exception("bitmap为空"); }
            if (format == null) { format = ImageFormat.Png; }

            using (var ms = new MemoryStream())
            {
                bm.Save(ms, format);
                return ms.ToArray();
            }
        }

        public static Bitmap BytesToBitmap(byte[] buffer)
        {
            if (!ValidateHelper.IsPlumpList(buffer))
            {
                throw new Exception("bytes is null");
            }
            using (var ms = new MemoryStream())
            {
                ms.Write(buffer, 0, buffer.Length);
                var bmp = new Bitmap(ms);
                return bmp;
            }
        }

        public static string BytesToBase64(byte[] b)
        {
            return Convert.ToBase64String(b);
        }

        public static byte[] Base64ToBytes(string b64)
        {
            return Convert.FromBase64String(b64);
        }

        public static Bitmap ImgFromBase64(string Img)
        {
            return BytesToBitmap(Base64ToBytes(Img));
        }

        /*
         string username = "chenxizhang";
byte[] buffer = System.Text.Encoding.UTF8.GetBytes(username); //这是把字符串转成字节数组
Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer)); //这是把字节数组再转回到字符串

Console.WriteLine(BitConverter.ToString(buffer)); // 这是把字节数组当作字符串输出（长度较长）
Console.WriteLine(Convert.ToBase64String(buffer)); //这是把字节数组当作一种所谓的Base64的字符串格式输出
         */

        public static string GetByteString(string str, Encoding encode = null)
        {
            if (encode == null) { encode = Encoding.UTF8; }
            byte[] b = encode.GetBytes(ConvertHelper.GetString(str));
            return BitConverter.ToString(b);
        }

        /// <summary>
        /// 将字符串编码为Base64字符串
        /// </summary>
        /// <param name="str">要编码的字符串</param>
        public static string Base64Encode(string str)
        {
            byte[] barray = Encoding.Default.GetBytes(str);
            return Convert.ToBase64String(barray);
        }

        /// <summary>
        /// 将Base64字符串解码为普通字符串
        /// </summary>
        /// <param name="str">要解码的字符串</param>
        public static string Base64Decode(string str)
        {
            byte[] barray = Convert.FromBase64String(str);
            return Encoding.Default.GetString(barray);
        }

        /// <summary>
        /// 使用stream的length来定义数组，只有stream可以查找(seek)的情况下使用
        /// 用完不会关闭流
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] StreamToBytes(Stream stream)
        {
            if (stream == null || !stream.CanRead) { throw new Exception("流为空，或者不可读"); }
            if (stream.CanSeek)
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return bytes;
            }
            else
            {
                var bytes = new List<byte>();
                int b = 0;
                while (true)
                {
                    if ((b = stream.ReadByte()) == -1)
                    {
                        break;
                    }
                    bytes.Add((byte)b);
                }
                return bytes.ToArray();
            }
        }

        /// <summary>
        /// 返回memorystream，即便流已经关闭
        /// </summary>
        /// <returns></returns>
        public static byte[] MemoryStreamToBytes(MemoryStream stream)
        {
            return stream.ToArray();
        }

        /// <summary>
        /// stream转成string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string StreamToString(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        #endregion

        #region 其他转换
        /// <summary>
        /// 把ilist转换为list
        /// 不返回null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> NotNullList<T>(IEnumerable<T> list) => list?.ToList() ?? new List<T>() { };

        [Obsolete]
        public static List<T> NotNullEnumerable<T>(IEnumerable<T> list) => ConvertHelper.NotNullList(list);
        #endregion

        #region 通用转换
        /// <summary>
        /// 通用数据转换
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        public static T ChangeType<T>(object obj, T deft = default(T))
        {
            try
            {
                var o = Convert.ChangeType(obj, typeof(T));
                if (o is T re)
                {
                    return re;
                }
                //强转
                return (T)o;
            }
            catch
            {
                return deft;
            }
        }
        #endregion
    }

}