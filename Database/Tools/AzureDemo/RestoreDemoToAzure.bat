@ECHO off
SETLOCAL
COLOR A

SET SRCSERVER=%1
SET SRCCCC7=%2
SET SRCANALYTICS=%3
SET SRCAGG=%4
SET DESTSERVER=%5
SET DESTCCC=%6
SET DESTANALYTICS=%7
SET AZUREADMINUSER=%8
SET AZUREADMINPWD=%9

SET /A ERRORLEV=0
SET SOURCEUSER=bcpUser
SET SOURCEPWD=abc123456
SET DESTUSER=TeleoptiDemoUser
SET DESTPWD=TeleoptiDemoPwd2
SET ROOTDIR_LOCAL=%~dp0
SET ROOTDIR_LOCAL=%ROOTDIR_LOCAL:~0,-1%
rem SET WORKINGDIR=%ROOTDIR_LOCAL%\Temp
SET ROOTDIR_TELEOPTI=C:\Program Files (x86)\Teleopti
SET ROOTDIR_DBMANAGER=%ROOTDIR_TELEOPTI%\DatabaseInstaller
SET DBMANAGER=%ROOTDIR_DBMANAGER%\DBManager.exe
SET SECURITY=%ROOTDIR_TELEOPTI%\DatabaseInstaller\Enrypted\Teleopti.Support.Security.exe

CLS
SET LocalWorkDir=C:\temp\AzureRestore
IF NOT EXIST "%LocalWorkDir%" MKDIR "%LocalWorkDir%"
NET share BcpFiles="%LocalWorkDir%" /grant:everyone,FULL  /users:10
SET WORKINGDIR=\\%COMPUTERNAME%\BcpFiles


ECHO This batch file merge a local Teleopti WFM install
ECHO with 3 databases to an Azure install with 2 databases.
ECHO If the destination server is a local server use the function
ECHO Export Data-tier Application to from SQL Management Studio
ECHO to generate .bacpac files to use in Azure.
ECHO.
ECHO --------------------------------------------------------------------
ECHO.
ECHO ROOTDIR_LOCAL = %ROOTDIR_LOCAL%
ECHO.
ECHO DBMANAGER = %DBMANAGER%
ECHO.
ECHO SECURITY = %SECURITY%
ECHO. 
ECHO WORKINGDIR = %WORKINGDIR%
ECHO.
PAUSE

IF "%SRCSERVER%"=="" (SET /P SRCSERVER=Source SQL Server:)
IF "%SRCSERVER%"=="" SET ERRORLEV=1 & GOTO :error

IF "%SRCCCC7%"=="" (SET /P SRCCCC7=Source App database:)
IF "%SRCCCC7%"=="" SET ERRORLEV=1 & GOTO :error

IF "%SRCANALYTICS%"=="" (SET /P SRCANALYTICS=Source Analytics database:)
IF "%SRCANALYTICS%"=="" SET ERRORLEV=1 & GOTO :error

IF "%SRCAGG%"=="" (SET /P SRCAGG=Source Agg database:)
IF "%SRCAGG%"=="" SET ERRORLEV=1 & GOTO :error

IF "%DESTSERVER%"=="" (SET /P DESTSERVER=Destination server:)
IF "%DESTSERVER%"=="" SET ERRORLEV=1 & GOTO :error

IF "%DESTCCC%"=="" (SET /P DESTCCC=Destination application database:)
IF "%DESTCCC%"=="" SET ERRORLEV=1 & GOTO :error

IF "%DESTANALYTICS%"=="" (SET /P DESTANALYTICS=Destination analytics database:)
IF "%DESTANALYTICS%"=="" SET ERRORLEV=1 & GOTO :error

IF "%AZUREADMINUSER%"=="" (SET /P AZUREADMINUSER=Destination server admin SQL Login:)
IF "%AZUREADMINUSER%"=="" SET ERRORLEV=1 & GOTO :error

IF "%AZUREADMINPWD%"=="" (SET /P AZUREADMINPWD=Destination server admin SQL password:)
IF "%AZUREADMINPWD%"=="" SET ERRORLEV=1 & GOTO :error

FOR /F "tokens=1* delims=." %%A IN ("%DESTSERVER%") DO (
    SET DESTSERVERPREFIX=%%A
	SET DESTSERVERLONGNAME=%%B
)

