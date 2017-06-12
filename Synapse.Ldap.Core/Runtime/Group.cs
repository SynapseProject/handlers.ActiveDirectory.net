using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;


namespace Synapse.Ldap.Core
{
    public partial class DirectoryServices
    {
        public static GroupPrincipalObject GetGroup(string sAMAccountName, bool getGroups)
        {
            GroupPrincipalObject g = null;
            using( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity( context, IdentityType.SamAccountName, sAMAccountName );
                g = new GroupPrincipalObject( group );
                if( getGroups )
                    g.GetGroups();
            }
            return g;
        }

        public static void CreateGroup(string ouPath, string groupName, bool dryRun = false)
        {
            if (String.IsNullOrWhiteSpace(ouPath))
            {
                throw new Exception("OU path is not specified.");
            }
            ouPath = ouPath.Contains("LDAP://") ? ouPath : $"LDAP://{ouPath}";


            if (String.IsNullOrWhiteSpace(groupName))
            {
                throw new Exception("Group name is not specified.");
            }

            string groupPath = $"LDAP://CN={groupName},{ouPath.Replace("LDAP://", "")}";

            try
            {
                if (!DirectoryEntry.Exists(groupPath))
                {
                    if (!dryRun)
                    {
                        DirectoryEntry entry = new DirectoryEntry(ouPath);
                        DirectoryEntry group = entry.Children.Add("CN=" + groupName, "group");
                        // By default if no GroupType property is set, the group is created as a domain security group.
                        group.Properties["sAmAccountName"].Value = groupName;
                        group.CommitChanges();
                    }
                }
                else
                {
                    throw new Exception(groupPath + " already exists.");
                }
            }
            catch (COMException e)
            {
                if (e.Message.Contains("Unknown error "))
                {
                    throw new Exception("OU path is not valid.");
                }
            }
        }

        public static void DeleteGroup(string groupName, bool dryRun = false)
        {
            if (String.IsNullOrWhiteSpace(groupName))
            {
                throw new Exception("Group name is not specified.");
            }

            try
            {
                using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain))
                {
                    GroupPrincipal groupPrincipal = new GroupPrincipal(ctx) {Name = groupName};

                    // create your principal searcher passing in the QBE principal    
                    PrincipalSearcher searcher = new PrincipalSearcher(groupPrincipal);

                    Principal foundGroup = searcher.FindOne();
                    if (foundGroup != null)
                    {
                        if (!dryRun)
                        {
                            foundGroup.Delete();
                        }
                    }
                    else
                    {
                        throw new Exception("Group does not exist.");
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                throw new Exception(e.Message);
            }
        }

        public static void DeleteGroupEx(string ouPath, string groupPath, bool dryRun)
        {
            if (String.IsNullOrWhiteSpace(ouPath))
            {
                throw new Exception("OU path is not specified.");
            }

            if (String.IsNullOrWhiteSpace(groupPath))
            {
                throw new Exception("Group path is not specified.");
            }

            ouPath = $"LDAP://{ouPath.Replace("LDAP://", "")}";
            groupPath = $"LDAP://{groupPath.Replace("LDAP://", "")}";

            try
            {
                if (DirectoryEntry.Exists("LDAP://" + groupPath))
                {
                    try
                    {
                        if (!dryRun)
                        {
                            DirectoryEntry entry = new DirectoryEntry(ouPath);
                            DirectoryEntry group = new DirectoryEntry(groupPath);
                            entry.Children.Remove(group);
                            group.CommitChanges();
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("Unknown error "))
                        {
                            throw new Exception("OU path is not valid.");
                        }
                    }
                }
                else
                {
                    throw new Exception(ouPath + " doesn't exist");
                }
            }
            catch (COMException e)
            {
                if (e.Message.Contains("Unknown error "))
                {
                    throw new Exception("OU path is not valid.");
                }
            }
        }

        public static void UpdateGroupAttribute(string groupName, string attribute, string value, string ldapPath = "", bool dryRun = false)
        {
            if (String.IsNullOrWhiteSpace(groupName))
            {
                throw new Exception("No group name is specified.");
            }

            if (!IsValidGroupAttribute(attribute))
            {
                throw new Exception("The attribute specified is not valid.");
            }

            ldapPath = String.IsNullOrWhiteSpace(ldapPath) ? $"LDAP://{GetDomainDistinguishedName()}" : $"LDAP://{ldapPath.Replace("LDAP://", "")}";

            using (DirectoryEntry entry = new DirectoryEntry(ldapPath))
            {
                using (DirectorySearcher mySearcher = new DirectorySearcher(entry) { Filter = "(sAMAccountName=" + groupName + ")" })
                {
                    try
                    {
                        mySearcher.PropertiesToLoad.Add("" + attribute + "");
                        SearchResult result = mySearcher.FindOne();
                        if (result != null)
                        {
                            if (!dryRun)
                            {
                                DirectoryEntry entryToUpdate = result.GetDirectoryEntry();

                                if (result.Properties.Contains("" + attribute + ""))
                                {
                                    if (!(String.IsNullOrEmpty(value)))
                                    {
                                        entryToUpdate.Properties["" + attribute + ""].Value = value;
                                    }
                                    else
                                    {
                                        entryToUpdate.Properties["" + attribute + ""].Clear();
                                    }
                                }
                                else
                                {
                                    entryToUpdate.Properties["" + attribute + ""].Add(value);
                                }
                                entryToUpdate.CommitChanges();
                            }
                        }
                        else
                        {
                            throw new Exception("Group cannot be found.");
                        }
                    }
                    catch (DirectoryServicesCOMException ex)
                    {
                        if (ex.Message.Contains("The attribute syntax specified to the directory service is invalid."))
                        {
                            throw new Exception("The attribute value is invalid.");
                        }
                        if (ex.Message.Contains("A constraint violation occurred."))
                        {
                            throw new Exception("The attribute value is invalid.");
                        }
                        throw;
                    }
                    catch (COMException ex)
                    {
                        if (ex.Message.Contains("The server is not operational."))
                        {
                            throw new Exception("LDAP path specifieid is not valid.");
                        }
                        throw;
                    }
                }
            };
        }

        public static bool IsValidGroupAttribute(string attribute)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>()
            {
                { "description", "Description" },
                { "mail", "E-mail" },
                { "managedBy", "Managed By" }
            };

            return attributes.ContainsKey(attribute);
        }
    }
}