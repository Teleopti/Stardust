@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-14%
SET masterSettings=%ROOTDIR%\.debug-setup\config\settingsSikuliTest.txt
SET CCC7DB=%~1
SET AnalyticsDB=%~2
SET configuration=%3
SET MSBUILD="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
SET logFile=%ROOTDIR%\.debug-setup\SikuliConfig.log
SET configFilesFolder=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\ConfigFiles
SET buildServerConfigFiles="%configFilesFolder%\BuildServerConfigFiles.txt"
SET MySettings=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\settings.txt

::Build Teleopti.Support.Tool.exe if source files are available (they aren't in pipeline)
if exist "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" %MSBUILD% /t:build "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" /p:Configuration=%configuration%

::get a fresh Settings.txt
COPY "%masterSettings%" "%MySettings%"
ECHO. >> "%MySettings%"
ECHO $(DB_CCC7)^|%CCC7DB%>>"%MySettings%"
ECHO $(DB_ANALYTICS)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(AS_DATABASE)^|%AnalyticsDB%>>"%MySettings%"

:: run fix my config to fix config files on web and SmartClientPortal configs
FixMyConfig %CCC7DB% %AnalyticsDB% %configuration%

::telling what files to modify
if not exist "%configFilesFolder%" mkdir "%configFilesFolder%"
echo ..\..\..\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\bin\%configuration%\Teleopti.Ccc.SmartClientPortal.Shell.exe.config,BuildArtifacts\AppRaptor.config>%buildServerConfigFiles%
echo %ROOTDIR%\nhib\SikulitestConfig.json,BuildArtifacts\SikulitestConfig.json>>%buildServerConfigFiles%

::Run supportTool to replace all config
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -MOTest
ECHO RC>"c:\nhib\Toggles.txt"

:: Run SikulitestConfigFix
ECHO %ROOTDIR%\.debug-setup\SikulitestConfigFix.bat >>%logFile%

:: write WFM connection strings to tenant database
ECHO Set WFM path in Tenant table ...
SQLCMD -S. -E -d"%CCC7DB%" -i"%ROOTDIR%\.debug-Setup\database\tsql\SetPath.sql"

:: restart iis express
taskkill /F /IM iisexpress.exe
timeout 10
cd "c:\Program Files\IIS Express\"
start iisexpress /AppPool:Clr4IntegratedAppPool

ENDLOCAL
GOTO:EOF