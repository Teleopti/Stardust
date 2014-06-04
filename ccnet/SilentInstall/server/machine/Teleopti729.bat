@echo off
::=====================
::hard coded for each environment
::=====================
SET INSTALLDIR=C:\Program Files (x86)\Teleopti\
SET DBServerInstance=Teleopti729.toptinet.teleopti.com
SET AppServer=Teleopti729.toptinet.teleopti.com
SET DB_ANALYTICS=TeleoptiAnalytics_Demo
SET DB_CCC7=TeleoptiCCC7_Demo
SET DB_CCCAGG=TeleoptiCCC7Agg_Demo
SET /A SSL=1
SET /A SQLSSL=0
SET ADDLOCAL=Database,AgentPortalWeb,Analytics,ClickOnce,AgentPortal,AdminClient,SDK,ServiceBus,ETL,Service,Tool,RTA,MessageBroker

SET DB_AdminAccess=WINAUTH
SET DB_EndUserAccess=WINAUTH
SET DB_WINGROUP=Teleopti729\TeleoptiCCC_Users