@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-7%
SET Raptor2=ccnet\Raptor2.proj
SET CCNetProject=none
SET NightlyBuild=ccnet\NightlyBuild.proj
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
IF ERRORLEVEL 2 (
SET MsbuildProj=%NightlyBuild%
SET CCNetProject=NightlyBuild
)
ECHO.
ECHO 
::Apply special project name?
IF "%CCNetProject%"=="" (
ECHO Some special CCNET projects names will provide extended test scenarios.
ECHO PBI15494, NightlyBuild and BuildMSIxxxxxx
ECHO To run standard tests leave blank
SET /P CCNetProject=CCNetProject:
)

::Run Build
ECHO "%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProj%"
"%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProj%"
ECHO.
ECHO Done!
PAUSE