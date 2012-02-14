@echo off
::check CPU Type
if "%PROCESSOR_ARCHITECTURE%"=="x86" set ProgRoot=%ProgramFiles%
if not "%ProgramFiles(x86)%" == "" set ProgRoot=%ProgramFiles(x86)%

C:

net stop teleoptiRtaService
net stop teleoptiEtlService
net stop "Teleopti Message Broker"

echo "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase"
cd "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase"

net start MSSQLSERVER

SQLCMD -S. -E -v BakDir = "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase" -i"RestoreDemo.sql"
SQLCMD -S. -E -v BakDir = "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase" -i"RestoreUsers.sql"

"%ProgRoot%\Teleopti\DatabaseInstaller\DBManager.exe" -S. -DTeleoptiCCC7_Demo -OTeleoptiCCC7 -E
"%ProgRoot%\Teleopti\DatabaseInstaller\DBManager.exe" -S. -DTeleoptiCCC7Agg_Demo -OTeleoptiCCCAgg -E
"%ProgRoot%\Teleopti\DatabaseInstaller\DBManager.exe" -S. -DTeleoptiAnalytics_Demo -OTeleoptiAnalytics -E

net start "Teleopti Message Broker"
net start teleoptiEtlService
net start teleoptiRtaService