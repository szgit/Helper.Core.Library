/*
 * 作用：利用 NPOI 读取/写入 Excel 文档，是对 ExcelHelper 的扩展，参数使用表达式，目的是减少属性名的拼写错误。
 * */
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Helper.Core.Library
{
    public class ExcelExpressionHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetName">Sheet 表单名称</param>
        /// <param name="propertyMatchExpression">属性匹配，例：p=> new { ID = p.UserID }</param>
        /// <param name="headerIndex">表头起始索引，默认值：0，表示第一行是表头数据，与 dataIndex 相同时，表示 Excel 无表头</param>
        /// <param name="dataIndex">数据行起始索引，默认值：1，表示数据从第二行开始</param>
        /// <param name="primaryKey">主键标识，如果未指定，则表示第一列是主键</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(string excelPath, string sheetName, Expression<Func<T, object>> propertyMatchExpression, int headerIndex = 0, int dataIndex = 1, string primaryKey = "", ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<T> dataList = new List<T>();
            ExcelHelper.ExecuteIWorkbookRead(excelPath, (IWorkbook workbook) =>
            {
                Dictionary<string, object> propertyDict = CommonHelper.GetExpressionDict<T>(propertyMatchExpression);
                dataList = ExcelHelper.SheetEntityList<T>(workbook, sheetName, propertyDict, headerIndex, dataIndex, primaryKey, reflectionType);
            });
            return dataList;
        }
        /// <summary>
        /// 根据实体数据列表创建 Excel
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataList">实体数据列表</param>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetName">Sheet 表单名称</param>
        /// <param name="propertyMatchExpression">属性匹配，例：p=> new { ID = p.UserID }</param>
        /// <param name="propertyExpression">属性列表，如果指定，则按指定属性列表生成 Excel 数据，例：p=> new { p.UserID }</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="cellCallback">单元格写入之后调用</param>
        /// <param name="sheetCallback">表单数据写入之后调用</param>
        /// <param name="isHeader">是否创建表头</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static bool ToExcel<T>(List<T> dataList, string excelPath, string sheetName, Expression<Func<T, object>> propertyMatchExpression, Expression<Func<T, object>> propertyExpression = null, bool propertyContain = true, Action<ICell, object> cellCallback = null, Action<ISheet, List<string>> sheetCallback = null, bool isHeader = true, object columnValueFormat = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            bool result = ExcelHelper.ExecuteIWorkbookWrite(excelPath, (IWorkbook workbook) =>
            {
                Dictionary<string, object> propertyDict = CommonHelper.GetExpressionDict<T>(propertyMatchExpression);
                Dictionary<string, object> valueFormatDict = CommonHelper.GetParameterDict(columnValueFormat);
                List<string> propertyNameList = CommonHelper.GetExpressionList<T>(propertyExpression);

                ExcelHelper.ToSheet(workbook, dataList, sheetName, cellCallback, sheetCallback, isHeader, propertyNameList, propertyContain, propertyDict, valueFormatDict, reflectionType);
            });
            return result;
        }
        #endregion
    }
}
