@echo off
::=====================
::hard coded for each environment
::=====================
SET INSTALLDIR=C:\Program Files (x86)\Teleopti\
SET DBServerInstance=%COMPUTERNAME%.toptinet.teleopti.com
SET AppServer=%COMPUTERNAME%.toptinet.teleopti.com
SET DB_ANALYTICS=TeleoptiAnalytics_Demo
SET DB_CCC7=TeleoptiCCC7_Demo
SET DB_CCCAGG=TeleoptiCCC7Agg_Demo
SET DB_WINGROUP=%COMPUTERNAME%\TeleoptiCCC_Users
SET /A SSL=1
SET ADDLOCAL=Database,AgentPortalWeb,Analytics,ClickOnce,AgentPortal,AdminClient,SDK,ServiceBus,ETL,Service,Tool,RTA,MessageBroker,RestoreDemo,PM
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

::Pm Stuff
SET PM_INSTALL=True
SET AS_DATABASE=TeleoptiAnalytics_Demo
SET AS_SERVER_NAME=%COMPUTERNAME%.toptinet.teleopti.com
SET PM_ANONYMOUS_DOMAINUSER=
SET PM_ANONYMOUS_PWD=
SET PM_AUTH_MODE=Windows
SET PM_PROCESS_CUBE=TRUE

SET DEFAULT_IDENTITY_PROVIDER=Teleopti
SET HOST_NAME=%COMPUTERNAME%.toptinet.teleopti.com