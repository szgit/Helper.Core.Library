/*
 * 作用：消息侦听/广播。
 * */
using System;

namespace Helper.Core.Library
{
    public class MessageHelper
    {
        #region 私有属性常量
        /// <summary>
        /// 初始化消息触发对象
        /// </summary>
        private static readonly MessageDispatcherHelper dispatcher = new MessageDispatcherHelper();
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 添加消息侦听
        /// </summary>
        /// <param name="messageType">消息类别</param>
        /// <param name="callback">消息处理回调函数</param>
        public static void addEventListener(string messageType, Action<MessageEvent> callback)
        {
            dispatcher.addEventListener(messageType, callback);
        }
        /// <summary>
        /// 移除消息侦听
        /// </summary>
        /// <param name="messageType">消息类别</param>
        /// <param name="callback">消息处理回调函数</param>
        public static void removeEventListener(string messageType, Action<MessageEvent> callback)
        {
            dispatcher.removeEventListener(messageType, callback);
        }
        /// <summary>
        /// 判断是否存在相应的消息类别
        /// </summary>
        /// <param name="messageType">消息类别</param>
        /// <returns></returns>
        public static bool hasListener(string messageType)
        {
            return dispatcher.hasListener(messageType);
        }
        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="messageEvent">MessageEvent 对象</param>
        /// <param name="targetObject">消息广播对象</param>
        public static void dispatchEvent(MessageEvent messageEvent, object targetObject = null)
        {
            dispatcher.dispatchEvent(messageEvent, targetObject);
        }
        #endregion
    }
}
