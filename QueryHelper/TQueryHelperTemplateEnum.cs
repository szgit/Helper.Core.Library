using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library.QueryHelper
{
    internal class TQueryHelperTemplateEnum
    {
        public const string BREAK = "({0})";
        public const string FIELD = "[{0}]";

        public const string WHERE_CONDITION = " {0} {1}";
        public const string WHERE_CONDITION_BREAK = " {0} ({1})";
        public const string WHERE_IN = "[{0}].[{1}] {2} ({3})";
        public const string WHERE_IN_DIRECT = "{0} {1} ({2})";
        public const string SELECT_AS = "({0}) as [{1}]";
        public const string FROM_IN = " ({0}) as [{1}] ";
        public const string EXISTS = " exists ({0}) ";
        public const string NOTEXISTS = " not exists ({0}) ";
        public const string TABLE_NAME = "[{0}]";
        public const string TABLE_NAME_WITHNOLOCK = "[{0}] with(nolock) ";
        public const string TABLE_ALL = "[{0}].* ";
        public const string TABLE_FIELD = "[{0}].[{1}]";
        public const string TABLE_FIELD_BORDERBY = "[{0}].[{1}] {2}";
        public const string JOINON = " {0} join [{1}] with(nolock) on ";
        public const string JOINON_FIELD = " {0} join [{1}] with(nolock) on [{2}].[{3}]=[{1}].[{3}] ";
        public const string QUERY_DEFAULT = "select [{0}].* from [{0}] with(nolock)";
        public const string QUERY_DISTINCT = "select distinct [{0}].* from [{0}] with(nolock)";
        public const string QUERY_TOP = "select top {1} [{0}].* from [{0}] with(nolock)";
        public const string QUERY_DISTINCT_TOP = "select distinct top {1} [{0}].* from [{0}] with(nolock)";
        public const string INSERT = "insert into {0}({1})values({2})";
        public const string UPDATE = "update {0} set {1} ";
        public const string DELETE = "delete from {0} ";
        public const string AGGREGATE = "{0}({1})";
        public const string AGGREGATEAS = "{0}({1}) as [{2}]";
    }
}
