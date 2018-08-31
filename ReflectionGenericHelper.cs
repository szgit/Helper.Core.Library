/*
 * 作用：通过表达式/Emit 反射获取/设置泛型属性数据。
 * */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Helper.Core.Library
{
    public class ReflectionGenericHelper
    {
        #region 私有属性常量
        public static readonly ReflectionGenericHelper Instance = new ReflectionGenericHelper();
        #endregion

        #region 对外公开方法

        #region 创建/属性检测
        /// <summary>
        /// 创建一个 T 对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns></returns>
        public static T New<T>() where T : class, new()
        {
            return new T();
        }
        /// <summary>
        /// 创建一个 List&lt;T&gt; 对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns></returns>
        public static IList<T> NewList<T>() where T : class
        {
            Type newListType = typeof(List<>);
            newListType = newListType.MakeGenericType(new Type[] { typeof(T) });
            return Activator.CreateInstance(newListType) as IList<T>;
        }
        /// <summary>
        /// 判断 T 类型是否是自定义类型
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns></returns>
        public static bool IsCustomType<T>() where T : class
        {
            Type type = typeof(T);
            return (type != typeof(object) && Type.GetTypeCode(type) == TypeCode.Object);
        }
        /// <summary>
        /// 判断 T 类型是否包含属性
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public static bool IsContainProperty<T>(string propertyName) where T : class
        {
            return typeof(T).GetProperty(propertyName) != null;
        }
        #endregion

        #region 属性列表获取/遍历
        /// <summary>
        /// 属性列表遍历
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="callback">属性处理函数</param>
        public static void Foreach<T>(Action<PropertyInfo> callback) where T : class
        {
            PropertyInfo[] propertyList = typeof(T).GetProperties();
            foreach (PropertyInfo propertyInfo in propertyList)
            {
                callback(propertyInfo);
            }
        }
        /// <summary>
        /// 获取 T 属性名和属性值
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="t">实体类型数据</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetPropertyDict<T>(T t) where T : class
        {
            Dictionary<string, object> resultDict = new Dictionary<string, object>();
            Foreach<T>((PropertyInfo propertyInfo) =>
            {
                resultDict.Add(propertyInfo.Name, propertyInfo.GetValue(t));
            });
            return resultDict;
        }
        #endregion

        #region 表达式或 Emit 反射获取设置属性值（泛型）
        /// <summary>
        /// T 类型索引器获取值委托
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <typeparam name="K">索引器参数类型</typeparam>
        /// <typeparam name="P">索引器返回类型</typeparam>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static Func<T, K, object> PropertyIndexGetCall<T, K, P>(ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            if(reflectionType == ReflectionTypeEnum.Expression)
            {
                return ExpressionPropertyIndexGetCall<T, K, P>();
            }
            return EmitPropertyIndexGetCall<T, K, P>();
        }
        /// <summary>
        /// T 类型属性获取值委托列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static Dictionary<string, Func<T, object>> PropertyGetCallDict<T>(ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            if(reflectionType == ReflectionTypeEnum.Expression)
            {
                return ExpressionPropertyGetCallDict<T>();
            }
            return EmitPropertyGetCallDict<T>();
        }
        /// <summary>
        /// T 类型属性获取值委托
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static Func<T, object> PropertyGetCall<T>(string propertyName, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            return PropertyGetCall<T>(typeof(T).GetProperty(propertyName), reflectionType);
        }
        /// <summary>
        /// T 类型属性获取值委托
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static Func<T, object> PropertyGetCall<T>(PropertyInfo propertyInfo, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            if(reflectionType == ReflectionTypeEnum.Expression)
            {
                return ExpressionPropertyGetCall<T>(propertyInfo);
            }
            return EmitPropertyGetCall<T>(propertyInfo);
        }
        /// <summary>
        /// T 类型属性设置值委托列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static Dictionary<string, Action<T, object>> PropertySetCallDict<T>(ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            if(reflectionType == ReflectionTypeEnum.Expression)
            {
                return ExpressionPropertySetCallDict<T>();
            }
            return EmitPropertySetCallDict<T>();
        }
        /// <summary>
        /// T 类型属性设置值委托
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static Action<T, object> PropertySetCall<T>(string propertyName, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            return PropertySetCall<T>(typeof(T).GetProperty(propertyName), reflectionType);
        }
        /// <summary>
        /// T 类型属性设置值委托
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static Action<T, object> PropertySetCall<T>(PropertyInfo propertyInfo, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            if(reflectionType == ReflectionTypeEnum.Expression)
            {
                return ExpressionPropertySetCall<T>(propertyInfo);
            }
            return EmitPropertySetCall<T>(propertyInfo);
        }
        /// <summary>
        /// 委托设置属性值
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="call">委托</param>
        /// <param name="t">实体类型数据</param>
        /// <param name="dataValue">要设置的数据</param>
        /// <param name="propertyInfo">PropertyInfo</param>
        public static void SetPropertyValue<T>(dynamic call, T t, dynamic dataValue, PropertyInfo propertyInfo) where T : class
        {
            DynamicSetPropertyValue(call, t, dataValue, propertyInfo);
        }
        #endregion

        #region 表达式或 Emit 反射获取设置属性值（Type）
        /// <summary>
        /// 属性获取值委托列表
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertyGetCallDict(Type type, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            MethodInfo method = typeof(ReflectionGenericHelper).GetMethod("PropertyGetCallDict", new Type[] { typeof(ReflectionTypeEnum) });
            MethodInfo generic = method.MakeGenericMethod(type);
            return generic.Invoke(ReflectionGenericHelper.Instance, new object[] { reflectionType });
        }
        /// <summary>
        /// 属性获取值委托
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertyGetCall(Type type, string propertyName, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            return PropertyGetCall(type, type.GetProperty(propertyName), reflectionType);
        }
        /// <summary>
        /// 属性获取值委托
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertyGetCall(Type type, PropertyInfo propertyInfo, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            MethodInfo method = typeof(ReflectionGenericHelper).GetMethod("PropertyGetCall", new Type[] { typeof(PropertyInfo), typeof(ReflectionTypeEnum) });
            MethodInfo generic = method.MakeGenericMethod(type);
            return generic.Invoke(ReflectionGenericHelper.Instance, new object[] { propertyInfo, reflectionType });
        }
        /// <summary>
        /// 属性设置值委托列表
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertySetCallDict(Type type, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            MethodInfo method = typeof(ReflectionGenericHelper).GetMethod("PropertySetCallDict", new Type[] { typeof(ReflectionTypeEnum) });
            MethodInfo generic = method.MakeGenericMethod(type);
            return generic.Invoke(ReflectionGenericHelper.Instance, new object[] { reflectionType });
        }
        /// <summary>
        /// 属性设置值委托
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertySetCall(Type type, string propertyName, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            return PropertySetCall(type, type.GetProperty(propertyName), reflectionType);
        }
        /// <summary>
        /// 属性设置值委托
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static dynamic PropertySetCall(Type type, PropertyInfo propertyInfo, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            MethodInfo method = typeof(ReflectionGenericHelper).GetMethod("PropertySetCall", new Type[] { typeof(PropertyInfo), typeof(ReflectionTypeEnum) });
            MethodInfo generic = method.MakeGenericMethod(type);
            return generic.Invoke(ReflectionGenericHelper.Instance, new object[] { propertyInfo, reflectionType });
        }
        /// <summary>
        /// 委托设置属性值
        /// </summary>
        /// <param name="call">委托</param>
        /// <param name="t">数据</param>
        /// <param name="dataValue">要设置的数据</param>
        /// <param name="propertyInfo">PropertyInfo</param>
        public static void SetPropertyValue(dynamic call, dynamic t, dynamic dataValue, PropertyInfo propertyInfo)
        {
            DynamicSetPropertyValue(call, t, dataValue, propertyInfo);
        }
        #endregion

        #endregion

        #region 逻辑处理私有方法

        #region 表达式反射获取设置属性值
        private static Func<T, K, object> ExpressionPropertyIndexGetCall<T, K, P>() where T : class
        {
            Type type = typeof(T);
            Type paraType = typeof(K);

            var instance = Expression.Parameter(type, "instance");
            var parameter = Expression.Parameter(paraType);

            var method = type.GetMethod("get_Item", new Type[] { paraType });

            Expression expression = Expression.Call(instance, method, parameter);
            return Expression.Lambda<Func<T, K, object>>(Expression.Convert(expression, typeof(object)), new ParameterExpression[] { instance, parameter }).Compile();
        }
        private static Dictionary<string, Func<T, object>> ExpressionPropertyGetCallDict<T>() where T : class
        {
            Dictionary<string, Func<T, object>> resultDict = new Dictionary<string, Func<T, object>>();
            PropertyInfo[] propertyInfoList = typeof(T).GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                resultDict.Add(propertyInfo.Name, ExpressionPropertyGetCall<T>(propertyInfo));
            }
            return resultDict;
        }
        private static Func<T, object> ExpressionPropertyGetCall<T>(PropertyInfo propertyInfo) where T : class
        {
            Type type = typeof(T);

            var instance = Expression.Parameter(type);

            var body = Expression.Convert(Expression.Property(instance, propertyInfo), typeof(object));
            return Expression.Lambda<Func<T, object>>(body, instance).Compile();
        }
        private static Dictionary<string, Action<T, object>> ExpressionPropertySetCallDict<T>() where T : class
        {
            Dictionary<string, Action<T, object>> resultDict = new Dictionary<string, Action<T, object>>();
            PropertyInfo[] propertyInfoList = typeof(T).GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                resultDict.Add(propertyInfo.Name, ExpressionPropertySetCall<T>(propertyInfo));
            }
            return resultDict;
        }
        private static Action<T, object> ExpressionPropertySetCall<T>(PropertyInfo propertyInfo) where T : class
        {
            Type type = typeof(T);

            var instance = Expression.Parameter(type, "instance");
            var parmameter = Expression.Parameter(typeof(object), "parmameter");

            var instanceType = Expression.Convert(instance, propertyInfo.ReflectedType);
            var parmameterType = Expression.Convert(parmameter, propertyInfo.PropertyType);

            var property = Expression.Property(instanceType, propertyInfo);
            var body = Expression.Assign(property, parmameterType);

            var lambda = Expression.Lambda<Action<T, object>>(body, instance, parmameter);
            return lambda.Compile();
        }
        #endregion

        #region Emit 反射获取/设置属性值
        private static Func<T, K, object> EmitPropertyIndexGetCall<T, K, P>() where T : class
        {
            var type = typeof(T);
            Type paraType = typeof(K);
            Type returnType = typeof(P);

            var dynamicMethod = new DynamicMethod("get_Item", typeof(object), new[] { type, paraType }, type);

            var iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);

            var indexMethod = type.GetMethod("get_Item", new Type[] { paraType });
            iLGenerator.Emit(OpCodes.Callvirt, indexMethod);

            if (returnType.IsValueType)
            {
                // 如果是值类型，装箱
                iLGenerator.Emit(OpCodes.Box, returnType);
            }
            else
            {
                // 如果是引用类型，转换
                iLGenerator.Emit(OpCodes.Castclass, returnType);
            }

            iLGenerator.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate(typeof(Func<T, K, object>)) as Func<T, K, object>;
        }
        private static Dictionary<string, Func<T, object>> EmitPropertyGetCallDict<T>() where T : class
        {
            Dictionary<string, Func<T, object>> resultDict = new Dictionary<string, Func<T, object>>();
            PropertyInfo[] propertyInfoList = typeof(T).GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                resultDict.Add(propertyInfo.Name, EmitPropertyGetCall<T>(propertyInfo));
            }
            return resultDict;
        }
        private static Func<T, object> EmitPropertyGetCall<T>(PropertyInfo propertyInfo) where T : class
        {
            var type = typeof(T);

            var dynamicMethod = new DynamicMethod("get_" + propertyInfo.Name, typeof(object), new[] { type }, type);
            var iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);

            iLGenerator.Emit(OpCodes.Callvirt, propertyInfo.GetMethod);

            if (propertyInfo.PropertyType.IsValueType)
            {
                // 如果是值类型，装箱
                iLGenerator.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }
            else
            {
                // 如果是引用类型，转换
                iLGenerator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            }

            iLGenerator.Emit(OpCodes.Ret);
            return dynamicMethod.CreateDelegate(typeof(Func<T, object>)) as Func<T, object>;
        }
        private static Dictionary<string, Action<T, object>> EmitPropertySetCallDict<T>() where T : class
        {
            Dictionary<string, Action<T, object>> resultDict = new Dictionary<string, Action<T, object>>();
            PropertyInfo[] propertyInfoList = typeof(T).GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                resultDict.Add(propertyInfo.Name, EmitPropertySetCall<T>(propertyInfo));
            }
            return resultDict;
        }
        private static Action<T, object> EmitPropertySetCall<T>(PropertyInfo propertyInfo) where T : class
        {
            var type = typeof(T);

            var dynamicMethod = new DynamicMethod("EmitCallable", null, new[] { type, typeof(object) }, type.Module);
            var iLGenerator = dynamicMethod.GetILGenerator();

            var callMethod = type.GetMethod("set_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            var parameterInfo = callMethod.GetParameters()[0];
            var local = iLGenerator.DeclareLocal(parameterInfo.ParameterType, true);

            iLGenerator.Emit(OpCodes.Ldarg_1);
            if (parameterInfo.ParameterType.IsValueType)
            {
                // 如果是值类型，拆箱
                iLGenerator.Emit(OpCodes.Unbox_Any, parameterInfo.ParameterType);
            }
            else
            {
                // 如果是引用类型，转换
                iLGenerator.Emit(OpCodes.Castclass, parameterInfo.ParameterType);
            }

            iLGenerator.Emit(OpCodes.Stloc, local);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldloc, local);

            iLGenerator.EmitCall(OpCodes.Callvirt, callMethod, null);
            iLGenerator.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate(typeof(Action<T, object>)) as Action<T, object>;
        }
        #endregion

        private static void DynamicSetPropertyValue(dynamic call, dynamic t, dynamic dataValue, PropertyInfo propertyInfo)
        {
            if (dataValue == null) return;
            if (dataValue.GetType() == typeof(string))
            {
                if (propertyInfo.PropertyType == typeof(int))
                {
                    call(t, int.Parse(dataValue));
                }
                else if (propertyInfo.PropertyType == typeof(string))
                {
                    call(t, dataValue);
                }
                else if (propertyInfo.PropertyType == typeof(float))
                {
                    call(t, float.Parse(dataValue));
                }
                else if (propertyInfo.PropertyType == typeof(DateTime))
                {
                    if (!string.IsNullOrEmpty(dataValue))
                    {
                        call(t, DateTime.Parse(dataValue));
                    }
                }
            }
            else
            {
                call(t, dataValue);
            }
        }

        #endregion
    }
}
