using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Lib.extension
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// 计算当月的总天数
        /// </summary>
        public static int DaysOfThisMonth(this DateTime time)
        {
            var border = time.GetMonthBorder();
            var days = (border.end - border.start).TotalDays;
            Debug.Assert((int)days == days, "每月的天数应该是整数");
            return (int)Math.Ceiling(days);
        }

        /// <summary>
        /// 明天凌晨
        /// </summary>
        public static DateTime ToTomorrowMorning(this DateTime time)
        {
            return time.Date.AddDays(1);
        }

        /// <summary>
        /// 今天凌晨
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime ToToDayMorning(this DateTime time)
        {
            return time.Date;
        }

        /// <summary>
        /// 获取年龄
        /// </summary>
        /// <param name="birthday"></param>
        /// <returns></returns>
        public static int GetAge(this DateTime birthday)
        {
            var today = DateTime.Now;

            int age = today.Year - birthday.Year;

            if (birthday > today.AddYears(-age))
            {
                --age;
            }

            return age;
        }

        /// <summary>
        /// 获取格式化字符串，带时分秒，格式："yyyy-MM-dd HH:mm:ss"
        /// </summary>
        public static string ToDateTimeString(this DateTime dateTime, bool isRemoveSecond = false)
        {
            if (isRemoveSecond)
                return dateTime.ToString("yyyy-MM-dd HH:mm");
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy-MM-dd"
        /// </summary>
        public static string ToDateString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 获取格式化字符串，不带年月日，格式："HH:mm:ss"
        /// </summary>
        public static string ToTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("HH:mm:ss");
        }

        /// <summary>
        /// 获取格式化字符串，带毫秒，格式："yyyy-MM-dd HH:mm:ss.fff"
        /// </summary>
        public static string ToMillisecondString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy年MM月dd日"
        /// </summary>
        public static string ToChineseDateString(this DateTime dateTime)
        {
            return string.Format("{0}年{1}月{2}日", dateTime.Year, dateTime.Month, dateTime.Day);
        }

        /// <summary>
        /// 转为n秒前，n分钟前 etc
        /// </summary>
        public static string ToFriendlyDateTime(this DateTime dateTime)
        {
            return DateTimeHelper.GetFriendlyDateTime(dateTime);
        }

        /// <summary>
        /// 转为n秒前，n分钟前 etc
        /// </summary>
        public static string GetSimpleFriendlyDateTime(this DateTime dateTime)
        {
            return DateTimeHelper.GetSimpleFriendlyDateTime(dateTime);
        }

        /// <summary>
        /// 获取每年开始结束
        /// </summary>
        public static (DateTime start, DateTime end) GetYearBorder(this DateTime time)
        {
            var start = new DateTime(time.Year, 1, 1).Date;

            return (start, start.AddYears(1));
        }

        /// <summary>
        /// 获取每月开始结束
        /// </summary>
        public static (DateTime start, DateTime end) GetMonthBorder(this DateTime time)
        {
            var start = new DateTime(time.Year, time.Month, 1).Date;

            return (start, start.AddMonths(1));
        }

        /// <summary>
        /// 获取当前周的开始结束
        /// </summary>
        public static (DateTime start, DateTime end) GetWeekBorder(this DateTime time,
            DayOfWeek weekStart = DayOfWeek.Monday)
        {
            var date = time.Date;
            while (true)
            {
                if (date.DayOfWeek == weekStart)
                {
                    break;
                }
                date = date.AddDays(-1);
            }
            return (date, date.AddDays(7));
        }

        /// <summary>
        /// 获取一天的开始和第二天的开始
        /// </summary>
        public static (DateTime start, DateTime end) GetDateBorder(this DateTime dateTime)
        {
            var date = dateTime.Date;
            return (date, date.AddDays(1));
        }

        /// <summary>
        /// 判断是否是同一天
        /// </summary>
        public static bool IsSameDay(this DateTime dateTime, DateTime time)
        {
            var border = dateTime.GetDateBorder();
            return border.start <= time && time < border.end;
        }

        /// <summary>
        /// 是今天
        /// </summary>
        public static bool IsToday(this DateTime time)
        {
            return time.IsSameDay(DateTime.Now);
        }

        /// <summary>
        /// 是昨天
        /// </summary>
        public static bool IsYesterday(this DateTime time)
        {
            return time.IsSameDay(DateTime.Now.AddDays(-1));
        }

    }
}
