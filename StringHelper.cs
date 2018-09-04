/*
 * 作用：字符串操作。
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    public enum StringCaseTypeEnum
    {
        /// <summary>
        /// 默认
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 小写
        /// </summary>
        Lower = 1,

        /// <summary>
        /// 大写
        /// </summary>
        Upper = 2
    }
    public enum StringSeedTypeEnum
    {
        /// <summary>
        /// 纯数字
        /// </summary>
        Number = 0,
        /// <summary>
        /// 纯字符
        /// </summary>
        Char = 1,
        /// <summary>
        /// 数字字符组合
        /// </summary>
        NumberChar = 2
    }
    #endregion
    public class StringHelper
    {
        #region 私有属性常量
        private static readonly char[] NUMBER_SEED_LIST = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private static readonly char[] CHAR_SEED_LIST = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        private static readonly char[] NUMBER_CHAR_SEED_LIST = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 获取随机字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="prefix">前缀</param>
        /// <param name="suffix">后缀</param>
        /// <param name="seedType">StringSeedTypeEnum</param>
        /// <param name="caseType">StringCaseTypeEnum</param>
        /// <returns></returns>
        public static string GetRandomCode(int length, string prefix = "", string suffix = "", StringSeedTypeEnum seedType = StringSeedTypeEnum.Number, StringCaseTypeEnum caseType = StringCaseTypeEnum.Normal)
        {
            char[] seedList = null;
            if (seedType == StringSeedTypeEnum.Number)
            {
                seedList = NUMBER_SEED_LIST;
            }
            else if (seedType == StringSeedTypeEnum.Char)
            {
                seedList = CHAR_SEED_LIST;
            }
            else
            {
                seedList = NUMBER_CHAR_SEED_LIST;
            }
            int seedLength = seedList.Length;

            Random random = new Random();

            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(prefix)) stringBuilder.Append(prefix);
            for (int index = 0; index < length; index++)
            {
                stringBuilder.Append(seedList[random.Next(0, seedLength)]);
            }
            if (!string.IsNullOrEmpty(suffix)) stringBuilder.Append(suffix);

            if (caseType == StringCaseTypeEnum.Lower)
            {
                return stringBuilder.ToString().ToLower();
            }
            else if (caseType == StringCaseTypeEnum.Upper)
            {
                return stringBuilder.ToString().ToUpper();
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 获取字节长度
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static int GetLength(string str)
        {
            return System.Text.UTF8Encoding.Default.GetBytes(str).Length;
        }
        /// <summary>
        /// 左补齐字符串
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="paddingStr">填充字符串</param>
        /// <param name="repeat">重复次数</param>
        /// <returns></returns>
        public static string PadLeft(string str, string paddingStr, int repeat)
        {
            return StringPad(str, paddingStr, repeat, true);
        }
        /// <summary>
        /// 右补齐字符串
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="paddingStr">填充字符串</param>
        /// <param name="repeat">重复次数</param>
        /// <returns></returns>
        public static string PadRight(string str, string paddingStr, int repeat)
        {
            return StringPad(str, paddingStr, repeat, false);
        }
        /// <summary>
        /// 补充前缀或者后缀
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="charStr">要补充的字符</param>
        /// <returns></returns>
        public static string PadChar(string str, string charStr = ",")
        {
            if (string.IsNullOrEmpty(str)) return str;

            if (!str.StartsWith(charStr)) str = charStr + str;
            if (!str.EndsWith(charStr)) str = str + charStr;

            return str;
        }
        /// <summary>
        /// 截取前面字符
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="trim">要移除的字符串</param>
        /// <returns></returns>
        public static string TrimStart(string str, string trim)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(trim)) return str;
            return str.Substring(trim.Length);
        }
        /// <summary>
        /// 截取后面字符
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="trim">要移除的字符串</param>
        /// <returns></returns>
        public static string TrimEnd(string str, string trim)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(trim)) return str;
            return str.Substring(0, str.Length - trim.Length);
        }
        /// <summary>
        /// 截取前后字符串
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="charStr">要移除的字符</param>
        /// <returns></returns>
        public static string TrimChar(string str, string charStr = ",")
        {
            if (string.IsNullOrEmpty(str)) return str;

            if (str.StartsWith(str)) str = str.TrimStart(charStr.ToCharArray());
            if (str.EndsWith(str)) str = str.TrimEnd(charStr.ToCharArray());
            return str;
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <typeparam name="T">简单类型，例：string</typeparam>
        /// <param name="str">要拆分的字符串，例：id,name</param>
        /// <param name="splitChar">分隔符，例：,</param>
        /// <param name="removeRepeat">是否去除重复值</param>
        /// <param name="caseEnum">StringCaseTypeEnum 枚举</param>
        /// <returns></returns>
        public static List<T> ToList<T>(string str, string splitChar, bool removeRepeat = false, StringCaseTypeEnum caseEnum = StringCaseTypeEnum.Normal)
        {
            if (caseEnum != StringCaseTypeEnum.Normal)
            {
                if (caseEnum == StringCaseTypeEnum.Lower)
                {
                    str = str.ToLower();
                    splitChar = splitChar.ToLower();
                }
                else
                {
                    str = str.ToUpper();
                    splitChar = splitChar.ToUpper();
                }
            }

            List<string> stringList = str.Split(new string[] { splitChar }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            if (removeRepeat) stringList = stringList.Distinct<string>().ToList();

            List<T> dataList = new List<T>();
            foreach (string stringData in stringList)
            {
                dataList.Add((T)Convert.ChangeType(stringData.Trim(), typeof(T)));
            }
            return dataList;
        }
        /// <summary>
        /// 转成键值对
        /// </summary>
        /// <typeparam name="T">简单类型，例：string</typeparam>
        /// <typeparam name="K">简单类型，例：int</typeparam>
        /// <param name="str">要拆分的字符串，例：id,name</param>
        /// <param name="splitChar">分隔符，例：,</param>
        /// <returns></returns>
        public static StringKeyValueData<T, K> ToKeyValueData<T, K>(string str, string splitChar)
        {
            List<string> stringList = str.Split(new string[] { splitChar }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            if (stringList == null || stringList.Count != 2) return null;

            return new StringKeyValueData<T, K>()
            {
                Key = (T)Convert.ChangeType(stringList[0].Trim(), typeof(T)),
                Value = (K)Convert.ChangeType(stringList[1].Trim(), typeof(K))
            };
        }
        /// <summary>
        /// 转成键值对列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="str"></param>
        /// <param name="firstChar"></param>
        /// <param name="secondChar"></param>
        /// <param name="caseEnum"></param>
        /// <returns></returns>
        public static List<StringKeyValueData<T, K>> ToKeyValueList<T, K>(string str, string firstChar, string secondChar, StringCaseTypeEnum caseEnum = StringCaseTypeEnum.Normal)
        {
            if (caseEnum != StringCaseTypeEnum.Normal)
            {
                if (caseEnum == StringCaseTypeEnum.Lower)
                {
                    str = str.ToLower();
                    firstChar = firstChar.ToLower();
                    secondChar = secondChar.ToLower();
                }
                else
                {
                    str = str.ToUpper();
                    firstChar = firstChar.ToUpper();
                    secondChar = secondChar.ToLower();
                }
            }
            List<StringKeyValueData<T, K>> resultList = new List<StringKeyValueData<T, K>>();
            string[] stringList = str.Split(new string[] { firstChar }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringData in stringList)
            {
                resultList.Add(ToKeyValueData<T, K>(stringData, secondChar));
            }
            return resultList;
        }
        /// <summary>
        /// 获取字典
        /// </summary>
        /// <typeparam name="T">简单类型，例：string</typeparam>
        /// <typeparam name="K">简单类型，例：string</typeparam>
        /// <param name="str">要拆分的字符串，例：id asc, name desc</param>
        /// <param name="firstChar">第一个分隔符，例：,</param>
        /// <param name="secondChar">第二个分隔符，例：|</param>
        /// <returns></returns>
        public static Dictionary<T, K> ToDict<T, K>(string str, string firstChar, string secondChar)
        {
            Dictionary<T, K> dict = new Dictionary<T, K>();

            string[] stringList = str.Split(new string[] { firstChar }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringData in stringList)
            {
                string[] itemList = stringData.Split(new string[] { secondChar }, StringSplitOptions.RemoveEmptyEntries);
                if (itemList != null && itemList.Length == 2)
                {
                    T t = (T)Convert.ChangeType(itemList[0].Trim(), typeof(T));
                    if (!dict.ContainsKey(t)) dict.Add(t, (K)Convert.ChangeType(itemList[1].Trim(), typeof(K)));
                }
            }

            return dict;
        }
        /// <summary>
        /// 列表创建字符串
        /// </summary>
        /// <typeparam name="T">简单类型，例：int</typeparam>
        /// <param name="dataList">列表数据</param>
        /// <param name="splitChar">分隔符，例：,</param>
        /// <returns></returns>
        public static string ToString<T>(List<T> dataList, string splitChar)
        {
            if (dataList == null) return null;

            string str = string.Join(splitChar, dataList);
            if (!string.IsNullOrEmpty(str)) str = str.TrimEnd(splitChar.ToCharArray());

            return str;
        }
        /// <summary>
        /// 字典创建字符串
        /// </summary>
        /// <typeparam name="T">简单类型，例：string</typeparam>
        /// <typeparam name="K">简单类型，例：int</typeparam>
        /// <param name="dataDict">字典数据</param>
        /// <param name="firstChar">第一个分隔符，例：,</param>
        /// <param name="secondChar">第二个分隔符，例：|</param>
        /// <returns></returns>
        public static string ToString<T, K>(Dictionary<T, K> dataDict, string firstChar, string secondChar)
        {
            if (dataDict == null) return null;

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var keyValueItem in dataDict)
            {
                stringBuilder.Append(keyValueItem.Key);
                stringBuilder.Append(firstChar);
                stringBuilder.Append(keyValueItem.Value);
                stringBuilder.Append(secondChar);
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="firstChar">第一个字符，如果 secondChar = null，则是 0-index，如果 secondChar = ""，则是 index-length</param>
        /// <param name="secondChar">第二个字符</param>
        /// <param name="firstEnd">是否从结尾查找第一个字符，默认：false</param>
        /// <param name="secondEnd">是否从结尾查找第二个字符，默认：false</param>
        /// <returns></returns>
        public static string Substring(string str, string firstChar, string secondChar = null, bool firstEnd = false, bool secondEnd = false)
        {
            if (string.IsNullOrEmpty(str)) return null;

            int beginIndex = 0;
            if (!firstEnd)
            {
                beginIndex = str.IndexOf(firstChar);
            }
            else
            {
                beginIndex = str.LastIndexOf(firstChar);
            }

            if (secondChar == null)
            {
                str = str.Substring(0, beginIndex);
            }
            else
            {
                str = str.Substring(beginIndex + firstChar.Length);
            }
            if (string.IsNullOrEmpty(secondChar)) return str;

            int endIndex = 0;
            if (!secondEnd)
            {
                endIndex = str.IndexOf(secondChar);
            }
            else
            {
                endIndex = str.LastIndexOf(secondChar);
            }

            return str.Substring(0, endIndex);
        }
        /// <summary>
        /// 过滤特殊字符
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="specList">特殊字符</param>
        /// <returns></returns>
        public static string FilterSpecChar(string str, params string[] specList)
        {
            if (string.IsNullOrEmpty(str)) return str;

            List<string> filterList = new List<string>()
            {
                "*", "'", "<", ">", "=", "select", "delete", "update", "insert"
            };
            if (specList != null && specList.Length > 0)
            {
                filterList.AddRange(specList);
                filterList = filterList.Distinct().ToList();
            }
            foreach (string filterData in filterList)
            {
                str = str.Replace(filterData, "");
            }
            return str;
        }
        #endregion

        #region 逻辑处理私有函数
        private static string StringPad(string str, string paddingStr, int repeat, bool isLeft)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!isLeft) stringBuilder.Append(str);
            for (int index = 0; index < repeat; index++)
            {
                stringBuilder.Append(paddingStr);
            }
            if (isLeft) stringBuilder.Append(str);
            return stringBuilder.ToString();
        }
        #endregion
    }

    #region 逻辑处理辅助类
    public class StringKeyValueData<T, K>
    {
        public T Key { get; set; }
        public K Value { get; set; }
    }
    #endregion
}
