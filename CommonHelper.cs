using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library
{
    internal class CommonHelper
    {
        /// <summary>
        /// 实体写属性映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="propertyMatch"></param>
        /// <param name="filterNameList"></param>
        /// <returns></returns>
        internal static Dictionary<string, string> InitPropertyWriteMapper<T, K>(Dictionary<string, object> propertyDict = null, List<string> filterNameList = null, bool filterPropertyContain = true)
            where T : class
            where K : BaseReadAndWriteTAttribute
        {
            Dictionary<string, string> resultDict = new Dictionary<string, string>();
            ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
            {
                if (filterNameList == null || (filterPropertyContain && filterNameList.IndexOf(propertyInfo.Name) >= 0) || (!filterPropertyContain && filterNameList.IndexOf(propertyInfo.Name) < 0))
                {
                    string attributeName = null;
                    if (propertyDict != null && propertyDict.ContainsKey(propertyInfo.Name))
                    {
                        attributeName = propertyDict[propertyInfo.Name].ToString();
                    }
                    else
                    {
                        K k = propertyInfo.GetCustomAttribute<K>();
                        if (k != null)
                        {
                            if (k.Type == AttributeReadAndWriteTypeEnum.ReadAndWrite || k.Type == AttributeReadAndWriteTypeEnum.Write)
                            {
                                if (!string.IsNullOrEmpty(k.Name))
                                {
                                    attributeName = k.Name;
                                }
                                else
                                {
                                    attributeName = propertyInfo.Name;
                                }
                            }
                        }
                        else
                        {
                            attributeName = propertyInfo.Name;
                        }
                    }
                    if (!string.IsNullOrEmpty(attributeName)) resultDict.Add(attributeName, propertyInfo.Name);
                }
            });
            return resultDict;
        }

        internal static Dictionary<string, PropertyInfo> InitPropertyWriteMapperFormat<T, K>(Dictionary<string, object> propertyDict = null, List<string> filterNameList = null, bool filterPropertyContain = true)
            where T : class
            where K : BaseReadAndWriteTAttribute
        {
            Dictionary<string, PropertyInfo> resultDict = new Dictionary<string, PropertyInfo>();
            ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
            {
                if (filterNameList == null || (filterPropertyContain && filterNameList.IndexOf(propertyInfo.Name) >= 0) || (!filterPropertyContain && filterNameList.IndexOf(propertyInfo.Name) < 0))
                {
                    string attributeName = null;
                    if (propertyDict != null && propertyDict.ContainsKey(propertyInfo.Name))
                    {
                        attributeName = propertyDict[propertyInfo.Name].ToString();
                    }
                    else
                    {
                        K k = propertyInfo.GetCustomAttribute<K>();
                        if (k != null)
                        {
                            if (k.Type == AttributeReadAndWriteTypeEnum.ReadAndWrite || k.Type == AttributeReadAndWriteTypeEnum.Write)
                            {
                                if (!string.IsNullOrEmpty(k.Name))
                                {
                                    attributeName = k.Name;
                                }
                                else
                                {
                                    attributeName = propertyInfo.Name;
                                }
                            }
                        }
                        else
                        {
                            attributeName = propertyInfo.Name;
                        }
                    }
                    if (!string.IsNullOrEmpty(attributeName)) resultDict.Add(attributeName, propertyInfo);
                }
            });
            return resultDict;
        }

        internal static Dictionary<PropertyInfo, string> InitPropertyReadMapper<T, K>(Dictionary<string, object> propertyDict, Func<string, bool> callback)
            where T : class
            where K : BaseReadAndWriteTAttribute
        {
            Dictionary<PropertyInfo, string> resultDict = new Dictionary<PropertyInfo, string>();
            ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
            {
                string mapperName = null;
                if (propertyDict != null && propertyDict.ContainsKey(propertyInfo.Name))
                {
                    mapperName = propertyDict[propertyInfo.Name].ToString();
                }
                else
                {
                    K attribute = propertyInfo.GetCustomAttribute<K>();
                    if (attribute != null)
                    {
                        if (attribute.Type == AttributeReadAndWriteTypeEnum.ReadAndWrite || attribute.Type == AttributeReadAndWriteTypeEnum.Read)
                        {
                            if (!string.IsNullOrEmpty(attribute.Name))
                            {
                                mapperName = attribute.Name;
                            }
                            else
                            {
                                mapperName = propertyInfo.Name;
                            }
                        }
                    }
                    else
                    {
                        mapperName = propertyInfo.Name;
                    }
                }
                if (mapperName != null && callback(mapperName)) resultDict.Add(propertyInfo, mapperName);
            });
            return resultDict;
        }

        internal static Dictionary<string, object> GetExpressionDict<T>(Expression<Func<T, object>> expression) where T : class
        {
            if (expression == null) return null;

            Dictionary<string, object> dict = new Dictionary<string, object>();

            dynamic body = expression.Body;
            var argumentList = body.Arguments;
            var memberList = body.Members;

            int count = argumentList.Count;
            for (int index = 0; index < count; index++)
            {
                var nodeType = argumentList[index].NodeType;
                string argumentName = null;
                if (nodeType == ExpressionType.Constant)
                {
                    argumentName = argumentList[index].Value;
                }
                else if (nodeType == ExpressionType.MemberAccess)
                {
                    argumentName = argumentList[index].Member.Name;
                }
                string memberName = memberList[index].Name;
                if (!string.IsNullOrEmpty(argumentName))
                {
                    dict.Add(argumentName, memberName);
                }
            }

            return dict;
        }

        internal static List<string> GetExpressionList<T>(Expression<Func<T, object>> expression) where T : class
        {
            if (expression == null) return null;

            List<string> dataList = new List<string>();

            dynamic body = expression.Body;
            var argumentList = body.Arguments;

            int count = argumentList.Count;
            for (int index = 0; index < count; index++)
            {
                var nodeType = argumentList[index].NodeType;
                string argumentName = null;
                if (nodeType == ExpressionType.Constant)
                {
                    argumentName = argumentList[index].Value;
                }
                else if (nodeType == ExpressionType.MemberAccess)
                {
                    argumentName = argumentList[index].Member.Name;
                }
                dataList.Add(argumentName);
            }

            return dataList;
        }

        internal static Dictionary<string, object> GetParameterDict(object param)
        {
            Dictionary<string, object> parameterDict = null;
            if (param != null)
            {
                if (param is Dictionary<string, object>)
                {
                    parameterDict = param as Dictionary<string, object>;
                }
                else
                {
                    parameterDict = ReflectionHelper.GetPropertyDict(param);
                }
            }
            return parameterDict;
        }
    }

    #region Attribute 基类

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class BaseTAttribute : Attribute
    {
        private string name;

        public BaseTAttribute(string name)
        {
            this.name = name;
        }

        public string Name { get { return this.name; } }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class BaseReadAndWriteTAttribute : Attribute
    {
        private string name;
        private AttributeReadAndWriteTypeEnum type;

        public BaseReadAndWriteTAttribute(string name, AttributeReadAndWriteTypeEnum type = AttributeReadAndWriteTypeEnum.ReadAndWrite)
        {
            this.name = name;
            this.type = type;
        }

        public string Name { get { return this.name; } }

        public AttributeReadAndWriteTypeEnum Type { get { return this.type; } }
    }
    public enum AttributeReadAndWriteTypeEnum
    {
        ReadAndWrite = 0,
        Read = 1,
        Write = 2
    }
    #endregion
}
