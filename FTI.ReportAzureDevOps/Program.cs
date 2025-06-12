using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using FTI.Model;

namespace FTI.ReportAzureDevOps
{
    internal class Program
    {


        private static readonly string pat = "xx";
        private static readonly string devOpsApiUrl = "https://dev.azure.com";

       // private static string[] reposAccelerate = { "api-platform-iteris", "web-platform-iteris", "spring-starter-service-bus", "spring-web-autoconfigure", "spring-web-starter", "plugin-gerador-cenarios", "plugin-gerador-historias" };
               


        static async Task Main(string[] args)
        {
            // List<string> reportData = new List<string>();
            // reportData.Add("Organization;Project;Description;Repository");
            List<Repository> repositoriesJson = new List<Repository>();

            var organizations = await GetOrganizationsAsync();

            foreach (var org in organizations)
            {
                var projects = await GetProjectsAsync(org);

                foreach (var p in projects)
                {
                    var repositories = await GetRepositoriesAsync(org, p.Name);

                    foreach (var repo in repositories)
                    {
                        string descriptionAdjust = p.Description.Replace("\n", " ").Replace("\r", " ");


                        var repReport = new Repository
                        {
                            OrganizationAzureDevops = org,
                            ProjectAzureDevOps = p.Name,
                            RepoNameAzureDevOps = repo.Name,
                            RepoUrlAzureDevOps = repo.URL,
                            WebURLAzureDevOps = repo.WebURL,
                            RemoteURLAzureDevOps = repo.RemoteURL,
                            Description = descriptionAdjust,
                            LastUpdateRepoAzureDevOps = repo.ProjectLastUpdate,
                            RepoLastCommitAzureDevOps = repo.RepoLastCommit,
                            IsDisabled = repo.IsDisabled
                        };

                       // if (reposAccelerate.Contains(repo.Name))
                        //   repReport.Accelerate = true;

                        repositoriesJson.Add(repReport);    
                        //reportData.Add($"{org};{p.Name};{descriptionAdjust};{repo}");
                    }
                }
            }
            

            File.WriteAllText($"AzureDevOpsMigration_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.json", JsonConvert.SerializeObject(repositoriesJson, Newtonsoft.Json.Formatting.Indented));

            //File.WriteAllLines($"AzureDevOpsReport_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.json", reportData, Encoding.UTF8);
            Console.WriteLine("Arquivo de Migração gerado com sucesso: AzureDevOpsReport.csv");

        }


        static async Task<List<string>> GetOrganizationsAsync()
        {

            // return new List<string> { "iteris-igc", "iteris-internal", "iteris-idt" }; 
            return new List<string> { "iteris-clientes" };
        }

        static async Task<List<Project>> GetProjectsAsync(string organization)
        {
            string url = $"{devOpsApiUrl}/{organization}/_apis/projects?api-version=7.1-preview.4";
            var response = await GetApiResponseAsync(url);
            using var doc = JsonDocument.Parse(response);
            var projects = new List<Project>();
            foreach (var element in doc.RootElement.GetProperty("value").EnumerateArray())
            {
                string name = element.GetProperty("name").GetString();
                string description = element.TryGetProperty("description", out var desc) ? desc.GetString() : "Sem descrição";

                projects.Add(new Project { Name = name, Description = description });
            }
            return projects;
        }

        static async Task<List<RepositoryPOCO>> GetRepositoriesAsync(string organization, string projectName)
        {
            string url = $"{devOpsApiUrl}/{organization}/{projectName}/_apis/git/repositories?api-version=7.1-preview.1";
            var response = await GetApiResponseAsync(url);
            using var doc = JsonDocument.Parse(response);
            var repositories = new List<RepositoryPOCO>();



            foreach (var element in doc.RootElement.GetProperty("value").EnumerateArray())
            {
                var repoId = element.GetProperty("id").GetString();
                var lastCommitDate = GetLastCommitDate(repoId, organization, projectName);

                repositories.Add(new RepositoryPOCO {
                    Name = element.GetProperty("name").GetString(),
                    URL = element.GetProperty("url").GetString(),
                    RemoteURL = element.GetProperty("remoteUrl").GetString(),
                    WebURL = element.GetProperty("webUrl").GetString(),
                    IsDisabled = element.GetProperty("isDisabled").GetBoolean(),
                    RepoLastCommit = lastCommitDate,
                    ProjectLastUpdate = element.GetProperty("project").GetProperty("lastUpdateTime").GetDateTime() });
                    
            }
            return repositories;
        }

        static DateTime? GetLastCommitDate(string repoId, string organization, string projectName)
        {
            string commitUrl = $"{devOpsApiUrl}/{organization}/{projectName}/_apis/git/repositories/{repoId}/commits?api-version=7.1-preview.1&$top=1";

            try
            {
                var commitResponse = GetApiResponseAsync(commitUrl).Result;

                using var commitDoc = JsonDocument.Parse(commitResponse);
                if (commitDoc.RootElement.TryGetProperty("value", out var commits) && commits.GetArrayLength() > 0)
                {
                    return commits[0].GetProperty("committer").GetProperty("date").GetDateTime();
                }
                return null;
            }
            catch (Exception ex)
            {

                return null;
            }
           
            
        }



        static async Task<string> GetApiResponseAsync(string url)
        {
            using var client = new HttpClient();
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

    }

    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class RepositoryPOCO
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string RemoteURL { get; set; }
        public string WebURL { get; set; }
        public DateTime ProjectLastUpdate { get; set; }
        public DateTime? RepoLastCommit { get; set; }
        public bool IsDisabled { get; set; }
    }
}
