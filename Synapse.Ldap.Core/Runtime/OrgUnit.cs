using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;


namespace Synapse.Ldap.Core
{
    public partial class DirectoryServices
    {
        public static OrganizationalUnitObject GetOrganizationalUnit(string name, string ldapRoot)
        {
            using (DirectoryEntry root = new DirectoryEntry(ldapRoot))
            using (DirectorySearcher searcher = new DirectorySearcher(root))
            {
                searcher.Filter = $"(&(objectClass=organizationalUnit))"; //(name={name})
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PropertiesToLoad.Add("name");
                searcher.PropertiesToLoad.Add("distinguishedName");
                searcher.ReferralChasing = ReferralChasingOption.All;

                DirectoryEntry ou = null;
                SearchResultCollection results = searcher.FindAll();
                foreach (SearchResult result in results)
                    if (result.Properties["name"][0].ToString().Equals(name, StringComparison.OrdinalIgnoreCase))
                        ou = result.GetDirectoryEntry();

                if (ou == null)
                    return null;
                else
                    return new OrganizationalUnitObject(ou);
            }
        }

        public static OrganizationalUnitObject CreateOrganizationUnit(string parentOrgUnitDistName, string newOrgUnitName)
        {
            OrganizationalUnitObject newOrgUnitObj = null;

            if (String.IsNullOrEmpty(newOrgUnitName) || String.IsNullOrWhiteSpace(newOrgUnitName))
            {
                throw new Exception("No name is specified for the new organization unit.");
            }

            parentOrgUnitDistName = String.IsNullOrEmpty(parentOrgUnitDistName) || String.IsNullOrWhiteSpace(parentOrgUnitDistName) ?
                GetDomainDistinguishedName() : parentOrgUnitDistName.Replace("LDAP://", "");

            DirectoryEntry parentOrgUnit = new DirectoryEntry(
                $"LDAP://{parentOrgUnitDistName}",
                null, // Username
                null, // Password
                AuthenticationTypes.Secure);

            try
            {
                // Bind to the native AdsObject to force authentication.
                object obj = parentOrgUnit.NativeObject; //not IDisposable
            }
            catch (Exception ex)
            {
                throw new Exception($"Encountered exception while trying to authenticate against domain controller: {ex.Message}");
            }

            if (DirectoryEntry.Exists(parentOrgUnit.Path))
            {
                DirectoryEntry newOrgUnit = new DirectoryEntry(
                    $"LDAP://OU={newOrgUnitName},{parentOrgUnitDistName}",
                    null, // Username
                    null, // Password
                    AuthenticationTypes.Secure);

                if (DirectoryEntry.Exists(newOrgUnit.Path))
                {
                    throw new Exception($"New organization unit '{newOrgUnit.Path}' already exists.");
                }
                newOrgUnit = parentOrgUnit.Children.Add($"OU={newOrgUnitName}", "OrganizationalUnit");
                newOrgUnit.Properties["Description"].Value = "Created by Synapse Ldap Handler";
                newOrgUnit.CommitChanges();
                // TODO: Need to check with Steve on the recursive parent lookup of SetPropertiesFromDirectoryEntry()
                // in synapse.ldap.core\classes\directoryentry.cs.
                newOrgUnitObj = new OrganizationalUnitObject(null);
            }
            else
            {
                throw new Exception($"Parent organization unit {parentOrgUnit.Path} doesn't exist.");
            }

            return newOrgUnitObj;
        }

        public static bool DeleteOrganizationUnit(string orgUnitDistName)
        {
            // Exact distinguished name of the organization unit is expected.
            if (String.IsNullOrEmpty(orgUnitDistName) || String.IsNullOrWhiteSpace(orgUnitDistName))
            {
                Console.WriteLine("No organization unit is specified for deletion.");
                return false;
            }

            orgUnitDistName = orgUnitDistName.Replace("LDAP://", "");

            DirectoryEntry orgUnitForDeletion = new DirectoryEntry(
                $"LDAP://{orgUnitDistName}",
                null, // Username
                null, // Password
                AuthenticationTypes.Secure);

            try
            {
                if (DirectoryEntry.Exists(orgUnitForDeletion.Path))
                {
                    orgUnitForDeletion.DeleteTree();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Encountered exception while deleting organization unit: {ex.Message}");
                return false;
            }

            return true;
        }

        public static string GetDomainDistinguishedName()
        {
            // connect to "RootDSE" to find default naming context.
            // "RootDSE" is not a container.
            DirectoryEntry rootDSE = new DirectoryEntry("LDAP://RootDSE");

            // Return the distinguished name for the domain of which this directory server is a member.
            return rootDSE.Properties["defaultNamingContext"][0].ToString();
        }

        public static string FriendlyDomainToLdapDomain(string friendlyDomainName)
        {
            string ldapPath = null;
            try
            {
                DirectoryContext objContext = new DirectoryContext(
                    DirectoryContextType.Domain, friendlyDomainName);
                Domain objDomain = Domain.GetDomain(objContext);
                ldapPath = objDomain.Name;
            }
            catch (DirectoryServicesCOMException e)
            {
                ldapPath = e.Message.ToString();
            }
            return ldapPath;
        }

        public static void DirectoryEntryConfigurationSettings(string domainADsPath)
        {
            // Result may look like below:
            // Server: XXXXXX.XXX.XXX
            // Page Size: 99
            // Password Encoding: PasswordEncodingSsl
            // Password Port: 636
            // Referral: External
            // Security Masks: Owner, Group, Dacl
            // Is Mutually Authenticated: True
            // Bind to current domain
            DirectoryEntry entry = new DirectoryEntry(domainADsPath);
            DirectoryEntryConfiguration entryConfiguration = entry.Options;

            Console.WriteLine("Server: " + entryConfiguration.GetCurrentServerName());
            Console.WriteLine("Page Size: " + entryConfiguration.PageSize.ToString());
            Console.WriteLine("Password Encoding: " +
                entryConfiguration.PasswordEncoding.ToString());
            Console.WriteLine("Password Port: " +
                entryConfiguration.PasswordPort.ToString());
            Console.WriteLine("Referral: " + entryConfiguration.Referral.ToString());
            Console.WriteLine("Security Masks: " +
                entryConfiguration.SecurityMasks.ToString());
            Console.WriteLine("Is Mutually Authenticated: " +
                entryConfiguration.IsMutuallyAuthenticated().ToString());
            Console.WriteLine();
            Console.Read();
        }

        public static List<string> EnumerateDomainControllers()
        {
            List<string> alDcs = new List<string>();
            Domain domain = Domain.GetCurrentDomain();
            foreach (DomainController dc in domain.DomainControllers)
            {
                alDcs.Add(dc.Name);
            }
            return alDcs;
        }

        public static List<string> EnumerateOUMembers(string OrgUnitDistName)
        {
            // The parameter OrgUnitDistName is the Organizational Unit distinguishedName
            // such as OU=Users,dc=myDomain,dc=com
            List<string> alObjects = new List<string>();
            try
            {
                DirectoryEntry directoryObject = new DirectoryEntry("LDAP://" + OrgUnitDistName);
                foreach (DirectoryEntry child in directoryObject.Children)
                {
                    string childPath = child.Path.ToString();
                    alObjects.Add(childPath.Remove(0, 7));
                    //remove the LDAP prefix from the path

                    child.Close();
                    child.Dispose();
                }
                directoryObject.Close();
                directoryObject.Dispose();
            }
            catch (DirectoryServicesCOMException e)
            {
                Console.WriteLine("An Error Occurred: " + e.Message.ToString());
            }
            return alObjects;
        }
    }
}