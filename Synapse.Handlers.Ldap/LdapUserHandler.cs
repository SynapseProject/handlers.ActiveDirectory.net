using System;
using System.Threading;
using Newtonsoft.Json;

using Synapse.Core;
using Synapse.Ldap.Core;

public class LdapUserHandler : HandlerRuntimeBase
{
    LdapRoot _ldapRoot = null;

    public override object GetConfigInstance()
    {
        return new LdapRoot() {LdapPath = "LDAP://XXX.XXX"};
    }

    public override object GetParametersInstance()
    {
        return new UserCredentials() {UserName = "XXX", UserPassword = "XXXX"};
    }

    public override IHandlerRuntime Initialize(string config)
    {
        //deserialize the Config from the Handler declaration
        _ldapRoot = DeserializeOrNew<LdapRoot>(config);

        return this;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        //declare/initialize method-scope variables
        int cheapSequence = 0; //used to order message flowing out from the Handler
        const string __context = "Execute";
        ExecuteResult result = new ExecuteResult()
        {
            Status = StatusType.Failed,
            Sequence = Int32.MaxValue
        };
        string msg = "Complete";
        Exception exc = null;

        //deserialize the Parameters from the Action declaration
        UserCredentials parms = DeserializeOrNew<UserCredentials>(startInfo.Parameters);

        DirectoryServices.CreateUser(_ldapRoot.LdapPath, parms.UserName, parms.UserPassword);

        //if (!String.IsNullOrWhiteSpace(userGuid))
        //{
        //    result.Status = StatusType.Success;
        //    result.ExitData = userGuid;
        //}
        //final runtime notification, return sequence=Int32.MaxValue by convention to supercede any other status message
        OnProgress(__context, msg, result.Status, sequence: Int32.MaxValue, ex: exc);

        return result;
    }
}

public class LdapRoot
{
    public string LdapPath { get; set; }
}

public class UserCredentials
{
    public string UserName { get; set; }
    public string UserPassword { get; set; }
}