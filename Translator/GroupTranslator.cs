using Helper.Core.Library.QueryHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.Translator
{
    internal class GroupTranslator : BaseTranslator
    {
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
                    this.stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.TABLE_FIELD, mapperItem.TableName, fieldName));
                    if (index < expressionMemberCount - 1) this.stringBuilder.Append(",");
                }
            }

            return expression;
        }
    }
}
