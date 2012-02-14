/* 
BuildTime is: 
2009-04-29 
08:44
*/ 
/* 
Trunk initiated: 
2009-04-28 
14:58
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: David Jonsson
--Date: 2009-04-28
--Desc: Drop BU relation on KPI  
----------------
ALTER TABLE mart.dim_kpi
	DROP COLUMN business_unit_id
GO
ALTER TABLE stage.stg_kpi
	DROP COLUMN business_unit_code
GO
ALTER TABLE stage.stg_kpi
	DROP COLUMN business_unit_name
GO

 
GO 
 
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (98,'7.0.98') 
