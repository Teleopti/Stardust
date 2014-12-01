SETLOCAL
SET ROOTDIR=%~dp0

@ECHO off

SET WorkingDirectory=%~1
SET TargetDir=%~2
SET DB_CCC7=%~3
SET DB_ANALYTICS=%~4
SET config=%~5
SET AppSqlLogin=%~6
SET AppSqlPwd=%~7
SET SqlInstanceName=%~8

SET SQL_AUTH_STRING=Data Source=%SqlInstanceName%;User Id=%AppSqlLogin%;Password=%AppSqlPwd%
SET ETL_SERVICE_nhibConfPath=%TargetDir%

::replace dbnames
ECHO "%WorkingDirectory%\.debug-Setup\FixMyConfig.bat" "%DB_CCC7%" "%DB_ANALYTICS%" %config%
CALL "%WorkingDirectory%\.debug-Setup\FixMyConfig.bat" "%DB_CCC7%" "%DB_ANALYTICS%" %config%

::fix some special stuff
cscript "%ROOTDIR%replace.vbs" "c:\nhib" "%TargetDir%" "%WorkingDirectory%\Teleopti.Support.Tool\bin\%config%\settings.txt"
cscript "%ROOTDIR%replace.vbs" "Data Source=.;Integrated Security=SSPI" "%SQL_AUTH_STRING%" "%WorkingDirectory%\Teleopti.Support.Tool\bin\%config%\settings.txt"

ECHO.
"%WorkingDirectory%\Teleopti.Support.Tool\bin\%config%\Teleopti.Support.Tool.exe" -MODebug
ENDLOCAL