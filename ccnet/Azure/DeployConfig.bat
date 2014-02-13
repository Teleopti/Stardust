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
IF %ERRORLEV% EQU 203 ECHO Could not sign Clickonce for SmartClientPortal & exit /b 203
IF %ERRORLEV% EQU 204 ECHO Could not sign Clickonce for AgentPortal & exit /b 204

:EOF
exit /b 0