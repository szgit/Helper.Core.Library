/*
 * 作用：DataTable 与实体数据列表相互转换，是对 DataTableHelper 的扩展，参数使用表达式，目的是减少属性名的拼写错误。
 * */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Helper.Core.Library
{
    public class DataTableExpressionHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 返回 DataTable 数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataList">实体类型数据列表</param>
        /// <param name="propertyMatchExpression">属性匹配，例：p=> new { ID = p.UserID }</param>
        /// <param name="propertyExpression">属性列表，如果指定，则按指定属性列表 DataTable 数据，例：p=> new { p.UserID }</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(List<T> dataList, Expression<Func<T, object>> propertyMatchExpression, Expression<Func<T, object>> propertyExpression = null, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Dictionary<string, object> propertyDict = CommonHelper.GetExpressionDict<T>(propertyMatchExpression);
            List<string> propertyNameList = CommonHelper.GetExpressionList<T>(propertyExpression);

            return DataTableHelper.ExecuteToDataTable<T>(dataList, propertyDict, propertyNameList, propertyContain, reflectionType);
        }
        /// <summary>
        /// 返回实体类型数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataTable">DataTable 数据</param>
        /// <param name="propertyMatchExpression">属性匹配，例：p=> new { ID = p.UserID }</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(DataTable dataTable, Expression<Func<T, object>> propertyMatchExpression, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            Dictionary<string, object> propertyDict = CommonHelper.GetExpressionDict<T>(propertyMatchExpression);
            return DataTableHelper.ExecuteToEntityList<T>(dataTable, propertyDict, reflectionType);
        }
        #endregion
    }
}
