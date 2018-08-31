/*
 * 作用：调用外部/系统 DLL，判断文档是否处于打开状态，读取/写入 Ini 数据。
 * */
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Helper.Core.Library
{
    public class DllImportHelper
    {
        #region 私有属性常量
        [DllImport("kernel32.dll")]
        private static extern IntPtr _lopen(string lpPathName, int iReadWrite);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32.dll")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private const int OF_READWRITE = 2;
        private const int OF_SHARE_DENY_NONE = 0x40;
        private static readonly IntPtr HFILE_ERROR = new IntPtr(-1);
        #endregion

        #region 对外公开方法

        #region 判断文档是否打开
        /// <summary>
        /// 判断文档是否打开
        /// </summary>
        /// <param name="filePath">文档路径</param>
        /// <param name="existStatus">是否检查文件是否存在，true 文件不存在时判断为未打开状态</param>
        /// <returns></returns>
        public static bool IsFileOpen(string filePath, bool existStatus = true)
        {
            if (existStatus) if (!System.IO.File.Exists(filePath)) return false;

            IntPtr vHandle = _lopen(filePath, OF_READWRITE | OF_SHARE_DENY_NONE);
            if (vHandle == HFILE_ERROR)
            {
                CloseHandle(vHandle);
                return true;
            }
            CloseHandle(vHandle);
            return false;
        }
        /// <summary>
        /// 判断文档是否打开
        /// </summary>
        /// <param name="existStatus">是否检查文件是否存在，true 文件不存在时判断为未打开状态</param>
        /// <param name="filePathList">文档路径列表</param>
        /// <returns></returns>
        public static bool IsFileOpen(bool existStatus = true, params string[] filePathList)
        {
            foreach (string filePath in filePathList)
            {
                if (IsFileOpen(filePath, existStatus)) return true;
            }
            return false;
        }
        #endregion

        #region 读取/设置 INI 数据

        /// <summary>
        /// 设置 INI 数据
        /// </summary>
        /// <param name="iniPath">ini 文件路径</param>
        /// <param name="section">节点名，[] 符号内表示节点</param>
        /// <param name="key">节点键名称，= 符号左侧表示键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static bool SetIniData(string iniPath, string section, string key, string value)
        {
            if (!System.IO.File.Exists(iniPath)) System.IO.File.Create(iniPath).Close();

            long tag = WritePrivateProfileString(section, key, value, iniPath);
            if (tag > 0) return true;

            return false;
        }
        /// <summary>
        /// 设置 INI 数据
        /// </summary>
        /// <param name="iniPath">ini 文件路径</param>
        /// <param name="section">节点名，[] 符号内表示节点</param>
        /// <param name="keyValueDict">键值数据</param>
        /// <returns></returns>
        public static bool SetIniDataDict(string iniPath, string section, Dictionary<string, string> keyValueDict)
        {
            if (!System.IO.File.Exists(iniPath)) System.IO.File.Create(iniPath).Close();
            if (keyValueDict != null && keyValueDict.Count > 0)
            {
                bool status = false;
                foreach(KeyValuePair<string, string> keyValueItem in keyValueDict)
                {
                    status = SetIniData(iniPath, section, keyValueItem.Key, keyValueItem.Value);
                    if (!status) return false;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取 INI 数据
        /// </summary>
        /// <param name="iniPath">ini 文件路径</param>
        /// <param name="section">节点名，[] 符号内表示节点</param>
        /// <param name="key">节点键名称，= 符号左侧表示键</param>
        /// <param name="size">读取字节长度</param>
        /// <returns></returns>
        public static string GetIniData(string iniPath, string section, string key, int size = 1024)
        {
            if (!System.IO.File.Exists(iniPath)) return null;

            StringBuilder stringBuilder = new StringBuilder(size);
            GetPrivateProfileString(section, key, "", stringBuilder, size, iniPath);

            return stringBuilder.ToString();
        }
        /// <summary>
        /// 获取 INI 数据
        /// </summary>
        /// <param name="iniPath">ini 文件路径</param>
        /// <param name="section">节点名，[] 符号内表示节点</param>
        /// <param name="keyList">节点键列表</param>
        /// <param name="size">读取字节长度</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetIniDataDict(string iniPath, string section, string[] keyList, int size = 1024)
        {
            if (keyList == null && keyList.Length == 0) return null;

            Dictionary<string, string> resultDict = new Dictionary<string, string>();
            foreach(string key in keyList)
            {
                resultDict.Add(key, GetIniData(iniPath, section, key, size));
            }
            return resultDict;
        }

        #endregion

        #endregion
    }
}
