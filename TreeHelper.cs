/*
 * 树形数据生成。
 * */
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Helper.Core.Library
{
    public class TreeHelper
    {
        #region 私有属性常量
        private const string TreeUniqueIdentityFormat = "1{0}{1}";
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 生成树形 JSON 数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataList">数据列表</param>
        /// <param name="maxLength">最大编号长度，为 0 程序自动生成</param>
        /// <param name="rootID">根节点 ID</param>
        /// <param name="jsonTemplate">Json 模板，例：{ id:1, name:"name" }</param>
        /// <returns></returns>
        public static string ToJson<T>(List<T> dataList, int maxLength = 0, string rootID = "0", ITreeJsonTemplate<T> jsonTemplate = null) where T : ITreeData
        {
            if (dataList == null || dataList.Count == 0) return "{}";
            List<TreeNode<T>> nodeList = ToTree<T>(dataList, maxLength, rootID, jsonTemplate);
            if (nodeList == null || nodeList.Count == 0) return "{}";

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            int index = 0;
            foreach(TreeNode<T> nodeItem in nodeList)
            {
                stringBuilder.Append(nodeItem.JsonText);
                if (index < nodeList.Count - 1) stringBuilder.Append(",");
                index++;
            }
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 获取下拉列表树形数据（以 / 符号分隔）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataList">数据列表</param>
        /// <param name="rootID">根节点 ID</param>
        /// <param name="layerIndex">层次索引</param>
        /// <param name="layerName">层次名称，会以 / 符号连接各节点名称</param>
        /// <param name="desc">是否倒序</param>
        /// <param name="splitChar">分隔符，例：/</param>
        /// <returns></returns>
        public static List<T> ToMenuList<T>(List<T> dataList, int rootID = 0, int layerIndex = 1, string layerName = "", bool desc = true, string splitChar = "/") where T : ITreeMenu
        {
            List<T> resultList = new List<T>();

            var query = dataList.Where(p => p.ParentID == rootID);
            if (!desc)
            {
                query = query.OrderBy(p => p.TreeSort);
            }
            else
            {
                query = query.OrderByDescending(p => p.TreeSort);
            }

            List<T> trunkList = query.ToList();
            if (trunkList != null && trunkList.Count > 0)
            {
                foreach (T trunkItem in trunkList)
                {
                    trunkItem.LayerIndex = layerIndex;
                    if (layerIndex == 1)
                    {
                        trunkItem.LayerName = trunkItem.TreeName;
                    }
                    else
                    {
                        trunkItem.LayerName = string.Format("{0}{2}{1}", layerName, trunkItem.TreeName, splitChar);
                    }
                    resultList.Add(trunkItem);
                    List<T> nodeList = ToMenuList<T>(dataList, trunkItem.TreeID, layerIndex + 1, trunkItem.LayerName);
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        resultList.AddRange(nodeList);
                    }
                }
            }
            return resultList;
        }
        /// <summary>
        /// 获取树形数据（按顺序）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataList">数据列表</param>
        /// <param name="maxLength">最大编号长度，为 0 程序自动生成</param>
        /// <param name="rootID">根节点 ID</param>
        /// <returns></returns>
        public static List<TreeNode<T>> ToList<T>(List<T> dataList, int maxLength = 0, int rootID = 0) where T : ITreeData
        {
            List<TreeNode<T>> nodeList = TreeHelper.ToTree<T>(dataList, maxLength, rootID.ToString());
            return ToNodeList<T>(nodeList);
        }
        /// <summary>
        /// 获取树形数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dataList">数据列表</param>
        /// <param name="maxLength">最大编号长度，为 0 程序自动生成</param>
        /// <param name="rootID">根节点 ID</param>
        /// <returns></returns>
        public static List<TreeNode<T>> ToTree<T>(List<T> dataList, int maxLength = 0, string rootID = "0", ITreeJsonTemplate<T> jsonTemplate = null) where T : ITreeData
        {
            #region 设置排序序号
            if (maxLength > 0)
            {
                foreach (T t in dataList)
                {
                    t.TreeUniqueIdentity = long.Parse(string.Format(TreeUniqueIdentityFormat, t.TreeParentID.PadLeft(maxLength, '0'), t.TreeID.PadLeft(maxLength, '0')));
                }
            }
            #endregion

            #region 根据排序序号排序
            dataList.Sort((x, y) =>
            {
                return (int)(x.TreeUniqueIdentity - y.TreeUniqueIdentity);
            });
            #endregion

            TreeNode<T> treeNode = null;

            List<TreeNode<T>> nodeList = new List<TreeNode<T>>();
            Dictionary<string, TreeNode<T>> nodeDict = new Dictionary<string, TreeNode<T>>();
            foreach (T t in dataList)
            {
                // 设置根数据
                if (t.TreeParentID == rootID)
                {
                    treeNode = new TreeNode<T>() { Data = t, NodeList = new List<TreeNode<T>>(), LayerIndex = 1, IJsonTemplate = jsonTemplate };
                    nodeList.Add(treeNode);
                    nodeDict.Add(t.TreeID, treeNode);
                }
                else
                {
                    if (nodeDict.ContainsKey(t.TreeParentID))
                    {
                        treeNode = new TreeNode<T>() { Data = t, NodeList = new List<TreeNode<T>>(), LayerIndex = nodeDict[t.TreeParentID].LayerIndex + 1, IJsonTemplate = jsonTemplate };
                        treeNode.ParentNode = nodeDict[t.TreeParentID];
                        treeNode.ParentNode.NodeList.Add(treeNode);
                        nodeDict.Add(t.TreeID, treeNode);
                    }
                }
            }
            return nodeList;
        }
        #endregion

        #region 逻辑处理私有方法
        private static List<TreeNode<T>> ToNodeList<T>(List<TreeNode<T>> nodeList, List<TreeNode<T>> resultList = null)
        {
            if (resultList == null)
            {
                resultList = new List<TreeNode<T>>();
            }

            foreach (TreeNode<T> nodeItem in nodeList)
            {
                resultList.Add(nodeItem);
                ToNodeList<T>(nodeItem.NodeList, resultList);
            }

            return resultList;
        }
        #endregion
    }

    #region 逻辑处理辅助类
    public class TreeNode<T>
    {
        /// <summary>
        /// 节点层级索引
        /// </summary>
        public int LayerIndex { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        public TreeNode<T> ParentNode { get; set; }

        /// <summary>
        /// 节点列表
        /// </summary>
        public List<TreeNode<T>> NodeList { get; set; }

        /// <summary>
        /// 节点数据
        /// </summary>
        public T Data { get; set; }

        public ITreeJsonTemplate<T> IJsonTemplate { get; set; }

        /// <summary>
        /// Json 数据
        /// </summary>
        public string JsonText
        {
            get
            {
                if (this.IJsonTemplate == null) return "{}";

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("{");
                stringBuilder.Append(this.IJsonTemplate.Format(this));
                if(this.NodeList != null && this.NodeList.Count > 0)
                {
                    stringBuilder.Append(", \\\"dataList\\\":[");
                }
                int index = 0;
                foreach (TreeNode<T> t in this.NodeList)
                {
                    stringBuilder.Append(t.JsonText);
                    if (index < this.NodeList.Count - 1) stringBuilder.Append(",");
                    index++;
                }
                if(this.NodeList != null && this.NodeList.Count > 0)
                {
                    stringBuilder.Append("]");
                }
                stringBuilder.Append("}");
                return stringBuilder.ToString();
            }
        }
    }
    #endregion

    #region 逻辑处理辅助接口
    public interface ITreeData
    {
        /// <summary>
        /// 树 ID
        /// </summary>
        string TreeID { get; }

        /// <summary>
        /// 树父 ID
        /// </summary>
        string TreeParentID { get; }

        /// <summary>
        /// 树排序标识，通过是 父 ID + 树 ID 组合
        /// </summary>
        long TreeUniqueIdentity { get; set; }
    }
    public interface ITreeJsonTemplate<T>
    {
        string Format(TreeNode<T> t);
    }
    public interface ITreeMenu
    {
        int ParentID { get; set; }
        int TreeID { get; set; }
        string TreeName { get; set; }
        int LayerIndex { get; set; }
        string LayerName { get; set; }
        int TreeSort { get; set; }
    }
    #endregion
}
