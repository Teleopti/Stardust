@ECHO off
::=======================
::Init
::=======================
::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

::Move one level up in the folder structure
SET ROOTDIR=%ROOTDIR%\..
::=======================

SET /P TeleoptiAnalytics_Stage=My TeleoptiAnalytics_Stage: 
SET /P TeleoptiAnalytics=My TeleoptiAnalytics: 
SET /P TeleoptiCCC7=My TeleoptiCCC7:
SET /P TeleoptiCCCAgg=My TeleoptiCCCAgg: 

SET BOStore=%TeleoptiAnalytics_Stage%

SET /P MyServerInstance=Server\instance to upgrade:
::------------------------
::DBManager connection
:DBMangerLogin
ECHO The DBManager have to execute as ServerAdmin role at the database instance
SET /P DBMangerLogin=Whould you like to use WinAuth for DBManager [Y/N]?
IF "%DBMangerLogin%" == "N" GOTO SQL1
IF "%DBMangerLogin%" == "n" GOTO SQL1
GOTO WinAuth1

:SQL1
SET /P USER=DBManager SQL Login: 
SET /P PWD=DBManager SQL password:
SET DBMangerLogin=-U%USER% -P%PWD%
GOTO update

:WinAuth1
SET DBMangerLogin=-E
::------------------------

:update
::Apply trunk?
SET /P IFTRUNK=Would like to deploy the Trunk? [Y/N)
IF "%IFTRUNK%"=="Y" SET TRUNK=-T
IF "%IFTRUNK%"=="y" SET TRUNK=-T

::update databases
"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiAnalytics_Stage% %DBMangerLogin% -OTeleoptiAnalytics_Stage %TRUNK%

"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiAnalytics% %DBMangerLogin% -OTeleoptiAnalytics %TRUNK%

"%ROOTDIR%\DBManager.exe" -S%MyServerInstance% -D%TeleoptiCCC7% %DBMangerLogin% -OTeleoptiCCC7 %TRUNK%

::Deploy all cross database views
SQLCMD -S%MyServerInstance% %DBMangerLogin% -d%TeleoptiAnalytics% -i"%ROOTDIR%\TeleoptiAnalytics\ApplicationConfig\CrossDatabase Views - load.sql"

ECHO Done upgrade!
PAUSE