using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;


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

        public static void CreateGroup(string ouPath, string name)
        {
            if (!DirectoryEntry.Exists("LDAP://CN=" + name + "," + ouPath))
            {
                try
                {
                    DirectoryEntry entry = new DirectoryEntry("LDAP://" + ouPath);
                    DirectoryEntry group = entry.Children.Add("CN=" + name, "group");
                    // By default if no GroupType property is set, the group is created as a domain security group.
                    group.Properties["sAmAccountName"].Value = name;
                    group.CommitChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
            }
            else { Console.WriteLine(ouPath + " already exists"); }
        }

        public static void DeleteGroup(string ouPath, string groupPath)
        {
            if (DirectoryEntry.Exists("LDAP://" + groupPath))
            {
                try
                {
                    DirectoryEntry entry = new DirectoryEntry("LDAP://" + ouPath);
                    DirectoryEntry group = new DirectoryEntry("LDAP://" + groupPath);
                    entry.Children.Remove(group);
                    group.CommitChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
            }
            else
            {
                Console.WriteLine(ouPath + " doesn't exist");
            }
        }
    }
}