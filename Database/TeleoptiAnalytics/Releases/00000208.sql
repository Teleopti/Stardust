/* 
Trunk initiated: 
2010-03-01 
11:43
By: TOPTINET\andersf 
On ANDERSFNC8430 
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-03-02
--Desc: SQL Server maintplans crashes
----------------
ALTER INDEX [UQ_Configuration_ConfigurationName] ON [msg].[Configuration] SET (ALLOW_PAGE_LOCKS = ON) 
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (208,'7.1.208') 
