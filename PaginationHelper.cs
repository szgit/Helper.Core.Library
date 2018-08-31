/*
 * 作用：分页显示，显示格式为：数据总数 页总数 首页/前一页 N页 后一页/尾页。
 * */
using System;
using System.Text;

namespace Helper.Core.Library
{
    public class PaginationHelper
    {
        #region 私有属性常量
        private const string SPAN_FORMAT = "<span class=\"{2}\">共<i class=\"{3}\">{0}</i>条数据，共<i class=\"{4}\">{1}</i>页！</span>";
        private const string UL_FORMAT = "<ul class=\"{0}\" style=\"{2}\">{1}</ul>";
        private const string LI_FORMAT = "<li class=\"{0}\" style=\"{3}\"><a href=\"{1}\">{2}</a></li>";
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 显示分页
        /// </summary>
        /// <param name="paginationData">PaginationData</param>
        /// <returns></returns>
        public static string Page(PaginationData paginationData)
        {
            if (paginationData.ShowPageCount % 2 == 0) paginationData.ShowPageCount += 1;

            int beginIndex = 0;
            int endIndex = 0;

            int halfCount = (int)Math.Floor(paginationData.ShowPageCount * 0.5f);

            beginIndex = Math.Max(1, paginationData.PageIndex - halfCount);
            endIndex = Math.Min(paginationData.PageCount, paginationData.PageIndex + halfCount);

            if (endIndex < paginationData.PageCount) endIndex = Math.Min(beginIndex - 1 + paginationData.ShowPageCount, paginationData.PageCount);
            if (beginIndex > paginationData.PageCount - paginationData.ShowPageCount) beginIndex = Math.Max(1, paginationData.PageCount - paginationData.ShowPageCount + 1);

            StringBuilder stringBuilder = new StringBuilder();
            if(paginationData.IsShowRecords)
            {
                stringBuilder.Append(string.Format(SPAN_FORMAT, paginationData.TotalCount, paginationData.PageCount, paginationData.RecordsClassName, paginationData.RecordsTotalCountClassName, paginationData.RecordsPageCountClassName));
            }

            #region 分页显示字符串拼装
            if (paginationData.IsFull && paginationData.PageIndex > 1)
            {
                stringBuilder.Append(string.Format(LI_FORMAT, paginationData.ItemClassName, paginationData.PageUrl.Replace(paginationData.PageFormat, "1"), paginationData.PageFirstText, paginationData.ItemStyle));
            }
            if (paginationData.IsFull && paginationData.PageIndex - 1 >= 1)
            {
                stringBuilder.Append(string.Format(LI_FORMAT, paginationData.ItemClassName, paginationData.PageUrl.Replace(paginationData.PageFormat, (paginationData.PageIndex - 1).ToString()), paginationData.PagePrevText, paginationData.ItemStyle));
            }
            for (int currentIndex = beginIndex; currentIndex <= endIndex; currentIndex++)
            {
                string itemClassName = paginationData.ItemClassName + " ";
                if(currentIndex == paginationData.PageIndex)
                {
                    itemClassName = itemClassName + paginationData.ActiveItemClassName;
                }
                stringBuilder.Append(string.Format(LI_FORMAT, itemClassName, paginationData.PageUrl.Replace(paginationData.PageFormat, currentIndex.ToString()), string.Format(paginationData.PageText, currentIndex), paginationData.ItemStyle));
            }
            if (paginationData.IsFull && paginationData.PageIndex + 1 <= paginationData.PageCount)
            {
                stringBuilder.Append(string.Format(LI_FORMAT, paginationData.ItemClassName, paginationData.PageUrl.Replace(paginationData.PageFormat, (paginationData.PageIndex + 1).ToString()), paginationData.PageNextText, paginationData.ItemStyle));
            }
            if (paginationData.IsFull && paginationData.PageIndex < paginationData.PageCount)
            {
                stringBuilder.Append(string.Format(LI_FORMAT, paginationData.ItemClassName, paginationData.PageUrl.Replace(paginationData.PageFormat, paginationData.PageCount.ToString()), paginationData.PageLastText, paginationData.ItemStyle));
            }
            #endregion

            return string.Format(UL_FORMAT, paginationData.ClassName, stringBuilder.ToString(), paginationData.Style);
        }
        #endregion
    }

