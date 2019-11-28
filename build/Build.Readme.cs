using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities;

partial class Build
{
    string ReadmeFile => RootDirectory / "README.md";

    Target UpdateReadme => _ => _
        .Executes(() =>
        {
            var content = TextTasks.ReadAllLines(ReadmeFile).ToList();
            var table =
                new[]
                {
                    "| Name   | Latest ReSharper Version | Latest Rider Version |",
                    "|:-------|:------------------------:|:--------------------:|"
                }.Concat(Plugins
                    .Where(x => !x.name.StartsWith("_"))
                    .OrderBy(x => x.name)
                    .Select(x => $"| {x.GetRepositoryLink()} | {x.GetReSharperBadge()} | {x.GetRiderBadge()} |"));
            TemplateUtility.ExtractAndRemoveRegions(content, "<!-- BEGIN: TABLE", "<!-- END: TABLE");
            TemplateUtility.AddRegion(content, "<!-- BEGIN: TABLE -->", table);

            TextTasks.WriteAllLines(ReadmeFile, content);
        });

    public class Plugin
    {
        public string name { get; set; }
        public string repository { get; set; }
        public string resharper { get; set; }
        public string rider { get; set; }

        public string GetRepositoryLink() => $"[{name}]({repository})";

        public string GetReSharperBadge()
        {
            var badge = $"![ReSharper](https://img.shields.io/resharper/v/{resharper}.svg?label=)";
            return $"[{badge}](https://resharper-plugins.jetbrains.com/packages/{resharper})";
        }

        public string GetRiderBadge()
        {
            var badge = $"![ReSharper](https://img.shields.io/jetbrains/plugin/v/{rider}.svg?label=)";
            return $"[{badge}](https://plugins.jetbrains.com/plugin/{rider})";
        }
    }
}