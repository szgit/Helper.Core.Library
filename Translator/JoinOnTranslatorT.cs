using Helper.Core.Library.QueryHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.Translator
{
    internal class JoinOnTranslator<K> : BaseTranslator where K : class
    {
        public string Translate(Expression expression, string joinType, string tableName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.JOINON, joinType, tableName));
            stringBuilder.Append(this.Translate(expression));

            return stringBuilder.ToString();
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            if (expression.Expression != null && expression.Expression.NodeType == ExpressionType.Parameter)
            {
                dynamic expressionMember = expression.Expression;
                TranslatorMapperItem mapperItem = this.typeDict[expressionMember.Name];
                string fieldName = TQueryReflectionHelper.GetFieldName(mapperItem.TableType, expression.Member.Name);

                this.stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.TABLE_FIELD, TQueryReflectionHelper.GetTableName(typeof(K)), fieldName));
                this.stringBuilder.Append("=");
                this.stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.TABLE_FIELD, mapperItem.TableName, fieldName));
            }
            return expression;
        }
    }
}
