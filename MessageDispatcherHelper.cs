/*
 * 作用：消息侦听/广播。
 * */
using System;
using System.Collections.Generic;

namespace Helper.Core.Library
{
    public class MessageDispatcherHelper
    {
        #region 私有属性常量
        internal Dictionary<string, MessageListener> eventListenerDict;

        public MessageDispatcherHelper()
        {
            this.eventListenerDict = new Dictionary<string, MessageListener>();
        }
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 添加消息侦听
        /// </summary>
        /// <param name="messageType">消息类别</param>
        /// <param name="callback">消息处理回调函数</param>
        public void addEventListener(string messageType, Action<MessageEvent> callback)
        {
            if (!this.eventListenerDict.ContainsKey(messageType))
            {
                this.eventListenerDict.Add(messageType, new MessageListener(messageType));
            }

            this.eventListenerDict[messageType].OnEvent += callback;
        }
        /// <summary>
        /// 移除消息侦听
        /// </summary>
        /// <param name="messageType">消息类别</param>
        /// <param name="callback">消息处理回调函数</param>
        public void removeEventListener(string messageType, Action<MessageEvent> callback)
        {
            if (this.eventListenerDict.ContainsKey(messageType))
            {
                this.eventListenerDict[messageType].OnEvent -= callback;
                // 如果没有消息侦听，则移除
                if(this.eventListenerDict[messageType].ActionCount == 0)
                {
                    this.eventListenerDict.Remove(messageType);
                }
            }
        }
        /// <summary>
        /// 判断是否存在相应的消息类别
        /// </summary>
        /// <param name="eventType">消息类别</param>
        /// <returns></returns>
        public bool hasListener(string messageType)
        {
            return this.eventListenerDict.ContainsKey(messageType);
        }
        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="messageEvent">MessageEvent 对象</param>
        /// <param name="targetObject">消息广播对象</param>
        public void dispatchEvent(MessageEvent messageEvent, object targetObject)
        {
            if (this.eventListenerDict.ContainsKey(messageEvent.MessageType))
            {
                MessageListener eventListener = this.eventListenerDict[messageEvent.MessageType];
                if (eventListener != null)
                {
                    messageEvent.Target = targetObject;
                    eventListener.Excute(messageEvent);
                }
            }
        }
        #endregion
    }

    #region 逻辑处理辅助类
    /// <summary>
    /// 消息侦听
    /// </summary>
    internal class MessageListener
    {
        /// <summary>
        /// 消息类别
        /// </summary>
        public string MessageType { get; set; }

        public MessageListener(string messageType)
        {
            this.MessageType = messageType;
        }

        /// <summary>
        /// 消息事件
        /// </summary>
        public event Action<MessageEvent> OnEvent;

        /// <summary>
        /// 获取委托数量
        /// </summary>
        public int ActionCount
        {
            get
            {
                if (OnEvent == null) return 0;
                return OnEvent.GetInvocationList().Length;
            }
        }

        /// <summary>
        /// 触发消息
        /// </summary>
        /// <param name="messageEvent"></param>
        public void Excute(MessageEvent messageEvent)
        {
            if (OnEvent != null) this.OnEvent(messageEvent);
        }
    }

    /// <summary>
    /// 消息事件
    /// </summary>
    public class MessageEvent
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// 消息传递数据
        /// </summary>
        public object MessageData { get; set; }

        /// <summary>
        /// 消息触发者
        /// </summary>
        public object Target { get; set; }

        public MessageEvent(string messageType, object messageData = null)
        {
            this.MessageType = messageType;
            this.MessageData = messageData;
        }
    }
    #endregion
}