@ECHO off

REM To run a specific target
REM BuildOnLocalPC.bat /t:Compile

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-7%
SET MsbuildProjRaptor2=ccnet\Raptor2.proj
SET CCNetWorkingDirectory=%ROOTDIR%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

::Get to root
CD %CCNetWorkingDirectory%
ECHO %CCNetWorkingDirectory%

::get MSBuild stuff
"%CCNetWorkingDirectory%\.nuget\nuget.exe" install .nuget\packages.config -o packages -source "http://hestia/nuget";"https://nuget.org/api/v2"

::Standard build
::Select build type
SET CCNetProject=RaptorMain

::Run Build
ECHO "%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProjRaptor2%" %1
"%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProjRaptor2%" %1

ECHO Done!
PAUSE