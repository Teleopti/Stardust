@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-14%
SET masterSettings=%ROOTDIR%\.debug-setup\config\settingsSikuliTest.txt
SET CCC7DB=%~1
SET AnalyticsDB=%~2
SET FEATURETOGGLE=%~3
set configuration=%4
set MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

if "%configuration%"=="" (
set configuration=Debug
)

if "%1" == "" (
SET CCC7DB=Infratest_CCC7
)

if "%2" == "" (
SET AnalyticsDB=Infratest_Analytics
)


set licensefile=
if "%FEATURETOGGLE%" == "ALL" (
SET licensefile=Teleopti_RD.xml
)

if "%FEATURETOGGLE%" == "RC" (
SET licensefile=Teleopti_RC.xml
)

if "%FEATURETOGGLE%" == "R" (
SET licensefile=Teleopti_Prod.xml
)

if "%licensefile%" == "" (
CHOICE /C drc /M "Do you want to run (d)ev, (r)c or (c)ustomer toggle settings"
IF ERRORLEVEL 1 SET licensefile=Teleopti_RD.xml
IF ERRORLEVEL 2 SET licensefile=Teleopti_RC.xml
IF ERRORLEVEL 3 SET licensefile=Teleopti_Prod.xml
)

::copy licensefile to webbehaviortest
copy /y "%rootdir%\LicenseFiles\%licensefile%" "%rootdir%\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\license.xml"
::ugly - if compiled, copy to bin folder (fix this!)
if exist "%rootdir%\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\%configuration%\license.xml" copy /y "%rootdir%\LicenseFiles\%licensefile%" "%rootdir%\Teleopti.Ccc.Web\Teleopti.Ccc.WebBehaviorTest\bin\%configuration%\license.xml"

SET MySettings=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\settings.txt

::Build Teleopti.Support.Tool.exe if source files are available (they aren't in pipeline)
if exist "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" %MSBUILD% /t:build "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" /p:Configuration=%configuration%

::get a fresh Settings.txt
COPY "%masterSettings%" "%MySettings%"
ECHO. >> "%MySettings%"
ECHO $(FeatureToggle)^|c:\nhib\Toggles.txt>>"%MySettings%"
ECHO $(nhibconfpath)^|c:\nhib>>"%MySettings%"
ECHO $(DB_CCC7)^|%CCC7DB%>>"%MySettings%"
ECHO $(DB_ANALYTICS)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(AS_DATABASE)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(DATASOURCE_NAME)^|Sikuli>>"%MySettings%"

::telling what config to modify
set configFilesFolder=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\ConfigFiles
set buildServerConfigFiles="%configFilesFolder%\BuildServerConfigFiles.txt"
if not exist "%configFilesFolder%" mkdir "%configFilesFolder%"
echo ..\..\..\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\bin\%configuration%\Teleopti.Ccc.SmartClientPortal.Shell.exe.config,BuildArtifacts\Teleopti.Ccc.SmartClientPortal.Shell.Sikuli.config>%buildServerConfigFiles%
echo c:\nhib\FixMyConfig.nhib.xml,BuildArtifacts\TeleoptiCCC7.nhib.xml>>%buildServerConfigFiles%

::Run supportTool to replace all config
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -MOTest
ECHO RC>"c:\nhib\Toggles.txt"

ENDLOCAL
goto:eof