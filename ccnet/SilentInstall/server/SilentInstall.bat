@echo off
COLOR A
cls

::init
SET /A ERRORLEV=0
SET /A msierror=0
SET CCCServer=52613B22-2102-4BFB-AAFB-EF420F3A24B5
SET EtlSrvname=TeleoptiETLService

::change drive letter
%~d0
::change path
CD "%~dp0"

::check argument
set argC=0
for %%x in (%*) do Set /A argC+=1
if %argC% EQU 0 goto :userinput
goto :commandlineInput
goto :help

:userinput
set /P msipath=Provide the full path to your msi file: 
set /P SvcAccount=Provide a valid SvcAccount: 
set /P SvcAccountPwd=... and password: 
set /P machineName=What is your bat file name ^(no extension^): 
goto :start

:commandlineInput
set msipath=%~1
set machineName=%~2
set SvcAccount=%~3
set SvcAccountPwd=%~4
set TempFile=%~5
goto :start

:Start
::remove any double quotes
set msipath=%msipath:"=%

::set log file path
CALL :SetLogPath "%msipath%" logFile

::Set machine specific config
CALL "%~dp0machine\%machineName%.bat" 

::Set common config based on machine specific config
CALL "%~dp0common.bat" 

::verify default website
cscript "%~dp0tools\BrowseUrl.vbs" "%DNS_ALIAS%"
if %errorlevel% neq 200 (
COLOR E
ECHO WARNING: could not send GET request to SDK!
ECHO Your default web site is not answering on: %DNS_ALIAS%
ECHO Will continue anyway
ping -n 4 127.0.0.1 > NUL
)
ECHO.

