@ECHO off
COLOR A
SET version=%1
SET DESTSERVER=%2
SET AZUREADMINUSER=%3
set AZUREADMINPWD=%4

set /A ERRORLEV=0
set SOURCEUSER=bcpUser
set SOURCEPWD=abc123456
set PREVIOUSBUILD=\\hebe\Installation\PreviousBuilds
set DESTUSER=TeleoptiDemoUser
set DESTPWD=TeleoptiDemoPwd2
set SRCANALYTICS=TeleoptiWFMAnalytics_Demo
set SRCCCC7=TeleoptiWFM_Demo
set SRCAGG=TeleoptiWFMAgg_Demo
set workingdir=c:\temp\AzureRestore
set ROOTDIR=%~dp0
set ROOTDIR=%ROOTDIR:~0,-1%

IF "%version%"=="" (
SET /P version=Provide exact CCC version to copy to SQL Azure: 
)
IF "%version%"=="" SET ERRORLEV=1 & GOTO :error

IF "%DESTSERVER%"=="" (
SET /P DESTSERVER=Provide the SQL Azure destination server:
)
IF "%DESTSERVER%"=="" SET ERRORLEV=1 & GOTO :error

IF "%AZUREADMINUSER%"=="" (
SET /P AZUREADMINUSER=Provide the SQL Azure admin SQL Login:
)
IF "%AZUREADMINUSER%"=="" SET ERRORLEV=1 & GOTO :error

IF "%AZUREADMINPWD%"=="" (
SET /P AZUREADMINPWD=Provide the SQL Azure admin SQL password:
)
IF "%AZUREADMINPWD%"=="" SET ERRORLEV=1 & GOTO :error


For /F "tokens=1* delims=." %%A IN ("%DESTSERVER%") DO (
    set DESTSERVERPREFIX=%%A
)

IF NOT EXIST "%PREVIOUSBUILD%\%version%" SET ERRORLEV=33 & GOTO :error


set DESTANALYTICS=TeleoptiAnalytics_Baseline_%version%
set DESTCCC7=TeleoptiCCC7_Baseline_%version%

::--------
::Get source files for this version
::--------
if exist "%workingdir%" RMDIR "%workingdir%" /S /Q
mkdir "%workingdir%"

::stop any local running Demo installation (might messup data)
net stop TeleoptiETLService
net stop TeleoptiBrokerService
net stop TeleoptiServiceBus

::Get files needed
ROBOCOPY "\\hebe\Installation\Dependencies\ccc7_server\DemoDatabase" "%workingdir%\DatabaseInstaller\DemoDatabase" *.bak
ROBOCOPY "%PREVIOUSBUILD%\%version%\DemoDatabase" "%workingdir%\DatabaseInstaller\DemoDatabase" /E
ROBOCOPY "%PREVIOUSBUILD%\%version%\Database" "%workingdir%\DatabaseInstaller" /E

::--------
::Local database
::--------
::Allow_XP_cmdShell
SQLCMD -S. -E -i"%ROOTDIR%\Allow_XP_cmdShell.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :Error

::Restore Demo to Local SQL Server
SQLCMD -S. -E -v BakDir = "%workingdir%\DatabaseInstaller\DemoDatabase" -i"%workingdir%\DatabaseInstaller\DemoDatabase\RestoreDemo.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=3 & GOTO :Error
SQLCMD -S. -E -v BakDir = "%workingdir%\DatabaseInstaller\DemoDatabase" -i"%workingdir%\DatabaseInstaller\DemoDatabase\RestoreUsers.sql" -v CurrentUser=""
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=4 & GOTO :Error

::Patch local database
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCANALYTICS% -OTeleoptiAnalytics -E -T
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=5 & GOTO :Error
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCCCC7% -OTeleoptiCCC7 -E -T
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=6 & GOTO :Error
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCAGG% -OTeleoptiCCCAgg -E -T
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=7 & GOTO :Error

::Prepare (clean out and prepare data) in local database
SQLCMD -S. -E -b -d%SRCCCC7% -i"%ROOTDIR%\TeleoptiCCC7-PrepareData.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=11 & GOTO :error
SQLCMD -S. -E -b -d%SRCANALYTICS% -i"%ROOTDIR%\TeleoptiAnalytics-PrepareData.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=12 & GOTO :error

