package patches.projects

import jetbrains.buildServer.configs.kotlin.v2018_1.*
import jetbrains.buildServer.configs.kotlin.v2018_1.Project
import jetbrains.buildServer.configs.kotlin.v2018_1.ui.*

/*
This patch script was generated by TeamCity on settings change in UI.
To apply the patch, change the root project
accordingly, and delete the patch script.
*/
changeProject(DslContext.projectId) {
    params {
        expect {
            password("env.PublishToken", "", label = "PublishToken", display = ParameterDisplay.PROMPT)
        }
        update {
            password("env.PublishToken", "credentialsJSON:ae5da7cf-d9d2-4289-b1f2-98cd069d198d", label = "PublishToken", display = ParameterDisplay.PROMPT)
        }
    }
}
