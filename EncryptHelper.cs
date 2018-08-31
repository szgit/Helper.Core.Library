/*
 * 作用：字符串加密/解密，MD5，AES，DES，BASE64。
 * */
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Helper.Core.Library
{
    public class EncryptHelper
    {
        #region 对外公开方法

        #region MD5
        /// <summary>
        /// MD5 加密
        /// </summary>
        /// <param name="data">需要加密的数据</param>
        /// <returns></returns>
        public static string MD5(string data)
        {
            byte[] buffer = System.Text.Encoding.Default.GetBytes(data);
            MD5CryptoServiceProvider cryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] hashValue = cryptoServiceProvider.ComputeHash(buffer);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte byteData in hashValue)
            {
                if (byteData < 16)
                {
                    stringBuilder.Append("0");
                }
                stringBuilder.Append(byteData.ToString("X"));
            }
            return stringBuilder.ToString().ToLower();
        }
        /// <summary>
        /// MD5 加密
        /// </summary>
        /// <param name="data">需要加密的数据</param>
        /// <param name="confuseData">混淆字符</param>
        /// <param name="encryptTimes">加密次数</param>
        /// <returns></returns>
        public static string MD5(string data, string confuseData, int encryptTimes = 3)
        {
            string encryptData = data;
            string encryptConfuseData = confuseData;
            do
            {
                encryptTimes--;
                encryptData = MD5(encryptData);
                encryptConfuseData = MD5(encryptConfuseData);
            } while (encryptTimes > 0);

            encryptData = encryptData.Insert(0, encryptConfuseData.Substring(24));
            encryptData = encryptData.Insert(32, encryptConfuseData.Substring(0, 8));
            encryptData = encryptData.Insert(16, encryptConfuseData.Substring(8, 8));
            encryptData = encryptData.Insert(32, encryptConfuseData.Substring(16, 8));
            return encryptData;
        }
        #endregion

        #region AES 加密/解密
        /// <summary>
        /// AES 加密
        /// </summary>
        /// <param name="data">需要加密的数据</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string AESEncrypt(string data, string key)
        {
            MemoryStream memoryStream = null;
            CryptoStream cryptoStream = null;
            RijndaelManaged rijndaelManaged = null;
            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(data);

                Byte[] byteKeys = new Byte[32];
                Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(byteKeys.Length)), byteKeys, byteKeys.Length);

                string result = null;

                using (memoryStream = new MemoryStream())
                {
                    rijndaelManaged = new RijndaelManaged();
                    rijndaelManaged.Mode = CipherMode.ECB;
                    rijndaelManaged.Padding = PaddingMode.PKCS7;
                    rijndaelManaged.KeySize = 128;
                    rijndaelManaged.Key = byteKeys;

                    using (cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                        cryptoStream.FlushFinalBlock();
                    }
                    result = Convert.ToBase64String(memoryStream.ToArray());
                }
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cryptoStream != null) cryptoStream.Close();
                if (memoryStream != null) memoryStream.Close();
                if (rijndaelManaged != null) rijndaelManaged.Clear();
            }
        }
        /// <summary>
        /// AES 加密
        /// </summary>
        /// <param name="data">需要加密的数据</param>
        /// <param name="key">密钥</param>
        /// <param name="vector">向量</param>
        /// <returns></returns>
        public static string AESEncrypt(string data, string key, string vector)
        {
            MemoryStream memoryStream = null;
            CryptoStream cryptoStream = null;
            Rijndael rijndael = null;
            try
            {
                Byte[] plainBytes = Encoding.UTF8.GetBytes(data);

                Byte[] byteKeys = new Byte[32];
                Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(byteKeys.Length)), byteKeys, byteKeys.Length);

                Byte[] byteVectors = new Byte[16];
                Array.Copy(Encoding.UTF8.GetBytes(vector.PadRight(byteVectors.Length)), byteVectors, byteVectors.Length);

                Byte[] cryptograph = null;

                rijndael = Rijndael.Create();
                using (memoryStream = new MemoryStream())
                {
                    using (cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(byteKeys, byteVectors), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                        cryptoStream.FlushFinalBlock();

                        cryptograph = memoryStream.ToArray();
                    }
                }
                return Convert.ToBase64String(cryptograph);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cryptoStream != null) cryptoStream.Close();
                if (memoryStream != null) memoryStream.Close();
                if (rijndael != null) rijndael.Clear();
            }
        }
        /// <summary>
        /// AES 解密
        /// </summary>
        /// <param name="data">需要解密的数据</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string AESDecrypt(string data, string key)
        {
            MemoryStream memoryStream = null;
            CryptoStream cryptoStream = null;
            RijndaelManaged rijndaelManaged = null;
            try
            {
                Byte[] encryptedBytes = Convert.FromBase64String(data);

                Byte[] byteKeys = new Byte[32];
                Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(byteKeys.Length)), byteKeys, byteKeys.Length);

                string result = null;
                using (memoryStream = new MemoryStream(encryptedBytes))
                {
                    rijndaelManaged = new RijndaelManaged();
                    rijndaelManaged.Mode = CipherMode.ECB;
                    rijndaelManaged.Padding = PaddingMode.PKCS7;
                    rijndaelManaged.KeySize = 128;
                    rijndaelManaged.Key = byteKeys;

                    using (cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        byte[] tmp = new byte[encryptedBytes.Length + 32];
                        int len = cryptoStream.Read(tmp, 0, encryptedBytes.Length + 32);
                        byte[] ret = new byte[len];
                        Array.Copy(tmp, 0, ret, 0, len);
                        result = Encoding.UTF8.GetString(ret);
                    }
                }
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cryptoStream != null) cryptoStream.Close();
                if (memoryStream != null) memoryStream.Close();
                if (rijndaelManaged != null) rijndaelManaged.Clear();
            }
        }
        /// <summary>
        /// AES 解密
        /// </summary>
        /// <param name="data">需要解密的数据</param>
        /// <param name="key">密钥</param>
        /// <param name="vector">向量</param>
        /// <returns></returns>
        public static string AESDecrypt(string data, string key, string vector)
        {
            MemoryStream memoryStream = null;
            MemoryStream originalMemory = null;
            CryptoStream cryptoStream = null;
            Rijndael rijndael = null;
            try
            {
                Byte[] encryptedBytes = Convert.FromBase64String(data);

                Byte[] byteKeys = new Byte[32];
                Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(byteKeys.Length)), byteKeys, byteKeys.Length);
                Byte[] byteVectors = new Byte[16];
                Array.Copy(Encoding.UTF8.GetBytes(vector.PadRight(byteVectors.Length)), byteVectors, byteVectors.Length);

                Byte[] original = null;

                rijndael = Rijndael.Create();
                using (memoryStream = new MemoryStream(encryptedBytes))
                {
                    using (cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(byteKeys, byteVectors), CryptoStreamMode.Read))
                    {
                        using (originalMemory = new MemoryStream())
                        {
                            Byte[] Buffer = new Byte[1024];
                            Int32 readBytes = 0;
                            while ((readBytes = cryptoStream.Read(Buffer, 0, Buffer.Length)) > 0)
                            {
                                originalMemory.Write(Buffer, 0, readBytes);
                            }
                            original = originalMemory.ToArray();
                        }
                    }
                }
                return Encoding.UTF8.GetString(original);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (originalMemory != null) originalMemory.Close();
                if (cryptoStream != null) cryptoStream.Close();
                if (memoryStream != null) memoryStream.Close();
                if (rijndael != null) rijndael.Clear();
            }
        }
        #endregion

        #region DES 加密/解密
        /// <summary>
        /// DES 加密
        /// </summary>
        /// <param name="data">需要加密的数据</param>
        /// <param name="key">密钥（必须 8 位）</param>
        /// <param name="vector">向量（必须 8 位）</param>
        /// <returns></returns>
        public static string DESEncrypt(string data, string key, string vector = null)
        {
            DESCryptoServiceProvider desCryptoService = null;
            MemoryStream memoryStream = null;
            CryptoStream cryptoStream = null;
            try
            {
                string result = null;

                using(desCryptoService = new DESCryptoServiceProvider())
                {
                    byte[] byteDataList = Encoding.UTF8.GetBytes(data);
                    desCryptoService.Key = ASCIIEncoding.ASCII.GetBytes(key);

                    if (string.IsNullOrEmpty(vector)) vector = key;
                    desCryptoService.IV = ASCIIEncoding.ASCII.GetBytes(vector);

                    using(memoryStream = new MemoryStream())
                    {
                        using(cryptoStream = new CryptoStream(memoryStream, desCryptoService.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(byteDataList, 0, byteDataList.Length);
                            cryptoStream.FlushFinalBlock();
                        }
                        result = Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cryptoStream != null) cryptoStream.Close();
                if (memoryStream != null) memoryStream.Close();
                if (desCryptoService != null) desCryptoService.Clear();
            }
        }
        /// <summary>
        /// DES 解密
        /// </summary>
        /// <param name="data">需要解密的数据</param>
        /// <param name="key">密钥（必须 8 位）</param>
        /// <param name="vector">向量（必须 8 位）</param>
        /// <returns></returns>
        public static string DESDecrypt(string data, string key, string vector = null)
        {
            DESCryptoServiceProvider desCryptoService = null;
            MemoryStream memoryStream = null;
            CryptoStream cryptoStream = null;
            try
            {
                string result = "";
                byte[] byteDataList = Convert.FromBase64String(data);
                using (desCryptoService = new DESCryptoServiceProvider())
                {
                    desCryptoService.Key = ASCIIEncoding.ASCII.GetBytes(key);
                    
                    if (string.IsNullOrEmpty(vector)) vector = key;
                    desCryptoService.IV = ASCIIEncoding.ASCII.GetBytes(vector);
                    
                    using (memoryStream = new MemoryStream())
                    {
                        using (cryptoStream = new CryptoStream(memoryStream, desCryptoService.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(byteDataList, 0, byteDataList.Length);
                            cryptoStream.FlushFinalBlock();
                        }
                        result = Encoding.UTF8.GetString(memoryStream.ToArray());
                    }
                }
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cryptoStream != null) cryptoStream.Close();
                if (memoryStream != null) memoryStream.Close();
                if (desCryptoService != null) desCryptoService.Clear();
            }
        }
        #endregion

        #region Base64 加密/解密
        /// <summary>
        /// Base64 加密
        /// </summary>
        /// <param name="data">要加密的数据</param>
        /// <returns></returns>
        public static string Base64Encrypt(string data)
        {
            byte[] byteDataList = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(byteDataList, 0, byteDataList.Length);
        }
        /// <summary>
        /// Base64 解密
        /// </summary>
        /// <param name="data">要解密的数据</param>
        /// <returns></returns>
        public static string Base64Decrypt(string data)
        {
            byte[] byteDataList = Convert.FromBase64String(data);
            return Encoding.UTF8.GetString(byteDataList);
        }
        #endregion

        #endregion
    }
}
