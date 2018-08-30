using Helper.Core.Library.QueryHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.Translator
{
    internal class OperaterTranslator : BaseTranslator
    {
        private Dictionary<string, string> mapperDict = null;

        protected override Expression VisitNew(NewExpression expression)
        {
            var expressionArguments = expression.Arguments;
            int expressionMemberCount = expression.Members.Count;
            dynamic expressionArgument = null;

            string fieldName = null;
            string memberName = null;
            TranslatorMapperItem mapperItem = null;

            if (this.mapperDict == null) this.mapperDict = new Dictionary<string, string>();

            for (int index = 0; index < expressionMemberCount; index++)
            {
                expressionArgument = expressionArguments[index];

                mapperItem = this.typeDict[expressionArgument.Expression.Name];
                memberName = expressionArgument.Member.Name;
                fieldName = TQueryReflectionHelper.GetFieldName(mapperItem.TableType, memberName);

                if (mapperItem.TableName != null && !string.IsNullOrEmpty(memberName) && !string.IsNullOrEmpty(fieldName))
                {
                    if (memberName != expression.Members[index].Name)
                    {
                        this.mapperDict.Add(fieldName, expression.Members[index].Name);
                    }
                    else
                    {
                        this.mapperDict.Add(fieldName, memberName);
                    }
                }
            }

            return expression;
        }

        public Dictionary<string, string> MapperDict { get { return this.mapperDict; } }
    }
}
