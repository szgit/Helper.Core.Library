/*
 * 作用：利用 NPOI 根据模板生成 Excel 文档。
 * 以 $ 前缀：通过匿名类型替换数据
 * 以 # 前缀：通过实体属性值替换数据
 * 以 &= 前缀：公式计算
 * 以 i 后缀：行数，会根据行索引进行替换
 * 以 _dataBegin 后缀：数据行开始，计算过程中会替换
 * 以 _dataEnd 后缀：数据行结束，计算过程中会替换
 * */
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library
{
    public class ExcelTemplateHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 按模板生成 Excel 文档
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="templatePath">模板路径</param>
        /// <param name="templateSheetName">模板表单名称</param>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="dataMatchList">数据匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="dataList">数据列表</param>
        /// <param name="reflectionType">反射类型</param>
        public static void ToExcel<T>(string templatePath, string templateSheetName, string excelPath, object dataMatchList, List<T> dataList, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            Dictionary<string, object> propertyDict = CommonHelper.GetParameterDict(dataMatchList);

            FileStream fileStream = null;
            IWorkbook workbook = null;
            try
            {
                using (fileStream = new FileStream(templatePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    workbook = ExcelHelper.ExecuteIWorkBookGet(templatePath, fileStream);
                    if (workbook == null) throw new Exception(ExcelHelper.ExcelWorkbookNullException);
                }

                Dictionary<int, string> propertyMapperDict = new Dictionary<int, string>();
                List<ExcelTemplateFormulaItem> formulaItemList = new List<ExcelTemplateFormulaItem>();

                int lastRowIndex = 0;
                int insertRowIndex = -1;

                ISheet sheet = workbook.GetSheet(templateSheetName);
                if (sheet != null)
                {
                    lastRowIndex = sheet.LastRowNum;
                    for (int rowIndex = 0; rowIndex <= lastRowIndex; rowIndex++)
                    {
                        IRow iRow = sheet.GetRow(rowIndex);
                        if (iRow != null)
                        {
                            for (int colIndex = 0; colIndex <= iRow.LastCellNum; colIndex++)
                            {
                                ICell iCell = iRow.GetCell(colIndex);
                                if (iCell != null && !string.IsNullOrEmpty(iCell.ToString()))
                                {
                                    string cellText = iCell.ToString();
                                    if (cellText.StartsWith("$"))
                                    {
                                        cellText = cellText.TrimStart(new char[] { '$' });
                                        if (propertyDict != null && propertyDict.ContainsKey(cellText))
                                        {
                                            iCell.SetCellValue(propertyDict[cellText].ToString());
                                        }
                                    }
                                    else if (cellText.StartsWith("#"))
                                    {
                                        if (insertRowIndex == -1) insertRowIndex = rowIndex;

                                        cellText = cellText.TrimStart(new char[] { '#' });
                                        propertyMapperDict.Add(colIndex, cellText);
                                    }
                                    else if (cellText.StartsWith("&="))
                                    {
                                        if (rowIndex != insertRowIndex)
                                        {
                                            formulaItemList.Add(new ExcelTemplateFormulaItem() { Cell = iCell, FormulaText = cellText });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (propertyMapperDict != null && propertyMapperDict.Count > 0 && insertRowIndex != -1)
                {
                    if (dataList != null && dataList.Count > 0)
                    {
                        dynamic propertyGetDict = null;
                        if (reflectionType != ReflectionTypeEnum.Original) propertyGetDict = ReflectionExtendHelper.PropertyGetCallDict<T>(reflectionType);

                        CopyRow(sheet, insertRowIndex, dataList, propertyMapperDict, propertyGetDict);

                        if (formulaItemList != null && formulaItemList.Count > 0)
                        {
                            foreach (ExcelTemplateFormulaItem formulaItem in formulaItemList)
                            {
                                formulaItem.FormulaText = formulaItem.FormulaText.TrimStart(new char[] { '&', '=' });
                                formulaItem.FormulaText = formulaItem.FormulaText.Replace("_dataBegin", (insertRowIndex + 1).ToString());
                                formulaItem.FormulaText = formulaItem.FormulaText.Replace("_dataEnd", (insertRowIndex + dataList.Count).ToString());

                                formulaItem.Cell.SetCellFormula(formulaItem.FormulaText);
                                formulaItem.Cell.SetCellType(CellType.Formula);
                            }
                        }
                    }
                }

                sheet.ForceFormulaRecalculation = true;

                if (System.IO.File.Exists(excelPath)) System.IO.File.Delete(excelPath);
                using (fileStream = new FileStream(excelPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    workbook.Write(fileStream);
                }
                workbook.Close();
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
        #endregion

        #region 逻辑处理私有函数
        private static void CopyRow<T>(ISheet sheet, int templateRowIndex, List<T> dataList, Dictionary<int, string> propertyMapperDict, dynamic propertyGetDict) where T : class
        {
            if (dataList == null || dataList.Count == 0) return;

            IRow templateDataRow = sheet.GetRow(templateRowIndex);
            if (templateDataRow == null) return;

            List<ExcelMergeCell> mergeCellList = new List<ExcelMergeCell>();
            for (int i = 0; i < sheet.NumMergedRegions; i++)
            {
                CellRangeAddress cellRangeAddress = sheet.GetMergedRegion(i);
                if (cellRangeAddress.FirstRow == templateDataRow.RowNum)
                {
                    mergeCellList.Add(new ExcelMergeCell() { BeginColumn = cellRangeAddress.FirstColumn, EndColumn = cellRangeAddress.LastColumn });
                }
            }

            int templateDataCellCount = templateDataRow.Cells.Count;
            sheet.ShiftRows(templateRowIndex, //开始行
                            sheet.LastRowNum, //结束行
                            dataList.Count - 1, //插入行总数
                            true,        //是否复制行高
                            false        //是否重置行高
                            );

            IRow dataRow = null;
            ICell templateCell = null;
            ICell dataCell = null;

            int dataCount = dataList.Count;
            int dataLastIndex = templateRowIndex + dataCount;
            for (int i = templateRowIndex, j = 0; i < dataLastIndex && j < dataCount; i++, j++)
            {
                dataRow = sheet.CreateRow(i);
                dataRow.Height = templateDataRow.Height;//复制行高

                for (int colIndex = templateDataRow.FirstCellNum; colIndex < templateDataRow.LastCellNum; colIndex++)
                {
                    templateCell = templateDataRow.GetCell(colIndex);
                    if (templateCell == null) continue;

                    dataCell = dataRow.CreateCell(colIndex);
                    if (dataCell == null) dataCell = templateDataRow.CreateCell(colIndex);

                    dataCell.CellStyle = templateCell.CellStyle;//赋值单元格格式

                    string templateCellText = templateCell.ToString();
                    if (!templateCellText.StartsWith("&="))
                    {
                        dataCell.SetCellType(templateCell.CellType);

                        if (propertyMapperDict.ContainsKey(colIndex))
                        {
                            object propertyValue = null;
                            if (propertyGetDict != null && propertyGetDict.ContainsKey(propertyMapperDict[colIndex]))
                            {
                                propertyValue = propertyGetDict[propertyMapperDict[colIndex]](dataList[j]);
                            }
                            else
                            {
                                propertyValue = ReflectionHelper.GetPropertyValue(dataList[j], propertyMapperDict[colIndex]);
                            }
                            if (propertyValue != null)
                            {
                                Type propertyType = typeof(T).GetProperty(propertyMapperDict[colIndex]).PropertyType;
                                if (propertyType == typeof(int))
                                {
                                    dataCell.SetCellValue(int.Parse(propertyValue.ToString()));
                                }
                                else if (propertyType == typeof(float))
                                {
                                    dataCell.SetCellValue(float.Parse(propertyValue.ToString()));
                                }
                                else if (propertyType == typeof(DateTime))
                                {
                                    dataCell.SetCellValue(DateTime.Parse(propertyValue.ToString()));
                                }
                                else
                                {
                                    dataCell.SetCellValue(propertyValue.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        templateCellText = templateCellText.TrimStart(new char[] { '&', '=' });
                        templateCellText = templateCellText.Replace("i", (dataRow.RowNum + 1).ToString());

                        dataCell.SetCellType(CellType.Formula);
                        dataCell.SetCellFormula(templateCellText);
                    }
                }

                if (i != dataLastIndex - 1 && mergeCellList != null && mergeCellList.Count > 0)
                {
                    foreach (ExcelMergeCell excelMergeCell in mergeCellList)
                    {
                        CellRangeAddress cellRangeAddress = new CellRangeAddress(dataRow.RowNum, dataRow.RowNum, excelMergeCell.BeginColumn, excelMergeCell.EndColumn);
                        sheet.AddMergedRegion(cellRangeAddress);
                    }
                }
            }
        }
        #endregion
    }

    #region 逻辑处理辅助类
    internal class ExcelTemplateFormulaItem
    {
        public ICell Cell { get; set; }
        public string FormulaText { get; set; }
    }
    #endregion
}
