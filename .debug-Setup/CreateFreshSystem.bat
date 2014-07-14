@ECHO off

::Get path to this batchfile
SET ROOTDIR=%~dp0
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

COLOR A
cls
SET DefaultDB=%1
IF "%DefaultDB%"=="" SET DefaultDB=Fresh

::Default values
SET configuration=Debug
SET /A ERRORLEV=0

SET Customer=%DefaultDB%
SET TRUNK=-T -C -R -Lsa:dummyPwd
SET ROOTDIR=%ROOTDIR:~0,-1%
SET DataFolder=
SET SQLLogin=sa
SET SQLPwd=cadadi

::Get current Branch
CD "%ROOTDIR%\.."
SET HgFolder=%CD%
CALL :BRANCH "%CD%"
ECHO Current branch is: "%BRANCH%"
ECHO.

::Clean up last log files
CD "%ROOTDIR%"
IF EXIST DBManager*.log DEL DBManager*.log /Q

::Instance were the Baseline will  be restored
SET INSTANCE=%COMPUTERNAME%

::Build DbManager
ECHO msbuild "%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" 
%MSBUILD% "%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% EQU 0 (
SET DATABASEPATH="%ROOTDIR%\..\Database"
SET DBMANAGER="%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\%configuration%\DBManager.exe"
SET DBMANAGERPATH="%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\%configuration%"
) else (
SET /A ERRORLEV=6
GOTO :error
)

GOTO UserInput


:UserInput

::Customer?
:PickDb
SET /P Customer=Pre-fix of your new databases: 

:Start

::Check if stat Databases exists, in that case leave as is
::else, create via DBManager
ECHO.
ECHO ------
ECHO Create new databases ...


::create Analytics
ECHO %DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiAnalytics" -E -OTeleoptiAnalytics %TRUNK% %CreateAnalytics% -F"%DATABASEPATH%" 
%DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiAnalytics" -E -OTeleoptiAnalytics %TRUNK% %CreateAnalytics% -F"%DATABASEPATH%" 
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :Error

::Create Agg
ECHO %DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCCAgg" -E -OTeleoptiCCCAgg %TRUNK% %CreateAgg% -F"%DATABASEPATH%"
%DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCCAgg" -E -OTeleoptiCCCAgg %TRUNK% %CreateAgg% -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=4 & GOTO :Error

::create ccc/wfm
CD "%DBMANAGERPATH%"
ECHO %DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCC7" -E -OTeleoptiCCC7 %TRUNK% -F"%DATABASEPATH%"
%DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCC7" -E -OTeleoptiCCC7 %TRUNK% -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=3 & GOTO :error

ECHO Create databases. Done!
ECHO ------
ECHO.


::Build Teleopti.Ccc.ApplicationConfig.exe
ECHO Building %ROOTDIR%\..\Teleopti.Ccc.ApplicationConfig\Teleopti.Ccc.ApplicationConfig.csproj
%MSBUILD% "%ROOTDIR%\..\Teleopti.Ccc.ApplicationConfig\Teleopti.Ccc.ApplicationConfig.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=12 & GOTO :error

::create first BU
CD "%ROOTDIR%\..\Teleopti.Ccc.ApplicationConfig\bin\%configuration%\"
CccAppConfig.exe -DS%INSTANCE% -DD"%Branch%_%Customer%_TeleoptiCCC7" -EE -NA"CCCAdmin" -NP"password" -BU"First BU"
CccAppConfig.exe -DS%INSTANCE% -DD"%Branch%_%Customer%_TeleoptiCCC7" -EE -TZ"W. Europe Standard Time" -BU"Second BU" -CUen-GB

::Build Teleopti.Support.Security.exe
ECHO Building %ROOTDIR%\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj
%MSBUILD% "%ROOTDIR%\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=12 & GOTO :error

CD "%ROOTDIR%"

ECHO Running: scheduleConverter, ForecasterDateAdjustment, PersonFirstDayOfWeekSetter, PasswordEncryption, LicenseStatusChecker
"%ROOTDIR%\..\Teleopti.Support.Security\bin\%configuration%\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%Branch%_%Customer%_TeleoptiCCC7" -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=10 & GOTO :error

ECHO Running: CrossDatabaseViewUpdate
"%ROOTDIR%\..\Teleopti.Support.Security\bin\%configuration%\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%Branch%_%Customer%_TeleoptiAnalytics" -CD"%Branch%_%Customer%_TeleoptiCCCAgg" -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

::FixMyConfig
ECHO.
CALL "%ROOTDIR%\FixMyConfig.bat" "%Branch%_%Customer%_TeleoptiCCC7" "%Branch%_%Customer%_TeleoptiAnalytics"

GOTO Finish

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 1 ECHO Could not connect Mart to Agg: EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', '%Branch%_%Customer%_TeleoptiCCCAgg'
IF %ERRORLEV% EQU 2 ECHO Analytics DB have a bad trunk or the database is out of version sync
IF %ERRORLEV% EQU 3 ECHO CCC7 DB have a bad trunk or the database is out of version sync
IF %ERRORLEV% EQU 4 ECHO Agg DB have a bad trunk or the database is out of version sync
IF %ERRORLEV% EQU 5 ECHO Could not create views in Mart: EXEC %Branch%_%Customer%_TeleoptiAnalytics.mart.sys_crossDatabaseView_load
IF %ERRORLEV% EQU 6 ECHO Could not build DBManager.exe & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 10 ECHO An error occured while encrypting
IF %ERRORLEV% EQU 11 ECHO Could not restore databases
IF %ERRORLEV% EQU 12 ECHO Could not build Teleopti.Support.Security & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 12 ECHO Could not build Teleopti.Ccc.ApplicationConfig & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 17 ECHO Failed to update msgBroker setings in Analytics
IF %ERRORLEV% EQU 18 ECHO You dont have permisson or file missing: "%DBPath%"
ECHO.
ECHO --------
PAUSE
GOTO :EOF

:Finish
CD "%ROOTDIR%"
GOTO :EOF

:BRANCH
SET BRANCH=%~n1
SET BRANCH=%BRANCH%%~x1
GOTO :EOF

:LocalTfiles
SETLOCAL
if not exist "%~2" (
ECHO %~1\Tfiles> %~2
)
set /p localTfiles= <%~2
mkdir "%localTfiles%"
(
ENDLOCAL
set "%~3=%localTfiles%"
)
goto:eof

:GETDATAPATH
SET /P CustomPath=Please provide a custom path for data storage:
GOTO :EOF

:SETDATAPATH
ECHO %1 > "%CustomPathConfig%"
SET DataFolder=%1\Data
SET RarFolder=%1\Baseline
SET Zip7Folder=%1\7zip
GOTO :EOF

:EOF
