using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.IO;

partial class Build
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>();

    string PluginsFile => RootDirectory / "plugins.yml";
    IEnumerable<Plugin> Plugins => SerializationTasks.YamlDeserializeFromFile<List<Plugin>>(PluginsFile);

    Target Compile => _ => _
        .DependsOn(CheckoutExternalRepositories)
        .Produces(ExternalRepositoriesDirectory / "*" / "output" / "*.zip")
        .Produces(ExternalRepositoriesDirectory / "*" / "output" / "*.nupkg")
        .Executes(() =>
        {
            foreach (var plugin in Plugins.Where(x => !x.name.StartsWith("_")))
            {
                TeamCity.Instance.SetBuildStatus($"{plugin.name}:", prepend: true);
                using (Logger.Block(plugin.name))
                {
                    DotNetRun(_ => _
                        .SetProjectFile(ReSharperRiderUpdateProjectFile)
                        .SetProcessWorkingDirectory(GetPluginDirectory(plugin))
                        .SetApplicationArguments("compile --root --no-logo"));
                }
            }
        });

    AbsolutePath ReSharperRiderUpdateProjectFile
        => ExternalRepositoriesDirectory / "_ReSharperRiderUpdate" / "src" / "ReSharperRiderUpdate.csproj";
}