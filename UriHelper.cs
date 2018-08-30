/*
 * 作用：URL 操作。
 * */
using System;
using System.Collections.Generic;
using System.Text;

namespace Helper.Core.Library
{
    public class UriHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 生成短链接
        /// </summary>
        /// <param name="url">原始链接</param>
        /// <param name="key">加密 KEY</param>
        /// <returns></returns>
        public static string[] Short(string url, string key)
        {
            // 要使用生成 URL 的字符字典
            string[] charDict = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            // 加密 URL 地址
            string hex = EncryptHelper.MD5(key + url);

            string[] resultList = new string[4];
            StringBuilder stringBuilder = null;
            for (int i = 0; i < 4; i++)
            {
                // 把加密过的字符按照每 8 位一组 16 进制与 0x3FFFFFFF 进行位与运算
                int hexint = 0x3FFFFFFF & Convert.ToInt32("0x" + hex.Substring(i * 8, 8), 16);

                stringBuilder = new StringBuilder();
                for (int j = 0; j < 6; j++)
                {
                    // 把得到的值与 0x0000003D 进行位与运算，取得字符字典索引
                    int index = 0x0000003D & hexint;
                    stringBuilder.Append(charDict[index]);
                    hexint = hexint >> 5;
                }
                resultList[i] = stringBuilder.ToString();
            }
            return resultList;
        }
        /// <summary>
        /// 获取域名
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetDomain(string url)
        {
            Uri uri = new Uri(url);
            return uri.Host;
        }
        /// <summary>
        /// 获取 URL
        /// </summary>
        /// <param name="url">URL 地址</param>
        /// <returns></returns>
        public static string GetUrl(string url)
        {
            Uri uri = new Uri(url);
            return uri.AbsolutePath;
        }
        /// <summary>
        /// URL 解码
        /// </summary>
        /// <param name="url">URL 地址</param>
        /// <returns></returns>
        public static string UrlDecode(string url)
        {
            return System.Web.HttpUtility.UrlDecode(url);
        }
        /// <summary>
        /// URL 解码
        /// </summary>
        /// <param name="url">URL 地址</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static string UrlDecode(string url, Encoding encoding)
        {
            return System.Web.HttpUtility.UrlDecode(url, encoding);
        }
        /// <summary>
        /// URL 编码
        /// </summary>
        /// <param name="url">URL 地址</param>
        /// <returns></returns>
        public static string UrlEncode(string url)
        {
            return System.Web.HttpUtility.UrlEncode(url);
        }
        /// <summary>
        /// URL 编码
        /// </summary>
        /// <param name="url">URL 地址</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static string UrlEncode(string url, Encoding encoding)
        {
            return System.Web.HttpUtility.UrlEncode(url, encoding);
        }
        /// <summary>
        /// 获取参数列表
        /// </summary>
        /// <param name="url">Url 地址</param>
        /// <returns></returns>
        public static List<string> GetQueryList(string url)
        {
            return GetQueryList(url, Encoding.UTF8);
        }
        /// <summary>
        /// 获取参数列表
        /// </summary>
        /// <param name="url">Url 地址</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static List<string> GetQueryList(string url, Encoding encoding)
        {
            Uri uri = new Uri(url);
            string query = uri.PathAndQuery;
            if (query.IndexOf(".") > 0) query = query.Substring(0, query.IndexOf("."));

            List<string> dataList = new List<string>();
            string[] queryDataList = query.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (queryDataList != null && queryDataList.Length > 0)
            {
                foreach (string queryData in queryDataList)
                {
                    dataList.Add(System.Web.HttpUtility.UrlDecode(queryData, encoding));
                }
            }
            return dataList;
        }
        /// <summary>
        /// 获取参数字典
        /// </summary>
        /// <param name="url">Url 地址</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetQueryDict(string url)
        {
            return GetQueryDict(url, Encoding.UTF8);
        }
        /// <summary>
        /// 获取参数字典
        /// </summary>
        /// <param name="url">Url 地址</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetQueryDict(string url, Encoding encoding)
        {
            Uri uri = new Uri(url);
            string query = uri.Query.TrimStart(new char[] { '?' });

            Dictionary<string, string> dataDict = new Dictionary<string, string>();
            string[] queryDataList = query.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            if (queryDataList != null && queryDataList.Length > 0)
            {
                foreach (string queryData in queryDataList)
                {
                    string[] keyValue = queryData.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!dataDict.ContainsKey(keyValue[0]))
                    {
                        dataDict.Add(keyValue[0].ToLower(), System.Web.HttpUtility.UrlDecode(keyValue[1], encoding));
                    }
                }
            }
            return dataDict;
        }
        #endregion
    }
}
