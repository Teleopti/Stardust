@ECHO off

SET CCNetWorkDir=%~1
SET TargetDir=%~2
SET CCC7DB=%~3
SET AnalyticsDB=%~4
SET config=%~5

::Delete target Dir
ECHO RMDIR "%TargetDir%" /S /Q
RMDIR %TargetDir% /S /Q

::Create Dir 
MKDIR %TargetDir%

::Copy the files into TargetDir
ECHO ROBOCOPY "%CCNetWorkDir%\Teleopti.Analytics\Teleopti.Analytics.Etl.ServiceHost\bin\%config%" "%TargetDir%" /E
ROBOCOPY "%CCNetWorkDir%\Teleopti.Analytics\Teleopti.Analytics.Etl.ServiceHost\bin\%config%" "%TargetDir%" /E

::Del TFS Config
DEL "%TargetDir%\Teleopti.Analytics.Etl.ServiceHost.exe.config" /Q
DEL "%TargetDir%\test.nhib.xml" /Q

::Get Prepared config for the restored DBs
COPY "%CCNetWorkDir%\BuildArtifacts\AppETLService.config" "%TargetDir%\Teleopti.Analytics.Etl.ServiceHost.exe.config" /Y
COPY "%CCNetWorkDir%\BuildArtifacts\TeleoptiCCC7.nhib.xml" "%TargetDir%\TeleoptiCCC7.nhib.xml" /Y

::replace dbnames
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" $(CCC7DB) %CCC7DB% "%TargetDir%\TeleoptiCCC7.nhib.xml"
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" $(AnalyticsDB) %AnalyticsDB% "%TargetDir%\TeleoptiCCC7.nhib.xml"
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" $(AnalyticsDB) %AnalyticsDB% "%TargetDir%\Teleopti.Analytics.Etl.ServiceHost.exe.config"