using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.IO.SerializationTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>();

    string PluginsFile => RootDirectory / "plugins.yml";
    IEnumerable<Plugin> Plugins => YamlDeserializeFromFile<List<Plugin>>(PluginsFile);

    Target Update => _ => _
        .DependsOn(CheckoutExternalRepositories)
        .Executes(() =>
        {
            var plugins = Plugins.Where(x => !x.name.StartsWith("_")).OrderBy(x => x.name).ToList();
            foreach (var plugin in plugins)
            {
                DotNetRun(_ => _
                    .SetProjectFile(ReSharperRiderUpdateProjectFile)
                    .SetProcessWorkingDirectory(GetPluginDirectory(plugin))
                    .SetApplicationArguments("compile --root --no-logo")
                    .DisableProcessLogOutput());
            }
        });

    AbsolutePath ReSharperRiderUpdateProjectFile
        => ExternalRepositoriesDirectory / "_ReSharperRiderUpdate" / "src" / "ReSharperRiderUpdate.csproj";
}