using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;

using Synapse.Core;
using Synapse.ActiveDirectory.Core;
using Synapse.Handlers.ActiveDirectory;

public class ActiveDirectoryHandler : HandlerRuntimeBase
{
    ActiveDirectoryHandlerConfig config = null;
    ActiveDirectoryHandlerResults results = new ActiveDirectoryHandlerResults();
    bool isDryRun = false;

    public override IHandlerRuntime Initialize(string config)
    {
        //deserialize the Config from the Handler declaration
        this.config = DeserializeOrNew<ActiveDirectoryHandlerConfig>( config );
        return this;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        int cheapSequence = 0;
        const string __context = "Execute";
        ExecuteResult result = new ExecuteResult()
        {
            Status = StatusType.Complete,
            Sequence = int.MaxValue
        };
        string msg = "Complete";
        Exception exc = null;

        isDryRun = startInfo.IsDryRun;

        //deserialize the Parameters from the Action declaration
        Synapse.Handlers.ActiveDirectory.ActiveDirectoryHandlerParameters parameters = base.DeserializeOrNew<Synapse.Handlers.ActiveDirectory.ActiveDirectoryHandlerParameters>( startInfo.Parameters );

        OnLogMessage( "Execute", $"Running Handler As User [{System.Security.Principal.WindowsIdentity.GetCurrent().Name}]" );

        try
        {
            //TODO : if IsDryRun == true, test if ConnectionString is valid and works.
            if( startInfo.IsDryRun )
            {
                OnProgress( __context, "Attempting connection", sequence: cheapSequence++ );


                result.ExitData = "Success";
                result.Message = msg =
                    $"Connection test successful!";
            }
            //else, select data as declared in Parameters.QueryString
            else
            {
                switch( config.Action )
                {
                    case ActionType.Query:
                        ProcessActiveDirectoryObjects( parameters.Users, ProcessQuery );
                        ProcessActiveDirectoryObjects( parameters.Groups, ProcessQuery );
                        ProcessActiveDirectoryObjects( parameters.OrganizationalUnits, ProcessQuery );
                        break;
                    case ActionType.Create:
                        ProcessActiveDirectoryObjects( parameters.OrganizationalUnits, ProcessCreate );
                        ProcessActiveDirectoryObjects( parameters.Groups, ProcessCreate );
                        ProcessActiveDirectoryObjects( parameters.Users, ProcessCreate );
                        break;
                    case ActionType.Modify:
                        ProcessActiveDirectoryObjects( parameters.OrganizationalUnits, ProcessModify );
                        ProcessActiveDirectoryObjects( parameters.Groups, ProcessModify );
                        ProcessActiveDirectoryObjects( parameters.Users, ProcessModify );
                        break;
                    case ActionType.Delete:
                        ProcessActiveDirectoryObjects( parameters.Users, ProcessDelete );
                        ProcessActiveDirectoryObjects( parameters.Groups, ProcessDelete );
                        ProcessActiveDirectoryObjects( parameters.OrganizationalUnits, ProcessDelete );
                        break;
                    case ActionType.AddToGroup:
                        ProcessActiveDirectoryObjects( parameters.Users, ProcessGroupAdd );
                        ProcessActiveDirectoryObjects( parameters.Groups, ProcessGroupAdd );
                        break;
                    case ActionType.RemoveFromGroup:
                        ProcessActiveDirectoryObjects( parameters.Users, ProcessGroupRemove );
                        ProcessActiveDirectoryObjects( parameters.Groups, ProcessGroupRemove );
                        break;
                    case ActionType.AddAccessRule:
                        ProcessActiveDirectoryObjects( parameters.Users, ProcessAccessRules );
                        ProcessActiveDirectoryObjects( parameters.Groups, ProcessAccessRules );
                        ProcessActiveDirectoryObjects( parameters.OrganizationalUnits, ProcessAccessRules );
                        break;
                    case ActionType.RemoveAccessRule:
                        ProcessActiveDirectoryObjects( parameters.Users, ProcessAccessRules );
                        ProcessActiveDirectoryObjects( parameters.Groups, ProcessAccessRules );
                        ProcessActiveDirectoryObjects( parameters.OrganizationalUnits, ProcessAccessRules );
                        break;
                    case ActionType.SetAccessRule:
                        ProcessActiveDirectoryObjects( parameters.Users, ProcessAccessRules );
                        ProcessActiveDirectoryObjects( parameters.Groups, ProcessAccessRules );
                        ProcessActiveDirectoryObjects( parameters.OrganizationalUnits, ProcessAccessRules );
                        break;
                    case ActionType.PurgeAccessRules:
                        ProcessActiveDirectoryObjects( parameters.Users, ProcessAccessRules );
                        ProcessActiveDirectoryObjects( parameters.Groups, ProcessAccessRules );
                        ProcessActiveDirectoryObjects( parameters.OrganizationalUnits, ProcessAccessRules );
                        break;
                    default:
                        throw new AdException( $"Unknown Action {config.Action} Specified", AdStatusType.NotSupported );
                }
            }
        }
        //something wnet wrong: hand-back the Exception and mark the execution as Failed
        catch ( Exception ex )
        {
            exc = ex;
            result.Status = StatusType.Failed;
            result.ExitData = msg =
                ex.Message + " | " + ex.InnerException?.Message;
        }

        if (string.IsNullOrWhiteSpace(result.ExitData?.ToString()))
            result.ExitData = results.Serialize( config.OutputType, config.PrettyPrint );

        if (!config.SuppressOutput)
            OnProgress( __context, result.ExitData?.ToString(), result.Status, sequence: cheapSequence++, ex: exc );

        //final runtime notification, return sequence=Int32.MaxValue by convention to supercede any other status message
        OnProgress( __context, msg, result.Status, sequence: int.MaxValue, ex: exc );

        return result;
    }

