/*
 * 作用：通过 XPath 获取/设置 XML 节点数据。
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Collections;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    internal class XmlFormat
    {
        public const string Xml = ".xml";
    }
    #endregion

    public class XmlHelper
    {
        #region 私有属性常量
        private const string XmlFormatErrorException = "Xml 文件后缀不正确！";
        private const string XmlToEntityTypeException = "IList<T> 类型获取出错";
        private static readonly Dictionary<string, Dictionary<PropertyInfo, XmlTAttribute>> PropertyAttributeDict = new Dictionary<string, Dictionary<PropertyInfo, XmlTAttribute>>();
        private static readonly object lockItem = new object();
        #endregion

        #region 对外公开方法

        #region GetNode
        /// <summary>
        /// 根据 XPath 获取 XmlNode
        /// </summary>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="xmlDocument">XmlDocument</param>
        /// <returns></returns>
        public static XmlNode GetNode(string xPath, XmlDocument xmlDocument)
        {
            return xmlDocument.SelectSingleNode(xPath);
        }
        #endregion

        #region GetValue
        /// <summary>
        /// 根据 XPath 获取属性值
        /// </summary>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="attributeName">属性名称</param>
        /// <param name="xmlDocument">XmlDocument</param>
        /// <returns></returns>
        public static string GetValue(string xPath, string attributeName, XmlDocument xmlDocument)
        {
            XmlNode xmlNode = GetNode(xPath, xmlDocument);
            if (xmlNode != null)
            {
                if (xmlNode.Attributes[attributeName] != null) return xmlNode.Attributes[attributeName].Value;
            }
            return null;
        }
        #endregion

        #region SetValue
        /// <summary>
        /// 根据 XPath 设置属性值
        /// </summary>
        /// <param name="xmlPath">Xml 文件地址，未设置则不保存</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="attributeName">属性名称</param>
        /// <param name="attributeValue">属性值</param>
        /// <param name="xmlDocument">XmlDocument</param>
        public static void SetValue(string xmlPath, string xPath, string attributeName, string attributeValue, XmlDocument xmlDocument)
        {
            XmlNode xmlNode = GetNode(xPath, xmlDocument);
            if (xmlNode == null) return;

            SetXmlNodeAttribute(xmlNode, attributeName, attributeValue, xmlDocument);
            if (!string.IsNullOrEmpty(xmlPath))
            {
                xmlDocument.Save(xmlPath);
            }
        }
        /// <summary>
        /// 根据 XPath 设置属性值
        /// </summary>
        /// <param name="xmlPath">Xml 文件地址</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="attributeName">属性名称</param>
        /// <param name="attributeValue">属性值</param>
        public static void SetValue(string xmlPath, string xPath, string attributeName, string attributeValue)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);

            SetValue(xmlPath, xPath, attributeName, attributeValue, xmlDocument);
        }
        #endregion

        #region SetValue<T>
        /// <summary>
        /// 根据 XPath 设置属性值
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="xmlPath">Xml 文件地址，未设置则不保存</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="data">实体数据</param>
        /// <param name="xmlDocument">XmlDocument</param>
        /// <param name="append">是否子节点</param>
        /// <param name="propertyList">属性列表</param>
        /// <param name="reflectionType">反射类型</param>
        public static void SetValue<T>(string xmlPath, string xPath, T data, XmlDocument xmlDocument = null, bool append = false, string[] propertyList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            if(xmlDocument == null)
            {
                xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlPath);
            }
            ExecuteSetValue<T>((List<string> filterNameList, dynamic propertyGetDict, XmlNode xmlNode, string elementName) =>
            {
                Dictionary<string, EntityToXmlMapper> propertyMapperDict = InitEntityToXmlMapper<T>(data, propertyGetDict, filterNameList);
                if (propertyMapperDict == null) return;

                if (!append)
                {
                    foreach (var keyValueItem in propertyMapperDict)
                    {
                        SetXmlNodeAttribute(xmlNode, keyValueItem.Key, keyValueItem.Value.Value, xmlDocument, keyValueItem.Value.ValueEnum);
                    }
                }
                else
                {
                    XmlElement xmlElement = CreateXmlElement<T>(propertyMapperDict, xmlDocument, elementName);
                    if(xmlElement != null) xmlNode.AppendChild(xmlElement);
                }
            }, xmlPath, xPath, xmlDocument, propertyList, reflectionType);
        }
        /// <summary>
        /// 根据 XPath 设置属性值
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="xmlPath">Xml 文件地址，未设置则不保存</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="dataList">实体数据列表</param>
        /// <param name="xmlDocument">XmlDocument</param>
        /// <param name="propertyList">属性列表</param>
        /// <param name="reflectionType">反射类型</param>
        public static void SetValue<T>(string xmlPath, string xPath, List<T> dataList, XmlDocument xmlDocument, string[] propertyList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            if (xmlDocument == null)
            {
                xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlPath);
            }
            ExecuteSetValue<T>((List<string> filterNameList, dynamic propertyGetDict, XmlNode xmlNode, string elementName) =>
            {
                foreach (T t in dataList)
                {
                    Dictionary<string, EntityToXmlMapper> propertyMapperDict = InitEntityToXmlMapper<T>(t, propertyGetDict, filterNameList);
                    if (propertyMapperDict != null)
                    {
                        XmlElement xmlElement = CreateXmlElement<T>(propertyMapperDict, xmlDocument, elementName);
                        if (xmlElement != null) xmlNode.AppendChild(xmlElement);
                    }
                }
            }, xmlPath, xPath, xmlDocument, propertyList, reflectionType);
        }
        #endregion

        #region ToEntity<T>
        /// <summary>
        /// 根据 XPath 返回实体数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="xmlDocument">XmlDocument</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static T ToEntity<T>(XmlDocument xmlDocument, string xPath, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            XmlNode xmlNode = GetNode(xPath, xmlDocument);
            if (xmlNode != null)
            {
                return XmlNodeToEntity<T>(xmlNode, null, null, reflectionType);
            }
            return null;
        }
        /// <summary>
        /// 根据 XPath 返回实体数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="xmlPath">Xml 路径</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static T ToEntity<T>(string xmlPath, string xPath, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            T t = null;

            ExecuteXmlDocument(xmlPath, (XmlDocument xmlDocument) =>
            {
                t = ToEntity<T>(xmlDocument, xPath, reflectionType);
            });

            return t;
        }
        #endregion

        #region ToEntityList<T>
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">泛型实体对象</typeparam>
        /// <param name="xmlDocument">XmlDocument</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(XmlDocument xmlDocument, string xPath, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<T> dataList = new List<T>();

            Type type = typeof(T);

            Dictionary<PropertyInfo, XmlTAttribute> propertyNameDict = InitXmlToEntityMapper(type);

            dynamic propertySetDict = null;
            if (reflectionType != ReflectionTypeEnum.Original) propertySetDict = ReflectionExtendHelper.PropertySetCallDict<T>(reflectionType);

            // 根据 XPath 查询 XmlNodeList 数据
            XmlNodeList xmlNodeList = xmlDocument.SelectNodes(xPath);
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                dataList.Add(XmlNodeToEntity<T>(xmlNode, propertySetDict, propertyNameDict, reflectionType));
            }

            return dataList;
        }
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">泛型实体对象</typeparam>
        /// <param name="xmlPath">Xml 路径</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(string xmlPath, string xPath, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<T> dataList = null;
            ExecuteXmlDocument(xmlPath, (XmlDocument xmlDocument) =>
            {
                dataList = ToEntityList<T>(xmlDocument, xPath, reflectionType);
            });
            return dataList;
        }
        /// <summary>
        /// 返回基本类型数据列表
        /// </summary>
        /// <typeparam name="T">基本类型，例：string</typeparam>
        /// <param name="xmlDocument">XmlDocument</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="attributeName">属性名，为空则取 innerText 属性</param>
        /// <returns></returns>
        public static List<T> ToList<T>(XmlDocument xmlDocument, string xPath, string attributeName)
        {
            List<T> dataList = new List<T>();

            string attributeData = null;

            // 根据 XPath 查询 XmlNodeList 数据
            XmlNodeList xmlNodeList = xmlDocument.SelectNodes(xPath);
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if(string.IsNullOrEmpty(attributeName))
                {
                    attributeData = xmlNode.InnerText;
                }
                else
                {
                    attributeData = xmlNode.Attributes[attributeName].Value;
                }
                dataList.Add((T)Convert.ChangeType(attributeData, typeof(T)));
            }
            return dataList;
        }
        /// <summary>
        /// 返回基本类型数据列表
        /// </summary>
        /// <typeparam name="T">基本类型，例：string</typeparam>
        /// <param name="xmlPath">Xml 路径</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="attributeName">属性名，为空则取 innerText 属性</param>
        /// <returns></returns>
        public static List<T> ToList<T>(string xmlPath, string xPath, string attributeName)
        {
            List<T> dataList = null;
            ExecuteXmlDocument(xmlPath, (XmlDocument xmlDocument) =>
            {
                dataList = ToList<T>(xmlDocument, xPath, attributeName);
            });
            return dataList;
        }
        /// <summary>
        /// 返回字典数据
        /// </summary>
        /// <typeparam name="T">基本类型，例：string</typeparam>
        /// <typeparam name="K">基本类型，例：int</typeparam>
        /// <param name="xmlDocument">XmlDocument</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="keyAttributeName">键属性名称，为空则取 innerText 属性</param>
        /// <param name="valueAttributeName">值属性名称，为空则取 innerText 属性</param>
        /// <returns></returns>
        public static Dictionary<T, K> ToDict<T, K>(XmlDocument xmlDocument, string xPath, string keyAttributeName, string valueAttributeName)
        {
            Dictionary<T, K> dataDict = new Dictionary<T, K>();

            string keyData = null;
            string valueData = null;

            // 根据 XPath 查询 XmlNodeList 数据
            XmlNodeList xmlNodeList = xmlDocument.SelectNodes(xPath);
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if (string.IsNullOrEmpty(keyAttributeName))
                {
                    keyData = xmlNode.InnerText;
                }
                else
                {
                    keyData = xmlNode.Attributes[keyAttributeName].Value;
                }
                if (string.IsNullOrEmpty(valueAttributeName))
                {
                    valueData = xmlNode.InnerText;
                }
                else
                {
                    valueData = xmlNode.Attributes[valueAttributeName].Value;
                }
                if (!string.IsNullOrEmpty(keyData))
                {
                    dataDict.Add((T)Convert.ChangeType(keyData, typeof(T)), (K)Convert.ChangeType(valueData, typeof(K)));
                }
            }
            return dataDict;
        }
        /// <summary>
        /// 返回字典数据
        /// </summary>
        /// <typeparam name="T">基本类型，例：string</typeparam>
        /// <typeparam name="K">基本类型，例：int</typeparam>
        /// <param name="xmlPath">Xml 路径</param>
        /// <param name="xPath">XPath 查询语句</param>
        /// <param name="keyAttributeName">键属性名称，为空则取 innerText 属性</param>
        /// <param name="valueAttributeName">值属性名称，为空则取 innerText 属性</param>
        /// <returns></returns>
        public static Dictionary<T, K> ToDict<T, K>(string xmlPath, string xPath, string keyAttributeName, string valueAttributeName)
        {
            Dictionary<T, K> dataDict = null;
            ExecuteXmlDocument(xmlPath, (XmlDocument xmlDocument) =>
            {
                dataDict = ToDict<T, K>(xmlDocument, xPath, keyAttributeName, valueAttributeName);
            });
            return dataDict;
        }
        #endregion

        #region Xml <-> Object
        /// <summary>
        /// Xml 数据转实体 Object
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="xmlPath">Xml 文件地址</param>
        /// <returns></returns>
        public static object XmlToObject(Type type, string xmlPath)
        {
            string suffix = FileHelper.GetSuffix(xmlPath);
            if (suffix != XmlFormat.Xml) throw new Exception(XmlFormatErrorException);

            object result = null;
            using (FileStream fileStream = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                result = new XmlSerializer(type).Deserialize(fileStream);
            }
            return result;
        }

        /// <summary>
        /// Object 生成 Xml
        /// </summary>
        /// <param name="data">Object 数据</param>
        /// <param name="xmlPath">Xml 文件地址</param>
        /// <returns></returns>
        public static bool ObjectToXml(object data, string xmlPath)
        {
            string suffix = FileHelper.GetSuffix(xmlPath);
            if (suffix != XmlFormat.Xml) throw new Exception(XmlFormatErrorException);

            bool directoryResult = FileHelper.CreateDirectory(xmlPath);
            if (!directoryResult) return false;

            using (FileStream fileStream = new FileStream(xmlPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                new XmlSerializer(data.GetType()).Serialize(fileStream, data);
            }
            return true;
        }
        #endregion

        #endregion

        #region 逻辑处理私有方法
        private static void ExecuteXmlDocument(string xmlPath, Action<XmlDocument> callback)
        {
            string suffix = FileHelper.GetSuffix(xmlPath);
            if (suffix != XmlFormat.Xml) throw new Exception(XmlFormatErrorException);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);

            if (callback != null) callback(xmlDocument);
        }
        private static T XmlNodeToEntity<T>(XmlNode xmlNode, dynamic propertySetDict, Dictionary<PropertyInfo, XmlTAttribute> propertyNameDict, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            Type type = typeof(T);
            return XmlNodeToObject(type, xmlNode, propertySetDict, propertyNameDict, reflectionType) as T;
        }
        private static object XmlNodeToObject(Type type, XmlNode xmlNode, dynamic propertySetDict, Dictionary<PropertyInfo, XmlTAttribute> propertyNameDict, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            object t = ReflectionHelper.New(type);

            if (propertyNameDict == null) propertyNameDict = InitXmlToEntityMapper(type);
            if (propertySetDict == null)
            {
                if (reflectionType != ReflectionTypeEnum.Original) propertySetDict = ReflectionExtendHelper.PropertySetCallDict(type, reflectionType);
            }

            foreach(var keyValueItem in propertyNameDict)
            {
                XmlNodeList childXmlNodeList = null;
                string propertyValue = null;

                switch(keyValueItem.Value.NameType)
                {
                    case XmlTEnum.Attribute:
                        // 如果是获得 XmlNode 属性值
                        if (xmlNode.Attributes[keyValueItem.Value.Name] != null) propertyValue = xmlNode.Attributes[keyValueItem.Value.Name].Value;
                        ; break;
                    case XmlTEnum.Text:
                        // 如果是获得 XmlNode 的 Text 数据
                        propertyValue = xmlNode.InnerText;
                        break;
                    case XmlTEnum.Xml:
                        // 如果是获得 XmlNode 的子元素数据
                        propertyValue = xmlNode.InnerXml;
                        break;
                    case XmlTEnum.Element:
                    case XmlTEnum.ElementList:
                        // 如果是元素或者元素列表
                        childXmlNodeList = xmlNode.SelectNodes("descendant::" + keyValueItem.Value.Name);
                        break;
                }

                if (keyValueItem.Value.NameType == XmlTEnum.Element)
                {
                    SetPropertyValueByElement(t, keyValueItem.Key, childXmlNodeList, propertySetDict, reflectionType);
                }
                else if (keyValueItem.Value.NameType == XmlTEnum.ElementList)
                {
                    SetPropertyValueByElementList(t, keyValueItem.Key, childXmlNodeList, reflectionType);
                }
                else
                {
                    if (propertySetDict != null && propertySetDict.ContainsKey(keyValueItem.Key.Name))
                    {
                        ReflectionGenericHelper.SetPropertyValue(propertySetDict[keyValueItem.Key.Name], t, propertyValue, keyValueItem.Key);
                    }
                    else
                    {
                        ReflectionHelper.SetPropertyValue(t, propertyValue, keyValueItem.Key);
                    }
                }
            }

            return t;
        }
        private static void SetPropertyValueByElement(object t, PropertyInfo propertyInfo, XmlNodeList xmlNodeList, dynamic propertySetDict, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            if (t != null && xmlNodeList.Count > 0)
            {
                object propertyValue = XmlNodeToObject(propertyInfo.PropertyType, xmlNodeList[0], null, null, reflectionType);
                if (propertySetDict != null && propertySetDict.ContainsKey(propertyInfo.Name))
                {
                    propertySetDict[propertyInfo.Name](t, propertyValue);
                }
                else
                {
                    ReflectionHelper.SetPropertyValue(t, propertyValue, propertyInfo);
                }
            }
        }
        private static void SetPropertyValueByElementList(object t, PropertyInfo propertyInfo, XmlNodeList xmlNodeList, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            Type itemType = ReflectionHelper.GetListGenericType(propertyInfo);

            Dictionary<PropertyInfo, XmlTAttribute> propertyNameDict = InitXmlToEntityMapper(itemType);

            dynamic propertySetDict = null;
            if (reflectionType != ReflectionTypeEnum.Original) propertySetDict = ReflectionExtendHelper.PropertySetCallDict(itemType, reflectionType);

            var objectDataList = ReflectionHelper.NewList(itemType) as IList;
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                objectDataList.Add(XmlNodeToObject(itemType, xmlNode, propertySetDict, propertyNameDict, reflectionType));
            }
            if (propertySetDict != null && propertySetDict.ContainsKey(propertyInfo.Name))
            {
                propertySetDict[propertyInfo.Name](t, objectDataList);
            }
            else
            {
                ReflectionHelper.SetPropertyValue(t, objectDataList, propertyInfo);
            }
        }
        private static Dictionary<PropertyInfo, XmlTAttribute> InitXmlToEntityMapper(Type type)
        {
            string key = type.FullName;
            if (PropertyAttributeDict.ContainsKey(key)) return PropertyAttributeDict[key];

            lock (lockItem)
            {
                Dictionary<PropertyInfo, XmlTAttribute> propertyDict = new Dictionary<PropertyInfo, XmlTAttribute>();
                XmlTAttribute attribute = null;
                ReflectionHelper.Foreach((PropertyInfo propertyInfo) =>
                {
                    attribute = propertyInfo.GetCustomAttribute<XmlTAttribute>();
                    if (attribute != null)
                    {
                        propertyDict.Add(propertyInfo, attribute);
                    }
                    else
                    {
                        if (ReflectionHelper.IsListType(propertyInfo.PropertyType))
                        {
                            attribute = new XmlTAttribute(propertyInfo.Name, XmlTEnum.ElementList);
                        }
                        else if (ReflectionHelper.IsCustomType(propertyInfo.PropertyType))
                        {
                            attribute = new XmlTAttribute(propertyInfo.Name, XmlTEnum.Element);
                        }
                        else
                        {
                            attribute = new XmlTAttribute(propertyInfo.Name, XmlTEnum.Attribute);
                        }
                        propertyDict.Add(propertyInfo, attribute);
                    }
                }, type);

                if (!PropertyAttributeDict.ContainsKey(key)) PropertyAttributeDict.Add(key, propertyDict);

                return propertyDict;
            }
        }
        private static Dictionary<string, EntityToXmlMapper> InitEntityToXmlMapper<T>(T t, dynamic propertyGetDict, List<string> propertyList) where T : class
        {
            Dictionary<string, EntityToXmlMapper> mapperDict = new Dictionary<string, EntityToXmlMapper>();
            string propertyValue = null;
            XmlTAttribute attribute = null;
            ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
            {
                if (propertyList == null || propertyList.IndexOf(propertyInfo.Name) >= 0)
                {
                    if (propertyGetDict != null && propertyGetDict.ContainsKey(propertyInfo.Name))
                    {
                        propertyValue = propertyGetDict[propertyInfo.Name](t).ToString();
                    }
                    else
                    {
                        propertyValue = ReflectionHelper.GetPropertyValue(t, propertyInfo).ToString();
                    }
                    attribute = propertyInfo.GetCustomAttribute<XmlTAttribute>();
                    if (attribute != null)
                    {
                        if (!string.IsNullOrEmpty(attribute.Name))
                        {
                            mapperDict.Add(attribute.Name, new EntityToXmlMapper() { Value = propertyValue, ValueEnum = attribute.NameType });
                        }
                        else
                        {
                            mapperDict.Add(propertyInfo.Name, new EntityToXmlMapper() { Value = propertyValue, ValueEnum = attribute.NameType });
                        }
                    }
                    else
                    {
                        mapperDict.Add(propertyInfo.Name, new EntityToXmlMapper() { Value = propertyValue, ValueEnum = XmlTEnum.Attribute });
                    }
                }
            });
            return mapperDict;
        }
        private static void SetXmlNodeAttribute(XmlNode xmlNode, string attributeName, string attributeValue, XmlDocument xmlDocument, XmlTEnum xmlEnum = XmlTEnum.Attribute)
        {
            if (xmlEnum == XmlTEnum.Attribute)
            {
                if (xmlNode.Attributes[attributeName] != null)
                {
                    xmlNode.Attributes[attributeName].Value = attributeValue;
                }
                else
                {
                    XmlAttribute attribute = xmlDocument.CreateAttribute(attributeName);
                    attribute.Value = attributeValue;
                    xmlNode.Attributes.Append(attribute);
                }
            }
            else
            {
                xmlNode.InnerText = attributeValue;
            }
        }
        private static void ExecuteSetValue<T>(Action<List<string>, dynamic, XmlNode, string> callback, string xmlPath, string xPath, XmlDocument xmlDocument, string[] propertyList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            List<string> filterNameList = null;
            if (propertyList != null && propertyList.Length > 0) filterNameList = propertyList.ToList();

            dynamic propertyGetDict = null;
            if (reflectionType != ReflectionTypeEnum.Original) propertyGetDict = ReflectionExtendHelper.PropertyGetCallDict<T>(reflectionType);

            XmlNode xmlNode = GetNode(xPath, xmlDocument);
            if (xmlNode == null) return;

            string elementName = ReflectionExtendHelper.GetAttributeValue<XmlTAttribute>(typeof(T), p => p.Name);
            if (elementName == null) elementName = typeof(T).Name;

            if (callback != null) callback(filterNameList, propertyGetDict, xmlNode, elementName);

            if (!string.IsNullOrEmpty(xPath))
            {
                xmlDocument.Save(xmlPath);
            }
        }
        private static XmlElement CreateXmlElement<T>(Dictionary<string, EntityToXmlMapper> propertyMapperDict, XmlDocument xmlDocument, string elementName) where T : class
        {
            XmlElement xmlElement = xmlDocument.CreateElement(elementName);
            XmlAttribute xmlAttribute = null;
            foreach (var keyValueItem in propertyMapperDict)
            {
                if (keyValueItem.Value.ValueEnum == XmlTEnum.Attribute)
                {
                    xmlAttribute = xmlDocument.CreateAttribute(keyValueItem.Key);
                    xmlAttribute.Value = keyValueItem.Value.Value;
                    xmlElement.Attributes.Append(xmlAttribute);
                }
                else
                {
                    xmlElement.InnerText = keyValueItem.Value.Value;
                }
            }
            return xmlElement;
        }
        #endregion
    }

    #region 逻辑处理辅助特性
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class XmlTAttribute : Attribute
    {
        private string name;
        private XmlTEnum nameType;

        /// <summary>
        /// 实体属性映射 Xml 属性名称
        /// </summary>
        /// <param name="name">Xml 属性名</param>
        /// <param name="nameType">属性名类型</param>
        public XmlTAttribute(string name, XmlTEnum nameType = XmlTEnum.Attribute)
        {
            this.name = name;
            this.nameType = nameType;
        }
        /// <summary>
        /// Xml 属性值，当 Name 不为空时，对应属性数据，当 Name 为空时，需要根据 NameType 枚举类型取值
        /// </summary>
        public string Name { get { return this.name; } }
        /// <summary>
        /// Xml 数据取值枚举类型
        /// </summary>
        public XmlTEnum NameType { get { return this.nameType; } }
    }

    /// <summary>
    /// Xml 数据取值枚举类型
    /// <item itemID="1" itemName="item1">ItemText</item>
    /// </summary>
    public enum XmlTEnum
    {
        /// <summary>
        /// 对应 Xml 元素的属性数据
        /// </summary>
        Attribute = 0,
        /// <summary>
        /// 对应 Xml 元素标签之间的文本内容
        /// </summary>
        Text = 1,
        /// <summary>
        /// 对应 Xml 元素标签之间的子标签内容
        /// </summary>
        Xml = 2,
        /// <summary>
        /// 对应 Element
        /// </summary>
        Element = 3,
        /// <summary>
        /// 对应 Element 列表，只能是 IList 子类，比如：List
        /// </summary>
        ElementList = 4
    }

    internal class EntityToXmlMapper
    {
        public string Value { get; set; }
        public XmlTEnum ValueEnum { get; set; }
    }
    #endregion
}