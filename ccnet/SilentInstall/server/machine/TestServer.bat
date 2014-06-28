@echo off
::=====================
::hard coded for each environment
::=====================
SET INSTALLDIR=C:\Program Files (x86)\Teleopti\
SET DBServerInstance=tcp:ARES\CCC
SET AppServer=%COMPUTERNAME%
SET DB_ANALYTICS=%COMPUTERNAME%_TeleoptiAnalytics
SET DB_CCC7=%COMPUTERNAME%_TeleoptiCCC7
SET DB_CCCAGG=%COMPUTERNAME%_TeleoptiCCCAgg
SET DB_WINGROUP=TOPTINET\#secDevelopCCC
SET /A SSL=0
SET ADDLOCAL=Database,AgentPortalWeb,Analytics,ClickOnce,AgentPortal,AdminClient,SDK,ServiceBus,ETL,Service,Tool,RTA,MessageBroker
SET /A SQLSSL=0

SET DB_AdminAccess=WINAUTH
::if you specify:
::SET DB_AdminAccess=SQL"
::Then you must also provide:
::SET DB_ADMIN_SQLLOGIN
::SET DB_ADMIN_PWD

SET DB_EndUserAccess=SQL
::if you specify:
::SET DB_EndUserAccess=SQL
::you must also provide:
SET DB_ENDUSER_SQLLOGIN=TeleoptiDemoUser
SET DB_ENDUSER_PWD=TeleoptiDemoPwd2

SET DEFAULT_IDENTITY_PROVIDER=Teleopti
SET HOST_NAME=%COMPUTERNAME%