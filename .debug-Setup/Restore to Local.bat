@ECHO off

::Get path to this batchfile
SET ROOTDIR=%~dp0
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

COLOR A
cls
SET DefaultDB=%1
SET configuration=%2
SET /A Sikuli=%3

IF "%Sikuli%"=="" SET /A Sikuli=0
IF NOT "%DefaultDB%"=="" SET IFFLOW=y
IF "%DefaultDB%"=="" SET DefaultDB=DemoSales

::Default values
IF "%configuration%"=="" SET configuration=Debug
SET /A ERRORLEV=0

SET Customer=%DefaultDB%
SET AppRar=%DefaultDB%App.rar
SET StatRar=%DefaultDB%Stat.rar
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
SET CustomTfiles=%DriveLetter%\CustomTfiles.txt
SET SQLLogin=sa
SET SQLPwd=cadadi
SET CreateAgg=
SET CreateAnalytics=
SET Tfiles=\\gigantes\Customer Databases\CCC\RestoreToLocal\Baselines

::Read/set config file
SET DbBaseline=C:\DbBaseline.txt
if not exist "%DbBaseline%" (
echo %Tfiles%> "%DbBaseline%"
)
set /p Tfiles= <"%DbBaseline%"

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

IF %Sikuli% equ 1 (
CALL "%ROOTDIR%\SikulitestConfig.bat" "%Branch%_%Customer%_TeleoptiCCC7" "%Branch%_%Customer%_TeleoptiAnalytics" ALL %configuration%
SQLCMD -S%INSTANCE% -E -d"%Branch%_%Customer%_TeleoptiCCC7" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"
PAUSE
PAUSE
GOTO Finish
)

::Build DbManager
ECHO msbuild "%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" 
IF EXIST "%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" %MSBUILD% "%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% EQU 0 (
SET DATABASEPATH="%ROOTDIR%\..\Database"
SET DBMANAGER="%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\%configuration%\DBManager.exe"
SET DBMANAGERPATH="%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\%configuration%"
) else (
SET /A ERRORLEV=6
GOTO :error
)

::checkAccess
:checkAccess
DIR "%Tfiles%" > NUL
IF %ERRORLEVEL% NEQ 0 (
ECHO Could not read files from "%Tfiles%"
Call :LocalTFiles "%DriveLetter%" "%CustomTfiles%" Tfiles
GOTO :checkAccess
)

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

ECHO Note: Database will be restored from "%Tfiles%". Feel free to change this path in "%DbBaseline%" if you want restore from other location!
ECHO.

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

IF NOT "%IFFLOW%"=="" GOTO Flow
SET /P IFFLOW=Go with the flow? [Y/N]
IF "%IFFLOW%"=="Y" GOTO Flow
IF "%IFFLOW%"=="y" GOTO Flow

GOTO UserInput

:Flow
SET DBPath=%Tfiles%
GOTO Start

:UserInput
::Get user input
CLS
ECHO Available Baselines:
ECHO -------------------------------------------
ECHO Database   @@Version/Comment         Path
ECHO -------------------------------------------
for /f "usebackq tokens=1,2 delims=;" %%g in ("%Tfiles%\Databases.txt") do echo %%g
ECHO.
ECHO -------------------------------------------

::Customer?
:PickDb
SET /P Customer=Restore which fileset (e.g. Demo):

findstr /B /C:"%Customer%" /I "%Tfiles%\Databases.txt" > %temp%\string.txt
if %errorlevel% neq 0 GOTO PickDb
for /f "tokens=1,2,3 delims=;" %%g in (%temp%\string.txt) do set DBPath=%%i
if "%DBPath%"=="" set DBPath=%Tfiles%

SET AppRar=%Customer%App.rar
SET StatRar=%Customer%Stat.rar

::Load statistics?
ECHO.
SET LOADSTAT=0
SET /P IFLOADSTAT=Would like to restore the corresponding Agg and Analytics databases? [Y/N]
IF "%IFLOADSTAT%"=="Y" SET LOADSTAT=1
IF "%IFLOADSTAT%"=="y" SET LOADSTAT=1

:Start
ECHO.
ECHO ------
ECHO Refresh .rar-file(s) ...
ECHO. > "%temp%\NumberOfFiles.txt"

::Check if app databases changed
ECHO Refreshing %AppRar% ...

if not exist "%DBPath%\%AppRar%" SET /A ERRORLEV=18 & GOTO :error
XCOPY "%DBPath%\%AppRar%" "%RarFolder%\" /D /Y > "%temp%\NumberOfFiles.txt"

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
XCOPY "%DBPath%\%StatRar%" "%RarFolder%\" /D /Y > "%temp%\NumberOfFiles.txt"
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
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\database\tsql\Restore.sql" -v DATAFOLDER="%DataFolder%" -v RARFOLDER="%RarFolder%" -v CUSTOMER=%CUSTOMER% -v LOADSTAT=%LOADSTAT% -v BRANCH="%BRANCH%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=11 & GOTO :error

