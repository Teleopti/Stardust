@ECHO off

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-27%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe


set WorkingDirectory=%rootdir%\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\bin\Release

%msbuild% %rootdir%\teamcity\clientclickonce.msbuild

pause