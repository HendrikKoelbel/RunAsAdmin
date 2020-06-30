using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Windows.Forms;

namespace RunAsAdmin.Helper
{

    /// <summary>
    /// TODO: Optimization of the helper class
    /// </summary>

    public static class Helper
    {
        #region  Bind textbox custom source
        public static void SetDataSource(System.Windows.Controls.ComboBox comboBox, params string[] stringArray)
        {
            try
            {
                if (stringArray != null)
                {
                    Array.Sort(stringArray);
                    AutoCompleteStringCollection col = new AutoCompleteStringCollection();
                    foreach (var item in stringArray)
                    {
                        col.Add(item);
                    }
                    comboBox.ItemsSource = col;
                }
                else
                {
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Get all Domains as string list
        public static List<string> GetAllDomains()
        {
            var domainList = new List<string>();
            try
            {
                domainList.Add(Environment.MachineName);
                using (var forest = Forest.GetCurrentForest())
                {
                    foreach (Domain domain in forest.Domains)
                    {
                        domainList.Add(domain.Name);
                        domain.Dispose();
                    }
                    return domainList;
                }
            }
            catch
            {
                return domainList;
            }
        }
        #endregion

        #region Get all local users as string list
        private const int UF_ACCOUNTDISABLE = 0x0002;
        public static List<string> GetLocalUsers()
        {
            var users = new List<string>();
            try
            {
                var path = string.Format("WinNT://{0},computer", Environment.MachineName);
                using (var computerEntry = new DirectoryEntry(path))
                {
                    foreach (DirectoryEntry childEntry in computerEntry.Children)
                    {
                        if (childEntry.SchemaClassName == "User")// filter all users
                        {
                            if (((int)childEntry.Properties["UserFlags"].Value & UF_ACCOUNTDISABLE) != UF_ACCOUNTDISABLE)// only if accounts are enabled
                            {
                                users.Add(childEntry.Name); // add active user to list
                            }
                        }
                    }
                    return users;
                }
            }
            catch (Exception)
            {
                return users;
            }
        }
        #endregion

        #region Get all ad users as string list
        public static List<string> GetADUsers()
        {
            var ADUsers = new List<string>();
            try
            {
                using (var forest = Forest.GetCurrentForest())
                {
                    foreach (Domain domain in forest.Domains)
                    {
                        using (var context = new PrincipalContext(ContextType.Domain, domain.Name))
                        {
                            using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                            {
                                foreach (var result in searcher.FindAll())
                                {
                                    DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                                    ADUsers.Add(de.Properties["samAccountName"].Value.ToString());
                                }
                            }
                        }
                        domain.Dispose();
                    }
                    return ADUsers;
                }
            }
            catch (Exception)
            {
                return ADUsers;
            }
        }
        #endregion

        #region Get all users (AD + Local)
        public static List<string> GetAllUsers()
        {
            var allUsers = new List<string>();
            try
            {
                foreach (var user in GetLocalUsers())
                {
                    allUsers.Add(user);
                }
                foreach (var user in GetADUsers())
                {
                    allUsers.Add(user);
                }
                return allUsers;
            }
            catch (Exception)
            {
                return allUsers;
            }
        }
        #endregion

        #region Uppercase first letter
        public static string UppercaseFirst(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }
        #endregion

        #region Get users SID
        public static string GetUsersSID()
        {
            try
            {
                // create your domain context
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                // find the user
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, WindowsIdentity.GetCurrent().Name);

                if (user != null)
                {
                    var usersSid = user.Sid.ToString();
                    var username = user.DisplayName;
                    var userSamAccountName = user.SamAccountName;
                    return usersSid;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region 
        public static string GetUserDirectoryPath()
        {
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                path = Directory.GetParent(path).ToString();
                return path;
            }
            return path;
        }
        #endregion

        #region Add the access control entry to the file
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="account"></param>
        /// <param name="rights"></param>
        /// <param name="controlType"></param>
        public static void AddFileSecurity(string fileName, string account, FileSystemRights rights, AccessControlType controlType)
        {
            try
            {
                // Get a FileSecurity object that represents the
                // current security settings.
                FileSecurity fSecurity = File.GetAccessControl(fileName);
                // Add the FileSystemAccessRule to the security settings.
                fSecurity.AddAccessRule(new FileSystemAccessRule(account, rights, InheritanceFlags.ContainerInherit, PropagationFlags.None, controlType));
                fSecurity.AddAccessRule(new FileSystemAccessRule(account, rights, InheritanceFlags.ObjectInherit, PropagationFlags.None, controlType));            // Set the new access settings.
                File.SetAccessControl(fileName, fSecurity);
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="identity"></param>
        /// <param name="rights"></param>
        /// <param name="controlType"></param>
        public static void AddFileSecurity(string fileName, IdentityReference identity, FileSystemRights rights, AccessControlType controlType)
        {
            try
            {
                // Get a FileSecurity object that represents the
                // current security settings.
                FileSecurity fSecurity = File.GetAccessControl(fileName);
                // Add the FileSystemAccessRule to the security settings.
                fSecurity.AddAccessRule(new FileSystemAccessRule(identity, rights, InheritanceFlags.ContainerInherit, PropagationFlags.None, controlType));
                fSecurity.AddAccessRule(new FileSystemAccessRule(identity, rights, InheritanceFlags.ObjectInherit, PropagationFlags.None, controlType));            // Set the new access settings.
                File.SetAccessControl(fileName, fSecurity);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Remove the access control entry from the file
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="account"></param>
        /// <param name="rights"></param>
        /// <param name="controlType"></param>
        public static void RemoveFileSecurity(string fileName, string account, FileSystemRights rights, AccessControlType controlType)
        {
            try
            {
                // Get a FileSecurity object that represents the
                // current security settings.
                FileSecurity fSecurity = File.GetAccessControl(fileName);
                // Remove the FileSystemAccessRule from the security settings.
                fSecurity.RemoveAccessRule(new FileSystemAccessRule(account, rights, InheritanceFlags.ContainerInherit, PropagationFlags.None, controlType));
                fSecurity.RemoveAccessRule(new FileSystemAccessRule(account, rights, InheritanceFlags.ObjectInherit, PropagationFlags.None, controlType));
                // Set the new access settings.
                File.SetAccessControl(fileName, fSecurity);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Adds an ACL entry on the specified directory for the specified account
        public static void AddDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            try
            {
                // Create a new DirectoryInfo object.
                DirectoryInfo dInfo = new DirectoryInfo(FileName);
                // Get a DirectorySecurity object that represents the 
                // current security settings.
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                // Add the FileSystemAccessRule to the security settings. 
                dSecurity.AddAccessRule(new FileSystemAccessRule(Account, Rights, InheritanceFlags.ContainerInherit, PropagationFlags.None, ControlType));
                dSecurity.AddAccessRule(new FileSystemAccessRule(Account, Rights, InheritanceFlags.ObjectInherit, PropagationFlags.None, ControlType));
                // Set the new access settings.
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception)
            {
            }
        }
        public static void AddDirectorySecurity(string FileName, IdentityReference identity, FileSystemRights Rights, AccessControlType ControlType)
        {
            try
            {
                // Create a new DirectoryInfo object.
                DirectoryInfo dInfo = new DirectoryInfo(FileName);
                // Get a DirectorySecurity object that represents the 
                // current security settings.
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                // Add the FileSystemAccessRule to the security settings. 
                dSecurity.AddAccessRule(new FileSystemAccessRule(identity, Rights, InheritanceFlags.ContainerInherit, PropagationFlags.None, ControlType));
                dSecurity.AddAccessRule(new FileSystemAccessRule(identity, Rights, InheritanceFlags.ObjectInherit, PropagationFlags.None, ControlType));
                // Set the new access settings.
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Removes an ACL entry on the specified directory for the specified account
        public static void RemoveDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            try
            {
                // Create a new DirectoryInfo object.
                DirectoryInfo dInfo = new DirectoryInfo(FileName);
                // Get a DirectorySecurity object that represents the 
                // current security settings.
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                // Add the FileSystemAccessRule to the security settings. 
                dSecurity.RemoveAccessRule(new FileSystemAccessRule(Account, Rights, InheritanceFlags.ContainerInherit, PropagationFlags.None, ControlType));
                dSecurity.RemoveAccessRule(new FileSystemAccessRule(Account, Rights, InheritanceFlags.ObjectInherit, PropagationFlags.None, ControlType));
                // Set the new access settings.
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Check if directory has write permission
        public static bool HasFolderRights(string path, FileSystemRights rights, WindowsIdentity user)
        {
            try
            {
                var security = Directory.GetAccessControl(path);
                var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                return rules.OfType<FileSystemAccessRule>().Any(r =>
                        ((int)r.FileSystemRights & (int)rights) != 0 && r.IdentityReference.Value == user.User.Value);
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Get a secure string
        public static SecureString GetSecureString(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password is invalid or null");

            var securePassword = new SecureString();

            foreach (char c in password)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }
        #endregion

        #region Update GroupPolicy
        public static bool UpdateGroupPolicy()
        {
            try
            {
                FileInfo execFile = new FileInfo("gpupdate.exe");
                Process proc = new Process();
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.FileName = execFile.Name;
                proc.StartInfo.Arguments = "/force";
                proc.Start();
                //Wait for GPUpdate to finish
                while (!proc.HasExited)
                {
                    Application.DoEvents();
                    Thread.Sleep(100);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Configure network link in windows registry
        public static void ConfigureWindowsRegistry()
        {
            try
            {
                RegistryKey localMachine = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64); //here you specify where exactly you want your entry

                var reg = localMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", true);
                if (reg == null)
                {
                    reg = localMachine.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", true);
                }

                if (reg.GetValue("EnableLinkedConnections") == null)
                {
                    reg.SetValue("EnableLinkedConnections", "1", RegistryValueKind.DWord);
                    //MessageBox.Show(
                    //    "Your configuration is now created,you have to restart your device to let app work perfektly");
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}