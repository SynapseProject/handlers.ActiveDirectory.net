using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.DirectoryServices.AccountManagement;

using Synapse.Core;
using Synapse.Ldap.Core;
using Synapse.Handlers.Ldap;

public class LdapHandler : HandlerRuntimeBase
{
    LdapHandlerConfig config = null;
    LdapHandlerResults results = new LdapHandlerResults();
    bool isDryRun = false;

    public override IHandlerRuntime Initialize(string config)
    {
        //deserialize the Config from the Handler declaration
        this.config = DeserializeOrNew<LdapHandlerConfig>( config );
        return this;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        int cheapSequence = 0;
        const string __context = "Execute";
        ExecuteResult result = new ExecuteResult()
        {
            Status = StatusType.Complete,
            Sequence = Int32.MaxValue
        };
        string msg = "Complete";
        Exception exc = null;

        isDryRun = startInfo.IsDryRun;

        //deserialize the Parameters from the Action declaration
        Synapse.Handlers.Ldap.LdapHandlerParameters parameters = base.DeserializeOrNew<Synapse.Handlers.Ldap.LdapHandlerParameters>( startInfo.Parameters );

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
                        ProcessLdapObjects( parameters.Users, ProcessQuery );
                        ProcessLdapObjects( parameters.Groups, ProcessQuery );
                        ProcessLdapObjects( parameters.OrganizationalUnits, ProcessQuery );
                        break;
                    case ActionType.Create:
                        ProcessLdapObjects( parameters.OrganizationalUnits, ProcessCreate );
                        ProcessLdapObjects( parameters.Groups, ProcessCreate );
                        ProcessLdapObjects( parameters.Users, ProcessCreate );
                        break;
                    case ActionType.Modify:
                        // TODO : Implement Me
                        break;
                    case ActionType.Delete:
                        ProcessLdapObjects( parameters.Users, ProcessDelete );
                        ProcessLdapObjects( parameters.Groups, ProcessDelete );
                        ProcessLdapObjects( parameters.OrganizationalUnits, ProcessDelete );
                        break;
                    case ActionType.AddToGroup:
                        ProcessLdapObjects( parameters.Users, ProcessGroupAdd );
                        ProcessLdapObjects( parameters.Groups, ProcessGroupAdd );
                        break;
                    case ActionType.RemoveFromGroup:
                        ProcessLdapObjects( parameters.Users, ProcessGroupRemove );
                        ProcessLdapObjects( parameters.Groups, ProcessGroupRemove );
                        break;
                }
            }
        }
        //something wnet wrong: hand-back the Exception and mark the execution as Failed
        catch( Exception ex )
        {
            exc = ex;
            result.Status = StatusType.Failed;
            result.ExitData = msg =
                ex.Message + " | " + ex.InnerException?.Message;
        }

        result.ExitData = results.Serialize( config.OutputType, config.PrettyPrint );

        OnProgress( __context, result.ExitData?.ToString(), result.Status, sequence: cheapSequence++, ex: exc );

        //final runtime notification, return sequence=Int32.MaxValue by convention to supercede any other status message
        OnProgress( __context, msg, result.Status, sequence: Int32.MaxValue, ex: exc );

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

    private void ProcessLdapObjects(IEnumerable<LdapObject> objs, Action<LdapObject> processFunction)
    {
        if ( objs != null )
        {
            if ( config.RunSequential )
            {
                foreach ( LdapObject obj in objs )
                    processFunction( obj );
            }
            else
                Parallel.ForEach( objs, obj =>
                {
                    processFunction( obj );
                } );
        }
    }

    private void ProcessQuery(LdapObject obj)
    {
        switch ( obj.Type )
        {
            case ObjectClass.User:
                LdapUser user = (LdapUser)obj;
                UserPrincipalObject upo = DirectoryServices.GetUser( user.Name, config.QueryGroupMembership );
                results.Add( upo );
                break;
            case ObjectClass.Group:
                LdapGroup group = (LdapGroup)obj;
                GroupPrincipalObject gpo = DirectoryServices.GetGroup( group.Name, config.QueryGroupMembership );
                results.Add( gpo );
                break;
            case ObjectClass.OrganizationalUnit:
                LdapOrganizationalUnit ou = (LdapOrganizationalUnit)obj;
                OrganizationalUnitObject ouo = DirectoryServices.GetOrganizationalUnit( ou.Name, config.LdapRoot );
                results.Add( ouo );

                break;
            default:
                throw new Exception( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]" );
        }
    }

    private void ProcessCreate(LdapObject obj)
    {
        switch ( obj.Type )
        {
            case ObjectClass.User:
                LdapUser user = (LdapUser)obj;
                DirectoryServices.CreateUser( user.OUPath, user.Name, user.Password, user.GivenName, user.Surname, user.Description );
                break;
            case ObjectClass.Group:
                LdapGroup group = (LdapGroup)obj;
                DirectoryServices.CreateGroup( group.OUPath, group.Name, group.Description, group.Scope, group.IsSecurityGroup, isDryRun );
                break;
            case ObjectClass.OrganizationalUnit:
                LdapOrganizationalUnit ou = (LdapOrganizationalUnit)obj;
                DirectoryServices.CreateOrganizationUnit( ou.Parent, ou.Name );
                break;
            default:
                throw new Exception( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]" );
        }
    }

    private void ProcessDelete(LdapObject obj)
    {
        switch ( obj.Type )
        {
            case ObjectClass.User:
                LdapUser user = (LdapUser)obj;
                DirectoryServices.DeleteUser( user.Name );
                break;
            case ObjectClass.Group:
                LdapGroup group = (LdapGroup)obj;
                DirectoryServices.DeleteGroup( group.Name, isDryRun );
                break;
            case ObjectClass.OrganizationalUnit:
                LdapOrganizationalUnit ou = (LdapOrganizationalUnit)obj;
                DirectoryServices.DeleteOrganizationUnit( ou.Name );
                break;
            default:
                throw new Exception( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]" );
        }
    }

    private void ProcessGroupAdd(LdapObject obj)
    {
        switch ( obj.Type )
        {
            case ObjectClass.User:
                LdapUser user = (LdapUser)obj;
                foreach ( String userGroup in user.Groups )
                    DirectoryServices.AddUserToGroup( user.Name, userGroup, isDryRun );
                break;
            case ObjectClass.Group:
                LdapGroup group = (LdapGroup)obj;
                foreach ( String groupGroup in group.Groups )
                    DirectoryServices.AddGroupToGroup( group.Name, groupGroup, isDryRun );
                break;
            default:
                throw new Exception( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]" );
        }
    }

    private void ProcessGroupRemove(LdapObject obj)
    {
        switch ( obj.Type )
        {
            case ObjectClass.User:
                LdapUser user = (LdapUser)obj;
                foreach ( String userGroup in user.Groups )
                    DirectoryServices.RemoveUserFromGroup( user.Name, userGroup, isDryRun );
                break;
            case ObjectClass.Group:
                LdapGroup group = (LdapGroup)obj;
                foreach ( String groupGroup in group.Groups )
                    DirectoryServices.RemoveGroupFromGroup( group.Name, groupGroup, isDryRun );
                break;
            default:
                throw new Exception( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]" );
        }
    }

}