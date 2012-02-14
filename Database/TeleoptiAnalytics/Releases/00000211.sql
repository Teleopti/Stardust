/* 
Trunk initiated: 
2010-03-05 
19:36
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
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (211,'7.1.211') 
