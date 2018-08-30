/*
 * 作用：通过 Quartz.Net 实现作业/任务调度。
 * Quartz 下载地址：https://www.quartz-scheduler.net/
 * */
using Quartz.Impl;
using Quartz;
using System.Collections.Generic;
using System;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    internal enum QuartzTypeEnum
    {
        Interval = 1,
        Task = 2
    }
    #endregion

    public class QuartzHelper
    {
        #region 私有属性常量
        private const string KeyExistsErrorException = "任务已经存在！";
        private static readonly object lockItem = new object();
        private static readonly Dictionary<string, QuartzKey> Dict = new Dictionary<string, QuartzKey>();
        private static IScheduler iScheduler = null;
        #endregion

        #region 对外公开方法

        /// <summary>
        /// 间隔执行任务
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="seconds">秒</param>
        /// <param name="jobKey">作业唯一标识</param>
        public static void Interval<T>(int seconds, string jobKey) where T : IJob
        {
            ExecuteScheduler<T>(jobKey, seconds.ToString(), QuartzTypeEnum.Interval, () =>
            {
                ITrigger iTrigger = TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(seconds).RepeatForever())
                .Build();

                return iTrigger;
            });
        }
        /// <summary>
        /// 定时执行任务
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cronExpression">参数顺序：0 0 9,16 * * ?，参数说明：秒|分|时(多个时间用逗号分隔)|每月中哪一天|月|每周中的哪一天|年</param>
        /// <param name="jobKey">作业唯一标识</param>
        public static void Task<T>(string cronExpression, string jobKey) where T : IJob
        {
            ExecuteScheduler<T>(jobKey, cronExpression, QuartzTypeEnum.Task, () =>
            {
                ICronTrigger iTrigger = (ICronTrigger)TriggerBuilder.Create()
                .WithCronSchedule(cronExpression)
                .Build();
                return iTrigger;
            });
        }
        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="jobKey">作业唯一标识</param>
        /// <returns></returns>
        public static bool Delete(string jobKey)
        {
            if (iScheduler == null || !Dict.ContainsKey(jobKey)) return true;
            bool result = iScheduler.DeleteJob(Dict[jobKey].JobKey);
            if (result) Dict.Remove(jobKey);
            return result;
        }
        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="jobKey">作业唯一标识</param>
        public static void Pause(string jobKey)
        {
            if (iScheduler == null || !Dict.ContainsKey(jobKey)) return;
            iScheduler.PauseJob(Dict[jobKey].JobKey);
        }
        /// <summary>
        /// 恢复任务
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="jobKey">作业唯一标识</param>
        /// <param name="isReset">为 false 时，错过的触发，将会重新执行，为 true 时，会忽略错过的触发</param>
        public static void Resume<T>(string jobKey, bool isReset = false) where T : IJob
        {
            if (iScheduler == null || !Dict.ContainsKey(jobKey)) return;
            if (!isReset)
            {
                iScheduler.ResumeJob(Dict[jobKey].JobKey);
            }
            else
            {
                QuartzKey quartKey = Dict[jobKey];
                Delete(jobKey);
                if (quartKey.TypeEnum == QuartzTypeEnum.Interval)
                {
                    Interval<T>(int.Parse(quartKey.Data), jobKey);
                }
                else
                {
                    Task<T>(quartKey.Data, jobKey);
                }
            }
        }

        #endregion

        #region 逻辑处理私有函数
        private static IScheduler InitScheduler()
        {
            lock (lockItem)
            {
                if (iScheduler == null)
                {
                    ISchedulerFactory iSchedulerFactory = new StdSchedulerFactory();
                    iScheduler = iSchedulerFactory.GetScheduler();
                }
                return iScheduler;
            }
        }
        private static void ExecuteScheduler<T>(string jobKey, string data, QuartzTypeEnum typeEnum, Func<ITrigger> callback) where T : IJob
        {
            if (Dict.ContainsKey(jobKey)) throw new Exception(KeyExistsErrorException);
            IScheduler iScheduler = InitScheduler();

            IJobDetail iJobDetail = JobBuilder.Create<T>().Build();
            ITrigger iTrigger = callback();

            iScheduler.ScheduleJob(iJobDetail, iTrigger);
            if (!iScheduler.IsStarted)
            {
                iScheduler.Start();
            }
            if (!Dict.ContainsKey(jobKey)) Dict.Add(jobKey, new QuartzKey() { Data = data, TypeEnum = typeEnum, JobKey = iJobDetail.Key, TriggerKey = iTrigger.Key, Trigger = iTrigger });
        }
        #endregion
    }

    #region 逻辑处理辅助类
    internal class QuartzKey
    {
        public string Data { get; set; }
        public QuartzTypeEnum TypeEnum { get; set; }
        public JobKey JobKey { get; set; }
        public TriggerKey TriggerKey { get; set; }
        public ITrigger Trigger { get; set; }
    }
    #endregion
}
