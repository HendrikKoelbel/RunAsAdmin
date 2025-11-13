using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;

namespace RunAsAdmin.Core
{
    public static class UserListHelper
    {
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
                GlobalVars.Loggi.Debug("UserListHelper: Successfully retrieved {Count} AD users", ADUsers.Count);
                return ADUsers;
            }
            catch (ActiveDirectoryObjectNotFoundException adEx)
            {
                GlobalVars.Loggi.Warning(adEx, "UserListHelper: Active Directory not available");
                return ADUsers;
            }
            catch (PrincipalServerDownException psEx)
            {
                GlobalVars.Loggi.Warning(psEx, "UserListHelper: Domain controller is not available");
                return ADUsers;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "UserListHelper: Error retrieving AD users");
                return ADUsers;
            }
        }

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

                GlobalVars.Loggi.Debug("UserListHelper: Successfully retrieved {Count} total users ({LocalCount} local, {ADCount} AD)",
                    allUsers.Count, localUsers.Count, adUsers.Count);
                return allUsers;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "UserListHelper: Error retrieving all users");
                return allUsers;
            }
        }

    }
}
