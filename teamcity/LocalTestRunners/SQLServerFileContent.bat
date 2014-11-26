@ECHO OFF
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-27%

SET configuration=debug
SET WorkingDirectory=%ROOTDIR%
CD "%WorkingDirectory%"
.nuget\nuget.exe install .nuget\packages.config -o packages -source "http://hestia/nuget";"https://nuget.org/api/v2"

SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
"%MSBUILD%" /property:WorkingDirectory=%WorkingDirectory%;SQLServerInstance=%COMPUTERNAME% "%ROOTDIR%\teamcity\SQLServerFileContent.msbuild"
CD %~dp0
PAUSE