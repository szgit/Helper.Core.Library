/*
 * 作用：上传图片处理。
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web;

namespace Helper.Core.Library
{
    public class UploadImageHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="httpPostedFile">HttpPostedFileBase 数据</param>
        /// <param name="path">保存路径</param>
        /// <param name="fileMaxSize">上传文件大小</param>
        /// <param name="suffixList">合法后缀列表，例：.rar</param>
        /// <param name="serialEnumList">VerifyFormatSerialEnum</param>
        /// <returns></returns>
        public bool Upload(HttpPostedFileBase httpPostedFile, string path, int fileMaxSize, List<string> suffixList = null, params VerifyFormatSerialEnum[] serialEnumList)
        {
            if (this.Verify(httpPostedFile, fileMaxSize, suffixList, serialEnumList))
            {
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                    httpPostedFile.SaveAs(path);
                    return true;
                }
                catch
                {
                    throw;
                }
            }
            return false;
        }
        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="httpPostedFile">HttpPostedFileBase 数据</param>
        /// <param name="path">保存路径</param>
        /// <param name="width">图片宽，当值为 0 时，宽按高的缩放比例生成</param>
        /// <param name="height">图片高，当值为 0 时，高按宽的缩放比例生成</param>
        /// <param name="fileMaxSize">上传文件大小</param>
        /// <param name="logoPath">标志水印地址</param>
        /// <param name="left">标志左间距，值小于 0 表示从右开始</param>
        /// <param name="top">标志上间距，值小于 0 表示从底开始</param>
        /// <param name="suffixList">合法后缀列表，例：.rar</param>
        /// <param name="serialEnumList">VerifyFormatSerialEnum</param>
        /// <returns></returns>
        public bool Upload(HttpPostedFileBase httpPostedFile, string path, int width, int height, int fileMaxSize, string logoPath = "", int left = 0, int top = 0, List<string> suffixList = null, params VerifyFormatSerialEnum[] serialEnumList)
        {
            if (this.Verify(httpPostedFile, fileMaxSize, suffixList, serialEnumList))
            {
                Bitmap bitmap = new Bitmap(httpPostedFile.InputStream, true);
                bitmap = this.CreateBitmap(bitmap, width, height, logoPath, left, top);

                if (bitmap != null)
                {
                    if (File.Exists(path)) File.Delete(path);
                    bitmap.Save(path);
                    bitmap.Dispose();

                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 逻辑处理私有方法
        private Bitmap CreateBitmap(Bitmap originalBitmap, int width, int height, string logoPath = "", int left = 0, int top = 0)
        {
            Graphics graphics = null;
            try
            {
                int originalWidth = originalBitmap.Width;
                int originalHeight = originalBitmap.Height;

                Bitmap targetBitmap = null;

                if ((width == 0 && height == 0) || (originalWidth <= width && originalHeight <= height))
                {
                    // 绘制最终图像
                    targetBitmap = new Bitmap(originalWidth, originalHeight);
                    using (graphics = Graphics.FromImage(targetBitmap))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(originalBitmap, 0, 0, originalWidth, originalHeight);
                    }
                }
                else
                {

                    int newWidth = width;
                    int newHeight = height;

                    if (width == 0)
                    {
                        newWidth = (int)Math.Round((decimal)originalBitmap.Width * height / originalBitmap.Height);
                    }
                    else if (height == 0)
                    {
                        newHeight = (int)Math.Round((decimal)originalBitmap.Height * width / originalBitmap.Width);
                    }
                    else
                    {
                        if (width * originalBitmap.Height < height * originalBitmap.Width)
                        {
                            newHeight = (int)Math.Round((decimal)originalBitmap.Height * width / originalBitmap.Width);
                        }
                        else
                        {
                            newWidth = (int)Math.Round((decimal)originalBitmap.Width * height / originalBitmap.Height);
                        }
                    }

                    // 按比例生成缩略图
                    Bitmap newBitmap = new Bitmap(newWidth, newHeight);
                    using (graphics = Graphics.FromImage(newBitmap))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                        graphics.DrawImage(originalBitmap, 0, 0, newWidth, newHeight);
                    }

                    if (width == 0) width = newWidth;
                    if (height == 0) height = newHeight;

                    // 绘制最终图像
                    targetBitmap = new Bitmap(width, height);
                    using (graphics = Graphics.FromImage(targetBitmap))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.Clear(Color.White);
                        graphics.DrawImage(newBitmap, 0, 0);
                    }
                }
                if(!string.IsNullOrEmpty(logoPath))
                {
                    Bitmap logoBitmap = new Bitmap(Image.FromFile(logoPath));

                    int posX = 0;
                    int posY = 0;

                    if (left >= 0)
                    {
                        posX = left;
                    }
                    else
                    {
                        posX = targetBitmap.Width - logoBitmap.Width + left;
                    }
                    if (top >= 0)
                    {
                        posY = top;
                    }
                    else
                    {
                        posY = targetBitmap.Height - logoBitmap.Height + top;
                    }

                    using (graphics = Graphics.FromImage(targetBitmap))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(logoBitmap, posX, posY);
                    }
                }
                return targetBitmap;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (graphics != null) graphics.Dispose();
            }
        }
        private bool Verify(HttpPostedFileBase httpPostedFile, int maxSize, List<string> suffixList, params VerifyFormatSerialEnum[] serialEnumList)
        {
            if (httpPostedFile.ContentLength > maxSize)
            {
                return false;
            }

            if (suffixList == null)
            {
                return UploadVerifyHelper.Verify(httpPostedFile);
            }
            else
            {
                return UploadVerifyHelper.Verify(httpPostedFile, suffixList, serialEnumList);
            }
        }
        #endregion
    }
}
