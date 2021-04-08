using System.Linq;
using Nuke.Common;
using static Nuke.Common.IO.TextTasks;
using static Nuke.Common.Utilities.TemplateUtility;

partial class Build
{
    string ReadmeFile => RootDirectory / "README.md";

    Target UpdateReadme => _ => _
        .Executes(() =>
        {
            var content = ReadAllLines(ReadmeFile).ToList();
            var table =
                new[]
                {
                    "| Name   | Latest ReSharper Version | Latest Rider Version |",
                    "|:-------|:------------------------:|:--------------------:|"
                }.Concat(Plugins
                    .Where(x => !x.name.StartsWith("_"))
                    .OrderBy(x => x.name)
                    .Select(x => $"| {x.GetRepositoryLink()} | {x.GetReSharperBadge()} | {x.GetRiderBadge()} |"));
            ExtractAndRemoveRegions(content, "<!-- BEGIN: TABLE", "<!-- END: TABLE");
            AddRegion(content, "<!-- BEGIN: TABLE -->", table);

            WriteAllLines(ReadmeFile, content);
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
            return resharper != null
                ? $"[![ReSharper](https://img.shields.io/jetbrains/plugin/v/{resharper}.svg?label=)](https://plugins.jetbrains.com/plugin/{resharper})"
                : string.Empty;
        }

        public string GetRiderBadge()
        {
            return rider != null
                ? $"[![Rider](https://img.shields.io/jetbrains/plugin/v/{rider}.svg?label=)](https://plugins.jetbrains.com/plugin/{rider})"
                : string.Empty;
        }
    }
}