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
                if (comboBox == null)
                {
                    GlobalVars.Loggi.Error("SetDataSource called with null comboBox");
                    throw new ArgumentNullException(nameof(comboBox));
                }

                if (stringArray != null && stringArray.Length > 0)
                {
                    Array.Sort(stringArray);
                    AutoCompleteStringCollection col = new AutoCompleteStringCollection();
                    foreach (var item in stringArray)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            col.Add(item);
                        }
                    }
                    comboBox.ItemsSource = col;
                    GlobalVars.Loggi.Debug("Successfully set data source with {Count} items", col.Count);
                }
                else
                {
                    GlobalVars.Loggi.Warning("SetDataSource called with null or empty stringArray");
                    comboBox.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Error setting data source for comboBox");
                throw;
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
                GlobalVars.Loggi.Debug("Successfully retrieved {Count} domains", domainList.Count);
                return domainList;
            }
            catch (ActiveDirectoryObjectNotFoundException adEx)
            {
                GlobalVars.Loggi.Warning(adEx, "Active Directory not available, returning only local machine");
                return domainList;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Error retrieving domain list, returning only local machine");
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
                    childEntry.Dispose(); // Dispose each child entry
                }
                GlobalVars.Loggi.Debug("Successfully retrieved {Count} local users", users.Count);
                return users;
            }
            catch (UnauthorizedAccessException uaEx)
            {
                GlobalVars.Loggi.Warning(uaEx, "Access denied when retrieving local users");
                return users;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Error retrieving local users");
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
                            using (result)
                            {
                                DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                                if (de?.Properties["samAccountName"]?.Value != null)
                                {
                                    ADUsers.Add(de.Properties["samAccountName"].Value.ToString());
                                }
                            }
                        }
                    }
                    domain.Dispose();
                }
                GlobalVars.Loggi.Debug("Successfully retrieved {Count} AD users", ADUsers.Count);
                return ADUsers;
            }
            catch (ActiveDirectoryObjectNotFoundException adEx)
            {
                GlobalVars.Loggi.Warning(adEx, "Active Directory not available");
                return ADUsers;
            }
            catch (PrincipalServerDownException psEx)
            {
                GlobalVars.Loggi.Warning(psEx, "Domain controller is not available");
                return ADUsers;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Error retrieving AD users");
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
                var localUsers = GetLocalUsers();
                foreach (var user in localUsers)
                {
                    allUsers.Add(user);
                }

                var adUsers = GetADUsers();
                foreach (var user in adUsers)
                {
                    // Avoid duplicates
                    if (!allUsers.Contains(user))
                    {
                        allUsers.Add(user);
                    }
                }

                GlobalVars.Loggi.Debug("Successfully retrieved {Count} total users ({LocalCount} local, {ADCount} AD)",
                    allUsers.Count, localUsers.Count, adUsers.Count);
                return allUsers;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Error retrieving all users");
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
                using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain))
                {
                    // find the user
                    using (UserPrincipal user = UserPrincipal.FindByIdentity(ctx, WindowsIdentity.GetCurrent().Name))
                    {
                        if (user != null)
                        {
                            var usersSid = user.Sid.ToString();
                            var username = user.DisplayName;
                            var userSamAccountName = user.SamAccountName;
                            GlobalVars.Loggi.Debug("Successfully retrieved SID for user: {Username}", username);
                            return usersSid;
                        }

                        GlobalVars.Loggi.Warning("User not found in domain context for: {CurrentUser}", WindowsIdentity.GetCurrent().Name);
                        return null;
                    }
                }
            }
            catch (PrincipalServerDownException psEx)
            {
                GlobalVars.Loggi.Error(psEx, "Domain controller is not available");
                return null;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Error retrieving user SID for: {CurrentUser}", WindowsIdentity.GetCurrent().Name);
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