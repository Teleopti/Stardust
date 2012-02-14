/* 
Trunk initiated: 
2010-06-11 
13:36
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 

 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (283,'7.1.283') 
