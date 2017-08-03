﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Synapse.Core;
using Synapse.ActiveDirectory.Core;

public class GroupMembershipHandler : HandlerRuntimeBase
{
    public override object GetConfigInstance()
    {
        return null;
    }

    public override object GetParametersInstance()
    {
        return new GroupMembershipRequest()
        {
            AddSection = new List<AddSection>()
            {
                new AddSection()
                {
                    Groups = new List<string>()
                    {
                        "xxxxxx"
                    },
                    Users = new List<string>()
                    {
                        "xxxxxx"
                    }
                },
                new AddSection()
                {
                    Groups = new List<string>()
                    {
                        "xxxxxx"
                    },
                    Users = new List<string>()
                    {
                        "xxxxxx"
                    }
                },
                new AddSection()
                {
                    Groups = new List<string>()
                    {
                        "xxxxxx"
                    },
                    Users = new List<string>()
                    {
                        "xxxxxx"
                    }
                }
            },
            DeleteSection = new List<DeleteSection>()
            {
                new DeleteSection()
                {
                    Groups = new List<string>()
                    {
                        "xxxxxx",
                        "xxxxxx",
                        "xxxxxx",
                        "xxxxxx"
                    },
                    Users = new List<string>()
                    {
                        "xxxxxx"
                    }
                }
            }
        };
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        //int cheapSequence = 0; //used to order message flowing out from the Handler
        const string context = "Execute";
        ExecuteResult result = new ExecuteResult()
        {
            Status = StatusType.Initializing,
            Sequence = int.MaxValue
        };
        string msg = "Processing incoming requests...";
        Exception exception = null;

        // Deserialize the Parameters from the Action declaration
        GroupMembershipRequest parms = DeserializeOrNew<GroupMembershipRequest>(startInfo.Parameters);
        result.Status = StatusType.Running;

        bool encounteredFailure = false;
        GroupMembershipResponse response = new GroupMembershipResponse { Results = new List<Result>() };

        try
        {
            if ( parms?.AddSection != null )
            {
                foreach ( AddSection addsection in parms.AddSection )
                {
                    foreach ( string group in addsection.Groups )
                    {
                        foreach ( string user in addsection.Users )
                        {
                            try
                            {
                                DirectoryServices.AddUserToGroup( user, group, startInfo.IsDryRun );
                                Result r = new Result()
                                {
                                    User = user,
                                    Group = group,
                                    Action = "add",
                                    ExitCode = 0,
                                    Note = startInfo.IsDryRun ? "Dry run has been completed." : "User has been successfully added to the group."
                                };
                                response.Results.Add( r );
                            }
                            catch ( Exception ex )
                            {
                                Result r = new Result()
                                {
                                    User = user,
                                    Group = group,
                                    Action = "add",
                                    ExitCode = -1,
                                    Note = ( startInfo.IsDryRun ? "Dry run has been completed. " : "" ) + ex.Message
                                };
                                response.Results.Add( r );
                                encounteredFailure = true;
                            }
                        }
                    }
                }

                if ( parms?.DeleteSection != null )
                {
                    foreach ( DeleteSection addsection in parms.DeleteSection )
                    {
                        foreach ( string group in addsection.Groups )
                        {
                            foreach ( string user in addsection.Users )
                            {
                                try
                                {
                                    DirectoryServices.RemoveUserFromGroup( user, group, startInfo.IsDryRun );
                                    Result r = new Result()
                                    {
                                        User = user,
                                        Group = group,
                                        Action = "delete",
                                        ExitCode = 0,
                                        Note = startInfo.IsDryRun ? "Dry run has been completed." : "User has been successfully removed from the group."
                                    };
                                    response.Results.Add( r );
                                }
                                catch ( Exception ex )
                                {
                                    Result r = new Result()
                                    {
                                        User = user,
                                        Group = group,
                                        Action = "delete",
                                        ExitCode = -1,
                                        Note = (startInfo.IsDryRun ? "Dry run has been completed. " : "") + ex.Message
                                    };
                                    response.Results.Add( r );
                                    encounteredFailure = true;
                                }
                            }
                        }
                    }
                }

                msg = "Request has been processed" + ( encounteredFailure ? " with error" : "" ) + ".";
                result.Status = encounteredFailure ? StatusType.CompletedWithErrors : StatusType.Success;
            }
        }
        catch ( Exception ex )
        {
            exception = ex;
            msg = $"Processing has been aborted due to: {ex.Message}";
            result.Status = StatusType.Failed;
        }

        response.Status = msg;
        result.ExitData = JsonConvert.SerializeObject( response );

        // Final runtime notification, return sequence=Int32.MaxValue by convention to supercede any other status message
        OnProgress( context, msg, result.Status, sequence: int.MaxValue, ex: exception );

        return result;
    }
}

public class AddSection
{
    [JsonProperty(PropertyName = "groups")]
    public List<string> Groups { get; set; }

    [JsonProperty(PropertyName = "users")]
    public List<string> Users { get; set; }
}

public class DeleteSection
{
    [JsonProperty(PropertyName = "groups")]
    public List<string> Groups { get; set; }

    [JsonProperty(PropertyName = "users")]
    public List<string> Users { get; set; }
}

public class GroupMembershipRequest
{
    [JsonProperty(PropertyName = "addSection")]
    public List<AddSection> AddSection { get; set; }

    [JsonProperty(PropertyName = "deleteSection")]
    public List<DeleteSection> DeleteSection { get; set; }
}


public class Result
{
    [JsonProperty(PropertyName = "user")]
    public string User { get; set; }

    [JsonProperty(PropertyName = "group")]
    public string Group { get; set; }

    [JsonProperty(PropertyName = "action")]
    public string Action { get; set; }

    [JsonProperty(PropertyName = "exitCode")]
    public int ExitCode { get; set; }

    [JsonProperty(PropertyName = "note")]
    public string Note { get; set; }
}

public class GroupMembershipResponse
{
    [JsonProperty(PropertyName = "status")]
    public string Status { get; set; }

    [JsonProperty(PropertyName = "results")]
    public List<Result> Results { get; set; }
}