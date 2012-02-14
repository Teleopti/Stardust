/* 
Trunk initiated: 
2010-05-11 
09:07
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Anders F 
--Date: 2010-05-19  
--Desc: Peter A says this configuration id is no longer in use 
----------------  
delete from msg.configuration where configurationid = 13
GO
----------------  
--Name: Robin K
--Date: 2010-05-20  
--Desc: Now DataSourceId is used in ExternalAgentState instead of log object id 
----------------
EXEC dbo.sp_rename @objname=N'[RTA].[ExternalAgentState].[LogObjectId]', @newname=N'DataSourceId', @objtype=N'COLUMN'
GO

ALTER TABLE RTA.ExternalAgentState DROP COLUMN source_id
GO 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (264,'7.1.264') 
