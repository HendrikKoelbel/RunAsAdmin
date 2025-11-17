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

                // Try to get system domain
                try
                {
                    string systemDomain = Environment.UserDomainName;
                    if (!string.IsNullOrEmpty(systemDomain) &&
                        !systemDomain.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) &&
                        !domainList.Contains(systemDomain))
                    {
                        domainList.Add(systemDomain);
                        GlobalVars.Loggi.Debug("Added system domain: {Domain}", systemDomain);
                    }
                }
                catch (Exception domainEx)
                {
                    GlobalVars.Loggi.Debug(domainEx, "Could not retrieve system domain");
                }

                using var forest = Forest.GetCurrentForest();
                foreach (Domain domain in forest.Domains)
                {
                    if (!domainList.Contains(domain.Name))
                    {
                        domainList.Add(domain.Name);
                    }
                    domain.Dispose();
                }
                GlobalVars.Loggi.Debug("Successfully retrieved {Count} domains", domainList.Count);
                return domainList;
            }
            catch (ActiveDirectoryObjectNotFoundException adEx)
            {
                GlobalVars.Loggi.Warning(adEx, "Active Directory not available, returning available domains");
                return domainList;
            }
            catch (ActiveDirectoryOperationException adOpEx)
            {
                GlobalVars.Loggi.Warning(adOpEx, "Not associated with Active Directory domain or forest, returning available domains");
                return domainList;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Error retrieving domain list, returning available domains");
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
            catch (ActiveDirectoryOperationException adOpEx)
            {
                GlobalVars.Loggi.Warning(adOpEx, "Not associated with Active Directory domain or forest");
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

        #region Get cached AD users from local profiles
        /// <summary>
        /// Gets AD users that have logged into this machine and have local profiles
        /// This works even when AD is not accessible
        /// </summary>
        public static List<string> GetCachedADUsers()
        {
            var cachedUsers = new List<string>();
            try
            {
                // Open the ProfileList registry key
                using (var profileListKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList"))
                {
                    if (profileListKey != null)
                    {
                        foreach (string sidString in profileListKey.GetSubKeyNames())
                        {
                            try
                            {
                                // Try to convert SID string to SecurityIdentifier
                                if (sidString.StartsWith("S-1-5-21-")) // Domain user SID pattern
                                {
                                    var sid = new SecurityIdentifier(sidString);

                                    // Try to translate SID to account name
                                    try
                                    {
                                        var account = sid.Translate(typeof(NTAccount)) as NTAccount;
                                        if (account != null)
                                        {
                                            string accountName = account.Value;
                                            // Extract username from DOMAIN\Username format
                                            if (accountName.Contains("\\"))
                                            {
                                                string username = accountName.Split('\\')[1];
                                                if (!cachedUsers.Contains(username))
                                                {
                                                    cachedUsers.Add(username);
                                                }
                                            }
                                        }
                                    }
                                    catch (IdentityNotMappedException)
                                    {
                                        // SID cannot be resolved (user might be deleted from AD)
                                        GlobalVars.Loggi.Debug("Could not resolve SID: {SID}", sidString);
                                    }
                                }
                            }
                            catch (Exception sidEx)
                            {
                                GlobalVars.Loggi.Debug(sidEx, "Error processing SID: {SID}", sidString);
                            }
                        }
                    }
                }

                GlobalVars.Loggi.Debug("Successfully retrieved {Count} cached AD users from profiles", cachedUsers.Count);
                return cachedUsers;
            }
            catch (SecurityException secEx)
            {
                GlobalVars.Loggi.Warning(secEx, "Insufficient permissions to read user profiles");
                return cachedUsers;
            }
            catch (UnauthorizedAccessException uaEx)
            {
                GlobalVars.Loggi.Warning(uaEx, "Access denied when reading user profiles");
                return cachedUsers;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Error retrieving cached AD users");
                return cachedUsers;
            }
        }
        #endregion

        #region Get all users (AD + Local + Cached)
        public static List<string> GetAllUsers()
        {
            var allUsers = new List<string>();
            try
            {
                // Get local users first
                var localUsers = GetLocalUsers();
                foreach (var user in localUsers)
                {
                    if (!allUsers.Contains(user))
                    {
                        allUsers.Add(user);
                    }
                }

                // Try to get AD users from directory
                var adUsers = GetADUsers();
                foreach (var user in adUsers)
                {
                    if (!allUsers.Contains(user))
                    {
                        allUsers.Add(user);
                    }
                }

                // If AD is not available or returned no users, get cached AD users
                if (adUsers.Count == 0)
                {
                    GlobalVars.Loggi.Information("AD users not available, retrieving cached AD users from local profiles");
                    var cachedUsers = GetCachedADUsers();
                    foreach (var user in cachedUsers)
                    {
                        if (!allUsers.Contains(user))
                        {
                            allUsers.Add(user);
                        }
                    }
                    GlobalVars.Loggi.Debug("Successfully retrieved {Count} total users ({LocalCount} local, {CachedCount} cached AD)",
                        allUsers.Count, localUsers.Count, cachedUsers.Count);
                }
                else
                {
                    GlobalVars.Loggi.Debug("Successfully retrieved {Count} total users ({LocalCount} local, {ADCount} AD)",
                        allUsers.Count, localUsers.Count, adUsers.Count);
                }

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