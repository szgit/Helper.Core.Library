/*
 * 作用：服务器端文件上传。
 * */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Helper.Core.Library
{
    public class UploadHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="httpUrl">上传 URL</param>
        /// <param name="httpPostedFile">转发 HttpPostedFile</param>
        /// <param name="httpPostedFileName">HttpPostedFile 名称</param>
        /// <param name="parameterList">参数列表</param>
        /// <param name="cookieContainer">CookieContainer</param>
        /// <param name="timeout">超时</param>
        /// <returns></returns>
        public static string Upload(string httpUrl, System.Web.HttpPostedFileBase httpPostedFile, string httpPostedFileName = "file", Dictionary<string, object> parameterList = null, CookieContainer cookieContainer = null, int timeout = 20000)
        {
            HttpWebResponse httpWebResponse = null;
            StreamReader streamReader = null;
            Stream stream = null;
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(httpUrl);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = timeout;
                httpWebRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;
                httpWebRequest.KeepAlive = true;

                // 创建分界线
                string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                // 创建内容类型
                httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                // 创建表单数据模板
                string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

                // 读取上传文件数据流
                byte[] buffer = new byte[httpPostedFile.ContentLength];
                httpPostedFile.InputStream.Read(buffer, 0, buffer.Length);

                // 写入请求流数据
                string headerContent = "Content-Disposition:application/x-www-form-urlencoded; name=\"{0}\";filename=\"{1}\"\r\nContent-Type:{2}\r\n\r\n";
                headerContent = string.Format(headerContent, httpPostedFileName, httpPostedFile.FileName, httpPostedFile.ContentType);

                // 创建 HTTP 请求头
                byte[] byteHeader = System.Text.ASCIIEncoding.ASCII.GetBytes(headerContent);
                // 获得响应数据流
                using (stream = httpWebRequest.GetRequestStream())
                {
                    // 写入请求流
                    if (parameterList != null && parameterList.Count > 0)
                    {
                        foreach (KeyValuePair<string, object> keyValuePair in parameterList)
                        {
                            stream.Write(boundaryBytes, 0, boundaryBytes.Length);//写入分界线
                            byte[] formBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(formdataTemplate, keyValuePair.Key, keyValuePair.Value));
                            stream.Write(formBytes, 0, formBytes.Length);
                        }
                    }
                    // 写入分界线，此步骤不可省略
                    stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                    // 写入请求头
                    stream.Write(byteHeader, 0, byteHeader.Length);
                    // 把文件流写入请求流
                    stream.Write(buffer, 0, buffer.Length);
                    // 写入分界线流
                    byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    stream.Write(trailer, 0, trailer.Length);
                    stream.Flush();
                }

                string responseText = "";
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    responseText = streamReader.ReadToEnd();
                }
                httpWebResponse.Close();
                return responseText;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (stream != null) stream.Close();
                if (streamReader != null) streamReader.Close();
                if (httpWebResponse != null) httpWebResponse.Close();
            }
        }
        #endregion
    }
}
