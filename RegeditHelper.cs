/*
 * 作用：设置/获取注册表数据。
 * */
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    public enum RegeditTypeEnum
    {
        HKEY_CLASSES_ROOT = 0,
        HKEY_CURRENT_USER = 1,
        HKEY_LOCAL_MACHINE = 2,
        HKEY_USERS = 3,
        HKEY_CURRENT_CONFIG = 4
    }
    #endregion

    public class RegeditHelper
    {
        #region 私有属性常量
        private const string REGEDIT_KEY_RUN = @"Software\Microsoft\Windows\CurrentVersion\Run";
        #endregion

        #region 对外公开方法

        #region SetValue
        /// <summary>
        /// 设置注册表值
        /// </summary>
        /// <param name="regeditType">注册表根节点</param>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="regeditValue">数据</param>
        /// <param name="valueKind">RegistryValueKind 枚举</param>
        /// <returns></returns>
        public static bool SetValue(RegeditTypeEnum regeditType, string regeditKey, string regeditName, object regeditValue, RegistryValueKind valueKind = RegistryValueKind.None)
        {
            RegistryKey registryRoot = null;
            RegistryKey registryItem = null;
            try
            {
                registryRoot = GetRegistryKey(regeditType);
                registryItem = registryRoot.OpenSubKey(regeditKey, true);

                if (registryItem == null)
                {
                    registryItem = registryRoot.CreateSubKey(regeditKey);
                }
                registryItem.SetValue(regeditName, regeditValue, valueKind);
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (registryItem != null) registryItem.Close();
                if (registryRoot != null) registryRoot.Close();
            }
        }
        /// <summary>
        /// 设置注册表值(HKEY_CLASSES_ROOT)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="regeditValue">数据</param>
        /// <param name="valueKind">RegistryValueKind 枚举</param>
        /// <returns></returns>
        public static bool SetValueClassesRoot(string regeditKey, string regeditName, object regeditValue, RegistryValueKind valueKind = RegistryValueKind.None)
        {
            return SetValue(RegeditTypeEnum.HKEY_CLASSES_ROOT, regeditKey, regeditName, regeditValue, valueKind);
        }
        /// <summary>
        /// 设置注册表值(HKEY_CURRENT_USER)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="regeditValue">数据</param>
        /// <param name="valueKind">RegistryValueKind 枚举</param>
        /// <returns></returns>
        public static bool SetValueCurrentUser(string regeditKey, string regeditName, object regeditValue, RegistryValueKind valueKind = RegistryValueKind.None)
        {
            return SetValue(RegeditTypeEnum.HKEY_CURRENT_USER, regeditKey, regeditName, regeditValue, valueKind);
        }
        /// <summary>
        /// 设置注册表值(HKEY_LOCAL_MACHINE)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="regeditValue">数据</param>
        /// <param name="valueKind">RegistryValueKind 枚举</param>
        /// <returns></returns>
        public static bool SetValueLocalMachine(string regeditKey, string regeditName, object regeditValue, RegistryValueKind valueKind = RegistryValueKind.None)
        {
            return SetValue(RegeditTypeEnum.HKEY_LOCAL_MACHINE, regeditKey, regeditName, regeditValue, valueKind);
        }
        /// <summary>
        /// 设置注册表值(HKEY_USERS)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="regeditValue">数据</param>
        /// <param name="valueKind">RegistryValueKind 枚举</param>
        /// <returns></returns>
        public static bool SetValueUsers(string regeditKey, string regeditName, object regeditValue, RegistryValueKind valueKind = RegistryValueKind.None)
        {
            return SetValue(RegeditTypeEnum.HKEY_USERS, regeditKey, regeditName, regeditValue, valueKind);
        }
        /// <summary>
        /// 设置注册表值(HKEY_CURRENT_CONFIG)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="regeditValue">数据</param>
        /// <param name="valueKind">RegistryValueKind 枚举</param>
        /// <returns></returns>
        public static bool SetValueCurrentConfig(string regeditKey, string regeditName, object regeditValue, RegistryValueKind valueKind = RegistryValueKind.None)
        {
            return SetValue(RegeditTypeEnum.HKEY_CURRENT_CONFIG, regeditKey, regeditName, regeditValue, valueKind);
        }
        #endregion

        #region GetValue
        /// <summary>
        /// 获取注册表值
        /// </summary>
        /// <param name="regeditType">注册表根节点</param>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static object GetValue(RegeditTypeEnum regeditType, string regeditKey, string regeditName)
        {
            RegistryKey registryRoot = null;
            RegistryKey registryItem = null;
            try
            {
                registryRoot = GetRegistryKey(regeditType);
                registryItem = registryRoot.OpenSubKey(regeditKey);

                if (registryItem == null) return null;
                return registryItem.GetValue(regeditName);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (registryItem != null) registryItem.Close();
                if (registryRoot != null) registryRoot.Close();
            }
        }
        /// <summary>
        /// 获取注册表值(HKEY_CLASSES_ROOT)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static object GetValueClassesRoot(string regeditKey, string regeditName)
        {
            return GetValue(RegeditTypeEnum.HKEY_CLASSES_ROOT, regeditKey, regeditName);
        }
        /// <summary>
        /// 获取注册表值(HKEY_CURRENT_USER)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static object GetValueCurrentUser(string regeditKey, string regeditName)
        {
            return GetValue(RegeditTypeEnum.HKEY_CURRENT_USER, regeditKey, regeditName);
        }
        /// <summary>
        /// 获取注册表值(HKEY_LOCAL_MACHINE)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static object GetValueLocalMachine(string regeditKey, string regeditName)
        {
            return GetValue(RegeditTypeEnum.HKEY_LOCAL_MACHINE, regeditKey, regeditName);
        }
        /// <summary>
        /// 获取注册表值(HKEY_USERS)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static object GetValueUsers(string regeditKey, string regeditName)
        {
            return GetValue(RegeditTypeEnum.HKEY_USERS, regeditKey, regeditName);
        }
        /// <summary>
        /// 获取注册表值(HKEY_CURRENT_CONFIG)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static object GetValueCurrentConfig(string regeditKey, string regeditName)
        {
            return GetValue(RegeditTypeEnum.HKEY_CURRENT_CONFIG, regeditKey, regeditName);
        }
        #endregion

        #region 删除 KEY 或者 VALUE
        /// <summary>
        /// 删除注册表项/值
        /// </summary>
        /// <param name="regeditType">注册表根节点</param>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="deleteTree">是否删除子树</param>
        /// <returns></returns>
        public static bool DeleteKeyOrValue(RegeditTypeEnum regeditType, string regeditKey, string regeditName = null, bool deleteTree = false)
        {
            RegistryKey registryRoot = null;
            RegistryKey registryItem = null;
            try
            {
                registryRoot = GetRegistryKey(regeditType);
                if (string.IsNullOrEmpty(regeditName))
                {
                    if (!deleteTree)
                    {
                        registryRoot.DeleteSubKey(regeditKey);
                    }
                    else
                    {
                        registryRoot.DeleteSubKeyTree(regeditKey);
                    }
                }
                else
                {
                    registryItem = registryRoot.OpenSubKey(regeditKey, true);
                    if (registryItem != null) registryItem.DeleteValue(regeditName, false);
                }
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (registryItem != null) registryItem.Close();
                if (registryRoot != null) registryRoot.Close();
            }
        }
        /// <summary>
        /// 删除注册表项/值(HKEY_CLASSES_ROOT)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="deleteTree">是否删除子树</param>
        /// <returns></returns>
        public static bool DeleteKeyOrValueClassesRoot(string regeditKey, string regeditName = null, bool deleteTree = false)
        {
            return DeleteKeyOrValue(RegeditTypeEnum.HKEY_CLASSES_ROOT, regeditKey, regeditName, deleteTree);
        }
        /// <summary>
        /// 删除注册表项/值(HKEY_CURRENT_USER)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="deleteTree">是否删除子树</param>
        /// <returns></returns>
        public static bool DeleteKeyOrValueCurrentUser(string regeditKey, string regeditName = null, bool deleteTree = false)
        {
            return DeleteKeyOrValue(RegeditTypeEnum.HKEY_CURRENT_USER, regeditKey, regeditName, deleteTree);
        }
        /// <summary>
        /// 删除注册表项/值(HKEY_LOCAL_MACHINE)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="deleteTree">是否删除子树</param>
        /// <returns></returns>
        public static bool DeleteKeyOrValueLocalMachine(string regeditKey, string regeditName = null, bool deleteTree = false)
        {
            return DeleteKeyOrValue(RegeditTypeEnum.HKEY_LOCAL_MACHINE, regeditKey, regeditName, deleteTree);
        }
        /// <summary>
        /// 删除注册表项/值(HKEY_USERS)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="deleteTree">是否删除子树</param>
        /// <returns></returns>
        public static bool DeleteKeyOrValueUsers(string regeditKey, string regeditName = null, bool deleteTree = false)
        {
            return DeleteKeyOrValue(RegeditTypeEnum.HKEY_USERS, regeditKey, regeditName, deleteTree);
        }
        /// <summary>
        /// 删除注册表项/值(HKEY_CURRENT_CONFIG)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <param name="deleteTree">是否删除子树</param>
        /// <returns></returns>
        public static bool DeleteKeyOrValueCurrentConfig(string regeditKey, string regeditName = null, bool deleteTree = false)
        {
            return DeleteKeyOrValue(RegeditTypeEnum.HKEY_CURRENT_CONFIG, regeditKey, regeditName, deleteTree);
        }
        #endregion

        #region 判断注册表项/键是否存在
        /// <summary>
        /// 验证是否存在注册表项/值
        /// </summary>
        /// <param name="regeditType">注册表根节点</param>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static bool IsExistsKeyOrValue(RegeditTypeEnum regeditType, string regeditKey, string regeditName = null)
        {
            RegistryKey registryRoot = null;
            RegistryKey registryItem = null;
            try
            {
                registryRoot = GetRegistryKey(regeditType);
                registryItem = registryRoot.OpenSubKey(regeditKey, true);

                if (registryItem == null) return false;

                if (!string.IsNullOrEmpty(regeditName))
                {
                    string[] keyNameList = registryItem.GetSubKeyNames();
                    if (keyNameList == null || keyNameList.Length == 0) return false;

                    foreach (string keyName in keyNameList)
                    {
                        if (keyName == regeditName) return true;
                    }
                    return false;
                }
                return true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (registryItem != null) registryItem.Close();
                if (registryRoot != null) registryRoot.Close();
            }
        }
        /// <summary>
        /// 验证是否存在注册表项/值(HKEY_CLASSES_ROOT)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static bool IsExistsKeyOrValueClassesRoot(string regeditKey, string regeditName = null)
        {
            return IsExistsKeyOrValue(RegeditTypeEnum.HKEY_CLASSES_ROOT, regeditKey, regeditName);
        }
        /// <summary>
        /// 验证是否存在注册表项/值(HKEY_CURRENT_USER)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static bool IsExistsKeyOrValueCurrentUser(string regeditKey, string regeditName = null)
        {
            return IsExistsKeyOrValue(RegeditTypeEnum.HKEY_CURRENT_USER, regeditKey, regeditName);
        }
        /// <summary>
        /// 验证是否存在注册表项/值(HKEY_LOCAL_MACHINE)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static bool IsExistsKeyOrValueLocalMachine(string regeditKey, string regeditName = null)
        {
            return IsExistsKeyOrValue(RegeditTypeEnum.HKEY_LOCAL_MACHINE, regeditKey, regeditName);
        }
        /// <summary>
        /// 验证是否存在注册表项/值(HKEY_USERS)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static bool IsExistsKeyOrValueUsers(string regeditKey, string regeditName = null)
        {
            return IsExistsKeyOrValue(RegeditTypeEnum.HKEY_USERS, regeditKey, regeditName);
        }
        /// <summary>
        /// 验证是否存在注册表项/值(HKEY_CURRENT_CONFIG)
        /// </summary>
        /// <param name="regeditKey">注册表项</param>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static bool IsExistsKeyOrValueCurrentConfig(string regeditKey, string regeditName = null)
        {
            return IsExistsKeyOrValue(RegeditTypeEnum.HKEY_CURRENT_CONFIG, regeditKey, regeditName);
        }
        #endregion

        #region 常用注册表项（开机启动）
        /// <summary>
        /// 设置注册表启动项
        /// </summary>
        /// <param name="regeditName">注册表键</param>
        /// <param name="regeditValue">数据</param>
        /// <param name="valueKind">RegistryValueKind 枚举</param>
        /// <returns></returns>
        public static bool SetValueRun(string regeditName, object regeditValue, RegistryValueKind valueKind = RegistryValueKind.None)
        {
            return SetValueLocalMachine(REGEDIT_KEY_RUN, regeditName, regeditValue, valueKind);
        }
        /// <summary>
        /// 获取注册表启动项
        /// </summary>
        /// <param name="regeditName">注册表键</param>
        /// <returns></returns>
        public static object GetValueRun(string regeditName)
        {
            return GetValueLocalMachine(REGEDIT_KEY_RUN, regeditName);
        }
        /// <summary>
        /// 删除注册表启动项/值
        /// </summary>
        /// <param name="regeditName">注册表键</param>
        /// <param name="deleteTree">是否删除子树</param>
        /// <returns></returns>
        public static bool DeleteKeyOrValueRun(string regeditName = null, bool deleteTree = false)
        {
            return DeleteKeyOrValueLocalMachine(REGEDIT_KEY_RUN, regeditName, deleteTree);
        }
        /// <summary>
        /// 判断注册表启动项是否有值
        /// </summary>
        /// <param name="regeditName"></param>
        /// <returns></returns>
        public static bool IsExistsKeyOrValueRun(string regeditName = null)
        {
            return IsExistsKeyOrValueLocalMachine(REGEDIT_KEY_RUN, regeditName);
        }
        #endregion

        #endregion

        #region 逻辑处理私有方法
        private static RegistryKey GetRegistryKey(RegeditTypeEnum regeditType)
        {
            if (regeditType == RegeditTypeEnum.HKEY_CLASSES_ROOT) return Registry.ClassesRoot;
            if (regeditType == RegeditTypeEnum.HKEY_CURRENT_USER) return Registry.CurrentUser;
            if (regeditType == RegeditTypeEnum.HKEY_LOCAL_MACHINE) return Registry.LocalMachine;
            if (regeditType == RegeditTypeEnum.HKEY_USERS) return Registry.Users;
            return Registry.CurrentConfig;
        }
        #endregion
    }
}
