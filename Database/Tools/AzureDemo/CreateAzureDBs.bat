@ECHO off
SETLOCAL
COLOR A

SET DESTSERVER=%1
SET DESTCCC=%2
SET DESTANALYTICS=%3
SET AZUREADMINUSER=%4
SET AZUREADMINPWD=%5
SET DESTUSER=%6
SET DESTPWD=%7
SET BU=%8

SET /A ERRORLEV=0
SET ROOTDIR_LOCAL=%~dp0
SET ROOTDIR_LOCAL=%ROOTDIR_LOCAL:~0,-1%
CLS

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

ECHO Destination Server: %DESTSERVER%
ECHO App database: %DESTCCC%
ECHO Analytics database: %DESTANALYTICS%
ECHO SQL Azure Admin User: %AZUREADMINUSER%
ECHO SQL Azure Admin Pwd: %AZUREADMINPWD%
ECHO Teleopti WFM User name: %DESTUSER%
ECHO Teleopti WFM User pwd: %DESTPWD%
ECHO Business Unit: %BU%
PAUSE

FOR /F "tokens=1* delims=." %%A IN ("%DESTSERVER%") DO (
    SET DESTSERVERPREFIX=%%A
	SET DESTSERVERLONGNAME=%%B
)

SET DESTSERVERADMINUSER=%AZUREADMINUSER%
IF "%DESTSERVERLONGNAME%"=="database.windows.net" (
	SET DESTSERVERADMINUSER=%AZUREADMINUSER%@%DESTSERVERPREFIX%
)

SET DBMANAGER=%ROOTDIR_LOCAL%\..\..\DBManager.exe
SET SECURITY=%ROOTDIR_LOCAL%\..\..\Enrypted\Teleopti.Support.Security.exe
SET APPCONFIG=%ROOTDIR_LOCAL%\..\..\..\ApplicationConfiguration\CccAppConfig.exe

::--------
::SQL Azure
::--------
::Drop databases in Azure
ECHO Dropping Azure Dbs...
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE [%DESTANALYTICS%]"
SQLCMD -Stcp:%DESTSERVER% -U%DESTSERVERADMINUSER% -P%AZUREADMINPWD% -dmaster -Q"DROP DATABASE [%DESTCCC%]"
ECHO Dropping Azure DBs . Done!

::Create Azure Databases
ECHO Creating Analytics Db...
"%DBMANAGER%" -S%DESTSERVER% -D%DESTANALYTICS% -OTeleoptiAnalytics -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD% -T -F"%ROOTDIR_LOCAL%\..\.."
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :error
ECHO Creating Analytics Db. Done!

ECHO Creating App Db...
"%DBMANAGER%" -S%DESTSERVER% -D%DESTCCC% -OTeleoptiCCC7 -U%AZUREADMINUSER% -P%AZUREADMINPWD% -C -L%DESTUSER%:%DESTPWD% -T -F"%ROOTDIR_LOCAL%\..\.."
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=3 & GOTO :error
ECHO Creating App Db. Done!

::Insert default data
ECHO Inserting default data...
CD %ROOTDIR_LOCAL%\..\..\..\ApplicationConfiguration
"%APPCONFIG%" -DS%DESTSERVER% -DD%DESTCCC% -DU"%AZUREADMINUSER%" -DP"%AZUREADMINPWD%" -NA"%DESTUSER%" -NP"%DESTPWD%" -BU"%BU%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=3 & GOTO :error
ECHO Inserting default data. Done!

::------------
::Done
::------------
ECHO.
ECHO Azure databases with default data created!
PAUSE
GOTO :Finish


:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 1 ECHO Sorry, you need to provide correct input!
IF %ERRORLEV% EQU 2 ECHO Problems creating Analytics db
IF %ERRORLEV% EQU 3 ECHO Problems creating App db
IF %ERRORLEV% EQU 4 ECHO Problems inserting default data

ECHO.
ECHO --------
ENDLOCAL
PAUSE
GOTO :EOF

:Finish
CD "%ROOTDIR_LOCAL%"
GOTO :EOF

:EOF
exit /b %ERRORLEV%