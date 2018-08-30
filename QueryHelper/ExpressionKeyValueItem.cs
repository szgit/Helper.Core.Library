using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.QueryHelper
{
    internal class ExpressionKeyValueItem
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public ExpressionKeyValueItem(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