SET ISAZURE=0
SET DESTSERVERADMINUSER=%AZUREADMINUSER%
IF "%DESTSERVERLONGNAME%"=="database.windows.net" (
	SET ISAZURE=1
	SET DESTSERVERADMINUSER=%AZUREADMINUSER%@%DESTSERVERPREFIX%
)

NET STOP TeleoptiServiceBus
NET STOP TeleoptiETLService

REM "%ROOTDIR_TELEOPTI%\SupportTools\StartStopSystem\StopSystem.bat"

::--------
::Local database
::--------
::Allow_XP_cmdShell
SQLCMD -S%SRCSERVER% -E -i"%ROOTDIR_LOCAL%\Allow_XP_cmdShell.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :Error

::Patch source databases
REM ECHO.
REM ECHO Patching source (local) databases ...
REM ECHO.
REM "%DBMANAGER%" -S%SRCSERVER% -D%SRCCCC7% -OTeleoptiCCC7 -E -T 
REM "%DBMANAGER%" -S%SRCSERVER% -D%SRCANALYTICS% -OTeleoptiAnalytics -E -T 
REM "%DBMANAGER%" -S%SRCSERVER% -D%SRCAGG% -OTeleoptiCCCAgg -E -T 
REM "%SECURITY%"  -DS. -DD%SRCCCC7% -EE 
REM "%SECURITY%"  -DS. -DD%SRCANALYTICS% -CD%SRCAGG% -EE 

REM ECHO.
REM ECHO Patching source databases ... Done!
REM ECHO.

SET ISAGG=0

::Generate BCP in+out batch files
::CCC7
SQLCMD -S%SRCSERVER% -E -b -d%SRCCCC7% -i"%ROOTDIR_LOCAL%\DropCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=13 & GOTO :error
SQLCMD -S%SRCSERVER% -E -b -d%SRCCCC7% -i"%ROOTDIR_LOCAL%\GenerateBCPStatements.sql" -v DESTDB = "%DESTCCC%" WORKINGDIR = "%WORKINGDIR%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%AZUREADMINUSER%" DESTPWD = "%AZUREADMINPWD%" ISAGG = "%ISAGG%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=14 & GOTO :error
SQLCMD -S%SRCSERVER% -E -b -d%SRCCCC7% -i"%ROOTDIR_LOCAL%\CreateCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=15 & GOTO :error
::Analytics
SQLCMD -S%SRCSERVER% -E -b -d%SRCANALYTICS% -i"%ROOTDIR_LOCAL%\GenerateBCPStatements.sql" -v DESTDB = "%DESTANALYTICS%" WORKINGDIR = "%WORKINGDIR%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%AZUREADMINUSER%" DESTPWD = "%AZUREADMINPWD%" ISAGG = "%ISAGG%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=16 & GOTO :error
::Agg
SET ISAGG=1
SQLCMD -S%SRCSERVER% -E -b -d%SRCAGG% -i"%ROOTDIR_LOCAL%\GenerateBCPStatements.sql" -v DESTDB = "%DESTANALYTICS%" WORKINGDIR = "%WORKINGDIR%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%AZUREADMINUSER%" DESTPWD = "%AZUREADMINPWD%" ISAGG = "%ISAGG%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=16 & GOTO :error

::Execute bcp export from local databases
CMD /C "%WORKINGDIR%\%SRCANALYTICS%\Out.bat"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=17 & GOTO :error
CMD /C "%WORKINGDIR%\%SRCCCC7%\Out.bat"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=18 & GOTO :error
CMD /C "%WORKINGDIR%\%SRCAGG%\Out.bat"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=18 & GOTO :error

DEL "%WORKINGDIR%\%SRCCCC7%\Logs\*.log"
DEL "%WORKINGDIR%\%SRCANALYTICS%\Logs\*.log"
DEL "%WORKINGDIR%\%SRCAGG%\Logs\*.log"

::--------
::SQL Azure
::--------
::Drop current Demo in Azure
echo dropping Azure Dbs...
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE [%DESTANALYTICS%]"
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE [%DESTCCC%]"
echo dropping Azure. Done!

::Create Azure Demo
"%DBMANAGER%" -S%DESTSERVER% -D%DESTANALYTICS% -OTeleoptiAnalytics -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD% -T -F"%ROOTDIR_DBMANAGER%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=21 & GOTO :error
"%DBMANAGER%" -S%DESTSERVER% -D%DESTCCC% -OTeleoptiCCC7 -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD% -T -F"%ROOTDIR_DBMANAGER%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=22 & GOTO :error

