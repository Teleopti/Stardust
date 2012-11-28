@ECHO off

::Get path to this batchfile
SET ROOTDIR=%~dp0
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

COLOR A
cls

::Default values
SET /A ERRORLEV=0
SET Demoreg=Demoreg
SET Customer=%Demoreg%
SET AppRar=%Demoreg%App.rar
SET StatRar=%Demoreg%Stat.rar
SET LOADSTAT=1
SET TRUNK=-T -R -Lsa:dummyPwd
SET UNRAR=7
SET Relative=Relative
SET ROOTDIR=%ROOTDIR:~0,-1%
SET DataFolder=
SET RarFolder=
SET Zip7Folder=
SET DriveLetter=%ROOTDIR:~0,2%
SET CustomPathConfig=%DriveLetter%\CustomPath.txt
SET SQLLogin=sa
SET SQLPwd=cadadi
SET SUPPORTTOOL=
SET CreateAgg=
SET CreateAnalytics=
SET Tfiles=\\a380\T-files\Develop\Test\Baselines

::Get current Branch
CD "%ROOTDIR%\..\..\.."
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
ECHO msbuild "%ROOTDIR%\..\..\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" 
%MSBUILD% "%ROOTDIR%\..\..\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% EQU 0 (
SET DBMANAGER="%ROOTDIR%\..\..\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\Debug\DBManager.exe"
SET DBMANAGERPATH="%ROOTDIR%\..\..\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\Debug"
) else (
SET /A ERRORLEV=6
GOTO :error
)

::Init some static paths
SET IDE=Microsoft Visual Studio 10.0\Common7\IDE
SET WEBDEV=Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer.EXE

::Used for check: Did we copy a new file?
SET FINDTHIS=0 File(s) copied

ECHO.

goto MakeCustomPath
:MakeCustomPath
if exist "%CustomPathConfig%" (
set /p CustomPath= <%CustomPathConfig%
) ELSE (
CALL :GETDATAPATH
)

if not "%CustomPath:~1,2%"==":\" (
COLOR E
CLS
ECHO Sorry
echo The data storage path must now be a valid local path, separate from your source control folder^(s^)
CALL :GETDATAPATH
)

echo "%CustomPath%" | FIND "%HgFolder%"
if %errorlevel% equ 0 (
COLOR E
CLS
ECHO Sorry
ECHO CustomPath is : %CustomPath%
ECHO HgPath is     : %HgFolder%
ECHO.
echo You should not keep database files under any Hg Repository.
ECHO Please separate database files etc. from your source control folder^(s^)
CALL :GETDATAPATH
)
COLOR A

IF NOT EXIST "%CustomPath%" MKDIR "%CustomPath%"
IF %ERRORLEVEL% NEQ 0 (
echo could not create direcroty: %CustomPath%
goto MakeCustomPath
)

CALL :SETDATAPATH %CustomPath%

IF NOT EXIST "%DataFolder%" MKDIR "%DataFolder%"
IF NOT EXIST "%RarFolder%" MKDIR "%RarFolder%"
IF NOT EXIST "%Zip7Folder%" MKDIR "%Zip7Folder%"

::Un-zip exe
SET UNRAR="%Zip7Folder%\7z.exe" x -y -o"%RarFolder%"
ECHO un-rar Exe-file is: %UNRAR%

::Get 7-zip exe
ECHO Refreshing 7-zip ...
ECHO ROBOCOPY "%Tfiles%\7zip" "%Zip7Folder%" /MIR
ROBOCOPY "%Tfiles%\7zip" "%Zip7Folder%" /MIR

CLS
ECHO --- Note! ---
ECHO "Go with the flow" will:
ECHO - Install all three Demoreg databases on the default SQL Server instance
ECHO - Add the Trunk on top of the release.
ECHO - Encryption of passwords will be ensured.
ECHO - Add a valid license
ECHO --- --- ---
ECHO.
SET /P IFFLOW=Go with the flow? [Y/N]
IF "%IFFLOW%"=="Y" GOTO Flow
IF "%IFFLOW%"=="y" GOTO Flow

GOTO UserInput

:Flow
SET SUPPORTTOOL=SUPPORTTOOL
SET MB=
SET BUS=
SET DBPath=%Tfiles%
GOTO Start

