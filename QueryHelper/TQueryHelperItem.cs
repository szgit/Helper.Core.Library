using Helper.Core.Library.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.QueryHelper
{
    internal class TQueryHelperItem<T> where T : class
    {
        private string queryType = null;
        private List<ExpressionKeyValueItem> queryList;
        private string queryTableName = null;
        private string fromInTableName = null;
        private string fromInNewTableName = null;
        private int queryTop = 0;
        private bool distinct = false;
        private Dictionary<string, Type> joinTableDict;

        public void Init(int top, bool distinct, string queryType = TQueryHelperTypeEnum.QUERY_TYPE)
        {
            this.queryType = queryType;
            this.queryList = new List<ExpressionKeyValueItem>();

            if (queryType == TQueryHelperTypeEnum.QUERY_TYPE)
            {
                this.queryTableName = TQueryReflectionHelper.GetTableName(typeof(T), true);
            }
            else
            {
                this.queryTableName = TQueryReflectionHelper.GetTableName(typeof(T), false);
            }
            this.queryTop = top;
            this.distinct = distinct;
        }

        #region 增/改/删
        public void Insert(Expression<Func<T, object>> lambda = null)
        {
            this.Init(0, false, TQueryHelperTypeEnum.INSERT_TYPE);

            Dictionary<string, string> fieldMapperDict = null;
            if (lambda == null)
            {
                fieldMapperDict = TQueryReflectionHelper.GetPropertyDict(typeof(T));
            }
            else
            {
                OperaterTranslator operaterTranslator = new OperaterTranslator();
                operaterTranslator.Translate(lambda);
                fieldMapperDict = operaterTranslator.MapperDict;
            }

            int propertyCount = fieldMapperDict.Count;
            int propertyIndex = 0;

            StringBuilder fieldStringBuilder = new StringBuilder();
            StringBuilder paramStringBuilder = new StringBuilder();
            foreach (var keyValueItem in fieldMapperDict)
            {
                fieldStringBuilder.Append(string.Format(TQueryHelperTemplateEnum.FIELD, keyValueItem.Key));
                paramStringBuilder.Append("@");
                paramStringBuilder.Append(keyValueItem.Value);

                if (propertyIndex < propertyCount - 1)
                {
                    fieldStringBuilder.Append(",");
                    paramStringBuilder.Append(",");
                }

                propertyIndex++;
            }

            string querySql = string.Format(TQueryHelperTemplateEnum.INSERT, this.queryTableName, fieldStringBuilder.ToString(), paramStringBuilder.ToString());
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.INSERT, querySql));
        }
        public void Update(Expression<Func<T, object>> lambda = null)
        {
            this.Init(0, false, TQueryHelperTypeEnum.UPDATE_TYPE);

            Dictionary<string, string> fieldMapperDict = null;
            if (lambda == null)
            {
                fieldMapperDict = TQueryReflectionHelper.GetPropertyDict(typeof(T));
            }
            else
            {
                OperaterTranslator operaterTranslator = new OperaterTranslator();
                operaterTranslator.Translate(lambda);
                fieldMapperDict = operaterTranslator.MapperDict;
            }

            int propertyCount = fieldMapperDict.Count;
            int propertyIndex = 0;

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var keyValueItem in fieldMapperDict)
            {
                stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.FIELD, keyValueItem.Key));
                stringBuilder.Append("=");
                stringBuilder.Append("@");
                stringBuilder.Append(keyValueItem.Value);

                if (propertyIndex < propertyCount - 1)
                {
                    stringBuilder.Append(",");
                }

                propertyIndex++;
            }

            string querySql = string.Format(TQueryHelperTemplateEnum.UPDATE, this.queryTableName, stringBuilder.ToString());
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.UPDATE, querySql));
        }
        public void Delete()
        {
            this.Init(0, false, TQueryHelperTypeEnum.DELETE_TYPE);
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.DELETE, string.Format(TQueryHelperTemplateEnum.DELETE, this.queryTableName)));
        }
        #endregion

        #region Select 语句
        public void Select(Type type, Expression lambda = null)
        {
            string querySql = null;
            if (lambda != null)
            {
                querySql = new QueryTranslator().Translate(lambda);
            }
            else
            {
                querySql = string.Format(TQueryHelperTemplateEnum.TABLE_ALL, TQueryReflectionHelper.GetTableName(type));
            }
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.SELECT, querySql));
        }
        public void Select(Type type, string fieldNameList)
        {
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.SELECT, TQueryReflectionHelper.GetFieldString(fieldNameList)));
        }
        public void Select<K>(TQueryHelper<K> helper, string newFieldName = null) where K : class
        {
            string querySql = helper.ToSql();
            if (!string.IsNullOrEmpty(newFieldName))
            {
                querySql = string.Format(TQueryHelperTemplateEnum.SELECT_AS, querySql, newFieldName);
            }
            else
            {
                querySql = string.Format(TQueryHelperTemplateEnum.BREAK, querySql);
            }
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.SELECT, querySql));
        }
        #endregion

        #region 聚合函数
        public void Aggregate(string newFieldName, string aggregateType)
        {
            string querySql = null;
            if (!string.IsNullOrEmpty(newFieldName))
            {
                querySql = string.Format(" {0}(0) as {1} ", aggregateType, newFieldName);
            }
            else
            {
                querySql = string.Format(" {0}(0) ", aggregateType);
            }
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.AGGREGATE, querySql));
        }
        public void Aggregate(Expression lambda, string newFieldName, string aggregateType)
        {
            string querySql = null;
            if (!string.IsNullOrEmpty(newFieldName))
            {
                querySql = string.Format(TQueryHelperTemplateEnum.AGGREGATEAS, aggregateType, new AggregateTranslator().Translate(lambda), newFieldName);
            }
            else
            {
                querySql = string.Format(TQueryHelperTemplateEnum.AGGREGATE, aggregateType, new AggregateTranslator().Translate(lambda));
            }
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.AGGREGATE, querySql));
        }
        #endregion

        #region 公共函数
        private void SetJoinTableName(params Type[] typeList)
        {
            if (this.joinTableDict == null) this.joinTableDict = new Dictionary<string, Type>();
            foreach (Type type in typeList)
            {
                string tableName = type.Name;
                if (!this.joinTableDict.ContainsKey(tableName))
                {
                    this.joinTableDict.Add(tableName, type);
                }
            }
        }
        private string GetFieldValueString<K>(IList<K> fieldValueList)
        {
            bool isInt = (typeof(K) == typeof(int));
            int dataCount = fieldValueList.Count;

            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < dataCount; index++)
            {
                if (!isInt)
                {
                    stringBuilder.Append(string.Format("'{0}'", fieldValueList[index].ToString()));
                }
                else
                {
                    stringBuilder.Append(fieldValueList[index].ToString());
                }
                if (index < dataCount - 1) stringBuilder.Append(",");
            }
            return stringBuilder.ToString();
        }
        #endregion

        #region 表连接
        public void JoinT<K>(Type type, Expression lambda, string joinType = "inner", bool isGeneric = false) where K : class
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(" ");
            if (isGeneric)
            {
                stringBuilder.Append(new JoinOnTranslator<K>().Translate(lambda, joinType, TQueryReflectionHelper.GetTableName(type)));
            }
            else
            {
                stringBuilder.Append(new JoinOnTranslator().Translate(lambda, joinType, TQueryReflectionHelper.GetTableName(type)));
            }
            stringBuilder.Append(" ");
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.JOINON, stringBuilder.ToString()));
        }
        public void Join<K>(Expression<Func<T, K, bool>> lambda, string joinType = "inner") where K : class
        {
            this.SetJoinTableName(typeof(K));
            this.JoinT<T>(typeof(K), lambda, joinType);
        }
        public void Join<K>(Expression<Func<K, object>> lambda, string joinType = "inner") where K : class
        {
            this.SetJoinTableName(typeof(K));
            this.JoinT<T>(typeof(K), lambda, joinType, true);
        }
        public void Join<K, P>(Expression<Func<K, P, bool>> lambda, string joinType = "inner")
            where K : class
            where P : class
        {
            this.SetJoinTableName(typeof(K), typeof(P));
            this.JoinT<K>(typeof(P), lambda, joinType);
        }
        public void Join<K, P>(Expression<Func<P, object>> lambda, string joinType = "inner")
            where K : class
            where P : class
        {
            this.SetJoinTableName(typeof(K), typeof(P));
            this.JoinT<K>(typeof(P), lambda, joinType, true);
        }
        public void JoinAnd(string querySql, bool isBreak = false)
        {
            querySql = string.Format(TQueryHelperTemplateEnum.BREAK, querySql);
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.JOINONAND, querySql));
        }
        public void JoinAnd(Expression lambda, bool isBreak = false)
        {
            string querySql = new WhereTranslator().Translate(lambda);
            if (isBreak)
            {
                querySql = string.Format(TQueryHelperTemplateEnum.BREAK, querySql);
            }
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.JOINONAND, querySql));
        }
        #endregion

        #region Where 语句
        public void Where(string querySql, bool isBreak = false)
        {
            querySql = string.Format(TQueryHelperTemplateEnum.BREAK, querySql);
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.WHERE, querySql));
        }
        public void Where(Expression lambda, string conditionType = null, bool isBreak = false)
        {
            string querySql = new WhereTranslator().Translate(lambda);
            if (string.IsNullOrEmpty(conditionType))
            {
                if (isBreak) querySql = string.Format(TQueryHelperTemplateEnum.BREAK, querySql);
            }
            else
            {
                if (isBreak)
                {
                    querySql = string.Format(TQueryHelperTemplateEnum.WHERE_CONDITION_BREAK, conditionType, querySql);
                }
                else
                {
                    querySql = string.Format(TQueryHelperTemplateEnum.WHERE_CONDITION, conditionType, querySql);
                }
            }
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.WHERE, querySql));
        }
        public void WhereIn<K>(string fieldName, string charType, TQueryHelper<K> helper, bool directUseFieldName = false) where K : class
        {
            string querySql = null;
            if (!directUseFieldName)
            {
                querySql = string.Format(TQueryHelperTemplateEnum.WHERE_IN, TQueryReflectionHelper.GetTableName(typeof(T)), fieldName, charType, helper.ToSql());
            }
            else
            {
                querySql = string.Format(TQueryHelperTemplateEnum.WHERE_IN_DIRECT, fieldName, charType, helper.ToSql());
            }
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.WHERE, querySql));
        }
        public void WhereIn<K>(string fieldName, string charType, IList<K> fieldValueList, bool directUseFieldName = false)
        {
            string querySql = null;
            if (!directUseFieldName)
            {
                querySql = string.Format(TQueryHelperTemplateEnum.WHERE_IN, TQueryReflectionHelper.GetTableName(typeof(T)), fieldName, charType, this.GetFieldValueString<K>(fieldValueList));
            }
            else
            {
                querySql = string.Format(TQueryHelperTemplateEnum.WHERE_IN_DIRECT, fieldName, charType, this.GetFieldValueString<K>(fieldValueList));
            }
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.WHERE, querySql));
        }
        public void WhereIn<K>(Expression<Func<T, object>> lambda, string charType, TQueryHelper<K> helper) where K : class
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(new WhereInTranslator().Translate(lambda, charType));
            stringBuilder.Append(" ");
            stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.BREAK, helper.ToSql()));

            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.WHERE, stringBuilder.ToString()));
        }
        public void WhereIn<K>(Expression<Func<T, object>> lambda, string charType, IList<K> fieldValueList)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(new WhereInTranslator().Translate(lambda, charType));
            stringBuilder.Append(" ");
            stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.BREAK, this.GetFieldValueString<K>(fieldValueList)));

            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.WHERE, stringBuilder.ToString()));
        }
        public void WhereIn<K, P>(Expression<Func<K, P, object>> lambda, string charType, TQueryHelper<K> helper)
            where K : class
            where P : class
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(new WhereInTranslator().Translate(lambda, charType));
            stringBuilder.Append(" ");
            stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.BREAK, helper.ToSql()));

            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.WHERE, stringBuilder.ToString()));
        }
        public void WhereIn<K, P>(Expression<Func<K, object>> lambda, string charType, IList<P> fieldValueList) where K : class
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(new WhereInTranslator().Translate(lambda, charType));
            stringBuilder.Append(" ");
            stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.BREAK, this.GetFieldValueString<P>(fieldValueList)));

            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.WHERE, stringBuilder.ToString()));
        }
        #endregion

        #region Order By 语句
        public void Order(Type type, string orderFieldList)
        {
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.ORDERBY, TQueryReflectionHelper.GetFieldString(orderFieldList)));
        }
        public void Order(Expression lambda, string orderBy)
        {
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.ORDERBY, new OrderTranslator().Translate(lambda, orderBy)));
        }
        #endregion

        #region Group By 语句
        public void Group(Type type, string groupFieldList)
        {
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.GROUPBY, TQueryReflectionHelper.GetFieldString(groupFieldList)));
        }
        public void Group(Expression lambda)
        {
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.GROUPBY, new GroupTranslator().Translate(lambda)));
        }
        #endregion

        #region From In 语句
        public void FromIn<K>(TQueryHelper<K> helper, string newTableName = null) where K : class
        {
            this.fromInTableName = TQueryReflectionHelper.GetTableName(typeof(K));
            this.fromInNewTableName = newTableName;

            string querySql = string.Format(TQueryHelperTemplateEnum.FROM_IN, helper.ToSql(), string.IsNullOrEmpty(newTableName) ? this.fromInTableName : newTableName);
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.FROMIN, querySql));
        }
        public void Exists<K>(TQueryHelper<K> helper) where K : class
        {
            string querySql = string.Format(TQueryHelperTemplateEnum.EXISTS, helper.ToSql());
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.WHERE, querySql));
        }
        public void NotExists<K>(TQueryHelper<K> helper) where K : class
        {
            string querySql = string.Format(TQueryHelperTemplateEnum.NOTEXISTS, helper.ToSql());
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.WHERE, querySql));
        }
        #endregion

        #region Union/Union All 语句
        public void Union<K>(TQueryHelper<K> helper) where K : class
        {
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.UNION, helper.ToSql()));
        }
        #endregion

        #region (/) 括号
        public void BreakLeft()
        {
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.BREAKLEFT, "("));
        }
        public void BreakRight()
        {
            this.queryList.Add(new ExpressionKeyValueItem(TQueryHelperTypeEnum.BREAKRIGHT, ")"));
        }
        #endregion

        public string ToSql(bool withNoLock = true)
        {
            if (this.queryType == TQueryHelperTypeEnum.INSERT_TYPE) return this.CreateInsertSql();
            if (this.queryType == TQueryHelperTypeEnum.UPDATE_TYPE) return this.CreateUpdateSql();
            if (this.queryType == TQueryHelperTypeEnum.DELETE_TYPE) return this.CreateDeleteSql();

            string resultQuerySql = this.CreateQuerySql();
            if (!withNoLock)
            {
                resultQuerySql = resultQuerySql.Replace("with(nolock)", "");
            }
            return resultQuerySql;
        }

        #region 生成 增/删/改/查 语句
        private string CreateInsertSql()
        {
            if (this.queryList == null) return "";

            StringBuilder stringBuilder = new StringBuilder();

            List<ExpressionKeyValueItem> dataList = this.queryList.Where(p => p.Key == TQueryHelperTypeEnum.INSERT).ToList();
            int dataCount = dataList.Count;
            for (int index = 0; index < dataCount; index++)
            {
                stringBuilder.Append(dataList[index].Value);
                if (index < dataCount - 1)
                {
                    stringBuilder.Append(";");
                }
            }

            return stringBuilder.ToString();
        }
        private string CreateUpdateSql()
        {
            if (this.queryList == null) return "";

            StringBuilder stringBuilder = new StringBuilder();

            bool isFirst = true;
            bool isWhere = false;
            List<ExpressionKeyValueItem> dataList = this.queryList.Where(p => p.Key == TQueryHelperTypeEnum.UPDATE || p.Key == TQueryHelperTypeEnum.WHERE || p.Key == TQueryHelperTypeEnum.BREAKLEFT || p.Key == TQueryHelperTypeEnum.BREAKRIGHT).ToList();

            foreach (var keyValueItem in dataList)
            {
                if (keyValueItem.Key == TQueryHelperTypeEnum.UPDATE)
                {
                    if (!isFirst)
                    {
                        stringBuilder.Append(";");
                    }
                    stringBuilder.Append(keyValueItem.Value);
                    isFirst = false;
                }
                else if (keyValueItem.Key == TQueryHelperTypeEnum.WHERE)
                {
                    if (!isWhere)
                    {
                        stringBuilder.Append(" where ");
                    }
                    stringBuilder.Append(keyValueItem.Value);
                }
                else
                {
                    stringBuilder.Append(keyValueItem.Value);
                }
            }

            return stringBuilder.ToString();
        }
        private string CreateDeleteSql()
        {
            if (this.queryList == null) return "";

            StringBuilder stringBuilder = new StringBuilder();

            bool isFirst = true;
            bool isWhere = false;
            List<ExpressionKeyValueItem> dataList = this.queryList.Where(p => p.Key == TQueryHelperTypeEnum.DELETE || p.Key == TQueryHelperTypeEnum.WHERE || p.Key == TQueryHelperTypeEnum.BREAKLEFT || p.Key == TQueryHelperTypeEnum.BREAKRIGHT).ToList();
            foreach (var keyValueItem in dataList)
            {
                if (keyValueItem.Key == TQueryHelperTypeEnum.DELETE)
                {
                    if (!isFirst)
                    {
                        stringBuilder.Append(";");
                    }
                    stringBuilder.Append(keyValueItem.Value);
                    isFirst = false;
                }
                else if (keyValueItem.Key == TQueryHelperTypeEnum.WHERE)
                {
                    if (!isWhere)
                    {
                        stringBuilder.Append(" where ");
                    }
                    stringBuilder.Append(keyValueItem.Value);
                }
                else
                {
                    stringBuilder.Append(keyValueItem.Value);
                }
            }

            return stringBuilder.ToString();
        }
        private string CreateQuerySql()
        {
            string tableName = TQueryReflectionHelper.GetTableName(typeof(T));

            #region 当无数据时
            if (this.queryList == null || this.queryList.Count == 0)
            {
                string querySql = null;
                if (this.distinct)
                {
                    if (this.queryTop > 0)
                    {
                        querySql = string.Format(TQueryHelperTemplateEnum.QUERY_DISTINCT_TOP, tableName, this.queryTop);
                    }
                    else
                    {
                        querySql = string.Format(TQueryHelperTemplateEnum.QUERY_DISTINCT, tableName);
                    }
                }
                else
                {
                    if (this.queryTop > 0)
                    {
                        querySql = string.Format(TQueryHelperTemplateEnum.QUERY_TOP, tableName, this.queryTop);
                    }
                    else
                    {
                        querySql = string.Format(TQueryHelperTemplateEnum.QUERY_DEFAULT, tableName);
                    }
                }
                return querySql;
            }
            #endregion

            StringBuilder stringBuilder = new StringBuilder();
            int queryCount = this.queryList != null ? this.queryList.Count : 0;

            if (queryCount > 0)
            {
                string expressionName = null;
                ExpressionKeyValueItem keyValueItem = null;

                #region 处理别名映射
                if (!string.IsNullOrEmpty(this.fromInNewTableName))
                {
                    foreach (var mapperKeyValueItem in this.queryList)
                    {
                        if (mapperKeyValueItem.Key == TQueryHelperTypeEnum.SELECT || mapperKeyValueItem.Key == TQueryHelperTypeEnum.WHERE || mapperKeyValueItem.Key == TQueryHelperTypeEnum.ORDERBY || mapperKeyValueItem.Key == TQueryHelperTypeEnum.GROUPBY)
                        {
                            mapperKeyValueItem.Value = mapperKeyValueItem.Value.Replace(this.fromInTableName, this.fromInNewTableName);
                        }
                    }
                }
                #endregion

                #region 创建 Select 语句

                stringBuilder.Append("select ");
                if (this.distinct) stringBuilder.Append(" distinct ");
                if (this.queryTop > 0)
                {
                    stringBuilder.Append(" top ");
                    stringBuilder.Append(this.queryTop);
                    stringBuilder.Append(" ");
                }

                List<ExpressionKeyValueItem> aggregateDataList = this.queryList.Where(p => p.Key == TQueryHelperTypeEnum.AGGREGATE).ToList();
                List<ExpressionKeyValueItem> dataList = this.queryList.Where(p => p.Key == TQueryHelperTypeEnum.SELECT).ToList();
                int dataCount = dataList != null ? dataList.Count : 0;
                if ((dataList == null || dataCount == 0) && (aggregateDataList == null || aggregateDataList.Count == 0))
                {
                    stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.TABLE_ALL, tableName));
                    if (this.joinTableDict != null)
                    {
                        if (this.joinTableDict.Count > 0)
                        {
                            stringBuilder.Append(",");
                        }
                        int joinIndex = 0;
                        foreach (var joinKeyValueItem in this.joinTableDict)
                        {
                            stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.TABLE_ALL, TQueryReflectionHelper.GetTableName(joinKeyValueItem.Value)));
                            if (joinIndex < this.joinTableDict.Count - 1) stringBuilder.Append(",");
                            joinIndex++;
                        }
                    }
                }
                else
                {
                    for (int index = 0; index < dataCount; index++)
                    {
                        stringBuilder.Append(dataList[index].Value);
                        if (index < dataCount - 1)
                        {
                            stringBuilder.Append(",");
                        }
                    }
                }
                if (aggregateDataList != null && aggregateDataList.Count > 0)
                {
                    if (dataList != null && dataList.Count > 0) stringBuilder.Append(",");
                    dataCount = aggregateDataList.Count;
                    for (int index = 0; index < dataCount; index++)
                    {
                        stringBuilder.Append(aggregateDataList[index].Value);
                        if (index < dataCount - 1)
                        {
                            stringBuilder.Append(",");
                        }
                    }
                }
                #endregion

                #region From 语句
                stringBuilder.Append(" from ");
                // 判断子查询
                keyValueItem = this.queryList.Where(p => p.Key == TQueryHelperTypeEnum.FROMIN).FirstOrDefault();
                // 当没有子查询时，追加
                if (keyValueItem == null) stringBuilder.Append(this.queryTableName);
                #endregion

                #region 遍历
                for (int index = 0; index < queryCount; index++)
                {
                    keyValueItem = this.queryList[index];
                    if (keyValueItem.Key == TQueryHelperTypeEnum.SELECT || keyValueItem.Key == TQueryHelperTypeEnum.AGGREGATE || keyValueItem.Key == TQueryHelperTypeEnum.ORDERBY || keyValueItem.Key == TQueryHelperTypeEnum.GROUPBY || keyValueItem.Key == TQueryHelperTypeEnum.UNION) continue;

                    if (keyValueItem.Key == TQueryHelperTypeEnum.WHERE && expressionName != TQueryHelperTypeEnum.WHERE)
                    {
                        stringBuilder.Append(" where ");
                    }
                    if (keyValueItem.Key == TQueryHelperTypeEnum.WHERE && expressionName == TQueryHelperTypeEnum.WHERE)
                    {
                        if (!keyValueItem.Value.StartsWith(" and ") && !keyValueItem.Value.StartsWith(" or "))
                        {
                            stringBuilder.Append(" and ");
                        }
                    }
                    if (keyValueItem.Key == TQueryHelperTypeEnum.JOINONAND)
                    {
                        if (!keyValueItem.Value.StartsWith(" and ") && !keyValueItem.Value.StartsWith(" or "))
                        {
                            stringBuilder.Append(" and ");
                        }
                    }
                    stringBuilder.Append(keyValueItem.Value);
                    expressionName = keyValueItem.Key;
                }
                #endregion

                #region Group By 语句
                dataList = this.queryList.Where(p => p.Key == TQueryHelperTypeEnum.GROUPBY).ToList();
                dataCount = dataList != null ? dataList.Count : 0;
                if (dataList != null && dataCount > 0)
                {
                    stringBuilder.Append(" group by ");
                    for (int index = 0; index < dataCount; index++)
                    {
                        stringBuilder.Append(dataList[index].Value);
                        if (index < dataCount - 1)
                        {
                            stringBuilder.Append(",");
                        }
                    }
                }
                #endregion

                #region Order By 语句
                dataList = this.queryList.Where(p => p.Key == TQueryHelperTypeEnum.ORDERBY).ToList();
                dataCount = dataList != null ? dataList.Count : 0;
                if (dataList != null && dataCount > 0)
                {
                    stringBuilder.Append(" order by ");
                    for (int index = 0; index < dataCount; index++)
                    {
                        stringBuilder.Append(dataList[index].Value);
                        if (index < dataCount - 1)
                        {
                            stringBuilder.Append(",");
                        }
                    }
                }
                #endregion
            }

            #region 处理 Union 语句
            List<ExpressionKeyValueItem> unionDataList = this.queryList.Where(p => p.Key == TQueryHelperTypeEnum.UNION).ToList();
            if (unionDataList == null || unionDataList.Count == 0)
            {
                return stringBuilder.ToString();
            }
            else
            {
                StringBuilder unionStringBuilder = new StringBuilder();
                unionStringBuilder.Append(string.Format(TQueryHelperTemplateEnum.BREAK, stringBuilder.ToString()));

                foreach(ExpressionKeyValueItem keyValueItem in unionDataList)
                {
                    unionStringBuilder.Append(" union ");
                    unionStringBuilder.Append(string.Format(TQueryHelperTemplateEnum.BREAK, keyValueItem.Value));
                }

                return unionStringBuilder.ToString();
            }
            #endregion
        }
        #endregion
    }
}
