@echo off
::check CPU Type
if "%PROCESSOR_ARCHITECTURE%"=="x86" set ProgRoot=%ProgramFiles%
if not "%ProgramFiles(x86)%" == "" set ProgRoot=%ProgramFiles(x86)%

C:

net stop "Teleopti Service Bus"
net stop teleoptiEtlService
net stop "Teleopti Message Broker"

echo "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase"
cd "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase"

net start MSSQLSERVER

SQLCMD -S. -E -v BakDir = "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase" -i"RestoreDemo.sql"
SQLCMD -S. -E -v BakDir = "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase" -i"RestoreUsers.sql"

"%ProgRoot%\Teleopti\DatabaseInstaller\DBManager.exe" -S. -DTeleoptiCCC7_Demo -OTeleoptiCCC7 -E -T
"%ProgRoot%\Teleopti\DatabaseInstaller\DBManager.exe" -S. -DTeleoptiCCC7Agg_Demo -OTeleoptiCCCAgg -E -T
"%ProgRoot%\Teleopti\DatabaseInstaller\DBManager.exe" -S. -DTeleoptiAnalytics_Demo -OTeleoptiAnalytics -E -T

SQLCMD -S. -E -dTeleoptiAnalytics_Demo -Q"EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', 'TeleoptiCCC7Agg_Demo'"
SQLCMD -S. -E -dTeleoptiAnalytics_Demo -Q"EXEC mart.sys_crossDatabaseView_load"

"%ProgRoot%\Teleopti\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS. -DDTeleoptiCCC7_Demo -EE

net start "Teleopti Message Broker"
net start teleoptiEtlService
net start "Teleopti Service Bus"
