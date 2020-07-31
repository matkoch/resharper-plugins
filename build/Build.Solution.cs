
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.ControlFlow;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.IO.SerializationTasks;
using static Nuke.Common.ProjectModel.ProjectModelTasks;
using static Nuke.Common.Tools.Git.GitTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild
{
    [Parameter] readonly bool UseHttps;

    [Solution] readonly Solution Solution;

    AbsolutePath ExternalRepositoriesDirectory => RootDirectory / "external";
    IEnumerable<Solution> ExternalSolutions => ExternalRepositoriesDirectory.GlobFiles("*/*.sln").Select(x => ParseSolution(x));

    Target CheckoutExternalRepositories => _ => _
        .Executes(async () =>
        {
            foreach (var plugin in Plugins)
            {
                var repository = GitRepository.FromUrl(plugin.repository);
                var repositoryDirectory = ExternalRepositoriesDirectory / plugin.name.Replace(" ", "-");
                var origin = UseHttps ? repository.HttpsUrl : repository.SshUrl;

                if (!Directory.Exists(repositoryDirectory))
                    Git($"clone {origin} {repositoryDirectory} --progress");
                else
                {
                    SuppressErrors(() => Git($"remote add origin {origin}", repositoryDirectory));
                    Git($"remote set-url origin {origin}", repositoryDirectory);
                }
            }
        });

    [UsedImplicitly]
    Target GenerateGlobalSolution => _ => _
        .DependsOn(CheckoutExternalRepositories)
        .Executes(() =>
        {
            Solution.Configurations =
                new Dictionary<string, string>
                {
                    { "Debug|Any CPU", "Debug|Any CPU" },
                    { "Release|Any CPU", "Release|Any CPU" }
                };

            SolutionFolder GetParentFolder(PrimitiveProject solutionFolder) =>
                Solution.AllSolutionFolders.FirstOrDefault(x => x.ProjectId == solutionFolder.SolutionFolder?.ProjectId);

            void AddSolution(Solution solution, SolutionFolder folder = null)
            {
                IDictionary<string, string> GetItems(SolutionFolder solutionFolder)
                {
                    return solutionFolder.Items.Keys
                        .Select(x =>
                            (string) (WinRelativePath) GetRelativePath(Solution.Directory, solution.Directory / x))
                        .ToDictionary(x => x, x => x);
                }

                solution.AllSolutionFolders.ForEach(x => Solution.AddSolutionFolder(x.Name, x.ProjectId, GetParentFolder(x) ?? folder));
                solution.AllSolutionFolders.ForEach(x => Solution.GetSolutionFolder(x.ProjectId).Items = GetItems(x));
                solution.AllProjects.ForEach(x => Solution.AddProject(x.Name, x.TypeId, x.Path, x.ProjectId, x.Configurations, GetParentFolder(x) ?? folder));

                Solution.AllSolutionFolders.ForEach(x => x.ProjectId = Guid.NewGuid());
                Solution.AllProjects.ForEach(x => x.ProjectId = Guid.NewGuid());
            }

            Solution.AllProjects.Where(x => x.Name != "_build" || x.SolutionFolder != null).ForEach(x => Solution.RemoveProject(x));
            Solution.AllSolutionFolders.ForEach(x => Solution.RemoveSolutionFolder(x));

            ExternalSolutions.ForEach(x => AddSolution(x, Solution.AddSolutionFolder(Path.GetFileName(x.Directory))));

            Solution.Save();
        });
}
