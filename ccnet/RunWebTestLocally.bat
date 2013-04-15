@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-7%
SET configuration=Debug

SET CCNetWorkingDirectory=%ROOTDIR%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

CALL :REPO "%ROOTDIR%"
SET CCNetProject=%REPO%

::register ncover licens locally ^(we only have two lics on Teleopti...^)
CD "%CCNetWorkingDirectory%\ccnet\NCover64"
NCover.Registration.exe //License NC3CMPLIC.lic

::Get to root
CD %CCNetWorkingDirectory%
ECHO %CCNetWorkingDirectory%

::Build
"%MSBUILD%" "%CCNetWorkingDirectory%\CruiseControl.sln" /t:Build /p:Configuration=%configuration%

::Fix Build server config, create website
"%MSBUILD%" "%CCNetWorkingDirectory%\ccnet\raptor2.proj" /p:Configuration=%configuration% /target:FixConfigFiles;CreateAgentPortalWeb

::revert any build config changes
ECHO reverting updated checked in config files ...
hg revert -C "%ROOTDIR%\ccnet\Infratest.ini"
hg revert -C "%ROOTDIR%\ccnet\CruiseControl.config"
hg revert -C "%ROOTDIR%\ccnet\SetupTestWeb\Teleopti.Ccc.WebBehaviorTest.dll.config"
hg revert -C "%ROOTDIR%\InfrastructureTest\Teleopti.Ccc.InfrastructureTest.dll.config"
hg revert -C "%ROOTDIR%\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\App.config"
hg revert -C "%ROOTDIR%\Teleopti.Ccc.ApplicationConfigTest\Teleopti.Ccc.ApplicationConfigTest.dll.Config"

ECHO I will now start web Test, ready?
PAUSE
::Test
"%CCNetWorkingDirectory%\packages\NUnit.Runners.2.6.2\tools\nunit-console.exe" /output=c:\WebTest.xml /framework=4.0.30319 "%CCNetWorkingDirectory%\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\%configuration%\Teleopti.Ccc.WebBehaviorTest.dll"

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
GOTO :EOF

:REPO
SET REPO=%~n1
SET REPO=%REPO%%~x1
GOTO :EOF