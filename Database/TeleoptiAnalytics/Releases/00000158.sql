/* 
Trunk initiated: 
2009-10-06 
08:07
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (158,'7.0.158') 
