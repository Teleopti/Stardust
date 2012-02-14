/* 
Manuell
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-04-13
--Desc: Get in version sync
----------------  
  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (229,'7.1.229') 

----------------  
--Name: Robin Karlsson
--Date: 2010-04-14
--Desc: Added after branch code was taken to fix bug 10168
----------------  
Update [RTA].[ExternalAgentState] Set BatchId=TimestampValue Where IsSnapshot=0 and BatchId='17530101'
GO