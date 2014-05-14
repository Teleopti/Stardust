::Test msbuild targets on webscenarios.msbuild
:: usage
:: sikulitest.bat [CCNetLabel] [destinationFolder] [SikuliPath] [targets]
:: sikulitest.bat 138 c:\Temp\Pipeline RunSikuliTests C:\Sikuli

@ECHO off

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-27%
SET CCNetWorkingDirectory=%ROOTDIR%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

set CCNetLabel=%1
set DestinationFolder=%2
set SikuliPath=%3
set targets=%4

echo %rootdir%
echo %msbuild% %CCNetWorkingDirectory%\ccnet\pipeline\acceptance\sikulitest.msbuild /t:%targets%
%msbuild% %CCNetWorkingDirectory%\ccnet\pipeline\acceptance\sikulitest.msbuild /t:%targets%

echo %ROOTDIR%