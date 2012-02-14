/* 
Trunk initiated: 
2010-04-09 
08:54
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
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (228,'7.1.228') 
