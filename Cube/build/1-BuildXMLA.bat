@ECHO off

::Init some static paths
SET ROOTDIR=%~dp0

::Remove trailer slash
SET ROOTDIR=%ROOTDIR:~0,-1%
SET msbuildproj=%ROOTDIR%\BuildXMLA.msbuild
SET CCNetWorkingDirectory=%ROOTDIR%\..\..

CD "%ROOTDIR%\..\..\.nuget"
NuGet.exe install packages.config -o ..\packages -source "http://hestia/nuget";"https://nuget.org/api/v2
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
%msbuild% "%msbuildproj%" /t:BuildXMLA

CD "%ROOTDIR%"
PAUSE