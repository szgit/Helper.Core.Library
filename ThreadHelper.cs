/*
 * 作用：多线程处理，在线程中可以实现同步/异步操作。
 * */
using System;
using System.Collections.Generic;
using System.Threading;

namespace Helper.Core.Library
{
    public class ThreadHelper
    {
        #region 私有属性常量
        // 静态锁对象
        private static readonly object lockItem = new object();
        // 终止状态时 WaitOne() 允许线程访问下边的语句
        private ManualResetEvent manualEvent = new ManualResetEvent(true);

        // 已执行结束线程数量
        private int callbackCount;
        // 线程对象列表
        private List<IThreadItem> threadItemList;
        // 每个 ThreadItem 执行完回调
        private Action<IThreadItem> itemCallback;
        // 线程执行结束回调函数
        private Action<int> callback;
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 启动线程
        /// </summary>
        /// <param name="threadItemList">线程对象列表</param>
        /// <param name="itemCallback">IThreadItem 执行完回调函数</param>
        /// <param name="callback">线程结束回调函数</param>
        /// <param name="initThreadCount">初始执行线程数量</param>
        public void Run(List<IThreadItem> threadItemList, Action<IThreadItem> itemCallback, Action<int> callback, int initThreadCount = 10)
        {
            this.callbackCount = 0;
            this.threadItemList = threadItemList;
            this.itemCallback = itemCallback;
            this.callback = callback;

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(initThreadCount, initThreadCount);

            foreach(IThreadItem threadItem in threadItemList)
            {
                ThreadItemHelper threadItemHelper = new ThreadItemHelper(threadItem, this.ThreadCallback);
                System.Threading.WaitCallback waitCallback = new WaitCallback(threadItemHelper.Callback);
                ThreadPool.QueueUserWorkItem(waitCallback);
            }

            this.manualEvent.WaitOne();
            this.manualEvent.Reset();
        }
        /// <summary>
        /// 线程等待
        /// </summary>
        /// <param name="callback">线程等待之后要处理的逻辑</param>
        public void Wait(Action callback = null)
        {
            if (callback != null) callback();
            this.manualEvent.WaitOne();
            this.manualEvent.Reset();
        }
        /// <summary>
        /// 线程继续
        /// </summary>
        public void Set()
        {
            this.manualEvent.Set();
        }
        #endregion

        #region 逻辑处理私有函数
        private void ThreadCallback(IThreadItem threadItem)
        {
            lock (lockItem)
            {
                if (this.itemCallback != null) this.itemCallback(threadItem);
                this.callbackCount++;
                if (this.callbackCount >= this.threadItemList.Count && this.callback != null)
                {
                    this.callback(this.callbackCount);
                    this.callback = null;
                    this.manualEvent.Set();
                }
            }
        }
        #endregion
    }

    #region 逻辑处理辅助类
    /// <summary>
    /// 线程帮助类，用来执行线程逻辑
    /// </summary>
    internal class ThreadItemHelper
    {
        private IThreadItem threadItem;
        private Action<IThreadItem> callback;

        public ThreadItemHelper(IThreadItem threadItem, Action<IThreadItem> callback)
        {
            this.threadItem = threadItem;
            this.callback = callback;
        }

        public void Callback(object obj)
        {
            if (this.threadItem.IsAsync)
            {
                this.threadItem.ThreadProcess(() =>
                {
                    if (this.callback != null) this.callback(this.threadItem);
                });
            }
            else
            {
                this.threadItem.ThreadProcess();
                if (this.callback != null) this.callback(this.threadItem);
            }
        }
    }
    #endregion

    #region 逻辑处理接口
    public interface IThreadItem
    {
        /// <summary>
        /// 线程处理逻辑，线程之间异步
        /// </summary>
        /// <returns></returns>
        void ThreadProcess(Action callback);

        /// <summary>
        /// 线程处理逻辑，线程之间同步
        /// </summary>
        void ThreadProcess();

        /// <summary>
        /// 是否异步
        /// </summary>
        bool IsAsync { get; }
    }
    #endregion
}