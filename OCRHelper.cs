/*
 * 作用：通过 Tesseract 实现图片文字识别。
 * Tesseract 下载地址：https://www.nuget.org/packages/Tesseract/
 * 语言包地址：https://github.com/tesseract-ocr/langdata
 * */
using System.Drawing;
using Tesseract;

namespace Helper.Core.Library
{
    public class OCRHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 图片文字识别
        /// </summary>
        /// <param name="imageUrl">图片路径</param>
        /// <param name="tessdataUrl">语言包路径</param>
        /// <param name="language">语言包名称</param>
        /// <param name="variableData">设置识别变量，默认识别数字</param>
        /// <param name="pageSegMode">PageSegMode</param>
        /// <returns></returns>
        public static string Identity(string imageUrl, string tessdataUrl, string language = "eng", string variableData = "0123456789", PageSegMode pageSegMode = PageSegMode.Auto)
        {
            Bitmap bitmap = null;
            TesseractEngine tesseractEngine = null;
            Page page = null;
            try
            {
                bitmap = new Bitmap(imageUrl);

                tesseractEngine = new TesseractEngine(tessdataUrl, language, EngineMode.Default);
                if (!string.IsNullOrEmpty(variableData)) tesseractEngine.SetVariable("tessedit_char_whitelist", variableData);
                page = tesseractEngine.Process(PixConverter.ToPix(bitmap), pageSegMode);
                return page.GetText();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (bitmap != null) bitmap.Dispose();
                if (page != null) page.Dispose();
                if (tesseractEngine != null) tesseractEngine.Dispose();
            }
        }
        #endregion
    }
}
