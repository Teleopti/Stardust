/* 
Trunk initiated: 
2009-09-14 
08:27
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (147,'7.0.147') 
