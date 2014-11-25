@ECHO OFF
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-10%

SET configuration=debug
SET WorkingDirectory=%ROOTDIR%
CD "%WorkingDirectory%"
.nuget\nuget.exe install .nuget\packages.config -o packages -source "http://hestia/nuget";"https://nuget.org/api/v2"

SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
"%MSBUILD%" "%~dp0SQLVersionTests.msbuild"
CD %~dp0
PAUSE





