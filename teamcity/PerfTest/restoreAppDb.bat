@ECHO off

::Get path to this batchfile
SET THIS=%~dp0
Set RepoRoot=%THIS:~0,-19%
set destinationBakFile=%repoRoot%\db.bak
set appDb=PerfApp
set analDb=PerfAnal
set aggDb=PerfAgg
set sourceFolder=%1
IF [%1]==[] (
	echo Pass in a path to a folder containing "ccc7.bak" and "app.config"
	pause
	exit
)

::copy bak file
COPY "%sourceFolder%\ccc7.bak" "%destinationBakFile%" /Y

::restore db
SQLCMD -S. -E -dmaster -i"%RepoRoot%\.debug-Setup\database\tsql\DemoDatabase\RestoreDatabase.sql" -v BAKFILE="%destinationBakFile%" DATAFOLDER="%RepoRoot%" -v DATABASENAME="%appDb%"

::Skapa agg + analytics
SQLCMD -S. -E -Q "alter database [%analDb%] set single_user with rollback immediate"
SQLCMD -S. -E -Q "if exists(select 1 from sys.databases where name=""%analDb%"") drop database [%analDb%]"
SQLCMD -S. -E -Q "alter database [%aggDb%] set single_user with rollback immediate"
SQLCMD -S. -E -Q "if exists(select 1 from sys.databases where name=""%aggDb%"") drop database [%aggDb%]"
%RepoRoot%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\debug\DBManager.exe -S. -D%analDb% -E -OTeleoptiAnalytics -F"%RepoRoot%\Database" -C
%RepoRoot%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\debug\DBManager.exe -S. -D%aggDb% -E -OTeleoptiCCCAgg -F"%RepoRoot%\Database" -C

::upgrade appdb
%RepoRoot%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\debug\DBManager.exe -S. -D"%appDb%" -E -OTeleoptiCCC7 -F"%RepoRoot%\Database"
%RepoRoot%\Teleopti.Support.Security\bin\debug\Teleopti.Support.Security.exe -DS. -AP"%appDb%" -AN"%analDb%" -CD"%aggDb%" -EE 


::copy app.config
COPY "%sourceFolder%\app.config" "%RepoRoot%\Teleopti.Ccc.Scheduling.PerformanceTest\app.config" /Y