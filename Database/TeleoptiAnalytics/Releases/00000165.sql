/* 
Trunk initiated: 
2009-10-13 
08:27
By: TOPTINET\johanr 
On TELEOPTI565 
*/
GO 
EXEC mart.sys_crossdatabaseview_load  
GO 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (165,'7.0.165') 
