::Test msbuild targets on webscenarios.msbuild
:: usage
:: webscenarios.bat [CCNetLabel] [destinationFolder] [targets]

@ECHO off

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-27%
SET CCNetWorkingDirectory=%ROOTDIR%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

set CCNetLabel=%1
set DestinationFolder=%2
set targets=%3

echo %rootdir%

%msbuild% %CCNetWorkingDirectory%\ccnet\pipeline\acceptance\webscenarios.msbuild /t:%targets%

echo %ROOTDIR%