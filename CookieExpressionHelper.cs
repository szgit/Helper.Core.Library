/*
 * 作用：读取/设置 Cookie 数据，是对 CookieHelper 的扩展，参数使用表达式，目的是减少属性名的拼写错误。
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Helper.Core.Library
{
    public class CookieExpressionHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 设置 Cookie 数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="data">实体数据</param>
        /// <param name="httpResponse">HttpResponseBase，如果未指定则取自 HttpContext.Current.Response</param>
        /// <param name="propertyMatchExpression">属性匹配，例：p=> new { ID = p.UserID }</param>
        /// <param name="propertyExpression">属性列表，如果指定则按指定属性列表设置 Cookie 数据，例：p=> new { p.UserID }</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="reflectionType">反射类型</param>
        public static void SetCookieT<T>(string cookieName, T data, HttpResponseBase httpResponse = null, Expression<Func<T, object>> propertyMatchExpression = null, Expression<Func<T, object>> propertyExpression = null, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            SetCookieT<T>(cookieName, data, DateTime.MaxValue, httpResponse, propertyMatchExpression, propertyExpression, propertyContain, reflectionType);
        }
        /// <summary>
        /// 设置 Cookie 数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="data">实体数据</param>
        /// <param name="expires">过期时间</param>
        /// <param name="httpResponse">HttpResponseBase，如果未指定则取自 HttpContext.Current.Response</param>
        /// <param name="propertyMatchExpression">属性匹配，例：p=> new { ID = p.UserID }</param>
        /// <param name="propertyExpression">属性列表，如果指定则按指定属性列表设置 Cookie 数据，例：p=> new { p.UserID }</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="reflectionType">反射类型</param>
        public static void SetCookieT<T>(string cookieName, T data, DateTime expires, HttpResponseBase httpResponse = null, Expression<Func<T, object>> propertyMatchExpression = null, Expression<Func<T, object>> propertyExpression = null, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Dictionary<string, object> propertyDict = CommonHelper.GetExpressionDict<T>(propertyMatchExpression);
            List<string> propertyNameList = CommonHelper.GetExpressionList<T>(propertyExpression);

            CookieHelper.ExecuteSetCookie<T>(httpResponse, cookieName, data, expires, propertyDict, propertyNameList, propertyContain, reflectionType);
        }
        /// <summary>
        /// 获取 Cookie 数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="httpRequest">HttpRequestBase，如果未指定则取自 HttpContext.Current.Request</param>
        /// <param name="propertyMatchExpression">属性匹配，例：p=> new { ID = p.UserID }</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static T GetCookieT<T>(string cookieName, HttpRequestBase httpRequest = null, Expression<Func<T, object>> propertyMatchExpression = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class,new()
        {
            Dictionary<string, object> propertyDict = CommonHelper.GetExpressionDict<T>(propertyMatchExpression);
            return CookieHelper.ExecuteGetCookie<T>(httpRequest, cookieName, propertyDict, reflectionType);
        }
        #endregion
    }
}
