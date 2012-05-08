@ECHO off

::=======================
::Init
::=======================
::Get path to this batchfile
SET ROOTDIR=%~dp0
SET ERRORLEVEL=0
SET CROSSDB=0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

SET /P MyServerInstance=Your SQL Server instance: 
SET /P DATABASE=Database name to patch: 

SET /P DATABASETYPE=Database type [TeleoptiCCC7,TeleoptiCCCAgg,TeleoptiAnalytics]: 

IF "%DATABASETYPE%"=="TeleoptiAnalytics" (
SET /P TeleoptiCCCAgg=Analytics is linked to which Agg-database?: 
SET CROSSDB=1
)

::Patch DB
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -E -D%DATABASE% -O%DATABASETYPE% -E -T
IF %ERRORLEVEL% NEQ 0 GOTO ERROR_Schema

IF "%DATABASETYPE%"=="TeleoptiCCC7" (
"%ROOTDIR%\Enrypted\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%DATABASE% -EE
)

IF %CROSSDB% EQU 1 (
ECHO Adding Crossdatabases
SQLCMD -S%MyServerInstance% -E -d%DATABASE% -Q"EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', '%TeleoptiCCCAgg%'"
if %errorlevel% NEQ 0 goto CROSSVIEW_LOAD
ECHO Adding views for Crossdatabases
SQLCMD -S%MyServerInstance% -E -d%DATABASE% -Q"EXEC mart.sys_crossDatabaseView_load"
if %errorlevel% NEQ 0 goto CROSSVIEW_LOAD
)


ECHO.
ECHO upgrade successfull!
GOTO Finish

:ERROR_Schema
ECHO Error deploying Schema objects for database: %DATABASE%!!
ECHO.
ECHO ---------
Echo Aborting ...
ECHO ---------
SET ERRORLEVEL=1
GOTO Finish

:CROSSVIEW_LOAD
ECHO Could not deploy crossDB views
ECHO Check that all need databases and remote table exist!
ECHO.
ECHO ---------
Echo Aborting ...
ECHO ---------
SET ERRORLEVEL=3
GOTO Finish

:Finish
PAUSE
