/*
 * 作用：多线程处理，在线程中可以实现同步/异步操作。
 * */
using System;
using System.Collections.Generic;
using System.Threading;

namespace Helper.Core.Library
{
    public class ThreadDataHelper<T> where T:class
    {
        #region 私有属性常量
        // 静态锁对象
        private static readonly object lockItem = new object();
        // 终止状态时 WaitOne() 允许线程访问下边的语句
        private ManualResetEvent manualEvent = new ManualResetEvent(true);

        // 已执行结束线程数量
        private int callbackCount;
        // 多线程需要处理的数据
        private List<T> dataList;
        // 数据处理逻辑类
        private IThreadDataItem<T> iThreadDataItem;
        // 每个 IThreadDataItem 执行完回调
        private Action<T> itemCallback;
        // 线程执行结束回调函数
        private Action<int> callback;
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 启动线程
        /// </summary>
        /// <param name="dataList">数据列表</param>
        /// <param name="iThreadDataItem">数据处理逻辑类</param>
        /// <param name="itemCallback">IThreadDataItem 执行完回调函数</param>
        /// <param name="callback">线程结束回调函数</param>
        /// <param name="initThreadCount">初始执行线程数量</param>
        public void Run(List<T> dataList, IThreadDataItem<T> iThreadDataItem, Action<T> itemCallback, Action<int> callback, int initThreadCount = 10)
        {
            this.callbackCount = 0;
            this.dataList = dataList;
            this.iThreadDataItem = iThreadDataItem;
            this.itemCallback = itemCallback;
            this.callback = callback;

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(initThreadCount, initThreadCount);

            foreach (T t in dataList)
            {
                // 初始化线程帮助类，传递相关数据来执行线程逻辑
                ThreadItemDataHelper<T> threadItemDataHelper = new ThreadItemDataHelper<T>(t, this.iThreadDataItem.NewItem(), this.ThreadCallback);
                // 使用线程池
                System.Threading.WaitCallback waitCallback = new System.Threading.WaitCallback(threadItemDataHelper.Callback);
                System.Threading.ThreadPool.QueueUserWorkItem(waitCallback);
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
        /// <summary>
        /// 线程回调函数，每个线程执行结束都会调用此函数
        /// </summary>
        private void ThreadCallback(T t)
        {
            lock (lockItem)
            {
                if (this.itemCallback != null) this.itemCallback(t);
                this.callbackCount++;

                if (this.callbackCount >= this.dataList.Count && this.callback != null)
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
    internal class ThreadItemDataHelper<T> where T:class
    {
        private IThreadDataItem<T> threadItem;
        private Action<T> callback;
        private T t;

        public ThreadItemDataHelper(T t, IThreadDataItem<T> threadItem, Action<T> callback)
        {
            this.t = t;
            this.threadItem = threadItem;
            this.callback = callback;
        }

        public void Callback(object obj)
        {
            if (this.threadItem.IsAsync)
            {
                this.threadItem.ThreadProcess(t, () =>
                {
                    if (this.callback != null) this.callback(this.t);
                });
            }
            else
            {
                this.threadItem.ThreadProcess(t);
                if (this.callback != null) this.callback(this.t);
            }
        }
    }
    #endregion

    #region 逻辑处理接口
    public interface IThreadDataItem<T> where T : class
    {
        /// <summary>
        /// 线程处理逻辑，线程之间同步
        /// </summary>
        /// <returns></returns>
        void ThreadProcess(T t);

        /// <summary>
        /// 线程处理逻辑，线程之间异步
        /// </summary>
        /// <param name="t"></param>
        /// <param name="callback"></param>
        void ThreadProcess(T t, Action callback);

        /// <summary>
        /// 是否异步
        /// </summary>
        bool IsAsync { get; }

        /// <summary>
        /// 对象克隆
        /// </summary>
        /// <returns></returns>
        IThreadDataItem<T> NewItem();
    }
    #endregion
}
