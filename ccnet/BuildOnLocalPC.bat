@ECHO off

REM To run a specific target
REM BuildOnLocalPC.bat /t:Compile

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-7%
SET MsbuildProjRaptor2=ccnet\Raptor2.proj
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
SET CCNetProject=RaptorMain

::Run Build
ECHO "%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProjRaptor2%" %1
"%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProjRaptor2%" %1

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