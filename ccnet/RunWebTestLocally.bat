@ECHO off
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-7%
SET configuration=Debug

SET CCNetWorkingDirectory=%ROOTDIR%
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

::Get to root
CD %CCNetWorkingDirectory%
ECHO %CCNetWorkingDirectory%
set CCNetProject=EtlAndAzure

::get MSBuild stuff
"%CCNetWorkingDirectory%\.nuget\nuget.exe" install .nuget\packages.config -o packages -source "http://hestia/nuget";"https://nuget.org/api/v2"

::Fix Build server config, create website
"%MSBUILD%" "%CCNetWorkingDirectory%\ccnet\ETLAndAzure.proj" /p:Configuration=%configuration% /target:PrepareDB
PAUSE
