@ECHO OFF
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-30%

SET WorkingDirectory=%ROOTDIR%

SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

%msbuild% ..\confuserexInfrastructure.msbuild