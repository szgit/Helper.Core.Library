/*
 * 作用：工具集合。
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.IO;

namespace Helper.Core.Library
{
    public class ToolHelper
    {
        #region 设置目录权限
        /// <summary>
        /// 设置目录可操作权限
        /// </summary>
        /// <param name="user">系统用户，例：Users</param>
        /// <param name="directoryPath">目录路径</param>
        /// <returns></returns>
        public static bool SetDirectoryAccess(string user, string directoryPath)
        {
            FileSystemRights rights = FileSystemRights.FullControl;

            FileSystemAccessRule rule = new FileSystemAccessRule(user, rights, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            DirectorySecurity security = directoryInfo.GetAccessControl(AccessControlSections.Access);

            bool result = false;
            security.ModifyAccessRule(AccessControlModification.Set, rule, out result);
            if (!result) return false;

            InheritanceFlags flags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
            rule = new FileSystemAccessRule(user, rights, flags, PropagationFlags.InheritOnly, AccessControlType.Allow);

            security.ModifyAccessRule(AccessControlModification.Add, rule, out result);
            if (!result) return false;

            directoryInfo.SetAccessControl(security);
            
            return true;
        }
        #endregion
    }
}