:UserInput
::Get user input
CLS
ECHO Available Baselines:
ECHO -------------------------------------------
ECHO Database	@@Version	Comment	Path
ECHO -------------------------------------------
for /f "tokens=1,2 delims=;" %%g in (%Tfiles%\Databases.txt) do echo %%g
ECHO.
ECHO -------------------------------------------

::Customer?
:PickDb
SET /P Customer=Restore which fileset (e.g. Demo):

findstr /B /C:"%Customer%" /I "%Tfiles%\Databases.txt" > %temp%\string.txt
if %errorlevel% neq 0 GOTO PickDb
for /f "tokens=1,2 delims=;" %%g in (%temp%\string.txt) do set DBPath=%%h
if "%DBPath%"=="" set DBPath=%Tfiles%

SET AppRar=%Customer%App.rar
SET StatRar=%Customer%Stat.rar

::Load statistics?
ECHO.
SET LOADSTAT=0
SET /P IFLOADSTAT=Would like to restore the corresponding Agg and Analytics databases? [Y/N]
IF "%IFLOADSTAT%"=="Y" SET LOADSTAT=1
IF "%IFLOADSTAT%"=="y" SET LOADSTAT=1

::use support tool?
SET SUPPORTTOOL=SUPPORTTOOL

::install BUS?
ECHO.
SET BUS=
SET /P IFBUS=Would like to build and install a local Service Bus? [Y/N]
IF "%IFBUS%"=="Y" SET BUS=BUS
IF "%IFBUS%"=="y" SET BUS=BUS

ECHO.
ECHO OBSERVE!: This script will now destroy the current databases namned:
ECHO "%Branch%_%Customer%_TeleoptiCCC7"
IF %LOADSTAT% EQU 1 ECHO "%Branch%_%Customer%_TeleoptiAnalytics"
IF %LOADSTAT% EQU 1 ECHO "%Branch%_%Customer%_TeleoptiCCCAgg"
ECHO.
ECHO on instance: %INSTANCE%
ECHO.
ECHO Please double check that databases and instance data is as intended!
ECHO Press any key to continue or any other key to exit...
PAUSE
GOTO Start

:Start
ECHO.
ECHO ------
ECHO Refresh .rar-file(s) ...
ECHO. > "%temp%\NumberOfFiles.txt"

::Check if app databases changed
ECHO Refreshing %AppRar% ...
XCOPY "%Tfiles%\%AppRar%" "%RarFolder%\" /D /Y > "%temp%\NumberOfFiles.txt"

::unRar only if new
findstr /C:"0 File(s) copied" "%temp%\NumberOfFiles.txt"
if %errorlevel% EQU 0 (
ECHO No need to un-rar. File is the same
ping 127.0.0.1 -n 3 > NUL
) ELSE (
ECHO Unrar file: "%RarFolder%\%AppRar%" ...
%UNRAR% "%RarFolder%\%AppRar%"
)
::Check if stat databases changed
ECHO. > "%temp%\NumberOfFiles.txt"

IF %LOADSTAT% EQU 1 (
ECHO Refreshing %StatRar% ...
XCOPY "%Tfiles%\%StatRar%" "%RarFolder%\" /D /Y > "%temp%\NumberOfFiles.txt"
) ELSE (
GOTO SkipStat
)
ECHO Refresh .rar-file(s). Done!
ECHO ------
ECHO.

::unRar only if new
ECHO.
ECHO ------
ECHO Un-rar ...
findstr /C:"0 File(s) copied" "%temp%\NumberOfFiles.txt"
if %errorlevel% EQU 0 (
ECHO No need to un-rar. File is the same
ping 127.0.0.1 -n 3 > NUL
) ELSE (
ECHO Unrar file: "%RarFolder%\%StatRar%" ...
%UNRAR% "%RarFolder%\%StatRar%"
)
ECHO Un-rar. Done!
ECHO ------
ECHO.

:SkipStat
::Restore Customer databases
ECHO.
ECHO ------
ECHO Restoring baselines databases from backup. This will take a few minutes...
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\tsql\Restore.sql" -v DATAFOLDER="%DataFolder%" -v RARFOLDER="%RarFolder%" -v CUSTOMER=%CUSTOMER% -v LOADSTAT=%LOADSTAT% -v BRANCH="%BRANCH%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=11 & GOTO :error

ECHO Restoring baselines. Done!
ECHO ------
ECHO.

