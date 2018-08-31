/*
 * 作用：利用 NPOI 读取/写入 Excel 文档，支持读取合并单元格以及带有公式的数据。
 * */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.Text.RegularExpressions;
using NPOI.SS.Util;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    internal class ExcelFormat
    {
        public const string XLSX = ".xlsx";
        public const string XLS = ".xls";

        private static List<string> formatList;
        public static List<string> FormatList
        {
            get
            {
                if (formatList == null || formatList.Count == 0)
                {
                    formatList = new List<string>()
                    {
                        XLSX,
                        XLS
                    };
                }
                return formatList;
            }
        }
    }
    #endregion

    public class ExcelHelper
    {
        public static readonly ExcelHelper Instance = new ExcelHelper();

        #region 私有属性常量
        private const string ExcelFormatErrorException = "Excel 文件格式不正确！";
        internal const string ExcelWorkbookNullException = "Workbook 为空！";
        #endregion

        #region 对外公开方法

        #region ToEntityList<T>
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetName">Sheet 表单名称</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="headerIndex">表头起始索引，默认值：0，表示第一行是表头数据，与 dataIndex 相同时，表示 Excel 无表头</param>
        /// <param name="dataIndex">数据行起始索引，默认值：1，表示数据从第二行开始</param>
        /// <param name="primaryKey">主键标识，如果未指定，则表示第一列是主键</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(string excelPath, string sheetName, object propertyMatchList = null, int headerIndex = 0, int dataIndex = 1, string primaryKey = "", ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<T> dataList = new List<T>();
            ExecuteIWorkbookRead(excelPath, (IWorkbook workbook) =>
            {
                Dictionary<string, object> propertyDict = CommonHelper.GetParameterDict(propertyMatchList);
                dataList = SheetEntityList<T>(workbook, sheetName, propertyDict, headerIndex, dataIndex, primaryKey, reflectionType);
            });
            return dataList;
        }
        /// <summary>
        /// 返回字典数据
        /// </summary>
        /// <typeparam name="T">基本类型，例：int</typeparam>
        /// <typeparam name="K">基本类型，例：string</typeparam>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetName">Sheet 表单名称</param>
        /// <param name="keyIndex">键列数据索引，默认值：0</param>
        /// <param name="valueIndex">值列数据索引，默认值：1</param>
        /// <param name="dataIndex">数据行起始索引，默认值：1，表示数据从第二行开始</param>
        /// <returns></returns>
        public static Dictionary<T, K> ToDict<T, K>(string excelPath, string sheetName, int keyIndex = 0, int valueIndex = 1, int dataIndex = 1)
        {
            Dictionary<T, K> resultDict = new Dictionary<T, K>();
            ExecuteIWorkbookRead(excelPath, (IWorkbook workbook) =>
            {
                ISheet sheet = workbook.GetSheet(sheetName);

                IRow row = null;
                ICell keyCell = null;
                ICell valueCell = null;
                for (int index = dataIndex; index <= sheet.LastRowNum; index++)
                {
                    row = sheet.GetRow(index);
                    if (row != null)
                    {
                        keyCell = row.GetCell(keyIndex);
                        valueCell = row.GetCell(valueIndex);
                        // 检查主键列是否有数据
                        if (keyCell != null && !string.IsNullOrEmpty(keyCell.ToString()))
                        {
                            resultDict.Add((T)Convert.ChangeType(keyCell.ToString(), typeof(T)), (K)Convert.ChangeType(valueCell.ToString(), typeof(K)));
                        }
                    }
                }
            });
            return resultDict;
        }
        /// <summary>
        /// 返回基本类型数据列表
        /// </summary>
        /// <typeparam name="T">基本类型，例：int</typeparam>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetName">Sheet 表单名称</param>
        /// <param name="fieldIndex">字段列索引，默认值：0，表示取第一列数据</param>
        /// <param name="dataIndex">数据行起始索引，默认值：1，表示数据从第二行开始</param>
        /// <returns></returns>
        public static List<T> ToList<T>(string excelPath, string sheetName, int fieldIndex = 0, int dataIndex = 1)
        {
            List<T> dataList = new List<T>();
            ExecuteIWorkbookRead(excelPath, (IWorkbook workbook) =>
            {
                ISheet sheet = workbook.GetSheet(sheetName);

                IRow row = null;
                ICell cell = null;
                for (int index = dataIndex; index <= sheet.LastRowNum; index++)
                {
                    row = sheet.GetRow(index);
                    if (row != null)
                    {
                        cell = row.GetCell(fieldIndex);
                        if (cell != null && !string.IsNullOrEmpty(cell.ToString()))
                        {
                            dataList.Add((T)Convert.ChangeType(cell.ToString(), typeof(T)));
                        }
                    }
                }
            });
            return dataList;
        }
        #endregion

        #region ToDataTable
        /// <summary>
        /// 返回 DataTable 数据
        /// </summary>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetName">Sheet 表单名称</param>
        /// <param name="columnNameList">Excel 表头列列表，如果指定，则按指定表头列列表返回数据，当无表头时，通过 new string[] {"0", "2"} 来筛选指定列数据</param>
        /// <param name="nameMatchList">DataTable 表头列列表，如果指定，则按指定表头列列表创建 DataTable</param>
        /// <param name="headerIndex">表头起始索引，默认值：0，表示第一行是表头数据，与 dataIndex 相同时，表示 Excel 无表头</param>
        /// <param name="dataIndex">数据行起始索引，默认值：1，表示数据从第二行开始</param>
        /// <param name="primaryKey">主键标识，如果未指定，则表示第一列是主键</param>
        /// <returns></returns>
        public static DataTable ToDataTable(string excelPath, string sheetName, string[] columnNameList = null, object nameMatchList = null, int headerIndex = 0, int dataIndex = 1, string primaryKey = "")
        {
            DataTable dataTable = new DataTable();
            ExecuteIWorkbookRead(excelPath, (IWorkbook workbook) =>
            {
                List<string> filterColumnDataList = null;
                if (columnNameList != null) filterColumnDataList = columnNameList.ToList<string>();

                Dictionary<string, object> nameMatchDict = CommonHelper.GetParameterDict(nameMatchList);

                bool intValue = false;
                if (filterColumnDataList != null && filterColumnDataList.Count >= 1)
                {
                    if (Regex.IsMatch(filterColumnDataList[0], @"^\d+$")) intValue = true;
                }

                bool isHeader = headerIndex != dataIndex;

                Dictionary<string, int> filterExcelDataDict = new Dictionary<string, int>();

                ISheet sheet = workbook.GetSheet(sheetName);
                // 获得标题
                IRow headerRow = sheet.GetRow(headerIndex);

                // 获得列数量
                int cellCount = headerRow.LastCellNum;
                int primaryIndex = 0;

                DataColumn headerColumn = null;
                string columnName = "";
                int columnIndex = 0;
                if (!isHeader && !string.IsNullOrEmpty(primaryKey)) primaryIndex = int.Parse(primaryKey);
                for (int index = headerRow.FirstCellNum; index < cellCount; index++)
                {
                    if (isHeader)
                    {
                        if (string.IsNullOrEmpty(primaryKey))
                        {
                            if (index == headerRow.FirstCellNum) primaryIndex = index;
                        }
                        else
                        {
                            if (headerRow.GetCell(index).ToString() == primaryKey) primaryIndex = index;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(primaryKey))
                        {
                            if (index == headerRow.FirstCellNum) primaryIndex = index;
                        }
                    }
                    columnName = headerRow.GetCell(index).ToString();
                    if (!intValue)
                    {
                        if (filterColumnDataList != null && filterColumnDataList.IndexOf(columnName) < 0)
                        {
                            columnIndex++;
                            continue;
                        }
                    }
                    else
                    {
                        if (filterColumnDataList != null && filterColumnDataList.IndexOf(index.ToString()) < 0)
                        {
                            columnIndex++;
                            continue;
                        }
                    }
                    if (isHeader)
                    {
                        filterExcelDataDict.Add(columnName, index);
                    }
                    else
                    {
                        filterExcelDataDict.Add(index.ToString(), index);
                    }
                    if (nameMatchDict != null)
                    {
                        headerColumn = new DataColumn(nameMatchDict.ContainsKey(columnName) ? nameMatchDict[columnName].ToString() : "");
                    }
                    else
                    {
                        if (isHeader)
                        {
                            headerColumn = new DataColumn(columnName);
                        }
                        else
                        {
                            headerColumn = new DataColumn("column_" + index);
                        }
                    }
                    dataTable.Columns.Add(headerColumn);
                    columnIndex++;
                }
                IRow row = null;
                ICell iCell = null;
                DataRow dataRow = null;
                for (int index = dataIndex; index <= sheet.LastRowNum; index++)
                {
                    row = sheet.GetRow(index);
                    if (row != null)
                    {
                        iCell = row.GetCell(primaryIndex);
                        // 检查主键列是否有数据
                        if (iCell != null && !string.IsNullOrEmpty(iCell.ToString()))
                        {
                            dataRow = dataTable.NewRow();
                            columnIndex = 0;
                            foreach (KeyValuePair<string, int> keyValuePair in filterExcelDataDict)
                            {
                                iCell = row.GetCell(keyValuePair.Value);
                                dataRow[columnIndex] = (iCell != null && !string.IsNullOrEmpty(iCell.ToString())) ? iCell.ToString() : null;
                                columnIndex++;
                            }
                            dataTable.Rows.Add(dataRow);
                        }
                    }
                }
            });
            return dataTable;
        }
        #endregion

        #region ToExcel<T>
        /// <summary>
        /// 根据实体数据列表创建 Excel
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataList">实体数据列表</param>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetName">Sheet 表单名称</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="propertyList">属性列表，如果指定，则按指定属性列表生成 Excel</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="cellCallback">单元格写入之后调用</param>
        /// <param name="sheetCallback">表单数据写入之后调用</param>
        /// <param name="isHeader">是否创建表头</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static bool ToExcel<T>(List<T> dataList, string excelPath, string sheetName, object propertyMatchList = null, string[] propertyList = null, bool propertyContain = true, Action<ICell, object> cellCallback = null, Action<ISheet, List<string>> sheetCallback = null, bool isHeader = true, object columnValueFormat = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            bool result = ExecuteIWorkbookWrite(excelPath, (IWorkbook workbook) =>
            {
                List<string> filterNameList = null;
                if (propertyList != null) filterNameList = propertyList.ToList<string>();

                Dictionary<string, object> propertyDict = CommonHelper.GetParameterDict(propertyMatchList);
                Dictionary<string, object> valueFormatDict = CommonHelper.GetParameterDict(columnValueFormat);
                ToSheet(workbook, dataList, sheetName, cellCallback, sheetCallback, isHeader, filterNameList, propertyContain, propertyDict, valueFormatDict, reflectionType);
            });
            return result;
        }
        /// <summary>
        /// 根据 DataTable 数据创建 Excel
        /// </summary>
        /// <param name="dataTable">DataTable 数据</param>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetName">Sheet 表单名称</param>
        /// <param name="isHeader">是否创建表头</param>
        /// <param name="columnNameList">DataTable 表头列列表，如果指定，则按指定表头列列表创建 Excel 数据</param>
        /// <param name="nameMatchList">Excel 表头列列表，如果指定，则按指定表头列列表生成 Excel 表头数据</param>
        /// <returns></returns>
        public static bool ToExcel(DataTable dataTable, string excelPath, string sheetName, bool isHeader = true, string[] columnNameList = null, object nameMatchList = null)
        {
            bool result = ExecuteIWorkbookWrite(excelPath, (IWorkbook workbook) =>
            {
                List<string> filterNameList = null;
                if (columnNameList != null) filterNameList = columnNameList.ToList<string>();

                ISheet iSheet = workbook.CreateSheet(sheetName);
                int columnIndex = 0;
                // 获得表头数据
                Dictionary<string, int> headerColumnNameDict = new Dictionary<string, int>();
                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    headerColumnNameDict.Add(dataColumn.ColumnName, columnIndex);
                    columnIndex++;
                }
                // 获取名称匹配
                Dictionary<string, object> nameMatchDict = CommonHelper.GetParameterDict(nameMatchList);
                Dictionary<string, string> headerHeaderNameDict = new Dictionary<string, string>();
                int headerNameIndex = 0;
                foreach (KeyValuePair<string, int> keyValuePair in headerColumnNameDict)
                {
                    if (filterNameList != null && filterNameList.IndexOf(keyValuePair.Key) < 0)
                    {
                        headerNameIndex++;
                        continue;
                    }
                    if (nameMatchDict != null && nameMatchDict.ContainsKey(keyValuePair.Key))
                    {
                        headerHeaderNameDict.Add(keyValuePair.Key, nameMatchDict[keyValuePair.Key].ToString());
                    }
                    else
                    {
                        headerHeaderNameDict.Add(keyValuePair.Key, keyValuePair.Key);
                    }
                    headerNameIndex++;
                }
                int dataIndex = 0;
                if (isHeader)
                {
                    columnIndex = 0;
                    IRow headerRow = iSheet.CreateRow(dataIndex);
                    // 遍历设置表头
                    foreach (KeyValuePair<string, string> keyValuePair in headerHeaderNameDict)
                    {
                        headerRow.CreateCell(columnIndex).SetCellValue(keyValuePair.Value);
                        columnIndex++;
                    }
                    dataIndex = 1;
                }
                IRow row = null;
                // 遍历设置数据
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    columnIndex = 0;
                    row = iSheet.CreateRow(dataIndex);
                    foreach (KeyValuePair<string, string> keyValuePair in headerHeaderNameDict)
                    {
                        row.CreateCell(columnIndex).SetCellValue(dataRow[headerColumnNameDict[keyValuePair.Key]].ToString());
                        columnIndex++;
                    }
                    dataIndex++;
                }
            });
            return result;
        }
        #endregion

        #endregion

        #region 逻辑处理私有方法
        internal static void ExecuteIWorkbookRead(string excelPath, Action<IWorkbook> callback)
        {
            FileStream fileStream = null;
            IWorkbook workbook = null;
            try
            {
                string suffix = FileHelper.GetSuffix(excelPath);
                if (ExcelFormat.FormatList.IndexOf(suffix) < 0) throw new Exception(ExcelFormatErrorException);

                using (fileStream = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
                {
                    workbook = ExecuteIWorkBookGet(excelPath, fileStream);
                    if (workbook == null)
                    {
                        throw new Exception(ExcelWorkbookNullException);
                    }
                    if (callback != null) callback(workbook);
                    workbook.Close();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (fileStream != null) fileStream.Close();
                if (workbook != null) workbook.Close();
            }
        }
        internal static bool ExecuteIWorkbookWrite(string excelPath, Action<IWorkbook> callback)
        {
            FileStream fileStream = null;
            IWorkbook workbook = null;
            try
            {
                //获得 Excel 后缀
                string suffix = FileHelper.GetSuffix(excelPath);
                if (ExcelFormat.FormatList.IndexOf(suffix) < 0) throw new Exception(ExcelFormatErrorException);

                // 创建对应目录
                bool createDirectoryStatus = FileHelper.CreateDirectory(excelPath);
                // 如果创建目录失败，则终止处理
                if (!createDirectoryStatus) return false;

                // 如果存在 Excel 文件，先删除文件
                if (File.Exists(excelPath)) File.Delete(excelPath);
                // 创建 Excel
                using (fileStream = new FileStream(excelPath, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    workbook = ExecuteIWorkBookCreate(suffix);
                    if (workbook == null) throw new Exception(ExcelWorkbookNullException);

                    callback(workbook);

                    workbook.Write(fileStream);
                    workbook.Close();
                }

                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (fileStream != null) fileStream.Close();
                if (workbook != null) workbook.Close();
            }
        }
        internal static IWorkbook ExecuteIWorkBookGet(string excelPath, FileStream fileStream)
        {
            string suffix = FileHelper.GetSuffix(excelPath);
            IWorkbook workbook = null;
            if (suffix == ExcelFormat.XLSX) // 2007版本  
            {
                workbook = new XSSFWorkbook(fileStream);  //xlsx数据读入workbook  
            }
            else if (suffix == ExcelFormat.XLS) // 2003版本  
            {
                workbook = new HSSFWorkbook(fileStream);  //xls数据读入workbook  
            }
            return workbook;
        }
        internal static IWorkbook ExecuteIWorkBookCreate(string suffix)
        {
            IWorkbook workbook = null;
            if (suffix == ExcelFormat.XLSX)
            {
                workbook = new XSSFWorkbook();
            }
            else if (suffix == ExcelFormat.XLS)
            {
                workbook = new HSSFWorkbook();
            }
            return workbook;
        }
        internal static List<T> SheetEntityList<T>(IWorkbook workbook, string sheetName, Dictionary<string, object> propertyDict = null, int headerIndex = 0, int dataIndex = 1, string primaryKey = "", ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<T> dataList = new List<T>();

            ISheet sheet = workbook.GetSheet(sheetName);

            bool isHeader = headerIndex != dataIndex;
            IRow headerRow = sheet.GetRow(headerIndex);

            int primaryIndex = 0;

            Dictionary<PropertyInfo, ExcelToEntityColumnMapper> excelToEntityMapperList = InitExcelToEntityMapper<T>(headerRow, isHeader, primaryKey, ref primaryIndex, propertyDict);

            dynamic propertySetDict = null;
            if (reflectionType != ReflectionTypeEnum.Original) propertySetDict = ReflectionExtendHelper.PropertySetCallDict<T>(reflectionType);

            List<ExcelMergeCell> mergeCellList = SheetMergeCellList(sheet);

            IRow row = null;
            ICell iCell = null;
            for (int index = dataIndex; index <= sheet.LastRowNum; index++)
            {
                row = sheet.GetRow(index);
                if (row != null)
                {
                    iCell = row.GetCell(primaryIndex);
                    // 检查主键列是否有数据
                    if (iCell != null && !string.IsNullOrEmpty(iCell.ToString()))
                    {
                        dataList.Add(IRowToEntity<T>(index, row, propertySetDict, excelToEntityMapperList, mergeCellList));
                    }
                }
            }

            return dataList;
        }
        internal static void ToSheet<T>(IWorkbook workbook, List<T> dataList, string sheetName, Action<ICell, object> cellCallback = null, Action<ISheet, List<string>> sheetCallback = null, bool isHeader = true, List<string> filterNameList = null, bool propertyContain = true, Dictionary<string, object> propertyDict = null, Dictionary<string, object> valueFormatDict = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            ISheet iSheet = workbook.CreateSheet(sheetName);
            // 获得表头数据
            Dictionary<string, PropertyInfo> headerColumnNameDict = CommonHelper.InitPropertyWriteMapperFormat<T, ExcelTAttribute>(propertyDict, filterNameList, propertyContain);

            int dataIndex = 0;
            if (isHeader)
            {
                IRow headerRow = iSheet.CreateRow(dataIndex);
                int columnIndex = 0;
                // 遍历设置表头
                foreach (KeyValuePair<string, PropertyInfo> keyValuePair in headerColumnNameDict)
                {
                    headerRow.CreateCell(columnIndex).SetCellValue(keyValuePair.Key);
                    columnIndex++;
                }
                dataIndex = 1;
            }

            dynamic propertyGetDict = null;
            if (reflectionType != ReflectionTypeEnum.Original) propertyGetDict = ReflectionExtendHelper.PropertyGetCallDict<T>(reflectionType);

            IRow row = null;
            if (dataList != null)
            {
                // 遍历设置数据
                for (int rowIndex = 0; rowIndex < dataList.Count; rowIndex++, dataIndex++)
                {
                    row = iSheet.CreateRow(dataIndex);
                    SetRowDataValue(row, cellCallback, dataList[rowIndex], propertyGetDict, headerColumnNameDict, valueFormatDict);
                }
            }
            if (sheetCallback != null) sheetCallback(iSheet, headerColumnNameDict.Keys.ToList());
        }
        internal static string GetCellText(ICell iCell, Type type)
        {
            string cellText = "";

            if (type == typeof(int) || type == typeof(float) || type == typeof(decimal) || type == typeof(double))
            {
                if (iCell.CellType == CellType.Numeric || iCell.CellType == CellType.Formula)
                {
                    cellText = iCell.NumericCellValue.ToString();
                }
                else
                {
                    cellText = iCell.ToString();
                }
            }
            else
            {
                if (type == typeof(DateTime))
                {
                    if (iCell.CellType == CellType.String)
                    {
                        cellText = iCell.StringCellValue;
                    }
                    else
                    {
                        cellText = iCell.DateCellValue.ToString();
                    }
                }
                else
                {
                    if (iCell.CellType == CellType.Formula && type == typeof(string))
                    {
                        cellText = iCell.StringCellValue;
                    }
                    else
                    {
                        cellText = iCell.ToString();
                    }
                }
            }

            return cellText;
        }
        
        #region ToEntity 相关
        private static T IRowToEntity<T>(int rowIndex, IRow row, dynamic propertySetDict, Dictionary<PropertyInfo, ExcelToEntityColumnMapper> excelToEntityMapperList, List<ExcelMergeCell> mergeCellList) where T : class, new()
        {
            T t = ReflectionGenericHelper.New<T>();
            ICell iCell = null;
            string cellText = null;

            foreach (var keyValueItem in excelToEntityMapperList)
            {
                cellText = GetSheetMergeCellText(mergeCellList, rowIndex, keyValueItem.Value.ColumnIndex);
                if (cellText == null)
                {
                    iCell = row.GetCell(keyValueItem.Value.ColumnIndex);
                    if (iCell != null)
                    {
                        cellText = GetCellText(iCell, keyValueItem.Key.PropertyType);
                    }
                }
                if (!string.IsNullOrEmpty(cellText))
                {
                    if (propertySetDict != null && propertySetDict.ContainsKey(keyValueItem.Key.Name))
                    {
                        ReflectionGenericHelper.SetPropertyValue(propertySetDict[keyValueItem.Key.Name], t, cellText, keyValueItem.Key);
                    }
                    else
                    {
                        ReflectionHelper.SetPropertyValue(t, cellText, keyValueItem.Key);
                    }
                }
            }
            return t;
        }
        private static Dictionary<PropertyInfo, ExcelToEntityColumnMapper> InitExcelToEntityMapper<T>(IRow row, bool isHeader, string primaryKey, ref int primaryIndex, Dictionary<string, object> propertyDict = null) where T : class
        {
            Dictionary<string, int> columnNameDict = null;
            Dictionary<int, int> columnIndexDict = null;
            if (isHeader)
            {
                columnNameDict = InitExcelPrimaryMapperByName(row, primaryKey, ref primaryIndex);
            }
            else
            {
                columnIndexDict = InitExcelPrimaryMapperByIndex(row, primaryKey, ref primaryIndex);
            }

            Dictionary<PropertyInfo, ExcelToEntityColumnMapper> resultList = new Dictionary<PropertyInfo, ExcelToEntityColumnMapper>();
            ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
            {
                if (isHeader)
                {
                    string columnName = null;
                    if (propertyDict != null && propertyDict.ContainsKey(propertyInfo.Name))
                    {
                        columnName = propertyDict[propertyInfo.Name].ToString();
                    }
                    else
                    {
                        ExcelTAttribute attribute = propertyInfo.GetCustomAttribute<ExcelTAttribute>();
                        if (attribute != null)
                        {
                            if (attribute.Type == AttributeReadAndWriteTypeEnum.ReadAndWrite || attribute.Type == AttributeReadAndWriteTypeEnum.Read)
                            {
                                if (!string.IsNullOrEmpty(attribute.Name))
                                {
                                    columnName = attribute.Name;
                                }
                                else
                                {
                                    columnName = propertyInfo.Name;
                                }
                            }
                        }
                        else
                        {
                            columnName = propertyInfo.Name;
                        }
                    }
                    if (columnName != null && columnNameDict.ContainsKey(columnName))
                    {
                        resultList.Add(propertyInfo, new ExcelToEntityColumnMapper() { ColumnName = columnName, ColumnIndex = columnNameDict[columnName] });
                    }
                }
                else
                {
                    int columnIndex = -1;
                    if (propertyDict != null && propertyDict.ContainsKey(propertyInfo.Name))
                    {
                        columnIndex = int.Parse(propertyDict[propertyInfo.Name].ToString());
                    }
                    else
                    {
                        ExcelTAttribute attribute = propertyInfo.GetCustomAttribute<ExcelTAttribute>();
                        if (attribute != null) columnIndex = attribute.Index;
                    }
                    if (columnIndexDict.ContainsKey(columnIndex))
                    {
                        resultList.Add(propertyInfo, new ExcelToEntityColumnMapper() { ColumnName = null, ColumnIndex = columnIndex });
                    }
                }
            });
            return resultList;
        }
        private static Dictionary<string, int> InitExcelPrimaryMapperByName(IRow row, string primaryKey, ref int primaryIndex)
        {
            Dictionary<string, int> columnNameDict = new Dictionary<string, int>();
            // 获得列数量
            int cellCount = row.LastCellNum;
            // 获得所有列名
            for (int index = row.FirstCellNum; index < cellCount; index++)
            {
                columnNameDict[row.GetCell(index).ToString()] = index;
                if (string.IsNullOrEmpty(primaryKey))
                {
                    if (index == row.FirstCellNum) primaryIndex = index;
                }
                else
                {
                    if (row.GetCell(index).ToString() == primaryKey) primaryIndex = index;
                }
            }
            return columnNameDict;
        }
        private static Dictionary<int, int> InitExcelPrimaryMapperByIndex(IRow row, string primaryKey, ref int primaryIndex)
        {
            if (!string.IsNullOrEmpty(primaryKey)) primaryIndex = int.Parse(primaryKey);
            Dictionary<int, int> columnIndexDict = new Dictionary<int, int>();
            // 获得列数量
            int cellCount = row.LastCellNum;
            // 获得所有列名
            for (int index = row.FirstCellNum; index < cellCount; index++)
            {
                columnIndexDict[index] = index;
                if (string.IsNullOrEmpty(primaryKey))
                {
                    if (index == row.FirstCellNum) primaryIndex = index;
                }
            }
            return columnIndexDict;
        }
        #endregion

        #region ToExcel 相关
        private static void SetRowDataValue<T>(IRow row, Action<ICell, object> cellCallback, T t, dynamic propertyGetDict, Dictionary<string, PropertyInfo> headerColumnNameDict, Dictionary<string, object> valueFormatDict) where T : class
        {
            Type type = typeof(T);

            int columnIndex = 0;

            object propertyValue = null;
            foreach (KeyValuePair<string, PropertyInfo> keyValuePair in headerColumnNameDict)
            {
                if (propertyGetDict != null && propertyGetDict.ContainsKey(keyValuePair.Value.Name))
                {
                    propertyValue = propertyGetDict[keyValuePair.Value.Name](t);
                }
                else
                {
                    propertyValue = ReflectionHelper.GetPropertyValue(t, keyValuePair.Value);
                }
                if (propertyValue != null)
                {
                    if ((keyValuePair.Value.PropertyType == typeof(DateTime) || keyValuePair.Value.PropertyType == typeof(Nullable<DateTime>)) && valueFormatDict != null && valueFormatDict.ContainsKey(keyValuePair.Value.Name))
                    {
                        propertyValue = ((DateTime)propertyValue).ToString(valueFormatDict[keyValuePair.Value.Name].ToString());
                    }

                    ICell iCell = row.CreateCell(columnIndex);
                    iCell.SetCellValue(propertyValue.ToString());

                    if (cellCallback != null) cellCallback(iCell, propertyValue);
                }
                columnIndex++;
            }
        }
        #endregion

        #region 合并单元格相关
        internal static List<ExcelMergeCell> SheetMergeCellList(ISheet sheet)
        {
            List<ExcelMergeCell> dataList = new List<ExcelMergeCell>();
            int mergeCount = sheet.NumMergedRegions;
            for (int mergeIndex = 0; mergeIndex < mergeCount; mergeIndex++)
            {
                CellRangeAddress range = sheet.GetMergedRegion(mergeIndex);
                ICell cell = sheet.GetRow(range.FirstRow).GetCell(range.FirstColumn);
                if (cell != null && !string.IsNullOrEmpty(cell.ToString()))
                {
                    dataList.Add(new ExcelMergeCell() { BeginRow = range.FirstRow, BeginColumn = range.FirstColumn, EndRow = range.LastRow, EndColumn = range.LastColumn, CellText = cell.ToString() });
                }
            }
            return dataList;
        }
        internal static string GetSheetMergeCellText(List<ExcelMergeCell> dataList, int rowIndex, int columnIndex)
        {
            ExcelMergeCell dataCell = dataList.Where(p => (p.BeginRow <= rowIndex && p.EndRow >= rowIndex) && (p.BeginColumn <= columnIndex && p.EndColumn >= columnIndex)).FirstOrDefault();
            if (dataCell != null) return dataCell.CellText;
            return null;
        }
        #endregion

        #endregion
    }

    #region 逻辑处理辅助特性
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ExcelTAttribute : BaseReadAndWriteTAttribute
    {
        private int index;

        /// <summary>
        /// 实体属性映射 Excel 列名
        /// </summary>
        /// <param name="name">Excel 列名</param>
        public ExcelTAttribute(string name, AttributeReadAndWriteTypeEnum type = AttributeReadAndWriteTypeEnum.ReadAndWrite)
            : base(name, type)
        {
            this.index = -1;
        }

        /// <summary>
        /// 实体属性映射 Excel 列索引
        /// </summary>
        /// <param name="index">Excel 列索引</param>
        public ExcelTAttribute(int index, AttributeReadAndWriteTypeEnum type = AttributeReadAndWriteTypeEnum.ReadAndWrite)
            : base(null, type)
        {
            this.index = index;
        }

        /// <summary>
        /// 实体属性所对应的 Excel 列索引
        /// </summary>
        public int Index { get { return this.index; } }
    }
    #endregion

    #region 逻辑处理辅助类
    internal class ExcelToEntityColumnMapper
    {
        public string ColumnName { get; set; }

        public int ColumnIndex { get; set; }
    }
    internal class ExcelMergeCell
    {
        public int BeginRow { get; set; }
        public int BeginColumn { get; set; }
        public int EndRow { get; set; }
        public int EndColumn { get; set; }

        public string CellText { get; set; }
    }
    #endregion
}
