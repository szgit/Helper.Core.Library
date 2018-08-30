/*
 * 作用：读取/设置 Cookie 数据。
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
        /// <param name="httpResponse">HttpResponseBase</param>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="data">实体数据</param>
        /// <param name="propertyMatchExpression">属性匹配，例：p=> new { ID = p.UserID }</param>
        /// <param name="propertyExpression">属性列表，如果指定则按指定属性列表设置 Cookie 数据，例：p=> new { p.UserID }</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="reflectionType">反射类型</param>
        public static void SetCookie<T>(HttpResponseBase httpResponse, string cookieName, T data, Expression<Func<T, object>> propertyMatchExpression, Expression<Func<T, object>> propertyExpression = null, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            SetCookie<T>(httpResponse, cookieName, data, DateTime.MaxValue, propertyMatchExpression, propertyExpression, propertyContain, reflectionType);
        }

        /// <summary>
        /// 设置 Cookie 数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="httpResponse">HttpResponseBase</param>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="data">实体数据</param>
        /// <param name="expires">过期时间</param>
        /// <param name="propertyMatchExpression">属性匹配，例：p=> new { ID = p.UserID }</param>
        /// <param name="propertyExpression">属性列表，如果指定则按指定属性列表设置 Cookie 数据，例：p=> new { p.UserID }</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="reflectionType">反射类型</param>
        public static void SetCookie<T>(HttpResponseBase httpResponse, string cookieName, T data, DateTime expires, Expression<Func<T, object>> propertyMatchExpression, Expression<Func<T, object>> propertyExpression = null, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Dictionary<string, object> propertyDict = CommonHelper.GetExpressionDict<T>(propertyMatchExpression);
            List<string> propertyNameList = CommonHelper.GetExpressionList<T>(propertyExpression);

            CookieHelper.ExecuteSetCookie<T>(httpResponse, cookieName, data, expires, propertyDict, propertyNameList, propertyContain, reflectionType);
        }

        /// <summary>
        /// 获取 Cookie 数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="httpRequest">HttpRequestBase</param>
        /// <param name="cookieName">Cookie 名称</param>
        /// <param name="propertyMatchExpression">属性匹配，例：p=> new { ID = p.UserID }</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static T GetCookie<T>(HttpRequestBase httpRequest, string cookieName, Expression<Func<T, object>> propertyMatchExpression, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class,new()
        {
            Dictionary<string, object> propertyDict = CommonHelper.GetExpressionDict<T>(propertyMatchExpression);
            return CookieHelper.ExecuteGetCookie<T>(httpRequest, cookieName, propertyDict, reflectionType);
        }
        #endregion
    }
}
