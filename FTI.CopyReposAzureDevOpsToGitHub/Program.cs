using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.IO;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using FTI.Model;

namespace FTI.CopyReposAzureDevOpsToGitHub
{
    internal class Program
    {

        static List<RepositoryRead> repositories;
        static string jsonFilePath;
        static string tempFolderClone = "C:\\_CloneMigration";

        private static readonly string pacAzureDevOps = "xxx";




        //static string pacGithub = "xxxxxxxxxxxx";
        //static string userGitHub = "fabiotinelo";

        //static string pacGithub = "xxxxx";  //Solutioning
        //static string orgNameGitHub = "rrrrrrr";

        static string pacGithub = "xxx";    //Accelerate
        static string orgNameGitHub = "xxx"; //Accelerate

        static string userGitHub = "fabio-tinelo_xxxx";
       
        

        static void Main()
        {

            Console.WriteLine("'Digite M caso queira iniciar a migração");

            string op = Console.ReadLine();

            if (op.Equals("M", StringComparison.InvariantCultureIgnoreCase))
            {

                if (!Directory.Exists(tempFolderClone))
                    Directory.CreateDirectory(tempFolderClone);


                //Console.WriteLine("Informe o nome do arquivo json que será usado como base desta migração: ");
                jsonFilePath = "repos.json";// Console.ReadLine();

                if (!File.Exists(jsonFilePath))
                {
                    Console.WriteLine("O nome do arquivo informado não existe no diretório atual.");
                    return;
                }

                GetRepoJson();
                bool resultMigration = false;

                // foreach (var repo in repositories.Where(c => c.Status == string.Empty && c.Accelerate == false && c.IsDisabled == false))
                foreach (var repo in repositories.Where(c => c.Status == string.Empty && c.Accelerate == true && c.IsDisabled == false))
                {
                    resultMigration = MigrateRepository(repo);


                    if (!resultMigration)
                        repo.Status = "Erro";
                    else
                        repo.Status = "Ok";

                    //Atualizar status
                    SaveRepoJson();

                    if (!resultMigration)
                    {
                        Console.WriteLine($"Falha ao migrar o repositório {repo.RepoNameAzureDevOps}. Operação interrompida.");
                        break;
                    }

                   // break;
                }
            }
            //else if (op.Equals("C", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    Console.WriteLine("Informe o nome do arquivo json que será gerado: ");
            //    string filename = Console.ReadLine();
            //    if (!ValidadeExistsRepoJson(filename))
            //        return;

            //    Console.WriteLine("Informe o nome da organização do Azure DevOps: ");
            //    string orgName = Console.ReadLine();

            //    Console.WriteLine("Informe o nome do projeto no Azure DevOps: ");
            //    string projectName = Console.ReadLine();

            //    CreateListRepos(filename, orgName, projectName);

            //}

        }



        static void GetRepoJson()
        {
            repositories = JsonConvert.DeserializeObject<List<RepositoryRead>>(File.ReadAllText(jsonFilePath, System.Text.Encoding.UTF8));
        }

        static void SaveRepoJson()
        {
            File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(repositories, Formatting.Indented), System.Text.Encoding.UTF8);
        }

