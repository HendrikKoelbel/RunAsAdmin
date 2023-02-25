using SimpleImpersonation;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;

namespace RunAsAdmin.Core
{
    public static class FileHelper
    {
		public static bool HasFileRights(string filePath, FileSystemRights fileRights, string winUserString = null, WindowsIdentity winUser = null)
        {
            try
            {
                var security = new DirectoryInfo(filePath).GetAccessControl();
                var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                if (winUserString != null)
                {
                    return rules.OfType<FileSystemAccessRule>().Any(r =>
                       ((int)r.FileSystemRights & (int)fileRights) != 0 && r.IdentityReference.Value == winUserString);
                }
                else if (winUser != null)
                {
                    return rules.OfType<FileSystemAccessRule>().Any(r =>
                       ((int)r.FileSystemRights & (int)fileRights) != 0 && r.IdentityReference.Value == winUser.User.Value);
                }
                return false;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
                return false;
            }
        }

		// Adds an ACL entry on the specified file for the specified account.
		public static void AddFileSecurity(string filePath, string winUserString = null, WindowsIdentity winUser = null,
            FileSystemRights rights = FileSystemRights.FullControl, AccessControlType controlType = AccessControlType.Allow)
        {
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = new FileInfo(filePath).GetAccessControl();
            if (winUserString != null)
            {
                // Add the FileSystemAccessRule to the security settings.
                fSecurity.AddAccessRule(new FileSystemAccessRule(winUserString,
                    rights, controlType));
            }
            else if (winUser != null)
            {
                // Add the FileSystemAccessRule to the security settings.
                fSecurity.AddAccessRule(new FileSystemAccessRule(winUser.User.Value,
                    rights, controlType));
            }
            FileInfo file = new FileInfo(filePath);
            // Set the new access settings.
            file.SetAccessControl(fSecurity);
        }

        // Removes an ACL entry on the specified file for the specified account.
        public static void RemoveFileSecurity(string filePath, string winUserString = null, WindowsIdentity winUser = null,
            FileSystemRights rights = FileSystemRights.FullControl, AccessControlType controlType = AccessControlType.Allow)
        {
            // Get a FileSecurity object that represents the
            // current security settings.
            FileSecurity fSecurity = new FileInfo(filePath).GetAccessControl();

            if (winUserString != null)
            {
                // Remove the FileSystemAccessRule from the security settings.
                fSecurity.RemoveAccessRule(new FileSystemAccessRule(winUserString,
                    rights, controlType));
            }
            else if (winUser != null)
            {
                // Remove the FileSystemAccessRule from the security settings.
                fSecurity.RemoveAccessRule(new FileSystemAccessRule(winUser.User.Value,
                    rights, controlType));
            }

			FileInfo file = new FileInfo(filePath);
			// Set the new access settings.
			file.SetAccessControl(fSecurity);
        }
    }
}
