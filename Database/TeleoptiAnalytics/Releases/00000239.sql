/* 
Trunk initiated: 
2010-04-21 
19:04
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Robin Karlsson 
--Date: 2010-05-02
--Desc: Preparations for upcoming RTA work during the summer
----------------  
ALTER TABLE mart.sys_datasource ADD source_id NVARCHAR(50) NULL
GO

UPDATE mart.sys_datasource SET source_id = CAST(log_object_id AS NVARCHAR(50))
GO

ALTER TABLE rta.ExternalAgentState ADD source_id NVARCHAR(50) NULL
GO 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (239,'7.1.239') 
