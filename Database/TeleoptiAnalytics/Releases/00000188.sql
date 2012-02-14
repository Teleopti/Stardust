/* 
Trunk initiated: 
2009-12-16 
17:52
By: TOPTINET\andersf 
On ANDERSFNC8430 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2009-xx-xx  
--Desc: Because ...  
----------------  
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (188,'7.1.188') 
