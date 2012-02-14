/* 
Trunk initiated: 
2009-09-16 
13:03
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (149,'7.0.149') 