::Check if stat Databases exists, in that case leave as is
::else, create via DBManager
ECHO.
ECHO ------
ECHO Upgrade databases ...

::check if we need to create Analytics (no stat)
SQLCMD -S. -E -Q"SET NOCOUNT ON;select name from sys.databases where name='%Branch%_%Customer%_TeleoptiAnalytics'" -h-1 > "%temp%\FindDB.txt"
findstr /I /C:"%Branch%_%Customer%_TeleoptiAnalytics" "%temp%\FindDB.txt"
if %errorlevel% NEQ 0 SET CreateAnalytics=-C

::create or patch Analytics
ECHO %DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiAnalytics" -E -OTeleoptiAnalytics %TRUNK% %CreateAnalytics%
%DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiAnalytics" -E -OTeleoptiAnalytics %TRUNK% %CreateAnalytics%
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :Error

::check if we need to create Agg (no stat)
SQLCMD -S. -E -Q"SET NOCOUNT ON;select name from sys.databases where name='%Branch%_%Customer%_TeleoptiCCCAgg'" -h-1 > "%temp%\FindDB.txt"
findstr /I /C:"%Branch%_%Customer%_TeleoptiCCCAgg" "%temp%\FindDB.txt"
if %errorlevel% NEQ 0 SET CreateAgg=-C

::Create or Patch Agg
ECHO %DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCCAgg" -E -OTeleoptiCCCAgg %TRUNK% %CreateAgg%
%DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCCAgg" -E -OTeleoptiCCCAgg %TRUNK% %CreateAgg%
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=4 & GOTO :Error

::Upgrade Raptor DB to latest version
CD "%DBMANAGERPATH%"
ECHO %DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCC7" -E -OTeleoptiCCC7 %TRUNK%
%DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCC7" -E -OTeleoptiCCC7 %TRUNK%
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=3 & GOTO :error

ECHO Upgrade databases. Done!
ECHO ------
ECHO.

CD "%ROOTDIR%"

::Build Teleopti.Support.Security.exe
ECHO Building %ROOTDIR%\..\..\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj
%MSBUILD% "%ROOTDIR%\..\..\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=12 & GOTO :error

ECHO Encrypting passwords ...
"%ROOTDIR%\..\..\..\Teleopti.Support.Security\bin\debug\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%Branch%_%Customer%_TeleoptiCCC7" -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=10 & GOTO :error

ECHO Fix Cross DB stuff
"%ROOTDIR%\..\..\..\Teleopti.Support.Security\bin\debug\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%Branch%_%Customer%_TeleoptiAnalytics" -CD"%Branch%_%Customer%_TeleoptiCCCAgg" -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

::Add license (only if Demoreg)
IF "%Customer%"=="%Demoreg%" (
SQLCMD -S%INSTANCE% -E -d"%Branch%_%Customer%_TeleoptiCCC7" -i"%ROOTDIR%\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\..\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=13 & GOTO :Error
)

::Build the Support Tool
ECHO msbuild "%ROOTDIR%\..\..\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" 
%MSBUILD% "%ROOTDIR%\..\..\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% EQU 0 (
CD "%ROOTDIR%\..\..\..\Teleopti.Support.Tool\Bin\Debug"
Teleopti.Support.Tool.exe
) else (
SET /A ERRORLEV=7
GOTO :Error
)

IF "%BUS%"=="BUS" (
::Build and install + start
ECHO CALL "%ROOTDIR%\ServiceBus\BuildAndInstall.bat"
CALL "%ROOTDIR%\ServiceBus\BuildAndInstall.bat"
::SDK start debug web server
%Cassini% /port:1335 /path:"%ROOTDIR%\..\..\..\Teleopti.Ccc.Sdk\Teleopti.Ccc.Sdk.Host\bin"
::Kick it
NET START TeleoptiServiceBus
IF %ERRORLEVEL% NEQ 0 ECHO Check Windows Application log for errors! & eventvwr
)

GOTO :Finish

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
IF %ERRORLEV% EQU 7 ECHO Could not build Teleopti.Support.Tool & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 10 ECHO An error occured while encrypting
IF %ERRORLEV% EQU 11 ECHO Could not restore databases
IF %ERRORLEV% EQU 12 ECHO Could not build Teleopti.Support.Security & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 13 ECHO Could not apply license on demoreg database
IF %ERRORLEV% EQU 17 ECHO Failed to update msgBroker setings in Analytics
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
