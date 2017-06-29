@ECHO off

::Get path to this batchfile
SET ROOTDIR=%~dp0

call "%~dp0CheckMsbuildPath.bat"
IF %ERRORLEVEL% NEQ 0 GOTO :error

COLOR A
cls

::Default values
SET /A ERRORLEV=0
SET DefaultDB=Fresh
SET Customer=%DefaultDB%
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
SET CreateParams=-E -T -C -L"teleopti_user:teleopti_pwd" -R

::Get current Branch
CD "%ROOTDIR%\.."
SET HgFolder=%CD%
CALL :BRANCH "%CD%"
ECHO Current branch is: "%BRANCH%"
ECHO.

::Clean up last log files
CD "%ROOTDIR%"
IF EXIST DBManager*.log DEL DBManager*.log /Q

::Instance were to deploy the new system
SET INSTANCE=%COMPUTERNAME%

::Build DbManager
ECHO Building "%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" 
%MSBUILD% "%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% EQU 0 (
SET DATABASEPATH="%ROOTDIR%\..\Database"
SET DBMANAGER="%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\Debug\DBManager.exe"
SET DBMANAGERPATH="%ROOTDIR%\..\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\bin\Debug"
) else (
SET /A ERRORLEV=6
GOTO :error
)

ECHO.
ECHO ------
ECHO Create databases ...

::drop and create Analytics
SQLCMD -S%INSTANCE% -E -dmaster -Q"ALTER DATABASE [%Branch%_%Customer%_TeleoptiAnalytics] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [%Branch%_%Customer%_TeleoptiAnalytics]"
CD "%DBMANAGERPATH%"
ECHO %DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiAnalytics" -E -OTeleoptiAnalytics %CreateParams% -F"%DATABASEPATH%" 
%DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiAnalytics" -E -OTeleoptiAnalytics %CreateParams% -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=2 & GOTO :Error

::Drop and create TeleoptiCCC7
SQLCMD -S%INSTANCE% -E -dmaster -Q"ALTER DATABASE [%Branch%_%Customer%_TeleoptiCCC7] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [%Branch%_%Customer%_TeleoptiCCC7]"
ECHO %DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCC7" -E -OTeleoptiCCC7 %CreateParams% -F"%DATABASEPATH%"
%DBMANAGER% -S%INSTANCE% -D"%Branch%_%Customer%_TeleoptiCCC7" -E -OTeleoptiCCC7 %CreateParams% -F"%DATABASEPATH%"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=3 & GOTO :error

ECHO Create databases. Done!
ECHO ------
ECHO.

::Build Teleopti.Ccc.ApplicationConfig.exe
ECHO Building %ROOTDIR%\..\Teleopti.Ccc.ApplicationConfig\Teleopti.Ccc.ApplicationConfig.csproj
%MSBUILD% "%ROOTDIR%\..\Teleopti.Ccc.ApplicationConfig\Teleopti.Ccc.ApplicationConfig.csproj" >> "%temp%\build.log"

::create first BU
CD "%ROOTDIR%\..\Teleopti.Ccc.ApplicationConfig\bin\Debug"
ECHO "%ROOTDIR%\..\Teleopti.Ccc.ApplicationConfig\bin\Debug\CccAppConfig.exe" -DS%INSTANCE% -DD"%Branch%_%Customer%_TeleoptiCCC7" -EE -NA"test" -NP"test" -BU"FirstBU"
"%ROOTDIR%\..\Teleopti.Ccc.ApplicationConfig\bin\Debug\CccAppConfig.exe" -DS%INSTANCE% -DD"%Branch%_%Customer%_TeleoptiCCC7" -EE -NA"test" -NP"test" -BU"FirstBU"

CD "%ROOTDIR%"

::Build Teleopti.Support.Security.exe
ECHO Building %ROOTDIR%\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj
%MSBUILD% "%ROOTDIR%\..\Teleopti.Support.Security\Teleopti.Support.Security.csproj" > "%temp%\build.log"
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=12 & GOTO :error

ECHO Running: Security exe
"%ROOTDIR%\..\Teleopti.Support.Security\bin\debug\Teleopti.Support.Security.exe" -DS%INSTANCE% -AP"%Branch%_%Customer%_TeleoptiCCC7" -AN"%Branch%_%Customer%_TeleoptiAnalytics" -CD"%Branch%_%Customer%_TeleoptiAnalytics" -EE
IF %ERRORLEVEL% NEQ 0 SET /A ERRORLEV=1 & GOTO :error

::insert initial ETL data + Agg log_object
SQLCMD -S%INSTANCE% -E -d%Branch%_%Customer%_TeleoptiAnalytics -i"%ROOTDIR%\database\tsql\mart.sys_setupTestData.sql"
SQLCMD -S%INSTANCE% -E -d%Branch%_%Customer%_TeleoptiAnalytics -Q"exec mart.sys_setupTestData"

::insert initial Agg Agent data + one Queue
SQLCMD -S%INSTANCE% -E -d%Branch%_%Customer%_TeleoptiAnalytics -i"%ROOTDIR%\database\tsql\dbo.Add_QueueAgent_stat.sql"
SQLCMD -S%INSTANCE% -E -d%Branch%_%Customer%_TeleoptiAnalytics -Q"exec dbo.Add_QueueAgent_stat"

:Add lic
::Add license
SQLCMD -S%INSTANCE% -E -d"%Branch%_%Customer%_TeleoptiCCC7" -i"%ROOTDIR%\database\tsql\AddLic.sql" -v LicFile="%ROOTDIR%\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\License.xml"

CD "%ROOTDIR%"

:BRANCH
SET BRANCH=%~n1
SET BRANCH=%BRANCH%%~x1
GOTO :EOF

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 1 ECHO Could not connect Mart to Agg: EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', '%Branch%_%Customer%_TeleoptiCCCAgg'
IF %ERRORLEV% EQU 2 ECHO Analytics DB have a bad trunk or the database is out of version sync
IF %ERRORLEV% EQU 3 ECHO CCC7 DB have a bad trunk or the database is out of version sync
IF %ERRORLEV% EQU 6 ECHO Could not build DBManager.exe & notepad "%temp%\build.log"
IF %ERRORLEV% EQU 10 ECHO An error occured while encrypting
IF %ERRORLEV% EQU 11 ECHO Could not restore databases
IF %ERRORLEV% EQU 12 ECHO Could not build Teleopti.Support.Security & notepad "%temp%\build.log"
ECHO.
ECHO --------
PAUSE
GOTO :EOF