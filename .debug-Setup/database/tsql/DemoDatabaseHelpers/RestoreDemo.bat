@echo off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

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
FOR /f "tokens=2 delims=\" %%G IN ('"ECHO %INSTANCE%"') DO SET INSTANCE_NAME=%%G
IF "%INSTANCE_NAME%"=="" SET INSTANCE_NAME=MSSQLSERVER

NET START | FINDSTR /C:"SQL Server (%INSTANCE_NAME%)" /I
if %ERRORLEVEL% NEQ 0 net start "SQL Server (%INSTANCE_NAME%)"

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

set INSTANCE=
for /f "tokens=2*" %%A in ('REG QUERY "%confirmedPath%" /v SQL_SERVER_NAME') DO (
  for %%F in (%%B) do (
    set INSTANCE=%%F
  )
)

SET INSTANCE_NAME=
FOR /f "tokens=2 delims=\" %%G IN ('"ECHO %INSTANCE%"') DO SET INSTANCE_NAME=%%G
IF "%INSTANCE_NAME%"=="" SET INSTANCE_NAME=MSSQLSERVER

NET START | FINDSTR /C:"SQL Server (%INSTANCE_NAME%)" /I
if %ERRORLEVEL% NEQ 0 net start "SQL Server (%INSTANCE_NAME%)"

SET TELEOPTIANALYTICS_BAKFILE=%ROOTDIR%\..\TeleoptiAnalytics_Demo.bak
SET TELEOPTIAGG_BAKFILE=%ROOTDIR%\..\TeleoptiCCC7Agg_Demo.bak
SET TELEOPTICCC_BAKFILE=%ROOTDIR%\..\TeleoptiCCC7_Demo.bak
SET TELEOPTIANALYTICS=TeleoptiAnalytics_Demo
SET TELEOPTIAGG=TeleoptiCCC7Agg_Demo
SET TELEOPTICCC=TeleoptiCCC7_Demo
SET SQLLogin=TeleoptiDemoUser
SET SQLPwd=TeleoptiDemoPwd2
cls
ECHO restoring databases ...
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\..\RestoreDatabase.sql" -v BAKFILE="%TELEOPTICCC_BAKFILE%" -v DATAFOLDER="xp_instance_regread" -v DATABASENAME="%TELEOPTICCC%" > "%ROOTDIR%\..\restoreDB.log"
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\..\RestoreAnalytics.sql" -v BAKFILE="%TELEOPTIANALYTICS_BAKFILE%" -v DATAFOLDER="xp_instance_regread" -v DATABASENAME="%TELEOPTIANALYTICS%" >> "%ROOTDIR%\..\restoreDB.log"
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\..\RestoreDatabase.sql" -v BAKFILE="%TELEOPTIAGG_BAKFILE%" -v DATAFOLDER="xp_instance_regread" -v DATABASENAME="%TELEOPTIAGG%"  >> "%ROOTDIR%\..\restoreDB.log"
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\..\CreateLoginDropUsers.sql" -v TELEOPTICCC="%TELEOPTICCC%" -v TELEOPTIANALYTICS="%TELEOPTIANALYTICS%" -v TELEOPTIAGG="%TELEOPTIAGG%" -v SQLLogin="%SQLLogin%" -v SQLPwd="%SQLPwd%"  >> "%ROOTDIR%\..\restoreDB.log"
ECHO restoring databases. Done!
ECHO.
ECHO patching databases ...
"%INSTALLDIR%\DatabaseInstaller\DBManager.exe" -S%INSTANCE% -DTeleoptiCCC7_Demo -OTeleoptiCCC7 -E -T -R -LTeleoptiDemoUser:TeleoptiDemoPwd2 > NUL
"%INSTALLDIR%\DatabaseInstaller\DBManager.exe" -S%INSTANCE% -DTeleoptiCCC7Agg_Demo -OTeleoptiCCCAgg -E -T -R -LTeleoptiDemoUser:TeleoptiDemoPwd2 > NUL
"%INSTALLDIR%\DatabaseInstaller\DBManager.exe" -S%INSTANCE% -DTeleoptiAnalytics_Demo -OTeleoptiAnalytics -E -T -R -LTeleoptiDemoUser:TeleoptiDemoPwd2 > NUL
"%INSTALLDIR%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%TELEOPTICCC%" -EE > NUL
"%INSTALLDIR%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%TELEOPTIANALYTICS%" -CD"%TELEOPTIAGG%" -EE > NUL
ECHO patching databases. Done!
ECHO.
ECHO fix data for Demo ...
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\..\AddingTeleoptiPermissions.sql" -v TELEOPTICCC="%TELEOPTICCC%" -v TELEOPTIANALYTICS="%TELEOPTIANALYTICS%" -v TELEOPTIAGG="%TELEOPTIAGG%"
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\..\MoveDataInDemo.sql" -v TELEOPTICCC="%TELEOPTICCC%" -v TELEOPTIANALYTICS="%TELEOPTIANALYTICS%" -v TELEOPTIAGG="%TELEOPTIAGG%" > NUL
ECHO fix data for Demo. Done!
ECHO.
net start teleoptiEtlService
net start "Teleopti Service Bus"