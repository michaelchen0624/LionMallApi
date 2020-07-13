using System;
using System.Collections.Generic;
using System.Text;

namespace Prod.Core
{
    public static class DateHelper
    {
        /// <summary>
        /// 获取unix时间戳,以本机时间为依据
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetTimeUnixLocal(this DateTime time)
        {
            TimeSpan cha = (time - TimeZoneInfo.ConvertTime(new DateTime(1970,1,1),TimeZoneInfo.Local));
            long t = (long)cha.TotalSeconds;
            return t;
        }
        /// <summary>
        /// 获取DateTime以本机时间为依据
        /// </summary>
        /// <param name="unix"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeLocal(this DateTime time, long unix)
        {
            DateTime dtStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            return  dtStart.AddSeconds(unix);
        }
        /// <summary>
        /// 获取当月第一unix时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetFirstOfMonth(this DateTime time)
        {
            var date = new DateTime(time.Year, time.Month, 1);
            return date.GetTimeUnixLocal();
        }
        /// <summary>
        /// 获取当月最后unix时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetLastOfMonth(this DateTime time)
        {
            var date = new DateTime(time.Year, time.Month, 1);
            var last = date.AddMonths(1);
            return last.GetTimeUnixLocal()-1;
        }
        /// <summary>
        /// 获取当前0.00 unix时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetFirstOfCurrent(this DateTime time)
        {
            var date = new DateTime(time.Year, time.Month, time.Day);
            return date.GetTimeUnixLocal();
        }
        /// <summary>
        /// 获取当前23.59unix时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetLastOfCurrent(this DateTime time)
        {
            var date = new DateTime(time.Year, time.Month, time.Day);
            var last = date.AddDays(1).AddSeconds(-1);
            return last.GetTimeUnixLocal();
        }
    }
}
