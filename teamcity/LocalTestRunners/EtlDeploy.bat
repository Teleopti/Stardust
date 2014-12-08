@ECHO OFF
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-27%

set Configuration=Debug
SET WorkingDirectory=%ROOTDIR%
CD "%WorkingDirectory%"

::install nuget packages
.nuget\nuget.exe install .nuget\packages.config -o packages -source "http://hestia/nuget";"https://nuget.org/api/v2"

::Build needed assemblies
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
"%MSBUILD%" /property:Configuration=%Configuration% "%WorkingDirectory%\Teleopti.Analytics.Etl.ServiceConsoleHost\Teleopti.Analytics.Etl.ServiceConsoleHost.csproj"
"%MSBUILD%" /property:Configuration=%Configuration% "%WorkingDirectory%\Teleopti.Support.Security\Teleopti.Support.Security.csproj"
"%MSBUILD%" /property:Configuration=%Configuration% "%WorkingDirectory%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj"
"%MSBUILD%" /property:Configuration=%Configuration% "%WorkingDirectory%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj"

::exucute MSBuild target
"%MSBUILD%" /property:WorkingDirectory=%WorkingDirectory% /target:RunETLTest "%ROOTDIR%\teamcity\EtlDeploy.msbuild"

::exucute MSBuild target
"%MSBUILD%" /property:WorkingDirectory=%WorkingDirectory% /target:CleanUp "%ROOTDIR%\teamcity\EtlDeploy.msbuild"
PAUSE