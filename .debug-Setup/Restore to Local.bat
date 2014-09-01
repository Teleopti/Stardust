@ECHO off

::Get path to this batchfile
SET ROOTDIR=%~dp0
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

COLOR A
cls
SET DefaultDB=%1
SET configuration=%2
SET /A Sikuli=%3
SET Branch=%4

IF "%Sikuli%"=="" SET /A Sikuli=0
IF NOT "%DefaultDB%"=="" SET IFFLOW=y
IF "%DefaultDB%"=="" SET DefaultDB=DemoSales

::Default values
IF "%configuration%"=="" SET configuration=Debug
SET /A ERRORLEV=0

SET Customer=%DefaultDB%
SET AppRar=%DefaultDB%App.rar
SET StatRar=%DefaultDB%Stat.rar
SET /A LOADSTAT=1
SET SQLLogin=TeleoptiDemoUser
SET SQLPwd=TeleoptiDemoPwd2
SET TRUNK=-T -R -L%SQLLogin%:%SQLPwd%
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
set /p Tfiles=<"%DbBaseline%"

::Get current Branch
CD "%ROOTDIR%\.."
SET HgFolder=%CD%
IF "%Branch%"=="" CALL :BRANCH "%CD%"
ECHO Current branch is: "%BRANCH%"
ECHO.

::Clean up last log files
CD "%ROOTDIR%"
IF EXIST DBManager*.log DEL DBManager*.log /Q

::Instance were the Baseline will  be restored
SET INSTANCE=%COMPUTERNAME%
SET INSTANCE=%COMPUTERNAME%\SQL2008R2

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
set /p CustomPath=<%CustomPathConfig%
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

dir c: >NUL
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
CHOICE /C yn /M "Would like to restore the corresponding Agg and Analytics databases?"
IF ERRORLEVEL 1 SET /A LOADSTAT=1
IF ERRORLEVEL 2 SET /A LOADSTAT=0

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
SET TELEOPTIANALYTICS_BAKFILE=%RARFOLDER%\%CUSTOMER%_TeleoptiAnalytics.BAK
SET TELEOPTIAGG_BAKFILE=%RARFOLDER%\%CUSTOMER%_TeleoptiCCCAgg.BAK
SET TELEOPTICCC_BAKFILE=%RARFOLDER%\%CUSTOMER%_TeleoptiCCC7.BAK

SET TELEOPTIANALYTICS=%Branch%_%Customer%_TeleoptiAnalytics
SET TELEOPTIAGG=%Branch%_%Customer%_TeleoptiCCCAgg
SET TELEOPTICCC=%Branch%_%Customer%_TeleoptiCCC7

ECHO.
ECHO ------
ECHO Restoring baselines databases from backup. This will take a few minutes...
ECHO    %TELEOPTICCC% ...
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\database\tsql\DemoDatabase\RestoreDatabase.sql" -v BAKFILE="%TELEOPTICCC_BAKFILE%" -v DATAFOLDER="%DataFolder%" -v DATABASENAME="%TELEOPTICCC%" > "%ROOTDIR%\restoreDB.log"
IF %LOADSTAT% EQU 1 (
ECHO    %TELEOPTIANALYTICS% ...
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\database\tsql\DemoDatabase\RestoreAnalytics.sql" -v BAKFILE="%TELEOPTIANALYTICS_BAKFILE%" -v DATAFOLDER="%DataFolder%" -v DATABASENAME="%TELEOPTIANALYTICS%" >> "%ROOTDIR%\restoreDB.log"
ECHO    %TELEOPTIAGG% ...
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\database\tsql\DemoDatabase\RestoreDatabase.sql" -v BAKFILE="%TELEOPTIAGG_BAKFILE%" -v DATAFOLDER="%DataFolder%" -v DATABASENAME="%TELEOPTIAGG%"  >> "%ROOTDIR%\restoreDB.log"
)
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\database\tsql\DemoDatabase\CreateLoginDropUsers.sql" -v TELEOPTICCC="%TELEOPTICCC%" -v TELEOPTIANALYTICS="%TELEOPTIANALYTICS%" -v TELEOPTIAGG="%TELEOPTIAGG%" -v SQLLogin="%SQLLogin%" -v SQLPwd="%SQLPwd%"  >> "%ROOTDIR%\restoreDB.log"

ECHO Restoring baselines. Done!
ECHO ------
ECHO.

::set sa as owner
SQLCMD -S%INSTANCE% -E -d%TELEOPTIANALYTICS% -Q"dbo.sp_changedbowner @loginame = N'sa', @map = false"
SQLCMD -S%INSTANCE% -E -d%TELEOPTICCC% -Q"dbo.sp_changedbowner @loginame = N'sa', @map = false"
SQLCMD -S%INSTANCE% -E -d%TELEOPTIAGG% -Q"dbo.sp_changedbowner @loginame = N'sa', @map = false"

::Check if stat Databases exists, in that case leave as is
::else, create via DBManager
ECHO.
ECHO ------
ECHO Upgrade databases ...

::check if we need to create Agg (no stat)
SQLCMD -S%INSTANCE% -E -Q"SET NOCOUNT ON;select name from sys.databases where name='%TELEOPTIAGG%'" -h-1 > "%temp%\FindDB.txt"
findstr /I /C:"%TELEOPTIAGG%" "%temp%\FindDB.txt" > NUL
if %errorlevel% NEQ 0 SET CreateAgg=-C

::check if we need to create Agg (no stat)
SQLCMD -S%INSTANCE% -E -Q"SET NOCOUNT ON;select name from sys.databases where name='%TELEOPTIANALYTICS%'" -h-1 > "%temp%\FindDB.txt"
findstr /I /C:"%TELEOPTIANALYTICS%" "%temp%\FindDB.txt" > NUL
if %errorlevel% NEQ 0 SET CreateAnalytics=-C

