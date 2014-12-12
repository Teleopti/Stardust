@ECHO off

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-27%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

set WorkingDirectory=%rootdir%

%msbuild% %rootdir%\teamcity\deletexmlfilesinoutputfolder.msbuild

pause