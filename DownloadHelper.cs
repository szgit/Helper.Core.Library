/*
 * 作用：输出文件流提供下载/从 URL 下载文件并保存。
 * */
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Helper.Core.Library
{
    #region 逻辑辅助枚举类型
    public enum DownloadTypeEnum
    {
        /// <summary>
        /// 文件大小无限制
        /// </summary>
        TransmitFile = 1,

        /// <summary>
        /// 小于 2G 文件
        /// </summary>
        WriteFile = 2,

        /// <summary>
        /// 小于 2G 文件
        /// </summary>
        BinaryWrite = 3,

        /// <summary>
        /// 文件大小无限制
        /// </summary>
        WriteFilePortion = 4
    }
    #endregion

    public class DownloadHelper
    {
        #region 私有属性常量
        private const int CHUNKSIZE = 102400; //100K
        private const int MAXSIZE = int.MaxValue;

        private const string SizeOver2GException = "文件大小超过 2G！";
        private const string DirectoryCreateFailed = "目录创建失败！";
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 生成文件流
        /// </summary>
        /// <param name="response">HttpResponseBase</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名称，为空，则根据 GUID 生成</param>
        /// <param name="downloadType">下载模式</param>
        /// <returns></returns>
        public static bool Download(HttpResponseBase response, string filePath, string fileName, DownloadTypeEnum downloadType = DownloadTypeEnum.TransmitFile)
        {
            return Download(response, filePath, fileName, Encoding.UTF8, downloadType);
        }
        /// <summary>
        /// 生成文件流
        /// </summary>
        /// <param name="response">HttpResponseBase</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileName">文件名称，为空，则根据 GUID 生成</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="downloadType">下载模式</param>
        /// <returns></returns>
        public static bool Download(HttpResponseBase response, string filePath, string fileName, Encoding encoding, DownloadTypeEnum downloadType = DownloadTypeEnum.TransmitFile)
        {
            if (downloadType == DownloadTypeEnum.TransmitFile)
            {
                return TransmitFileDownload(response, filePath, fileName, encoding);
            }
            else if (downloadType == DownloadTypeEnum.WriteFile)
            {
                return WriteFileDownload(response, filePath, fileName, encoding);
            }
            else if (downloadType == DownloadTypeEnum.BinaryWrite)
            {
                return BinaryWriteDownload(response, filePath, fileName, encoding);
            }
            else
            {
                return WriteFilePortionDownload(response, filePath, fileName, encoding);
            }
        }
        /// <summary>
        /// 下载文件并保存
        /// </summary>
        /// <param name="url">要下载的 URL</param>
        /// <param name="filePath">要保存的文件路径</param>
        /// <returns></returns>
        public static bool Download(string url, string filePath)
        {
            WebClient webClient = null;
            FileStream fileStream = null;
            try
            {
                bool directoryStatus = FileHelper.CreateDirectory(filePath);
                if (!directoryStatus) throw new Exception(DirectoryCreateFailed);

                if (File.Exists(filePath)) File.Delete(filePath);

                webClient = new WebClient();
                byte[] bytes = webClient.DownloadData(url);
                using (fileStream = new FileStream(filePath, FileMode.Create))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                }
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (fileStream != null) fileStream.Close();
                if (webClient != null) webClient.Dispose();
            }
        }
        #endregion

        #region 逻辑处理私有函数

        #region 输出文件下载流
        private static bool TransmitFileDownload(HttpResponseBase response, string filePath, string fileName, Encoding encoding)
        {
            try
            {
                return ExecuteDownload(response, () =>
                {
                    response.TransmitFile(filePath);
                }, true, filePath, fileName, encoding);
            }
            catch
            {
                if (response != null) response.Close();
                throw;
            }
        }
        private static bool WriteFileDownload(HttpResponseBase response, string filePath, string fileName, Encoding encoding)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length >= MAXSIZE) throw new Exception(SizeOver2GException);

                return ExecuteDownload(response, () =>
                {
                    response.AddHeader("Content-Length", fileInfo.Length.ToString());
                    response.AddHeader("Content-Transfer-Encoding", "binary");

                    response.WriteFile(fileInfo.FullName);
                }, true, filePath, fileName, encoding);
            }
            catch
            {
                if (response != null) response.Close();
                throw;
            }
        }
        private static bool BinaryWriteDownload(HttpResponseBase response, string filePath, string fileName, Encoding encoding)
        {
            FileStream fileStream = null;
            try
            {
                return ExecuteDownload(response, () =>
                {
                    using (fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        if (fileStream.Length >= MAXSIZE) throw new Exception(SizeOver2GException);

                        response.AddHeader("Content-Length", fileStream.Length.ToString());

                        byte[] bytes = new byte[(int)fileStream.Length];
                        fileStream.Read(bytes, 0, bytes.Length);
                        fileStream.Close();

                        response.BinaryWrite(bytes);
                    }
                }, true, filePath, fileName, encoding);
            }
            catch
            {
                if (fileStream != null) fileStream.Close();
                if (response != null) response.Close();
                throw;
            }
        }
        private static bool WriteFilePortionDownload(HttpResponseBase response, string filePath, string fileName, Encoding encoding)
        {
            FileStream fileStream = null;
            try
            {
                return ExecuteDownload(response, () =>
                {
                    byte[] buffer = new byte[CHUNKSIZE];
                    using (fileStream = File.OpenRead(filePath))
                    {
                        response.AddHeader("Content-Length", fileStream.Length.ToString());

                        long fileSize = fileStream.Length;
                        while (fileSize > 0 && response.IsClientConnected)
                        {
                            int readSize = fileStream.Read(buffer, 0, Convert.ToInt32(CHUNKSIZE));
                            response.OutputStream.Write(buffer, 0, readSize);
                            response.Flush();
                            fileSize = fileSize - readSize;
                        }
                    }
                    response.Close();
                }, false, filePath, fileName, encoding);
            }
            catch
            {
                if (fileStream != null) fileStream.Close();
                if (response != null) response.Close();

                throw;
            }
        }
        private static bool ExecuteDownload(HttpResponseBase response, Action callback, bool flush, string filePath, string fileName, Encoding encoding)
        {
            if (response == null || string.IsNullOrEmpty(filePath)) return false;

            response.ContentType = MimeHelper.GetMineType(MimeTypeEnum.ALL, true);
            response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", HttpUtility.UrlEncode(GetRandomName(filePath, fileName))));
            response.ContentEncoding = encoding;

            if (callback != null) callback();
            if (flush)
            {
                response.Flush();
                response.End();
            }
            return true;
        }
        #endregion

        private static string GetRandomName(string filePath, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName)) return fileName;

            string suffix = FileHelper.GetSuffix(filePath);
            string randomName = System.Guid.NewGuid().ToString("N") + suffix;
            return randomName;
        }
        #endregion
    }
}