    // TODO : Implement Me
    public override object GetConfigInstance()
    {
        throw new NotImplementedException();
    }

    // TODO : Implement Me
    public override object GetParametersInstance()
    {
        throw new NotImplementedException();
    }

    private void ProcessActiveDirectoryObjects(IEnumerable<AdObject> objs, Action<AdObject, bool> processFunction)
    {
        if ( objs != null )
        {
            if ( config.RunSequential )
            {
                foreach ( AdObject obj in objs )
                    processFunction( obj, config.ReturnObjects );
            }
            else
                Parallel.ForEach( objs, obj =>
                {
                    processFunction( obj, config.ReturnObjects );
                } );
        }
    }

    private void ProcessQuery(AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            Type = obj.Type,
            Identity = obj.Identity
        };

        DoQuery( result, obj, returnObject );
        results.Add( result );
    }

    private void DoQuery(ActiveDirectoryObjectResult result, AdObject obj, bool returnObject = true, bool returnStatus = true)
    {
        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            Action = config.Action,
            Status = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            object adObject = GetActiveDirectoryObject( obj );
            switch ( obj.Type )
            {
                case AdObjectType.User:
                    if ( returnObject )
                        result.User = (UserPrincipalObject)adObject;
                    break;
                case AdObjectType.Group:
                    if ( returnObject )
                        result.Group =  (GroupPrincipalObject)adObject;
                    break;
                case AdObjectType.OrganizationalUnit:
                    if ( returnObject )
                        result.OrganizationalUnit = (OrganizationalUnitObject)adObject;
                    break;
                default:
                    throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
            }

            if ( returnStatus )
                result.Statuses.Add( status );

        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessQuery", e.Message );
            OnLogMessage( "ProcessQuery", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.Action, obj );
        }
    }

    private object GetActiveDirectoryObject(AdObject obj)
    {
        switch ( obj.Type )
        {
            case AdObjectType.User:
                AdUser user = (AdUser)obj;
                UserPrincipalObject upo = null;
                upo = DirectoryServices.GetUser( user.Identity, config.QueryGroupMembership, config.ReturnAccessRules );
                return upo;
            case AdObjectType.Group:
                AdGroup group = (AdGroup)obj;
                GroupPrincipalObject gpo = null;
                gpo = DirectoryServices.GetGroup( group.Identity, config.QueryGroupMembership, config.ReturnAccessRules );
                return gpo;
            case AdObjectType.OrganizationalUnit:
                AdOrganizationalUnit ou = (AdOrganizationalUnit)obj;
                OrganizationalUnitObject ouo = null;
                ouo = DirectoryServices.GetOrganizationalUnit( ou.Identity, config.ReturnAccessRules );
                return ouo;
            default:
                throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
        }
    }

    private void ProcessCreate(AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            Type = obj.Type,
            Identity = obj.Identity
        };

        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            Action = config.Action,
            Status = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            object adObject = null;

            switch ( obj.Type )
            {
                case AdObjectType.User:
                    AdUser user = (AdUser)obj;
                    UserPrincipal up = null;
                    if ( config.UseUpsert && DirectoryServices.IsExistingUser( obj.Identity ) )
                    {
                        up = DirectoryServices.GetUserPrincipal( obj.Identity );
                        if ( up == null )
                            throw new AdException( $"User [{obj.Identity}] Not Found.", AdStatusType.DoesNotExist );
                        user.UpdateUserPrincipal( up );
                    }
                    else if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                    {
                        up = user.CreateUserPrincipal();
                    }
                    else
                        throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For User Creation.", AdStatusType.MissingInput );

                    DirectoryServices.SaveUser( up, isDryRun );
                    OnLogMessage( "ProcessCreate", obj.Type + " [" + obj.Identity + "] Created." );
                    result.Statuses.Add( status );
                    if ( user.Groups != null )
                        AddToGroup( result, user, false );

                    if ( returnObject )
                    {
                        adObject = GetActiveDirectoryObject( obj );
                        result.User = (UserPrincipalObject)adObject;
                    }

                    break;
                case AdObjectType.Group:
                    AdGroup group = (AdGroup)obj;
                    GroupPrincipal gp = null;
                    if ( config.UseUpsert && DirectoryServices.IsExistingGroup( obj.Identity ) )
                    {
                        gp = DirectoryServices.GetGroupPrincipal( obj.Identity );
                        if ( gp == null )
                            throw new AdException( $"Group [{obj.Identity}] Not Found.", AdStatusType.DoesNotExist );
                        group.UpdateGroupPrincipal( gp );
                    }
                    else if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                    {
                        gp = group.CreateGroupPrincipal();
                    }
                    else
                        throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For Group Creation.", AdStatusType.MissingInput );

                    DirectoryServices.SaveGroup( gp, isDryRun );
                    OnLogMessage( "ProcessCreate", obj.Type + " [" + obj.Identity + "] Created." );
                    result.Statuses.Add( status );
                    if ( group.Groups != null )
                        AddToGroup( result, group, false );
                    if ( returnObject )
                    {
                        adObject = GetActiveDirectoryObject( obj );
                        result.Group = (GroupPrincipalObject)adObject;
                    }

                    break;
                case AdObjectType.OrganizationalUnit:
                    AdOrganizationalUnit ou = (AdOrganizationalUnit)obj;

                    // Get DistinguishedName from User or Group Identity for ManagedBy Property
                    if ( !String.IsNullOrWhiteSpace(ou.ManagedBy) )
                    {
                        if ( ou.Properties == null )
                            ou.Properties = new Dictionary<string, List<string>>();

                        if ( !ou.Properties.ContainsKey( "managedBy" ) )
                        {
                            String distinguishedName = DirectoryServices.GetDistinguishedName( ou.ManagedBy );
                            if ( distinguishedName == null )
                                distinguishedName = ou.ManagedBy;

                            List<String> values = new List<string>();
                            values.Add( distinguishedName );
                            ou.Properties.Add( "managedBy", values );
                        }
                    }

                    if ( config.UseUpsert && DirectoryServices.IsExistingDirectoryEntry( obj.Identity ) )
                        DirectoryServices.ModifyOrganizationUnit( ou.Identity, ou.Description, ou.Properties, isDryRun );
                    else if (DirectoryServices.IsDistinguishedName(ou.Identity))
                        DirectoryServices.CreateOrganizationUnit( ou.Identity, ou.Description, ou.Properties, isDryRun );
                    else
                        throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For Organizational Unit Creation.", AdStatusType.MissingInput );

                    OnLogMessage( "ProcessCreate", obj.Type + " [" + obj.Identity + "] Created." );
                    result.Statuses.Add( status );
                    if ( returnObject )
                    {
                        adObject = GetActiveDirectoryObject( obj );
                        result.OrganizationalUnit = (OrganizationalUnitObject)adObject;
                    }
                    break;
                default:
                    throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
            }
        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessCreate", e.Message );
            OnLogMessage( "ProcessCreate", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.Action, obj );
        }

        results.Add( result );

    }

    private void ProcessModify(AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            Type = obj.Type,
            Identity = obj.Identity
        };

        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            Action = config.Action,
            Status = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            object adObject = null;

            switch ( obj.Type )
            {
                case AdObjectType.User:
                    AdUser user = (AdUser)obj;
                    UserPrincipal up = null;
                    if ( config.UseUpsert && !DirectoryServices.IsExistingUser( obj.Identity ) )
                    {
                        if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                            up = user.CreateUserPrincipal();
                        else
                            throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For User Creation.", AdStatusType.MissingInput );
                    }
                    else
                    {
                        up = DirectoryServices.GetUserPrincipal( obj.Identity );
                        if ( up == null )
                            throw new AdException( $"User [{obj.Identity}] Not Found.", AdStatusType.DoesNotExist );
                        user.UpdateUserPrincipal( up );
                    }

                    DirectoryServices.SaveUser( up, isDryRun );

                    OnLogMessage( "ProcessModify", obj.Type + " [" + obj.Identity + "] Modified." );
                    if ( user.Groups != null )
                        ProcessGroupAdd( user, false );
                    result.Statuses.Add( status );
                    if ( returnObject )
                    {
                        adObject = GetActiveDirectoryObject( obj );
                        result.User = (UserPrincipalObject)adObject;
                    }
                    break;
                case AdObjectType.Group:
                    AdGroup group = (AdGroup)obj;
                    GroupPrincipal gp = null;
                    if ( config.UseUpsert && !DirectoryServices.IsExistingGroup( obj.Identity ) )
                    {
                        if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                            gp = group.CreateGroupPrincipal();
                        else
                            throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For Group Creation.", AdStatusType.MissingInput );
                    }
                    else
                    {
                        gp = DirectoryServices.GetGroupPrincipal( obj.Identity );
                        if ( gp == null )
                            throw new AdException( $"Group [{obj.Identity}] Not Found.", AdStatusType.DoesNotExist );
                        group.UpdateGroupPrincipal( gp );
                    }

                    DirectoryServices.SaveGroup( gp, isDryRun );
                    OnLogMessage( "ProcessModify", obj.Type + " [" + obj.Identity + "] Modified." );
                    if ( group.Groups != null )
                        ProcessGroupAdd( group, false );
                    result.Statuses.Add( status );
                    if ( returnObject )
                    {
                        adObject = GetActiveDirectoryObject( obj );
                        result.Group = (GroupPrincipalObject)adObject;
                    }
                    break;
                case AdObjectType.OrganizationalUnit:
                    AdOrganizationalUnit ou = (AdOrganizationalUnit)obj;

                    // Get DistinguishedName from User or Group Identity for ManagedBy Property
                    if ( !String.IsNullOrWhiteSpace( ou.ManagedBy ) )
                    {
                        if ( ou.Properties == null )
                            ou.Properties = new Dictionary<string, List<string>>();

                        if ( !ou.Properties.ContainsKey( "managedBy" ) )
                        {
                            String distinguishedName = DirectoryServices.GetDistinguishedName( ou.ManagedBy );
                            if ( distinguishedName == null )
                                distinguishedName = ou.ManagedBy;

                            List<String> values = new List<string>();
                            values.Add( distinguishedName );
                            ou.Properties.Add( "managedBy", values );
                        }
                    }

                    if ( config.UseUpsert && !DirectoryServices.IsExistingDirectoryEntry(obj.Identity))
                    {
                        if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                            DirectoryServices.CreateOrganizationUnit( obj.Identity, ou.Description, ou.Properties, isDryRun );
                        else
                            throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For Organizational Unit Creation.", AdStatusType.MissingInput );
                    }
                    else
                        DirectoryServices.ModifyOrganizationUnit( ou.Identity, ou.Description, ou.Properties, isDryRun );
                    OnLogMessage( "ProcessModify", obj.Type + " [" + obj.Identity + "] Modified." );
                    result.Statuses.Add( status );
                    if ( returnObject )
                    {
                        adObject = GetActiveDirectoryObject( obj );
                        result.OrganizationalUnit = (OrganizationalUnitObject)adObject;
                    }
                    break;
                default:
                    throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
            }
        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessCreate", e.Message );
            OnLogMessage( "ProcessCreate", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.Action, obj );
        }

        results.Add( result );

    }

    private void ProcessDelete(AdObject obj, bool returnObject = false)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            Type = obj.Type,
            Identity = obj.Identity
        };

        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            Action = config.Action,
            Status = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            switch ( obj.Type )
            {
                case AdObjectType.User:
                    AdUser user = (AdUser)obj;
                    DirectoryServices.DeleteUser( user.Identity );
                    result.Statuses.Add( status );
                    break;
                case AdObjectType.Group:
                    AdGroup group = (AdGroup)obj;
                    DirectoryServices.DeleteGroup( group.Identity, isDryRun );
                    result.Statuses.Add( status );
                    break;
                case AdObjectType.OrganizationalUnit:
                    AdOrganizationalUnit ou = (AdOrganizationalUnit)obj;
                    DirectoryServices.DeleteOrganizationUnit( ou.Identity );
                    result.Statuses.Add( status );
                    break;
                default:
                    throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
            }

            String message = $"{obj.Type} [{obj.Identity}] Deleted.";
            OnLogMessage( "ProcessDelete", message );
        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessDelete", e.Message );
            OnLogMessage( "ProcessDelete", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.Action, obj );
        }

        results.Add( result );

    }

    private void ProcessGroupAdd(AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            Type = obj.Type,
            Identity = obj.Identity
        };

        AddToGroup( result, obj, returnObject );
        results.Add( result );
    }

    private void ProcessAccessRules(AdObject obj, bool returnObject = false)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            Type = obj.Type,
            Identity = obj.Identity
        };

        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            Action = config.Action,
            Status = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            // Get Target DirectoryEntry For Rules
            DirectoryEntry de = null;
            if ( obj.Type == AdObjectType.User || obj.Type == AdObjectType.Group )
            {
                Principal principal = DirectoryServices.GetPrincipal( obj.Identity );
                if ( principal.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                    de = (DirectoryEntry)principal.GetUnderlyingObject();
                else
                    throw new AdException( $"AddAccessRule Not Available For Object Type [{principal.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
            }
            else
                de = DirectoryServices.GetDirectoryEntry( obj.Identity );

            // Add Rules To Target DirectoryEntry
            foreach ( AdAccessRule rule in obj.AccessRules )
            {
                ActiveDirectoryRights rights = 0;
                if (rule.Rights != null)
                    foreach ( ActiveDirectoryRights right in rule.Rights )
                        rights |= right;

                String message = String.Empty;
                switch ( config.Action )
                {
                    case ActionType.AddAccessRule:
                        DirectoryServices.AddAccessRule( de, rule.Identity, rights, rule.Type );
                        message = $"{rule.Type} [{rights}] Rule Added To {obj.Type} [{obj.Identity}] For Identity [{rule.Identity}].";
                        break;
                    case ActionType.RemoveAccessRule:
                        DirectoryServices.DeleteAccessRule( de, rule.Identity, rights, rule.Type );
                        message = $"{rule.Type} [{rights}] Rule Deleted From {obj.Type} [{obj.Identity}] For Identity [{rule.Identity}].";
                        break;
                    case ActionType.SetAccessRule:
                        DirectoryServices.SetAccessRule( de, rule.Identity, rights, rule.Type );
                        message = $"{rule.Type} [{rights}] Rule Set On {obj.Type} [{obj.Identity}] For Identity [{rule.Identity}].";
                        break;
                    case ActionType.PurgeAccessRules:
                        DirectoryServices.PurgeAccessRules( de, rule.Identity );
                        message = $"All Rules Purged On {obj.Type} [{obj.Identity}] For Identity [{rule.Identity}].";
                        break;
                    default:
                        throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
                }

                result.Statuses.Add( status );
                OnLogMessage( "ProcessAccessRules", message );
            }

        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessDelete", e.Message );
            OnLogMessage( "ProcessDelete", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.Action, obj );
        }

        if ( returnObject )
        {
            object adObject = GetActiveDirectoryObject( obj );
            Type returnType = obj.GetType();
            if ( returnType == typeof( AdUser ) )
                result.User = (UserPrincipalObject)adObject;
            else if ( returnType == typeof( AdGroup ) )
                result.Group = (GroupPrincipalObject)adObject;
            else if ( returnType == typeof( AdOrganizationalUnit ) )
                result.OrganizationalUnit = (OrganizationalUnitObject)adObject;
            else
                throw new AdException( $"Unknown Object Return Type [{returnType}]", AdStatusType.NotSupported );
        }

        results.Add( result );

    }

    private void AddToGroup(ActiveDirectoryObjectResult result, AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            Action = ActionType.AddToGroup,
            Status = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            switch ( obj.Type )
            {
                case AdObjectType.User:
                    AdUser user = (AdUser)obj;
                    if ( user.Groups != null )
                    {
                        foreach ( string userGroup in user.Groups )
                        {
                            try
                            {
                                DirectoryServices.AddUserToGroup( user.Identity, userGroup, isDryRun );
                                String userMessage = $"{obj.Type} [{user.Identity}] Added To Group [{userGroup}].";
                                OnLogMessage( "ProcessGroupAdd", userMessage );
                                status.Message = userMessage;
                                result.Statuses.Add( new ActiveDirectoryStatus( status ) );
                            }
                            catch ( AdException ldeUserEx )
                            {
                                ProcessActiveDirectoryException( result, ldeUserEx, status.Action, obj );
                            }
                        }
                    }
                    break;
                case AdObjectType.Group:
                    AdGroup group = (AdGroup)obj;
                    if ( group.Groups != null )
                    {
                        foreach ( string groupGroup in group.Groups )
                        {
                            try
                            {
                                DirectoryServices.AddGroupToGroup( group.Identity, groupGroup, isDryRun );
                                String groupMessage = $"{obj.Type} [{group.Identity}] Added To Group [{groupGroup}].";
                                OnLogMessage( "ProcessGroupAdd", groupMessage );
                                status.Message = groupMessage;
                                result.Statuses.Add( new ActiveDirectoryStatus( status ) );
                            }
                            catch ( AdException ldeGroupEx )
                            {
                                ProcessActiveDirectoryException( result, ldeGroupEx, status.Action, obj );
                            }
                        }
                    }
                    break;
                default:
                    throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
            }

            if ( returnObject )
                DoQuery( result, obj, true, false );
        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessGroupAdd", e.Message );
            OnLogMessage( "ProcessGroupAdd", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.Action, obj );
        }

    }

    private void ProcessGroupRemove(AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            Type = obj.Type,
            Identity = obj.Identity
        };

        RemoveFromGroup( result, obj, returnObject );
        results.Add( result );
    }

    private void RemoveFromGroup(ActiveDirectoryObjectResult result, AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            Action = ActionType.RemoveFromGroup,
            Status = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            switch ( obj.Type )
            {
                case AdObjectType.User:
                    AdUser user = (AdUser)obj;
                    if ( user.Groups != null )
                    {
                        foreach ( string userGroup in user.Groups )
                        {
                            DirectoryServices.RemoveUserFromGroup( user.Identity, userGroup, isDryRun );
                            String userMessage = $"{obj.Type} [{user.Identity}] Removed From Group [{userGroup}].";
                            OnLogMessage( "ProcessGroupRemove", userMessage );
                            status.Message = userMessage;
                            result.Statuses.Add( new ActiveDirectoryStatus( status ) );
                        }
                    }
                    break;
                case AdObjectType.Group:
                    AdGroup group = (AdGroup)obj;
                    if ( group.Groups != null )
                    {
                        foreach ( string groupGroup in group.Groups )
                        {
                            DirectoryServices.RemoveGroupFromGroup( group.Identity, groupGroup, isDryRun );
                            String groupMessage = $"{obj.Type} [{group.Identity}] Removed From Group [{groupGroup}].";
                            OnLogMessage( "ProcessGroupRemove", groupMessage );
                            status.Message = groupMessage;
                            result.Statuses.Add( new ActiveDirectoryStatus( status ) );
                        }
                    }
                    break;
                default:
                    throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
            }

            if ( returnObject )
                DoQuery( result, obj, true, false );
        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessGroupRemove", e.Message );
            OnLogMessage( "ProcessGroupRemove", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.Action, obj );
        }
    }

    private void ProcessActiveDirectoryException(ActiveDirectoryObjectResult result, AdException ex, ActionType action, AdObject obj)
    {
        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            Action = action,
            Status = ex.Type,
            Message = ex.Message,
        };

        switch ( obj.Type )
        {
            case AdObjectType.User:
                result.Statuses.Add( status );
                break;
            case AdObjectType.Group:
                result.Statuses.Add( status );
                break;
            case AdObjectType.OrganizationalUnit:
                result.Statuses.Add( status );
                break;
            default:
                throw ex;
        }

        OnLogMessage( "Exception", ex.Message );
    }

}