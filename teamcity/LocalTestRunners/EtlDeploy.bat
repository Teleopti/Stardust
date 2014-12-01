@ECHO OFF
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-27%

set debug_release=Debug
SET WorkingDirectory=%ROOTDIR%
CD "%WorkingDirectory%"

::install nuget packages
.nuget\nuget.exe install .nuget\packages.config -o packages -source "http://hestia/nuget";"https://nuget.org/api/v2"

::Build needed assemblies
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
"%MSBUILD%" /property:Configuration=%debug_release% "%WorkingDirectory%\Teleopti.Support.Security\Teleopti.Support.Security.csproj"
"%MSBUILD%" /property:Configuration=%debug_release% "%WorkingDirectory%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj"
"%MSBUILD%" /property:Configuration=%debug_release% "%WorkingDirectory%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj"

::exucute MSBuild target
"%MSBUILD%" /property:WorkingDirectory=%WorkingDirectory% "%ROOTDIR%\teamcity\EtlDeploy.msbuild"
PAUSE