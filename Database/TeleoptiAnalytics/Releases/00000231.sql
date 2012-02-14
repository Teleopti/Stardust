/* 
Trunk initiated: 
2010-04-14 
15:12
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Robin Karlsson
--Date: 2010-04-14
--Desc: To fix bug 10168
----------------  
Update [RTA].[ExternalAgentState] Set BatchId=TimestampValue Where IsSnapshot=0 and BatchId='17530101'
GO 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (231,'7.1.231') 
