/*
 * 作用：数据库增/删/改/查操作，SqlServer 支持批量导入以及分页查询。
 * */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Reflection;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    /// <summary>
    /// 数据库类型
    /// </summary>
    public class DataBaseTypeEnum
    {
        /// <summary>
        /// Sql
        /// </summary>
        public const string Sql = "sql";
        /// <summary>
        /// MySql
        /// </summary>
        public const string MySql = "mysql";
    }
    /// <summary>
    /// 数据操作类型（用在 Transaction 开头的事务函数中）
    /// </summary>
    public class DataBaseExecuteTypeEnum
    {
        public const string ExecuteNonQuery = "ExecuteNonQuery";
        public const string ExecuteScalar = "ExecuteScalar";
        public const string ToEntityList = "ToEntityList";
        public const string ToEntity = "ToEntity";
    }
    /// <summary>
    /// 数据库分页处理所需字段
    /// </summary>
    public class DataBaseParameterEnum
    {
        /// <summary>
        /// 查询语句，表连接查询时，主表用 T 代替
        /// </summary>
        public const string FieldSql = "FieldSql";
        /// <summary>
        /// 查询语句，值为空时，与 FieldSql 字段相同
        /// </summary>
        public const string Field = "Field";
        /// <summary>
        /// 表名称
        /// </summary>
        public const string TableName = "TableName";
        /// <summary>
        /// 主键名称
        /// </summary>
        public const string PrimaryKey = "PrimaryKey";
        /// <summary>
        /// 页索引，从 1 开始
        /// </summary>
        public const string PageIndex = "PageIndex";
        /// <summary>
        /// 页大小
        /// </summary>
        public const string PageSize = "PageSize";
        /// <summary>
        /// Where 语句
        /// </summary>
        public const string WhereSql = "WhereSql";
        /// <summary>
        /// Order 语句
        /// </summary>
        public const string OrderSql = "OrderSql";
    }
    #endregion

    public class DataBaseHelper
    {
        #region 私有属性常量

        private static readonly object lockItem = new object(); 
        private static readonly Dictionary<string, Dictionary<PropertyInfo, string>> PropertyAttributeDict = new Dictionary<string, Dictionary<PropertyInfo, string>>();

        #region 异常消息
        private const string PaginationNotSupportException = "此函数目前不支持 Oracle 数据库！";
        private const string BatchImportException = "此函数目前只支持 Sql Server 数据库！";
        #endregion

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private static string ConnectionString = "";
        /// <summary>
        /// 数据库类型
        /// </summary>
        private static string DataBaseType = null;
        #endregion

        #region 对外公开方法

        #region 初始化数据库连接
        /// <summary>
        /// 初始化数据库连接字符串和数据库类型
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="dataBaseEnum">数据库类型</param>
        public static void InitConnectionString(string connectionString, string dataBaseType = DataBaseTypeEnum.Sql)
        {
            ConnectionString = connectionString;
            DataBaseType = dataBaseType;
        }
        #endregion

        #region ExecuteNonQuery
        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string commandText, object parameterList = null, CommandType commandType = CommandType.Text)
        {
            return ExecuteNonQuery(null, null, commandText, parameterList, commandType);
        }
        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataBaseType">数据库类型</param>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string connectionString, string dataBaseType, string commandText, object parameterList = null, CommandType commandType = CommandType.Text)
        {
            Dictionary<string, object> parameterDict = CommonHelper.GetParameterDict(parameterList);

            int result = 0;
            ExecuteCommand(connectionString, dataBaseType, commandText, parameterDict, null, commandType, (DbCommand command) =>
            {
                result = command.ExecuteNonQuery();
            });
            return result;
        }
        /// <summary>
        /// ExecuteNonQuery（事务）
        /// </summary>
        /// <param name="transactionCommandList">DataBaseTransactionItem 列表</param>
        /// <returns></returns>
        public static int TransactionNonQuery(List<DataBaseTransactionItem> transactionCommandList)
        {
            return TransactionNonQuery(null, null, transactionCommandList);
        }
        /// <summary>
        /// ExecuteNonQuery（事务）
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataBaseType">数据库类型</param>
        /// <param name="transactionCommandList">DataBaseTransactionItem 列表</param>
        /// <returns></returns>
        public static int TransactionNonQuery(string connectionString, string dataBaseType, List<DataBaseTransactionItem> transactionCommandList)
        {
            return (int)ExecuteTransaction<DataBaseTransactionItem>(connectionString, dataBaseType, transactionCommandList, ReflectionTypeEnum.Expression);
        }
        #endregion

        #region ExecuteScalar<T>
        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <typeparam name="T">基类类型，例：int</typeparam>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        /// <returns></returns>
        public static dynamic ExecuteScalar<T>(string commandText, object parameterList = null, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalar<T>(null, null, commandText, parameterList, commandType);
        }
        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <typeparam name="T">基类类型，例：int</typeparam>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataBaseType">数据库类型</param>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        /// <returns></returns>
        public static dynamic ExecuteScalar<T>(string connectionString, string dataBaseType, string commandText, object parameterList = null, CommandType commandType = CommandType.Text)
        {
            Dictionary<string, object> parameterDict = CommonHelper.GetParameterDict(parameterList);

            object result = null;
            ExecuteCommand(connectionString, dataBaseType, commandText, parameterDict, null, commandType, (DbCommand command) =>
            {
                result = command.ExecuteScalar();
            });
            if (result == null) return default(T);
            return (T)Convert.ChangeType(result, typeof(T));
        }
        /// <summary>
        /// ExecuteScalar（事务）
        /// </summary>
        /// <typeparam name="T">基类类型，例：int</typeparam>
        /// <param name="transactionCommandList">DataBaseTransactionItem 列表</param>
        /// <returns></returns>
        public static dynamic TransactionScalar<T>(List<DataBaseTransactionItem> transactionCommandList)
        {
            return TransactionScalar<T>(null, null, transactionCommandList);
        }
        /// <summary>
        /// ExecuteScalar（事务）
        /// </summary>
        /// <typeparam name="T">基类类型，例：int</typeparam>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataBaseType">数据库类型</param>
        /// <param name="transactionCommandList">DataBaseTransactionItem 列表</param>
        /// <returns></returns>
        public static dynamic TransactionScalar<T>(string connectionString, string dataBaseType, List<DataBaseTransactionItem> transactionCommandList)
        {
            object result = ExecuteTransaction<DataBaseTransactionItem>(connectionString, dataBaseType, transactionCommandList);
            return (T)Convert.ChangeType(result, typeof(T));
        }
        #endregion

        #region ExecuteDataReader
        /// <summary>
        /// ExecuteDataReader
        /// </summary>
        /// <param name="callback">DbDataReader 处理函数</param>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        public static void ExecuteDataReader(Action<DbDataReader> callback, string commandText, object parameterList = null, CommandType commandType = CommandType.Text)
        {
            ExecuteDataReader(null, null, callback, commandText, parameterList, commandType);
        }
        /// <summary>
        /// ExecuteDataReader
        /// </summary>
        /// <param name="callback">DbDataReader 处理函数</param>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="pageCount">页总数，输出</param>
        /// <param name="totalCount">数据总数，输出</param>
        /// <param name="parameterPageCountName">页总数参数名称，例如：PageCount</param>
        /// <param name="parameterTotalCountName">数据总数参数名称，例如：TotalCount</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        public static void ExecuteDataReader(Action<DbDataReader> callback, string commandText, object parameterList, ref int pageCount, ref int totalCount, string parameterPageCountName = "PageCount", string parameterTotalCountName = "TotalCount", CommandType commandType = CommandType.Text)
        {
            ExecuteDataReader(null, null, callback, commandText, parameterList, ref pageCount, ref totalCount, parameterPageCountName, parameterTotalCountName, commandType);
        }
        /// <summary>
        /// ExecuteDataReader
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataBaseType">数据库类型</param>
        /// <param name="callback">DbDataReader 处理函数</param>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        public static void ExecuteDataReader(string connectionString, string dataBaseType, Action<DbDataReader> callback, string commandText, object parameterList = null, CommandType commandType = CommandType.Text)
        {
            Dictionary<string, object> parameterDict = CommonHelper.GetParameterDict(parameterList);

            ExecuteCommand(connectionString, dataBaseType, commandText, parameterDict, null, commandType, (DbCommand command) =>
            {
                ExecuteDataReader(callback, command);
            });
        }
        /// <summary>
        /// ExecuteDataReader
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataBaseType">数据库类型</param>
        /// <param name="callback">DbDataReader 处理函数</param>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="pageCount">页总数，输出</param>
        /// <param name="totalCount">数据总数，输出</param>
        /// <param name="parameterPageCountName">页总数参数名称，例如：PageCount</param>
        /// <param name="parameterTotalCountName">数据总数参数名称，例如：TotalCount</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        public static void ExecuteDataReader(string connectionString, string dataBaseType, Action<DbDataReader> callback, string commandText, object parameterList, ref int pageCount, ref int totalCount, string parameterPageCountName = "PageCount", string parameterTotalCountName = "TotalCount", CommandType commandType = CommandType.Text)
        {
            Dictionary<string, object> parameterDict = CommonHelper.GetParameterDict(parameterList);

            Dictionary<string, object> outParameterList = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(parameterPageCountName)) outParameterList.Add("@" + parameterPageCountName, 0);
            if (!string.IsNullOrEmpty(parameterTotalCountName)) outParameterList.Add("@" + parameterTotalCountName, 0);

            if (string.IsNullOrEmpty(commandText) && DataBaseType == DataBaseTypeEnum.Sql)
            {
                commandText = SqlDataBaseItem.PaginationSql;
            }

            int commandPageCount = 0;
            int commandTotalCount = 0;

            ExecuteCommand(connectionString, dataBaseType, commandText, parameterDict, outParameterList, commandType, (DbCommand command) =>
            {
                ExecuteDataReader(callback, command);

                if (!string.IsNullOrEmpty(parameterPageCountName)) commandPageCount = (int)command.Parameters["@" + parameterPageCountName].Value;
                if (!string.IsNullOrEmpty(parameterTotalCountName)) commandTotalCount = (int)command.Parameters["@" + parameterTotalCountName].Value;
            });

            pageCount = commandPageCount;
            totalCount = commandTotalCount;
        }
        #endregion

        #region ToEntityList<T>
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(string commandText, object parameterList = null, object propertyMatchList = null, CommandType commandType = CommandType.Text, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            return ToEntityList<T>(null, null, commandText, parameterList, propertyMatchList, commandType, reflectionType);
        }
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="pageCount">页总数，输出</param>
        /// <param name="totalCount">数据总数，输出</param>
        /// <param name="propertyMatchList">属性匹配，，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="parameterPageCountName">页总数参数名称，例如：PageCount</param>
        /// <param name="parameterTotalCountName">数据总数参数名称，例如：TotalCount</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(string commandText, object parameterList, ref int pageCount, ref int totalCount, object propertyMatchList, string parameterPageCountName = "PageCount", string parameterTotalCountName = "TotalCount", CommandType commandType = CommandType.Text, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            return ToEntityList<T>(null, null, commandText, parameterList, ref pageCount, ref totalCount, propertyMatchList, parameterPageCountName, parameterTotalCountName, commandType, reflectionType);
        }
        /// <summary>
        /// 返回实体数据列表（事务）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="transactionCommandList">DataBaseTransactionItem 列表</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> TransactionEntityList<T>(List<DataBaseTransactionItem> transactionCommandList, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            return TransactionEntityList<T>(null, null, transactionCommandList, reflectionType);
        }
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataBaseType">数据库类型</param>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(string connectionString, string dataBaseType, string commandText, object parameterList = null, object propertyMatchList = null, CommandType commandType = CommandType.Text, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            Dictionary<string, object> parameterDict = CommonHelper.GetParameterDict(parameterList);
            return ReturnEntityList<T>(connectionString, dataBaseType, commandText, parameterDict, propertyMatchList, commandType, reflectionType);
        }
        /// <summary>
        /// 返回实体数据列表（事务）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataBaseType">数据库类型</param>
        /// <param name="transactionCommandList">DataBaseTransactionItem 列表</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> TransactionEntityList<T>(string connectionString, string dataBaseType, List<DataBaseTransactionItem> transactionCommandList, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            return (List<T>)ExecuteTransaction<T>(connectionString, dataBaseType, transactionCommandList, reflectionType);
        }
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataBaseType">数据库类型</param>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="pageCount">页总数，输出</param>
        /// <param name="totalCount">数据总数，输出</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="parameterPageCountName">页总数参数名称，例如：PageCount</param>
        /// <param name="parameterTotalCountName">数据总数参数名称，例如：TotalCount</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(string connectionString, string dataBaseType, string commandText, object parameterList, ref int pageCount, ref int totalCount, object propertyMatchList, string parameterPageCountName = "PageCount", string parameterTotalCountName = "TotalCount", CommandType commandType = CommandType.Text, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            Dictionary<string, object> parameterDict = CommonHelper.GetParameterDict(parameterList);

            return ReturnEntityList<T>(connectionString, dataBaseType, commandText, parameterDict, ref pageCount, ref totalCount, propertyMatchList, parameterPageCountName, parameterTotalCountName, commandType, reflectionType);
        }
        #endregion

        #region ToEntity<T>
        /// <summary>
        /// 返回实体数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static T ToEntity<T>(string commandText, object parameterList = null, object propertyMatchList = null, CommandType commandType = CommandType.Text, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            return ToEntity<T>(null, null, commandText, parameterList, propertyMatchList, commandType, reflectionType);
        }
        /// <summary>
        /// 返回实体数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataBaseType">数据库类型</param>
        /// <param name="commandText">Sql 语句或者存储过程名称</param>
        /// <param name="parameterList">参数列表，new {} 或 Dictionary&lt;string, object&gt;</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="commandType">CommandType 枚举类型</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static T ToEntity<T>(string connectionString, string dataBaseType, string commandText, object parameterList = null, object propertyMatchList = null, CommandType commandType = CommandType.Text, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<T> dataList = ToEntityList<T>(connectionString, dataBaseType, commandText, parameterList, propertyMatchList, commandType, reflectionType);
            if (dataList.Count > 0) return dataList[0];
            return null;
        }
        /// <summary>
        /// 返回实体数据（事务）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="transactionCommandList">DataBaseTransactionItem 列表</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static T TransactionEntity<T>(List<DataBaseTransactionItem> transactionCommandList, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            return TransactionEntity<T>(null, null, transactionCommandList, reflectionType);
        }
        /// <summary>
        /// 返回实体数据（事务）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dataBaseType">数据库类型</param>
        /// <param name="transactionCommandList">DataBaseTransactionItem 列表</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static T TransactionEntity<T>(string connectionString, string dataBaseType, List<DataBaseTransactionItem> transactionCommandList, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<T> dataList = TransactionEntityList<T>(connectionString, dataBaseType, transactionCommandList, reflectionType);
            if (dataList.Count > 0) return dataList[0];
            return null;
        }
        #endregion

        #region 批量导入数据
        /// <summary>
        /// 实体数据列表批量导入
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="tableName">数据库表名称</param>
        /// <param name="dataList">实体类型数据列表</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="propertyList">属性列表，如果指定，则按指定属性列表生成 DataTable 数据</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static bool EntityListBatchImport<T>(string tableName, List<T> dataList, object propertyMatchList = null, string[] propertyList = null, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            return EntityListBatchImport<T>(null, tableName, dataList, propertyMatchList, propertyList, propertyContain, reflectionType);
        }
        /// <summary>
        /// DataTable 数据批量导入
        /// </summary>
        /// <param name="tableName">数据库表名称</param>
        /// <param name="dataTable">DataTable 数据</param>
        /// <returns></returns>
        public static bool DataTableBatchImport(string tableName, DataTable dataTable)
        {
            return DataTableBatchImport(null, tableName, dataTable);
        }
        /// <summary>
        /// 实体数据列表批量导入
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connectionString"></param>
        /// <param name="tableName">数据库表名称</param>
        /// <param name="dataList">实体类型数据列表</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="propertyList">属性列表，如果指定，则按指定属性列表生成 DataTable 数据</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static bool EntityListBatchImport<T>(string connectionString, string tableName, List<T> dataList, object propertyMatchList = null, string[] propertyList = null, bool propertyContain = true, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            DataTable dataTable = DataTableHelper.ToDataTable<T>(dataList, propertyMatchList, propertyList, propertyContain, reflectionType);
            if (dataTable != null) return DataTableBatchImport(connectionString, tableName, dataTable);
            return false;
        }
        /// <summary>
        /// DataTable 数据批量导入
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="tableName">数据库表名称</param>
        /// <param name="dataTable">DataTable 数据</param>
        /// <returns></returns>
        public static bool DataTableBatchImport(string connectionString, string tableName, DataTable dataTable)
        {
            if (string.IsNullOrEmpty(connectionString)) connectionString = ConnectionString;

            if (DataBaseType != DataBaseTypeEnum.Sql) throw new Exception(BatchImportException);
            DbConnection con = null;
            try
            {
                using (con = CreateDbConnection(connectionString))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction))
                    {
                        sqlBulkCopy.DestinationTableName = tableName;

                        int columnCount = dataTable.Columns.Count;
                        DataColumn dataColumn = null;

                        for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                        {
                            dataColumn = dataTable.Columns[columnIndex];
                            sqlBulkCopy.ColumnMappings.Add(dataColumn.ColumnName, dataColumn.ColumnName);
                        }

                        sqlBulkCopy.WriteToServer(dataTable);
                    }
                    return true;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (con != null) con.Close();
            }
        }
        #endregion

        #endregion

        #region 逻辑处理私有方法

        #region 创建 CURD 相关
        private static DbConnection CreateDbConnection(string connectionString = null, string dataBaseType = null)
        {
            if (string.IsNullOrEmpty(connectionString)) connectionString = ConnectionString;
            if (string.IsNullOrEmpty(dataBaseType)) dataBaseType = DataBaseType;

            DbConnection connection = null;
            switch (dataBaseType)
            {
                case DataBaseTypeEnum.MySql: connection = new MySqlConnection(connectionString); break;
                default: connection = new SqlConnection(connectionString); break;
            }
            if (connection.State != ConnectionState.Open) connection.Open();
            return connection;
        }
        private static DbCommand CreateDbCommand(DbConnection connection, DbCommand command, string commandText, Dictionary<string, object> parameterList, Dictionary<string, object> outParameterList, CommandType commandType = CommandType.Text)
        {
            if (command == null)
            {
                command = connection.CreateCommand();
                command.Connection = connection;
            }
            command.CommandText = commandText;
            command.CommandType = commandType;

            // 清空参数
            command.Parameters.Clear();

            if (parameterList != null && parameterList.Count > 0)
            {
                foreach (var dictItem in parameterList)
                {
                    CreateDbParameter(command, dictItem.Key, dictItem.Value, ParameterDirection.Input);
                }
            }
            if (outParameterList != null && outParameterList.Count > 0)
            {
                foreach (var dictItem in outParameterList)
                {
                    CreateDbParameter(command, dictItem.Key, dictItem.Value, ParameterDirection.Output);
                }
            }
            return command;
        }
        private static void CreateDbParameter(DbCommand command, string parameterName, object value, ParameterDirection direction)
        {
            parameterName = parameterName.Trim();
            if (!parameterName.StartsWith("@")) parameterName = "@" + parameterName;

            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value == null ? "" : value;
            parameter.Direction = direction;
            command.Parameters.Add(parameter);
        }
        #endregion

        private static List<T> ReturnEntityList<T>(string connectionString, string dataBaseType, string commandText, Dictionary<string, object> parameterList, object propertyMatchList, CommandType commandType = CommandType.Text, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<T> dataList = null;
            ExecuteCommand(connectionString, dataBaseType, commandText, parameterList, null, commandType, (DbCommand command) =>
            {
                dataList = DataReaderToEntityList<T>(command, propertyMatchList, reflectionType);
            });
            return dataList;
        }
        private static List<T> ReturnEntityList<T>(string connectionString, string dataBaseType, string commandText, Dictionary<string, object> parameterList, ref int pageCount, ref int totalCount, object propertyMatchList, string parameterPageCountName = "PageCount", string parameterTotalCountName = "TotalCount", CommandType commandType = CommandType.Text, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<T> dataList = null;

            Dictionary<string, object> outParameterList = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(parameterPageCountName)) outParameterList.Add("@" + parameterPageCountName, 0);
            if (!string.IsNullOrEmpty(parameterTotalCountName)) outParameterList.Add("@" + parameterTotalCountName, 0);

            if (string.IsNullOrEmpty(commandText) && DataBaseType == DataBaseTypeEnum.Sql)
            {
                commandText = SqlDataBaseItem.PaginationSql;
            }

            int commandPageCount = 0;
            int commandTotalCount = 0;

            ExecuteCommand(connectionString, dataBaseType, commandText, parameterList, outParameterList, commandType, (DbCommand command) =>
            {
                dataList = DataReaderToEntityList<T>(command, propertyMatchList, reflectionType);

                if (!string.IsNullOrEmpty(parameterPageCountName)) commandPageCount = (int)command.Parameters["@" + parameterPageCountName].Value;
                if (!string.IsNullOrEmpty(parameterTotalCountName)) commandTotalCount = (int)command.Parameters["@" + parameterTotalCountName].Value;
            });

            pageCount = commandPageCount;
            totalCount = commandTotalCount;

            return dataList;
        }

        private static void ExecuteCommand(string connectionString, string dataBaseType, string commandText, Dictionary<string, object> parameterList, Dictionary<string, object> outParameterList, CommandType commandType = CommandType.Text, Action<DbCommand> callback = null)
        {
            DbConnection con = null;
            DbCommand command = null;
            try
            {
                using (con = CreateDbConnection(connectionString, dataBaseType))
                {
                    command = CreateDbCommand(con, null, commandText, parameterList, outParameterList, commandType);
                    if (callback != null) callback(command);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (command != null) command.Dispose();
            }
        }
        private static object ExecuteTransaction<T>(string connectionString, string dataBaseType, List<DataBaseTransactionItem> transactionItemList, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            DbConnection con = null;
            DbCommand command = null;
            DbTransaction transaction = null;
            try
            {
                object result = null;
                using (con = CreateDbConnection(connectionString, dataBaseType))
                {
                    transaction = con.BeginTransaction();
                    // 记录输出参数信息
                    Dictionary<string, object> outputParamDict = new Dictionary<string, object>();
                    foreach (DataBaseTransactionItem commandItem in transactionItemList)
                    {
                        Dictionary<string, object> parameterDict = CommonHelper.GetParameterDict(commandItem.ParameterList);
                        // 如果需要使用之前的输出数据
                        if (commandItem.InputList != null && commandItem.InputList.Length > 0)
                        {
                            foreach (string inputParamItem in commandItem.InputList)
                            {
                                if (outputParamDict.ContainsKey(inputParamItem) && parameterDict.ContainsKey(inputParamItem))
                                {
                                    parameterDict[inputParamItem] = outputParamDict[inputParamItem];
                                }
                            }
                        }
                        command = CreateDbCommand(con, command, commandItem.CommandText, parameterDict, null, CommandType.Text);
                        command.Transaction = transaction;

                        if (commandItem.ExecuteType == DataBaseExecuteTypeEnum.ExecuteNonQuery)
                        {
                            result = command.ExecuteNonQuery();
                        }
                        else if (commandItem.ExecuteType == DataBaseExecuteTypeEnum.ExecuteScalar)
                        {
                            result = command.ExecuteScalar();
                        }
                        else if (commandItem.ExecuteType == DataBaseExecuteTypeEnum.ToEntityList)
                        {
                            result = DataReaderToEntityList<T>(command, commandItem.PropertyMatchList, reflectionType);
                        }
                        else if (commandItem.ExecuteType == DataBaseExecuteTypeEnum.ToEntity)
                        {
                            List<T> dataList = DataReaderToEntityList<T>(command, commandItem.PropertyMatchList, reflectionType);
                            if (dataList.Count > 0) result = dataList[0];
                        }
                        // 如果输出参数不为空，则记录输出值
                        if (!string.IsNullOrEmpty(commandItem.OutputName))
                        {
                            outputParamDict.Add(commandItem.OutputName, result);
                        }
                    }
                    try
                    {
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (command != null) command.Dispose();
            }
        }
        private static void ExecuteDataReader(Action<DbDataReader> callback, DbCommand command)
        {
            if (callback == null) return;
            using (DbDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (dataReader.Read())
                {
                    callback(dataReader);
                }
            }
        }
        private static List<T> DataReaderToEntityList<T>(DbCommand command, object propertyMatchList = null, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<T> dataList = new List<T>();
            
            bool initStatus = false;
            List<string> columnNameList = null;
            Dictionary<PropertyInfo, string> columnNameDict = null;

            using (DbDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                dynamic propertySetDict = null;
                if (reflectionType != ReflectionTypeEnum.Original) propertySetDict = ReflectionExtendHelper.PropertySetCallDict<T>(reflectionType);

                if (dataReader.Read())
                {
                    if (!initStatus)
                    {
                        columnNameList = new List<string>();
                        for (int index = 0; index < dataReader.FieldCount; index++) columnNameList.Add(dataReader.GetName(index));
                        columnNameDict = InitDbToEntityMapper<T>(dataReader, propertyMatchList);
                        initStatus = true;
                    }
                    dataList.Add(DataReaderToEntity<T>(dataReader, propertySetDict, columnNameList, columnNameDict));
                }
                while (dataReader.Read())
                {
                    dataList.Add(DataReaderToEntity<T>(dataReader, propertySetDict, columnNameList, columnNameDict));
                }
            }
            return dataList;
        }
        private static T DataReaderToEntity<T>(DbDataReader reader, dynamic propertySetDict, List<string> columnNameList, Dictionary<PropertyInfo, string> columnNameDict) where T : class, new()
        {
            T t = ReflectionGenericHelper.New<T>();
            foreach(var keyValueItem in columnNameDict)
            {
                if (columnNameList.IndexOf(keyValueItem.Value) >= 0)
                {
                    if (propertySetDict != null && propertySetDict.ContainsKey(keyValueItem.Key.Name))
                    {
                        ReflectionGenericHelper.SetPropertyValue(propertySetDict[keyValueItem.Key.Name], t, reader[keyValueItem.Value].ToString(), keyValueItem.Key);
                    }
                    else
                    {
                        ReflectionHelper.SetPropertyValue(t, reader[keyValueItem.Value].ToString(), keyValueItem.Key);
                    }
                }
            }
            return t;
        }
        private static Dictionary<PropertyInfo, string> InitDbToEntityMapper<T>(DbDataReader reader, object propertyMatchList = null) where T : class
        {
            Dictionary<PropertyInfo, string> resultDict = new Dictionary<PropertyInfo, string>();
            if (propertyMatchList != null)
            {
                Dictionary<string, object> propertyMatchDict = CommonHelper.GetParameterDict(propertyMatchList);
                ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
                {
                    string columnName = propertyInfo.Name;
                    object propertyValue = propertyMatchDict[propertyInfo.Name];
                    if (propertyValue != null) columnName = propertyValue.ToString();
                    resultDict.Add(propertyInfo, columnName);
                });
                return resultDict;
            }

            string key = typeof(T).FullName;
            if (PropertyAttributeDict.ContainsKey(key)) return PropertyAttributeDict[key];

            lock (lockItem)
            {
                ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
                {
                    string columnName = ReflectionExtendHelper.GetAttributeValue<DataBaseTAttribute>(typeof(T), propertyInfo, p => p.Name);
                    if (string.IsNullOrEmpty(columnName)) columnName = propertyInfo.Name;
                    resultDict.Add(propertyInfo, columnName);
                });
                if (!PropertyAttributeDict.ContainsKey(key))
                {
                    PropertyAttributeDict.Add(key, resultDict);
                }
                return resultDict;
            }
        }
        #endregion
    }

    #region 逻辑处理辅助特性
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DataBaseTAttribute : Attribute
    {
        private string name;

        /// <summary>
        /// 实体属性映射数据库列名
        /// </summary>
        /// <param name="name"></param>
        public DataBaseTAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// 实体属性所对应的数据库列名
        /// </summary>
        public string Name { get { return this.name; } }
    }
    #endregion

    #region 逻辑处理辅助类
    public class DataBaseTransactionItem
    {
        /// <summary>
        /// DataBaseExecuteTypeEnum
        /// </summary>
        public string ExecuteType { get; set; }
        /// <summary>
        /// SQL 语句
        /// </summary>
        public string CommandText { get; set; }
        /// <summary>
        /// 参数列表
        /// </summary>
        public object ParameterList { get; set; }
        /// <summary>
        /// 数据匹配
        /// </summary>
        public object PropertyMatchList { get; set; }
        /// <summary>
        /// 使用前一步的导出参数数据
        /// </summary>
        public string[] InputList { get; set; }
        /// <summary>
        /// 导出数据信息
        /// </summary>
        public string OutputName { get; set; }
    }

    #region SqlServer 分页查询语句
    internal class SqlDataBaseItem
    {
        public static string PaginationSql
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("declare @Sql varchar(max)\r\n");
                stringBuilder.Append("declare @Sql1 nvarchar(max)\r\n");


                stringBuilder.Append("if @Field = ''\r\n");
                stringBuilder.Append("begin\r\n");
                stringBuilder.Append("set @Field = @FieldSql\r\n");
                stringBuilder.Append("end\r\n");

                stringBuilder.Append("if @OrderSql = ''\r\n");
                stringBuilder.Append("begin\r\n");
                stringBuilder.Append("set @OrderSql = @PrimaryKey + ' desc '\r\n");
                stringBuilder.Append("end\r\n");

                stringBuilder.Append("if (@WhereSql<>'')\r\n");
                stringBuilder.Append("begin\r\n");
                stringBuilder.Append("set @Sql = 'select ' + @FieldSql + ' from (select ROW_NUMBER() over(order by ' + @OrderSql + ') AS RowNums,' + @Field + ' from ' + @TableName + ' with(nolock) where ' + @WhereSql + ') AS T ' + ' where RowNums between ' + Str((@PageIndex-1) * @PageSize + 1) + ' and ' + Str(@PageIndex * @PageSize) + ' order by ' + @OrderSql\r\n");
                stringBuilder.Append("set @Sql1 = N'Select @Count=Count(0) from ['+@TableName+'] with(nolock) where '+@WhereSql\r\n");
                stringBuilder.Append("end\r\n");
                stringBuilder.Append("else\r\n");
                stringBuilder.Append("begin\r\n");
                stringBuilder.Append("set @Sql = 'select ' + @FieldSql + ' from (select ROW_NUMBER() over(order by ' + @OrderSql + ') AS RowNums,' + @Field + ' from ' + @TableName + ' with(nolock)) AS T ' + ' where RowNums between ' + Str((@PageIndex-1) * @PageSize + 1) + ' and ' + Str(@PageIndex * @PageSize) + ' order by ' + @OrderSql\r\n");
                stringBuilder.Append("set @Sql1 = N'Select @Count=Count(0) from ['+@TableName+'] with(nolock)'\r\n");
                stringBuilder.Append("end\r\n");
                stringBuilder.Append("execute sp_executesql @Sql1, N'@Count int output',@Count=@TotalCount output\r\n");
                stringBuilder.Append("set @PageCount = ceiling(convert(float,@TotalCount)/@PageSize)\r\n");
                stringBuilder.Append("print(@Sql)");
                stringBuilder.Append("exec (@Sql)\r\n");

                return stringBuilder.ToString();
            }
        }
    }
    #endregion

    #endregion
}
