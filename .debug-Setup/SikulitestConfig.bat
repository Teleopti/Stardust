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
ECHO $(FeatureToggle)^|%ROOTDIR%\Domain\FeatureFlags\toggles.txt>>"%MySettings%"
ECHO $(nhibconfpath)^|%ROOTDIR%\nhib>>"%MySettings%"
ECHO $(DB_CCC7)^|%CCC7DB%>>"%MySettings%"
ECHO $(DB_ANALYTICS)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(AS_DATABASE)^|%AnalyticsDB%>>"%MySettings%"
ECHO $(DATASOURCE_NAME)^|Sikuli>>"%MySettings%"

::telling what config to modify
SET configFilesFolder=%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\ConfigFiles
SET buildServerConfigFiles="%configFilesFolder%\BuildServerConfigFiles.txt"
if not exist "%configFilesFolder%" mkdir "%configFilesFolder%"
echo ..\..\..\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\bin\%configuration%\Teleopti.Ccc.SmartClientPortal.Shell.exe.config,BuildArtifacts\AppRaptor.config>%buildServerConfigFiles%
echo %ROOTDIR%\nhib\FixMyConfig.nhib.xml,BuildArtifacts\TeleoptiCCC7.nhib.xml>>%buildServerConfigFiles%

::Run supportTool to replace all config
"%ROOTDIR%\Teleopti.Support.Tool\bin\%configuration%\Teleopti.Support.Tool.exe" -MOTest
ECHO RC>"c:\nhib\Toggles.txt"

ECHO %ROOTDIR%\.debug-setup\SikulitestConfigFix.bat >>%logFile%

::final changes
SET configPath=%ROOTDIR%\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\bin\%configuration%\Teleopti.Ccc.SmartClientPortal.Shell.exe.config
SET commonFolder=%ROOTDIR%\.debug-setup\common
ECHO %configPath% >>%logFile%
ECHO %commonFolder% >>%logFile%

COPY "c:\XmlSetAttribute.exe" %commonFolder%\XmlSetAttribute.exe

SLEEP 3

SET nodePath=configuration/appSettings/add[@key='GetConfigFromWebService']
SET attributeName=value
SET value=false
%commonFolder%\XmlSetAttribute.exe %configPath% %nodePath% %attributeName% %value%

SET nodePath=configuration/appSettings/add[@key='nhibconfpath']
SET attributeName=value
SET value=%ROOTDIR%\nhib
%commonFolder%\XmlSetAttribute.exe %configPath% %nodePath% %attributeName% %value%

SET nodePath=configuration/appSettings/add[@key='FeatureToggle']
SET attributeName=value
SET value=%ROOTDIR%\Domain\FeatureFlags\toggles.txt
%commonFolder%\XmlSetAttribute.exe %configPath% %nodePath% %attributeName% %value%

SET nodePath=configuration/system.serviceModel/bindings/basicHttpBinding/binding/security
SET attributeName=mode
SET value=TransportCredentialOnly
%commonFolder%\XmlSetAttribute.exe %configPath% %nodePath% %attributeName% %value%

SET nodePath=configuration/system.serviceModel/bindings/basicHttpBinding/binding/security/transport
SET attributeName=clientCredentialType
SET value=Ntlm
%commonFolder%\XmlSetAttribute.exe %configPath% %nodePath% %attributeName% %value%

ENDLOCAL
GOTO:EOF