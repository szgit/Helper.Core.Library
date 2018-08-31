/*
 * 作用：根据文件名获取这个文件对应的 Mime 类型。
 * */
using System.Collections.Generic;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    public class MimeTypeEnum
    {
        public const string ALL = "*";
        public const string DOC = "doc";
        public const string EXE = "exe";
        public const string PDF = "pdf";
        public const string XLS = "xls";
        public const string XLSX = "xlsx";
        public const string GZ = "gz";
        public const string ZIP = "zip";
        public const string MID = "mid";
        public const string MP3 = "mp3";
        public const string WAV = "wav";
        public const string BMP = "bmp";
        public const string GIF = "gif";
        public const string JPEG = "jpeg";
        public const string JPG = "jpg";
        public const string CSS = "css";
        public const string HTM = "htm";
        public const string HTML = "html";
        public const string TXT = "txt";
    }
    #endregion

    public class MimeHelper
    {
        #region 私有属性常量
        private static Dictionary<string, string> MineDict = new Dictionary<string, string>()
        {
            {MimeTypeEnum.ALL, "application/octet-stream"},
            {MimeTypeEnum.DOC, "application/msword"},
            {MimeTypeEnum.EXE, "application/octet-stream"},
            {MimeTypeEnum.PDF, "application/pdf"},
            {MimeTypeEnum.XLS, "application/vnd.ms-excel"},
            {MimeTypeEnum.XLSX, "application/vnd.ms-excel"},
            {MimeTypeEnum.GZ, "application/x-gzip"},
            {MimeTypeEnum.ZIP, "application/zip"},
            {MimeTypeEnum.MID, "audio/mid"},
            {MimeTypeEnum.MP3, "audio/mpeg"},
            {MimeTypeEnum.WAV, "audio/x-wav"},
            {MimeTypeEnum.BMP, "image/bmp"},
            {MimeTypeEnum.GIF, "image/gif"},
            {MimeTypeEnum.JPEG, "image/jpeg"},
            {MimeTypeEnum.JPG, "image/jpeg"},
            {MimeTypeEnum.CSS, "text/css"},
            {MimeTypeEnum.HTM, "text/html"},
            {MimeTypeEnum.HTML, "text/html"},
            {MimeTypeEnum.TXT, "text/plain"}
        };
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 获得 Mine 类型
        /// </summary>
        /// <param name="filePath">文件地址</param>
        /// <param name="isSuffix">是否后缀名</param>
        /// <returns></returns>
        public static string GetMineType(string filePath, bool isSuffix = false)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            string suffix = filePath;
            if (!isSuffix)
            {
                suffix = FileHelper.GetSuffix(suffix);
                if (!string.IsNullOrEmpty(suffix)) suffix = suffix.TrimStart(new char[] { '.' });
            }
            if (MineDict.ContainsKey(suffix)) return MineDict[suffix];
            return null;
        }
        #endregion
    }
}
