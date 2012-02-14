/* 
Trunk initiated: 
2009-08-26 
16:02
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2009-xx-xx  
--Desc: Because ...  
----------------  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (143,'7.0.143') 
