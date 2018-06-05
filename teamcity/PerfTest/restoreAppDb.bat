@ECHO off

::Get path to this batchfile
SET THIS=%~dp0
Set RepoRoot=%THIS:~0,-19%
set destinationBakFile=%repoRoot%\db.bak
set appDb=PerfApp
set analDb=PerfAnal
set aggDb=PerfAgg
set dbServer=%1
set sourceFolder=%~2
set securityExe=%RepoRoot%\Teleopti.Support.Security\bin\release\Teleopti.Support.Security.exe
set dbmanagerExe=%RepoRoot%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\release\DBManager.exe
IF [%1]==[] goto wrongInput
if [%2]==[] goto wrongInput
if not exist "%securityExe%" goto missingAssemblies
if not exist "%dbmanagerExe%" goto missingAssemblies

::copy bak file
COPY "%sourceFolder%\ccc7.bak" "%destinationBakFile%" /Y

::restore db
SQLCMD -S%dbServer% -E -dmaster -i"%RepoRoot%\.debug-Setup\database\tsql\DemoDatabase\RestoreDatabase.sql" -v BAKFILE="%destinationBakFile%" DATAFOLDER="%RepoRoot%" -v DATABASENAME="%appDb%"
IF %ERRORLEVEL% NEQ 0 GOTO :restoreError


::Skapa agg + analytics
SQLCMD -S%dbServer% -E -Q "alter database [%analDb%] set single_user with rollback immediate"
SQLCMD -S%dbServer% -E -Q "if exists(select 1 from sys.databases where name=""%analDb%"") drop database [%analDb%]"
SQLCMD -S%dbServer% -E -Q "alter database [%aggDb%] set single_user with rollback immediate"
SQLCMD -S%dbServer% -E -Q "if exists(select 1 from sys.databases where name=""%aggDb%"") drop database [%aggDb%]"
%dbmanagerExe% -S%dbServer% -D%analDb% -E -OTeleoptiAnalytics -F"%RepoRoot%\Database" -C
%dbmanagerExe% -S%dbServer% -D%aggDb% -E -OTeleoptiCCCAgg -F"%RepoRoot%\Database" -C


::upgrade appdb
%dbmanagerExe% -S%dbServer% -D"%appDb%" -E -OTeleoptiCCC7 -F"%RepoRoot%\Database" -T
SQLCMD -S%dbServer% -E -d"%appDb%" -i"%RepoRoot%\.debug-Setup\database\tsql\DemoDatabase\SetTenantActive.sql"
%securityExe% -DS%dbServer% -AP"%appDb%" -AN"%analDb%" -CD"%aggDb%" -EE 


::copy app.config
COPY "%sourceFolder%\app.config" "%RepoRoot%\Teleopti.Ccc.Scheduling.PerformanceTest\bin\debug\SettingsForTest.config" /Y
COPY "%sourceFolder%\app.config" "%RepoRoot%\Teleopti.Ccc.Scheduling.PerformanceTest\bin\release\SettingsForTest.config" /Y

::apply teleopti license
SQLCMD -S. -E -d"%appDb%" -i"%RepoRoot%\.debug-Setup\database\tsql\AddLic.sql" -v LicFile="%sourceFolder%\20300523_Teleopti_RD.xml"

exit

:wrongInput
echo Two arguments are needed. Pass in server name and a path to a folder containing "ccc7.bak" and "app.config". Eg,
echo restoreAppDb.bat . \\buildsrv01\perftests\demosales
echo.
pause
exit -1

:restoreError
echo Something went wrong with restore database
echo.
exit -1

:missingAssemblies
echo To run this script, please first compile DBManager and Security in release mode!
echo.
pause
exit -1
