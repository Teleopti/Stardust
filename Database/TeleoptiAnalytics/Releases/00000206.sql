/* 
Trunk initiated: 
2010-02-03 
16:08
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-02-04
--Desc: Read dirty from Agg
----------------  
UPDATE mart.sys_crossdatabaseview
SET view_definition = 'SELECT * FROM $$$target$$$.dbo.agent_logg WITH (NOLOCK)'
WHERE view_id = 2

UPDATE mart.sys_crossdatabaseview
SET view_definition = 'SELECT * FROM $$$target$$$.dbo.queue_logg  WITH (NOLOCK)'
WHERE view_id = 5

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossDatabaseView_load]') AND type in (N'P', N'PC'))
EXEC mart.sys_crossDatabaseView_load 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (206,'7.1.206') 
