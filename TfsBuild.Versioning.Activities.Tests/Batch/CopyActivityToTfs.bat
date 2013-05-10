tf checkout "D:\_TfsProjects\BuildActivities\Custom Activity Storage\TfsBuild.Versioning.Activities.dll"

del /F "D:\_TfsProjects\BuildActivities\Custom Activity Storage\TfsBuild.Versioning.Activities.dll"

copy /Y /B "D:\_TfsProjects\BuildActivities\SolutionBuildVersioning\Dev\Version 1.5.0.0\TfsBuild.Versioning.Activities\bin\Debug\TfsBuild.Versioning.Activities.dll" "D:\_TfsProjects\BuildActivities\Custom Activity Storage\TfsBuild.Versioning.Activities.dll"

tf checkin /noprompt /comment:"Updating Activities" "D:\_TfsProjects\BuildActivities\Custom Activity Storage\TfsBuild.Versioning.Activities.dll"
