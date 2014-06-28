@echo off
::=====================
::Some logic to handle settings based on the ones set in Machine config batch file
::=====================
IF "%SUB_SITE%"=="" SET SUB_SITE=TeleoptiWFM
SET TeleoptiAnalytics_Demo_Default=TeleoptiAnalytics_Demo
SET TeleoptiApp_Demo_Default=TeleoptiApp_Demo
SET TeleoptiAgg_Demo_Default=TeleoptiAgg_Demo

IF %SSL% EQU 1 (
SET DNS_ALIAS=https://%AppServer%/
SET SDK_SSL=True
) ELSE (
SET DNS_ALIAS=http://%AppServer%/
SET SDK_SSL=False
)

::If RestoreDemo then use default demo names
echo %ADDLOCAL% | findstr /C:"RestoreDemo"
if %ERRORLEVEL% equ 0 (
SET DB_ANALYTICS=%TeleoptiAnalytics_Demo_Default%
SET DB_CCC7=%TeleoptiApp_Demo_Default%
SET DB_CCCAGG=%TeleoptiAgg_Demo_Default%
)

::If user forget to set name use default demo names
IF "%DB_ANALYTICS%"=="" SET DB_ANALYTICS=%TeleoptiAnalytics_Demo_Default%
IF "%DB_CCC7%"=="" SET DB_CCC7=%TeleoptiApp_Demo_Default%
IF "%DB_CCCAGG%"=="" SET DB_CCCAGG=%TeleoptiAgg_Demo_Default%

SET SDK_CRED_PROT=Ntlm
SET SERVICEBUSENABLED=true
SET SQL_SERVER_NAME=%DBServerInstance%

IF "%DB_AdminAccess%"=="WINAUTH" (
SET SQL_SERVER_AUTH=NT
SET WISE_SQL_CONN_STR=Data Source=%DBServerInstance%;Integrated Security=SSPI
) else (
SET SQL_SERVER_AUTH=SQL
SET WISE_SQL_CONN_STR=Data Source=%DBServerInstance%;User Id=%DB_ADMIN_SQLLOGIN%;Password=%DB_ADMIN_PWD%
)

IF "%DB_EndUserAccess%"=="WINAUTH" (
SET SQL_AUTH_STRING=Data Source=%DBServerInstance%;Integrated Security=SSPI
SET SQL_USER_AUTH=NT
SET MYUSERNAME=%SvcAccount%
SET MYPASSWORD=%SvcAccountPwd%
SET ETLUSERNAME=%SvcAccount%
SET ETLPASSWORD=%SvcAccountPwd%
) else (
SET SQL_AUTH_STRING=Data Source=%DBServerInstance%;User Id=%DB_ENDUSER_SQLLOGIN%;Password=%DB_ENDUSER_PWD%
SET SQL_USER_AUTH=SQL
SET MYUSERNAME=
SET MYPASSWORD=
SET ETLUSERNAME=
SET ETLPASSWORD=
SET SQL_USER_NAME=%DB_ENDUSER_SQLLOGIN%
SET SQL_USER_PASSWORD=%DB_ENDUSER_PWD%
)

::SQL SSL if present
IF %SQLSSL% EQU 1 (
SET SQL_AUTH_STRING=%SQL_AUTH_STRING%;encrypt^=true;trustServerCertificate^=true
)

SET CLICKONCE_CLIENT_URL=%DNS_ALIAS%%SUB_SITE%/Client/
SET CLICKONCE_MYTIME_URL=%DNS_ALIAS%%SUB_SITE%/MyTime/
SET MATRIX_WEB_SITE_URL=%DNS_ALIAS%%SUB_SITE%/Analytics

SET DB_ANALYTICS_STAGE=%DB_ANALYTICS%
SET DB_MESSAGING=%DB_ANALYTICS%
SET IIS_AUTH=Windows

SET SITEPATH=%INSTALLDIR%TeleoptiCCC\SDK\

SET AGENT_SERVICE=%DNS_ALIAS%%SUB_SITE%/SDK/TeleoptiCccSdkService.svc
SET RTA_SERVICE=%DNS_ALIAS%%SUB_SITE%/RTA/TeleoptiRtaService.svc
SET WEB_BROKER=%DNS_ALIAS%%SUB_SITE%/broker/
SET WEB_BROKER_BACKPLANE=%DNS_ALIAS%%SUB_SITE%/Broker.backplane/backplane

SET CONTEXT_HELP_URL=http://wiki.teleopti.com/TeleoptiCCC/Special:MyLanguage/

::PM Stuff
IF "%PM_INSTALL%"=="True" (
SET AS_DATABASE=%AS_DATABASE%
SET AS_SERVER_NAME=%AS_SERVER_NAME%
SET PM_ANONYMOUS_DOMAINUSER=%PM_ANONYMOUS_DOMAINUSER%
SET PM_ANONYMOUS_PWD=%PM_ANONYMOUS_PWD%
SET PM_AUTH_MODE=%PM_AUTH_MODE%
SET PM_PROCESS_CUBE=%PM_PROCESS_CUBE%
SET PM_ASMX=%DNS_ALIAS%TeleoptiPM/services/Analyzer2005.asmx
SET PM_SERVICE=%DNS_ALIAS%%SUB_SITE%/PmService/PmService.svc
) ELSE (
SET PM_INSTALL=False
SET AS_DATABASE=
SET AS_SERVER_NAME=
SET PM_ANONYMOUS_DOMAINUSER=
SET PM_ANONYMOUS_PWD=
SET PM_AUTH_MODE=Windows
SET PM_PROCESS_CUBE=False
SET PM_ASMX=PM not installed
SET PM_SERVICE=PM not installed
)
::PM vs. ETL Service Account
IF "%PM_INSTALL%"=="True" (
	IF "%PM_AUTH_MODE%"=="Anonymous" (
	SET ETLUSERNAME=%PM_ANONYMOUS_DOMAINUSER%
	SET ETLPASSWORD=%PM_ANONYMOUS_PWD%
	)
)