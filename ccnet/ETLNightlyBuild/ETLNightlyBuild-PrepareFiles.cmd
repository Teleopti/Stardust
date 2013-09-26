@ECHO off

SET CCNetWorkDir=%~1
SET TargetDir=%~2
SET DB_CCC7=%~3
SET DB_ANALYTICS=%~4
SET config=%~5
SET AppSqlLogin=%~6
SET AppSqlPwd=%~7

SET SQL_AUTH_STRING=Data Source=%computername%;User Id=%AppSqlLogin%;Password=%AppSqlPwd%

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
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" "$(DB_CCC7)" "%DB_CCC7%" "%TargetDir%\TeleoptiCCC7.nhib.xml"
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" "$(DB_ANALYTICS)" "%DB_ANALYTICS%" "%TargetDir%\TeleoptiCCC7.nhib.xml"
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" "$(SQL_AUTH_STRING)" "%SQL_AUTH_STRING%" "%TargetDir%\TeleoptiCCC7.nhib.xml"

cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" "$(DB_ANALYTICS)" "%DB_ANALYTICS%" "%TargetDir%\Teleopti.Analytics.Etl.ServiceHost.exe.config"
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" "$(ETL_SERVICE_nhibConfPath)" "%TargetDir%" "%TargetDir%\Teleopti.Analytics.Etl.ServiceHost.exe.config"
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" "$(SQL_AUTH_STRING)" "%SQL_AUTH_STRING%" "%TargetDir%\Teleopti.Analytics.Etl.ServiceHost.exe.config"
