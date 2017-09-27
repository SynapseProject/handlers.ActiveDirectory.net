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
        // Add Access Rights
        public static void AddAccessRights(Principal target, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                AddAccessRights( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"AddAccessRights Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void AddAccessRights(Principal target, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                AddAccessRights( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"AddAccessRights Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void AddAccessRights(DirectoryEntry de, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            AddAccessRights( de, principal, rights, type );
        }

        public static void AddAccessRights(DirectoryEntry de, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            ActiveDirectoryAccessRule newRule = new ActiveDirectoryAccessRule( principal.Sid, rights, type );

            de.ObjectSecurity.AddAccessRule( newRule );
            de.CommitChanges();
        }

        // Delete Access Rights
        public static void DeleteAccessRights(Principal target, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                DeleteAccessRights( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"DeleteAccessRights Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void DeleteAccessRights(Principal target, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                DeleteAccessRights( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"DeleteAccessRights Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void DeleteAccessRights(DirectoryEntry de, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            DeleteAccessRights( de, principal, rights, type );
        }

        public static void DeleteAccessRights(DirectoryEntry de, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            ActiveDirectoryAccessRule rule = new ActiveDirectoryAccessRule( principal.Sid, rights, type );

            de.ObjectSecurity.RemoveAccessRule( rule );
            de.CommitChanges();
        }

        // Set Access Rights
        public static void SetAccessRights(Principal target, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                SetAccessRights( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"SetAccessRights Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void SetAccessRights(Principal target, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                SetAccessRights( (DirectoryEntry)target.GetUnderlyingObject(), principal, rights, type );
            else
                throw new AdException( $"SetAccessRights Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void SetAccessRights(DirectoryEntry de, String identity, ActiveDirectoryRights rights, AccessControlType type)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            SetAccessRights( de, principal, rights, type );
        }

        public static void SetAccessRights(DirectoryEntry de, Principal principal, ActiveDirectoryRights rights, AccessControlType type)
        {
            ActiveDirectoryAccessRule rule = new ActiveDirectoryAccessRule( principal.Sid, rights, type );

            de.ObjectSecurity.SetAccessRule( rule );
            de.CommitChanges();
        }

        // Purge Access Rights - Removes All Rights For A Given Principal
        public static void PurgeAccessRights(Principal target, Principal principal)
        {
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                PurgeAccessRights( (DirectoryEntry)target.GetUnderlyingObject(), principal );
            else
                throw new AdException( $"PurgeAccessRights Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void PurgeAccessRights(Principal target, String identity)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            if ( target.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                PurgeAccessRights( (DirectoryEntry)target.GetUnderlyingObject(), principal );
            else
                throw new AdException( $"PurgeAccessRights Not Available For Object Type [{target.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static void PurgeAccessRights(DirectoryEntry de, String identity)
        {
            Principal principal = DirectoryServices.GetPrincipal( identity );
            PurgeAccessRights( de, principal );
        }

        public static void PurgeAccessRights(DirectoryEntry de, Principal principal)
        {
            de.ObjectSecurity.PurgeAccessRules( principal.Sid );
            de.CommitChanges();
        }


    }
}