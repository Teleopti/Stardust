@ECHO off
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
SET CCNetProject=LocalWebTest

::Run Build
ECHO "%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProjRaptor2%" /t:FixConfigFiles
"%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProjRaptor2%" /t:FixConfigFiles

ECHO Done!
PAUSE
