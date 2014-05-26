@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-14%
SET masterSettings=%ROOTDIR%\.debug-setup\config\settingsInfraTest.txt
SET CCC7DB=%~1
SET AnalyticsDB=%~2
SET FEATURETOGGLE=%~3
set configuration=%4
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

if "%1" == "" (
SET CCC7DB=Infratest_CCC7
)

if "%2" == "" (
SET AnalyticsDB=Infratest_Analytics
)

::init toggle values to "ALL"
SET TOGGLE_FILE=ALL

if "%FEATURETOGGLE%" == "RC" (
SET TOGGLE_FILE=bin/FeatureFlags/RC.toggles.txt
)

if "%FEATURETOGGLE%" == "R" (
SET TOGGLE_FILE=bin/FeatureFlags/released.toggles.txt
)

if "%configuration%"=="" (
set configuration=Debug
)

SET MySettings=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\settings.txt

::Build Teleopti.Support.Tool.exe if source files are available (they aren't in pipeline)
if exist "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" %MSBUILD% /t:build "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" /p:Configuration=%configuration%

::get a fresh Settings.txt
COPY "%masterSettings%" "%MySettings%"
ECHO. >> "%MySettings%"
ECHO $(AGENTPORTALWEB_nhibConfPath)^|bin>>"%MySettings%"
ECHO $(DB_CCC7)^|%CCC7DB%>>"%MySettings%"
ECHO $(DB_ANALYTICS)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(AS_DATABASE)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(TOGGLE_FILE)^|%TOGGLE_FILE%>>"%MySettings%"
ECHO $(DATASOURCE_NAME)^|TestData>>"%MySettings%"

::telling what config to modify
set configFilesFolder=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\ConfigFiles
set buildServerConfigFiles="%configFilesFolder%\BuildServerConfigFiles.txt"
if not exist "%configFilesFolder%" mkdir "%configFilesFolder%"
echo ..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.IntegrationTest\App.config,BuildArtifacts\Teleopti.Ccc.TestCommon.App.config>%buildServerConfigFiles%
echo ..\..\..\InfrastructureTest\App.config,BuildArtifacts\Teleopti.Ccc.TestCommon.App.config>>%buildServerConfigFiles%
echo ..\..\..\Teleopti.Ccc.ApplicationConfigTest\App.config,BuildArtifacts\Teleopti.Ccc.TestCommon.App.config>>%buildServerConfigFiles%
echo ..\..\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\App.config,BuildArtifacts\Teleopti.Ccc.TestCommon.App.config>>%buildServerConfigFiles%
::need to set config files in bin folder as well if run on compiled binaries - add other of above when needed...
:: or maybe move this crap out of here?
echo ..\..\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\%configuration%\Teleopti.Ccc.WebBehaviorTest.dll.config,BuildArtifacts\Teleopti.Ccc.TestCommon.App.config>>%buildServerConfigFiles%


::Run supportTool to replace all config
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -MOTEST

ENDLOCAL
goto:eof