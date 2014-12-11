@ECHO OFF
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-27%

set Configuration=Release
SET WorkingDirectory=%ROOTDIR%
CD "%WorkingDirectory%"

::install nuget packages
.nuget\nuget.exe install .nuget\packages.config -o packages -source "http://hestia/nuget";"https://nuget.org/api/v2"

::Build needed assemblies
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

"%MSBUILD%" /property:Configuration=%Configuration% "%WorkingDirectory%\Teleopti.Analytics\Teleopti.Analytics.Etl.ServiceHost\Teleopti.Analytics.Etl.ServiceHost.csproj"
"%MSBUILD%" /property:Configuration=%Configuration% "%WorkingDirectory%\Teleopti.Analytics.Etl.ServiceConsoleHost\Teleopti.Analytics.Etl.ServiceConsoleHost.csproj"
"%MSBUILD%" /property:Configuration=%Configuration% "%WorkingDirectory%\Teleopti.Support.Security\Teleopti.Support.Security.csproj"
"%MSBUILD%" /property:Configuration=%Configuration% "%WorkingDirectory%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj"
"%MSBUILD%" /property:Configuration=%Configuration% "%WorkingDirectory%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj"
"%MSBUILD%" /property:Configuration=%Configuration% "%WorkingDirectory%\Teleopti.Ccc.AnalysisServicesManager\Teleopti.Ccc.AnalysisServicesManager.csproj"

::exucute MSBuild target
"%MSBUILD%" /property:WorkingDirectory=%WorkingDirectory%;Configuration=%Configuration% /target:ProcessCube "%ROOTDIR%\teamcity\EtlDeploy.msbuild"

::CleanUp?
CHOICE /C yn /M "CleanUp?"
IF ERRORLEVEL 1 "%MSBUILD%" /property:WorkingDirectory=%WorkingDirectory%;Configuration=%Configuration% /target:CleanUp "%ROOTDIR%\teamcity\EtlDeploy.msbuild"
PAUSE