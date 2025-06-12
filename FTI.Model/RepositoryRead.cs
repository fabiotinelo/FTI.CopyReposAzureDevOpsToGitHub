using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FTI.Model
{
    public class RepositoryRead
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


        public string RepoNameForGitHub
        {
            get; set;
        }

        public bool Accelerate { get; set; }

        public string Status { get; set; } = "";

        public bool OnlyLastCommit { get; set; }

        public string MainBranch { get; set; }


    }
}
