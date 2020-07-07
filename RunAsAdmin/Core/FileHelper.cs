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
    internal static class FileHelper
    {
        public static bool CheckWriteAccess(string filePath)
        {
            try
            {
                File.Open(filePath, FileMode.Open, FileAccess.Write).Dispose();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

        // Adds an ACL entry on the specified file for the specified account.
        public static void AddFileSecurity(string fileName, string account = null,
            FileSystemRights rights = FileSystemRights.FullControl, AccessControlType controlType = AccessControlType.Allow)
        {
            account ??= WindowsIdentity.GetCurrent().Name;

            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Add the FileSystemAccessRule to the security settings.
            fSecurity.AddAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

        // Removes an ACL entry on the specified file for the specified account.
        public static void RemoveFileSecurity(string fileName, string account = null,
            FileSystemRights rights = FileSystemRights.FullControl, AccessControlType controlType = AccessControlType.Allow)
        {
            account ??= WindowsIdentity.GetCurrent().Name;

            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = File.GetAccessControl(fileName);

            // Remove the FileSystemAccessRule from the security settings.
            fSecurity.RemoveAccessRule(new FileSystemAccessRule(account,
                rights, controlType));

            // Set the new access settings.
            File.SetAccessControl(fileName, fSecurity);
        }

    }
}
