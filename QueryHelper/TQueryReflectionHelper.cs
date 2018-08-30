using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Helper.Core.Library.QueryHelper
{
    internal class TQueryReflectionHelper
    {
        private const string FieldFormat = "[{0}]";

        private static readonly object lockItem = new object();
        private static Dictionary<string, Dictionary<string, string>> TypeFieldMapperDict = new Dictionary<string, Dictionary<string, string>>();

        public static string GetTableName(Type type)
        {
            string tableName = ReflectionExtendHelper.GetAttributeValue<DataBaseTAttribute>(type, p => p.Name);
            if (!string.IsNullOrEmpty(tableName)) return tableName;
            return type.Name;
        }
        public static string GetTableName(Type type, bool withNolock)
        {
            string tableName = GetTableName(type);
            return GetTableName(tableName, withNolock);
        }
        public static string GetTableName(string tableName, bool withNolock = false)
        {
            if (withNolock)
            {
                tableName = string.Format(TQueryHelperTemplateEnum.TABLE_NAME_WITHNOLOCK, tableName);
            }
            else
            {
                tableName = string.Format(TQueryHelperTemplateEnum.TABLE_NAME, tableName);
            }
            return tableName;
        }
        public static string GetFieldString(string fieldList)
        {
            if (string.IsNullOrEmpty(fieldList)) return "";

            StringBuilder stringBuilder = new StringBuilder();
            string[] dataList = fieldList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int dataCount = dataList.Length;
            for (int index = 0; index < dataCount; index++)
            {
                stringBuilder.Append(dataList[index].Trim());
                if (index < dataCount - 1) stringBuilder.Append(",");
            }
            return stringBuilder.ToString();
        }
        public static string GetFieldName(Type type, string propertyName)
        {
            string fieldName = ReflectionExtendHelper.GetAttributeValue<DataBaseTAttribute>(type, propertyName, p => p.Name);
            if (!string.IsNullOrEmpty(fieldName)) return fieldName;
            return propertyName;
        }
        public static string FormatQuerySql(string text, params Type[] typeList)
        {
            if (typeList != null && typeList.Length == 0) return text;

            Dictionary<string, string> mapperDict = new Dictionary<string, string>();
            foreach (Type type in typeList)
            {
                string typeName = string.Format(FieldFormat, type.Name);
                if (!mapperDict.ContainsKey(typeName))
                {
                    mapperDict.Add(typeName, string.Format(FieldFormat, GetTableName(type)));
                }
                Dictionary<string, string> typePropertyDict = GetTypeFieldMapperDict(type);
                foreach (var keyValueItem in typePropertyDict)
                {
                    if (!mapperDict.ContainsKey(keyValueItem.Key))
                    {
                        mapperDict.Add(keyValueItem.Key, keyValueItem.Value);
                    }
                }
            }

            List<string> fieldList = new List<string>();

            Regex regex = new Regex(@"(\[.*?\])");
            MatchCollection matchList = regex.Matches(text);
            if (matchList != null && matchList.Count > 0)
            {
                string matchValue = null;
                foreach (Match match in matchList)
                {
                    matchValue = match.Value;
                    if (fieldList.IndexOf(matchValue) < 0)
                    {
                        fieldList.Add(matchValue);
                    }
                }
            }

            foreach (string field in fieldList)
            {
                if (mapperDict.ContainsKey(field)) text = text.Replace(field, mapperDict[field]);
            }
            return text;
        }
        public static Dictionary<string, string> GetPropertyDict(Type type)
        {
            Dictionary<string, string> resultDict = new Dictionary<string, string>();
            PropertyInfo[] propertyInfoList = type.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfoList)
            {
                resultDict.Add(GetFieldName(type, propertyInfo.Name), propertyInfo.Name);
            }
            return resultDict;
        }
        private static Dictionary<string, string> GetTypeFieldMapperDict(Type type)
        {
            string typeName = type.FullName;
            if (TypeFieldMapperDict.ContainsKey(typeName)) return TypeFieldMapperDict[typeName];

            lock(lockItem)
            {
                Dictionary<string, string> propertyDict = new Dictionary<string, string>();
                ReflectionHelper.Foreach((PropertyInfo propertyInfo) =>
                {
                    propertyDict.Add(string.Format(FieldFormat, propertyInfo.Name), string.Format(FieldFormat, GetFieldName(type, propertyInfo.Name)));
                }, type);

                TypeFieldMapperDict.Add(typeName, propertyDict);

                return propertyDict;
            }
        }
    }
}
