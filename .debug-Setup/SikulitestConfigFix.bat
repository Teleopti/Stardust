@ECHO off
SETLOCAL
SET ROOTDIR=%~dp0
SET ROOTDIR=%ROOTDIR:~0,-14%
SET configuration=Release

::final changes
SET configPath=%ROOTDIR%\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\bin\%configuration%\Teleopti.Ccc.SmartClientPortal.Shell.exe.config
SET commonFolder=%ROOTDIR%\.debug-setup\common
ECHO %configPath%
ECHO %commonFolder%


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