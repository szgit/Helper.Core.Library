/*
 * 作用：利用 NPOI 读取 Excel 指定单元格数据。
 * */
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library
{
    public class ExcelAssistHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 获取指定单元格数据
        /// </summary>
        /// <param name="assistCellDataList">指定查询单元格数据列表</param>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetName">表单名称</param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDict(List<ExcelAssistCellData> assistCellDataList, string excelPath, string sheetName)
        {
            Dictionary<string, string> resultDict = new Dictionary<string, string>();

            ExcelHelper.ExecuteIWorkbookRead(excelPath, (IWorkbook workbook) =>
            {
                ISheet sheet = workbook.GetSheet(sheetName);
                IRow row = null;
                ICell cell = null;

                foreach (ExcelAssistCellData assistCellData in assistCellDataList)
                {
                    row = sheet.GetRow(assistCellData.RowIndex);
                    if (row != null)
                    {
                        cell = row.GetCell(assistCellData.ColumnIndex);
                        if (cell != null) resultDict.Add(assistCellData.Key, ExcelHelper.GetCellText(cell, assistCellData.KeyType));
                    }
                }
            });

            return resultDict;
        }
        #endregion
    }

    #region 逻辑处理辅助类
    public class ExcelAssistCellData
    {
        #region 构造函数
        public ExcelAssistCellData() { }
        public ExcelAssistCellData(string key, int rowIndex, int columnIndex, Type keyType)
        {
            this.Key = key;
            this.RowIndex = rowIndex;
            this.ColumnIndex = columnIndex;
            this.KeyType = keyType;
        }
        #endregion

        /// <summary>
        /// 唯一标识，用来查询用
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 行索引
        /// </summary>
        public int RowIndex { get; set; }
        /// <summary>
        /// 列索引
        /// </summary>
        public int ColumnIndex { get; set; }
        /// <summary>
        /// 结果数据类型
        /// </summary>
        public Type KeyType { get; set; }
    }
    #endregion
}
