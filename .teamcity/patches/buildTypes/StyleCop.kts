package patches.buildTypes

import jetbrains.buildServer.configs.kotlin.v2018_1.*
import jetbrains.buildServer.configs.kotlin.v2018_1.ui.*

/*
This patch script was generated by TeamCity on settings change in UI.
To apply the patch, change the buildType with id = 'StyleCop'
accordingly, and delete the patch script.
*/
changeBuildType(RelativeId("StyleCop")) {
    requirements {
        add {
            matches("teamcity.agent.jvm.os.family", "Windows")
        }
    }
}
