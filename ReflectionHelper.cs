/*
 * 作用：反射获取/设置属性数据。
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    public enum ReflectionTypeEnum : int
    {
        /// <summary>
        /// 原始反射
        /// </summary>
        Original = 1,
        /// <summary>
        /// 表达式反射
        /// </summary>
        Expression = 2,
        /// <summary>
        /// Emit 反射
        /// </summary>
        Emit = 3
    }
    #endregion

    public class ReflectionHelper
    {
        #region 对外公开方法

        #region 创建/属性检测
        /// <summary>
        /// 根据 Type 创建实例
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public static object New(Type type)
        {
            return Activator.CreateInstance(type, true);
        }
        /// <summary>
        /// 根据 Type 创建 IList 实例
        /// </summary>
        /// <param name="itemType">Type</param>
        /// <returns></returns>
        public static IList NewList(Type itemType)
        {
            Type newListType = typeof(List<>);
            newListType = newListType.MakeGenericType(new Type[] { itemType });
            return Activator.CreateInstance(newListType) as IList;
        }
        /// <summary>
        /// 判断 Type 是否是自定义类型
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public static bool IsCustomType(Type type)
        {
            return (type != typeof(object) && Type.GetTypeCode(type) == TypeCode.Object);
        }
        /// <summary>
        /// 判断 Type 是否 IList 类型
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public static bool IsListType(Type type)
        {
            return (type.GetInterface("IList") != null);
        }
        /// <summary>
        /// 判断 Type 是否包含属性
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public static bool IsContainProperty(Type type, string propertyName)
        {
            return type.GetProperty(propertyName) != null;
        }
        /// <summary>
        /// 获得 List&lt;T&gt; 中 T 的类型
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <returns></returns>
        public static Type GetListGenericType(PropertyInfo propertyInfo)
        {
            PropertyInfo[] childPropertyList = propertyInfo.PropertyType.GetProperties();
            foreach (PropertyInfo childPropertyInfo in childPropertyList)
            {
                if (childPropertyInfo.Name == "Item") return childPropertyInfo.PropertyType;
            }
            return null;
        }
        /// <summary>
        /// 获取泛型对象的 T 类型
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public static Type GetGenericType(Type type)
        {
            Type itemType = type.GetGenericArguments()[0];
            return itemType;
        }
        #endregion

        #region 属性列表获取/遍历
        /// <summary>
        /// 属性列表遍历
        /// </summary>
        /// <param name="callback">属性处理函数</param>
        /// <param name="type">Type</param>
        public static void Foreach(Action<PropertyInfo> callback, Type type)
        {
            PropertyInfo[] propertyList = type.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyList)
            {
                callback(propertyInfo);
            }
        }
        /// <summary>
        /// 获取属性名和属性值
        /// </summary>
        /// <param name="data">实体数据</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetPropertyDict(object data)
        {
            if (data == null) return null;

            Dictionary<string, object> resultDict = new Dictionary<string, object>();
            Foreach((PropertyInfo propertyInfo) =>
            {
                resultDict.Add(propertyInfo.Name, propertyInfo.GetValue(data));
            }, data.GetType());

            return resultDict;
        }
        #endregion

        #region 原始反射获取设置属性值
        /// <summary>
        /// 获取索引值
        /// </summary>
        /// <param name="data">实体数据</param>
        /// <param name="indexName">索引器名称</param>
        /// <returns></returns>
        public static object GetPropertyIndexValue(object data, object indexName)
        {
            var method = data.GetType().GetMethod("get_Item", new Type[] { indexName.GetType() });
            return method.Invoke(data, new object[] { indexName });
        }
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="data">实体数据</param>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <returns></returns>
        public static object GetPropertyValue(object data, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) return null;
            return propertyInfo.GetValue(data, null);
        }
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="data">实体数据</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public static object GetPropertyValue(object data, string propertyName)
        {
            if (data == null) return null;
            return GetPropertyValue(data, data.GetType().GetProperty(propertyName));
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="data">实体数据</param>
        /// <param name="propertyValue">要设置的数据</param>
        /// <param name="propertyName">属性名称</param>
        public static void SetPropertyValue(object data, object propertyValue, string propertyName)
        {
            SetPropertyValue(data, propertyValue, data.GetType().GetProperty(propertyName));
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="data">实体数据</param>
        /// <param name="propertyValue">要设置的数据</param>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <returns></returns>
        public static void SetPropertyValue(object data, object propertyValue, PropertyInfo propertyInfo)
        {
            if (propertyValue == null) return;

            if (propertyValue.GetType() == typeof(string))
            {
                if (propertyInfo.PropertyType == typeof(int))
                {
                    data.GetType().GetProperty(propertyInfo.Name).SetValue(data, int.Parse(propertyValue.ToString()), null);
                }
                else if (propertyInfo.PropertyType == typeof(string))
                {
                    data.GetType().GetProperty(propertyInfo.Name).SetValue(data, propertyValue, null);
                }
                else if (propertyInfo.PropertyType == typeof(float))
                {
                    data.GetType().GetProperty(propertyInfo.Name).SetValue(data, float.Parse(propertyValue.ToString()), null);
                }
                else if (propertyInfo.PropertyType == typeof(DateTime))
                {
                    data.GetType().GetProperty(propertyInfo.Name).SetValue(data, DateTime.Parse(propertyValue.ToString()), null);
                }
            }
            else
            {
                data.GetType().GetProperty(propertyInfo.Name).SetValue(data, propertyValue, null);
            }
        }
        #endregion

        #endregion
    }
}
