/*
 * 作用：通过 HtmlAgilityPack 和 XPath 实现页面数据采集。
 * */
using DotNet.Utilities;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helper.Core.Library
{
    public class HtmlCollectionHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 异步获取数据
        /// </summary>
        /// <param name="analysisEnum">解析模式</param>
        /// <param name="httpItem">HttpItem</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="xPathMatchList">属性 XPath 查询语句列表</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public async Task<List<T>> AsyncToList<T>(HtmlAnalysisEnum analysisEnum, HttpItem httpItem, string xPath, List<XPathMatch> xPathMatchList = null, object propertyMatchList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            HttpHelper httpHelper = new HttpHelper();
            HttpResult httpResult = httpHelper.GetHtml(httpItem);

            if (httpResult == null) return null;

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(httpResult.Html);

            List<T> dataList = null;

            await Task.Run(() =>
            {
                dataList = HtmlAnalysisHelper.ToEntityList<T>(analysisEnum, htmlDocument, xPath, xPathMatchList, propertyMatchList, reflectionType);
            });

            return dataList;
        }
        /// <summary>
        /// 同步获取数据
        /// </summary>
        /// <param name="analysisEnum">解析模式</param>
        /// <param name="httpItem">HttpItem</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="xPathMatchList">属性 XPath 查询语句列表</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public List<T> ToList<T>(HtmlAnalysisEnum analysisEnum, HttpItem httpItem, string xPath, List<XPathMatch> xPathMatchList = null, object propertyMatchList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            HttpHelper httpHelper = new HttpHelper();
            HttpResult httpResult = httpHelper.GetHtml(httpItem);

            if (httpResult == null) return null;

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(httpResult.Html);

            return HtmlAnalysisHelper.ToEntityList<T>(analysisEnum, htmlDocument, xPath, xPathMatchList, propertyMatchList, reflectionType);
        }
        #endregion
    }
}
