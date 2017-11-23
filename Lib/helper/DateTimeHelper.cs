using Lib.helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using System.Globalization;

namespace Lib.helper
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// 时间戳生成规则
        /// </summary>
        /// <returns></returns>
        public static Int64 GetTimeStamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (Int64)ts.TotalSeconds;
        }
        public static string GetTime()
        {
            return DateTime.Now.ToString("HH:mm");
        }
        public static string GetDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
        public static string GetChineseDate()
        {
            return DateTime.Now.ToString("yyyy年MM月dd日");
        }
        public static string GetDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public static int GetDayOfWeek()
        {
            return (int)DateTime.Now.DayOfWeek;
        }
        public static int GetDayOfYear()
        {
            return DateTime.Now.DayOfYear;
        }

        public static string GetDateTimeStringFromCulture(DateTime time, CultureInfo cul) =>
            time.ToString(cul.DateTimeFormat);

        /// <summary>
        /// 获取系统时区(不返回null)
        /// </summary>
        /// <returns></returns>
        public static ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZone()
        {
            var collection = TimeZoneInfo.GetSystemTimeZones();
            if (collection == null)
            {
                collection = new ReadOnlyCollection<TimeZoneInfo>(new List<TimeZoneInfo>() { });
            }
            return collection;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetWeek(DateTime? time = null)
        {
            string week = string.Empty;
            switch ((time ?? DateTime.Now).DayOfWeek)
            {
                case DayOfWeek.Sunday: week = "星期日"; break;
                case DayOfWeek.Monday: week = "星期一"; break;
                case DayOfWeek.Tuesday: week = "星期二"; break;
                case DayOfWeek.Wednesday: week = "星期三"; break;
                case DayOfWeek.Thursday: week = "星期四"; break;
                case DayOfWeek.Friday: week = "星期五"; break;
                case DayOfWeek.Saturday: week = "星期六"; break;
                default: week = "错误的星期"; break;
            }
            return week;
        }

        /// <summary>
        /// 获取友好的时间格式
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetFriendlyDateTime(DateTime date)
        {
            try
            {
                if (date == null) { return string.Empty; }
                var now = DateTime.Now;
                var cursor = now;
                if (date > cursor)
                {
                    var span = date - now;
                    if (span.TotalDays < 1)
                    {
                        return $"明天{date.Hour}点{date.Minute}分";
                    }
                    if (span.TotalDays < 2)
                    {
                        return $"后天{date.Hour}点{date.Minute}分";
                    }
                    return date.ToString();
                }
                else
                {
                    //计算时间差
                    var span = cursor - date;
                    //如果是十分钟之内就显示几分钟之前
                    if (span.TotalMinutes >= 0 && span.TotalMinutes < 10)
                    {
                        if (((int)span.TotalMinutes) == 0) { return "刚刚"; }
                        return $"{(int)Math.Floor(span.TotalMinutes)}分钟前";
                    }
                    //今天
                    cursor = now.Date;
                    if (date >= cursor && date < cursor.AddDays(1))
                    {
                        return $"今天 {date.Hour}:{date.Minute}";
                    }
                    //昨天
                    cursor = now.Date.AddDays(-1);
                    if (date >= cursor && date < cursor.AddDays(1))
                    {
                        return $"昨天 {date.Hour}:{date.Minute}";
                    }
                    //前天
                    cursor = now.Date.AddDays(-2);
                    if (date >= cursor && date < cursor.AddDays(1))
                    {
                        return $"前天 {date.Hour}:{date.Minute}";
                    }
                    //不是最近三天 看是不是今年
                    if (date.Year != now.Year)
                    {
                        return $"{date.Year}年{date.Month}月{date.Day}日 {date.Hour}:{date.Minute}";
                    }
                    else
                    {
                        return $"今年{date.Month}月{date.Day}日 {date.Hour}:{date.Minute}";
                    }
                }
            }
            catch (Exception e)
            {
                e.AddLog(typeof(DateTimeHelper));
                return "时间转换错误";
            }
        }

        /// <summary>
        /// 获取友好的时间格式
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetSimpleFriendlyDateTime(DateTime date)
        {
            try
            {
                if (date == null) { return string.Empty; }
                var now = DateTime.Now;
                if (date > now)
                {
                    var span = date - now;
                    if (span.TotalDays < 1)
                    {
                        return $"明天{date.Hour}点{date.Minute}分";
                    }
                    if (span.TotalDays < 2)
                    {
                        return $"后天{date.Hour}点{date.Minute}分";
                    }
                    return date.ToString();
                }
                else
                {
                    //计算时间差
                    var span = now - date;

                    if (span.TotalMinutes < 1)
                    {
                        return "刚刚";
                    }
                    else if (span.TotalHours < 1)
                    {
                        return $"{(int)span.TotalMinutes}分钟前";
                    }
                    else if (span.TotalDays < 1)
                    {
                        return $"{(int)span.TotalHours}小时前";
                    }
                    else if (span.TotalDays < 31)
                    {
                        return $"{(int)span.TotalDays}天前";
                    }
                    else if (span.TotalDays < 365)
                    {
                        return $"{(int)(span.TotalDays / 31)}月前";
                    }
                    else
                    {
                        return $"{(int)(span.TotalDays / 365)}年前";
                    }
                }
            }
            catch (Exception e)
            {
                e.AddLog(typeof(DateTimeHelper));
                return "时间转换错误";
            }
        }
    }
}
