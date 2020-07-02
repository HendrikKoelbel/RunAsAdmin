using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;

namespace RunAsAdmin.Core
{
    public static class DomainHelper
    {

        /// <summary>
        /// Is a function which collects all domains or local PC names.
        /// </summary>
        /// <returns>A list with the local PC name and all connected domains.</returns>
        public static List<string> GetAllDomains()
        {
            var domainList = new List<string>();
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

        /// <summary>
        /// Is a function which collects all domains.
        /// </summary>
        /// <returns>A list with all connected domains.</returns>
        public static List<string> GetDomains()
        {
            var domainList = new List<string>();
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
    }
}
