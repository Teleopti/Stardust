/* 
Trunk initiated: 
2010-10-04 
10:50
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (300,'7.1.300') 
