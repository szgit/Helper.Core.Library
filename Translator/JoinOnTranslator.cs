using Helper.Core.Library.QueryHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.Translator
{
    internal class JoinOnTranslator : BaseTranslator
    {
        public string Translate(Expression expression, string joinType, string tableName)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.JOINON, joinType, tableName));
            stringBuilder.Append(this.Translate(expression));

            return stringBuilder.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            this.Visit(expression.Left);
            this.stringBuilder.Append("=");
            this.Visit(expression.Right);
            return expression;
        }
    }
}
