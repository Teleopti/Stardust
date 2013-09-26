@ECHO off

SET CCNetWorkDir=%~1
SET TargetDir=%~2
SET DB_CCC7=%~3
SET DB_ANALYTICS=%~4
SET config=%~5
SET AppSqlLogin=%~6
SET AppSqlPwd=%~7

SET SQL_AUTH_STRING=Data Source=%computername%;User Id=%AppSqlLogin%;Password=%AppSqlPwd%
SET ETL_SERVICE_nhibConfPath=%TargetDir%

::Delete target Dir
ECHO RMDIR "%TargetDir%" /S /Q
RMDIR %TargetDir% /S /Q

::Create Dir 
MKDIR %TargetDir%

::replace dbnames
ECHO "%CCNetWorkDir%\.debug-Setup\FixMyConfig.bat" "%DB_CCC7%" "%DB_ANALYTICS%"
CALL "%CCNetWorkDir%\.debug-Setup\FixMyConfig.bat" "%DB_CCC7%" "%DB_ANALYTICS%"

::fix some special stuff
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" "c:\nhib" "%TargetDir%" "%CCNetWorkDir%\Teleopti.Support.Tool\bin\%config%\settings.txt"
cscript "%CCNetWorkDir%\ccnet\ETLNightlyBuild\replace.vbs" "Data Source=.;Integrated Security=SSPI" "%SQL_AUTH_STRING%" "%CCNetWorkDir%\Teleopti.Support.Tool\bin\%config%\settings.txt"

ECHO "%CCNetWorkDir%\Teleopti.Support.Tool\bin\%config%\Teleopti.Support.Tool.exe" -MODebug
"%CCNetWorkDir%\Teleopti.Support.Tool\bin\%config%\Teleopti.Support.Tool.exe" -MODebug

