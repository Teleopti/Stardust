@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-28%
SET MsbuildProjRaptor2=ccnet\Raptor2.proj
SET CCNetWorkingDirectory=%ROOTDIR%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
SET ImADeveloper=TRUE

::Select build type
SET CCNetProject=Infratest

::Run Build
ECHO "%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProjRaptor2%" /t:FixConfigFiles
"%MSBUILD%" /nologo /p:Configuration=Debug "%CCNetWorkingDirectory%\%MsbuildProjRaptor2%" /t:FixConfigFiles

ECHO Done!
PAUSE
