using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Utilities.Text.Yaml;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static Nuke.Common.Utilities.TemplateUtility;

partial class Build
{
    public Build()
    {
        YamlExtensions.DefaultDeserializerBuilder = new DeserializerBuilder()
            .WithNamingConvention(LowerCaseNamingConvention.Instance);
    }

    AbsolutePath ReadmeFile => RootDirectory / "README.md";

    Target UpdateReadme => _ => _
        .Executes(() =>
        {
            var content = ReadmeFile.ReadAllLines().ToList();
            var table =
                new[]
                {
                    "| Name   | Latest ReSharper Version | Latest Rider Version |",
                    "|:-------|:------------------------:|:--------------------:|"
                }.Concat(Plugins
                    .Where(x => !x.Name.StartsWith("_"))
                    .OrderBy(x => x.Name)
                    .Select(x => $"| {x.GetRepositoryLink()} | {x.GetReSharperBadge()} | {x.GetRiderBadge()} |"));
            ExtractAndRemoveRegions(content, "<!-- BEGIN: TABLE", "<!-- END: TABLE");
            AddRegion(content, "<!-- BEGIN: TABLE -->", table);

            ReadmeFile.WriteAllLines(content);
        });

    public class Plugin
    {
        public string Name;
        public string Repository;
        public string ReSharper;
        public string Rider;

        public string GetRepositoryLink() => $"[{Name}]({Repository})";

        public string GetReSharperBadge() =>
            ReSharper != null
                ? $"[![ReSharper](https://img.shields.io/jetbrains/plugin/v/{ReSharper}.svg?label=)](https://plugins.jetbrains.com/plugin/{ReSharper})"
                : string.Empty;

        public string GetRiderBadge() =>
            Rider != null
                ? $"[![Rider](https://img.shields.io/jetbrains/plugin/v/{Rider}.svg?label=)](https://plugins.jetbrains.com/plugin/{Rider})"
                : string.Empty;
    }
}