::create or patch Analytics
%DBMANAGER% -S%INSTANCE% -D"%TELEOPTIANALYTICS%" -E -OTeleoptiAnalytics %TRUNK% %CreateAnalytics% -F"%DATABASEPATH%" > NUL
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :Error

::Create or Patch Agg
%DBMANAGER% -S%INSTANCE% -D"%TELEOPTIAGG%" -E -OTeleoptiCCCAgg %TRUNK% %CreateAgg% -F"%DATABASEPATH%" > NUL
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=4 & GOTO :Error

::Upgrade Raptor DB to latest version
CD "%DBMANAGERPATH%"
%DBMANAGER% -S%INSTANCE% -D"%TELEOPTICCC%" -E -OTeleoptiCCC7 %TRUNK% -F"%DATABASEPATH%" > NUL
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=3 & GOTO :error

::move SalesDemo to specific dates
%MSBUILD% "%ROOTDIR%\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=12 & GOTO :error

"%ROOTDIR%\..\Teleopti.Support.Security\bin\%configuration%\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%TELEOPTICCC%" -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=10 & GOTO :error

"%ROOTDIR%\..\Teleopti.Support.Security\bin\%configuration%\Teleopti.Support.Security.exe" -DS%INSTANCE% -DD"%TELEOPTIANALYTICS%" -CD"%TELEOPTIAGG%" -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

ECHO Upgrade databases. Done!
ECHO ------
ECHO.

::manipulate data and permissions
ECHO ------
ECHO Adding current Win User as xxAdmin role ...
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\database\tsql\DemoDatabase\AddingTeleoptiPermissions.sql" -v TELEOPTICCC="%TELEOPTICCC%" -v TELEOPTIANALYTICS="%TELEOPTIANALYTICS%" -v TELEOPTIAGG="%TELEOPTIAGG%"
ECHO Done!
ECHO ------
ECHO.

CD "%ROOTDIR%"

IF "%IFFLOW%"=="y" (
ECHO ------
ECHO Move DemoSales data ...
SQLCMD -S%INSTANCE% -E -dmaster -i"%ROOTDIR%\database\tsql\DemoDatabase\MoveDataInDemo.sql" -v TELEOPTICCC="%TELEOPTICCC%" -v TELEOPTIANALYTICS="%TELEOPTIANALYTICS%" -v TELEOPTIAGG="%TELEOPTIAGG%" > NUL
ECHO Move DemoSales data. Done!
ECHO ------
ECHO.
)

ECHO ------
ECHO Update data ...
ECHO Teleopti.Support.Security.exe. Done!
ECHO ------

::Build Teleopti.Support.Security.exe
ECHO ------
IF %Sikuli% equ 1 (
SQLCMD -S%INSTANCE% -E -d"%Branch%_%Customer%_TeleoptiCCC7" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\LicenseFiles\Teleopti_RD.xml"
GOTO Finish
)

Set IFFLOW=%IFFLOW:Y=y%
IF "%IFFLOW%"=="y" (
SQLCMD -S%INSTANCE% -E -d"%TELEOPTICCC%" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"
CALL "%ROOTDIR%\FixMyConfig.bat" "%TELEOPTICCC%" "%TELEOPTIANALYTICS%"
CALL "%ROOTDIR%\InfratestConfig.bat" "%Branch%_%Customer%_TeleoptiCCC7" "%Branch%_%Customer%_TeleoptiAnalytics" ALL %configuration%
SQLCMD -S%INSTANCE% -E -d"%Branch%_%Customer%_TeleoptiCCC7" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\LicenseFiles\Teleopti_RD.xml"
GOTO Finish
)

CHOICE /C yn /M "Add license?"
IF ERRORLEVEL 1 (
SQLCMD -S%INSTANCE% -E -d"%TELEOPTICCC%" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"
)

::FixMyConfig
ECHO.
CHOICE /C yn /M "Fix my config?"
IF ERRORLEVEL 2 GOTO Finish
IF ERRORLEVEL 1 (
CALL "%ROOTDIR%\FixMyConfig.bat" "%TELEOPTICCC%" "%TELEOPTIANALYTICS%"
CALL "%ROOTDIR%\InfratestConfig.bat" Infratest_CCC7 Infratest_Analytics ALL %configuration%
SQLCMD -S%INSTANCE% -E -d"%Branch%_%Customer%_TeleoptiCCC7" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\LicenseFiles\Teleopti_RD.xml"
)

GOTO Finish

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 1 ECHO Could not connect Mart to Agg: EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', '%TELEOPTIAGG%'
IF %ERRORLEV% EQU 2 ECHO Analytics DB have a bad trunk or the database is out of version sync
IF %ERRORLEV% EQU 3 ECHO CCC7 DB have a bad trunk or the database is out of version sync
IF %ERRORLEV% EQU 4 ECHO Agg DB have a bad trunk or the database is out of version sync
IF %ERRORLEV% EQU 5 ECHO Could not create views in Mart: EXEC %TELEOPTIANALYTICS%.mart.sys_crossDatabaseView_load
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
IF %Sikuli% equ 1 (
SET CustomPath=c:\temp\RestoreToLocal
) else (
SET /P CustomPath=Please provide a custom path for data storage:
)
GOTO :EOF

:SETDATAPATH
ECHO %1 > "%CustomPathConfig%"
SET DataFolder=%1\Data
SET RarFolder=%1\Baseline
SET Zip7Folder=%1\7zip
GOTO :EOF

:EOF
