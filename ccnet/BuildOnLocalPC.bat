@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-7%
SET Raptor2=ccnet\Raptor2.proj
SET DefaultCCNetProject=none
SET CCNetProject=%DefaultCCNetProject%
SET NightlyBuild=ccnet\NightlyBuild.proj
SET CCNetWorkingDirectory=%ROOTDIR%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

::register ncover licens locally ^(we only have two lics on Teleopti...^)
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

::Apply special project name?
IF "%CCNetProject%"=="%DefaultCCNetProject%" (
ECHO Some special CCNET projects names will provide extended test scenarios.
ECHO PBI15494, NightlyBuild
ECHO To run standard tests leave blank
SET /P CCNetProject=CCNetProject:
)
ECHO %CCNetProject%
PAUSE
::Run Build
ECHO "%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProj%"
"%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProj%"
ECHO.
ECHO reverting updated checked in config files ...
hg revert -C "%ROOTDIR%\ccnet\Infratest.ini"
hg revert -C "%ROOTDIR%\ccnet\CruiseControl.config"
hg revert -C "%ROOTDIR%\ccnet\SetupTestWeb\Teleopti.Ccc.WebBehaviorTest.dll.config"
hg revert -C "%ROOTDIR%\InfrastructureTest\Teleopti.Ccc.InfrastructureTest.dll.config"
hg revert -C "%ROOTDIR%\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\App.config"
hg revert -C "%ROOTDIR%\Teleopti.Ccc.ApplicationConfigTest\Teleopti.Ccc.ApplicationConfigTest.dll.Config"

ECHO Done!
PAUSE
