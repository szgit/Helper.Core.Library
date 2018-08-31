/*
 * 作用：利用 NPOI 读取 Excel 文档数据并导入 SqlServer 数据库。
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library
{
    public class ExcelToDataBaseHelper
    {
        #region 私有属性常量
        private const string TextDocumentWriteFailedException = "创建 Text 文档失败！";
        #endregion

        #region 对外公开方法
        /// <summary>
        /// Excel 数据批量导入 SqlServer
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="excelPath">Excel 路径</param>
        /// <param name="sheetName">Sheet 表单名称</param>
        /// <param name="tableName">数据库表名称</param>
        /// <param name="excelPropertyMatch">Excel 属性匹配，例：new { ID = "编号"}</param>
        /// <param name="txtPropertyMatch">Txt 属性匹配，例：new { ID = "编号"}，当实体类型属性与数据库字段顺序不同时使用</param>
        /// <param name="headerIndex">表头起始索引，默认值：0，表示第一行是表头数据，与 dataIndex 相同时，表示 Excel 无表头</param>
        /// <param name="dataIndex">数据行起始索引，默认值：1，表示数据从第二行开始</param>
        /// <param name="primaryKey">主键标识，如果未指定，则表示第一列是主键</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static bool BatchImport<T>(string excelPath, string sheetName, string tableName, object excelPropertyMatch = null, object txtPropertyMatch = null, int headerIndex = 0, int dataIndex = 1, string primaryKey = "", ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            string txtPath = "";
            try
            {
                // 从 Excel 表中获取数据
                List<T> dataList = ExcelHelper.ToEntityList<T>(excelPath, sheetName, excelPropertyMatch, headerIndex, dataIndex, primaryKey, reflectionType);
                // 根据 Excel 路径设置 Txt 路径
                txtPath = System.IO.Path.GetDirectoryName(excelPath) + "\\" + System.Guid.NewGuid().ToString() + ".txt";
                // 生成 Txt 文件
                bool txtStatus = TxtHelper.ToTxt<T>(txtPath, dataList, ",", System.Text.Encoding.GetEncoding("GBK"), TxtTypeEnum.Normal, txtPropertyMatch);
                // 如果生成 Txt 失败
                if (!txtStatus) throw new Exception(TextDocumentWriteFailedException);
                // Txt 导入数据
                return BatchImport(tableName, txtPath, 1);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (!string.IsNullOrEmpty(txtPath)) System.IO.File.Delete(txtPath);
            }
        }
        /// <summary>
        /// Txt/Csv 数据批量导入 SqlServer
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="rowIndex">数据起始索引，默认值：1</param>
        /// <param name="splitChar">数据分隔符，默认值：逗号（,）</param>
        /// <param name="lineSplitChar">行分隔符，默认值：\n</param>
        /// <returns></returns>
        public static bool BatchImport(string tableName, string filePath, int rowIndex = 1, string splitChar = ",", string lineSplitChar = "\n")
        {
            try
            {
                // 批量插入语句
                string commandText = string.Format("bulk insert {0} from '{1}' with (FIRSTROW = {2}, FIELDTERMINATOR = '{3}' , ROWTERMINATOR = '{4}')", tableName, filePath, rowIndex, splitChar, lineSplitChar);
                return DataBaseHelper.ExecuteNonQuery(commandText) > 0;
            }
            catch
            {
                throw;
            }
        }
        #endregion
    }
}
