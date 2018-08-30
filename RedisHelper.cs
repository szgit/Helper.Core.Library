/*
 * 作用：通过 Redis.Net 实现数据缓存。
 * Redis 下载地址：http://redis.io/
 * */
using ServiceStack.Redis;
using ServiceStack.Text;
using System;

namespace Helper.Core.Library
{
    public class RedisHelper
    {
        #region 对外公开方法
        /// <summary>
        /// Redis 初始化
        /// </summary>
        /// <param name="readWriteServerConfig">读写 Server 列表，多个 Server 用英文逗号分隔，例：127.0.0.1:6379</param>
        /// <param name="readServerConfig">只读 Server 列表，多个 Server 用英文逗号分隔，例：127.0.0.1:6379</param>
        /// <param name="maxWritePoolSize">最大写池大小</param>
        /// <param name="maxReadPoolSize">最大读池大小</param>
        /// <param name="autoStart">是否自动开始</param>
        public static void Init(string readWriteServerConfig, string readServerConfig, int maxWritePoolSize = 50, int maxReadPoolSize = 50, bool autoStart = true)
        {
            RedisManager.Init(readWriteServerConfig, readWriteServerConfig, maxWritePoolSize, maxReadPoolSize, autoStart);
        }
        /// <summary>
        /// 检查是否存在相应的 KEY
        /// </summary>
        /// <param name="redisKey">Key</param>
        /// <param name="hashID">Hash ID</param>
        /// <returns></returns>
        public static bool Exists(string redisKey, string hashID)
        {
            bool result = false;
            using (IRedisClient client = RedisManager.GetClient())
            {
                result = client.HashContainsEntry(hashID, redisKey);
            }
            return result;
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="redisKey">Key</param>
        /// <param name="hashID">Hash ID</param>
        /// <returns></returns>
        public static T Get<T>(string redisKey, string hashID = null) where T : class
        {
            T result = null;
            using(IRedisClient client = RedisManager.GetClient())
            {
                if (!string.IsNullOrEmpty(hashID))
                {
                    string value = client.GetValueFromHash(hashID, redisKey);
                    result = JsonSerializer.DeserializeFromString<T>(value);
                }
                else
                {
                    result = client.Get<T>(redisKey);
                }
            }
            return result;
        }
        /// <summary>
        /// 设置缓存数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="redisKey">Key</param>
        /// <param name="value">实体类型数据</param>
        /// <param name="expireSeconds">过期秒数</param>
        /// <param name="hashID">Hash ID</param>
        /// <returns></returns>
        public static bool Set<T>(string redisKey, T value, int expireSeconds = 0, string hashID = null) where T : class
        {
            bool result = false;
            using(IRedisClient client = RedisManager.GetClient())
            {
                if (!string.IsNullOrEmpty(hashID))
                {
                    var serializerValue = JsonSerializer.SerializeToString<T>(value);
                    result = client.SetEntryInHash(hashID, redisKey, serializerValue);
                }
                else
                {
                    result = client.Set<T>(redisKey, value);
                }
            }
            if (result && expireSeconds > 0)
            {
                Expire(redisKey, expireSeconds);
            }
            return result;
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="redisKey">Key</param>
        /// <param name="hashID">Hash ID</param>
        /// <returns></returns>
        public static bool Remove(string redisKey, string hashID = null)
        {
            bool result = false;
            using(IRedisClient client = RedisManager.GetClient())
            {
                if (!string.IsNullOrEmpty(hashID))
                {
                    result = client.RemoveEntryFromHash(hashID, redisKey);
                }
                else
                {
                    result = client.Remove(redisKey);
                }
            }
            return result;
        }
        /// <summary>
        /// 设置过期时间
        /// </summary>
        /// <param name="redisKey">Key</param>
        /// <param name="seconds">秒</param>
        public static bool Expire(string redisKey, int expireSeconds)
        {
            return Expire(redisKey, DateTime.Now.AddSeconds(expireSeconds));
        }
        /// <summary>
        /// 设置过期时间
        /// </summary>
        /// <param name="redisKey">Key</param>
        /// <param name="dateTime">过期时间</param>
        /// <returns></returns>
        public static bool Expire(string redisKey, DateTime expireDateTime)
        {
            bool result = false;
            using(IRedisClient client = RedisManager.GetClient())
            {
                result = client.ExpireEntryAt(redisKey, expireDateTime);
            }
            return result;
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="async">是否异步</param>
        public static void Save(bool async = false)
        {
            using (IRedisClient client = RedisManager.GetClient())
            {
                if (async)
                {
                    client.SaveAsync();
                }
                else
                {
                    client.Save();
                }
            }
        }
        #endregion
    }

    #region 逻辑处理辅助类
    internal class RedisManager
    {
        private const string PooledRedisClientManagerNullException = "PooledRedisClientManager 未初始化";
        private static PooledRedisClientManager manager;

        public static void Init(string readWriteServerConfig, string readServerConfig, int maxWritePoolSize = 50, int maxReadPoolSize = 50, bool autoStart = true)
        {
            string[] readWriteServerList = readWriteServerConfig.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string[] readServerList = readServerConfig.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            manager = new PooledRedisClientManager(readWriteServerList, readServerList, new RedisClientManagerConfig
            {
                MaxWritePoolSize = maxWritePoolSize,
                MaxReadPoolSize = maxReadPoolSize,
                AutoStart = autoStart
            });
        }

        public static IRedisClient GetClient()
        {
            if (manager == null) throw new Exception(PooledRedisClientManagerNullException);
            return manager.GetClient();
        }
    }
    #endregion
}
