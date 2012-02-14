@ECHO off

::=======================
::Init
::=======================
::Get path to this batchfile
SET ROOTDIR=%~dp0
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
SET /A CROSSDB=0
SET /A ISCCC7=0

COLOR A
cls

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

CD "%ROOTDIR%"

SET /P MyServerInstance=Your SQL Server instance: 
SET /P DATABASE=Database name to patch: 

SET /P DATABASETYPE=Database type [TeleoptiCCC7,TeleoptiCCCAgg,TeleoptiAnalytics]: 
ECHO %DATABASETYPE% > "%temp%\DATABASETYPE.txt"

findstr /C:"TeleoptiAnalytics" /I "%temp%\DATABASETYPE.txt"
If %ERRORLEVEL% EQU 0 (
SET /A CROSSDB=1
SET /P TeleoptiCCCAgg=Analytics is linked to which Agg-database?: 
)

FINDSTR /C:"TeleoptiCCC7" /I "%temp%\DATABASETYPE.txt"
If %ERRORLEVEL% EQU 0 (
SET /A ISCCC7=1
)

::Build Teleopti.Support.Security.exe
ECHO Building %ROOTDIR%\..\..\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj
%MSBUILD% "%ROOTDIR%\..\..\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=12
GOTO :error
)

::Build DbManager
ECHO msbuild "%ROOTDIR%\..\..\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" 
%MSBUILD% "%ROOTDIR%\..\..\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% EQU 0 (
SET DBMANAGER="%ROOTDIR%\..\..\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\Debug\DBManager.exe"
SET DBMANAGERPATH="%ROOTDIR%\..\..\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\Debug"
) else (
SET /A ERRORLEV=6
GOTO :error
)

::Patch DB
::Upgrade DB to latest version (WITHOUT Trunk)
CD "%DBMANAGERPATH%"
ECHO "%DBMANAGER%" -S%MyServerInstance% -E -D%DATABASE% -O%DATABASETYPE% -E
"%DBMANAGER%" -S%MyServerInstance% -E -D%DATABASE% -O%DATABASETYPE% -E
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=2
GOTO :error
)

CD "%ROOTDIR%"

IF %ISCCC7% EQU 1 (
ECHO Encrypting passwords ...
"%ROOTDIR%\..\..\..\Teleopti.Support.Security\bin\debug\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%DATABASE% -EE
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=10
GOTO :error
)

ECHO Changing to Date Only in Forecasts ...
"%ROOTDIR%\..\..\..\Teleopti.Support.Security\bin\debug\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%DATABASE% -FM -EE
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=9
GOTO :error
)

ECHO Changing FirstDayInWeek on Person ...
"%ROOTDIR%\..\..\..\Teleopti.Support.Security\bin\debug\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%DATABASE% -PU -EE
IF %ERRORLEVEL% NEQ 0 (
SET /A ERRORLEV=8
GOTO :error
)
)

IF %CROSSDB% EQU 1 (
ECHO Adding Crossdatabases
SQLCMD -S%MyServerInstance% -E -d%DATABASE% -Q"EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', '%TeleoptiCCCAgg%'"
if %errorlevel% NEQ 0 (
SET /A ERRORLEV=1
GOTO :error
)
ECHO Adding views for Crossdatabases
SQLCMD -S%MyServerInstance% -E -d%DATABASE% -Q"EXEC mart.sys_crossDatabaseView_load"
if %errorlevel% NEQ 0 (
SET /A ERRORLEV=5
GOTO :Error
)
)

ECHO.
ECHO upgrade successfull!
GOTO Finish

:Error
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
IF %ERRORLEV% EQU 13 ECHO Could not apply license on demoreg database
IF %ERRORLEV% EQU 14 ECHO Could not create empty Analytics DB
IF %ERRORLEV% EQU 15 ECHO Could not create empty Agg DB
IF %ERRORLEV% EQU 17 ECHO Failed to update msgBroker setings in Analytics
ECHO.
ECHO --------
PAUSE
GOTO :EOF


:Finish
PAUSE
