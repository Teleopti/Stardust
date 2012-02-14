/* 
BuildTime is: 
2009-04-22 
14:09
*/ 
/* 
Trunk initiated: 
2009-04-16 
11:49
By: TOPTINET\davidj 
On TELEOPTI625 
*/
----------------  
--Name: David Jonsson
--Date: 2009-04-20
--Desc: Changed column names  
----------------  
EXEC dbo.sp_rename @objname=N'[msg].[Filter].[ParentObjectId]', @newname=N'ReferenceObjectId', @objtype=N'COLUMN'
EXEC dbo.sp_rename @objname=N'[msg].[Filter].[ParentObjectType]', @newname=N'ReferenceObjectType', @objtype=N'COLUMN'
EXEC dbo.sp_rename @objname=N'[msg].[Event].[ParentObjectId]', @newname=N'ReferenceObjectId', @objtype=N'COLUMN'
EXEC dbo.sp_rename @objname=N'[msg].[Event].[ParentObjectType]', @newname=N'ReferenceObjectType', @objtype=N'COLUMN'
 
GO 
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (94,'7.0.94') 
