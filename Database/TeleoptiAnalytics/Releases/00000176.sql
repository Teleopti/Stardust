/* 
Trunk initiated: 
2009-11-11 
16:09
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: JN
--Date: 2009-11-11  
--Desc: Add is_delete columns for skill and workload  
----------------  
ALTER TABLE stage.stg_workload ADD
	skill_is_deleted bit NOT NULL CONSTRAINT DF_stg_workload_skill_is_deleted DEFAULT 0,
	is_deleted bit NOT NULL CONSTRAINT DF_stg_workload_is_deleted DEFAULT 0
	
ALTER TABLE mart.dim_skill ADD
	is_deleted bit NOT NULL CONSTRAINT DF_dim_skill_is_deleted DEFAULT 0

ALTER TABLE mart.dim_workload ADD
	is_deleted bit NOT NULL CONSTRAINT DF_dim_workload_is_deleted DEFAULT 0 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (176,'7.0.176') 
