using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.Translator
{
    internal class WhereInTranslator : BaseTranslator
    {
        public string Translate(Expression expression, string charType)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.Translate(expression));
            stringBuilder.Append(" ");
            stringBuilder.Append(charType);

            return stringBuilder.ToString();
        }
    }
}