::Run the more complex data updates
"%workingdir%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS. -DD%SRCCCC7% -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=10 & GOTO :error

::Generate BCP in+out batch files
::CCC7
SQLCMD -S. -E -b -d%SRCCCC7% -i"%ROOTDIR%\DropCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=13 & GOTO :error
SQLCMD -S. -E -b -d%SRCCCC7% -i"%ROOTDIR%\GenerateBCPStatements.sql" -v DESTDB = "%DESTCCC7%" WORKINGDIR = "%workingdir%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%AZUREADMINUSER%@%DESTSERVERPREFIX%" DESTPWD = "%AZUREADMINPWD%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=14 & GOTO :error
SQLCMD -S. -E -b -d%SRCCCC7% -i"%ROOTDIR%\CreateCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=15 & GOTO :error
::Analytics
SQLCMD -S. -E -b -d%SRCANALYTICS% -i"%ROOTDIR%\GenerateBCPStatements.sql" -v DESTDB = "%DESTANALYTICS%" WORKINGDIR = "%workingdir%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%AZUREADMINUSER%@%DESTSERVERPREFIX%" DESTPWD = "%AZUREADMINPWD%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=16 & GOTO :error

::Execute bcp export from local databases
CMD /C "%workingdir%\%SRCANALYTICS%\Out.bat"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=17 & GOTO :error
CMD /C "%workingdir%\%SRCCCC7%\Out.bat"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=18 & GOTO :error

::--------
::SQL Azure
::--------
::Drop current Demo in Azure
echo dropping Azure Dbs...
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE %DESTANALYTICS%"
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE %DESTCCC7%"
echo dropping Azure. Done!

::Create Azure Demo
"%workingdir%\DatabaseInstaller\DBManager.exe" -S%DESTSERVER% -D%DESTANALYTICS% -OTeleoptiAnalytics -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD% -T
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=21 & GOTO :error
"%workingdir%\DatabaseInstaller\DBManager.exe" -S%DESTSERVER% -D%DESTCCC7% -OTeleoptiCCC7 -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD% -T
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=22 & GOTO :error

::Prepare Azure DB = totally clean in out!
ECHO Dropping Circular FKs, Delete All Azure data. Working ...
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -b -d%DESTCCC7% -i"%ROOTDIR%\DropCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=23 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -b -d%DESTCCC7% -Q"DROP VIEW [dbo].[v_ExternalLogon]"
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -b -d%DESTCCC7% -i"%ROOTDIR%\DeleteAllData.sql"

SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -b -d%DESTCCC7% -i"%workingdir%\DatabaseInstaller\TeleoptiCCC7\Programmability\01Views\dbo.v_ExternalLogon.sql"

IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=24 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -b -d%DESTCCC7% -i"%ROOTDIR%\CreateCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=25 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -b -d%DESTANALYTICS% -i"%ROOTDIR%\DeleteAllData.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=26 & GOTO :error
ECHO Dropping Circular FKs, Delete All Azure data. Done!

::Import To Azure Demo
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -b -d%DESTANALYTICS% -i"%ROOTDIR%\AzureAnalyticsPreBcp.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=31 & GOTO :error
ECHO Running BcpIn on Azure Analytics ....
CMD /C "%workingdir%\%SRCANALYTICS%\In.bat"
if exist "%workingdir%\%SRCANALYTICS%\Logs\*.log" SET /A ERRORLEV=27 & GOTO :error
ECHO Running BcpIn on Azure Analytics. Done!
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -b -d%DESTANALYTICS% -i"%ROOTDIR%\AzureAnalyticsPostBcp.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=32 & GOTO :error


ECHO Running BcpIn on Azure ccc7 ....
CMD /C "%workingdir%\%SRCCCC7%\in.bat"
if exist "%workingdir%\%SRCCCC7%\Logs\*log" SET /A ERRORLEV=28 & GOTO :error
ECHO Running BcpIn on Azure ccc7. Done!