        static bool MigrateRepository(RepositoryRead repo)
        {
            try
            {
                string azureRepoUrl = repo.WebURLAzureDevOps.Replace("//", $"//{pacAzureDevOps}@");

                //Para Github pessoal
                //string githubRepoUrl = $"https://{pacGithub}@github.com/{userGitHub}/{repo.RepoNameForGitHub}.git";

                //Para EMU
                string githubRepoUrl = $"https://{pacGithub}@github.com/{orgNameGitHub}/{repo.RepoNameForGitHub}.git";
                string directoryName = Path.Combine(tempFolderClone, $"{repo.RepoNameForGitHub}.git");

                Console.WriteLine("------------------------------------------------------------------------------------------");

                Console.WriteLine($"1. Clonando o repositório: {repo.RepoNameForGitHub}");

                if (Directory.Exists(directoryName))
                {
                    Console.WriteLine($"1.1 Excluindo diretório: {repo.RepoNameForGitHub}");
                    DeleteDirectory(directoryName);
                }

                string commandClone = $"git clone --bare {azureRepoUrl} {directoryName}";

                if (repo.OnlyLastCommit)
                    commandClone = $"git clone --depth 1 --branch {repo.MainBranch} {azureRepoUrl} {directoryName}";


                if (!RunCommand(commandClone)) { repo.Status = "Erro"; return false; }
                

                Console.WriteLine($"2. Criando um repositório no Github: {repo.RepoNameForGitHub}");
                string createRepoJson = $"{{\\\"name\\\":\\\"{repo.RepoNameForGitHub}\\\", \\\"private\\\":true}}";

                //github pessoal
                //string createRepoCommand = $"curl -X POST https://api.github.com/user/repos -H \"Authorization: token {pacGithub}\" -H \"Accept: application/vnd.github.v3+json\" -d \"{createRepoJson}\"";

                //github EMU
                string createRepoCommand = $"curl -X POST https://api.github.com/orgs/{orgNameGitHub}/repos -H \"Authorization: token {pacGithub}\" -H \"Accept: application/vnd.github.v3+json\" -d \"{createRepoJson}\"";


                if (!RunCommand(createRepoCommand)) { repo.Status = "Erro"; return false; }

                Console.WriteLine($"3. Realizando o Push para o novo respositório: {repo.RepoNameForGitHub}");
                if (!RunCommand($"cd {directoryName} && git push --mirror {githubRepoUrl}")) { repo.Status = "Erro"; return false; }

                if (Directory.Exists(directoryName))
                {
                    Console.WriteLine($"4. Excluindo o diretório: {directoryName}");
                    DeleteDirectory(directoryName);
                }

                repo.Status = "Sucesso";
                Console.WriteLine($"Repositório {repo.RepoNameAzureDevOps} migrado com sucesso!\n");
                Console.WriteLine("------------------------------------------------------------------------------------------\n");
                return true;



            }
            catch (Exception ex)
            {
                repo.Status = "Erro";
                Console.WriteLine($"Erro ao migrar {repo.RepoNameAzureDevOps}: {ex.Message}");
                return false;
            }
        }

        static void DeleteDirectory(string targetDir)
        {
            if (!Directory.Exists(targetDir))
                return;

            // Remover Read-Only de todos os arquivos antes de deletar
            foreach (string file in Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false; // Remove o atributo de somente leitura
                }
            }

            Directory.Delete(targetDir, true);
        }

        static bool RunCommand(string command)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = $"/C {command}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine(output);
                    return true;
                }
                else
                {
                    Console.WriteLine($"Erro: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao executar comando: {ex.Message}");
                return false;
            }
        }

        //static void CreateListRepos(string filename, string organization, string projectName)
        //{
        //    string azureDevOpsUrl = $"https://dev.azure.com/{organization}/{Uri.EscapeDataString(projectName)}/_apis/git/repositories?api-version=7.2-preview.1";
        //    List<Repository> repositories = new List<Repository>();

        //    using (HttpClient client = new HttpClient())
        //    {
        //        var authToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{pat}"));
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        //        HttpResponseMessage response = client.GetAsync(azureDevOpsUrl).Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            string jsonResult = response.Content.ReadAsStringAsync().Result;
        //            JObject reposData = JObject.Parse(jsonResult);

        //            foreach (var repo in reposData["value"])
        //            {
        //                repositories.Add(new Repository
        //                {
        //                    Name = repo["name"].ToString(),
        //                    RepoUrl = repo["webUrl"].ToString(),
        //                    Organization = organization,
        //                    Project = projectName,
        //                    Status = ""
        //                });
        //            }

        //            File.WriteAllText(filename, JsonConvert.SerializeObject(repositories, Formatting.Indented));
        //            Console.WriteLine($"Arquivo {filename} criado com sucesso!");
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Erro: {response.StatusCode} - {response.ReasonPhrase}");
        //        }
        //    }
        //}
    }

}