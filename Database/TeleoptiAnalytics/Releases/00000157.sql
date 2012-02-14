/* 
Trunk initiated: 
2009-09-30 
10:14
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (157,'7.0.157') 
