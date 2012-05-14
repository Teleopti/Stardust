
GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (362,'7.2.362') 
