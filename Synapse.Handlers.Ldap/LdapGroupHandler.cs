using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Synapse.Core;
using Synapse.Ldap.Core;

public class LdapGroupHandler : HandlerRuntimeBase
{
    LdapRoot _ldapRoot = null;

    public override object GetConfigInstance()
    {
        return new LdapRoot() { LdapPath = "LDAP://DC=XXX" };
    }

    public override object GetParametersInstance()
    {
        return new GroupOperation() {Operation = "CreateGroup", OuPath = "LDAP://OU=XXX,DC=YYY", GroupName = "ZZZ"};
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
        const string context = "Execute";
        ExecuteResult result = new ExecuteResult()
        {
            Status = StatusType.Failed,
            Sequence = Int32.MaxValue
        };
        string msg = "In Progress";
        Exception exc = null;

        //deserialize the Parameters from the Action declaration
        GroupOperation parms = DeserializeOrNew<GroupOperation>(startInfo.Parameters);

        try
        {
            if (parms.Operation != null && parms.Operation.Equals("CreateGroup"))
            {
                DirectoryServices.CreateGroup(parms.OuPath, parms.GroupName);
                msg = "Complete";
                result.Status = StatusType.Success;
                result.ExitData = $"{parms.GroupName} has been successfully created under {parms.OuPath}.";
            } else if (parms.Operation != null && parms.Operation.Equals("DeleteGroup"))
            {
                DirectoryServices.DeleteGroup(parms.GroupName);
                msg = "Complete";
                result.Status = StatusType.Success;
                result.ExitData = $"{parms.GroupName} has been deleted.";
            }
            else
            {
                msg = "Failed";
                result.Status = StatusType.Failed;
                result.ExitData = $"{parms.Operation} is not a supported operation.";
            }
        }
        catch (Exception ex)
        {
            exc = ex;
            msg = "Failed";
            result.ExitData = ex.Message;
        }

        //final runtime notification, return sequence=Int32.MaxValue by convention to supercede any other status message
        OnProgress(context, msg, result.Status, sequence: Int32.MaxValue, ex: exc);

        return result;
    }

}

public class GroupOperation
{
    public string Operation { get; set; }
    public string OuPath { get; set; }
    public string GroupName { get; set; }
}