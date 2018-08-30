using Helper.Core.Library.QueryHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.Translator
{
    internal class OrderTranslator : BaseTranslator
    {
        private string orderBy;

        public string Translate(Expression expression, string orderBy)
        {
            this.orderBy = orderBy;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(this.Translate(expression));
            stringBuilder.Append(" ");
            return stringBuilder.ToString();
        }

        protected override Expression VisitNew(NewExpression expression)
        {
            var expressionArguments = expression.Arguments;
            int expressionMemberCount = expression.Members.Count;
            dynamic expressionArgument = null;

            TranslatorMapperItem mapperItem = null;
            string fieldName = null;

            for (int index = 0; index < expressionMemberCount; index++)
            {
                expressionArgument = expressionArguments[index];

                mapperItem = this.typeDict[expressionArgument.Expression.Name];
                fieldName = TQueryReflectionHelper.GetFieldName(mapperItem.TableType, expressionArgument.Member.Name);

                if (mapperItem != null && fieldName != null)
                {
                    this.stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.TABLE_FIELD_BORDERBY, mapperItem.TableName, fieldName, this.orderBy));
                    if (index < expressionMemberCount - 1)
                    {
                        this.stringBuilder.Append(",");
                    }
                }
            }

            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            if (expression.Expression != null && expression.Expression.NodeType == ExpressionType.Parameter)
            {
                dynamic expressionMember = expression.Expression;
                TranslatorMapperItem mapperItem = this.typeDict[expressionMember.Name];
                string fieldName = TQueryReflectionHelper.GetFieldName(mapperItem.TableType, expression.Member.Name);
                this.stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.TABLE_FIELD_BORDERBY, mapperItem.TableName, fieldName, this.orderBy));
            }
            return expression;
        }
    }
}
