// ------------------------------------------------------------------------------
// <auto-generated>
//
//     This code was generated.
//
//     - To turn off auto-generation set:
//
//         [TeamCity (AutoGenerate = false)]
//
//     - To trigger manual generation invoke:
//
//         nuke --generate-configuration TeamCity --host TeamCity
//
// </auto-generated>
// ------------------------------------------------------------------------------

import jetbrains.buildServer.configs.kotlin.v2018_1.*
import jetbrains.buildServer.configs.kotlin.v2018_1.buildFeatures.*
import jetbrains.buildServer.configs.kotlin.v2018_1.buildSteps.*
import jetbrains.buildServer.configs.kotlin.v2018_1.triggers.*
import jetbrains.buildServer.configs.kotlin.v2018_1.vcs.*
import jetbrains.buildServer.configs.kotlin.v2018_1.ui.*

version = "2020.2"

val projects = listOf(
    Pair("CognitiveComplexity", "https://github.com/matkoch/resharper-cognitivecomplexity"),
    Pair("CyclomaticComplexity", "https://github.com/JetBrains/resharper-cyclomatic-complexity/"),
    Pair("FluentAssertions", "https://github.com/matkoch/resharper-fluentassertions/"),
    Pair("FluentValidation", "https://github.com/matkoch/resharper-fluentvalidation/"),
    Pair("HeapView", "https://github.com/controlflow/resharper-heapview/"),
    Pair("InternalsVisibleTo", "https://github.com/matkoch/resharper-internalsvisibleto/"),
    Pair("NUKE", "https://github.com/nuke-build/resharper/"),
    Pair("PresentationAssistant", "https://github.com/JetBrains/resharper-presentation-assistant/"),
    Pair("Statiq", "https://github.com/matkoch/resharper-statiq/"),
    Pair("StyleCop", "https://github.com/StyleCop/StyleCop.ReSharper/"),
    Pair("Verify", "https://github.com/matkoch/resharper-verify/"),
    Pair("Xao", "https://github.com/matkoch/resharper-xao/"),
)

project {
    for (project in projects) {
        val pluginVcsRoot = GitVcsRoot {
            id("${project.first}VcsRoot")
            name = "${project.second}#refs/heads/master"
            url = project.second
            branch = "refs/heads/master"
            pollInterval = 60
        }
        val pluginBuildType = BuildType {
            id(project.first)
            name = project.first
            vcs {
                root(pluginVcsRoot)
                cleanCheckout = true
            }
            artifactRules = """
                output/*.zip
                output/*.nupkg
            """.trimIndent()
            steps {
                powerShell {
                    name = "Publish"
                    scriptMode = script {
                        content = """
                            ${'$'}version=${'$'}(git describe --abbrev=0 %build.vcs.number% --tags)
                            echo "##teamcity[buildNumber '${'$'}version']"
                            if (Test-Path "gradlew.bat") {
                                & gradle.bat :publishPlugin -PPluginVersion="${'$'}version" -PPublishToken="%env.PublishToken%"
                            } else {
                                & ./publishPlugin.ps1 -Version ${'$'}version -ApiKey %env.PublishToken%
                            }
                            exit ${'$'}LASTEXITCODE
                        """.trimIndent()
                    }
                    conditions { contains("teamcity.agent.jvm.os.name", "Windows") }
                }
                script {
                    name = "Publish"
                    scriptContent = """
                        export version=`git describe --abbrev=0 %build.vcs.number% --tags`
                        echo "##teamcity[buildNumber '${'$'}version']"
                        ./gradlew :publishPlugin -PPluginVersion=${'$'}version -PPublishToken=%env.PublishToken%
                    """.trimIndent()
                    conditions { doesNotContain("teamcity.agent.jvm.os.name", "Windows") }
                }
            }
            params {
                text(
                    "teamcity.ui.runButton.caption",
                    "Publish",
                    display = ParameterDisplay.HIDDEN
                )
            }
            triggers {
                vcs {
                    triggerRules = "+:**"
                    branchFilter = "+:refs/tags/*"
                }
            }
        }

        vcsRoot(pluginVcsRoot)
        buildType(pluginBuildType)
    }

    params {
        password (
            "env.PublishToken",
            label = "PublishToken",
            value = "",
            display = ParameterDisplay.PROMPT)
        text(
            "teamcity.runner.commandline.stdstreams.encoding",
            "UTF-8",
            display = ParameterDisplay.HIDDEN
        )
    }
}