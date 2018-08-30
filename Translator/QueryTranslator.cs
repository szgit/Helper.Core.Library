using Helper.Core.Library.QueryHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.Translator
{
    internal class QueryTranslator : BaseTranslator
    {
        protected override Expression VisitNew(NewExpression expression)
        {
            var expressionArguments = expression.Arguments;
            int expressionMemberCount = expression.Members.Count;
            dynamic expressionArgument = null;

            string fieldName = null;
            string memberName = null;
            TranslatorMapperItem mapperItem = null;

            for (int index = 0; index < expressionMemberCount; index++)
            {
                expressionArgument = expressionArguments[index];

                mapperItem = this.typeDict[expressionArgument.Expression.Name];
                memberName = expressionArgument.Member.Name;
                fieldName = TQueryReflectionHelper.GetFieldName(mapperItem.TableType, memberName);

                if (mapperItem.TableName != null && !string.IsNullOrEmpty(memberName) && !string.IsNullOrEmpty(fieldName))
                {
                    this.stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.TABLE_FIELD, mapperItem.TableName, fieldName));
                    if (memberName != expression.Members[index].Name)
                    {
                        this.stringBuilder.Append(" as ");
                        this.stringBuilder.Append(expression.Members[index].Name);
                    }
                    if (index < expressionMemberCount - 1)
                    {
                        this.stringBuilder.Append(",");
                    }
                }
            }

            return expression;
        }
    }
}
