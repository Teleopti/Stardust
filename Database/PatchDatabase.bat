@ECHO off

::=======================
::Init
::=======================
::Get path to this batchfile
SET ROOTDIR=%~dp0
SET ERRORLEVEL=0
SET CROSSDB=0
SET /A Silent=0
SET MyServerInstance=%1
SET DATABASE=%2
SET DATABASETYPE=%3
SET TeleoptiCCCAgg=%4
SET Conn1=-EE
SET Conn2=-E

IF NOT "%MyServerInstance%"=="" SET /A Silent=1
IF %Silent% equ 1 GOTO Execute

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

IF "%MyServerInstance%"=="" SET /P MyServerInstance=Your SQL Server instance:

CHOICE /C yn /M "Do you want to use WinAuth?"
IF ERRORLEVEL 1 SET /a WinAuth=1
IF ERRORLEVEL 2 SET /a WinAuth=0

::Check input
IF %WinAuth% equ 1 GOTO WinAuth
IF %WinAuth% equ 0 GOTO SQLAuth

::Set Auth string
:WinAuth
SET Conn1=-EE
SET Conn2=-E
GOTO Cont

:SQLAuth
SET /P SQLLogin=SQL Login: 
SET /P SQLPwd=SQL password: 
SET Conn1=-U%SQLLogin% -P%SQLPwd%
SET Conn2=-SU%SQLLogin% -SP%SQLPwd%
GOTO Cont

:Cont
IF "%DATABASE%"=="" SET /P DATABASE=Database name to patch: 
IF "%DATABASETYPE%"=="" SET /P DATABASETYPE=Database type [TeleoptiCCC7,TeleoptiCCCAgg,TeleoptiAnalytics]: 

IF "%DATABASETYPE%"=="TeleoptiAnalytics" (
IF "%TeleoptiCCCAgg%"=="" SET /P TeleoptiCCCAgg=Analytics is linked to which Agg-database?:
SET CROSSDB=1
)

:Execute
::Patch DB
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -E -D%DATABASE% -O%DATABASETYPE% %Conn1% -T
IF %ERRORLEVEL% NEQ 0 GOTO ERROR_Schema

IF "%DATABASETYPE%"=="TeleoptiCCC7" (
"%ROOTDIR%\Enrypted\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%DATABASE% %Conn2%
if %errorlevel% NEQ 0 goto Security_exe
)

IF %CROSSDB% EQU 1 (
"%ROOTDIR%\Enrypted\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%DATABASE% -CD"%TeleoptiCCCAgg%" %Conn2%
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

:Security_exe
ECHO Error running Teleopti.Support.Security.exe: %DATABASE%!!
ECHO.
ECHO ---------
Echo Aborting ...
ECHO ---------
SET ERRORLEVEL=2
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
IF %Silent% NEQ 1 (
PAUSE
)
