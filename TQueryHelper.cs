/*
 * 作用：通过 Lambda 表达式生成增/删/改/查 SQL 语句。
 * */
using Helper.Core.Library.QueryHelper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Helper.Core.Library
{
    public class TQueryHelper<T> where T : class
    {
        private TQueryHelperItem<T> helperItem;

        public TQueryHelper()
        {
            this.helperItem = new TQueryHelperItem<T>();
        }

        #region 对外公开方法

        #region 查询语句，入口点
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="top">Top 语句</param>
        /// <returns></returns>
        public TQueryHelper<T> Query(int top = 0)
        {
            this.helperItem.Init(top, false, TQueryHelperTypeEnum.QUERY_TYPE);
            return this;
        }

        /// <summary>
        /// 唯一查询
        /// </summary>
        /// <param name="top">Top 语句</param>
        /// <returns></returns>
        public TQueryHelper<T> Distinct(int top = 0)
        {
            this.helperItem.Init(top, true, TQueryHelperTypeEnum.QUERY_TYPE);
            return this;
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例： p=> new { ID = p.ID, p.Name }</param>
        /// <returns></returns>
        public TQueryHelper<T> Insert(Expression<Func<T, object>> lambda = null)
        {
            this.helperItem.Insert(lambda);
            return this;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例： p=> new { ID = p.ID, p.Name }</param>
        /// <returns></returns>
        public TQueryHelper<T> Update(Expression<Func<T, object>> lambda = null, string cacheKey = null)
        {
            this.helperItem.Update(lambda);
            return this;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public TQueryHelper<T> Delete()
        {
            this.helperItem.Delete();
            return this;
        }

        #endregion

        #region Select 语句
        /// <summary>
        /// 查询字段
        /// </summary>
        /// <param name="querySql">Sql 语句，多个字段用逗号分隔，例："IdentityID as ID, Name"</param>
        /// <param name="typeList">Sql 语句中字段匹配所需类型</param>
        /// <returns></returns>
        public TQueryHelper<T> Select(string querySql, params Type[] typeList)
        {
            if (string.IsNullOrEmpty(querySql)) return this;

            querySql = TQueryReflectionHelper.FormatQuerySql(querySql, typeList);

            this.helperItem.Select(typeof(T), querySql);
            return this;
        }

        /// <summary>
        /// 查询字段
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID 或 p=> new { ID = p.ID }</param>
        /// <returns></returns>
        public TQueryHelper<T> Select(Expression<Func<T, object>> lambda = null)
        {
            this.helperItem.Select(typeof(T), lambda);
            return this;
        }

        /// <summary>
        /// 查询字段
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID 或 p=> new { ID = p.ID }</param>
        /// <returns></returns>
        public TQueryHelper<T> Select<K>(Expression<Func<K, object>> lambda = null) where K : class
        {
            this.helperItem.Select(typeof(K), lambda);
            return this;
        }

        /// <summary>
        /// 查询字段
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="newFieldName">新的字段名称</param>
        /// <returns></returns>
        public TQueryHelper<T> Select<K>(TQueryHelper<K> helper, string newFieldName = null) where K : class
        {
            this.helperItem.Select<K>(helper, newFieldName);
            return this;
        }
        #endregion

        #region 聚合函数
        /// <summary>
        /// COUNT
        /// </summary>
        /// <param name="newFieldName">新的字段名</param>
        /// <returns></returns>
        public TQueryHelper<T> Count(string newFieldName = null)
        {
            this.helperItem.Aggregate(newFieldName, "COUNT");
            return this;
        }

        /// <summary>
        /// COUNT
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="newFieldName">新的字段名</param>
        /// <returns></returns>
        public TQueryHelper<T> Count(Expression<Func<T, object>> lambda, string newFieldName = null)
        {
            this.helperItem.Aggregate(lambda, newFieldName, "COUNT");
            return this;
        }

        /// <summary>
        /// COUNT
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="newFieldName">新的字段名</param>
        /// <returns></returns>
        public TQueryHelper<T> Count<K>(Expression<Func<K, object>> lambda, string newFieldName = null) where K : class
        {
            this.helperItem.Aggregate(lambda, newFieldName, "COUNT");
            return this;
        }

        /// <summary>
        /// MAX
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="newFieldName">新的字段名</param>
        /// <returns></returns>
        public TQueryHelper<T> Max(Expression<Func<T, object>> lambda, string newFieldName = null)
        {
            this.helperItem.Aggregate(lambda, newFieldName, "MAX");
            return this;
        }

        /// <summary>
        /// MAX
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="newFieldName">新的字段名</param>
        /// <returns></returns>
        public TQueryHelper<T> Max<K>(Expression<Func<K, object>> lambda, string newFieldName = null) where K : class
        {
            this.helperItem.Aggregate(lambda, newFieldName, "MAX");
            return this;
        }

        /// <summary>
        /// MIN
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="newFieldName">新的字段名</param>
        /// <returns></returns>
        public TQueryHelper<T> Min(Expression<Func<T, object>> lambda, string newFieldName = null)
        {
            this.helperItem.Aggregate(lambda, newFieldName, "MIN");
            return this;
        }

        /// <summary>
        /// MIN
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="newFieldName">新的字段名</param>
        /// <returns></returns>
        public TQueryHelper<T> Min<K>(Expression<Func<K, object>> lambda, string newFieldName = null) where K : class
        {
            this.helperItem.Aggregate(lambda, newFieldName, "MIN");
            return this;
        }

        /// <summary>
        /// SUM
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="newFieldName">新的字段名</param>
        /// <returns></returns>
        public TQueryHelper<T> Sum(Expression<Func<T, object>> lambda, string newFieldName = null)
        {
            this.helperItem.Aggregate(lambda, newFieldName, "SUM");
            return this;
        }

        /// <summary>
        /// SUM
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="newFieldName">新的字段名</param>
        /// <returns></returns>
        public TQueryHelper<T> Sum<K>(Expression<Func<K, object>> lambda, string newFieldName = null) where K : class
        {
            this.helperItem.Aggregate(lambda, newFieldName, "SUM");
            return this;
        }

        /// <summary>
        /// AVG
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="newFieldName">新的字段名</param>
        /// <returns></returns>
        public TQueryHelper<T> Avg(Expression<Func<T, object>> lambda, string newFieldName = null)
        {
            this.helperItem.Aggregate(lambda, newFieldName, "AVG");
            return this;
        }

        /// <summary>
        /// AVG
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="newFieldName">新的字段名</param>
        /// <returns></returns>
        public TQueryHelper<T> Avg<K>(Expression<Func<K, object>> lambda, string newFieldName = null) where K : class
        {
            this.helperItem.Aggregate(lambda, newFieldName, "AVG");
            return this;
        }
        #endregion

        #region Inner/Left/Right Join On And 语句

        #region Inner Join On
        /// <summary>
        /// 表内连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> InnerJoin<K>(Expression<Func<T, K, bool>> lambda) where K : class
        {
            this.helperItem.Join<K>(lambda);
            return this;
        }

        /// <summary>
        /// 表内连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> InnerJoin<K>(Expression<Func<K, object>> lambda) where K : class
        {
            this.helperItem.Join<K>(lambda);
            return this;
        }

        /// <summary>
        /// 表内连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> InnerJoin<K, P>(Expression<Func<K, P, bool>> lambda)
            where K : class
            where P : class
        {
            this.helperItem.Join<K, P>(lambda);
            return this;
        }

        /// <summary>
        /// 表内连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> InnerJoin<K, P>(Expression<Func<P, object>> lambda)
            where K : class
            where P : class
        {
            this.helperItem.Join<K, P>(lambda);
            return this;
        }
        #endregion

        #region Left Join On
        /// <summary>
        /// 表左连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> LeftJoin<K>(Expression<Func<T, K, bool>> lambda) where K : class
        {
            this.helperItem.Join<K>(lambda, "left");
            return this;
        }

        /// <summary>
        /// 表左连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> LeftJoin<K>(Expression<Func<K, object>> lambda) where K : class
        {
            this.helperItem.Join<K>(lambda);
            return this;
        }

        /// <summary>
        /// 表左连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> LeftJoin<K, P>(Expression<Func<K, P, bool>> lambda)
            where K : class
            where P : class
        {
            this.helperItem.Join<K, P>(lambda, "left");
            return this;
        }

        /// <summary>
        /// 表左连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> LeftJoin<K, P>(Expression<Func<P, object>> lambda)
            where K : class
            where P : class
        {
            this.helperItem.Join<K, P>(lambda, "left");
            return this;
        }
        #endregion

        #region Right Join On
        /// <summary>
        /// 表右连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> RightJoin<K>(Expression<Func<T, K, bool>> lambda) where K : class
        {
            this.helperItem.Join<K>(lambda, "right");
            return this;
        }

        /// <summary>
        /// 表右连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> RightJoin<K>(Expression<Func<K, object>> lambda) where K : class
        {
            this.helperItem.Join<K>(lambda, "right");
            return this;
        }

        /// <summary>
        /// 表右连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> RightJoin<K, P>(Expression<Func<K, P, bool>> lambda)
            where K : class
            where P : class
        {
            this.helperItem.Join<K, P>(lambda, "right");
            return this;
        }

        /// <summary>
        /// 表右连接
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <returns></returns>
        public TQueryHelper<T> RightJoin<K, P>(Expression<Func<P, object>> lambda)
            where K : class
            where P : class
        {
            this.helperItem.Join<K, P>(lambda, "right");
            return this;
        }
        #endregion

        #region Join On And
        /// <summary>
        /// Join And 条件
        /// </summary>
        /// <param name="querySql">Sql 语句，多个字段用逗号分隔，例：[TableName].[FieldID] = 1</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <param name="typeList">Sql 语句中字段匹配所需类型</param>
        /// <returns></returns>
        public TQueryHelper<T> JoinAnd(string querySql, bool isBreak = false, params Type[] typeList)
        {
            if (string.IsNullOrEmpty(querySql)) return this;

            querySql = TQueryReflectionHelper.FormatQuerySql(querySql, typeList);

            this.helperItem.JoinAnd(querySql, isBreak);
            return this;
        }

        /// <summary>
        /// Join And 条件
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=> p.ID == 1</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> JoinAnd(Expression<Func<T, bool>> lambda, bool isBreak = false)
        {
            this.helperItem.JoinAnd(lambda, isBreak);
            return this;
        }

        /// <summary>
        /// Join And 条件
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> JoinAnd<K>(Expression<Func<T, K, bool>> lambda, bool isBreak = false) where K : class
        {
            this.helperItem.JoinAnd(lambda, isBreak);
            return this;
        }

        /// <summary>
        /// Join And 条件
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> JoinAnd<K, P>(Expression<Func<K, P, bool>> lambda, bool isBreak = false)
            where K : class
            where P : class
        {
            this.helperItem.JoinAnd(lambda, isBreak);
            return this;
        }
        #endregion

        #endregion

        #region Where 语句
        /// <summary>
        /// Where 条件语句
        /// </summary>
        /// <param name="querySql">Sql 语句，多个字段用逗号分隔，例：[TableName].[FieldID] = 1</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <param name="typeList">Sql 语句中字段匹配所需类型</param>
        /// <returns></returns>
        public TQueryHelper<T> Where(string querySql, bool isBreak = false, params Type[] typeList)
        {
            if (string.IsNullOrEmpty(querySql)) return this;

            querySql = TQueryReflectionHelper.FormatQuerySql(querySql, typeList);

            this.helperItem.Where(querySql, isBreak);
            return this;
        }

        /// <summary>
        /// Where 条件语句
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=> p.ID == 1</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> Where(Expression<Func<T, bool>> lambda, bool isBreak = false)
        {
            this.helperItem.Where(lambda, null, isBreak);
            return this;
        }

        /// <summary>
        /// Where 条件语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> Where<K>(Expression<Func<T, K, bool>> lambda, bool isBreak = false) where K : class
        {
            this.helperItem.Where(lambda, null, isBreak);
            return this;
        }

        /// <summary>
        /// Where 条件语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> Where<K, P>(Expression<Func<K, P, bool>> lambda, bool isBreak = false)
            where K : class
            where P : class
        {
            this.helperItem.Where(lambda, null, isBreak);
            return this;
        }

        /// <summary>
        /// Where 条件语句，默认添加 and 前缀
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=> p.ID == 1</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> AndWhere(Expression<Func<T, bool>> lambda, bool isBreak = false)
        {
            this.helperItem.Where(lambda, "and", isBreak);
            return this;
        }

        /// <summary>
        /// Where 条件语句，默认添加 and 前缀
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> AndWhere<K>(Expression<Func<T, K, bool>> lambda, bool isBreak = false) where K : class
        {
            this.helperItem.Where(lambda, "and", isBreak);
            return this;
        }

        /// <summary>
        /// Where 条件语句，默认添加 and 前缀
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> AndWhere<K, P>(Expression<Func<K, P, bool>> lambda, bool isBreak = false)
            where K : class
            where P : class
        {
            this.helperItem.Where(lambda, "and", isBreak);
            return this;
        }

        /// <summary>
        /// Where 条件语句，默认添加 or 前缀
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=> p.ID == 1</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> OrWhere(Expression<Func<T, bool>> lambda, bool isBreak = false)
        {
            this.helperItem.Where(lambda, "or", isBreak);
            return this;
        }

        /// <summary>
        /// Where 条件语句，默认添加 or 前缀
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> OrWhere<K>(Expression<Func<T, K, bool>> lambda, bool isBreak = false) where K : class
        {
            this.helperItem.Where(lambda, "or", isBreak);
            return this;
        }

        /// <summary>
        /// Where 条件语句，默认添加 or 前缀
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> p.ID == y.ID</param>
        /// <param name="isBreak">是否块，为 True 时，会添加左右括号</param>
        /// <returns></returns>
        public TQueryHelper<T> OrWhere<K, P>(Expression<Func<K, P, bool>> lambda, bool isBreak = false)
            where K : class
            where P : class
        {
            this.helperItem.Where(lambda, "or", isBreak);
            return this;
        }
        #endregion

        #region Order By 语句
        /// <summary>
        /// Order 排序语句
        /// </summary>
        /// <param name="querySql">Sql 语句，多个字段用逗号分隔，例："ID asc,Name desc"</param>
        /// <param name="typeList">Sql 语句中字段匹配所需类型</param>
        /// <returns></returns>
        public TQueryHelper<T> Order(string querySql, params Type[] typeList)
        {
            if (string.IsNullOrEmpty(querySql)) return this;

            querySql = TQueryReflectionHelper.FormatQuerySql(querySql, typeList);

            this.helperItem.Order(typeof(T), querySql);
            return this;
        }

        /// <summary>
        /// Order 排序语句
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID 或 p=> new {p.ID, p.Title}</param>
        /// <returns></returns>
        public TQueryHelper<T> Order(Expression<Func<T, object>> lambda)
        {
            this.helperItem.Order(lambda, "asc");
            return this;
        }

        /// <summary>
        /// Order 排序语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID 或 p=> new {p.ID, p.Title}</param>
        /// <returns></returns>
        public TQueryHelper<T> Order<K>(Expression<Func<K, object>> lambda) where K : class
        {
            this.helperItem.Order(lambda, "asc");
            return this;
        }

        /// <summary>
        /// Order 排序语句，默认降序
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID 或 p=> new {p.ID, p.Title}</param>
        /// <returns></returns>
        public TQueryHelper<T> OrderDesc(Expression<Func<T, object>> lambda)
        {
            this.helperItem.Order(lambda, "desc");
            return this;
        }

        /// <summary>
        /// Order 排序语句，默认降序
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID 或 p=> new {p.ID, p.Title}</param>
        /// <returns></returns>
        public TQueryHelper<T> OrderDesc<K>(Expression<Func<K, object>> lambda) where K : class
        {
            this.helperItem.Order(lambda, "desc");
            return this;
        }
        #endregion

        #region Group By 语句
        /// <summary>
        /// Group 分组语句
        /// </summary>
        /// <param name="querySql">Sql 语句，多个字段用逗号分隔，例："ID asc,Name desc" 或 "[TableName].Field asc"</param>
        /// <param name="typeList">Sql 语句中字段匹配所需类型</param>
        /// <returns></returns>
        public TQueryHelper<T> Group(string querySql, params Type[] typeList)
        {
            if (string.IsNullOrEmpty(querySql)) return this;

            querySql = TQueryReflectionHelper.FormatQuerySql(querySql, typeList);

            this.helperItem.Group(typeof(T), querySql);
            return this;
        }

        /// <summary>
        /// Group 分组语句
        /// </summary>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID 或 p=> new {p.ID, p.Title}</param>
        /// <returns></returns>
        public TQueryHelper<T> Group(Expression<Func<T, object>> lambda)
        {
            this.helperItem.Group(lambda);
            return this;
        }

        /// <summary>
        /// Group 分组语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID 或 p=> new {p.ID, p.Title}</param>
        /// <returns></returns>
        public TQueryHelper<T> Group<K>(Expression<Func<K, object>> lambda) where K : class
        {
            this.helperItem.Group(lambda);
            return this;
        }
        #endregion

        #region In 语句
        /// <summary>
        /// From In 子查询语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="helper"></param>
        /// <returns></returns>
        public TQueryHelper<T> FromIn<K>(TQueryHelper<K> helper, string newTableName = null) where K : class
        {
            this.helperItem.FromIn<K>(helper, newTableName);
            return this;
        }

        /// <summary>
        /// Exists 子查询语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="helper"></param>
        /// <returns></returns>
        public TQueryHelper<T> Exists<K>(TQueryHelper<K> helper) where K : class
        {
            this.helperItem.Exists<K>(helper);
            return this;
        }

        /// <summary>
        /// NotExists 子查询语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="helper"></param>
        /// <returns></returns>
        public TQueryHelper<T> NotExists<K>(TQueryHelper<K> helper) where K : class
        {
            this.helperItem.Exists<K>(helper);
            return this;
        }

        /// <summary>
        /// Where Field In 子查询语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="fieldName">字段名</param>
        /// <param name="fieldValueList">数据列表，例：new List&lt;int&gt;(){1, 2, 3}</param>
        /// <param name="directUseFieldName">是否直接使用 fieldName</param>
        /// <returns></returns>
        public TQueryHelper<T> WhereIn<K>(string fieldName, IList<K> fieldValueList, bool directUseFieldName = false)
        {
            this.helperItem.WhereIn<K>(fieldName, "in", fieldValueList, directUseFieldName);
            return this;
        }

        /// <summary>
        /// Where Field In 子查询语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="fieldName">字段名</param>
        /// <param name="helper"></param>
        /// <param name="directUseFieldName">是否直接使用 fieldName</param>
        /// <returns></returns>
        public TQueryHelper<T> WhereIn<K>(string fieldName, TQueryHelper<K> helper, bool directUseFieldName = false) where K : class
        {
            this.helperItem.WhereIn<K>(fieldName, "in", helper, directUseFieldName);
            return this;
        }

        /// <summary>
        /// Where Field In 子查询语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="fieldValueList">数据列表，例：new List&lt;int&gt;(){1, 2, 3}</param>
        /// <returns></returns>
        public TQueryHelper<T> WhereIn<K>(Expression<Func<T, object>> lambda, IList<K> fieldValueList)
        {
            this.helperItem.WhereIn<K>(lambda, "in", fieldValueList);
            return this;
        }

        /// <summary>
        /// Where Field In 子查询语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：p=>p.ID</param>
        /// <param name="helper"></param>
        /// <returns></returns>
        public TQueryHelper<T> WhereIn<K>(Expression<Func<T, object>> lambda, TQueryHelper<K> helper) where K : class
        {
            this.helperItem.WhereIn<K>(lambda, "in", helper);
            return this;
        }

        /// <summary>
        /// Where Field In 子查询语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> y.ID</param>
        /// <param name="fieldValueList">数据列表，例：new List&lt;int&gt;(){1, 2, 3}</param>
        /// <returns></returns>
        public TQueryHelper<T> WhereIn<K, P>(Expression<Func<K, object>> lambda, IList<P> fieldValueList) where K : class
        {
            this.helperItem.WhereIn<K, P>(lambda, "in", fieldValueList);
            return this;
        }

        /// <summary>
        /// Where Field In 子查询语句
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="lambda">Lambda 表达式，例：(p, y)=> y.ID</param>
        /// <param name="helper"></param>
        /// <returns></returns>
        public TQueryHelper<T> WhereIn<K, P>(Expression<Func<K, P, object>> lambda, TQueryHelper<K> helper)
            where K : class
            where P : class
        {
            this.helperItem.WhereIn<K, P>(lambda, "in", helper);
            return this;
        }
        #endregion

        #region Union/Union All 语句
        /// <summary>
        /// Union 联合查询
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="helper"></param>
        /// <returns></returns>
        public TQueryHelper<T> Union<K>(TQueryHelper<K> helper) where K : class
        {
            this.helperItem.Union<K>(helper);
            return this;
        }
        #endregion

        #region (/) 括号
        /// <summary>
        /// 生成左括号
        /// </summary>
        /// <returns></returns>
        public TQueryHelper<T> BreakLeft()
        {
            this.helperItem.BreakLeft();
            return this;
        }

        /// <summary>
        /// 生成右括号
        /// </summary>
        /// <returns></returns>
        public TQueryHelper<T> BreakRight()
        {
            this.helperItem.BreakRight();
            return this;
        }
        #endregion

        #region 生成查询语句，出口点
        /// <summary>
        /// 生成查询语句
        /// </summary>
        /// <param name="withNoLock">是否添加 with(nolock) 语句</param>
        /// <returns></returns>
        public string ToSql(bool withNoLock = true)
        {
            return this.helperItem.ToSql(withNoLock);
        }
        #endregion

        #endregion
    }
}
