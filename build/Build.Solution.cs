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
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.ControlFlow;
using static Nuke.Common.Tools.Git.GitTasks;

[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild
{
    [Parameter] bool UseHttps { get; } = IsServerBuild;

    [Solution] readonly Solution Solution;

    AbsolutePath ExternalDirectory => RootDirectory / "external";
    IEnumerable<Solution> ExternalSolutions => ExternalDirectory.GlobFiles("*/*.sln").Select(x => x.ReadSolution());

    Target CheckoutExternalRepositories => _ => _
        .Executes(() =>
        {
            foreach (var plugin in Plugins)
            {
                var repository = GitRepository.FromUrl(plugin.Repository);
                var repositoryDirectory = ExternalDirectory / plugin.Name.Replace(" ", "-");
                var origin = UseHttps ? repository.HttpsUrl : repository.SshUrl;

                if (!Directory.Exists(repositoryDirectory))
                {
                    Git($"clone {origin} {repositoryDirectory} --progress", logOutput: false);
                }
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
                        .Select(x => Solution.Directory.GetRelativePathTo(solution.Directory / x).ToString())
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
