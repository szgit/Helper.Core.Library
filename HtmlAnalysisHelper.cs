/*
 * 作用：通过 HtmlAgilityPack 和 XPath 实现 Html 数据解析。
 * */
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    public enum HtmlAnalysisEnum
    {
        /// <summary>
        /// 严格模式
        /// </summary>
        Strict = 1,

        /// <summary>
        /// 非严格模式
        /// </summary>
        Normal = 2
    }
    #endregion

    public class HtmlAnalysisHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 获取标题
        /// </summary>
        /// <param name="htmlDocument">HtmlDocument</param>
        /// <returns></returns>
        public static string GetTitle(HtmlDocument htmlDocument)
        {
            return ToT<string>(htmlDocument, "//title", null, HtmlAnalysisAttributeEnum.Text);
        }
        /// <summary>
        /// 获取内容
        /// </summary>
        /// <typeparam name="T">基类类型，例：string</typeparam>
        /// <param name="htmlDocument">HtmlDocument</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="attributeName">属性名称</param>
        /// <param name="attributeEnum">属性枚举</param>
        /// <returns></returns>
        public static T ToT<T>(HtmlDocument htmlDocument, string xPath, string attributeName, HtmlAnalysisAttributeEnum attributeEnum = HtmlAnalysisAttributeEnum.Attribute)
        {
            T t = default(T);

            HtmlNode documentNode = htmlDocument.DocumentNode;
            if (documentNode == null) return t;

            HtmlNode htmlNode = documentNode.SelectSingleNode(xPath);
            if (htmlNode == null) return t;

            string attributeValue = GetNodeText(htmlNode, attributeName, attributeEnum);
            if (string.IsNullOrEmpty(attributeValue)) return t;

            t = (T)Convert.ChangeType(attributeValue, typeof(T));
            return t;
        }
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="analysisEnum">解析模式</param>
        /// <param name="htmlDocument">HtmlDocument</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="xPathMatchList">属性 XPath 查询语句列表</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(HtmlAnalysisEnum analysisEnum, HtmlDocument htmlDocument, string xPath, List<XPathMatch> xPathMatchList = null, object propertyMatchList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            if (analysisEnum == HtmlAnalysisEnum.Strict)
            {
                return ToEntityListStrict<T>(htmlDocument, xPath, xPathMatchList, propertyMatchList, reflectionType);
            }
            else
            {
                return ToEntityListNormal<T>(htmlDocument, xPath, xPathMatchList, propertyMatchList, reflectionType);
            }
        }
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="analysisEnum">解析模式</param>
        /// <param name="htmlDocument">HtmlDocument</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="xPathMatchDict">属性 XPath 查询语句字典</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(HtmlAnalysisEnum analysisEnum, HtmlDocument htmlDocument, string xPath, Dictionary<string, string> xPathMatchDict, object propertyMatchList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<XPathMatch> xPathMatchList = new List<XPathMatch>();
            foreach(var keyValueItem in xPathMatchDict)
            {
                xPathMatchList.Add(new XPathMatch() { PropertyName = keyValueItem.Key, XPath = keyValueItem.Value });
            }
            return ToEntityList<T>(analysisEnum, htmlDocument, xPath, xPathMatchList, propertyMatchList, reflectionType);
        }
        /// <summary>
        /// 返回基础类型数据列表
        /// </summary>
        /// <typeparam name="T">基类类型，例：string</typeparam>
        /// <param name="htmlDocument">HtmlDocument</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="attributeName">属性名称</param>
        /// <param name="attributeEnum">属性枚举</param>
        /// <returns></returns>
        public static List<T> ToList<T>(HtmlDocument htmlDocument, string xPath, string attributeName, HtmlAnalysisAttributeEnum attributeEnum = HtmlAnalysisAttributeEnum.Attribute)
        {
            HtmlNode documentNode = htmlDocument.DocumentNode;
            if (documentNode == null) return null;

            List<T> dataList = new List<T>();

            HtmlNodeCollection htmlNodeCollection = documentNode.SelectNodes(xPath);
            if (htmlNodeCollection == null || htmlNodeCollection.Count == 0) return null;

            foreach(HtmlNode htmlNode in htmlNodeCollection)
            {
                string attributeValue = GetNodeText(htmlNode, attributeName, attributeEnum);
                if (!string.IsNullOrEmpty(attributeValue))
                {
                    dataList.Add((T)Convert.ChangeType(attributeValue, typeof(T)));
                }
            }

            return dataList;
        }
        #endregion

        #region 逻辑处理私有函数
        private static List<T> ToEntityListStrict<T>(HtmlDocument htmlDocument, string xPath, List<XPathMatch> xPathMatchList = null, object propertyMatchList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            HtmlNode documentNode = htmlDocument.DocumentNode;
            if (documentNode == null) return null;

            List<T> dataList = new List<T>();

            HtmlNodeCollection htmlNodeCollection = documentNode.SelectNodes(xPath);
            if (htmlNodeCollection == null || htmlNodeCollection.Count == 0) return null;

            Dictionary<string, List<HtmlNode>> propertyNodeDict = GetXPathNodeDict(documentNode, xPathMatchList, htmlNodeCollection.Count);

            dynamic propertySetDict = null;
            if (reflectionType != ReflectionTypeEnum.Original) propertySetDict = ReflectionExtendHelper.PropertySetCallDict<T>(reflectionType);

            Dictionary<PropertyInfo, HtmlAttributeMapperItem> attributeMapperDict = GetAttributeMapperDict<T>(propertyMatchList);

            int index = 0;
            foreach (HtmlNode mathHtmlNode in htmlNodeCollection)
            {
                dataList.Add(HtmlNodeToEntity<T>(mathHtmlNode, index, propertyNodeDict, propertySetDict, attributeMapperDict));
                index++;
            }
            return dataList;
        }
        private static List<T> ToEntityListNormal<T>(HtmlDocument htmlDocument, string xPath, List<XPathMatch> xPathMatchList = null, object propertyMatchList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            HtmlNode documentNode = htmlDocument.DocumentNode;
            if (documentNode == null) return null;

            List<T> dataList = new List<T>();

            dynamic propertySetDict = null;
            if (reflectionType != ReflectionTypeEnum.Original) propertySetDict = ReflectionExtendHelper.PropertySetCallDict<T>(reflectionType);

            Dictionary<PropertyInfo, HtmlAttributeMapperItem> attributeMapperDict = GetAttributeMapperDict<T>(propertyMatchList);

            HtmlNodeCollection htmlNodeCollection = documentNode.SelectNodes(xPath);
            if (htmlNodeCollection == null || htmlNodeCollection.Count == 0) return null;

            Dictionary<PropertyInfo, XPathMatch> propertyInfoXPathDict = new Dictionary<PropertyInfo, XPathMatch>();

            XPathMatch matchItem = null;
            foreach (HtmlNode htmlNode in htmlNodeCollection)
            {
                T t = new T();
                ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
                {
                    matchItem = null;
                    if (propertyInfoXPathDict.ContainsKey(propertyInfo))
                    {
                        matchItem = propertyInfoXPathDict[propertyInfo];
                    }
                    else
                    {
                        matchItem = xPathMatchList.Where(p => p.PropertyName == propertyInfo.Name).FirstOrDefault();
                        if (matchItem != null) propertyInfoXPathDict.Add(propertyInfo, matchItem);
                    }
                    if (matchItem != null)
                    {
                        HtmlNode childHtmlNode = htmlNode.SelectSingleNode(matchItem.XPath);
                        SetEntityValue<T>(t, childHtmlNode, propertyInfo, attributeMapperDict[propertyInfo], propertySetDict);
                    }
                });
                dataList.Add(t);
            }
            return dataList;
        }
        private static T HtmlNodeToEntity<T>(HtmlNode htmlNode, int index, Dictionary<string, List<HtmlNode>> nodeDict, dynamic propertySetDict, Dictionary<PropertyInfo, HtmlAttributeMapperItem> attributeMapperDict) where T : class, new()
        {
            T t = ReflectionGenericHelper.New<T>();

            HtmlNode dataNode = htmlNode;
            foreach (var keyValueItem in attributeMapperDict)
            {
                if (nodeDict != null && nodeDict.ContainsKey(keyValueItem.Key.Name))
                {
                    dataNode = nodeDict[keyValueItem.Key.Name][index];
                }
                string attributeValue = GetNodeText(dataNode, keyValueItem.Value.AttributeName, keyValueItem.Value.AttributeEnum);
                if (!string.IsNullOrEmpty(attributeValue))
                {
                    if (propertySetDict != null && propertySetDict.ContainsKey(keyValueItem.Key.Name))
                    {
                        ReflectionGenericHelper.SetPropertyValue(propertySetDict[keyValueItem.Key.Name], t, attributeValue, keyValueItem.Key);
                    }
                    else
                    {
                        ReflectionHelper.SetPropertyValue(t, attributeValue, keyValueItem.Key);
                    }
                }
            }
            return t;
        }
        private static void SetEntityValue<T>(T t, HtmlNode htmlNode, PropertyInfo propertyInfo, HtmlAttributeMapperItem mapperItem, dynamic propertySetDict) where T : class
        {

            string attributeValue = GetNodeText(htmlNode, mapperItem.AttributeName, mapperItem.AttributeEnum);
            if (!string.IsNullOrEmpty(attributeValue))
            {
                if (propertySetDict != null && propertySetDict.ContainsKey(propertyInfo.Name))
                {
                    ReflectionGenericHelper.SetPropertyValue(propertySetDict[propertyInfo.Name], t, attributeValue, propertyInfo);
                }
                else
                {
                    ReflectionHelper.SetPropertyValue(t, attributeValue, propertyInfo);
                }
            }
        }
        private static Dictionary<string, List<HtmlNode>> GetXPathNodeDict(HtmlNode htmlNode, List<XPathMatch> xPathMatchList, int nodeCount)
        {
            Dictionary<string, List<HtmlNode>> nodeDict = new Dictionary<string, List<HtmlNode>>();
            foreach(XPathMatch matchItem in xPathMatchList)
            {
                HtmlNodeCollection htmlNodeList = htmlNode.SelectNodes(matchItem.XPath);
                if(htmlNodeList != null && htmlNodeList.Count == nodeCount)
                {
                    nodeDict.Add(matchItem.PropertyName, htmlNodeList.ToList());
                }
            }
            return nodeDict;
        }
        private static Dictionary<PropertyInfo, HtmlAttributeMapperItem> GetAttributeMapperDict<T>(object propertyMatchList) where T : class
        {
            Dictionary<string, object> propertyMatchDict = CommonHelper.GetParameterDict(propertyMatchList);
            Dictionary<PropertyInfo, HtmlAttributeMapperItem> propertyDict = new Dictionary<PropertyInfo, HtmlAttributeMapperItem>();
            ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
            {
                HtmlAttributeMapperItem mapperItem = new HtmlAttributeMapperItem();

                if (propertyMatchDict != null && propertyMatchDict.ContainsKey(propertyInfo.Name))
                {
                    mapperItem.AttributeName = propertyMatchDict[propertyInfo.Name].ToString();
                    mapperItem.AttributeEnum = HtmlAnalysisAttributeEnum.Attribute;
                }
                else
                {
                    HtmlAnalysisTAttribute attribute = propertyInfo.GetCustomAttribute<HtmlAnalysisTAttribute>();
                    if (attribute != null)
                    {
                        mapperItem.AttributeName = attribute.Name;
                        mapperItem.AttributeEnum = attribute.NameType;
                    }
                    else
                    {
                        mapperItem.AttributeName = propertyInfo.Name;
                        mapperItem.AttributeEnum = HtmlAnalysisAttributeEnum.Attribute;
                    }
                }
                propertyDict.Add(propertyInfo, mapperItem);
            });
            return propertyDict;
        }
        private static string GetNodeText(HtmlNode htmlNode, string attributeName, HtmlAnalysisAttributeEnum attributeEnum)
        {
            if (htmlNode == null) return null;
            if (attributeEnum == HtmlAnalysisAttributeEnum.Text) return htmlNode.InnerText;
            if (attributeEnum == HtmlAnalysisAttributeEnum.Html) return htmlNode.InnerHtml;
            if (htmlNode.Attributes[attributeName] == null) return null;
            return htmlNode.Attributes[attributeName].Value;
        }
        #endregion
    }

    #region 逻辑处理辅助特性
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class HtmlAnalysisTAttribute : Attribute
    {
        private string name;
        private HtmlAnalysisAttributeEnum nameType;
        
        /// <summary>
        /// 实体属性映射 Attribute 名称
        /// </summary>
        /// <param name="name">属性所对应的 Attribute 名称</param>
        /// <param name="nameType">属性枚举</param>
        public HtmlAnalysisTAttribute(string name, HtmlAnalysisAttributeEnum nameType = HtmlAnalysisAttributeEnum.Attribute)
        {
            this.name = name;
            this.nameType = nameType;
        }

        /// <summary>
        /// 实体属性所对应的 Attribute 名称
        /// </summary>
        public string Name { get { return this.name; } }

        /// <summary>
        /// 属性枚举
        /// </summary>
        public HtmlAnalysisAttributeEnum NameType { get { return this.nameType; } }
    }
    public enum HtmlAnalysisAttributeEnum
    {
        /// <summary>
        /// 属性值
        /// </summary>
        Attribute = 0,

        /// <summary>
        /// InnerText 数据
        /// </summary>
        Text = 1,

        /// <summary>
        /// InnerHtml 数据
        /// </summary>
        Html = 2
    }
    #endregion

    #region 逻辑处理辅助类
    internal class HtmlAttributeMapperItem
    {
        public string AttributeName { get; set; }
        public HtmlAnalysisAttributeEnum AttributeEnum { get; set; }
    }
    public class XPathMatch
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// XPath
        /// </summary>
        public string XPath { get; set; }
    }
    #endregion
}