    #region 逻辑处理辅助类
    public class PaginationData
    {
        #region 属性默认值
        private int _showPageCount = 5;

        private string _pageUrl = "-1";
        private string _pageFormat = "-1";
        private string _pageFirstText = "First";
        private string _pagePrevText = "Prev";
        private string _pageText = "{0}";
        private string _pageNextText = "Next";
        private string _pageLastText = "Last";

        private bool _isFull = true;
        private bool _isShowRecords = false;

        private string _activeItemClassName = "active";
        #endregion

        /// <summary>
        /// 当前页索引
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 格式化页索引，默认 -1
        /// </summary>
        public string PageFormat { get { return this._pageFormat; } set { this._pageFormat = value; } }
        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount { get; set; }
        /// <summary>
        /// 数据总数
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// 显示的页数数量，当设置偶数值时，会默认加 1
        /// </summary>
        public int ShowPageCount { get { return this._showPageCount; } set { this._showPageCount = value; } }
        /// <summary>
        /// 页面 URL 地址，其中页索引用 -1 代替，例：topiclist/-1 或 topiclist.html?pageIndex=-1
        /// </summary>
        public string PageUrl { get { return this._pageUrl; } set { this._pageUrl = value; } }
        /// <summary>
        /// 首页显示文本，默认值：First
        /// </summary>
        public string PageFirstText { get { return this._pageFirstText; } set { this._pageFirstText = value; } }
        /// <summary>
        /// 上一页显示文本，默认值：Prev
        /// </summary>
        public string PagePrevText { get { return this._pagePrevText; } set { this._pagePrevText = value; } }
        /// <summary>
        /// 页面显示文本，页索引用 {0} 代替，例：第 {0} 页
        /// </summary>
        public string PageText { get { return this._pageText; } set { this._pageText = value; } }
        /// <summary>
        /// 下一页显示文本，默认值：Next
        /// </summary>
        public string PageNextText { get { return this._pageNextText; } set { this._pageNextText = value; } }
        /// <summary>
        /// 尾页显示文本，默认值：Last
        /// </summary>
        public string PageLastText { get { return this._pageLastText; } set { this._pageLastText = value; } }
        /// <summary>
        /// 分页 class 信息，默认为空
        /// </summary>
        public string ClassName { get; set; }
        /// <summary>
        /// 分布 style 信息，默认为空
        /// </summary>
        public string Style { get; set; }
        /// <summary>
        /// 分页页索引 class 信息，默认为空
        /// </summary>
        public string ItemClassName { get; set; }
        /// <summary>
        /// 分页页索引 style 信息，默认为空
        /// </summary>
        public string ItemStyle { get; set; }
        /// <summary>
        /// 选中页索引 class 信息，默认为 active
        /// </summary>
        public string ActiveItemClassName { get { return this._activeItemClassName; } set { this._activeItemClassName = value; } }
        /// <summary>
        /// 是否显示首页/上一页/下一页/尾页
        /// </summary>
        public bool IsFull { get { return this._isFull; } set { this._isFull = value; } }
        /// <summary>
        /// 是否显示记录数
        /// </summary>
        public bool IsShowRecords { get { return this._isShowRecords; } set { this._isShowRecords = value; } }
        /// <summary>
        /// 记录 class 信息
        /// </summary>
        public string RecordsClassName { get; set; }
        /// <summary>
        /// 记录数据总数 class 信息
        /// </summary>
        public string RecordsTotalCountClassName { get; set; }
        /// <summary>
        /// 记录页总数 class 信息
        /// </summary>
        public string RecordsPageCountClassName { get; set; }
    }
    #endregion
}
