/* 
Trunk initiated: 
2010-02-01 
14:06
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Roger Kratz  
--Date: 2010-02-03 
--Desc: Because this (newly) added constraing caused crashes  
----------------  
ALTER TABLE [dbo].[PersonDayOff] DROP CONSTRAINT [UC_PersonAnchorScenario]
GO
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (205,'7.1.205') 
