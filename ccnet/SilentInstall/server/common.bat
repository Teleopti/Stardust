@echo off
::=====================
::Some logic to handle settings based on the ones set in Machine config batch file
::=====================
IF %SSL% EQU 1 (
SET DNS_ALIAS=https://%AppServer%/
SET HTTPGETENABLED=false
SET HTTPSGETENABLED=true
SET SDK_SSL_MEX_BINDING=mexHttpsBinding
SET SDK_SSL_SECURITY_MODE=Transport
) ELSE (
SET DNS_ALIAS=http://%AppServer%/
SET HTTPGETENABLED=true
SET HTTPSGETENABLED=false
SET SDK_SSL_MEX_BINDING=mexHttpBinding
SET SDK_SSL_SECURITY_MODE=TransportCredentialOnly
)


SET SQL_SERVER_NAME=%DBServerInstance%

IF "%DB_AdminAccess%"=="WINAUTH" (
SET WISE_SQL_CONN_STR=Data Source=%DBServerInstance%;Integrated Security=SSPI
) else (
SET WISE_SQL_CONN_STR=Data Source=%DBServerInstance%;User Id=%DB_ADMIN_SQLLOGIN%;Password=%DB_ADMIN_PWD%
)

IF "%DB_EndUserAccess%"=="WINAUTH" (
SET SQL_AUTH_STRING=Data Source=%DBServerInstance%;Integrated Security=SSPI
SET MYUSERNAME=%SvcAccount%
SET MYPASSWORD=%SvcAccountPwd%
SET ETLUSERNAME=%SvcAccount%
SET ETLPASSWORD=%SvcAccountPwd%
) else (
SET SQL_AUTH_STRING=Data Source=%DBServerInstance%;User Id=%DB_ENDUSER_SQLLOGIN%;Password=%DB_ENDUSER_PWD%
SET MYUSERNAME=
SET MYPASSWORD=
SET ETLUSERNAME=
SET ETLPASSWORD=
SET SQL_USER_NAME=%DB_ENDUSER_SQLLOGIN%
SET SQL_USER_PASSWORD=%DB_ENDUSER_PWD%
)

SET CLICKONCE_CLIENT_URL=%DNS_ALIAS%TeleoptiCCC/Client/
SET CLICKONCE_MYTIME_URL=%DNS_ALIAS%TeleoptiCCC/MyTime/
SET MATRIX_WEB_SITE_URL=%DNS_ALIAS%TeleoptiCCC/Analytics

SET DB_ANALYTICS_STAGE=%DB_ANALYTICS%
SET DB_MESSAGING=%DB_ANALYTICS%
SET IIS_AUTH=Windows

SET SITEPATH=%INSTALLDIR%TeleoptiCCC\SDK\

SET AGENT_SERVICE=%DNS_ALIAS%TeleoptiCCC/SDK/TeleoptiCccSdkService.svc
SET RTA_SERVICE=%DNS_ALIAS%TeleoptiCCC/RTA/TeleoptiRtaService.svc
SET WEB_BROKER=%DNS_ALIAS%TeleoptiCCC/broker/

SET CONTEXT_HELP_URL=http://wiki.teleopti.com/TeleoptiCCC/Special:MyLanguage/