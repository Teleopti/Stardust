/* 
Trunk initiated: 
2010-06-04 
13:10
By: TOPTINET\johanr 
On TELEOPTI565 
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
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (276,'7.1.276') 