ECHO Restoring baselines. Done!
ECHO ------
ECHO.

::set sa as owner
SQLCMD -S%INSTANCE% -E -d%Branch%_%Customer%_TeleoptiAnalytics -Q"dbo.sp_changedbowner @loginame = N'sa', @map = false"
SQLCMD -S%INSTANCE% -E -d%Branch%_%Customer%_TeleoptiCCC7 -Q"dbo.sp_changedbowner @loginame = N'sa', @map = false"
SQLCMD -S%INSTANCE% -E -d%Branch%_%Customer%_TeleoptiCCCAgg -Q"dbo.sp_changedbowner @loginame = N'sa', @map = false"
GO


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
ECHO %DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiAnalytics" -E -OTeleoptiAnalytics %TRUNK% %CreateAnalytics% -F"%DATABASEPATH%" 
%DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiAnalytics" -E -OTeleoptiAnalytics %TRUNK% %CreateAnalytics% -F"%DATABASEPATH%" 
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :Error

::check if we need to create Agg (no stat)
SQLCMD -S. -E -Q"SET NOCOUNT ON;select name from sys.databases where name='%Branch%_%Customer%_TeleoptiCCCAgg'" -h-1 > "%temp%\FindDB.txt"
findstr /I /C:"%Branch%_%Customer%_TeleoptiCCCAgg" "%temp%\FindDB.txt"
if %errorlevel% NEQ 0 SET CreateAgg=-C

::Create or Patch Agg
ECHO %DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCCAgg" -E -OTeleoptiCCCAgg %TRUNK% %CreateAgg% -F"%DATABASEPATH%"
%DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCCAgg" -E -OTeleoptiCCCAgg %TRUNK% %CreateAgg% -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=4 & GOTO :Error

::Upgrade Raptor DB to latest version
CD "%DBMANAGERPATH%"
ECHO %DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCC7" -E -OTeleoptiCCC7 %TRUNK% -F"%DATABASEPATH%"
%DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCC7" -E -OTeleoptiCCC7 %TRUNK% -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=3 & GOTO :error

ECHO Upgrade databases. Done!
ECHO ------
ECHO.

CD "%ROOTDIR%"

::Build Teleopti.Support.Security.exe
ECHO Building %ROOTDIR%\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj
IF EXIST "%ROOTDIR%\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj" %MSBUILD% "%ROOTDIR%\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=12 & GOTO :error

ECHO Running: scheduleConverter, ForecasterDateAdjustment, PersonFirstDayOfWeekSetter, PasswordEncryption, LicenseStatusChecker
"%ROOTDIR%\..\Teleopti.Support.Security\bin\%configuration%\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%Branch%_%Customer%_TeleoptiCCC7" -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=10 & GOTO :error

ECHO Running: CrossDatabaseViewUpdate
"%ROOTDIR%\..\Teleopti.Support.Security\bin\%configuration%\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%Branch%_%Customer%_TeleoptiAnalytics" -CD"%Branch%_%Customer%_TeleoptiCCCAgg" -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

IF %Sikuli% equ 1 (
CALL "%ROOTDIR%\SikulitestConfig.bat" "%Branch%_%Customer%_TeleoptiCCC7" "%Branch%_%Customer%_TeleoptiAnalytics" ALL %configuration%
SQLCMD -S%INSTANCE% -E -d"%Branch%_%Customer%_TeleoptiCCC7" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"
GOTO Finish
)

Set IFFLOW=%IFFLOW:Y=y%
IF "%IFFLOW%"=="y" (
CALL "%ROOTDIR%\FixMyConfig.bat" "%Branch%_%Customer%_TeleoptiCCC7" "%Branch%_%Customer%_TeleoptiAnalytics"
CALL "%ROOTDIR%\InfratestConfig.bat" "%Branch%_%Customer%_TeleoptiCCC7" "%Branch%_%Customer%_TeleoptiAnalytics" ALL %configuration%
SQLCMD -S%INSTANCE% -E -d"%Branch%_%Customer%_TeleoptiCCC7" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"
GOTO Finish
)

CHOICE /C yn /M "Add license?"
IF ERRORLEVEL 1 (
SQLCMD -S%INSTANCE% -E -d"%Branch%_%Customer%_TeleoptiCCC7" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"
)

::FixMyConfig
ECHO.
CHOICE /C yn /M "Fix my config?"
IF ERRORLEVEL 2 GOTO Finish
IF ERRORLEVEL 1 (
CALL "%ROOTDIR%\FixMyConfig.bat" "%Branch%_%Customer%_TeleoptiCCC7" "%Branch%_%Customer%_TeleoptiAnalytics"
CALL "%ROOTDIR%\InfratestConfig.bat" Infratest_CCC7 Infratest_Analytics ALL %configuration%
SQLCMD -S%INSTANCE% -E -d"%Branch%_%Customer%_TeleoptiCCC7" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"
)

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
