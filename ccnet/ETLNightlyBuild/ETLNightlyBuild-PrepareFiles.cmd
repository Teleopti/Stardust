@ECHO off

SET CCNetWorkDir=%~1
SET TargetDir=%~2
SET CCC7DB=%~3
SET AnalyticsDB=%~4
SET config=%~5
SET AppSqlLogin=%~6
SET AppSqlPwd=%~7

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
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" $(SitePath) "%TargetDir%" "%TargetDir%\Teleopti.Analytics.Etl.ServiceHost.exe.config"

::lowered permission account
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" "Integrated Security=True" "User Id=%AppSqlLogin%;Password=%AppSqlPwd%" "%TargetDir%\Teleopti.Analytics.Etl.ServiceHost.exe.config"
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" "Integrated Security=True" "User Id=%AppSqlLogin%;Password=%AppSqlPwd%" "%TargetDir%\TeleoptiCCC7.nhib.xml"