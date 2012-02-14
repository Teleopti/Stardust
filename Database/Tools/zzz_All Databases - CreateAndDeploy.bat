@ECHO off

::Get path to this batchfile
SET ROOTDIR=%~dp0
SET dummystring=342kj4hkj34qywerkjdfnsadn
SET BusinessUnit=%dummystring%
::Init some static paths
SET IDE=Microsoft Visual Studio 9.0\Common7\IDE

::Set path to Visual Studio Team Foundation
IF %PROCESSOR_ARCHITECTURE% == AMD64 (
ECHO OS is 64bit
SET TEAMFOUNDATION="%ProgramFiles(x86)%\%IDE%\tf.exe"
SET DEVENV="%ProgramFiles(x86)%\%IDE%\devenv.exe"
) ELSE (
ECHO OS is 32bit
SET TEAMFOUNDATION="%ProgramFiles%\%IDE%\tf.exe"
SET DEVENV="%ProgramFiles%\%IDE%\devenv.exe"
)

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

SET /P TeleoptiAnalytics=My TeleoptiAnalytics: 
SET /P TeleoptiCCC7=My TeleoptiCCC7:
SET /P TeleoptiCCCAgg=My TeleoptiCCCAgg: 

::Connect to a SQL Server instance
SET /P MyServerInstance=Server\instance to create and deploy to:

ECHO.
ECHO Would you like to create a first business unit?
ECHO In that case please provide a name (empty will skip this part):
SET /P BusinessUnit=Business unit name:

::------------------------
::DBManager connection
:DBMangerLogin
ECHO.
ECHO Setting the SQL Login to be used by DBManager (DB creator server role needed)
SET /P DBMangerLogin=Whould you like to use WinAuth for DBManager [Y/N]?
IF "%DBMangerLogin%" == "N" GOTO SQL1
IF "%DBMangerLogin%" == "n" GOTO SQL1
GOTO WinAuth1

:SQL1
SET /P USER=DBManager SQL Login: 
SET /P PWD=DBManager SQL password:
SET DBMangerLogin=-U%USER% -P%PWD%
GOTO ApplicationLogin

:WinAuth1
SET DBMangerLogin=-E
GOTO ApplicationLogin
::------------------------
::Application connections
:ApplicationLogin
ECHO.
ECHO Setting the SQL Login to be used by the application
::SET /P ApplicationLogin=Whould you like to use WinAuth for the Application (TeleoptiCCC7) [Y/N]?
SET ApplicationLogin=N
IF "%ApplicationLogin%" == "N" GOTO SQL2
IF "%ApplicationLogin%" == "n" GOTO SQL2
GOTO WinAuth2

:SQL2
SET /P AppSQLLOGIN=Application SQL Login: 
SET /P AppSQLPWD=Application SQL password:
SET ApplicationLogin=-L%AppSQLLOGIN%:%AppSQLPWD%
GOTO Trunk

:WinAuth2
SET /P WinGroup=Name of Windows Group (will be created on DB server)
SET ApplicationLogin=-W%WinGroup%
GOTO Trunk

:Trunk
::Apply trunk?
ECHO.
SET /P IFTRUNK=Would like to deploy the Trunk? [Y/N)
IF "%IFTRUNK%"=="Y" SET TRUNK=-T
IF "%IFTRUNK%"=="y" SET TRUNK=-T

::=======================
::Init
::=======================
::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

::Move one level up in the folder structure
SET ROOTDIR=%ROOTDIR%\..

::Get Latest from TeamFoundation on that version
::To do: Kolla hur workspace funkar på en maskin som redan har detta satt? I detta exempel funkar det för att min users workspace=SORUCEFOLDER
::"C:\Program Files\Microsoft Visual Studio 9.0\Common7\IDE\tf.exe" get $/raptorScrum/Database/%DBTYPE% /all /recursive

::Deploy databases
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiCCC7% %DBMangerLogin% %ApplicationLogin% -OTeleoptiCCC7 -C %TRUNK%

"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiCCCAgg% %DBMangerLogin% %ApplicationLogin% -OTeleoptiCCCAgg -C %TRUNK%

"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiAnalytics% %DBMangerLogin% %ApplicationLogin% -OTeleoptiAnalytics -C %TRUNK%

::Deploy all cross database views in TeleoptiAnalytics
::This step should be moved into DBManager! since we don't have SQLCMD availble on the web server.
::Add Cross DB-view targets
ECHO Adding Crossdatabases
SQLCMD -S%MyServerInstance% -E -d%TeleoptiAnalytics% -Q"EXEC mart.sys_crossdatabaseview_target_update 'TeleoptiCCCAgg', '%TeleoptiCCCAgg%'"
if %errorlevel% NEQ 0 goto CROSSVIEW_LOAD

::Create views for all Cross DBs
ECHO Adding views for Crossdatabases
SQLCMD -S%MyServerInstance% -E -d%TeleoptiAnalytics% -Q"EXEC mart.sys_crossDatabaseView_load"
if %errorlevel% NEQ 0 goto CROSSVIEW_LOAD

IF "%BusinessUnit%"=="%dummystring%" GOTO finished
::else ....
::Get the Teleopti.Config.Tool
%TEAMFOUNDATION% get $/RaptorScrum/Root/Teleopti.Ccc.ApplicationConfig /all /recursive

::Build the App Config (converter) .exe
ECHO.
ECHO Adding first business unit. Working ...
ECHO %DEVENV% "%ROOTDIR%\..\Teleopti.Ccc.ApplicationConfig\Teleopti.Ccc.ApplicationConfig.csproj" /Build Debug /projectconfig Debug
ECHO building ...
%DEVENV% "%ROOTDIR%\..\Teleopti.Ccc.ApplicationConfig\Teleopti.Ccc.ApplicationConfig.csproj" /Build Debug /projectconfig Debug

::Get the default raptor data
"%ROOTDIR%\..\Teleopti.Ccc.ApplicationConfig\bin\debug\CccAppConfig.exe" -DS%MyServerInstance% -DD%TeleoptiCCC7% -DU%AppSQLLOGIN% -DP%AppSQLPWD% -TZ"W. Europe Standard Time" -BU%BusinessUnit% -CUen-GB
ECHO Adding first business unit. Done!

:finished
ECHO.
ECHO Done deploy!
PAUSE
