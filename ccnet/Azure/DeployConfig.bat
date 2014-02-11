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
SET customerBlobStorageConfig=%customer%.BlobStorage

SET DataSourceName=
SET baseurl=

::Get data Soruce name
DIR
ECHO for /f "tokens=1,2 delims=," %%g IN ('findstr /C:"DataSourceName" /I .\Customer\%customer%.BlobStorage') do CALL SetDataSource %%h
for /f "tokens=1,2 delims=," %%g IN ('findstr /C:"DataSourceName" /I .\Customer\%customer%.BlobStorage') do CALL :SetDataSource %%h
SET baseurl=%DataSourceName%.teleopticloud.com

ECHO customerFile is: %customerFile%
ECHO DataSourceName is: %DataSourceName%

::Get customer config settings.txt
COPY Customer\%customerFile% "%ContentDest%\TeleoptiCCC\Tools\SupportTools\settings.txt" /Y

::Replace Baseurl accoring to settings
cscript replace.vbs $(BASEURL) %baseurl% "%ContentDest%\TeleoptiCCC\Tools\SupportTools\settings.txt"

::get Demo RTA settings into settings.txt
ECHO $^(RTA_STATE_CODE^)^|ACW,ADMIN,EMAIL,IDLE,InCall,LOGGED ON,OFF,Ready,WEB >> "%ContentDest%\TeleoptiCCC\Tools\SupportTools\settings.txt"
ECHO $^(RTA_QUEUE_ID^)^|2001,2002,0063,2000,0019,0068,0085,0202,0238,2003 >> "%ContentDest%\TeleoptiCCC\Tools\SupportTools\settings.txt"

ECHO "%ContentDest%\TeleoptiCCC\Tools\SupportTools\Teleopti.Support.Tool.exe" -MOAzure
"%ContentDest%\TeleoptiCCC\Tools\SupportTools\Teleopti.Support.Tool.exe" -MOAzure

::Replace dataSouceName in nhib
cscript replace.vbs "Teleopti CCC" "%DataSourceName%" "%ContentDest%\SDK\TeleoptiCCC7.nhib.xml"
cscript replace.vbs "Teleopti CCC" "%DataSourceName%" "%ContentDest%\TeleoptiCCC\Services\ETL\Tool\TeleoptiCCC7.nhib.xml"
cscript replace.vbs "Teleopti CCC" "%DataSourceName%" "%ContentDest%\TeleoptiCCC\Services\ETL\Service\TeleoptiCCC7.nhib.xml"
cscript replace.vbs "Teleopti CCC" "%DataSourceName%" "%ContentDest%\Web\TeleoptiCCC7.nhib.xml"

::Sign ClickOnce
SET ClickOnceSignPath=%ContentDest%\TeleoptiCCC\Tools\ClickOnceSign
ECHO ClickOnceSignPath: %ClickOnceSignPath%
SET ClientPath=%ContentDest%\Client
SET MyTimePath=%ContentDest%\MyTime

CD "%ClickOnceSignPath%"
ECHO ClickOnceSign.exe -s -a Teleopti.Ccc.SmartClientPortal.Shell.application -m "Teleopti.Ccc.SmartClientPortal.Shell.exe.manifest" -u "https://%baseurl%/Client/" -c "%ClickOnceSignPath%\TemporaryKey.pfx" -p "" -dir "%ClientPath%"
ClickOnceSign.exe -s -a Teleopti.Ccc.SmartClientPortal.Shell.application -m "Teleopti.Ccc.SmartClientPortal.Shell.exe.manifest" -u "https://%baseurl%/Client/" -c "%ClickOnceSignPath%\TemporaryKey.pfx" -p "" -dir "%ClientPath%"
if %errorlevel% NEQ 0 (
SET /A ERRORLEV=203
GOTO :Error
)

ECHO ClickOnceSign.exe -s -a Teleopti.Ccc.AgentPortal.application -m "Teleopti.Ccc.AgentPortal.exe.manifest" -u "https://%baseurl%/MyTime/" -c "%ClickOnceSignPath%\TemporaryKey.pfx" -p "" -dir "%MyTimePath%"
ClickOnceSign.exe -s -a Teleopti.Ccc.AgentPortal.application -m "Teleopti.Ccc.AgentPortal.exe.manifest" -u "https://%baseurl%/MyTime/" -c "%ClickOnceSignPath%\TemporaryKey.pfx" -p "" -dir "%MyTimePath%"
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

::BlobStorage config for each customer
for /f "tokens=1,2 delims=," %%g in (Customer\%customerBlobStorageConfig%) do cscript replace.vbs %%g %%h ServiceConfig.cscfg

::DataSourceName

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
:SetDataSource
::Get base url into variable
SET DataSourceName=%~1
Exit /b

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