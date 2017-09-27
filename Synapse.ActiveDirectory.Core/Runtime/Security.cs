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
        // Add Access Rule - Adds Rule For The Given Principal
        public static void AddAccessRule(Principal target, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                AddAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"AddAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void AddAccessRule(Principal target, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                AddAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"AddAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void AddAccessRule(DirectoryEntry de, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            AddAccessRule( de, principal, rights, type );
        }

        public static void AddAccessRule(DirectoryEntry de, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            ActiveDirectoryAccessRule newRule = new ActiveDirectoryAccessRule( principal.Sid, rights, type );

            de.ObjectSecurity.AddAccessRule( newRule );
            de.CommitChanges();
        }

        // Delete Access Rule - Deletes Rule For The Principal
        public static void DeleteAccessRule(Principal target, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                DeleteAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"DeleteAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void DeleteAccessRule(Principal target, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                DeleteAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"DeleteAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void DeleteAccessRule(DirectoryEntry de, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            DeleteAccessRule( de, principal, rights, type );
        }

        public static void DeleteAccessRule(DirectoryEntry de, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            ActiveDirectoryAccessRule rule = new ActiveDirectoryAccessRule( principal.Sid, rights, type );

            de.ObjectSecurity.RemoveAccessRule( rule );
            de.CommitChanges();
        }

        // Set Access Rights - Removes Any Existing Rules For The Principal And Sets Rules To Rights Passed In
        public static void SetAccessRule(Principal target, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                SetAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"SetAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void SetAccessRule(Principal target, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                SetAccessRule( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"SetAccessRule Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void SetAccessRule(DirectoryEntry de, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            SetAccessRule( de, principal, rights, type );
        }

        public static void SetAccessRule(DirectoryEntry de, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            ActiveDirectoryAccessRule rule = new ActiveDirectoryAccessRule( principal.Sid, rights, type );

            de.ObjectSecurity.SetAccessRule( rule );
            de.CommitChanges();
        }

        // Purge Access Rights - Removes All Rights For A Given Principal
        public static void PurgeAccessRules(Principal target, Principal principal)
        {
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                PurgeAccessRules( (DirectoryEntry)target.GetUnderlyingObject(), principal );
            else
                throw new AdException( $"PurgeAccessRules Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void PurgeAccessRules(Principal target, String identity)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                PurgeAccessRules( (DirectoryEntry)target.GetUnderlyingObject(), principal );
            else
                throw new AdException( $"PurgeAccessRules Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void PurgeAccessRules(DirectoryEntry de, String identity)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            PurgeAccessRules( de, principal );
        }

        public static void PurgeAccessRules(DirectoryEntry de, Principal principal)
        {
            de.ObjectSecurity.PurgeAccessRules( principal.Sid );
            de.CommitChanges();
        }


    }
}