::kick msi
ECHO Running installation ....
ECHO Please don't close any command line windows!
ECHO.
if "%TempFile%"=="" (
ECHO MSIExec /i "%msipath%" ADDLOCAL="%ADDLOCAL%" CLICKONCE_CLIENT_URL="%CLICKONCE_CLIENT_URL%" CLICKONCE_MYTIME_URL="%CLICKONCE_MYTIME_URL%" CLICKONCE_SIGN_METHOD="TEMP" INSTALLDIR="%INSTALLDIR%" MATRIX_WEB_SITE_URL="%MATRIX_WEB_SITE_URL%" SQL_SERVER_NAME="%SQL_SERVER_NAME%" SQL_SERVER_AUTH="NT" SQL_SERVER_USER="" SQL_SERVER_PASS="" WISE_SQL_CONN_STR="%WISE_SQL_CONN_STR%" SQL_USER_AUTH="NT" SQL_USER_NAME="" SQL_USER_PASSWORD="" SQL_AUTH_STRING="%SQL_AUTH_STRING%" MYPASSWORD="%MYPASSWORD%" MYUSERNAME="%MYUSERNAME%" DB_WINGROUP="%DB_WINGROUP%" DB_ANALYTICS="%DB_ANALYTICS%" DB_ANALYTICS_STAGE="%DB_ANALYTICS_STAGE%" DB_CCC7="%DB_CCC7%" DB_CCCAGG="%DB_CCCAGG%" DB_MESSAGING="%DB_MESSAGING%" DNS_ALIAS="%DNS_ALIAS%" AGENT_SERVICE="%AGENT_SERVICE%" SITEPATH="%SITEPATH%" SDK_CRED_PROT="Ntlm" SDK_SSL_MEX_BINDING="%SDK_SSL_MEX_BINDING%" SDK_SSL_SECURITY_MODE="%SDK_SSL_SECURITY_MODE%" HTTPGETENABLED="%HTTPGETENABLED%" HTTPSGETENABLED="%HTTPSGETENABLED%" CONTEXT_HELP_URL="%CONTEXT_HELP_URL%" WEB_BROKER="%WEB_BROKER%" WEB_BROKER_BACKPLANE="%WEB_BROKER_BACKPLANE%" SERVICEBUSENABLED="true" IIS_AUTH="%IIS_AUTH%" AS_DATABASE="%AS_DATABASE%" AS_SERVER_NAME="%AS_SERVER_NAME%" PM_ANONYMOUS_DOMAINUSER="" PM_ANONYMOUS_PWD="" PM_AUTH_MODE="Windows" PM_PROCESS_CUBE="FALSE" PM_ASMX="PM not installed" PM_INSTALL="false" RTA_SERVICE="%RTA_SERVICE%" ETLUSERNAME="%ETLUSERNAME%" ETLPASSWORD="%ETLPASSWORD%" /qn /l* "%logfile%"
) else (
ECHO MSIExec /i "%msipath%" ADDLOCAL="%ADDLOCAL%" CLICKONCE_CLIENT_URL="%CLICKONCE_CLIENT_URL%" CLICKONCE_MYTIME_URL="%CLICKONCE_MYTIME_URL%" CLICKONCE_SIGN_METHOD="TEMP" INSTALLDIR="%INSTALLDIR%" MATRIX_WEB_SITE_URL="%MATRIX_WEB_SITE_URL%" SQL_SERVER_NAME="%SQL_SERVER_NAME%" SQL_SERVER_AUTH="NT" SQL_SERVER_USER="" SQL_SERVER_PASS="" WISE_SQL_CONN_STR="%WISE_SQL_CONN_STR%" SQL_USER_AUTH="NT" SQL_USER_NAME="" SQL_USER_PASSWORD="" SQL_AUTH_STRING="%SQL_AUTH_STRING%" MYPASSWORD="%MYPASSWORD%" MYUSERNAME="%MYUSERNAME%" DB_WINGROUP="%DB_WINGROUP%" DB_ANALYTICS="%DB_ANALYTICS%" DB_ANALYTICS_STAGE="%DB_ANALYTICS_STAGE%" DB_CCC7="%DB_CCC7%" DB_CCCAGG="%DB_CCCAGG%" DB_MESSAGING="%DB_MESSAGING%" DNS_ALIAS="%DNS_ALIAS%" AGENT_SERVICE="%AGENT_SERVICE%" SITEPATH="%SITEPATH%" SDK_CRED_PROT="Ntlm" SDK_SSL_MEX_BINDING="%SDK_SSL_MEX_BINDING%" SDK_SSL_SECURITY_MODE="%SDK_SSL_SECURITY_MODE%" HTTPGETENABLED="%HTTPGETENABLED%" HTTPSGETENABLED="%HTTPSGETENABLED%" CONTEXT_HELP_URL="%CONTEXT_HELP_URL%" WEB_BROKER="%WEB_BROKER%" WEB_BROKER_BACKPLANE="%WEB_BROKER_BACKPLANE%" SERVICEBUSENABLED="true" IIS_AUTH="%IIS_AUTH%" AS_DATABASE="%AS_DATABASE%" AS_SERVER_NAME="%AS_SERVER_NAME%" PM_ANONYMOUS_DOMAINUSER="" PM_ANONYMOUS_PWD="" PM_AUTH_MODE="Windows" PM_PROCESS_CUBE="FALSE" PM_ASMX="PM not installed" PM_INSTALL="false" RTA_SERVICE="%RTA_SERVICE%" ETLUSERNAME="%ETLUSERNAME%" ETLPASSWORD="%ETLPASSWORD%" /qn /l* "%logfile%" > "%TempFile%"
)

IF %msierror% NEQ 0 (
SET /A ERRORLEV=2
GOTO :error
)

::done
GOTO :eof

:SetLogPath
SETLOCAL
SET logfile=%~dp0
SET logfile=%logfile%%~n1
SET logfile=%logfile%.log
(
ENDLOCAL
set "%~2=%logFile%"
)
goto:eof

:help
COLOR E
ECHO Run this batch file manully with no paramters, to enter input manually.
ECHO OR, Run this batch file from command line with paramters:
ECHO Msipath ^{local path^} ConfigName ^{Name of config file^} Action ^{show^|install^}
ECHO.
ECHO Example call:
ECHO %~nx0 "MyDomain\SvcAccount" "password" "C:\Temp\Teleopti CCC 7.3.374.10701.msi" "MyMachineFile"
GOTO :EOF

:Error
COLOR C
ECHO.
ECHO --------
IF %ERRORLEV% NEQ 0 ECHO Errors found!
IF %ERRORLEV% EQU 1 ECHO The msi is already installed. You must uninstall the current version before applying a new one
IF %ERRORLEV% EQU 2 ECHO The msi installation failed with errorlevel: %msierror%
ECHO.
ECHO --------
GOTO :EOF


:EOF