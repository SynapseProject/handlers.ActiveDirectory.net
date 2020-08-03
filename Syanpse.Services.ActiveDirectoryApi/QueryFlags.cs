using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;

public class QueryFlags
{
    public bool ReturnGroupMembership { get; set; } = false;
    public bool ReturnObjects { get; set; } = true;
    public bool ReturnObjectProperties { get; set; } = true;
    public bool ReturnAccessRules { get; set; } = false;

    public static QueryFlags GetFromRequest(HttpRequestMessage request)
    {
        QueryFlags flags = new QueryFlags();

        if (request != null)
        {
            IEnumerable<KeyValuePair<string, string>> queryString = request.GetQueryNameValuePairs();
            foreach (KeyValuePair<string, string> kvp in queryString)
            {
                try
                {
                    if (kvp.Key.Equals("ReturnGroupMembership", StringComparison.OrdinalIgnoreCase))
                        flags.ReturnGroupMembership = bool.Parse(kvp.Value);
                    else if (kvp.Key.Equals("ReturnObjects", StringComparison.OrdinalIgnoreCase))
                        flags.ReturnObjects = bool.Parse(kvp.Value);
                    else if (kvp.Key.Equals("ReturnObjectProperties", StringComparison.OrdinalIgnoreCase))
                        flags.ReturnObjectProperties = bool.Parse(kvp.Value);
                    else if (kvp.Key.Equals("ReturnAccessRules", StringComparison.OrdinalIgnoreCase))
                        flags.ReturnAccessRules = bool.Parse(kvp.Value);
                }
                catch { }
            }
        }

        return flags;
    }
}

