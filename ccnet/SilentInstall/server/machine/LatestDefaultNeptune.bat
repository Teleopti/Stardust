@echo off
::=====================
::hard coded for each environment
::=====================
SET INSTALLDIR=C:\Program Files (x86)\Teleopti\
SET DBServerInstance=neptune.toptinet.teleopti.com
SET AppServer=silentcccinstall.teleopti.com
SET DB_ANALYTICS=TeleoptiAnalytics_Demo
SET DB_CCC7=TeleoptiCCC7_Demo
SET DB_CCCAGG=TeleoptiCCC7Agg_Demo
SET DB_WINGROUP=NEPTUNE\EverybodyAtTeleopti
SET /A SSL=1
SET ADDLOCAL=Database,AgentPortalWeb,Analytics,ClickOnce,AgentPortal,AdminClient,SDK,ServiceBus,ETL,Service,Tool,RTA,MessageBroker,RestoreDemo
SET /A SQLSSL=1

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
SET WINDOWS_CLAIM_PROVIDER=<add identifier="urn:Windows" displayName="Windows" url="http://%AppServer%/TeleoptiCCC/WindowsIdentityProvider" protocolHandler="OpenIdHandler" />
SET TELEOPTI_CLAIM_PROVIDER=<add identifier="urn:Teleopti" displayName="Teleopti application" url="http://%AppServer%/TeleoptiCCC/Web/sso/" protocolHandler="OpenIdHandler" />
SET HOST_NAME=silentcccinstall.teleopti.com