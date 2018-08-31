/*
 * /*
 * 作用：通过 RabbitMQ.Net 实现消息队列。
 * ErLang 下载地址：http://www.erlang.org/downloads
 * RabbitMQ 下载地址：http://www.rabbitmq.com/
 * RabbitMQ Client 下载地址：http://www.rabbitmq.com/dotnet.html
 * */
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    public class RabbitMQExchangeTypeEnum
    {
        public const string Direct = "direct";
        public const string Fanout = "fanout";
        public const string Topic = "topic";
        public const string Headers = "headers";
    }
    #endregion

    public class RabbitMQHelper
    {
        #region 私有属性常量

        /// <summary>
        /// 非持久
        /// </summary>
        public const int NON_PERSISTENT = 1;
        /// <summary>
        /// 持久
        /// </summary>
        public const int PERSISTENT = 2;

        private ConnectionFactory connectionFactory;
        #endregion

        #region 构造函数
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostName">主机，默认：localhost</param>
        /// <param name="userName">用户名，默认：guest</param>
        /// <param name="password">密码，默认：guest</param>
        public RabbitMQHelper(string hostName, string userName = null, string password = null)
        {
            this.connectionFactory = new ConnectionFactory();
            this.connectionFactory.HostName = hostName;
            if (!string.IsNullOrEmpty(userName)) this.connectionFactory.UserName = userName;
            if (!string.IsNullOrEmpty(password)) this.connectionFactory.Password = password;
        }
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 发送
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="exchange">交换机名</param>
        /// <param name="queue">队列名</param>
        /// <param name="data">实体类型数据</param>
        /// <param name="type">交换机类型</param>
        /// <param name="deliveryMode">传递模式</param>
        public void Send<T>(string exchange, string queue, T data, string type = RabbitMQExchangeTypeEnum.Direct, int deliveryMode = PERSISTENT) where T : class
        {
            IConnection connection = null;
            IModel channel = null;
            try
            {
                connection = this.connectionFactory.CreateConnection();
                channel = connection.CreateModel();

                channel.ExchangeDeclare(exchange, type, false, false, null);
                channel.QueueDeclare(queue, true, false, false, null);
                channel.QueueBind(queue, exchange, queue);

                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = (byte)deliveryMode; //表示持久化消息

                //推送消息
                channel.BasicPublish(exchange, queue, properties, XmlSerializerHelper.Serialize<T>(data));
            }
            catch
            {
                throw;
            }
            finally
            {
                if (channel != null) channel.Dispose();
                if (connection != null) connection.Dispose();
            }
        }
        /// <summary>
        /// 侦听
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="queue">队列名</param>
        /// <param name="callback">回调函数</param>
        /// <param name="noAck">是否自动消费</param>
        /// <param name="prefetchCount">消费数量</param>
        /// <param name="ackCallback">转换 T 异常或者 noAck 为 false 时，要手动处理掉消息</param>
        public void Listen<T>(string queue, Action<T> callback, bool noAck = true, int prefetchCount = 1, Action<IModel, BasicDeliverEventArgs> ackCallback = null) where T : class
        {
            IConnection connection = null;
            IModel channel = null;
            try
            {
                connection = this.connectionFactory.CreateConnection();
                channel = connection.CreateModel();

                channel.QueueDeclare(queue, true, false, false, null);
                channel.BasicQos(0, (ushort)prefetchCount, false);

                EventingBasicConsumer eventingBasicConsumer = new EventingBasicConsumer(channel);
                eventingBasicConsumer.Received += (object sender, BasicDeliverEventArgs args) =>
                {
                    try
                    {
                        T t = XmlSerializerHelper.Deserialize<T>(args.Body);
                        if (t != null && (callback != null)) callback(t);

                        // 如果 noAck 为 False，需要手动处理掉消息
                        if (!noAck && ackCallback != null) ackCallback(channel, args);
                    }
                    catch
                    {
                        if (ackCallback != null)
                        {
                            ackCallback(channel, args);
                        }
                        else
                        {
                            if (channel.IsClosed) channel.BasicAck(args.DeliveryTag, false);
                        }
                        throw;
                    }
                };
                channel.BasicConsume(queue, noAck, eventingBasicConsumer);
            }
            catch
            {
                if (channel != null) channel.Dispose();
                if (connection != null) connection.Dispose();

                throw;
            }
        }
        #endregion
    }
}
