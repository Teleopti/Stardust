@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-7%
SET Raptor2=ccnet\Raptor2.proj
SET NightlyBuild=ccnet\NightlyBuild.proj
SET CCNetProject=MyLocalProject
SET CCNetWorkingDirectory=%ROOTDIR%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

::register ncover licens locally ^(we only have two ...^)
CD "%CCNetWorkingDirectory%\ccnet\NCover64"
NCover.Registration.exe //License NC3CMPLIC.lic

::Get to root
CD %CCNetWorkingDirectory%
ECHO %CCNetWorkingDirectory%

::Select build type
CHOICE /C rn /M "Do you want to run (r)aptor2 or (n)ightlyBuild"
IF ERRORLEVEL 1 SET MsbuildProj=%Raptor2%
IF ERRORLEVEL 2 SET MsbuildProj=%NightlyBuild%
ECHO.

::Apply special project name?
ECHO Some special CCNET projects names (NightlyBuild,RaptorMain) will provide
ECHO extended test scenarios (To run standard tests leave blank)
SET /P CCNetProject=CCNetProject: 

::Run Build
ECHO "%MSBUILD%" /nologo /m /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProj%"
"%MSBUILD%" /nologo /m /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProj%"
ECHO.
ECHO Done!
PAUSE