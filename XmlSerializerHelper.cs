using System.IO;
using System.Xml.Serialization;

namespace Helper.Core.Library
{
    public class XmlSerializerHelper
    {
        /// <summary>
        /// 实体类型数据序列化成字节数组
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="t">实体类型数据</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T t) where T : class
        {
            MemoryStream memoryStream = null;
            try
            {
                byte[] bytes = null;
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (memoryStream = new MemoryStream())
                {
                    xmlSerializer.Serialize(memoryStream, t);
                    bytes = memoryStream.ToArray();
                }
                return bytes;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (memoryStream != null) memoryStream.Dispose();
            }
        }
        /// <summary>
        /// 字节数组反序列化成实体类型数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] bytes) where T : class
        {
            MemoryStream memoryStream = null;
            try
            {
                T t = null;
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (memoryStream = new MemoryStream(bytes))
                {
                    t = xmlSerializer.Deserialize(memoryStream) as T;
                }
                return t;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (memoryStream != null) memoryStream.Dispose();
            }
        }
    }
}
