/*
 * 作用：通过缓存优化反射获取类/属性值特性数据。
 * */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Helper.Core.Library
{
    public class ReflectionExtendHelper
    {
        #region 私有属性常量

        private const string TYPE_ATTRIBUTE_FORMAT = "{0}_{1}";
        private const string PROPERTY_ATTRIBUTE_FORMAT = "{0}_{1}_{2}";
        private const string GET_TYPE_CALL_FORMAT = "Get_{0}_{1}";
        private const string SET_TYPE_CALL_FORMAT = "Set_{0}_{1}";
        private const string GET_NAME_CALL_FORMAT = "Get_{0}_{1}_{2}";
        private const string SET_NAME_CALL_FORMAT = "Set_{0}_{1}_{2}";

        /// <summary>
        /// Lock 对象
        /// </summary>
        private static readonly object lockItem = new object();
        /// <summary>
        /// Type 特性集合
        /// </summary>
        private static readonly Dictionary<string, dynamic> TypeAttributeDict = new Dictionary<string, dynamic>();
        /// <summary>
        /// 属性特性集合
        /// </summary>
        private static readonly Dictionary<string, dynamic> PropertyAttributeDict = new Dictionary<string, dynamic>();
        /// <summary>
        /// Type 属性委托集合
        /// </summary>
        private static readonly Dictionary<string, dynamic> PropertyTypeCallDict = new Dictionary<string, dynamic>();
        /// <summary>
        /// 属性委托集合
        /// </summary>
        private static readonly Dictionary<string, dynamic> PropertyCallDict = new Dictionary<string, dynamic>();
        #endregion

        #region 对外公开方法

        #region 获取特性
        /// <summary>
        /// 获取类的特性数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="type">Type</param>
        /// <param name="lambda">特性属性值，例：p=>p.Name</param>
        /// <returns></returns>
        public static dynamic GetAttributeValue<T>(Type type, Func<T, object> lambda) where T : Attribute
        {
            string key = string.Format(TYPE_ATTRIBUTE_FORMAT, type.FullName, typeof(T).Name);
            if (TypeAttributeDict.ContainsKey(key))
            {
                T attribute = TypeAttributeDict[key];
                if(attribute != null) return lambda(attribute);
                return null;
            }
            lock (lockItem)
            {
                T attribute = type.GetCustomAttribute<T>();
                if (!TypeAttributeDict.ContainsKey(key))
                {
                    TypeAttributeDict.Add(key, attribute);
                    if(attribute != null) return lambda(attribute);
                }
                return null;
            }
        }
        /// <summary>
        /// 获取类属性的特性数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="type">Type</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="lambda">特性属性值，例：p=>p.Name</param>
        /// <returns></returns>
        public static dynamic GetAttributeValue<T>(Type type, string propertyName, Func<T, object> lambda) where T : Attribute
        {
            return GetAttributeValue<T>(type, type.GetProperty(propertyName), lambda);
        }
        /// <summary>
        /// 获取类属性的特性数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="type">Type</param>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="lambda">特性属性值，例：p=>p.Name</param>
        /// <returns></returns>
        public static dynamic GetAttributeValue<T>(Type type, PropertyInfo propertyInfo, Func<T, object> lambda) where T : Attribute
        {
            string key = string.Format(PROPERTY_ATTRIBUTE_FORMAT, type.FullName, propertyInfo.Name, typeof(T).Name);
            if (PropertyAttributeDict.ContainsKey(key))
            {
                T attribute = PropertyAttributeDict[key];
                if (attribute != null) return lambda(attribute);
                return null;
            }
            lock (lockItem)
            {
                T attribute = propertyInfo.GetCustomAttribute<T>();
                if (!PropertyAttributeDict.ContainsKey(key))
                {
                    PropertyAttributeDict.Add(key, attribute);
                    if(attribute != null) return lambda(attribute);
                }
                return null;
            }
        }
        #endregion

        #region 获取委托
        /// <summary>
        /// 获取类属性的获取值委托
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertyGetCall<T>(string propertyName, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Type type = typeof(T);

            return ExecutePropertyGetCall(type, propertyName, () =>
            {
                return ReflectionGenericHelper.PropertyGetCall<T>(type.GetProperty(propertyName), reflectionType);
            }, reflectionType);
        }
        /// <summary>
        /// 获取类属性的获取值委托
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertyGetCall<T>(PropertyInfo propertyInfo, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Type type = typeof(T);

            return ExecutePropertyGetCall(type, propertyInfo.Name, () =>
            {
                return ReflectionGenericHelper.PropertyGetCall<T>(propertyInfo, reflectionType);
            }, reflectionType);
        }
        /// <summary>
        /// 获取类属性的获取值委托
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertyGetCall(Type type, string propertyName, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            return ExecutePropertyGetCall(type, propertyName, () =>
            {
                return ReflectionGenericHelper.PropertyGetCall(type, type.GetProperty(propertyName), reflectionType);
            }, reflectionType);
        }
        /// <summary>
        /// 获取类属性的获取值委托
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertyGetCall(Type type, PropertyInfo propertyInfo, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            return ExecutePropertyGetCall(type, propertyInfo.Name, () =>
            {
                return ReflectionGenericHelper.PropertyGetCall(type, propertyInfo, reflectionType);
            }, reflectionType);
        }
        /// <summary>
        /// 获取类属性的设置值委托
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertySetCall<T>(string propertyName, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Type type = typeof(T);

            return ExecutePropertySetCall(type, propertyName, () =>
            {
                return ReflectionGenericHelper.PropertySetCall<T>(type.GetProperty(propertyName), reflectionType);
            }, reflectionType);
        }
        /// <summary>
        /// 获取类属性的设置值委托
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertySetCall<T>(PropertyInfo propertyInfo, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Type type = typeof(T);
            return ExecutePropertySetCall(type, propertyInfo.Name, () =>
            {
                return ReflectionGenericHelper.PropertySetCall<T>(propertyInfo, reflectionType);
            }, reflectionType);
        }
        /// <summary>
        /// 获取类属性的设置值委托
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertySetCall(Type type, string propertyName, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            return ExecutePropertySetCall(type, propertyName, () =>
            {
                return ReflectionGenericHelper.PropertySetCall(type, type.GetProperty(propertyName), reflectionType);
            }, reflectionType);
        }
        /// <summary>
        /// 获取类属性的设置值委托
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertySetCall(Type type, PropertyInfo propertyInfo, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            return ExecutePropertySetCall(type, propertyInfo.Name, () =>
            {
                return ReflectionGenericHelper.PropertySetCall(type, propertyInfo, reflectionType);
            }, reflectionType);
        }
        /// <summary>
        /// 获取类属性的获取值委托列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertyGetCallDict<T>(ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Type type = typeof(T);

            return ExecutePropertyGetCall(type, () =>
            {
                return ReflectionGenericHelper.PropertyGetCallDict<T>(reflectionType);
            }, reflectionType);
        }
        /// <summary>
        /// 获取类属性的获取值委托列表
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertyGetCallDict(Type type, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            return ExecutePropertyGetCall(type, () =>
            {
                return ReflectionGenericHelper.PropertyGetCallDict(type, reflectionType);
            }, reflectionType);
        }
        /// <summary>
        /// 获取类属性的设置值委托列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertySetCallDict<T>(ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Type type = typeof(T);

            return ExecutePropertySetCall(type, () =>
            {
                return ReflectionGenericHelper.PropertySetCallDict<T>(reflectionType);
            }, reflectionType);
        }
        /// <summary>
        /// 获取类属性的设置值委托列表
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertySetCallDict(Type type, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            return ExecutePropertySetCall(type, () =>
            {
                return ReflectionGenericHelper.PropertySetCallDict(type, reflectionType);
            }, reflectionType);
        }
        #endregion

        #endregion

        #region 逻辑处理私有函数
        private static dynamic ExecutePropertyGetCall(Type type, Func<dynamic> callback, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            string key = string.Format(GET_TYPE_CALL_FORMAT, type.FullName, reflectionType.ToString());
            if (PropertyTypeCallDict.ContainsKey(key)) return PropertyTypeCallDict[key];

            lock (lockItem)
            {
                dynamic callDict = callback();
                if (!PropertyTypeCallDict.ContainsKey(key)) PropertyTypeCallDict.Add(key, callDict);
                return callDict;
            }
        }
        private static dynamic ExecutePropertyGetCall(Type type, string propertyName, Func<dynamic> callback, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            string key = string.Format(GET_NAME_CALL_FORMAT, type.FullName, propertyName, reflectionType.ToString());
            if (PropertyCallDict.ContainsKey(key)) return PropertyCallDict[key];

            lock (lockItem)
            {
                dynamic call = callback();
                if (!PropertyCallDict.ContainsKey(key)) PropertyCallDict.Add(key, call);
                return call;
            }
        }
        private static dynamic ExecutePropertySetCall(Type type, Func<dynamic> callback, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            string key = string.Format(SET_TYPE_CALL_FORMAT, type.FullName, reflectionType.ToString());
            if (PropertyTypeCallDict.ContainsKey(key)) return PropertyTypeCallDict[key];

            lock (lockItem)
            {
                dynamic callDict = callback();
                if (!PropertyTypeCallDict.ContainsKey(key)) PropertyTypeCallDict.Add(key, callDict);
                return callDict;
            }
        }
        private static dynamic ExecutePropertySetCall(Type type, string propertyName, Func<dynamic> callback, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            string key = string.Format(SET_NAME_CALL_FORMAT, type.FullName, propertyName, reflectionType.ToString());
            if (PropertyCallDict.ContainsKey(key)) return PropertyCallDict[key];

            lock (lockItem)
            {
                dynamic call = callback();
                if (!PropertyCallDict.ContainsKey(key)) PropertyCallDict.Add(key, call);
                return call;
            }
        }
        #endregion
    }
}