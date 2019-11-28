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

}