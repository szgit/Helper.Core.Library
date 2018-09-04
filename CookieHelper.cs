/*
 * 作用：读取/设置 Cookie 数据。
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;

namespace Helper.Core.Library
{
    public class CookieHelper
    {
        #region 对外公开方法

        #region 设置 Cookie
        /// <summary>
        /// 设置 Cookie 数据
        /// </summary>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="cookieValue">Cookie 数据</param>
        /// <param name="httpResponse">HttpResponseBase，如果未指定则取自 HttpContext.Current.Response</param>
        public static void SetCookie(string cookieName, string cookieValue, HttpResponseBase httpResponse = null)
        {
            SetCookie(cookieName, cookieValue, DateTime.MaxValue, httpResponse);
        }
        /// <summary>
        /// 设置 Cookie 数据
        /// </summary>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="cookieValue">Cookie 数据</param>
        /// <param name="expires">过期时间</param>
        /// <param name="httpResponse">HttpResponseBase，如果未指定则取自 HttpContext.Current.Response</param>
        public static void SetCookie(string cookieName, string cookieValue, DateTime expires, HttpResponseBase httpResponse = null)
        {
            HttpCookie httpCookie = new HttpCookie(cookieName);
            httpCookie.HttpOnly = true;
            httpCookie.Expires = expires;
            httpCookie.Value = cookieValue;

            if (httpResponse != null)
            {
                httpResponse.Cookies.Add(httpCookie);
            }
            else
            {
                HttpContext.Current.Response.Cookies.Add(httpCookie);
            }
        }
        /// <summary>
        /// 设置 Cookie 数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="data">实体数据</param>
        /// <param name="httpResponse">HttpResponseBase，如果未指定则取自 HttpContext.Current.Response</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="propertyList">属性列表，如果指定，则按指定属性列表设置 Cookie 数据</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="reflectionType">反射类型</param>
        public static void SetCookieT<T>(string cookieName, T data, HttpResponseBase httpResponse = null, object propertyMatchList = null, string[] propertyList = null, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            SetCookieT<T>(cookieName, data, DateTime.MaxValue, httpResponse, propertyMatchList, propertyList, propertyContain, reflectionType);
        }
        /// <summary>
        /// 设置 Cookie 数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="data">实体数据</param>
        /// <param name="expires">过期时间</param>
        /// <param name="httpResponse">HttpResponseBase，如果未指定则取自 HttpContext.Current.Response</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="propertyList">属性列表，如果指定，则按指定属性列表设置 Cookie 数据</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="reflectionType">反射类型</param>
        public static void SetCookieT<T>(string cookieName, T data, DateTime expires, HttpResponseBase httpResponse = null, object propertyMatchList = null, string[] propertyList = null, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            List<string> filterNameList = null;
            if (propertyList != null) filterNameList = propertyList.ToList<string>();

            Dictionary<string, object> propertyDict = CommonHelper.GetParameterDict(propertyMatchList);
            ExecuteSetCookie<T>(httpResponse, cookieName, data, expires, propertyDict, filterNameList, propertyContain, reflectionType);
        }
        #endregion

        #region 读取 Cookie
        /// <summary>
        /// 读取 Cookie 数据
        /// </summary>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="httpRequest">HttpRequestBase，如果未指定则取自 HttpContext.Current.Request</param>
        /// <returns></returns>
        public static string GetCookie(string cookieName, HttpRequestBase httpRequest = null)
        {
            HttpCookie httpCookie = GetHttpCookie(httpRequest, cookieName);
            if (httpCookie == null) return null;

            return httpCookie.Value;
        }
        /// <summary>
        /// 获取 Cookie 数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="httpRequest">HttpRequestBase，如果未指定则取自 HttpContext.Current.Request</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static T GetCookieT<T>(string cookieName, HttpRequestBase httpRequest = null, object propertyMatchList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class,new()
        {
            Dictionary<string, object> propertyDict = CommonHelper.GetParameterDict(propertyMatchList);
            return ExecuteGetCookie<T>(httpRequest, cookieName, propertyDict, reflectionType);
        }
        #endregion

        #region 删除 Cookie
        /// <summary>
        /// 删除 Cookie 数据
        /// </summary>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="httpResponse">HttpResponseBase，如果未指定则取自 HttpContext.Current.Response</param>
        public static void DeleteCookie(string cookieName, HttpResponseBase httpResponse = null)
        {
            HttpCookie httpCookie = GetHttpCookie(httpResponse, cookieName);
            if (httpCookie == null) return;
            httpCookie.Expires = DateTime.Now.AddDays(-1);
        }
        #endregion

        #endregion

        #region 逻辑处理私有方法
        internal static void ExecuteSetCookie<T>(HttpResponseBase httpResponse, string cookieName, T data, DateTime expires, Dictionary<string, object> propertyDict, List<string> propertyList, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Dictionary<string, string> cookieNameDict = CommonHelper.InitPropertyWriteMapper<T, CookieTAttribute>(propertyDict, propertyList, propertyContain);

            dynamic propertyGetDict = null;
            if (reflectionType != ReflectionTypeEnum.Original) propertyGetDict = ReflectionExtendHelper.PropertyGetCallDict<T>(reflectionType);

            HttpCookie httpCookie = new HttpCookie(cookieName);
            httpCookie.HttpOnly = true;
            httpCookie.Expires = expires;

            object cookieValue = null;
            foreach (KeyValuePair<string, string> keyValueItem in cookieNameDict)
            {
                if (propertyGetDict != null && propertyGetDict.ContainsKey(keyValueItem.Value))
                {
                    cookieValue = propertyGetDict[keyValueItem.Value](data);
                }
                else
                {
                    cookieValue = ReflectionHelper.GetPropertyValue(data, keyValueItem.Value);
                }
                if (cookieValue != null)
                {
                    httpCookie.Values.Add(keyValueItem.Key, HttpUtility.UrlEncode(cookieValue.ToString()));
                }
            }

            if (httpResponse != null)
            {
                httpResponse.Cookies.Add(httpCookie);
            }
            else
            {
                HttpContext.Current.Response.Cookies.Add(httpCookie);
            }
        }
        internal static T ExecuteGetCookie<T>(HttpRequestBase httpRequest, string cookieName, Dictionary<string, object> propertyDict, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class,new()
        {
            HttpCookie httpCookie = GetHttpCookie(httpRequest, cookieName);
            if (httpCookie == null) return null;

            Dictionary<PropertyInfo, string> mapperDict = CommonHelper.InitPropertyReadMapper<T, CookieTAttribute>(propertyDict, (name) => httpCookie.Values[name] != null);

            dynamic propertySetDict = null;
            if (reflectionType != ReflectionTypeEnum.Original) propertySetDict = ReflectionExtendHelper.PropertySetCallDict<T>(reflectionType);

            T t = ReflectionGenericHelper.New<T>();
            foreach (var keyValueItem in mapperDict)
            {
                if (propertySetDict != null && propertySetDict.ContainsKey(keyValueItem.Key.Name))
                {
                    ReflectionGenericHelper.SetPropertyValue(propertySetDict[keyValueItem.Key.Name], t, HttpUtility.UrlDecode(httpCookie[keyValueItem.Value]), keyValueItem.Key);
                }
                else
                {
                    ReflectionHelper.SetPropertyValue(t, HttpUtility.UrlDecode(httpCookie[keyValueItem.Value]), keyValueItem.Key);
                }
            }
            return t;
        }
        private static HttpCookie GetHttpCookie(HttpResponseBase httpResponse, string cookieName)
        {
            if(httpResponse != null)
            {
                return httpResponse.Cookies[cookieName];
            }
            else
            {
                return HttpContext.Current.Response.Cookies[cookieName];
            }
        }
        private static HttpCookie GetHttpCookie(HttpRequestBase httpRequest, string cookieName)
        {
            if (httpRequest != null)
            {
                return httpRequest.Cookies[cookieName];
            }
            else
            {
                return HttpContext.Current.Request.Cookies[cookieName];
            }
        }
        #endregion
    }

    #region 逻辑处理辅助特性
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class CookieTAttribute : BaseReadAndWriteTAttribute
    {
        public CookieTAttribute(string name, AttributeReadAndWriteTypeEnum type = AttributeReadAndWriteTypeEnum.ReadAndWrite)
            : base(name, type)
        {

        }
    }
    #endregion
}
