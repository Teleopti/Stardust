@echo off & setLocal enableDELAYedeXpansion
::---------------------
::Get content
::Uninstall customer config
::Install customer config files
::Copy to content foler
::cxpack
::---------------------

::init
SET customerFile=%1
SET customer=%customerFile:~0,-4%
SET customerSSLCertConfig=%customer%.SSLcert
SET customerRDPCertConfig=%customer%.RDPcert


::Get base url into variable
SET /P baseurl=< Customer\%customer%.BaseUrl

::Replace tempoprary txt-file for Msi configuration
COPY Customer\%customerFile% MSIInput.txt /Y

::Replace Baseurl accoring to settings
cscript replace.vbs $(BASEURL) %baseurl% MSIInput.txt

::Uninstall customer config
MSIExec /uninstall "%msi%" /passive

::Get all input on one string
for /f "tokens=* delims= " %%a in (MSIInput.txt) do (
set S=!S!%%a 
)
::set S=!S:~0,-1!
set S=MSIExec /i "%msi%" INSTALLDIR="%ConfigPath%" !S! /passive
> installConfig.bat echo.!S!

::Install customer config files
CALL installConfig.bat

::AzureConfig is added by WISE
SET ConfigPath=%ConfigPath%\AzureConfig
IF NOT EXIST "%ConfigPath%\DummyFolder" mkdir "%ConfigPath%\DummyFolder"

::Copy Config (3) to Content foler (2)
for /f "tokens=2,3 delims=," %%g in (contentMapping.txt) do xcopy /e /d /y "%ConfigPath%\%%h" "%ContentDest%\%%g\" 

::Sign ClickOnce
SET ClickOnceSignPath=%ContentDest%\TeleoptiCCC\Tools\ClickOnceSign
SET ClientPath=%ContentDest%\Client
SET MyTimePath=%ContentDest%\MyTime
CD "%ClickOnceSignPath%"
"%ClickOnceSignPath%\ClickOnceSign.exe" -s -a Teleopti.Ccc.SmartClientPortal.Shell.application -m "Teleopti.Ccc.SmartClientPortal.Shell.exe.manifest" -u "https://%baseurl%/Client/" -c "%ClickOnceSignPath%\TemporaryKey.pfx" -p "" -dir "%ClientPath%"
"%ClickOnceSignPath%\ClickOnceSign.exe" -s -a Teleopti.Ccc.AgentPortal.application -m "Teleopti.Ccc.AgentPortal.exe.manifest" -u "https://%baseurl%/MyTime/" -c "%ClickOnceSignPath%\TemporaryKey.pfx" -p "" -dir "%MyTimePath%"
CD "%ROOTDIR%"

::copy and update ServiceDefinition.csdef.BuildTemplate
COPY ServiceDefinition.csdef.BuildTemplate ServiceDefinition.csdef /Y
COPY ServiceConfig.cscfg.BuildTemplate ServiceConfig.cscfg /Y

::Replace SSL config in Azure config files
for /f "tokens=1,2 delims=," %%g in (Customer\%customerSSLCertConfig%) do cscript replace.vbs %%g %%h ServiceDefinition.csdef
for /f "tokens=1,2 delims=," %%g in (Customer\%customerSSLCertConfig%) do cscript replace.vbs %%g %%h ServiceConfig.cscfg

::Deploy the customer .cscfg-file
ECHO COPY "ServiceConfig.cscfg" "%output%\%customer%.cscfg" /Y
COPY "ServiceConfig.cscfg" "%output%\%customer%.cscfg" /Y

::cxpack
echo building cspack ...
if not exist "%output%" MKDIR "%output%"
"c:\Program Files\Windows Azure SDK\v1.6\bin\cspack.exe" "ServiceDefinition.csdef" /role:TeleoptiCCC;Temp\AzureContent\TeleoptiCCC /out:"%output%\%customer%.cspkg"
echo building cspack. done!
