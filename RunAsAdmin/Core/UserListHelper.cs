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

    }
}
