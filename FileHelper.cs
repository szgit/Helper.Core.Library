using System.IO;

namespace Helper.Core.Library
{
    public class FileHelper
    {
        #region 对外公开方法

        /// <summary>
        /// 根据文件路径获取后缀名
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static string GetSuffix(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            return Path.GetExtension(path);
        }
        /// <summary>
        /// 根据文件路径创建目录
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="isDirectory">是否目录</param>
        /// <returns></returns>
        public static bool CreateDirectory(string path, bool isDirectory = false)
        {
            if (string.IsNullOrEmpty(path)) return false;

            path = path.Replace("/", "\\");

            string directoryPath = path;
            if (!isDirectory) directoryPath = path.Substring(0, path.LastIndexOf("\\"));

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return true;
        }

        #endregion
    }
}
