@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-7%
SET MsbuildProj=ccnet\Raptor2.proj
SET CCNetWorkingDirectory=%ROOTDIR%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

::register ncover licens locally ^(we only have two lics on Teleopti...^)
CD "%CCNetWorkingDirectory%\ccnet\NCover64"
NCover.Registration.exe //License NC3CMPLIC.lic

::Get to root
CD %CCNetWorkingDirectory%
ECHO %CCNetWorkingDirectory%

::Standard build
::Select build type
CHOICE /C wn /M "Do you want to include (w)eb-test or run (n)ot"
IF ERRORLEVEL 1 SET CCNetProject=NightlyBuild
IF ERRORLEVEL 2 SET CCNetProject=RaptorMain
ECHO.


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
PAUSE
PAUSE