using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Security.AccessControl;

namespace Synapse.ActiveDirectory.Core
{
    public partial class DirectoryServices
    {
        // Get Access Rules - Retrieves AccessRules Associated With A DirectoryEntry
        public static List<AccessRuleObject> GetAccessRules(Principal principal)
        {
            if ( principal == null )
                throw new AdException( $"Principal Can Not Be NULL", AdStatusType.MissingInput );
            else if ( principal.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                return GetAccessRules( (DirectoryEntry)principal.GetUnderlyingObject() );
            else
                throw new AdException( $"GetAccessRules Not Available For Object Type [{principal.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static List<AccessRuleObject> GetAccessRules(DirectoryEntry de)
        {
            if ( de == null )
                throw new AdException( $"DirectoryEntry Can Not Be NULL", AdStatusType.MissingInput );

            List<AccessRuleObject> accessRules = new List<AccessRuleObject>();
            AuthorizationRuleCollection rules = de?.ObjectSecurity?.GetAccessRules( true, true, typeof( System.Security.Principal.SecurityIdentifier ) );
            if ( rules != null )
            {
                foreach ( AuthorizationRule rule in rules )
                {
                    ActiveDirectoryAccessRule accessRule = (ActiveDirectoryAccessRule)rule;
                    AccessRuleObject aro = new AccessRuleObject()
                    {
                        ControlType = accessRule.AccessControlType,
                        Rights = accessRule.ActiveDirectoryRights,
                        IdentityReference = accessRule.IdentityReference.Value,
                        InheritanceFlags = accessRule.InheritanceFlags,
                        IsInherited = accessRule.IsInherited,
                    };

                    String identity = aro.IdentityReference;

                    if (DirectoryServices.IsSid(aro.IdentityReference))
                    {
                        // Get User-Readable Principal Name from Sid
                        System.Security.Principal.SecurityIdentifier sid = (System.Security.Principal.SecurityIdentifier)rule.IdentityReference;
                        System.Security.Principal.NTAccount acct = (System.Security.Principal.NTAccount)sid.Translate(typeof(System.Security.Principal.NTAccount));
                        identity = acct.Value;
                    }

                    aro.IdentityName = identity;
                    accessRules.Add( aro );

                }
            }

            return accessRules;
        }

        // Add Access Rule - Adds Rule For The Given Principal
        public static void AddAccessRule(Principal target, Principal principal, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            if ( target == null )
                throw new AdException( $"Target Pricinpal Can Not Be NULL", AdStatusType.MissingInput );
            else if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                AddAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type, inherit );
            else
                throw new AdException( $"AddAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void AddAccessRule(String target, String identity, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            DirectoryEntry de = GetDirectoryEntry( target );
            if ( de == null )
                throw new AdException( $"Target [{target}] Can Not Be Found.", AdStatusType.DoesNotExist );
            AddAccessRule( de, identity, rights, type, inherit );
        }

        public static void AddAccessRule(Principal target, String identity, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            string id = null;
            string domain = GetDomain(identity, out id);
            Principal principal = DirectoryServices.GetPrincipal( id, domain );
            if ( principal == null )
                throw new AdException( $"Principal [{identity}] Can Not Be Found.", AdStatusType.DoesNotExist );
            else if ( target == null )
                throw new AdException( $"Target Pricinpal Can Not Be NULL", AdStatusType.MissingInput );
            else if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                AddAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type, inherit );
            else
                throw new AdException( $"AddAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void AddAccessRule(DirectoryEntry de, String identity, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            string id = null;
            string domain = GetDomain(identity, out id);
            Principal principal = DirectoryServices.GetPrincipal( id, domain );
            if ( principal == null )
                throw new AdException( $"Principal [{identity}] Can Not Be Found.", AdStatusType.DoesNotExist );
            AddAccessRule( de, principal, rights, type );
        }

        public static void AddAccessRule(DirectoryEntry de, Principal principal, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            if ( principal == null )
                throw new AdException( $"Principal Can Not Be NULL", AdStatusType.MissingInput );

            ActiveDirectoryAccessRule newRule = new ActiveDirectoryAccessRule( principal.Sid, rights, type, inherit );

            if ( de != null )
            {
                de.ObjectSecurity.AddAccessRule(newRule);
                de.CommitChanges();
            }
            else
                throw new AdException( $"Target DirectoryEntry Can Not Be NULL", AdStatusType.MissingInput );
        }

        // Delete Access Rule - Deletes Rule For The Principal
        public static void DeleteAccessRule(Principal target, Principal principal, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            if ( target == null )
                throw new AdException( $"Target Principal Can Not Be NULL", AdStatusType.MissingInput );
            else if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                DeleteAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type, inherit );
            else
                throw new AdException( $"DeleteAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void DeleteAccessRule(String target, String identity, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            DirectoryEntry de = GetDirectoryEntry( target );
            if ( de == null )
                throw new AdException( $"Target [{target}] Can Not Be Found.", AdStatusType.DoesNotExist );
            DeleteAccessRule( de, identity, rights, type, inherit);
        }

        public static void DeleteAccessRule(Principal target, String identity, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( principal == null )
                throw new AdException( $"Principal [{identity}] Can Not Be Found.", AdStatusType.DoesNotExist );
            else if ( target == null )
                throw new AdException( $"Target Principal Can Not Be NULL", AdStatusType.MissingInput );
            else if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                DeleteAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type, inherit );
            else
                throw new AdException( $"DeleteAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void DeleteAccessRule(DirectoryEntry de, String identity, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( principal == null )
                throw new AdException( $"Principal [{identity}] Can Not Be Found.", AdStatusType.DoesNotExist );
            DeleteAccessRule( de, principal, rights, type, inherit );
        }

        public static void DeleteAccessRule(DirectoryEntry de, Principal principal, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            if ( principal == null )
                throw new AdException( "Principal Can Not Be NULL", AdStatusType.MissingInput );

            ActiveDirectoryAccessRule rule = new ActiveDirectoryAccessRule( principal.Sid, rights, type, inherit );

            if ( de != null )
            {
                de.ObjectSecurity.RemoveAccessRule( rule );
                de.CommitChanges();
            }
            else
                throw new AdException( "Directory Entry Can Not Be NULL", AdStatusType.MissingInput );
        }

        // Set Access Rights - Removes Any Existing Rules For The Principal And Sets Rules To Rights Passed In
        public static void SetAccessRule(Principal target, Principal principal, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            if ( target == null )
                throw new AdException( $"Target Principal Can Not Be NULL", AdStatusType.MissingInput );
            else if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                SetAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type, inherit );
            else
                throw new AdException( $"SetAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void SetAccessRule(Principal target, String identity, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( principal == null )
                throw new AdException( $"Principal [{identity}] Can Not Be Found.", AdStatusType.DoesNotExist );
            else if ( target == null )
                throw new AdException( $"Target Principal Can Not Be NULL", AdStatusType.MissingInput );
            else if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                SetAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type, inherit );
            else
                throw new AdException( $"SetAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void SetAccessRule(String target, String identity, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            DirectoryEntry de = GetDirectoryEntry( target );
            if ( de == null )
                throw new AdException( $"Target [{target}] Can Not Be Found.", AdStatusType.DoesNotExist );
            SetAccessRule( de, identity, rights, type, inherit );
        }

        public static void SetAccessRule(DirectoryEntry de, String identity, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( principal == null )
                throw new AdException( $"Principal [{identity}] Can Not Be Found.", AdStatusType.DoesNotExist );
            SetAccessRule( de, principal, rights, type, inherit );
        }

        public static void SetAccessRule(DirectoryEntry de, Principal principal, ActiveDirectoryRights rights, AccessControlType type, ActiveDirectorySecurityInheritance inherit = ActiveDirectorySecurityInheritance.None)
        {
            if ( principal == null )
                throw new AdException( "Principal Can Not Be NULL", AdStatusType.MissingInput );

            ActiveDirectoryAccessRule rule = new ActiveDirectoryAccessRule( principal.Sid, rights, type, inherit );

            if ( de != null )
            {
                de.ObjectSecurity.SetAccessRule( rule );
                de.CommitChanges();
            }
            else
                throw new AdException( "DirectoryEntry Can Not Be NULL", AdStatusType.MissingInput );
        }

        // Purge Access Rights - Removes All Rights For A Given Principal
        public static void PurgeAccessRules(string target, string principal)
        {
            DirectoryEntry de = GetDirectoryEntry( target );
            if ( de == null )
                throw new AdException( $"Target [{target}] Can Not Be Found.", AdStatusType.DoesNotExist );
            PurgeAccessRules( de, principal );
        }

        public static void PurgeAccessRules(Principal target, Principal principal)
        {
            if ( target == null )
                throw new AdException( $"Target Principal Can Not Be NULL", AdStatusType.MissingInput );
            else if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                PurgeAccessRules( (DirectoryEntry)target.GetUnderlyingObject(), principal );
            else
                throw new AdException( $"PurgeAccessRules Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void PurgeAccessRules(Principal target, String identity)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( principal == null )
                throw new AdException( $"Principal [{identity}] Can Not Be Found.", AdStatusType.DoesNotExist );
            else if ( target == null )
                throw new AdException( $"Target Principal Can Not Be NULL", AdStatusType.MissingInput );
            else if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                PurgeAccessRules( (DirectoryEntry)target.GetUnderlyingObject(), principal );
            else
                throw new AdException( $"PurgeAccessRules Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void PurgeAccessRules(DirectoryEntry de, String identity)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( principal == null )
                throw new AdException( $"Principal [{identity}] Can Not Be Found.", AdStatusType.DoesNotExist );
            PurgeAccessRules( de, principal );
        }

        public static void PurgeAccessRules(DirectoryEntry de, Principal principal)
        {
            if ( principal == null )
                throw new AdException( "Principal Can Not Be NULL", AdStatusType.MissingInput );
            else if ( de != null )
            {
                de.ObjectSecurity.PurgeAccessRules( principal.Sid );
                de.CommitChanges();
            }
            else
                throw new AdException( "DirectoryEntry Can Not Be NULL", AdStatusType.MissingInput );
        }

    }
}