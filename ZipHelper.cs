/*
 * 作用：通过 SharpZipLib 实现文件/目录压缩/解压缩。
 * */
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    internal class ZipFormat
    {
        public const string Zip = ".zip";
        public const string Rar = ".rar";

        private static List<string> formatList;
        public static List<string> FormatList
        {
            get
            {
                if (formatList == null || formatList.Count == 0)
                {
                    formatList = new List<string>()
                    {
                        Zip,
                        Rar
                    };
                }
                return formatList;
            }
        }
    }
    #endregion

    public class ZipHelper
    {
        #region 私有属性常量
        private const string DirectoryNotExistsException = "目录不存在";
        private const string FileNotExistsException = "文件不存在";
        private const string ZipFormatErrorException = "文件后缀不正确";
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="directoryPath">压缩目录</param>
        /// <param name="zipPath">Zip 文件路径</param>
        /// <param name="append">是否附加</param>
        /// <param name="password">密码</param>
        /// <param name="compressLevel">压缩等级，取值范围：0-9</param>
        /// <returns></returns>
        public static bool Compress(string directoryPath, string zipPath, bool append, string password = "", int compressLevel = 6)
        {
            // 如果目录不存在
            if (!Directory.Exists(directoryPath)) throw new Exception(DirectoryNotExistsException);

            directoryPath = directoryPath.Replace("/", "\\");
            if (directoryPath.EndsWith("\\")) directoryPath = directoryPath.TrimEnd(new char[] { '\\' });

            // 检查后缀
            string suffix = FileHelper.GetSuffix(zipPath);
            // 如果生成文件后缀不是 .zip|.rar，抛出异常
            if (ZipFormat.FormatList.IndexOf(suffix) < 0) throw new Exception(ZipFormatErrorException);

            // 创建文件目录
            bool directoryResult = FileHelper.CreateDirectory(zipPath);
            if (!directoryResult) return false;

            if (!append)
            {
                if (File.Exists(zipPath)) File.Delete(zipPath);
            }

            bool result = false;
            using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipPath)))
            {
                zipStream.SetLevel(compressLevel);
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                result = CompressDirectory(directoryPath, zipStream);
                zipStream.Finish();
            }

            return result;
        }
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="filePath">文件地址</param>
        /// <param name="zipPath">生成 Zip 路径</param>
        /// <param name="password">密码</param>
        /// <param name="compressLevel">压缩等级，取值范围：0-9</param>
        /// <returns></returns>
        public static bool Compress(string filePath, string zipPath, string password = "", int compressLevel = 6)
        {
            if (!File.Exists(filePath)) throw new Exception(FileNotExistsException);

            ZipOutputStream zipStream = null;
            FileStream fileStream = null;
            ZipEntry zipEntry = null;
            try
            {
                // 检查后缀
                string suffix = FileHelper.GetSuffix(zipPath);
                // 如果生成文件后缀不是 .zip|.rar，抛出异常
                if (ZipFormat.FormatList.IndexOf(suffix) < 0) throw new Exception(ZipFormatErrorException);

                // 创建文件目录
                bool directoryResult = FileHelper.CreateDirectory(zipPath);
                if (!directoryResult) return false;

                fileStream = File.OpenRead(filePath);

                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                fileStream.Close();

                using (fileStream = File.Create(zipPath))
                {
                    using (zipStream = new ZipOutputStream(fileStream))
                    {
                        if (!string.IsNullOrEmpty(password)) zipStream.Password = password;

                        zipEntry = new ZipEntry(Path.GetFileName(filePath));
                        zipStream.PutNextEntry(zipEntry);
                        zipStream.SetLevel(compressLevel);

                        zipStream.Write(buffer, 0, buffer.Length);
                    }
                }
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (zipStream != null) zipStream.Close();
                if (fileStream != null) fileStream.Close();
                if (zipEntry != null) zipEntry = null;
            }
        }
        /// <summary>
        /// 解压缩文件
        /// </summary>
        /// <param name="zipPath">Zip 路径</param>
        /// <param name="directoryPath">输出目录</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public static bool UnCompress(string zipPath, string directoryPath, string password = "")
        {
            FileStream fileStream = null;
            ZipInputStream zipStream = null;
            ZipEntry zipEntry = null;
            try
            {
                string suffix = FileHelper.GetSuffix(zipPath);
                // 如果生成文件后缀不是 .zip|.rar，抛出异常
                if (ZipFormat.FormatList.IndexOf(suffix) < 0) throw new Exception(ZipFormatErrorException);

                // 如果文件不存在
                if (!File.Exists(zipPath)) throw new Exception(FileNotExistsException);

                // 创建输出目录
                bool directoryResult = FileHelper.CreateDirectory(directoryPath, true);
                if (!directoryResult) return false;

                zipStream = new ZipInputStream(File.OpenRead(zipPath));
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;

                string filePath = "";
                while ((zipEntry = zipStream.GetNextEntry()) != null)
                {
                    if (!string.IsNullOrEmpty(zipEntry.Name))
                    {
                        filePath = Path.Combine(directoryPath, zipEntry.Name);
                        filePath = filePath.Replace('/', '\\');

                        if (filePath.EndsWith("\\"))
                        {
                            Directory.CreateDirectory(filePath);
                            continue;
                        }

                        if (File.Exists(filePath)) File.Delete(filePath);

                        using (fileStream = File.Create(filePath))
                        {
                            int size = 1024;
                            byte[] data = new byte[size];
                            while (true)
                            {
                                size = zipStream.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    fileStream.Write(data, 0, data.Length);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            fileStream.Flush();
                        }
                    }
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
                if (zipStream != null) zipStream.Close();
                if (zipEntry != null) zipEntry = null;
            }
        }
        #endregion

        #region 逻辑处理私有方法
        /// <summary>
        /// 遍历压缩文件夹文件
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="zipStream"></param>
        /// <param name="parentDirectoryPath"></param>
        /// <returns></returns>
        private static bool CompressDirectory(string directoryPath, ZipOutputStream zipStream, string parentDirectoryPath = "")
        {
            ZipEntry zipEntry = null;
            FileStream fileStream = null;
            string entryDirectoryPath = "";

            bool result = false;

            try
            {
                entryDirectoryPath = Path.Combine(parentDirectoryPath, Path.GetFileName(directoryPath) + "\\");

                zipEntry = new ZipEntry(entryDirectoryPath);
                zipStream.PutNextEntry(zipEntry);
                zipStream.Flush();

                string[] filePathList = Directory.GetFiles(directoryPath);
                foreach (string filePath in filePathList)
                {
                    fileStream = File.OpenRead(filePath);

                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);

                    zipEntry = new ZipEntry(Path.Combine(entryDirectoryPath + Path.GetFileName(filePath)));
                    zipEntry.DateTime = DateTime.Now;
                    zipEntry.Size = fileStream.Length;

                    fileStream.Close();

                    zipStream.PutNextEntry(zipEntry);
                    zipStream.Write(buffer, 0, buffer.Length);
                }

                result = true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (zipEntry != null) zipEntry = null;
                if (fileStream != null) fileStream.Close();
            }
            
            string[] childDirectoryPathList = Directory.GetDirectories(directoryPath);
            foreach (string childDirectoryPath in childDirectoryPathList)
            {
                if (!CompressDirectory(childDirectoryPath, zipStream, entryDirectoryPath)) return false;
            }
            return result;
        }
        #endregion
    }
}
