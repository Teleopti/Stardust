@ECHO off
COLOR A
SET version=%1
SET version=7.1.356.1536
set /A ERRORLEV=0
set PREVIOUSBUILD=\\hebe\Installation\PreviousBuilds
set SOURCEUSER=bcpUser
set SOURCEPWD=abc123456
set SOURCESERVER=%COMPUTERNAME%\TELEOPTIFORECAST
set DESTSERVER=s8v4m110k9.database.windows.net
set DESTSERVERPREFIX=s8v4m110k9
set DESTUSER=forecast
set DESTPWD=Teleopti#2012
set DESTCCC7=Forecasts_TeleoptiCCC
set SRCCCC7=TeleoptiCCC_Forecasts
set SRCAGG=TeleoptiCCC7Agg_Demo
set DBManager=C:\Program Files (x86)\Teleopti\Forecasts\DatabaseInstaller\DBManager.exe
set AZUREADMINUSER=teleopti
set AZUREADMINPWD=T3l30pt1
set workingdir=c:\temp\AzureRestore
set ROOTDIR=%~dp0
set ROOTDIR=%ROOTDIR:~0,-1%

ECHO I will now Copy the local forecast database FROM:
ECHO %SOURCESERVER% %SRCCCC7%
ECHO into SQL Azure TO:
ECHO %DESTSERVER% %DESTCCC7%

IF "%version%"=="" (
SET /P version=Provide exact CCC version to copy to SQL Azure: 
)

IF "%version%"=="" SET ERRORLEV=1 & GOTO :error

IF NOT EXIST "%PREVIOUSBUILD%\%version%" SET ERRORLEV=33 & GOTO :error

ROBOCOPY "%PREVIOUSBUILD%\%version%\Database" "%workingdir%\DatabaseInstaller" /E

::--------
::Local database
::--------
::Allow_XP_cmdShell
SQLCMD -S%SOURCESERVER% -E -i"%ROOTDIR%\Allow_XP_cmdShell.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :Error

::Generate BCP in+out batch files
::CCC7
SQLCMD -S%SOURCESERVER% -E -b -d%SRCCCC7% -i"%ROOTDIR%\DropCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=13 & GOTO :error
echo DropCircularFKs.sql done
SQLCMD -S%SOURCESERVER% -E -b -d%SRCCCC7% -i"%ROOTDIR%\GenerateBCPStatements.sql" -v DESTDB = "%DESTCCC7%" WORKINGDIR = "%workingdir%" SOURCEUSER = "%SOURCEUSER%" SOURCEPWD = "%SOURCEPWD%" DESTSERVER = "tcp:%DESTSERVER%" DESTUSER = "%DESTUSER%@%DESTSERVERPREFIX%" DESTPWD = "%DESTPWD%" > c:\out.txt
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=14 & GOTO :error
echo GenerateBCPStatements done
SQLCMD -S%SOURCESERVER% -E -b -d%SRCCCC7% -i"%ROOTDIR%\CreateCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=15 & GOTO :error
echo CreateCircularFKs done

::Execute bcp export from local databases
CMD /C "%workingdir%\%SRCCCC7%\Out.bat"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=18 & GOTO :error

PAUSE

::--------
::SQL Azure
::--------
::Drop current Demo in Azure
echo dropping Azure Dbs...
SQLCMD -Stcp:%DESTSERVER% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -U%AZUREADMINUSER%@%DESTSERVERPREFIX% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE %DESTCCC7%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=20 & GOTO :error
echo dropping Azure. Done!

::Create Azure Demo
"%workingdir%\DatabaseInstaller\DBManager.exe" -S%DESTSERVER% -D%DESTCCC7% -OTeleoptiCCC7 -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD% -T
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=22 & GOTO :error

::Prepare Azure DB = totally clean in out!
ECHO Dropping Circular FKs, Delete All Azure data. Working ...
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -b -d%DESTCCC7% -i"%ROOTDIR%\DropCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=23 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -b -d%DESTCCC7% -i"%ROOTDIR%\DeleteAllData.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=24 & GOTO :error
SQLCMD -Stcp:%DESTSERVER% -U%DESTUSER%@%DESTSERVERPREFIX% -P%DESTPWD% -b -d%DESTCCC7% -i"%ROOTDIR%\CreateCircularFKs.sql"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=25 & GOTO :error
ECHO Dropping Circular FKs, Delete All Azure data. Done!

ECHO Running BcpIn on Azure ccc7 ....
CMD /C "%workingdir%\%SRCCCC7%\in.bat"
if exist "%workingdir%\%SRCCCC7%\Logs\*log" SET /A ERRORLEV=28 & GOTO :error
ECHO Running BcpIn on Azure ccc7. Done!

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
IF %ERRORLEV% EQU 8 ECHO An error occured while changing FirstDayInWeek on Person
IF %ERRORLEV% EQU 9 ECHO An error occured while changing to Date Only in Forecasts
IF %ERRORLEV% EQU 11 ECHO error in TeleoptiCCC7-PrepareData.sql
IF %ERRORLEV% EQU 13 ECHO error in local CCC7 running DropCircularFKs.sql
IF %ERRORLEV% EQU 14 ECHO error in local CCC7 running GenerateBCPStatements.sql
IF %ERRORLEV% EQU 15 ECHO error in local CCC7 running CreateCircularFKs
IF %ERRORLEV% EQU 18 ECHO Something is wrong when doing bcp out, ccc7
IF %ERRORLEV% EQU 20 ECHO Error droping Azure ccc7
IF %ERRORLEV% EQU 22 ECHO Error Creating Azure ccc7
IF %ERRORLEV% EQU 23 ECHO Error running  Azure ccc7: DropCircularFKs.sql
IF %ERRORLEV% EQU 24 ECHO Error running  Azure ccc7: DeleteAllData.sql
IF %ERRORLEV% EQU 25 ECHO Error running  Azure ccc7: CreateCircularFKs.sql
IF %ERRORLEV% EQU 28 ECHO BcpIn error in Azure ccc7. Review log files: "%workingdir%\%SRCCCC7%\Logs"

ECHO.
ECHO --------
PAUSE
PAUSE
PAUSE
GOTO :EOF

:Finish
CD "%ROOTDIR%"
GOTO :EOF

:EOF
exit /b %ERRORLEV%