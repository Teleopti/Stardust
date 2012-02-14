/* 
Trunk initiated: 
2010-06-17 
15:14
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2010-xx-xx  
--Desc: Because ...  
----------------  
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (287,'7.1.287') 
