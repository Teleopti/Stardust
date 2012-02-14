/* 
Trunk initiated: 
2010-05-10 
15:18
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2009-xx-xx  
--Desc: Because ...  
----------------  
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (255,'7.1.255') 

----------------  
--Name: David Jonsson/Tamas Balog
--Date: 2010-05-17
--Desc: ApplicationFunciton Out of Sync! Fixed on Branch 255
----------------  
update ApplicationFunction
set IsDeleted = 1
where FunctionCode = 'ModifyPersonTimeActivity'
and ForeignId = '0015-1001'