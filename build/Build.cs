using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Utilities.Text.Yaml;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;

partial class Build
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>();

    AbsolutePath PluginsFile => RootDirectory / "plugins.yml";
    IEnumerable<Plugin> Plugins => PluginsFile.ReadYaml<List<Plugin>>();
    IEnumerable<AbsolutePath> PluginDirectories => RootDirectory.GlobFiles("**/buildPlugin.ps1").Select(x => x.Parent);
    AbsolutePath TemplateDirectory => RootDirectory / "external" / "_Template" / "template";

    AbsolutePath ReSharperRiderUpdateProjectFile
        => ExternalDirectory / "_ReSharperRiderUpdate" / "src" / "ReSharperRiderUpdate.csproj";

    Target UpdateBuild => _ => _
        .Executes(() =>
        {
            ProcessTasks.StartShell(TemplateDirectory / "install.cmd").WaitForExit();

            foreach (var pluginDirectory in PluginDirectories)
            {
                if (pluginDirectory.Parent == TemplateDirectory)
                    continue;

                var resharperOnly = !pluginDirectory.ContainsFile("build.gradle")
                    ? "--resharper-only"
                    : string.Empty;
                DotNet(
                    $"new resharper-rider-plugin --force --build-only {resharperOnly}",
                    pluginDirectory);
            }
        });

    [Parameter] [Secret] readonly string GitHubToken;

    Target UpdateSdk => _ => _
        .Executes(() =>
        {
            foreach (var pluginDirectory in PluginDirectories)
                DotNetRun(_ => _
                    .SetProjectFile(ReSharperRiderUpdateProjectFile)
                    .SetProcessWorkingDirectory(pluginDirectory)
                    .SetApplicationArguments("update --root --no-logo")
                    .DisableProcessLogOutput()
                    .SetProcessEnvironmentVariable("GITHUB_TOKEN", GitHubToken));
        });

    Target Commit => _ => _
        .Executes(() =>
        {
            foreach (var pluginDirectory in PluginDirectories)
            {
                if (GitHasCleanWorkingCopy(pluginDirectory))
                    continue;

                DotNetRun(_ => _
                    .SetProjectFile(ReSharperRiderUpdateProjectFile)
                    .SetProcessWorkingDirectory(pluginDirectory)
                    .SetApplicationArguments("commit --root --no-logo")
                    .DisableProcessLogOutput());
            }
        });
}
