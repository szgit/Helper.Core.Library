/*
 * 作用：通过 Stopwatch 检测程序性能。
 * */
using System;
using System.Diagnostics;

namespace Helper.Core.Library
{
    public class MonitorHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 耗时
        /// </summary>
        /// <param name="callback">耗时逻辑</param>
        /// <param name="repeat">重复次数</param>
        /// <returns></returns>
        public static TimeSpan Consume(Action callback, int repeat = 0)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (repeat > 0)
            {
                for(int index = 0; index < repeat; index ++)
                {
                    if (callback != null) callback();
                }
            }
            else
            {
                if (callback != null) callback();
            }
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
        /// <summary>
        /// 耗时
        /// </summary>
        /// <param name="callback">耗时逻辑</param>
        /// <param name="dateFormat">输出格式化，d=天，h=时，hh=时（补零），m=分，mm=分（补零），s=秒，ss=秒（补零），f=毫秒，fff=毫秒（补零）</param>
        /// <param name="repeat">重复次数</param>
        /// <returns></returns>
        public static string ConsumeFormat(Action callback, string dateFormat = "d hh:mm:ss fff", int repeat = 0)
        {
            TimeSpan timeSpan = Consume(callback, repeat);
            return DateTimeHelper.SubtractFormat(timeSpan, dateFormat);
        }
        #endregion
    }
}
