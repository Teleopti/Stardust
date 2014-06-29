@echo off
NET START | FINDSTR /C:"Teleopti Service Bus" /I
if %ERRORLEVEL% EQU 0 net stop "Teleopti Service Bus"

NET START | FINDSTR /C:"teleoptiEtlService" /I
if %ERRORLEVEL% EQU 0 net stop "teleoptiEtlService"

set x86RegKey=HKEY_LOCAL_MACHINE\SOFTWARE\Teleopti\TeleoptiCCC\InstallationSettings
set x64RegKey=HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings

if "%PROCESSOR_ARCHITECTURE%"=="AMD64" set confirmedPath=%x64RegKey%
if "%PROCESSOR_ARCHITECTURE%"=="x86" set confirmedPath=%x86RegKey%

set INSTALLDIR=
for /f "tokens=2,*" %%a in ('reg query "%confirmedPath%" /v "INSTALLDIR" 2^>NUL ^| findstr INSTALLDIR') do set INSTALLDIR=%%b

set SQL_SERVER_NAME=
for /f "tokens=2*" %%A in ('REG QUERY "%confirmedPath%" /v SQL_SERVER_NAME') DO (
  for %%F in (%%B) do (
    set SQL_SERVER_NAME=%%F
  )
)

SET INSTANCE_NAME=
FOR /f "tokens=2 delims=\" %%G IN ('"ECHO %SQL_SERVER_NAME%"') DO SET INSTANCE_NAME=%%G
IF "%INSTANCE_NAME%"=="" SET INSTANCE_NAME=MSSQLSERVER

NET START | FINDSTR /C:"SQL Server (%INSTANCE_NAME%)" /I
if %ERRORLEVEL% NEQ 0 net start "SQL Server (%INSTANCE_NAME%)"


SQLCMD -S%SQL_SERVER_NAME% -E -v BakDir = "%INSTALLDIR%\DatabaseInstaller\DemoDatabase" -i"%INSTALLDIR%\DatabaseInstaller\DemoDatabase\RestoreDemo.sql"

"%INSTALLDIR%\DatabaseInstaller\DBManager.exe" -S%SQL_SERVER_NAME% -DTeleoptiApp_Demo -OTeleoptiCCC7 -E -T -R -LTeleoptiDemoUser:TeleoptiDemoPwd2
"%INSTALLDIR%\DatabaseInstaller\DBManager.exe" -S%SQL_SERVER_NAME% -DTeleoptiAgg_Demo -OTeleoptiCCCAgg -E -T -R -LTeleoptiDemoUser:TeleoptiDemoPwd2
"%INSTALLDIR%\DatabaseInstaller\DBManager.exe" -S%SQL_SERVER_NAME% -DTeleoptiAnalytics_Demo -OTeleoptiAnalytics -E -T -R -LTeleoptiDemoUser:TeleoptiDemoPwd2

SQLCMD -S%SQL_SERVER_NAME% -E -v BakDir = "%INSTALLDIR%\DatabaseInstaller\DemoDatabase" -i"%INSTALLDIR%\DatabaseInstaller\DemoDatabase\RestoreUsers.sql" -v CurrentUser=""

"%INSTALLDIR%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS%SQL_SERVER_NAME% -DD"TeleoptiApp_Demo" -EE
echo re-add CrossDb views
"%INSTALLDIR%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS%SQL_SERVER_NAME% -DD"TeleoptiAnalytics_Demo" -CD"TeleoptiAgg_Demo" -EE

net start teleoptiEtlService
net start "Teleopti Service Bus"
