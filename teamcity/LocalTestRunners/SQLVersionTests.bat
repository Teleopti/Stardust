::================
::Pre-Req on each SQL Instance, or send "AdminSqlLogin" + "AdminSqlPwd" as Properties to MSBuild
::================
::IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'teamcityAdmin')
::	CREATE LOGIN [teamcityAdmin] WITH PASSWORD=N'Dkjh3###KKa908', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
::GO
::EXEC master..sp_addsrvrolemember @loginame = N'teamcityAdmin', @rolename = N'sysadmin'
::GO
::================

@ECHO OFF
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-27%

SET configuration=debug
SET WorkingDirectory=%ROOTDIR%
CD "%WorkingDirectory%"

::install nuget packages
.nuget\nuget.exe install .nuget\packages.config -o packages -source "http://hestia/nuget";"https://nuget.org/api/v2"

::Build needed assemblies
SET MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
"%MSBUILD%" /property:Configuration=%configuration% "%WorkingDirectory%\Teleopti.Support.Security\Teleopti.Support.Security.csproj"
"%MSBUILD%" /property:Configuration=%configuration% "%WorkingDirectory%\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager\Teleopti.Ccc.DBManager.csproj"

::Azure  = Microsoft SQL Azure (RTM) - 11.0.9227.6, SQL Azure
SET SQLServerInstance=s8v4m110k9.database.windows.net
SET TestMajorMinor=11.0
SET TestEdition=SQL Azure
SET AdminSqlLogin=teleopti
SET /P AdminSqlPwd=please provide Azure password: 
"%MSBUILD%" /property:WorkingDirectory=%WorkingDirectory%;SQLServerInstance=%SQLServerInstance%;AdminSqlLogin=%AdminSqlLogin%;AdminSqlPwd=%AdminSqlPwd%;TestEdition="%TestEdition%";TestMajorMinor=%TestMajorMinor% "%ROOTDIR%\teamcity\SQLVersionTests.msbuild"
PAUSE

::Pontus = Microsoft SQL Server 2008 R2 (SP1) - 10.50.2500.0 (X64), Developer Edition ^(64-bit^)
SET SQLServerInstance=Pontus
SET TestMajorMinor=10.50
SET TestEdition=Developer Edition ^(64-bit^)
"%MSBUILD%" /property:WorkingDirectory=%WorkingDirectory%;SQLServerInstance=%SQLServerInstance%;TestEdition="%TestEdition%";TestMajorMinor=%TestMajorMinor% "%ROOTDIR%\teamcity\SQLVersionTests.msbuild"
PAUSE

::Hebe\SQL2014 = Microsoft SQL Server 2014 - 12.0.2000.8 (X64), Express Edition ^(64-bit^)
SET SQLServerInstance=Hebe\SQL2014
SET TestEdition=Express Edition ^(64-bit^)
SET TestMajorMinor=12.0
"%MSBUILD%" /property:WorkingDirectory=%WorkingDirectory%;SQLServerInstance=%SQLServerInstance%;TestEdition="%TestEdition%";TestMajorMinor=%TestMajorMinor% "%ROOTDIR%\teamcity\SQLVersionTests.msbuild"

CD %~dp0
PAUSE





