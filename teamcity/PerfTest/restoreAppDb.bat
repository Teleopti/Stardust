@ECHO off

::Get path to this batchfile
SET THIS=%~dp0
Set RepoRoot=%THIS:~0,-19%
set destinationBakFile=%repoRoot%\db.bak
set appDb=PerfApp
set analDb=PerfAnal
set aggDb=PerfAgg
set dbServer=%1
set sourceFolder=%2
IF [%1]==[] goto wrongInput
if [%2]==[] goto wrongInput


::copy bak file
COPY "%sourceFolder%\ccc7.bak" "%destinationBakFile%" /Y

::restore db
SQLCMD -S%dbServer% -E -dmaster -i"%RepoRoot%\.debug-Setup\database\tsql\DemoDatabase\RestoreDatabase.sql" -v BAKFILE="%destinationBakFile%" DATAFOLDER="%RepoRoot%" -v DATABASENAME="%appDb%"

::Skapa agg + analytics
SQLCMD -S%dbServer% -E -Q "alter database [%analDb%] set single_user with rollback immediate"
SQLCMD -S%dbServer% -E -Q "if exists(select 1 from sys.databases where name=""%analDb%"") drop database [%analDb%]"
SQLCMD -S%dbServer% -E -Q "alter database [%aggDb%] set single_user with rollback immediate"
SQLCMD -S%dbServer% -E -Q "if exists(select 1 from sys.databases where name=""%aggDb%"") drop database [%aggDb%]"
%RepoRoot%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\debug\DBManager.exe -S%dbServer% -D%analDb% -E -OTeleoptiAnalytics -F"%RepoRoot%\Database" -C
%RepoRoot%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\debug\DBManager.exe -S%dbServer% -D%aggDb% -E -OTeleoptiCCCAgg -F"%RepoRoot%\Database" -C

::upgrade appdb
%RepoRoot%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\debug\DBManager.exe -S%dbServer% -D"%appDb%" -E -OTeleoptiCCC7 -F"%RepoRoot%\Database"
%RepoRoot%\Teleopti.Support.Security\bin\debug\Teleopti.Support.Security.exe -DS%dbServer% -AP"%appDb%" -AN"%analDb%" -CD"%aggDb%" -EE 


::copy app.config
COPY "%sourceFolder%\app.config" "%RepoRoot%\Teleopti.Ccc.Scheduling.PerformanceTest\app.config" /Y
COPY "%RepoRoot%\Teleopti.Ccc.Scheduling.PerformanceTest\app.config" "%RepoRoot%\Teleopti.Ccc.Scheduling.PerformanceTest\bin\debug\Teleopti.Ccc.Scheduling.PerformanceTest.dll.config" /Y
COPY "%RepoRoot%\Teleopti.Ccc.Scheduling.PerformanceTest\app.config" "%RepoRoot%\Teleopti.Ccc.Scheduling.PerformanceTest\bin\release\Teleopti.Ccc.Scheduling.PerformanceTest.dll.config" /Y

exit

:wrongInput
echo Two arguments are needed. Pass in server name and a path to a folder containing "ccc7.bak" and "app.config". Eg,
echo restoreAppDb.bat . \\devbuild01\perftests\demosales
echo.
pause
