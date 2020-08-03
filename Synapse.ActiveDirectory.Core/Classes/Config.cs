using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

using Synapse.Core.Utilities;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Synapse.ActiveDirectory.Core
{
    public class Config
    {
        public List<DomainConfig> Domains { get; set; } = new List<DomainConfig>();
        public bool ReturnUserCannotChangePasswordFlag { get; set; } = false;

        public DomainConfig DefaultDomain { get; internal set; }
        public Dictionary<string, DomainConfig> ValidDomains { get; internal set; } = new Dictionary<string, DomainConfig>(StringComparer.OrdinalIgnoreCase);

        public Config() { }

        public Config(string configFile)
        {
            Config config = new Config();
            if (File.Exists(configFile))
            {
                string content = File.ReadAllText(configFile);
                config = YamlHelpers.Deserialize<Config>(content);

                this.ReturnUserCannotChangePasswordFlag = config.ReturnUserCannotChangePasswordFlag;
            }

            foreach (DomainConfig domain in config.Domains)
            {
                ValidDomains.Add(domain.Name, domain);

                if (domain.IsDefault || DefaultDomain == null)
                    DefaultDomain = domain;

                foreach (string alias in domain.Aliases)
                    ValidDomains.Add(alias, domain);

                Console.WriteLine($"Domain Configured : {domain}");
            }

            if (DefaultDomain == null)
                DefaultDomain = new DomainConfig();

            Console.WriteLine($"Default Domain    : {DefaultDomain}");
        }

        public static string GetDefaultConfigFile()
        {
            Assembly dll = Assembly.GetExecutingAssembly();
            string directory = Path.GetDirectoryName(dll.Location);
            string dllName = Path.GetFileNameWithoutExtension(dll.Location);
            string configFile = Path.Combine(directory, $"{dllName}.yaml");

            return configFile;
        }

        public DomainConfig GetDomain(string domainName)
        {
            DomainConfig domain = DefaultDomain;
            if (!String.IsNullOrWhiteSpace(domainName) && ValidDomains.ContainsKey(domainName))
                domain = ValidDomains[domainName];
            return domain;
        }

        public bool HasDomain(string domainName)
        {
            if (!String.IsNullOrWhiteSpace(domainName) && ValidDomains.ContainsKey(domainName))
                return true;
            else
                return false;
        }
    }

    public class DomainConfig
    {
        public string Name { get; set; } = Environment.UserDomainName;
        public int Port { get; set; } = 389;
        public bool UseSSL { get; set; } = false;
        public int ContextOptions { get; set; } = 1;
        public int AuthenticationTypes { get; set; } = 1;
        public string Username { get; set; } = null;
        public string Password { get; set; } = null;
        public string RSAKey { get; set; } = null;
        public bool IsDefault { get; set; } = false;
        public List<string> Aliases { get; set; } = new List<string>();

        public ContextOptions ContextOptionsEnum
        {
            get
            {
                return (ContextOptions)this.ContextOptions;
            }
        }

        public AuthenticationTypes AuthenticationTypesEnum
        {
            get
            {
                return (AuthenticationTypes)this.AuthenticationTypes;
            }
        }

        public string DecryptedPassword
        {
            get
            {
                string pwd = Password;
                if (!String.IsNullOrWhiteSpace(RSAKey))
                    pwd = CryptoHelpers.Decrypt(filePath: RSAKey, value: pwd);
                return pwd;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Name}:{Port}");
            if (UseSSL)
                sb.Append($" (SSL [{ContextOptionsEnum}])");
            if (Aliases.Count > 0)
                sb.Append($", Aliases [{String.Join(",", Aliases)}]");

            return sb.ToString();
        }
    }
}
