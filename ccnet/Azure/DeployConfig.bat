@echo off & setLocal enableDELAYedeXpansion
::Depends on varible %AZUREDIR%, %output%, %msi%, %ConfigPath%
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
::SET customerRDPCertConfig=%customer%.RDPcert

ECHO customerFile is: %customerFile%

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
IF %ERRORLEVEL% NEQ 0 (
SET ERRORLEV=201
GOTO Error
)

::AzureConfig is added by WISE
SET ConfigPath=%ConfigPath%\AzureConfig
IF NOT EXIST "%ConfigPath%\DummyFolder" mkdir "%ConfigPath%\DummyFolder"

::Copy Config (3) to Content foler (2)
for /f "tokens=2,3 delims=," %%g in (contentMapping.txt) do xcopy /e /d /y "%ConfigPath%\%%h" "%ContentDest%\%%g\" 

::Settings for Sql Queues and Service bus

COPY SqlServiceBusConfig\Teleopti.Ccc.Sdk.ServiceBus.Client.config %ContentDest%\SDK\Bin\Teleopti.Ccc.Sdk.ServiceBus.Client.config /Y
COPY SqlServiceBusConfig\Teleopti.Ccc.Sdk.ServiceBus.Client.config %ContentDest%\Web\Bin\Teleopti.Ccc.Sdk.ServiceBus.Client.config /Y
COPY SqlServiceBusConfig\RequestQueue.config %ContentDest%\TeleoptiCCC\Services\ServiceBus\RequestQueue.config /Y
COPY SqlServiceBusConfig\RequestQueue.config %ContentDest%\TeleoptiCCC\Services\ServiceBus\GeneralQueue.config /Y
COPY SqlServiceBusConfig\RequestQueue.config %ContentDest%\TeleoptiCCC\Services\ServiceBus\DenormalizeQueue.config /Y

cscript replace.vbs $(BASEURL) %baseurl% "%ContentDest%\SDK\Bin\Teleopti.Ccc.Sdk.ServiceBus.Client.config"
cscript replace.vbs $(BASEURL) %baseurl% "%ContentDest%\Web\Bin\Teleopti.Ccc.Sdk.ServiceBus.Client.config"
cscript replace.vbs $(BASEURL) %baseurl% "%ContentDest%\TeleoptiCCC\Services\ServiceBus\RequestQueue.config"
cscript replace.vbs $(BASEURL) %baseurl% "%ContentDest%\TeleoptiCCC\Services\ServiceBus\GeneralQueue.config"
cscript replace.vbs $(BASEURL) %baseurl% "%ContentDest%\TeleoptiCCC\Services\ServiceBus\DenormalizeQueue.config"

SET /P analyticsconnection=< Customer\%customer%.AnalyticsConnection

cscript replace.vbs $(ANALYTICS_CONNECTION) %analyticsconnection% "%ContentDest%\SDK\Bin\Teleopti.Ccc.Sdk.ServiceBus.Client.config"
cscript replace.vbs $(ANALYTICS_CONNECTION) %analyticsconnection% "%ContentDest%\Web\Bin\Teleopti.Ccc.Sdk.ServiceBus.Client.config"
cscript replace.vbs $(ANALYTICS_CONNECTION) %analyticsconnection% "%ContentDest%\TeleoptiCCC\Services\ServiceBus\RequestQueue.config"
cscript replace.vbs $(ANALYTICS_CONNECTION) %analyticsconnection% "%ContentDest%\TeleoptiCCC\Services\ServiceBus\GeneralQueue.config"
cscript replace.vbs $(ANALYTICS_CONNECTION) %analyticsconnection% "%ContentDest%\TeleoptiCCC\Services\ServiceBus\DenormalizeQueue.config"

::Sign ClickOnce
SET ClickOnceSignPath=%ContentDest%\TeleoptiCCC\Tools\ClickOnceSign
SET ClientPath=%ContentDest%\Client
SET MyTimePath=%ContentDest%\MyTime
CD "%ClickOnceSignPath%"
"%ClickOnceSignPath%\ClickOnceSign.exe" -s -a Teleopti.Ccc.SmartClientPortal.Shell.application -m "Teleopti.Ccc.SmartClientPortal.Shell.exe.manifest" -u "https://%baseurl%/Client/" -c "%ClickOnceSignPath%\TemporaryKey.pfx" -p "" -dir "%ClientPath%"
if %errorlevel% NEQ 0 (
SET /A ERRORLEV=203
GOTO :Error
)
"%ClickOnceSignPath%\ClickOnceSign.exe" -s -a Teleopti.Ccc.AgentPortal.application -m "Teleopti.Ccc.AgentPortal.exe.manifest" -u "https://%baseurl%/MyTime/" -c "%ClickOnceSignPath%\TemporaryKey.pfx" -p "" -dir "%MyTimePath%"
if %errorlevel% NEQ 0 (
SET /A ERRORLEV=204
GOTO :Error
)
CD "%AZUREDIR%"

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
ECHO "c:\Program Files\Windows Azure SDK\v1.6\bin\cspack.exe" "ServiceDefinition.csdef" /role:TeleoptiCCC;Temp\AzureContent\TeleoptiCCC /out:"%output%\%customer%.cspkg"
"c:\Program Files\Windows Azure SDK\v1.6\bin\cspack.exe" "ServiceDefinition.csdef" /role:TeleoptiCCC;Temp\AzureContent\TeleoptiCCC /out:"%output%\%customer%.cspkg"
if %errorlevel% NEQ 0 (
SET /A ERRORLEV=202
GOTO :Error
)
echo building cspack. done!

GOTO :EOF

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 201 ECHO Could not install temporary msi-file & exit /b 201
IF %ERRORLEV% EQU 202 ECHO Could not build using SCPACK.exe install temporary msi-file & exit /b 202
IF %ERRORLEV% EQU 203 ECHO Could not sign Clickonce for SmartClientPortal & exit /b 203
IF %ERRORLEV% EQU 204 ECHO Could not sign Clickonce for AgentPortal & exit /b 204

:EOF
exit /b 0