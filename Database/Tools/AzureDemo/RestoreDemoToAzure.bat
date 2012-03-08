@ECHO off
COLOR A
SET version=%1

IF "%version%"=="" SET ERRORLEV=1 & GOTO :error

::-------------
set /A ERRORLEV=0
set SOURCEUSER=bcpUser
set SOURCEPWD=abc123456

set PREVIOUSBUILD=\\hebe\Installation\PreviousBuilds
set DESTSERVER=s8v4m110k9.database.windows.net
set DESTSERVERPREFIX=s8v4m110k9
set DESTUSER=TeleoptiDemoUser
set DESTPWD=TeleoptiDemoPwd2
set DESTANALYTICS=Demo_TeleoptiAnalytics
set SRCANALYTICS=TeleoptiAnalytics_Demo
set DESTCCC7=Demo_TeleoptiCCC7
set SRCCCC7=TeleoptiCCC7_Demo
set SRCAGG=TeleoptiCCC7Agg_Demo

set AZUREADMINUSER=teleopti
set AZUREADMINPWD=T3l30pt1

::--------------
set workingdir=c:\temp\AzureRestore
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

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
SQLCMD -S. -E -v BakDir = "%workingdir%\DatabaseInstaller\DemoDatabase" -i"%workingdir%\DatabaseInstaller\DemoDatabase\RestoreUsers.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=4 & GOTO :Error

::Patch local database
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCANALYTICS% -OTeleoptiAnalytics -E -T
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=5 & GOTO :Error
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCCCC7% -OTeleoptiCCC7 -E -T
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=6 & GOTO :Error
"%workingdir%\DatabaseInstaller\DBManager.exe" -S. -D%SRCAGG% -OTeleoptiCCCAgg -E -T
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=7 & GOTO :Error

::encrypt Pwd in local database
"%workingdir%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS. -DD%SRCCCC7% -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=10 & GOTO :error
"%workingdir%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS. -DD%SRCCCC7% -FM -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=9 & GOTO :error
"%workingdir%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe" -DS. -DD%SRCCCC7% -PU -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=8 & GOTO :error

::Prepare (clean out and prepare data) in local database
SQLCMD -S. -E -d%SRCCCC7% -i"%ROOTDIR%\TeleoptiCCC7-PrepareData.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=11 & GOTO :error
SQLCMD -S. -E -d%SRCANALYTICS% -i"%ROOTDIR%\TeleoptiAnalytics-PrepareData.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=12 & GOTO :error

::Generate BCP in+out batch files
::CCC7
SQLCMD -S. -E -d%SRCCCC7% -i"%ROOTDIR%\DropCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=13 & GOTO :error
SQLCMD -S. -E -d%SRCCCC7% -i"%ROOTDIR%\GenerateBCPStatements.sql" -v DESTDB = "%DESTCCC7%" WORKINGDIR = "%workingdir%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%DESTUSER%@%DESTSERVERPREFIX%" DESTPWD = "%DESTPWD%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=14 & GOTO :error
SQLCMD -S. -E -d%SRCCCC7% -i"%ROOTDIR%\CreateCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=15 & GOTO :error
::Analytics
SQLCMD -S. -E -d%SRCANALYTICS% -i"%ROOTDIR%\GenerateBCPStatements.sql" -v DESTDB = "%DESTANALYTICS%" WORKINGDIR = "%workingdir%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%DESTUSER%@%DESTSERVERPREFIX%" DESTPWD = "%DESTPWD%"
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
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=19 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE %DESTCCC7%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=20 & GOTO :error
echo dropping Azure. Done!

::Create Azure Demo
"%workingdir%\DatabaseInstaller\DBManager.exe" -S%DESTSERVER% -D%DESTANALYTICS% -OTeleoptiAnalytics -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD% -T
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=21 & GOTO :error
"%workingdir%\DatabaseInstaller\DBManager.exe" -S%DESTSERVER% -D%DESTCCC7% -OTeleoptiCCC7 -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD% -T
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=22 & GOTO :error

::Prepare Azure DB = totally clean in out!
ECHO Dropping Circular FKs, Delete All Azure data. Working ...
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTCCC7% -i"%ROOTDIR%\DropCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=23 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTCCC7% -i"%ROOTDIR%\DeleteAllData.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=24 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTCCC7% -i"%ROOTDIR%\CreateCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=25 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTANALYTICS% -i"%ROOTDIR%\DeleteAllData.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=26 & GOTO :error
ECHO Dropping Circular FKs, Delete All Azure data. Done!

::Import To Azure Demo
ECHO Running BcpIn on Azure Analytics ....
CMD /C "%workingdir%\%SRCANALYTICS%\In.bat"
if exist "%workingdir%\%SRCANALYTICS%\Logs\*.log" SET /A ERRORLEV=27 & GOTO :error
ECHO Running BcpIn on Azure Analytics. Done!

ECHO Running BcpIn on Azure ccc7 ....
CMD /C "%workingdir%\%SRCCCC7%\in.bat"
if exist "%workingdir%\%SRCCCC7%\Logs\*log" SET /A ERRORLEV=28 & GOTO :error
ECHO Running BcpIn on Azure ccc7. Done!

::Re-add Agg-views in Azure
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTANALYTICS% -Q"update mart.sys_crossdatabaseview_target set confirmed = 1"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=29 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTANALYTICS% -Q"exec mart.sys_crossDatabaseView_load"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=29 & GOTO :error

::Add ETL schedules and extra statistics
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -d%DESTANALYTICS% -i"%ROOTDIR%\EtlScheduleAndExtraStats.sql"
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
IF %ERRORLEV% EQU 1 ECHO Sorry, you need to provide a version!
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
IF %ERRORLEV% EQU 29 ECHO Error adding ETL Stuff to Azure
ECHO.
ECHO --------
PAUSE
GOTO :EOF

:Finish
CD "%ROOTDIR%"
GOTO :EOF

:EOF
exit /b %ERRORLEV%