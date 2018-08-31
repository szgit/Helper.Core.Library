/*
 * 作用：通过 ZXing.Net 实现二维码/条形码创建/识别，支持生成不带空白边框的二维码。
 * ZXing 下载地址：http://zxingnet.codeplex.com/
 * */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    internal class QrcodeImageFormat
    {
        public const string Png = ".png";
        public const string Jpg = ".jpg";

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
                        Jpg
                    };
                }
                return formatList;
            }
        }
    }
    #endregion

    public class QrCodeHelper
    {
        #region 私有属性常量
        private const string QrcodeWidthOrHeightException = "图片宽/高不能为零！";
        private const string QrcodeImageFormatErrorException = "二维码图片后缀不正确！";
        #endregion

        #region 对外公开方法

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="path">二维码地址</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="logoPath">标志地址，默认为空</param>
        /// <param name="margin">边距</param>
        /// <param name="characterSet">编码格式，默认为 UTF8</param>
        /// <returns></returns>
        public static bool Qrcode(string data, string path, int width, int height, string logoPath = "", int margin = 5, string characterSet = "UTF-8")
        {
            if (width == 0 || height == 0) throw new Exception(QrcodeWidthOrHeightException);

            Bitmap bitmap = null;
            Bitmap reviseBitmap = null;
            Bitmap newBitmap = null;
            Image logoImage = null;
            Graphics graphic = null;

            try
            {
                #region 生成二维码
                string suffix = FileHelper.GetSuffix(path);
                if (QrcodeImageFormat.FormatList.IndexOf(suffix) < 0) throw new Exception(QrcodeImageFormatErrorException);

                ImageFormat imageFormat = ImageFormat.Png;
                if (suffix == QrcodeImageFormat.Jpg) imageFormat = ImageFormat.Jpeg;

                bool directoryResult = FileHelper.CreateDirectory(path);
                if (!directoryResult) return false;

                if (File.Exists(path)) File.Delete(path);
                
                QrCodeEncodingOptions options = new QrCodeEncodingOptions()
                {
                    DisableECI = true, CharacterSet = characterSet, Width = width, Height = height, Margin = margin, ErrorCorrection = ErrorCorrectionLevel.H
                };

                BarcodeWriter barcodeWriter = new BarcodeWriter()
                {
                    Format = BarcodeFormat.QR_CODE, Options = options
                };

                bitmap = barcodeWriter.Write(data);
                #endregion

                #region 修正二维码边框
                reviseBitmap = ReviseBitmap(bitmap);
                bitmap.Dispose();
                
                newBitmap = new Bitmap(width, height);

                graphic = Graphics.FromImage(newBitmap);
                graphic.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);

                graphic.CompositingQuality = CompositingQuality.HighQuality;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                graphic.DrawImage(reviseBitmap, margin, margin, newBitmap.Width - margin * 2, newBitmap.Height - margin * 2);
                #endregion

                #region 添加 LOGO 图标
                if (!string.IsNullOrEmpty(logoPath))
                {
                    logoImage = new Bitmap(logoPath);

                    int offsetX = (int)((newBitmap.Width - logoImage.Width) * 0.5);
                    int offsetY = (int)((newBitmap.Height - logoImage.Height) * 0.5);

                    graphic.DrawImage(logoImage, offsetX, offsetY, logoImage.Width, logoImage.Height);
                }
                #endregion

                newBitmap.Save(path, imageFormat);

                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (graphic != null) graphic.Dispose();
                if (logoImage != null) logoImage.Dispose();
                if (bitmap != null) bitmap.Dispose();
                if (reviseBitmap != null) reviseBitmap.Dispose();
                if (newBitmap != null) newBitmap.Dispose();
            }
        }
        /// <summary>
        /// 生成条形码，128 码
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="path">条形码存储地址</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="margin">边距</param>
        /// <returns></returns>
        public static bool Gecode(string data, string path, int width, int height, int margin = 5)
        {
            if (width == 0 || height == 0) throw new Exception(QrcodeWidthOrHeightException);

            Bitmap bitmap = null;
            Bitmap reviseBitmap = null;
            Bitmap newBitmap = null;
            Graphics graphic = null;

            try
            {
                string suffix = FileHelper.GetSuffix(path);
                if (QrcodeImageFormat.FormatList.IndexOf(suffix) < 0) throw new Exception(QrcodeImageFormatErrorException);

                ImageFormat imageFormat = ImageFormat.Png;
                if (suffix == QrcodeImageFormat.Jpg) imageFormat = ImageFormat.Jpeg;

                bool directoryResult = FileHelper.CreateDirectory(path);
                if (!directoryResult) return false;

                if (File.Exists(path)) File.Delete(path);

                EncodingOptions options = new EncodingOptions()
                {
                    Width = width, Height = height, Margin = margin
                };

                BarcodeWriter barcodeWriter = new BarcodeWriter()
                {
                    Format = BarcodeFormat.CODE_128, Options = options
                };

                bitmap = barcodeWriter.Write(data);

                reviseBitmap = ReviseBitmap(bitmap);
                bitmap.Dispose();

                newBitmap = new Bitmap(width, height);

                graphic = Graphics.FromImage(newBitmap);
                graphic.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);

                graphic.CompositingQuality = CompositingQuality.HighQuality;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                graphic.DrawImage(reviseBitmap, margin, margin, newBitmap.Width - margin * 2, newBitmap.Height - margin * 2);

                newBitmap.Save(path, imageFormat);
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (graphic != null) graphic.Dispose();
                if (bitmap != null) bitmap.Dispose();
                if (reviseBitmap != null) reviseBitmap.Dispose();
                if (newBitmap != null) newBitmap.Dispose();
            }
        }
        /// <summary>
        /// 识别二维码/条形码
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <param name="characterSet">编码格式，默认为 UTF8</param>
        /// <returns></returns>
        public static string Extract(string path, string characterSet = "UTF-8")
        {
            Bitmap bitmap = null;
            try
            {
                BarcodeReader barcodeReader = new BarcodeReader();
                barcodeReader.Options.CharacterSet = characterSet;

                bitmap = new Bitmap(path);
                Result result = barcodeReader.Decode(bitmap);
                bitmap.Dispose();

                return result == null ? "" : result.Text;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (bitmap != null) bitmap.Dispose();
            }
        }

        #endregion

        #region 逻辑处理私有函数
        private static Bitmap ReviseBitmap(Bitmap bitmap)
        {
            Point borderPoint = BorderCheck(bitmap);

            Bitmap newBitmap = new Bitmap(bitmap.Width - borderPoint.X * 2, bitmap.Height - borderPoint.Y * 2);

            int width = newBitmap.Width;
            int height = newBitmap.Height;

            for (int posX = 0; posX < width; posX++)
            {
                for (int posY = 0; posY < height; posY++)
                {
                    newBitmap.SetPixel(posX, posY, bitmap.GetPixel(posX + borderPoint.X, posY + borderPoint.Y));
                }
            }
            return newBitmap;
        }
        private static Point BorderCheck(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            for (int posX = 0; posX < width; posX++)
            {
                for (int posY = 0; posY < height; posY++)
                {
                    if (bitmap.GetPixel(posX, posY).Name == "ff000000")
                    {
                        return new Point(posX, posY);
                    }
                }
            }
            return new Point();
        }
        #endregion
    }
}
