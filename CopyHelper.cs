/*
 * 作用：实体属性值之前相互拷贝。
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Helper.Core.Library
{
    public class CopyHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 复制数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="originalData">原实体数据</param>
        /// <param name="propertyMatchList">属性匹配，例：new { ID = "UserID" }</param>
        /// <param name="propertyIgnore">忽略属性列表，例：new { ID = "" }</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static T Copy<T>(object originalData, object propertyMatchList = null, object propertyIgnore = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            Dictionary<PropertyInfo, string> propertyMatchDict = InitPropertyMatchMapper(originalData.GetType(), typeof(T), propertyMatchList, propertyIgnore);

            dynamic propertyGetDict = ReflectionExtendHelper.PropertyGetCallDict(originalData.GetType(), reflectionType);
            dynamic propertySetDict = ReflectionExtendHelper.PropertySetCallDict<T>(reflectionType);

            return CopyData(typeof(T), propertySetDict, propertyGetDict, propertyMatchDict, originalData, propertyMatchList, propertyIgnore, reflectionType) as T;
        }
        /// <summary>
        /// 复制数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="K">拥有索引器的实体类型</typeparam>
        /// <typeparam name="P">索引器参数类型，例：int</typeparam>
        /// <typeparam name="R">索引器返回类型，例：string</typeparam>
        /// <param name="originalData">原实体数据</param>
        /// <param name="propertyMatchList">属性匹配，例：new { ID = "UserID" }</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static T Copy<T, K, P, R>(K originalData, object propertyMatchList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
            where T : class, new()
            where K : class
        {
            T t = ReflectionGenericHelper.New<T>();

            Func<K, P, object> indexCall = ReflectionGenericHelper.PropertyIndexGetCall<K, P, R>(reflectionType);
            Dictionary<PropertyInfo, string> propertyMatchDict = InitPropertyMatchMapper(originalData.GetType(), typeof(T), propertyMatchList, null);
            foreach (var keyValueItem in propertyMatchDict)
            {
                P indexName = (P)Convert.ChangeType(keyValueItem.Value, typeof(P), CultureInfo.InvariantCulture);
                ReflectionHelper.SetPropertyValue(t, indexCall(originalData, indexName).ToString(), keyValueItem.Key);
            }
            return t;
        }
        /// <summary>
        /// 复制数据
        /// </summary>
        /// <param name="originalData">原实体数据</param>
        /// <param name="targetData">目标实体数据</param>
        /// <param name="propertyMatchList">属性匹配，例：new { ID = "UserID" }</param>
        /// <param name="propertyIgnore">忽略属性列表，例：new { ID = "" }</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic Copy(object originalData, object targetData, object propertyMatchList = null, object propertyIgnore = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            Dictionary<PropertyInfo, string> propertyMatchDict = InitPropertyMatchMapper(originalData.GetType(), targetData.GetType(), propertyMatchList, propertyIgnore);

            dynamic propertyGetDict = ReflectionExtendHelper.PropertyGetCallDict(originalData.GetType(), reflectionType);
            dynamic propertySetDict = ReflectionExtendHelper.PropertySetCallDict(targetData.GetType(), reflectionType);

            object copyData = CopyData(targetData, propertySetDict, propertyGetDict, propertyMatchDict, originalData, propertyMatchList, propertyIgnore, reflectionType);
            return Convert.ChangeType(copyData, targetData.GetType());
        }
        #endregion

        #region 逻辑处理私有方法
        private static Dictionary<PropertyInfo, string> InitPropertyMatchMapper(Type sourceType, Type type, object propertyMatchList = null, object propertyIgnore = null)
        {
            Dictionary<PropertyInfo, string> resultList = new Dictionary<PropertyInfo, string>();

            string matchPropertyName = null;
            ReflectionHelper.Foreach((PropertyInfo propertyInfo) =>
            {
                if (propertyIgnore == null || !ReflectionHelper.IsContainProperty(propertyIgnore.GetType(), propertyInfo.Name))
                {
                    var propertyValue = ReflectionHelper.GetPropertyValue(propertyMatchList, propertyInfo.Name);
                    if (propertyValue != null)
                    {
                        if (ReflectionHelper.IsCustomType(propertyValue.GetType()))
                        {
                            var namePropertyValue = ReflectionHelper.GetPropertyValue(propertyValue, "___Name");
                            if (namePropertyValue != null)
                            {
                                matchPropertyName = namePropertyValue.ToString();
                            }
                            else
                            {
                                matchPropertyName = propertyInfo.Name;
                            }
                        }
                        else
                        {
                            matchPropertyName = propertyValue.ToString();
                        }
                    }
                    else
                    {
                        matchPropertyName = propertyInfo.Name;
                    }
                    if (matchPropertyName != null) resultList.Add(propertyInfo, matchPropertyName);
                }
            }, type);

            return resultList;
        }
        private static object CopyData(Type type, dynamic propertySetDict, dynamic propertyGetDict, Dictionary<PropertyInfo, string> propertyMatchDict, object originalData, object propertyMatch = null, object propertyIgnore = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            return CopyData(ReflectionHelper.New(type), propertySetDict, propertyGetDict, propertyMatchDict, originalData, propertyMatch, propertyIgnore, reflectionType);
        }
        private static object CopyData(object targetData, dynamic propertySetDict, dynamic propertyGetDict, Dictionary<PropertyInfo, string> propertyMatchDict, dynamic originalData, object propertyMatch = null, object propertyIgnore = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            if (originalData == null) return targetData;

            PropertyInfo propertyInfo = null;
            dynamic propertyValue = null;

            foreach (var keyValueItem in propertyMatchDict)
            {
                propertyInfo = keyValueItem.Key;
                if (propertyGetDict != null && propertyGetDict.ContainsKey(keyValueItem.Value))
                {
                    propertyValue = propertyGetDict[keyValueItem.Value](originalData);
                }
                else
                {
                    propertyValue = ReflectionHelper.GetPropertyValue(originalData, keyValueItem.Value);
                }
                if (propertyValue != null)
                {
                    if (ReflectionHelper.IsListType(propertyInfo.PropertyType))
                    {
                        Type originalTType = ReflectionHelper.GetListGenericType(originalData.GetType().GetProperty(keyValueItem.Value));
                        var originalPropertyMatchValue = ReflectionHelper.GetPropertyValue(propertyMatch, propertyInfo.Name);
                        var originalPropertyIgnoreValue = ReflectionHelper.GetPropertyValue(propertyIgnore, propertyInfo.Name);
                        var originalPropertyValue = ReflectionHelper.GetPropertyValue(targetData, propertyInfo) as IList;

                        Type listTType = ReflectionHelper.GetListGenericType(propertyInfo);
                        var objectDataList = ReflectionHelper.NewList(listTType) as IList;

                        dynamic listPropertySetDict = null;
                        if (reflectionType != ReflectionTypeEnum.Original) listPropertySetDict = ReflectionExtendHelper.PropertySetCallDict(listTType, reflectionType);

                        dynamic listPropertyGetDict = null;
                        if (reflectionType != ReflectionTypeEnum.Original) listPropertyGetDict = ReflectionExtendHelper.PropertyGetCallDict(originalTType, reflectionType);

                        for (int index = 0; index < propertyValue.Count; index++)
                        {
                            if (originalPropertyValue != null && index < originalPropertyValue.Count)
                            {
                                objectDataList.Add(CopyData(originalPropertyValue[index], listPropertySetDict, listPropertyGetDict, InitPropertyMatchMapper(originalTType, listTType, originalPropertyMatchValue, originalPropertyIgnoreValue), propertyValue[index], originalPropertyMatchValue, originalPropertyIgnoreValue, reflectionType));
                            }
                            else
                            {
                                objectDataList.Add(CopyData(listTType, listPropertySetDict, listPropertyGetDict, InitPropertyMatchMapper(originalTType, listTType, originalPropertyMatchValue, originalPropertyIgnoreValue), propertyValue[index], originalPropertyMatchValue, originalPropertyIgnoreValue, reflectionType));
                            }
                        }
                        targetData.GetType().GetProperty(propertyInfo.Name).SetValue(targetData, objectDataList, null);
                    }
                    else if (ReflectionHelper.IsCustomType(propertyInfo.PropertyType))
                    {
                        var originalPropertyMatchValue = ReflectionHelper.GetPropertyValue(propertyMatch, propertyInfo.Name);
                        var originalPropertyIgnoreValue = ReflectionHelper.GetPropertyValue(propertyIgnore, propertyInfo.Name);
                        var originalPropertyValue = ReflectionHelper.GetPropertyValue(targetData, propertyInfo);

                        if (originalPropertyValue == null)
                        {
                            targetData.GetType().GetProperty(propertyInfo.Name).SetValue(targetData, CopyData(propertyInfo.PropertyType, null, null, InitPropertyMatchMapper(propertyValue.GetType(), propertyInfo.PropertyType, originalPropertyMatchValue, originalPropertyIgnoreValue), propertyValue, originalPropertyMatchValue, originalPropertyIgnoreValue, reflectionType), null);
                        }
                        else
                        {
                            targetData.GetType().GetProperty(propertyInfo.Name).SetValue(targetData, CopyData(originalPropertyValue, null, null, InitPropertyMatchMapper(propertyValue.GetType(), propertyInfo.PropertyType, originalPropertyMatchValue, originalPropertyIgnoreValue), propertyValue, originalPropertyMatchValue, originalPropertyIgnoreValue, reflectionType), null);
                        }
                    }
                    else
                    {
                        if (propertySetDict != null && propertySetDict.ContainsKey(propertyInfo.Name))
                        {
                            ReflectionGenericHelper.SetPropertyValue(propertySetDict[propertyInfo.Name], targetData, propertyValue.ToString(), propertyInfo);
                        }
                        else
                        {
                            ReflectionHelper.SetPropertyValue(targetData, propertyValue.ToString(), propertyInfo);
                        }
                    }
                }
            }
            return targetData;
        }
        #endregion
    }
}
