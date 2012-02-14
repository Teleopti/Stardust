@ECHO off
::=======================
::Init
::=======================
::Get path to this batchfile
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%

::Move one level up in the folder structure
SET ROOTDIR=%ROOTDIR%\Encryption
::=======================

SET /P TeleoptiCCC7=My TeleoptiCCC7:

SET /P MyServerInstance=Server\instance to upgrade:
::------------------------
::DBManager connection
:DBMangerLogin
SET /P DBMangerLogin=Whould you like to use WinAuth when logging in [Y/N]?
IF "%DBMangerLogin%" == "N" GOTO SQL1
IF "%DBMangerLogin%" == "n" GOTO SQL1
GOTO WinAuth1

:SQL1
SET /P USER=DBManager SQL Login: 
SET /P PWD=DBManager SQL password:
SET DBMangerLogin=-DU%USER% -DP%PWD%
GOTO update

:WinAuth1
SET DBMangerLogin=-EE
::------------------------

:update

::update databases
"%ROOTDIR%\Teleopti.Support.Security.exe" -DS%MyServerInstance% -DD%TeleoptiCCC7% %DBMangerLogin%

ECHO Done encrypting!
PAUSE