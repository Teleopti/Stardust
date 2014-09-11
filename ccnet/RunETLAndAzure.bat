@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-7%
SET configuration=Release

SET CCNetWorkingDirectory=%ROOTDIR%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

CALL :REPO "%ROOTDIR%"
SET CCNetProject=EtlAndAzure

::register ncover licens locally ^(we only have two lics on Teleopti...^)
CD "%CCNetWorkingDirectory%\ccnet\NCover64"
NCover.Registration.exe //License NC3CMPLIC.lic

::Get to root
CD %CCNetWorkingDirectory%
ECHO %CCNetWorkingDirectory%

::Build
::"%MSBUILD%" "%CCNetWorkingDirectory%\CruiseControl.sln" /t:Build /p:Configuration=%configuration%

::Fix Build server config, create website
"%MSBUILD%" "%CCNetWorkingDirectory%\ccnet\EtlAndAzure.proj" /p:Configuration=%configuration% /target:Run
PAUSE