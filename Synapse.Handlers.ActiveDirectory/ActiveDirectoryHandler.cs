using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.DirectoryServices.AccountManagement;

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
            //if IsDryRun == true, test if ConnectionString is valid and works.
            if( startInfo.IsDryRun )
            {
                OnProgress( __context, "Attempting connection", sequence: cheapSequence++ );


                result.ExitData = config.LdapRoot;
                result.Message = msg =
                    $"Connection test successful! Connection string: {config.LdapRoot}";
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
                upo = DirectoryServices.GetUser( user.Identity, config.QueryGroupMembership );
                return upo;
            case AdObjectType.Group:
                AdGroup group = (AdGroup)obj;
                GroupPrincipalObject gpo = null;
                gpo = DirectoryServices.GetGroup( group.Identity, config.QueryGroupMembership );
                return gpo;
            case AdObjectType.OrganizationalUnit:
                AdOrganizationalUnit ou = (AdOrganizationalUnit)obj;
                OrganizationalUnitObject ouo = null;
                ouo = DirectoryServices.GetOrganizationalUnit( ou.Identity );
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
                    UserPrincipal up = user.CreateUserPrincipal();
                    if ( config.UseUpsert && DirectoryServices.IsExistingUser( obj.Identity ) )
                        DirectoryServices.ModifyUser( up, isDryRun );
                    else if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                        DirectoryServices.CreateUser( up, isDryRun );
                    else
                        throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For User Creation.", AdStatusType.MissingInput );

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
                    GroupPrincipal gp = group.CreateGroupPrincipal();
                    if ( config.UseUpsert && DirectoryServices.IsExistingGroup( obj.Identity ) )
                        DirectoryServices.ModifyGroup( gp, isDryRun );
                    else if (DirectoryServices.IsDistinguishedName(obj.Identity))
                        DirectoryServices.CreateGroup( gp, isDryRun );
                    else
                        throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For Group Creation.", AdStatusType.MissingInput );

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
                    if ( config.UseUpsert && DirectoryServices.IsExistingDirectoryEntry( obj.Identity ) )
                        DirectoryServices.ModifyOrganizationUnit( ou.Identity, ou.Description, isDryRun );
                    else if (DirectoryServices.IsDistinguishedName(ou.Identity))
                        DirectoryServices.CreateOrganizationUnit( ou.Identity, ou.Description, isDryRun );
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
                    UserPrincipal up = user.CreateUserPrincipal();
                    if ( config.UseUpsert && !DirectoryServices.IsExistingUser( obj.Identity ) )
                    {
                        if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                            DirectoryServices.CreateUser( up, isDryRun );
                        else
                            throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For User Creation.", AdStatusType.MissingInput );
                    }
                    else
                        DirectoryServices.ModifyUser( up, isDryRun );

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
                    GroupPrincipal gp = group.CreateGroupPrincipal();
                    if ( config.UseUpsert && !DirectoryServices.IsExistingGroup( obj.Identity ) )
                    {
                        if (DirectoryServices.IsDistinguishedName(obj.Identity))
                            DirectoryServices.CreateGroup( gp, isDryRun );
                        else
                            throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For Group Creation.", AdStatusType.MissingInput );
                    }
                    else
                        DirectoryServices.ModifyGroup( gp, isDryRun );

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
                    if (config.UseUpsert && !DirectoryServices.IsExistingDirectoryEntry(obj.Identity))
                    {
                        if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                            DirectoryServices.CreateOrganizationUnit( obj.Identity, ou.Description, isDryRun );
                        else
                            throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For Organizational Unit Creation.", AdStatusType.MissingInput );
                    }
                    else
                        DirectoryServices.ModifyOrganizationUnit( ou.Identity, ou.Description, isDryRun );
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