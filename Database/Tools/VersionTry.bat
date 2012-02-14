@ECHO off
SET ERRORLEVEL=0
::Init
SET TRUNK=
SET SKIPDEFAULTDATA=
SET ERRORLEV=0
SET USESQLCMD=1

IF "%1"=="" GOTO Usage
IF "%1"=="/?" GOTO Usage

::Get SQL Server instance
SET MyServerInstance=%1

::Database Names (x3)
SET TeleoptiAnalytics=%2
SET TeleoptiCCC7=%3
SET TeleoptiCCCAgg=%4

::Application connection
SET SQLLogin=%5
SET SQLPwd=%6

::Apply trunk?
IF "%7"=="Y" SET TRUNK=-T
IF "%7"=="y" SET TRUNK=-T

::Skip data (This is CCnet building)
IF "%8"=="" SET SKIPDEFAULTDATA=-A
IF "%8"=="" SET USESQLCMD=0

ECHO USESQLCMD is:  %USESQLCMD% 
::If this CCnet building we should not use SQLCMD
IF %USESQLCMD% EQU 1 (
SQLCMD -S%MyServerInstance% -dmaster -Q"ALTER DATABASE [%TeleoptiAnalytics%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [%TeleoptiAnalytics%]"
SQLCMD -S%MyServerInstance% -dmaster -Q"ALTER DATABASE [%TeleoptiCCC7%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [%TeleoptiCCC7%]"
SQLCMD -S%MyServerInstance% -dmaster -Q"ALTER DATABASE [%TeleoptiCCCAgg%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [%TeleoptiCCCAgg%]"
)

::=======================
::Init
::=======================
::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

::Move one level up in the folder structure
SET ROOTDIR=%ROOTDIR%\..

ECHO ROOTDIR: %ROOTDIR%

::Deploy databases
SET DATABASE=TeleoptiCCC7
ECHO "%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -E -D%TeleoptiCCC7% -OTeleoptiCCC7 -C %TRUNK% -L%SQLLogin%:%SQLPwd% %SKIPDEFAULTDATA%
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -E -D%TeleoptiCCC7% -OTeleoptiCCC7 -C %TRUNK% -L%SQLLogin%:%SQLPwd% %SKIPDEFAULTDATA% > NUL
IF %ERRORLEVEL% NEQ 0 GOTO ERROR_Schema

::Check naming standard for PK
IF %USESQLCMD% EQU 1 (
ECHO Check naming standard for PK
ECHO SQLCMD -S%MyServerInstance% -E -d%TeleoptiCCC7% -i"%ROOTDIR%\Tools\PK-namingStandard.sql"
SQLCMD -S%MyServerInstance% -E -d%TeleoptiCCC7% -i"%ROOTDIR%\Tools\PK-namingStandard.sql"
)
if %errorlevel% NEQ 0 goto PK_NAMING_STANDARD

SET DATABASE=TeleoptiAnalytics
ECHO "%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -E -D%TeleoptiAnalytics% -OTeleoptiAnalytics -C %TRUNK% -L%SQLLogin%:%SQLPwd% %SKIPDEFAULTDATA%
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -E -D%TeleoptiAnalytics% -OTeleoptiAnalytics -C %TRUNK% -L%SQLLogin%:%SQLPwd% %SKIPDEFAULTDATA% > NUL
IF %ERRORLEVEL% NEQ 0 GOTO ERROR_Schema

::Check naming standard for PK
IF %USESQLCMD% EQU 1 (
ECHO Check naming standard for PK
ECHO SQLCMD -S%MyServerInstance% -E -d%TeleoptiAnalytics% -i"%ROOTDIR%\Tools\PK-namingStandard.sql"
SQLCMD -S%MyServerInstance% -E -d%TeleoptiAnalytics% -i"%ROOTDIR%\Tools\PK-namingStandard.sql"
)
if %errorlevel% NEQ 0 goto PK_NAMING_STANDARD

SET DATABASE=TeleoptiCCCAgg
ECHO "%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -E -D%TeleoptiCCCAgg% -OTeleoptiCCCAgg -C %TRUNK% -L%SQLLogin%:%SQLPwd% %SKIPDEFAULTDATA%
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -E -D%TeleoptiCCCAgg% -OTeleoptiCCCAgg -C %TRUNK% -L%SQLLogin%:%SQLPwd% %SKIPDEFAULTDATA% > NUL
IF %ERRORLEVEL% NEQ 0 GOTO ERROR_Schema

::Add Cross DB-view targets
IF %USESQLCMD% EQU 1 (
ECHO Adding Crossdatabases
SQLCMD -S%MyServerInstance% -E -d%TeleoptiAnalytics% -Q"EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', '%TeleoptiCCCAgg%'"
)
if %errorlevel% NEQ 0 goto CROSSVIEW_LOAD

::Create views for all Cross DBs
IF %USESQLCMD% EQU 1 (
ECHO Adding views for Crossdatabases
SQLCMD -S%MyServerInstance% -E -d%TeleoptiAnalytics% -Q"EXEC mart.sys_crossDatabaseView_load"
)
if %errorlevel% NEQ 0 goto CROSSVIEW_LOAD

GOTO Finish_OK

:ERROR_Schema
ECHO.
ECHO ---------
Echo DBManager failed to create and/or apply release + trunk on database: %DATABASE%
ECHO ---------
SET ExitCode=101
GOTO Finish

:CROSSVIEW_LOAD
ECHO.
ECHO ---------
ECHO Could not deploy crossDB views
ECHO Check that all need databases and remote table exist!
ECHO ---------
SET ExitCode=202
GOTO Finish

:PK_NAMING_STANDARD
ECHO.
ECHO ---------
Echo The naming standard is wrong for some PK-key in database: %DATABASE%
ECHO ---------
SET ExitCode=303
GOTO Finish

:Finish_OK
ECHO.
ECHO ---------
ECHO Deploy to Empty Scehmas finished OK!
ECHO ---------
GOTO Finish

:Usage
ECHO VersionTry.bat [ServerName] [Analytcics-DB] [Raptor-DB] [Agg-DB] [SQLLogin] [SQLPwd] [WithTrunk]
ECHO ---------------------------
ECHO [ServerName]    = The name of your SQL Server instance
ECHO [Analytcics-DB] = Name of Matrix database
ECHO [Raptor-DB]     = Name of Raptor database
ECHO [Agg-DB]        = Name of Agg database
ECHO [SQLLogin]      = Name of SQL Login to be created by the DBManager.exe
ECHO [SQLPwd]        = Password for SQL Login to be created by the DBManager.exe
ECHO [WithTrunk]     = Would you like the trunk be deployed on to of the latest release
ECHO ---------------------------
ECHO Example:
ECHO VersionTry.bat Teleopti625 My_TeleoptiAnalytics My_TeleoptiCCC7 My_TeleoptiAgg david secretPwd Y
PING 127.0.0.1 -n 3 >NUL
GOTO Finish

:Finish
PING 127.0.0.1 -n 3 >NUL
EXIT %ExitCode%