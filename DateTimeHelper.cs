/*
 * 作用：日期操作，对 TimeSpan 进行格式化输出。
 * */
using System;

namespace Helper.Core.Library
{
    public class DateTimeHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 返回与当前时间差值
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <param name="dateFormat">输出格式化，d=天，h=时，hh=时（补零），m=分，mm=分（补零），s=秒，ss=秒（补零），f=毫秒，fff=毫秒（补零）</param>
        /// <returns></returns>
        public static string Subtract(DateTime dateTime, string dateFormat = "d hh:mm:ss fff")
        {
            return Subtract(dateTime, DateTime.Now, dateFormat);
        }
        /// <summary>
        /// 返回两个时间差值
        /// </summary>
        /// <param name="beginDateTime">开始日期</param>
        /// <param name="endDateTime">结束日期</param>
        /// <param name="dateFormat">输出格式化，d=天，h=时，hh=时（补零），m=分，mm=分（补零），s=秒，ss=秒（补零），f=毫秒，fff=毫秒（补零）</param>
        /// <returns></returns>
        public static string Subtract(DateTime beginDateTime, DateTime endDateTime, string dateFormat = "d hh:mm:ss fff")
        {
            return SubtractFormat(endDateTime.Subtract(beginDateTime));
        }
        #endregion

        #region 逻辑处理私有函数
        internal static string SubtractFormat(TimeSpan timeSpan, string dateFormat = "d hh:mm:ss fff")
        {
            dateFormat = string.Format(dateFormat.Replace("d", "{0}"), timeSpan.Days);
            dateFormat = string.Format(dateFormat.Replace("hh", "{0}"), timeSpan.Hours.ToString().PadLeft(2, '0'));
            dateFormat = string.Format(dateFormat.Replace("h", "{0}"), timeSpan.Hours);
            dateFormat = string.Format(dateFormat.Replace("mm", "{0}"), timeSpan.Minutes.ToString().PadLeft(2, '0'));
            dateFormat = string.Format(dateFormat.Replace("m", "{0}"), timeSpan.Minutes);
            dateFormat = string.Format(dateFormat.Replace("ss", "{0}"), timeSpan.Seconds.ToString().PadLeft(2, '0'));
            dateFormat = string.Format(dateFormat.Replace("s", "{0}"), timeSpan.Seconds);
            dateFormat = string.Format(dateFormat.Replace("fff", "{0}"), timeSpan.Milliseconds.ToString().PadLeft(3, '0'));
            dateFormat = string.Format(dateFormat.Replace("f", "{0}"), timeSpan.Milliseconds);
            return dateFormat;
        }
        #endregion
    }
}
