/* 
Trunk initiated: 
2010-03-31 
08:16
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: david Jonsson
--Date: 2010-03-31
--Desc: Resource key error
----------------  
update mart.dim_kpi
set resource_key = 'KpiAverageHandleTime'
where kpi_id = 3
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (224,'7.1.224') 
