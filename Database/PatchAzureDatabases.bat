@ECHO off

SET ROOTDIR=%~dp0
::SET CROSSDB=0
SET MyServerInstance=%1
SET AppDB=%2
SET AnalyticsDB=%3
::SET TeleoptiCCCAgg=TeleoptiAnalytics_Demo
SET SQLAdmin=%4
SET SQLAdminPwd=%5
SET SQLAppUser=%6
SET SQLAppPwd=%7

SET Conn1=-U%SQLAdmin% -P%SQLAdminPwd%
SET Conn2=-DU%SQLAdmin% -DP%SQLAdminPwd%
SET Conn3=-R -L"%SQLAppUser%:%SQLAppPwd%"

::Patch Databases
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%AppDB% -OTeleoptiCCC7 %Conn1% -T %Conn3%
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%AnalyticsDB% -OTeleoptiAnalytics %Conn1% -T %Conn3%

::Security stuff
"%ROOTDIR%\Enrypted\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%AppDB% %Conn2%

ECHO Upgrade of databases successful!



