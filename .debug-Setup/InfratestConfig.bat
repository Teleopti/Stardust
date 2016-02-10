@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-14%
SET masterSettings=%ROOTDIR%\.debug-setup\config\settingsInfraTest.txt
SET CCC7DB=%~1
SET AnalyticsDB=%~2
SET ToggleMode=%~3
set configuration=%4
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

if "%configuration%"=="" (
set configuration=Debug
)

if "%CCC7DB%" == "" (
SET CCC7DB=Infratest_CCC7
)

if "%AnalyticsDB%" == "" (
SET AnalyticsDB=Infratest_Analytics
)

if "%ToggleMode%" == "" (
CHOICE /C drc /M "Do you want to run (d)ev, (r)c or (c)ustomer toggle settings"
IF ERRORLEVEL 1 SET ToggleMode=ALL
IF ERRORLEVEL 2 SET ToggleMode=RC
IF ERRORLEVEL 3 SET ToggleMode=R
)

SET MySettings=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\settings.txt

::Build Teleopti.Support.Tool.exe if source files are available (they aren't in pipeline)
if exist "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" %MSBUILD% /t:build "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" /p:Configuration=%configuration%

::get a fresh Settings.txt
COPY "%masterSettings%" "%MySettings%"
ECHO. >> "%MySettings%"
ECHO $(DB_CCC7)^|%CCC7DB%>>"%MySettings%"
ECHO $(DB_ANALYTICS)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(AS_DATABASE)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(TOGGLE_MODE)^|%ToggleMode%>>"%MySettings%"

::telling what config to modify
set configFilesFolder=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\ConfigFiles
set buildServerConfigFiles="%configFilesFolder%\BuildServerConfigFiles.txt"
if not exist "%configFilesFolder%" mkdir "%configFilesFolder%"
echo ..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.IntegrationTest\App.config,BuildArtifacts\Teleopti.Ccc.TestCommon.App.config>%buildServerConfigFiles%
echo ..\..\..\InfrastructureTest\App.config,BuildArtifacts\Teleopti.Ccc.TestCommon.App.config>>%buildServerConfigFiles%
echo ..\..\..\Teleopti.Ccc.ApplicationConfigTest\App.config,BuildArtifacts\Teleopti.Ccc.TestCommon.App.config>>%buildServerConfigFiles%
echo ..\..\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\App.config,BuildArtifacts\Teleopti.Ccc.WebBehaviorTest.config>>%buildServerConfigFiles%
echo ..\..\..\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\%configuration%\Teleopti.Ccc.WebBehaviorTest.dll.config,BuildArtifacts\Teleopti.Ccc.WebBehaviorTest.config>>%buildServerConfigFiles%
echo ..\..\..\Teleopti.Wfm.AdministrationTest\App.config,BuildArtifacts\Teleopti.Ccc.TestCommon.App.config>>%buildServerConfigFiles%

::Run supportTool to replace all config
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -MOTEST

ENDLOCAL
goto:eof