::Re-add Agg-views in Azure
"%workingdir%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS%DESTSERVER% -DU%AZUREADMINUSER% -DP%AZUREADMINPWD% -DD%DESTANALYTICS% -CD%DESTANALYTICS% 
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=10 & GOTO :error

::Add ETL schedules and extra statistics
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -b -d%DESTANALYTICS% -i"%ROOTDIR%\EtlScheduleAndExtraStats.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=30 & GOTO :error

::------------
::Done
::------------
PAUSE
GOTO :Finish


:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 1 ECHO Sorry, you need to provide correct input!
IF %ERRORLEV% EQU 2 ECHO Could not execute Allow_XP_CmdShell
IF %ERRORLEV% EQU 3 ECHO Could not restore local database
IF %ERRORLEV% EQU 4 ECHO Could not restore local users in database
IF %ERRORLEV% EQU 5 ECHO Could not patch local Ananlytics
IF %ERRORLEV% EQU 6 ECHO Could not patch local CCC7
IF %ERRORLEV% EQU 7 ECHO Could not patch local Agg
IF %ERRORLEV% EQU 8 ECHO An error occured while changing FirstDayInWeek on Person
IF %ERRORLEV% EQU 9 ECHO An error occured while changing to Date Only in Forecasts
IF %ERRORLEV% EQU 10 ECHO An error occured while encrypting passwords
IF %ERRORLEV% EQU 11 ECHO error in TeleoptiCCC7-PrepareData.sql
IF %ERRORLEV% EQU 12 ECHO error in TeleoptiAnalytics-PrepareData.sql
IF %ERRORLEV% EQU 13 ECHO error in local CCC7 running DropCircularFKs.sql
IF %ERRORLEV% EQU 14 ECHO error in local CCC7 running GenerateBCPStatements.sql
IF %ERRORLEV% EQU 15 ECHO error in local CCC7 running CreateCircularFKs
IF %ERRORLEV% EQU 16 ECHO error in local Analytics running GenerateBCPStatements.sql
IF %ERRORLEV% EQU 17 ECHO Something is wrong when doing bcp out, Ananlytics
IF %ERRORLEV% EQU 18 ECHO Something is wrong when doing bcp out, ccc7
IF %ERRORLEV% EQU 19 ECHO Error droping Azure Analytics
IF %ERRORLEV% EQU 20 ECHO Error droping Azure ccc7
IF %ERRORLEV% EQU 21 ECHO Error Creating Azure Analytics
IF %ERRORLEV% EQU 22 ECHO Error Creating Azure ccc7
IF %ERRORLEV% EQU 23 ECHO Error running  Azure ccc7: DropCircularFKs.sql
IF %ERRORLEV% EQU 24 ECHO Error running  Azure ccc7: DeleteAllData.sql
IF %ERRORLEV% EQU 25 ECHO Error running  Azure ccc7: CreateCircularFKs.sql
IF %ERRORLEV% EQU 26 ECHO Error running  Azure Analytics: DeleteAllData.sql
IF %ERRORLEV% EQU 27 ECHO BcpIn error in Azure Analytics. Review log files: "%workingdir%\%SRCANALYTICS%\Logs"
IF %ERRORLEV% EQU 28 ECHO BcpIn error in Azure Analytics. Review log files: "%workingdir%\%SRCCCC7%\Logs"
IF %ERRORLEV% EQU 29 ECHO Error applying Crosss DB views
IF %ERRORLEV% EQU 30 ECHO Error adding ETL Stuff to Azure
IF %ERRORLEV% EQU 31 ECHO Error running Azure Analytics: AzureAnalyticsPreBcp.sql
IF %ERRORLEV% EQU 32 ECHO Error running Azure Analytics: AzureAnalyticsPostBcp.sql
IF %ERRORLEV% EQU 33 ECHO The Provided version is not built yet & EXPLORER "%PREVIOUSBUILD%\%version%"

ECHO.
ECHO --------
PAUSE
GOTO :EOF

:Finish
CD "%ROOTDIR%"
GOTO :EOF

:EOF
exit /b %ERRORLEV%