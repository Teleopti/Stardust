/* 
Trunk initiated: 
2010-06-16 
08:26
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-06-16
--Desc: Adding config table for ETL maint  
---------------- 
--add table
CREATE TABLE mart.etl_maintenance_configuration
	(
	configuration_id smallint NOT NULL,
	configuration_name nvarchar(50) NOT NULL,
	configuration_value nvarchar(255) NOT NULL
	)  ON [MART]
	
--add PK
ALTER TABLE mart.etl_maintenance_configuration ADD CONSTRAINT
	PK_etl_maintenance_configuration PRIMARY KEY CLUSTERED 
	(
	configuration_id
	) ON [MART]
	
--add default data
SET NOCOUNT ON
insert into mart.etl_maintenance_configuration (configuration_id,configuration_name,configuration_value)
values (1,'daysToKeepETLError',60)
insert into mart.etl_maintenance_configuration (configuration_id,configuration_name,configuration_value)
values (2,'daysToKeepETLExecution',45)
insert into mart.etl_maintenance_configuration (configuration_id,configuration_name,configuration_value)
values (3,'daysToKeepRTAEvents',2)
SET NOCOUNT OFF

----------------  
--Name: David Jonsson
--Date: 2010-06-16
--Desc: Bug #7989
--		An extra fnutt in Raptor made all KPIs go away in Mart
----------------
update mart.dim_kpi
set resource_key = 'KpiAverageHandleTime'
where resource_key = 'KpiAverageHandleTime''' 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (285,'7.1.285') 
