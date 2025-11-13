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
				if (string.IsNullOrWhiteSpace(directoryPath))
				{
					GlobalVars.Loggi.Warning("HasDirectoryRights called with null or empty directoryPath");
					return false;
				}

				if (!Directory.Exists(directoryPath))
				{
					GlobalVars.Loggi.Warning("Directory does not exist: {DirectoryPath}", directoryPath);
					return false;
				}

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

				GlobalVars.Loggi.Warning("HasDirectoryRights called without valid user identifier");
				return false;
			}
			catch (UnauthorizedAccessException uaEx)
			{
				GlobalVars.Loggi.Warning(uaEx, "Access denied when checking directory rights for: {DirectoryPath}", directoryPath);
				return false;
			}
			catch (Exception ex)
			{
				GlobalVars.Loggi.Error(ex, "Error checking directory rights for: {DirectoryPath}", directoryPath);
				return false;
			}
		}

		public static void AddDirectorySecurity(string directoryPath, string winUserString = null, WindowsIdentity winUser = null,
            FileSystemRights rights = FileSystemRights.FullControl, InheritanceFlags inheritanceFlags = InheritanceFlags.ObjectInherit, PropagationFlags propagationFlags = PropagationFlags.None, AccessControlType controlType = AccessControlType.Allow)
        {
            try
            {
				if (string.IsNullOrWhiteSpace(directoryPath))
				{
					GlobalVars.Loggi.Error("AddDirectorySecurity called with null or empty directoryPath");
					throw new ArgumentNullException(nameof(directoryPath));
				}

				if (!Directory.Exists(directoryPath))
				{
					GlobalVars.Loggi.Error("Cannot add security to non-existent directory: {DirectoryPath}", directoryPath);
					throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
				}

                // Create a new DirectoryInfo object.
                DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
                // Get a DirectorySecurity object that represents the
                // current security settings.
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                if (winUserString != null)
                {
                    // Add the FileSystemAccessRule to the security settings.
                    dSecurity.AddAccessRule(new FileSystemAccessRule(winUserString, rights, inheritanceFlags, propagationFlags, controlType));
					GlobalVars.Loggi.Information("Added directory security for user: {User} on {Path}", winUserString, directoryPath);
                }
                else if (winUser != null)
                {
                    // Add the FileSystemAccessRule to the security settings.
                    dSecurity.AddAccessRule(new FileSystemAccessRule(winUser.User.Value, rights, inheritanceFlags, propagationFlags, controlType));
					GlobalVars.Loggi.Information("Added directory security for user: {User} on {Path}", winUser.Name, directoryPath);
                }
				else
				{
					GlobalVars.Loggi.Error("AddDirectorySecurity called without valid user identifier");
					throw new ArgumentException("Either winUserString or winUser must be provided");
				}
                // Set the new access settings.
                dInfo.SetAccessControl(dSecurity);
            }
			catch (UnauthorizedAccessException uaEx)
			{
				GlobalVars.Loggi.Error(uaEx, "Access denied when adding directory security for: {DirectoryPath}", directoryPath);
				throw;
			}
            catch (Exception ex)
            {
				GlobalVars.Loggi.Error(ex, "Error adding directory security for: {DirectoryPath}", directoryPath);
				throw;
            }
        }

        public static void RemoveDirectorySecurity(string directoryPath, string winUserString = null, WindowsIdentity winUser = null,
            FileSystemRights rights = FileSystemRights.FullControl, InheritanceFlags inheritanceFlags = InheritanceFlags.ContainerInherit, PropagationFlags propagationFlags = PropagationFlags.None, AccessControlType controlType = AccessControlType.Allow)
        {
            try
            {
				if (string.IsNullOrWhiteSpace(directoryPath))
				{
					GlobalVars.Loggi.Error("RemoveDirectorySecurity called with null or empty directoryPath");
					throw new ArgumentNullException(nameof(directoryPath));
				}

				if (!Directory.Exists(directoryPath))
				{
					GlobalVars.Loggi.Error("Cannot remove security from non-existent directory: {DirectoryPath}", directoryPath);
					throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
				}

                // Create a new DirectoryInfo object.
                DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
                // Get a DirectorySecurity object that represents the
                // current security settings.
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                if (winUserString != null)
                {
                    // Removes the FileSystemAccessRule to the security settings.
                    dSecurity.RemoveAccessRule(new FileSystemAccessRule(winUserString, rights, inheritanceFlags, propagationFlags, controlType));
					GlobalVars.Loggi.Information("Removed directory security for user: {User} on {Path}", winUserString, directoryPath);
                }
                else if (winUser != null)
                {
                    // Removes the FileSystemAccessRule to the security settings.
                    dSecurity.RemoveAccessRule(new FileSystemAccessRule(winUser.User.Value, rights, inheritanceFlags, propagationFlags, controlType));
					GlobalVars.Loggi.Information("Removed directory security for user: {User} on {Path}", winUser.Name, directoryPath);
                }
				else
				{
					GlobalVars.Loggi.Error("RemoveDirectorySecurity called without valid user identifier");
					throw new ArgumentException("Either winUserString or winUser must be provided");
				}
                // Set the new access settings.
                dInfo.SetAccessControl(dSecurity);
            }
			catch (UnauthorizedAccessException uaEx)
			{
				GlobalVars.Loggi.Error(uaEx, "Access denied when removing directory security for: {DirectoryPath}", directoryPath);
				throw;
			}
            catch (Exception ex)
            {
				GlobalVars.Loggi.Error(ex, "Error removing directory security for: {DirectoryPath}", directoryPath);
				throw;
            }
        }
    }
}
