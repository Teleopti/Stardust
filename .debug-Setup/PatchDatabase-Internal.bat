@ECHO off
@ECHO off

::=======================
::Init
::=======================
::Get path to this batchfile
SET ROOTDIR=%~dp0
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
SET /A CROSSDB=0
SET /A ISCCC7=0
SET TRUNK=-T

COLOR A
cls

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

CD "%ROOTDIR%"

SET INSTANCE=%1
if "%1" == "" (
SET /P INSTANCE=Your SQL Server instance: 
)

CHOICE /C yn /M "Do you want to use WinAuth?"
IF %ERRORLEVEL% EQU 1 SET /a WinAuth=1
IF %ERRORLEVEL% EQU 2 SET /a WinAuth=0

::Check input
IF %WinAuth% equ 1 Call :WinAuth
IF %WinAuth% equ 0 Call :SQLAuth

SET DATABASEAPP=%2
if "%2" == "" (
SET /P DATABASEAPP=Application database name to patch:
)

SET DATABASEANAL=%3
if "%3" == "" (
SET /P DATABASEANAL=Analytics database name to patch:
)

SET DATABASEAGG=%4
if "%4" == "" (
SET /P DATABASEAGG=Agg database name to patch:
)

ECHO.
ECHO SQL Server instance:   %INSTANCE%
ECHO Application database:  %DATABASEAPP%
ECHO Analytics database:    %DATABASEANAL%
ECHO Agg database:          %DATABASEAGG%
ECHO.

::Build Teleopti.Support.Security.exe
ECHO Building %ROOTDIR%\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj
%MSBUILD% "%ROOTDIR%\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=12
GOTO :error
)

::Build DbManager
ECHO msbuild "%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" 
%MSBUILD% "%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% EQU 0 (
SET DATABASEPATH="%ROOTDIR%\..\Database"
SET DBMANAGER="%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\Debug\DBManager.exe"
SET DBMANAGERPATH="%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\Debug"
) else (
SET /A ERRORLEV=6
GOTO :error
)

::Patch DB
::Upgrade DB to latest version, we now always include trunk
CD "%DBMANAGERPATH%"
"%DBMANAGER%" -S%INSTANCE% %Conn1% -D%DATABASEAPP% -OTeleoptiCCC7 -R -T -LTeleoptiDemoUser:TeleoptiDemoPwd2 -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=2
GOTO :error
)
"%DBMANAGER%" -S%INSTANCE% %Conn1% -D%DATABASEANAL% -OTeleoptiAnalytics -R -T -LTeleoptiDemoUser:TeleoptiDemoPwd2 -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=2
GOTO :error
)
"%DBMANAGER%" -S%INSTANCE% %Conn1% -D%DATABASEAGG% -OTeleoptiCCCAgg -R -T -LTeleoptiDemoUser:TeleoptiDemoPwd2 -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=2
GOTO :error
)

CD "%ROOTDIR%"

ECHO Running: securityexe
"%ROOTDIR%\..\Teleopti.Support.Security\bin\debug\Teleopti.Support.Security.exe" -DS%INSTANCE% -AP"%DATABASEAPP%" -AN"%DATABASEANAL%" -CD"%DATABASEAGG%" %Conn2%
if %errorlevel% NEQ 0 (
SET /A ERRORLEV=1
GOTO :error


::Add license
IF %ISCCC7% EQU 1 Call :AddLic

ECHO.
ECHO upgrade successfull!
CD "%ROOTDIR%"
GOTO Finish

:AddLic
CHOICE /M "Add license?"
IF %ERRORLEVEL% EQU 1 (
ECHO SQLCMD -S%INSTANCE% -E -d"%DATABASEAPP%" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"
SQLCMD -S%INSTANCE% -E -d"%DATABASEAPP%" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"
)
exit /b

::Set Auth string
:WinAuth
SET Conn1=-E
SET Conn2=-EE
goto :eof

:SQLAuth
SET /P SQLLogin=SQL Login: 
SET /P SQLPwd=SQL password: 
SET Conn1=-U%SQLLogin% -P%SQLPwd%
SET Conn2=-DU%SQLLogin% -DP%SQLPwd%
goto :eof

:Error
CD "%ROOTDIR%"
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 1 ECHO Could not connect Mart to Agg: EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', '%Branch%_%Customer%_TeleoptiCCCAgg'
IF %ERRORLEV% EQU 2 ECHO DB have a release trunk or the database is out of version sync
IF %ERRORLEV% EQU 5 ECHO Could not create views in Mart: EXEC %Branch%_%Customer%_TeleoptiAnalytics.mart.sys_crossDatabaseView_load
IF %ERRORLEV% EQU 6 ECHO Could not build DBManager.exe & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 7 ECHO Could not build Teleopti.Support.Tool & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 8 ECHO An error occured while changing FirstDayInWeek on Person
IF %ERRORLEV% EQU 9 ECHO An error occured while changing to Date Only in Forecasts
IF %ERRORLEV% EQU 10 ECHO An error occured while encrypting
IF %ERRORLEV% EQU 11 ECHO Could not restore databases
IF %ERRORLEV% EQU 12 ECHO Could not build Teleopti.Support.Security & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 13 ECHO Could not apply a product activation key on demoreg database
IF %ERRORLEV% EQU 14 ECHO Could not create empty Analytics DB
IF %ERRORLEV% EQU 15 ECHO Could not create empty Agg DB
IF %ERRORLEV% EQU 17 ECHO Failed to update msgBroker setings in Analytics
ECHO.
ECHO --------
PAUSE
GOTO :EOF


:Finish
PAUSE
