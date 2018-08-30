/*
 * 作用：读取/写入文档数据。
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Helper.Core.Library
{
    #region 逻辑辅助枚举
    public enum TxtTypeEnum
    {
        /// <summary>
        /// 实体带有 TxtT 特性
        /// </summary>
        Attribute = 1,

        /// <summary>
        /// 实体不带有 TxtT 特性
        /// </summary>
        Normal = 2,
    }
    #endregion

    public class TxtHelper
    {
        #region 私有属性常量
        public const string NewLine = "\r\n";
        #endregion

        #region 对外公开方法

        #region Read
        /// <summary>
        /// 读取 Txt 数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static string Read(string path)
        {
            return Read(path, System.Text.Encoding.UTF8);
        }
        /// <summary>
        /// 读取 Txt 数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static string Read(string path, System.Text.Encoding encoding)
        {
            string content = "";
            ExecuteStreamReader(path, encoding, (StreamReader streamReader) =>
            {
                content = streamReader.ReadToEnd();
            });
            return content;
        }
        #endregion

        #region Write
        /// <summary>
        /// 数据写入 Txt 文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="content">要写入的数据</param>
        /// <param name="append">是否追加</param>
        /// <returns></returns>
        public static bool Write(string path, string content, bool append = false)
        {
            return Write(path, content, append, System.Text.Encoding.UTF8);
        }
        /// <summary>
        /// 数据写入 Txt 文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="content">要写入的数据</param>
        /// <param name="append">是否追加</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static bool Write(string path, string content, bool append, System.Text.Encoding encoding)
        {
            return ExecuteStreamWriter(path, encoding, append, () =>
            {
                return content;
            });
        }
        #endregion

        #region ToEntityList<T>
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="txtPath">Txt 路径</param>
        /// <param name="splitChar">列分隔符，例：|</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="typeEnum">TxtTypeEnum 枚举类型</param>
        /// <param name="lineSplitChar">行分隔符，例：\r\n</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(string txtPath, string splitChar, object propertyMatchList = null, TxtTypeEnum typeEnum = TxtTypeEnum.Normal, string lineSplitChar = NewLine, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            return ToEntityList<T>(txtPath, splitChar, System.Text.Encoding.UTF8, propertyMatchList, typeEnum, lineSplitChar, reflectionType);
        }
        /// <summary>
        /// 返回实体数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="txtPath">Txt 路径</param>
        /// <param name="splitChar">列分隔符，例：|</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="typeEnum">TxtTypeEnum 枚举类型</param>
        /// <param name="lineSplitChar">行分隔符，例：\r\n</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> ToEntityList<T>(string txtPath, string splitChar, System.Text.Encoding encoding, object propertyMatchList = null, TxtTypeEnum typeEnum = TxtTypeEnum.Normal, string lineSplitChar = NewLine, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            List<T> dataList = new List<T>();
            ExecuteStreamReader(txtPath, encoding, (StreamReader streamReader) =>
            {
                Dictionary<string, object> propertyDict = CommonHelper.GetParameterDict(propertyMatchList);
                Dictionary<PropertyInfo, int> propertyNameDict = InitTxtToEntityMapper<T>(propertyDict, typeEnum);

                dynamic propertySetDict = null;
                if (reflectionType != ReflectionTypeEnum.Original) propertySetDict = ReflectionExtendHelper.PropertySetCallDict<T>(reflectionType);

                string content = streamReader.ReadToEnd();
                string[] contentList = content.Split(new string[] { lineSplitChar }, StringSplitOptions.None);
                foreach (string contentItem in contentList)
                {
                    dataList.Add(TextToEntity<T>(contentItem, splitChar, propertySetDict, propertyNameDict));
                }
            });
            return dataList;
        }
        /// <summary>
        /// 返回字典数据
        /// </summary>
        /// <typeparam name="T">基类类型，例：string</typeparam>
        /// <typeparam name="K">基类类型，例：int</typeparam>
        /// <param name="txtPath">Txt 路径</param>
        /// <param name="splitChar">列分隔符，例：|</param>
        /// <param name="lineSplitChar">行分隔符，例：\r\n</param>
        /// <param name="keyIndex">键索引，默认：0</param>
        /// <param name="valueIndex">值索引，默认：1</param>
        /// <returns></returns>
        public static Dictionary<T, K> ToDict<T, K>(string txtPath, string splitChar, string lineSplitChar = NewLine, int keyIndex = 0, int valueIndex = 1)
        {
            return ToDict<T, K>(txtPath, splitChar, System.Text.Encoding.UTF8, lineSplitChar, keyIndex, valueIndex);
        }
        /// <summary>
        /// 返回字典数据
        /// </summary>
        /// <typeparam name="T">基类类型，例：string</typeparam>
        /// <typeparam name="K">基类类型，例：int</typeparam>
        /// <param name="txtPath">Txt 路径</param>
        /// <param name="splitChar">列分隔符，例：|</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="lineSplitChar">行分隔符，例：\r\n</param>
        /// <param name="keyIndex">键索引，默认：0</param>
        /// <param name="valueIndex">值索引，默认：1</param>
        /// <returns></returns>
        public static Dictionary<T, K> ToDict<T, K>(string txtPath, string splitChar, System.Text.Encoding encoding, string lineSplitChar = NewLine, int keyIndex = 0, int valueIndex = 1)
        {
            Dictionary<T, K> resultDict = new Dictionary<T, K>();
            ExecuteStreamReader(txtPath, encoding, (StreamReader streamReader) =>
            {
                string content = streamReader.ReadToEnd();
                string[] contentList = content.Split(new string[] { lineSplitChar }, StringSplitOptions.None);
                foreach (string contentItem in contentList)
                {
                    string[] itemList = contentItem.Split(new string[] { splitChar }, StringSplitOptions.None);
                    if (!string.IsNullOrEmpty(itemList[keyIndex]))
                    {
                        resultDict.Add((T)Convert.ChangeType(itemList[keyIndex], typeof(T)), (K)Convert.ChangeType(itemList[valueIndex], typeof(K)));
                    }
                }
            });
            return resultDict;
        }
        /// <summary>
        /// 返回基础类型数据列表
        /// </summary>
        /// <typeparam name="T">基类类型，例：string</typeparam>
        /// <param name="txtPath">Txt 路径</param>
        /// <param name="splitChar">列分隔符，例：|</param>
        /// <param name="lineSplitChar">行分隔符，例：\r\n</param>
        /// <param name="fieldIndex">字段索引，默认：0</param>
        /// <returns></returns>
        public static List<T> ToList<T>(string txtPath, string splitChar, string lineSplitChar = NewLine, int fieldIndex = 0)
        {
            return ToList<T>(txtPath, splitChar, System.Text.Encoding.UTF8, lineSplitChar, fieldIndex);
        }
        /// <summary>
        /// 返回基础类型数据列表
        /// </summary>
        /// <typeparam name="T">基类类型，例：string</typeparam>
        /// <param name="txtPath">Txt 路径</param>
        /// <param name="splitChar">列分隔符，例：|</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="lineSplitChar">行分隔符，例：\r\n</param>
        /// <param name="fieldIndex">字段索引，默认：0</param>
        /// <returns></returns>
        public static List<T> ToList<T>(string txtPath, string splitChar, System.Text.Encoding encoding, string lineSplitChar = NewLine, int fieldIndex = 0)
        {
            List<T> dataList = new List<T>();
            ExecuteStreamReader(txtPath, encoding, (StreamReader streamReader) =>
            {
                string content = streamReader.ReadToEnd();
                string[] contentList = content.Split(new string[] { lineSplitChar }, StringSplitOptions.None);
                foreach(string contentItem in contentList)
                {
                    string[] itemList = contentItem.Split(new string[] { splitChar }, StringSplitOptions.None);
                    dataList.Add((T)Convert.ChangeType(itemList[fieldIndex], typeof(T)));
                }
            });
            return dataList;
        }
        #endregion

        #region ToTxt<T>
        /// <summary>
        /// 列表数据写入 Txt
        /// </summary>
        /// <typeparam name="T">普通类型，例：int</typeparam>
        /// <param name="txtPath">Txt 路径</param>
        /// <param name="dataList">数据列表</param>
        /// <param name="splitChar">列分隔符，例：|</param>
        /// <returns></returns>
        public static bool ToTxt<T>(string txtPath, List<T> dataList, string splitChar)
        {
            return ToTxt<T>(txtPath, dataList, splitChar, System.Text.Encoding.UTF8);
        }
        /// <summary>
        /// 列表数据写入 Txt
        /// </summary>
        /// <typeparam name="T">普通类型，例：int</typeparam>
        /// <param name="txtPath">Txt 路径</param>
        /// <param name="dataList">数据列表</param>
        /// <param name="splitChar">列分隔符，例：|</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static bool ToTxt<T>(string txtPath, List<T> dataList, string splitChar, System.Text.Encoding encoding)
        {
            bool result = ExecuteStreamWriter(txtPath, encoding, false, () =>
            {
                return StringHelper.ToString<T>(dataList, splitChar);
            });
            return result;
        }
        /// <summary>
        /// 实体对象数据写入 Txt
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="txtPath">Txt 路径</param>
        /// <param name="dataList">实体数据列表</param>
        /// <param name="splitChar">列分隔符，例：|</param>
        /// <param name="typeEnum">TxtTypeEnum 枚举类型</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="propertyList">属性列表，如果指定，则按指定属性列表生成文本数据</param>
        /// <param name="propertyContain">是否包含，true 属性包含，flase 属性排除</param>
        /// <param name="lineSplitChar">行分隔符，例：\r\n</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static bool ToTxt<T>(string txtPath, List<T> dataList, string splitChar, TxtTypeEnum typeEnum, object propertyMatchList = null, string[] propertyList = null, bool propertyContain = true, string lineSplitChar = NewLine, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            return ToTxt<T>(txtPath, dataList, splitChar, System.Text.Encoding.UTF8, typeEnum, propertyMatchList, propertyList, propertyContain, lineSplitChar, reflectionType);
        }
        /// <summary>
        /// 实体对象数据写入 Txt
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="txtPath">Txt 路径</param>
        /// <param name="dataList">实体数据列表</param>
        /// <param name="splitChar">列分隔符，例：|</param>
        /// <param name="typeEnum">TxtTypeEnum 枚举类型</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="propertyMatchList">属性匹配，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <param name="propertyList">属性列表，如果指定，则按指定属性列表生成文本数据</param>
        /// <param name="lineSplitChar">行分隔符，例：\r\n</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static bool ToTxt<T>(string txtPath, List<T> dataList, string splitChar, System.Text.Encoding encoding, TxtTypeEnum typeEnum, object propertyMatchList = null, string[] propertyList = null, bool propertyContain = true, string lineSplitChar = NewLine, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            bool result = ExecuteStreamWriter(txtPath, encoding, false, () =>
            {
                StringBuilder stringBuilder = new StringBuilder();

                Dictionary<int, PropertyInfo> propertyNameDict = InitEntityToTxtMapper<T>(propertyMatchList, typeEnum, propertyList, propertyContain);
                List<int> propertyIndexList = propertyNameDict.Keys.ToList<int>();

                propertyIndexList.Sort((int x, int y) =>
                {
                    if (x > y) return 1;
                    if (x < y) return -1;
                    return 0;
                });

                dynamic propertyGetDict = null;
                if (reflectionType != ReflectionTypeEnum.Original) propertyGetDict = ReflectionExtendHelper.PropertyGetCallDict<T>(reflectionType);

                foreach (T t in dataList)
                {
                    stringBuilder.Append(EntityToText<T>(t, splitChar, propertyGetDict, propertyNameDict, propertyIndexList, lineSplitChar));
                }
                return stringBuilder.ToString();
            });
            return result;
        }
        #endregion

        #endregion

        #region 逻辑处理私有方法
        private static void ExecuteStreamReader(string txtPath, System.Text.Encoding encoding, Action<StreamReader> callback)
        {
            using (StreamReader streamReader = new StreamReader(txtPath, encoding))
            {
                if (callback != null) callback(streamReader);
            }
        }
        private static bool ExecuteStreamWriter(string txtPath, System.Text.Encoding encoding, bool append, Func<string> callback)
        {
            // 根据路径创建目录
            bool createDirectoryStatus = FileHelper.CreateDirectory(txtPath);
            if (!createDirectoryStatus) return false;

            string content = null;
            if (callback != null) content = callback();

            using (StreamWriter streamWriter = new StreamWriter(txtPath, append, encoding))
            {
                streamWriter.Write(content);
                streamWriter.Flush();
            }
            return true;
        }

        #region ToEntity
        private static T TextToEntity<T>(string text, string splitChar, dynamic propertySetDict, Dictionary<PropertyInfo, int> propertyNameDict) where T : class, new()
        {
            // 按分隔符拆分文本内容
            string[] dataList = text.Split(new string[] { splitChar }, StringSplitOptions.None);

            T t = ReflectionGenericHelper.New<T>();
            foreach(var keyValueItem in propertyNameDict)
            {
                if (keyValueItem.Value >= 0 && keyValueItem.Value < dataList.Length)
                {
                    // 设置实体对象属性数据
                    if (propertySetDict != null && propertySetDict.ContainsKey(keyValueItem.Key.Name))
                    {
                        ReflectionGenericHelper.SetPropertyValue(propertySetDict[keyValueItem.Key.Name], t, dataList[keyValueItem.Value], keyValueItem.Key);
                    }
                    else
                    {
                        ReflectionHelper.SetPropertyValue(t, dataList[keyValueItem.Value], keyValueItem.Key);
                    }
                }
            }
            return t;
        }
        private static Dictionary<PropertyInfo, int> InitTxtToEntityMapper<T>(Dictionary<string, object> propertyMatch = null, TxtTypeEnum typeEnum = TxtTypeEnum.Normal) where T : class
        {
            int propertyIndex = 0;
            Dictionary<PropertyInfo, int> propertyDict = new Dictionary<PropertyInfo, int>();
            ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
            {
                if (propertyMatch != null && propertyMatch.ContainsKey(propertyInfo.Name))
                {
                    propertyDict.Add(propertyInfo, int.Parse(propertyMatch[propertyInfo.Name].ToString()));
                }
                else
                {
                    if (typeEnum == TxtTypeEnum.Attribute)
                    {
                        TxtTAttribute attribute = propertyInfo.GetCustomAttribute<TxtTAttribute>();
                        if (attribute != null && (attribute.Type == AttributeReadAndWriteTypeEnum.ReadAndWrite || attribute.Type == AttributeReadAndWriteTypeEnum.Read))
                        {
                            propertyDict.Add(propertyInfo, attribute.Index);
                        }
                    }
                    else
                    {
                        propertyDict.Add(propertyInfo, propertyIndex);
                        propertyIndex++;
                    }
                }
            });
            return propertyDict;
        }
        #endregion

        #region ToTxt
        private static string EntityToText<T>(T t, string splitChar, dynamic propertyGetDict, Dictionary<int, PropertyInfo> propertyNameDict, List<int> propertyIndexList, string lineSplitChar = NewLine) where T : class
        {
            Type type = typeof(T);

            object propertyValue = null;

            StringBuilder stringBuilder = new StringBuilder();

            int count = propertyIndexList.Count;
            for (int index = 0; index < count; index++)
            {
                string propertyName = propertyNameDict[propertyIndexList[index]].Name;
                if (propertyGetDict != null && propertyGetDict.ContainsKey(propertyName))
                {
                    propertyValue = propertyGetDict[propertyName](t);
                }
                else
                {
                    propertyValue = ReflectionHelper.GetPropertyValue(t, propertyName);
                }
                if (propertyValue != null) stringBuilder.Append(propertyValue.ToString());
                if (index < count - 1)
                {
                    stringBuilder.Append(splitChar);
                }
            }
            stringBuilder.Append(lineSplitChar);
            return stringBuilder.ToString();
        }
        private static Dictionary<int, PropertyInfo> InitEntityToTxtMapper<T>(object propertyMatchList = null, TxtTypeEnum typeEnum = TxtTypeEnum.Normal, string[] propertyList = null, bool propertyContain = true) where T : class
        {
            List<string> filterNameList = null;
            if (propertyList != null) filterNameList = propertyList.ToList<string>();

            int propertyIndex = 0;

            Dictionary<string, object> propertyMatchDict = CommonHelper.GetParameterDict(propertyMatchList);
            Dictionary<int, PropertyInfo> propertyNameDict = new Dictionary<int, PropertyInfo>();

            ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
            {
                if (filterNameList == null || (propertyContain && filterNameList.IndexOf(propertyInfo.Name) >= 0) || (!propertyContain && filterNameList.IndexOf(propertyInfo.Name) < 0))
                {
                    if (propertyMatchDict != null && propertyMatchDict.ContainsKey(propertyInfo.Name))
                    {
                        propertyNameDict.Add(int.Parse(propertyMatchDict[propertyInfo.Name].ToString()), propertyInfo);
                    }
                    else
                    {
                        if (typeEnum == TxtTypeEnum.Attribute)
                        {
                            TxtTAttribute attribute = propertyInfo.GetCustomAttribute<TxtTAttribute>();
                            if (attribute != null && (attribute.Type == AttributeReadAndWriteTypeEnum.ReadAndWrite || attribute.Type == AttributeReadAndWriteTypeEnum.Write))
                            {
                                object attributeIndex = ReflectionExtendHelper.GetAttributeValue<TxtTAttribute>(typeof(T), propertyInfo, p => p.Index);
                                if (attributeIndex != null) propertyNameDict.Add(int.Parse(attributeIndex.ToString()), propertyInfo);
                            }
                        }
                        else
                        {
                            propertyNameDict.Add(propertyIndex, propertyInfo);
                            propertyIndex++;
                        }
                    }
                }
            });
            return propertyNameDict;
        }
        #endregion

        #endregion
    }

    #region 逻辑处理辅助特性
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class TxtTAttribute : Attribute
    {
        private int index;
        private AttributeReadAndWriteTypeEnum type;

        /// <summary>
        /// 实体属性映射 Txt 列索引
        /// </summary>
        /// <param name="index">列索引，索引 从 1 开始</param>
        public TxtTAttribute(int index, AttributeReadAndWriteTypeEnum type = AttributeReadAndWriteTypeEnum.ReadAndWrite)
        {
            this.index = index;
            this.type = type;
        }

        /// <summary>
        /// 实体属性所对应的 Txt 列索引，索引 从 1 开始
        /// </summary>
        public int Index { get { return this.index; } }

        /// <summary>
        /// 是否写入
        /// </summary>
        public AttributeReadAndWriteTypeEnum Type { get { return this.type; } }
    }
    #endregion
}