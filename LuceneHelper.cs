/*
 * 作用：通过 Lucene.Net 和盘古分词实现索引创建/查询。
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using PanGu;
using Lucene.Net.Index;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using System.Reflection;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using System.Linq.Expressions;

namespace Helper.Core.Library
{
    public class LuceneHelper
    {
        #region 私有属性常量
        private const string DESC = "desc";
        /// <summary>
        /// 索引目录
        /// </summary>
        private static Directory IndexDictDirectory;
        private static Directory RAMDirectory;
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 初始化索引文件存放地址和盘古分词所在目录
        /// </summary>
        /// <param name="indexDictPath">索引文件存放地址</param>
        /// <param name="panGuSegmentPath">盘古分词所在地址</param>
        public static bool Init(string indexDictPath, string panGuSegmentPath)
        {
            try
            {
                System.IO.DirectoryInfo directoryInfo = null;
                if (!System.IO.Directory.Exists(indexDictPath))
                {
                    directoryInfo = System.IO.Directory.CreateDirectory(indexDictPath);
                }
                else
                {
                    directoryInfo = new System.IO.DirectoryInfo(indexDictPath);
                }
                IndexDictDirectory = FSDirectory.Open(directoryInfo);
                Segment.Init(panGuSegmentPath);
                return true;
            }
            catch
            {
                if (IndexDictDirectory != null) IndexDictDirectory.Close();
                throw;
            }
        }
        /// <summary>
        /// 设置索引
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataList">实体数据列表</param>
        /// <param name="primaryKey">主键，默认为空，表示添加索引，如果主键不为空，则表示按主键更新索引</param>
        /// <param name="propertyList">属性列表，如果指定，则按指定属性列表生成索引数据</param>
        /// <param name="maxBufferedDocs">最小合并文档数</param>
        /// <param name="maxMergeFactory">最小合并因子</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static bool Set<T>(List<T> dataList, string primaryKey = "", string[] propertyList = null, int maxBufferedDocs = 1000, int maxMergeFactory = 1000, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            List<string> filterNameList = null;
            if (propertyList != null) filterNameList = propertyList.ToList<string>();

            return ExecuteSet<T>(dataList, primaryKey, filterNameList, maxBufferedDocs, maxMergeFactory, reflectionType);
        }
        /// <summary>
        /// 设置索引
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataList">实体数据列表</param>
        /// <param name="propertyExpression">属性筛选列表，例：p=> new { p.UserID }</param>
        /// <param name="primaryKey">主键，默认为空，表示添加索引，如果主键不为空，则表示按主键更新索引</param>
        /// <param name="maxBufferedDocs">最小合并文档数</param>
        /// <param name="maxMergeFactory">最小合并因子</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static bool Set<T>(List<T> dataList, Expression<Func<T, object>> propertyExpression, string primaryKey = "", int maxBufferedDocs = 1000, int maxMergeFactory = 1000, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            List<string> filterNameList = CommonHelper.GetExpressionList<T>(propertyExpression);
            return ExecuteSet<T>(dataList, primaryKey, filterNameList, maxBufferedDocs, maxMergeFactory, reflectionType);
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="queryFieldList">查询字段列表</param>
        /// <param name="search">搜索内容</param>
        /// <param name="sortField">排序字段，例："id desc"</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> Query<T>(string[] queryFieldList, string search, string sortField, int pageIndex, int pageSize, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            return Query<T>(queryFieldList, search, null, sortField, pageIndex, pageSize, reflectionType);
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="queryFieldList">查询字段列表</param>
        /// <param name="search">搜索内容</param>
        /// <param name="filter">Filter</param>
        /// <param name="sortField">排序字段，例："id desc"</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="reflectionType">反射类型</param>
        /// <returns></returns>
        public static List<T> Query<T>(string[] queryFieldList, string search, Filter filter, string sortField, int pageIndex, int pageSize, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class, new()
        {
            IndexSearcher indexSearcher = null;
            try
            {
                List<T> dataList = new List<T>();

                BooleanQuery booleanQuery = new BooleanQuery();
                if (!string.IsNullOrEmpty(search))
                {
                    QueryParser queryParser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, queryFieldList, new PanGuAnalyzer());
                    Query query = queryParser.Parse(search);
                    booleanQuery.Add(query, Lucene.Net.Search.BooleanClause.Occur.MUST);
                }

                indexSearcher = new IndexSearcher(IndexDictDirectory, true);

                bool desc = false;
                string[] sortFieldList = sortField.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (sortFieldList != null && sortFieldList.Length == 2 && sortFieldList[1] == DESC) desc = true;

                Sort sort = new Sort(new SortField(sortField, SortField.DOC, desc));

                dynamic propertySetDict = null;
                if (reflectionType != ReflectionTypeEnum.Original) ReflectionExtendHelper.PropertySetCallDict<T>(reflectionType);

                TopDocs topDocs = indexSearcher.Search(booleanQuery, filter, pageIndex * pageSize, sort);
                if (topDocs != null && topDocs.totalHits > 0)
                {
                    Document document = null;
                    for (int i = 0; i < topDocs.totalHits; i++)
                    {
                        document = indexSearcher.Doc(topDocs.scoreDocs[i].doc);
                        dataList.Add(DocumentToEntity<T>(document, propertySetDict));
                    }
                }
                indexSearcher.Close();
                return dataList;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (indexSearcher != null) indexSearcher.Close();
            }
        }

        #endregion

        #region 逻辑处理私有方法
        private static bool ExecuteSet<T>(List<T> dataList, string primaryKey = "", List<string> propertyList = null, int maxBufferedDocs = 1000, int maxMergeFactory = 1000, ReflectionTypeEnum reflectionType = ReflectionTypeEnum.Expression) where T : class
        {
            IndexWriter indexWriter = null;
            IndexWriter RAMWriter = null;
            try
            {
                RAMDirectory = new RAMDirectory();

                bool create = IndexReader.IndexExists(IndexDictDirectory);
                if (create)
                {
                    if (IndexWriter.IsLocked(IndexDictDirectory)) IndexWriter.Unlock(IndexDictDirectory);
                }

                dynamic propertyGetDict = null;
                if (reflectionType != ReflectionTypeEnum.Original) propertyGetDict = ReflectionExtendHelper.PropertyGetCallDict<T>(reflectionType);

                indexWriter = new IndexWriter(IndexDictDirectory, new PanGuAnalyzer(), !create, IndexWriter.MaxFieldLength.LIMITED);
                RAMWriter = new IndexWriter(RAMDirectory, new PanGuAnalyzer(), true, IndexWriter.MaxFieldLength.LIMITED);

                RAMWriter.SetMaxBufferedDocs(maxBufferedDocs);
                RAMWriter.SetMergeFactor(maxMergeFactory);

                List<List<LuceneIndexModel>> modelList = EntityListToLuceneIndexModelList(dataList, propertyGetDict, propertyList);

                Document document = null;
                Term term = null;
                string primaryValue = null;

                foreach (List<LuceneIndexModel> model in modelList)
                {
                    document = new Document();
                    foreach (LuceneIndexModel fieldModel in model)
                    {
                        if (fieldModel.Name == primaryKey) primaryValue = fieldModel.Value;
                        document.Add(new Field(fieldModel.Name, fieldModel.Value, fieldModel.StoreEnum, fieldModel.IndexEnum));
                    }
                    if (string.IsNullOrEmpty(primaryKey))
                    {
                        RAMWriter.AddDocument(document);
                    }
                    else
                    {
                        term = new Term(primaryKey, primaryValue);
                        RAMWriter.UpdateDocument(term, document);
                    }
                }
                RAMWriter.Close();

                indexWriter.AddIndexesNoOptimize(new Directory[] { RAMDirectory });
                indexWriter.Optimize();
                indexWriter.Close();
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (indexWriter != null) indexWriter.Close();
            }
        }
        private static T DocumentToEntity<T>(Document document, dynamic propertySetDict) where T : class, new()
        {
            T t = ReflectionGenericHelper.New<T>();

            ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
            {
                LuceneIndexTAttribute attribute = propertyInfo.GetCustomAttribute<LuceneIndexTAttribute>();
                if(attribute != null)
                {
                    string field = document.Get(propertyInfo.Name);
                    if (!string.IsNullOrEmpty(field))
                    {
                        if (propertySetDict != null && propertySetDict.ContainsKey(propertyInfo.Name))
                        {
                            ReflectionGenericHelper.SetPropertyValue(propertySetDict[propertyInfo.Name], t, field, propertyInfo);
                        }
                        else
                        {
                            ReflectionHelper.SetPropertyValue(t, field, propertyInfo);
                        }
                    }
                }
            });

            return t;
        }
        private static List<List<LuceneIndexModel>> EntityListToLuceneIndexModelList<T>(List<T> dataList, dynamic propertyGetDict, List<string> propertyList) where T : class
        {
            List<List<LuceneIndexModel>> resultList = new List<List<LuceneIndexModel>>();

            List<LuceneIndexModel> modelList = new List<LuceneIndexModel>();
            foreach (T t in dataList)
            {
                modelList = EntityToLuceneIndexModel<T>(t, propertyGetDict, propertyList);
                if (modelList != null && modelList.Count > 0) resultList.Add(modelList);
            }

            return resultList;
        }
        private static List<LuceneIndexModel> EntityToLuceneIndexModel<T>(T t, dynamic propertyGetDict, List<string> propertyList) where T : class
        {
            List<LuceneIndexModel> modelList = new List<LuceneIndexModel>();
            ReflectionGenericHelper.Foreach<T>((PropertyInfo propertyInfo) =>
            {
                if (propertyList == null || propertyList.IndexOf(propertyInfo.Name) < 0)
                {
                    object value = null;
                    if (propertyGetDict != null && propertyGetDict.ContainsKey(propertyInfo.Name))
                    {
                        value = propertyGetDict[propertyInfo.Name](t);
                    }
                    else
                    {
                        value = ReflectionHelper.GetPropertyValue(t, propertyInfo);
                    }
                    LuceneIndexTAttribute attribute = propertyInfo.GetCustomAttribute<LuceneIndexTAttribute>();
                    if (attribute != null)
                    {
                        LuceneIndexModel model = new LuceneIndexModel()
                        {
                            Name = propertyInfo.Name,
                            Value = value != null ? value.ToString() : "",
                            StoreEnum = GetIndexStoreEnum(attribute.StoreEnum),
                            IndexEnum = GetIndexIndexEnum(attribute.IndexEnum)
                        };
                        modelList.Add(model);
                    }
                }
            });

            return modelList;
        }
        private static Field.Store GetIndexStoreEnum(LuceneIndexStoreEnum storeEnum)
        {
            if (storeEnum == LuceneIndexStoreEnum.YES)
            {
                return Field.Store.YES;
            }
            else if (storeEnum == LuceneIndexStoreEnum.NO)
            {
                return Field.Store.NO;
            }
            else if (storeEnum == LuceneIndexStoreEnum.COMPRESS)
            {
                return Field.Store.COMPRESS;
            }
            return null;
        }
        private static Field.Index GetIndexIndexEnum(LuceneIndexIndexEnum indexEnum)
        {
            if (indexEnum == LuceneIndexIndexEnum.ANALYZED)
            {
                return Field.Index.ANALYZED;
            }
            else if (indexEnum == LuceneIndexIndexEnum.ANALYZED_NO_NORMS)
            {
                return Field.Index.ANALYZED_NO_NORMS;
            }
            else if (indexEnum == LuceneIndexIndexEnum.NO)
            {
                return Field.Index.NO;
            }
            else if (indexEnum == LuceneIndexIndexEnum.NOT_ANALYZED)
            {
                return Field.Index.NOT_ANALYZED;
            }
            else if (indexEnum == LuceneIndexIndexEnum.NOT_ANALYZED_NO_NORMS)
            {
                return Field.Index.NOT_ANALYZED_NO_NORMS;
            }
            return null;
        }
        #endregion
    }

    #region 逻辑处理辅助类
    internal class LuceneIndexModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public Field.Store StoreEnum { get; set; }
        public Field.Index IndexEnum { get; set; }
    }
    #endregion

    #region 逻辑处理辅助特性
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class LuceneIndexTAttribute : Attribute
    {
        private LuceneIndexStoreEnum storeEnum;
        private LuceneIndexIndexEnum indexEnum;

        public LuceneIndexTAttribute(LuceneIndexStoreEnum storeEnum, LuceneIndexIndexEnum indexEnum)
        {
            this.storeEnum = storeEnum;
            this.indexEnum = indexEnum;
        }

        public LuceneIndexStoreEnum StoreEnum { get { return this.storeEnum; } }
        public LuceneIndexIndexEnum IndexEnum { get { return this.indexEnum; } }
    }
    public enum LuceneIndexStoreEnum
    {
        YES,
        NO,
        COMPRESS
    }
    public enum LuceneIndexIndexEnum
    {
        ANALYZED,
        ANALYZED_NO_NORMS,
        NO,
        NOT_ANALYZED,
        NOT_ANALYZED_NO_NORMS
    }
    #endregion
}
