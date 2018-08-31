/*
 * 作用：获取客户端 IP 地址，以及 IP 与 Long 类型数据相互转换。
 * */
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Helper.Core.Library
{
    public class IPHelper
    {
        #region 私有属性常量
        private const string IPErrorException = "IP 地址错误！";
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 获取客户端 IP 地址
        /// </summary>
        /// <param name="httpRequest">HttpRequestBase</param>
        /// <returns></returns>
        public static string GetClientIP(HttpRequestBase httpRequest = null)
        {
            NameValueCollection variables = null;
            if (httpRequest != null)
            {
                variables = httpRequest.ServerVariables;
            }
            else
            {
                variables = HttpContext.Current.Request.ServerVariables;
            }
            string ip = "";
            if (variables["HTTP_VIA"] != null)
            {
                ip = variables["HTTP_X_FORWARDED_FOR"];
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = variables["REMOTE_ADDR"];
            }
            ip = GetIPInfo(Dns.GetHostAddresses(ip));
            if (IsIP(ip)) return ip;

            IPHostEntry entry = Dns.GetHostEntry(ip);
            if (entry != null)
            {
                ip = GetIPInfo(entry.AddressList);
                if (IsIP(ip)) return ip;
            }
            ip = GetIPInfo(Dns.GetHostAddresses(Dns.GetHostName()));
            if (IsIP(ip)) return ip;

            return ip == "::1" ? "127.0.0.1" : "";
        }
        /// <summary>
        /// IP 地址转 long 型数据
        /// </summary>
        /// <param name="ip">ip 地址，例：127.0.0.0</param>
        /// <returns></returns>
        public static long IPToLong(string ip)
        {
            string[] dataList = ip.Split(new char[] { '.' });
            if (dataList == null || dataList.Length != 4) throw new Exception(IPErrorException);
            return long.Parse(dataList[0]) << 24 | long.Parse(dataList[1]) << 16 | long.Parse(dataList[2]) << 8 | long.Parse(dataList[3]);
        }
        /// <summary>
        /// long 型数据转 IP 地址
        /// </summary>
        /// <param name="ip">ip 地址，例：127.0.0.0</param>
        /// <returns></returns>
        public static string LongToIP(long ip)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append((ip >> 24) & 0xFF).Append(".");
            stringBuilder.Append((ip >> 16) & 0xFF).Append(".");
            stringBuilder.Append((ip >> 8) & 0xFF).Append(".");
            stringBuilder.Append(ip & 0xFF);
            return stringBuilder.ToString();
        }
        #endregion

        #region 逻辑处理私有函数
        private static string GetIPInfo(IPAddress[] addressList)
        {
            if (addressList == null || addressList.Length == 0) return "";
            foreach (IPAddress address in addressList)
            {
                if (address.AddressFamily.ToString() == "InterNetwork") return address.ToString();
            }
            return "";
        }
        private static bool IsIP(string ip)
        {
            if (!string.IsNullOrEmpty(ip) && ip != "::1") return true;
            return false;
        }
        #endregion
    }
}
