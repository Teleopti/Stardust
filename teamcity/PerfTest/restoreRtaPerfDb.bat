@ECHO off

::Get path to this batchfile
SET THIS=%~dp0
Set RepoRoot=%THIS:~0,-19%
set destinationCC7BakFile=%repoRoot%\app.bak
set destinationAnalyticsBakFile=%repoRoot%\analytics.bak
set appDb=PerfApp
set analDb=PerfAnal
set aggDb=PerfAgg
set dbServer=%1
set sourceFolder=%2
set securityExe=%RepoRoot%\Teleopti.Support.Security\bin\release\Teleopti.Support.Security.exe
set dbmanagerExe=%RepoRoot%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\release\DBManager.exe
IF [%1]==[] goto wrongInput
if [%2]==[] goto wrongInput
if not exist "%securityExe%" goto missingAssemblies
if not exist "%dbmanagerExe%" goto missingAssemblies

::copy bak file
COPY "%sourceFolder%\ccc7.bak" "%destinationCC7BakFile%" /Y
COPY "%sourceFolder%\analytics.bak" "%destinationAnalyticsBakFile%" /Y

::restore app db
SQLCMD -S%dbServer% -E -dmaster -i"%RepoRoot%\.debug-Setup\database\tsql\DemoDatabase\RestoreDatabase.sql" -v BAKFILE="%destinationCC7BakFile%" DATAFOLDER="%RepoRoot%" -v DATABASENAME="%appDb%"
IF %ERRORLEVEL% NEQ 0 GOTO :restoreAppDbError

::restore analytics db
SQLCMD -S%dbServer% -E -dmaster -i"%RepoRoot%\.debug-Setup\database\tsql\DemoDatabase\RestoreAnalytics.sql" -v BAKFILE="%destinationAnalyticsBakFile%" DATAFOLDER="%RepoRoot%" -v DATABASENAME="%analDb%"
IF %ERRORLEVEL% NEQ 0 GOTO :restoreAnalyticsDbError


::Skapa agg
SQLCMD -S%dbServer% -E -Q "alter database [%aggDb%] set single_user with rollback immediate"
SQLCMD -S%dbServer% -E -Q "if exists(select 1 from sys.databases where name=""%aggDb%"") drop database [%aggDb%]"
%dbmanagerExe% -S%dbServer% -D%aggDb% -E -OTeleoptiCCCAgg -F"%RepoRoot%\Database" -C

::upgrade db
%dbmanagerExe% -S%dbServer% -D"%appDb%" -E -OTeleoptiCCC7 -F"%RepoRoot%\Database" -T
%dbmanagerExe% -S%dbServer% -D"%analDb%" -E -OTeleoptiAnalytics -F"%RepoRoot%\Database" -T
SQLCMD -S%dbServer% -E -d"%appDb%" -i"%RepoRoot%\.debug-Setup\database\tsql\DemoDatabase\SetTenantActive.sql"
%securityExe% -DS%dbServer% -AP"%appDb%" -AN"%analDb%" -CD"%aggDb%" -EE 

exit

:wrongInput
echo Two arguments are needed. Pass in server name and a path to a folder containing "ccc7.bak" and "app.config". Eg,
echo restoreAppDb.bat . \\buildsrv01\perftests\demosales
echo.
pause
exit -1

:restoreAppDbError
echo Something went wrong with restore app database
echo.
exit -1

:restoreAnalyticsDbError
echo Something went wrong with restore analytics database
echo.
exit -1

:missingAssemblies
echo To run this script, please first compile DBManager and Security in release mode!
echo.
pause
exit -1
