using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RunAsAdmin.Core
{
    public static class FileHelper
    {
        public static bool HasFolderRights(string folderPath, FileSystemRights fileRights, string winUserString = null, WindowsIdentity winUser = null)
        {
            try
            {                    
                var security = Directory.GetAccessControl(folderPath);
                var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                if (winUserString != null)
                {
                    return rules.OfType<FileSystemAccessRule>().Any(r =>
                       ((int)r.FileSystemRights & (int)fileRights) != 0 && r.IdentityReference.Value == winUserString);
                }
                else
                {
                    return rules.OfType<FileSystemAccessRule>().Any(r =>
                       ((int)r.FileSystemRights & (int)fileRights) != 0 && r.IdentityReference.Value == winUser.User.Value);
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
                return false;
            }
        }

        public static bool HasFileRights(string filePath, FileSystemRights fileRights, WindowsIdentity winUser)
        {
            try
            {
                var security = File.GetAccessControl(filePath);
                var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                return rules.OfType<FileSystemAccessRule>().Any(r =>
                        ((int)r.FileSystemRights & (int)fileRights) != 0 && r.IdentityReference.Value == winUser.User.Value);
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
                return false;
            }
        }

        // Adds an ACL entry on the specified file for the specified account.
        public static void AddFileSecurity(string filePath, string account = null,
            FileSystemRights rights = FileSystemRights.FullControl, AccessControlType controlType = AccessControlType.Allow)
        {
            account ??= WindowsIdentity.GetCurrent().Name;

            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(filePath);

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.AddAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(filePath, fSecurity);
        }

        // Removes an ACL entry on the specified file for the specified account.
        public static void RemoveFileSecurity(string filePath, string account = null,
            FileSystemRights rights = FileSystemRights.FullControl, AccessControlType controlType = AccessControlType.Allow)
        {
            account ??= WindowsIdentity.GetCurrent().Name;

            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(filePath);

            // Remove the FileSystemAccessRule from the security settings.
            fSecurity.RemoveAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(filePath, fSecurity);
        }

        // Adds an ACL entry on the specified directory for the specified account.
        public static void AddDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);

            // Get a DirectorySecurity object that represents the
            // current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings.
            dSecurity.AddAccessRule(new FileSystemAccessRule(Account, Rights, ControlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);
        }

        // Removes an ACL entry on the specified directory for the specified account.
        public static void RemoveDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);

            // Get a DirectorySecurity object that represents the
            // current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings.
            dSecurity.RemoveAccessRule(new FileSystemAccessRule(Account, Rights, ControlType));
            dInfo.SetAccessControl(dSecurity);
        }

    }
}
