using Helper.Core.Library.QueryHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.Translator
{
    internal class BaseTranslator : ExpressionVisitor
    {
        protected StringBuilder stringBuilder;
        protected Dictionary<string, TranslatorMapperItem> typeDict;

        public virtual string Translate(Expression expression)
        {
            dynamic queryExpression = expression;

            this.stringBuilder = new StringBuilder();
            this.typeDict = new Dictionary<string, TranslatorMapperItem>();

            foreach (var parameter in queryExpression.Parameters)
            {
                this.typeDict.Add(parameter.Name, new TranslatorMapperItem() { TableName = TQueryReflectionHelper.GetTableName(parameter.Type), TableType = parameter.Type });
            }

            this.Visit(expression);

            return this.stringBuilder.ToString();
        }
        protected override Expression VisitMember(MemberExpression expression)
        {
            if (expression.Expression != null && expression.Expression.NodeType == ExpressionType.Parameter)
            {
                dynamic expressionMember = expression.Expression;
                TranslatorMapperItem mapperItem = this.typeDict[expressionMember.Name];
                string fieldName = TQueryReflectionHelper.GetFieldName(mapperItem.TableType, expression.Member.Name);

                this.stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.TABLE_FIELD, mapperItem.TableName, fieldName));
            }
            return expression;
        }
    }
}
