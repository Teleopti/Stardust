@ECHO off
SETLOCAL
SET DB_CCC7=%~1
SET DB_ANALYTICS=%~2
SET Configuration=%~3
SET AS_SERVER_NAME=%~4
SET AS_DATABASE=%~5
SET SQL_AUTH_STRING=%~6
SET ETL_SERVICE_nhibConfPath=%~7

SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-1%

set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
SET MySettings=%ROOTDIR%\..\Teleopti.Support.Code\settings.txt
SET MySettingsCompiled=%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\settings.txt
SET DATASOURCE_NAME=ETLTest
SET PM_INSTALL=False
SET WEB_BROKER=http://localhost:54903/
SET PM_SERVICE=http://localhost:53396/PMService.svc
SET ETLPM_BINDING_NAME=Etl_Pm_Http_Binding

::get a fresh Settings.txt
IF EXIST "%MySettings%" DEL "%MySettings%" /F /Q

ECHO $^(DB_CCC7^)^|%DB_CCC7%>"%MySettings%"
ECHO $^(DB_ANALYTICS^)^|%DB_ANALYTICS%>>"%MySettings%"
ECHO $^(AS_DATABASE^)^|%AS_DATABASE%>>"%MySettings%"
ECHO $^(SQL_AUTH_STRING^)^|%SQL_AUTH_STRING%>>"%MySettings%"
ECHO $^(AS_SERVER_NAME^)^|%AS_SERVER_NAME%>>"%MySettings%"
ECHO $^(ETL_SERVICE_nhibConfPath^)^|%ETL_SERVICE_nhibConfPath%>>"%MySettings%"
ECHO $^(DATASOURCE_NAME^)^|%DATASOURCE_NAME%>>"%MySettings%"
ECHO $^(PM_INSTALL^)^|%PM_INSTALL%>>"%MySettings%"
ECHO $^(WEB_BROKER^)^|%WEB_BROKER%>>"%MySettings%"
ECHO $^(PM_SERVICE^)^|%PM_SERVICE%>>"%MySettings%"
ECHO $^(ETLPM_BINDING_NAME^)^|%ETLPM_BINDING_NAME%>>"%MySettings%"

IF NOT EXIST "%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" (
	::Build Teleopti.Support.Tool.exe
	ECHO Building %ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj
	IF EXIST "%ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" %MSBUILD% /property:Configuration=%Configuration% /t:rebuild "%ROOTDIR%\..\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" > "%ROOTDIR%\Teleopti.Support.Tool.build.log"
)

::Deploy new -MODEBUG input for Support.Tool
COPY "%MySettings%" "%MySettingsCompiled%"
SET ConfigFiles=%ROOTDIR%\..\Teleopti.Support.Tool\bin\Release\ConfigFiles\ConfigFiles.txt
ECHO ..\..\..\Teleopti.Analytics.Etl.ServiceConsoleHost\bin\%Configuration%\TeleoptiCCC7.nhib.xml,BuildArtifacts\TeleoptiCCC7.nhib.xml>"%ConfigFiles%"
ECHO ..\..\..\Teleopti.Analytics.Etl.ServiceConsoleHost\bin\%Configuration%\Teleopti.Analytics.Etl.ServiceConsoleHost.exe.config,BuildArtifacts\AppETLService.config>>"%ConfigFiles%"

::Run supportTool to replace all config
"%ROOTDIR%\..\Teleopti.Support.Tool\bin\%Configuration%\Teleopti.Support.Tool.exe" -MODebug

ENDLOCAL
goto:eof