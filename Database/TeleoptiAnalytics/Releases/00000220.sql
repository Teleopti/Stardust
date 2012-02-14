/* 
Trunk initiated: 
2010-03-22 
10:34
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2009-xx-xx  
--Desc: Because ...  
----------------  


----------------  
--Name: Henry Greijer
--Date: 2010-03-23  
--Desc: Part of 9347 fix
----------------  
UPDATE mart.report SET Target = '_blank' WHERE report_id <> 6

 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (220,'7.1.220') 
