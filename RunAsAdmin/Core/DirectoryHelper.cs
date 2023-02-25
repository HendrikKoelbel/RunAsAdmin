using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace RunAsAdmin.Core
{
    public static class DirectoryHelper
    {
		public static bool HasDirectoryRights(string directoryPath, FileSystemRights rights, string winUserString = null, WindowsIdentity winUser = null)
		{
			try
			{
                var ds = new DirectorySecurity();
                DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
                ds = FileSystemAclExtensions.GetAccessControl(dirInfo);
				var rules = ds.GetAccessRules(true, true, typeof(SecurityIdentifier));

				if (winUserString != null)
				{
					return rules.OfType<FileSystemAccessRule>().Any(r =>
						 ((int)r.FileSystemRights & (int)rights) != 0 && r.IdentityReference.Value == winUserString);
				}
				else if (winUser != null)
				{
					return rules.OfType<FileSystemAccessRule>().Any(r =>
						 ((int)r.FileSystemRights & (int)rights) != 0 && r.IdentityReference.Value == winUser.User.Value);
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static void AddDirectorySecurity(string directoryPath, string winUserString = null, WindowsIdentity winUser = null,
            FileSystemRights rights = FileSystemRights.FullControl, InheritanceFlags inheritanceFlags = InheritanceFlags.ObjectInherit, PropagationFlags propagationFlags = PropagationFlags.None, AccessControlType controlType = AccessControlType.Allow)
        {
            try
            {
                // Create a new DirectoryInfo object.
                DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
                // Get a DirectorySecurity object that represents the 
                // current security settings.
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                if (winUserString != null)
                {
                    // Add the FileSystemAccessRule to the security settings. 
                    dSecurity.AddAccessRule(new FileSystemAccessRule(winUserString, rights, inheritanceFlags, propagationFlags, controlType));
                }
                else if (winUser != null)
                {
                    // Add the FileSystemAccessRule to the security settings. 
                    dSecurity.AddAccessRule(new FileSystemAccessRule(winUser.User.Value, rights, inheritanceFlags, propagationFlags, controlType));
                }
                // Set the new access settings.
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception)
            {
            }
        }

        public static void RemoveDirectorySecurity(string directoryPath, string winUserString = null, WindowsIdentity winUser = null,
            FileSystemRights rights = FileSystemRights.FullControl, InheritanceFlags inheritanceFlags = InheritanceFlags.ContainerInherit, PropagationFlags propagationFlags = PropagationFlags.None, AccessControlType controlType = AccessControlType.Allow)
        {
            try
            {
                // Create a new DirectoryInfo object.
                DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
                // Get a DirectorySecurity object that represents the 
                // current security settings.
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                if (winUserString != null)
                {
                    // Removes the FileSystemAccessRule to the security settings. 
                    dSecurity.RemoveAccessRule(new FileSystemAccessRule(winUserString, rights, inheritanceFlags, propagationFlags, controlType));
                }
                else if (winUser != null)
                {
                    // Removes the FileSystemAccessRule to the security settings. 
                    dSecurity.RemoveAccessRule(new FileSystemAccessRule(winUser.User.Value, rights, inheritanceFlags, propagationFlags, controlType));
                }
                // Set the new access settings.
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception)
            {
            }
        }
    }
}
