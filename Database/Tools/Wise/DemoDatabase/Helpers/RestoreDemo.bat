@echo off
::check CPU Type
if "%PROCESSOR_ARCHITECTURE%"=="x86" set ProgRoot=%ProgramFiles%
if not "%ProgramFiles(x86)%" == "" set ProgRoot=%ProgramFiles(x86)%

C:

net stop "Teleopti Service Bus"
net stop teleoptiEtlService

echo "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase"
cd "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase"

net start MSSQLSERVER

SQLCMD -S. -E -v BakDir = "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase" -i"RestoreDemo.sql"

"%ProgRoot%\Teleopti\DatabaseInstaller\DBManager.exe" -S. -DTeleoptiCCC7_Demo -OTeleoptiCCC7 -E -T -R -LTeleoptiDemoUser:TeleoptiDemoPwd2
"%ProgRoot%\Teleopti\DatabaseInstaller\DBManager.exe" -S. -DTeleoptiCCC7Agg_Demo -OTeleoptiCCCAgg -E -T -R -LTeleoptiDemoUser:TeleoptiDemoPwd2
"%ProgRoot%\Teleopti\DatabaseInstaller\DBManager.exe" -S. -DTeleoptiAnalytics_Demo -OTeleoptiAnalytics -E -T -R -LTeleoptiDemoUser:TeleoptiDemoPwd2

SQLCMD -S. -E -v BakDir = "%ProgRoot%\Teleopti\DatabaseInstaller\DemoDatabase" -i"RestoreUsers.sql"

"%ProgRoot%\Teleopti\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS. -DD"TeleoptiCCC7_Demo" -EE
echo re-add CrossDb views
"%ProgRoot%\Teleopti\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS. -DD"TeleoptiAnalytics_Demo" -CD"TeleoptiCCC7Agg_Demo" -EE

net start teleoptiEtlService
net start "Teleopti Service Bus"