ECHO Changing database editions...
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -dmaster -Q"ALTER DATABASE [%DESTANALYTICS%] MODIFY	( EDITION = 'standard', SERVICE_OBJECTIVE = 's0', MAXSIZE = 20 GB )"
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -dmaster -Q"ALTER DATABASE [%DESTCCC%] MODIFY	( EDITION = 'standard', SERVICE_OBJECTIVE = 's0', MAXSIZE = 2 GB )"
rem SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -dmaster -Q"ALTER DATABASE [%DESTCCC%] MODIFY	( EDITION = 'basic')"

::Prepare Azure DB = totally clean in out!
ECHO Dropping Circular FKs, Delete All Azure data. Working ...
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -b -d%DESTCCC% -i"%ROOTDIR_LOCAL%\DropCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=23 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -b -d%DESTCCC% -Q"DROP VIEW [dbo].[v_ExternalLogon]"
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -b -d%DESTCCC% -i"%ROOTDIR_LOCAL%\DeleteAllData.sql"
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -b -d%DESTCCC% -i"%ROOTDIR_DBMANAGER%\TeleoptiCCC7\Programmability\01Views\dbo.v_ExternalLogon.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=24 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -b -d%DESTCCC% -i"%ROOTDIR_LOCAL%\CreateCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=25 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -b -d%DESTANALYTICS% -i"%ROOTDIR_LOCAL%\DeleteAllData.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=26 & GOTO :error
ECHO Dropping Circular FKs, Delete All Azure data. Done!

::Import To Azure Demo
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -b -d%DESTANALYTICS% -i"%ROOTDIR_LOCAL%\AzureAnalyticsPreBcp.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=31 & GOTO :error

ECHO Running BcpIn on Azure Analytics ....
CMD /C "%WORKINGDIR%\%SRCANALYTICS%\In.bat"
if exist "%WORKINGDIR%\%SRCANALYTICS%\Logs\*.log" SET /A ERRORLEV=27 & GOTO :error

ECHO Running BcpIn on Azure Analytics from agg....
CMD /C "%WORKINGDIR%\%SRCAGG%\In.bat"
if exist "%WORKINGDIR%\%SRCAGG%\Logs\*.log" SET /A ERRORLEV=27 & GOTO :error
ECHO Running BcpIn on Azure Analytics. Done!

SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -b -d%DESTANALYTICS% -i"%ROOTDIR_LOCAL%\AzureAnalyticsPostBcp.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=32 & GOTO :error

ECHO Running BcpIn on Azure ccc7 ....
CMD /C "%WORKINGDIR%\%SRCCCC7%\in.bat"
if exist "%WORKINGDIR%\%SRCCCC7%\Logs\*log" SET /A ERRORLEV=28 & GOTO :error
ECHO Running BcpIn on Azure ccc7. Done!

::Drop Extended property so that .bacpac file can be generated
IF "%ISAZURE%"=="0" (
	ECHO Dropping Extended Properties in Azure databases. 
	SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -b -d%DESTCCC% -Q"EXEC sp_dropextendedproperty @name = N'DatabaseType'"
	SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -b -d%DESTANALYTICS% -Q"EXEC sp_dropextendedproperty @name = N'DatabaseType'"
)


::------------
::Done
::------------
ECHO.
ECHO Export to 2 Azure databases done!
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
IF %ERRORLEV% EQU 27 ECHO BcpIn error in Azure Analytics. Review log files: "%WORKINGDIR%\%SRCANALYTICS%\Logs"
IF %ERRORLEV% EQU 28 ECHO BcpIn error in Azure Analytics. Review log files: "%WORKINGDIR%\%SRCCCC7%\Logs"
IF %ERRORLEV% EQU 29 ECHO Error applying Crosss DB views
IF %ERRORLEV% EQU 30 ECHO Error adding ETL Stuff to Azure
IF %ERRORLEV% EQU 31 ECHO Error running Azure Analytics: AzureAnalyticsPreBcp.sql
IF %ERRORLEV% EQU 32 ECHO Error running Azure Analytics: AzureAnalyticsPostBcp.sql

ECHO.
ECHO --------
ENDLOCAL
GOTO :EOF

:Finish
NET START TeleoptiServiceBus
NET START TeleoptiETLService

rem "%ROOTDIR_TELEOPTI%\SupportTools\StartStopSystem\StartSystem.bat"
CD "%ROOTDIR_LOCAL%"
GOTO :EOF

:EOF
exit /b %ERRORLEV%
