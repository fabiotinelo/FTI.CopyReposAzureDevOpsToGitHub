using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FTI.Model
{
    public class Repository
    {
        public string OrganizationAzureDevops { get; set; }
        public string ProjectAzureDevOps { get; set; }
        public string RepoNameAzureDevOps { get; set; }
        public string Description { get; set; }
        public string RepoUrlAzureDevOps { get; set; }

        public string RemoteURLAzureDevOps { get; set; }
        public string WebURLAzureDevOps { get; set; }

        public bool IsDisabled { get; set; }
        public DateTime LastUpdateRepoAzureDevOps { get; set; }

        public DateTime? RepoLastCommitAzureDevOps { get; set; }

        // [JsonIgnore]
        public string RepoNameForGitHub
        {
            get
            {

                string fullName = string.Empty;


                if (Accelerate)
                {
                    fullName = RepoNameAzureDevOps;
                    fullName = fullName.Replace("Iteris", "", StringComparison.InvariantCultureIgnoreCase).ToLower();
                    fullName = fullName.Replace(" ", "-");
                    fullName = fullName.Replace("---", "-");
                    fullName = fullName.Replace("--", "-");

                    if (!string.IsNullOrEmpty(fullName) && fullName[^1] == '-')
                    {
                        fullName += "accelerate";
                    }
              
                    return fullName;
                }
                else
                {
                    string organizationAzureDevops = string.Empty;
                    if (OrganizationAzureDevops.Equals("iteris-internal", StringComparison.InvariantCultureIgnoreCase))
                        organizationAzureDevops = "shared-code";
                    else if (OrganizationAzureDevops.Equals("iteris-igc", StringComparison.InvariantCultureIgnoreCase))
                        organizationAzureDevops = "knowledge-management";
                    else if (OrganizationAzureDevops.Equals("iteris-idt", StringComparison.InvariantCultureIgnoreCase))
                        organizationAzureDevops = "digital-transformation";

                    string projectAzureDevOps = ProjectAzureDevOps;

                    string repoName = string.Empty;
                    if (RepoNameAzureDevOps.EndsWith(".wiki", StringComparison.InvariantCultureIgnoreCase))
                    {
                        repoName = RepoNameAzureDevOps.Replace(".wiki", "-wiki");
                    }
                    else
                        repoName = RepoNameAzureDevOps;


                    fullName = organizationAzureDevops + "-" + projectAzureDevOps + "-" + repoName;
                    fullName = fullName.Replace("Iteris", "", StringComparison.InvariantCultureIgnoreCase).ToLower();
                    fullName = fullName.Replace(" ", "-");
                    fullName = fullName.Replace("---", "-");
                    fullName = fullName.Replace("--", "-");

                    return fullName;
                }
            }
        }

        public bool Accelerate { get; set; }

        public string Status { get; set; } = "";

        public bool OnlyLastCommit { get; set; }

        public string MainBranch { get; set; }
        

    }
}
