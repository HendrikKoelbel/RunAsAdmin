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

namespace RunAsAdmin.Core
{
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
                using var forest = Forest.GetCurrentForest();
                foreach (Domain domain in forest.Domains)
                {
                    domainList.Add(domain.Name);
                    domain.Dispose();
                }
                return domainList;
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
                using var computerEntry = new DirectoryEntry(path);
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
                using var forest = Forest.GetCurrentForest();
                foreach (Domain domain in forest.Domains)
                {
                    using (var context = new PrincipalContext(ContextType.Domain, domain.Name))
                    {
                        using var searcher = new PrincipalSearcher(new UserPrincipal(context));
                        foreach (var result in searcher.FindAll())
                        {
                            DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                            ADUsers.Add(de.Properties["samAccountName"].Value.ToString());
                        }
                    }
                    domain.Dispose();
                }
                return ADUsers;
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

        #region SecureString Helper
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

        public static string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
        #endregion

        #region Get current user direcetory
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
    }
}