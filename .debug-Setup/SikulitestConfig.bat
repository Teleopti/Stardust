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

SET MySettings=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\settings.txt

::Build Teleopti.Support.Tool.exe if source files are available (they aren't in pipeline)
if exist "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" %MSBUILD% /t:build "%ROOTDIR%\Teleopti.Support.Tool\Teleopti.Support.Tool.csproj" /p:Configuration=%configuration%

::get a fresh Settings.txt
COPY "%masterSettings%" "%MySettings%"
ECHO. >> "%MySettings%"
ECHO $(DB_CCC7)^|%CCC7DB%>>"%MySettings%"
ECHO $(DB_ANALYTICS)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(AS_DATABASE)^|%AnalyticsDB%>>"%MySettings%"

::telling what files to modify
SET configFilesFolder=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\ConfigFiles
SET buildServerConfigFiles="%configFilesFolder%\BuildServerConfigFiles.txt"
if not exist "%configFilesFolder%" mkdir "%configFilesFolder%"
echo ..\..\..\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\bin\%configuration%\Teleopti.Ccc.SmartClientPortal.Shell.exe.config,BuildArtifacts\AppRaptor.config>%buildServerConfigFiles%
echo %ROOTDIR%\nhib\FixMyConfig.nhib.xml,BuildArtifacts\TeleoptiCCC7.nhib.xml>>%buildServerConfigFiles%
echo %ROOTDIR%\nhib\authentication.json,BuildArtifacts\authentication.json>>%buildServerConfigFiles%
echo %ROOTDIR%\nhib\configserver.json,BuildArtifacts\configserver.json>>%buildServerConfigFiles%

::Run supportTool to replace all config
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -MOTest
ECHO RC>"c:\nhib\Toggles.txt"

ECHO %ROOTDIR%\.debug-setup\SikulitestConfigFix.bat >>%logFile%

::final changes on Teleopti.Ccc.SmartClientPortal.Shell.exe.config
SET configPath=%ROOTDIR%\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\bin\%configuration%\Teleopti.Ccc.SmartClientPortal.Shell.exe.config
SET commonFolder=%ROOTDIR%\.debug-setup\common
ECHO %configPath% >>%logFile%
ECHO %commonFolder% >>%logFile%

COPY "c:\XmlSetAttribute.exe" %commonFolder%\XmlSetAttribute.exe

SET nodePath=configuration/appSettings/add[@key='FeatureToggle']
SET attributeName=value
SET attributeValue=%ROOTDIR%\Domain\FeatureFlags\toggles.txt
%commonFolder%\XmlSetAttribute.exe %configPath% %nodePath% %attributeName% %attributeValue%

SET nodePath=configuration/appSettings/add[@key='TenantServer']
SET attributeName=value
SET attributeValue=%ROOTDIR%\nhib\authentication.json
%commonFolder%\XmlSetAttribute.exe %configPath% %nodePath% %attributeName% %attributeValue%

SET nodePath=configuration/appSettings/add[@key='ConfigServer']
SET attributeName=value
SET attributeValue=%ROOTDIR%\nhib\configserver.json
%commonFolder%\XmlSetAttribute.exe %configPath% %nodePath% %attributeName% %attributeValue%

ENDLOCAL
GOTO:EOF