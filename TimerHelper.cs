/*
 * 作用：计时器。
 * */
using System;
using System.Timers;

namespace Helper.Core.Library
{
    public class TimerHelper
    {
        #region 私有属性常量
        private Timer timer;
        private Action callback;
        private int repeat;
        private bool pauseStatus;
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 任务运行，间隔时间
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <param name="interval">执行间隔</param>
        /// <param name="repeat">重复次数，小于 0 表示无限次</param>
        /// <param name="executeCallback">是否立刻执行</param>
        /// <returns></returns>
        public void Run(Action callback, int interval = 1000, int repeat = 1, bool executeCallback = false)
        {
            this.callback = callback;
            this.repeat = repeat;
            this.pauseStatus = false;

            if (this.timer != null) this.Stop();

            this.timer = new Timer();
            this.timer.Interval = interval;
            this.timer.Elapsed += OnTimerElapsedHandler;
            this.timer.Start();

            if (executeCallback && callback != null) callback();
        }
        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            this.pauseStatus = true;
        }
        /// <summary>
        /// 继续
        /// </summary>
        public void Continue()
        {
            this.pauseStatus = false;
        }
        /// <summary>
        /// 停止任务
        /// </summary>
        public void Stop()
        {
            if (this.timer != null)
            {
                this.timer.Elapsed -= OnTimerElapsedHandler;
                this.timer.Stop();
            }
            this.timer = null;
        }
        #endregion

        #region 逻辑处理私有函数
        private void OnTimerElapsedHandler(object sender, ElapsedEventArgs e)
        {
            if (this.pauseStatus) return;

            if (this.repeat > 0) this.repeat--;
            if (this.repeat == 0) this.Stop();

            if (this.callback != null) this.callback();
        }
        #endregion
    }
}
