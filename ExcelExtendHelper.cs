/*
 * 作用：利用 NPOI 读取/写入 Excel 文档，支持多表单文档的读取/写入。
 * */
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Helper.Core.Library
{
    public class ExcelExtendHelper
    {
        #region 私有属性常量
        private const string ExcelFormatErrorException = "Excel 文件后缀不正确！";
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 返回多 Sheet 表单数据
        /// </summary>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetDataList">单元格数据，类型：ReadMultiSheet</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static Dictionary<string, object> ToEntityList(string excelPath, List<object> sheetDataList, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            Dictionary<string, object> dataDict = new Dictionary<string, object>();
            ExcelHelper.ExecuteIWorkbookRead(excelPath, (IWorkbook workbook) =>
            {
                MethodInfo method = typeof(ExcelHelper).GetMethod("SheetEntityList", BindingFlags.Static | BindingFlags.NonPublic);
                foreach (object sheetData in sheetDataList)
                {
                    IReadMultiSheet excelMultiSheet = sheetData as IReadMultiSheet;
                    if (excelMultiSheet != null)
                    {
                        Type sheetType = ReflectionHelper.GetGenericType(sheetData.GetType());
                        MethodInfo generic = method.MakeGenericMethod(sheetType);
                        object objectData = generic.Invoke(ExcelHelper.Instance, new object[] { 
                            workbook,
                            excelMultiSheet.SheetName,
                            excelMultiSheet.PropertyMatchList,
                            excelMultiSheet.HeaderIndex,
                            excelMultiSheet.DataIndex,
                            excelMultiSheet.PrimaryKey,
                            reflectionType
                        });
                        dataDict.Add(excelMultiSheet.SheetName, objectData);
                    }
                }
            });
            return dataDict;
        }
        /// <summary>
        /// 创建多 Sheet Excel 文档
        /// </summary>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetDataList">单元格数据，类型：WriteMultiSheet</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static bool ToExcel(string excelPath, List<object> sheetDataList, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression)
        {
            bool result = ExcelHelper.ExecuteIWorkbookWrite(excelPath, (IWorkbook workbook) =>
            {
                MethodInfo method = typeof(ExcelHelper).GetMethod("ToSheet", BindingFlags.Static | BindingFlags.NonPublic);
                foreach (object sheetData in sheetDataList)
                {
                    IWriteMultiSheet excelMultiSheet = sheetData as IWriteMultiSheet;
                    if (excelMultiSheet != null)
                    {
                        Type sheetType = ReflectionHelper.GetGenericType(sheetData.GetType());
                        MethodInfo generic = method.MakeGenericMethod(sheetType);
                        generic.Invoke(ExcelHelper.Instance, new object[] { 
                            workbook,
                            ReflectionHelper.GetPropertyValue(sheetData, sheetData.GetType().GetProperty("DataList")),
                            excelMultiSheet.SheetName,
                            excelMultiSheet.CellCallback,
                            excelMultiSheet.SheetCallback,
                            excelMultiSheet.IsHeader,
                            excelMultiSheet.PropertyList,
                            excelMultiSheet.PropertyContain,
                            excelMultiSheet.PropertyMatchList,
                            excelMultiSheet.ColumnValueFormat,
                            reflectionType
                        });
                    }
                }
            });
            return result;
        }
        #endregion
    }

    #region 逻辑处理接口对象
    internal interface IWriteMultiSheet
    {
        string SheetName { get; set; }
        bool IsHeader { get; set; }
        string[] PropertyList { get; set; }
        bool PropertyContain { get; set; }
        object PropertyMatchList { get; set; }
        object ColumnValueFormat { get; set; }
        Action<ICell, object> CellCallback { get; set; }
        Action<ISheet, List<string>> SheetCallback { get; set; }
    }
    internal interface IReadMultiSheet
    {
        string SheetName { get; set; }
        int HeaderIndex { get; set; }
        int DataIndex { get; set; }
        string PrimaryKey { get; set; }
        object PropertyMatchList { get; set; }
    }
    #endregion

    #region 逻辑处理辅助类
    public class ReadMultiSheet<T> : IReadMultiSheet where T : class
    {
        private int _headerIndex = 0;
        private int _dataIndex = 1;

        /// <summary>
        /// Sheet 名称
        /// </summary>
        public string SheetName { get; set; }

        /// <summary>
        /// 表头起始索引，默认值：0，表示第一行是表头数据，与 dataIndex 相同时，表示 Excel 无表头
        /// </summary>
        public int HeaderIndex
        {
            get { return this._headerIndex; }
            set { this._headerIndex = value; }
        }

        /// <summary>
        /// 数据行起始索引，默认值：1，表示数据从第二行开始
        /// </summary>
        public int DataIndex
        {
            get { return this._dataIndex; }
            set { this._dataIndex = value; }
        }

        /// <summary>
        /// 主键标识，如果未指定，则表示第一列是主键
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        /// 属性匹配，Dictionary&lt;string, object&gt; 或 new {}
        /// </summary>
        public object PropertyMatchList { get; set; }

    }
    public class WriteMultiSheet<T> : IWriteMultiSheet where T : class
    {
        private bool _isHeader = true;
        private bool _propertyContain = true;

        /// <summary>
        /// Sheet 表单名称
        /// </summary>
        public string SheetName { get; set; }

        /// <summary>
        /// 实体数据列表
        /// </summary>
        public List<T> DataList { get; set; }

        /// <summary>
        /// 是否创建表头
        /// </summary>
        public bool IsHeader
        {
            get { return this._isHeader; }
            set { this._isHeader = value; }
        }

        /// <summary>
        /// 属性筛选列表
        /// </summary>
        public string[] PropertyList { get; set; }

        /// <summary>
        /// 是否包含
        /// </summary>
        public bool PropertyContain
        {
            get { return this._propertyContain; }
            set { this._propertyContain = value; }
        }

        /// <summary>
        /// 属性匹配，Dictionary&lt;string, object&gt; 或 new {}
        /// </summary>
        public object PropertyMatchList { get; set; }

        /// <summary>
        /// 日期格式化
        /// </summary>
        public object ColumnValueFormat { get; set; }

        /// <summary>
        /// 单元格写入之后调用
        /// </summary>
        public Action<ICell, object> CellCallback { get; set; }

        /// <summary>
        /// 表单数据写入之后调用
        /// </summary>
        public Action<ISheet, List<string>> SheetCallback { get; set; }
    }
    #endregion
}
