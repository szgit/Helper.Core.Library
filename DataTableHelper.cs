/*
 * 作用：DataTable 与实体数据列表相互转换。
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;
using System.Linq.Expressions;

namespace Helper.Core.Library
{
    public class DataTableHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 返回 DataTable 数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataList">实体类型数据列表</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="propertyList">属性列表，如果指定，则按指定属性列表生成 DataTable 数据</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(List<T> dataList, object propertyMatchList = null, string[] propertyList = null, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            List<string> propertyNameList = null;
            if (propertyList != null) propertyNameList = propertyList.ToList<string>();

            Dictionary<string, object> propertyDict = CommonHelper.GetParameterDict(propertyMatchList);
            return ExecuteToDataTable<T>(dataList, propertyDict, propertyNameList, propertyContain, reflectionType);
        }
        /// <summary>
        /// 返回实体类型数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataTable">DataTable 数据</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(DataTable dataTable, object propertyMatchList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            Dictionary<string, object> propertyDict = CommonHelper.GetParameterDict(propertyMatchList);
            return ExecuteToEntityList<T>(dataTable, propertyDict, reflectionType);
        }

        #endregion

        #region 逻辑处理私有方法
        internal static DataTable ExecuteToDataTable<T>(List<T> dataList, Dictionary<string, object> propertyDict, List<string> propertyList = null, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Dictionary<string, string> columnNameList = CommonHelper.InitPropertyWriteMapper<T, DataTableTAttribute>(propertyDict, propertyList, propertyContain);

            DataTable dataTable = new DataTable();

            // 遍历设置标题
            foreach (KeyValuePair<string, string> keyValuePair in columnNameList)
            {
                dataTable.Columns.Add(keyValuePair.Key);
            }

            dynamic propertyGetDict = null;
            if (reflectionType != ReflectionTypeEnum.Original) propertyGetDict = ReflectionExtendHelper.PropertyGetCallDict<T>(reflectionType);

            DataRow dataRow = null;
            // 遍历数据并设置到 DataTable
            foreach (T t in dataList)
            {
                dataRow = dataTable.NewRow();
                SetRowDataValue(dataRow, t, propertyGetDict, columnNameList);
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }
        internal static List<T> ExecuteToEntityList<T>(DataTable dataTable, Dictionary<string, object> propertyDict, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            Dictionary<PropertyInfo, string> columnNameDict = CommonHelper.InitPropertyReadMapper<T, DataTableTAttribute>(propertyDict, (name) => dataTable.Columns.Contains(name));

            List<T> dataList = new List<T>();

            dynamic propertySetDict = null;
            if (reflectionType != ReflectionTypeEnum.Original) propertySetDict = ReflectionExtendHelper.PropertySetCallDict<T>(reflectionType);

            // 读取 DataTable 数据到实体对象列表中
            foreach (DataRow dataRow in dataTable.Rows)
            {
                dataList.Add(DataRowToEntity<T>(dataRow, propertySetDict, columnNameDict));
            }
            return dataList;
        }
        private static T DataRowToEntity<T>(DataRow dataRow, dynamic propertySetDict, Dictionary<PropertyInfo, string> columnNameDict) where T : class, new()
        {
            T t = ReflectionGenericHelper.New<T>();
            foreach (var keyValueItem in columnNameDict)
            {
                if (dataRow[keyValueItem.Value] != null)
                {
                    if (propertySetDict != null && propertySetDict.ContainsKey(keyValueItem.Key.Name))
                    {
                        ReflectionGenericHelper.SetPropertyValue(propertySetDict[keyValueItem.Key.Name], t, dataRow[keyValueItem.Value].ToString(), keyValueItem.Key);
                    }
                    else
                    {
                        ReflectionHelper.SetPropertyValue(t, dataRow[keyValueItem.Value].ToString(), keyValueItem.Key);
                    }
                }
            }
            return t;
        }
        private static void SetRowDataValue<T>(DataRow dataRow, T t, dynamic propertyGetDict, Dictionary<string, string> headerColumnNameDict) where T : class
        {
            object propertyValue = null;
            foreach (var keyValueItem in headerColumnNameDict)
            {
                if (propertyGetDict != null && propertyGetDict.ContainsKey(keyValueItem.Value))
                {
                    propertyValue = propertyGetDict[keyValueItem.Value](t);
                }
                else
                {
                    propertyValue = ReflectionHelper.GetPropertyValue(t, keyValueItem.Value);
                }
                if (propertyValue != null) dataRow[keyValueItem.Key] = propertyValue;
            }
        }
        #endregion
    }

    #region 逻辑处理辅助特性
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DataTableTAttribute : BaseReadAndWriteTAttribute
    {
        /// <summary>
        /// 实体属性映射 DataTable 列名
        /// </summary>
        /// <param name="name">属性所对应的 DataTable 列名称</param>
        public DataTableTAttribute(string name, AttributeReadAndWriteTypeEnum type = AttributeReadAndWriteTypeEnum.ReadAndWrite)
            : base(name, type)
        {
        }
    }
    #endregion
}
