/*
 * 作用：验证上传文件格式。
 * */
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Helper.Core.Library
{
    #region 逻辑辅助枚举类
    internal class VerifyFormat
    {
        public const string Jpg = ".jpg";
        public const string Png = ".png";
        public const string Gif = ".gif";
        public const string Bmp = ".bmp";

        private static List<string> formatList;
        public static List<string> FormatList
        {
            get
            {
                if (formatList == null || formatList.Count == 0)
                {
                    formatList = new List<string>()
                    {
                        Png,
                        Jpg,
                        Gif,
                        Bmp
                    };
                }
                return formatList;
            }
        }
    }
    #endregion

    public class UploadVerifyHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 验证上传文件是否合法
        /// </summary>
        /// <param name="httpPostedFile">HttpPostedFileBase</param>
        /// <param name="suffixList">合法后缀列表，例：.rar</param>
        /// <param name="serialEnumList">VerifyFormatSerialEnum</param>
        /// <returns></returns>
        public static bool Verify(HttpPostedFileBase httpPostedFile, List<string> suffixList, params VerifyFormatSerialEnum[] serialEnumList)
        {
            MemoryStream memoryStream = null;
            BinaryReader binaryReader = null;
            try
            {
                Stream stream = httpPostedFile.InputStream;

                string suffix = FileHelper.GetSuffix(httpPostedFile.FileName);
                if (suffixList.IndexOf(suffix) < 0) return false;

                Byte[] bytesContent = new Byte[2];
                stream.Read(bytesContent, 0, 2);
                stream.Seek(0, SeekOrigin.Begin);

                memoryStream = new MemoryStream(bytesContent);
                binaryReader = new BinaryReader(memoryStream);

                string bufferText = string.Empty;
                byte buffer = byte.MinValue;

                buffer = binaryReader.ReadByte();
                bufferText = buffer.ToString();
                buffer = binaryReader.ReadByte();
                bufferText += buffer.ToString();

                foreach (VerifyFormatSerialEnum formatSerialEnum in serialEnumList)
                {
                    if (int.Parse(bufferText) == (int)formatSerialEnum) return true;
                }
                return false;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (memoryStream != null) memoryStream.Dispose();
                if (binaryReader != null) binaryReader.Dispose();
            }
        }
        /// <summary>
        /// 验证上传图片类型是否合法
        /// </summary>
        /// <param name="httpPostedFile">HttpPostedFileBase</param>
        /// <returns></returns>
        public static bool Verify(HttpPostedFileBase httpPostedFile)
        {
            return Verify(httpPostedFile, VerifyFormat.FormatList, VerifyFormatSerialEnum.Jpg, VerifyFormatSerialEnum.Gif, VerifyFormatSerialEnum.Png, VerifyFormatSerialEnum.Bmp);
        }
        #endregion
    }

    #region 逻辑处理辅助类
    public enum VerifyFormatSerialEnum
    {
        None = 0,
        Jpg = 255216,
        Gif = 7173,
        Bmp = 6677,
        Png = 13780,
        Swf = 6787,
        Rar = 8297,
        Zip = 8075,
        Xml = 6063,
        Doc = 208207,
        Docx = 8075,
        Aspx = 239187,
        Cs = 117115,
        Sql = 255254,
        Html = 6063,
        Exe = 7790
    }
    #endregion
}
