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

----------------  
--Name: jonas n
--Date: 2010-04-01
--Desc: Change PK on stage.stg_schedule
----------------  

ALTER TABLE stage.stg_schedule
	DROP CONSTRAINT PK_stg_schedule
GO
ALTER TABLE stage.stg_schedule ADD CONSTRAINT
	PK_stg_schedule PRIMARY KEY CLUSTERED 
	(
	schedule_date,
	person_code,
	interval_id,
	activity_start,
	scenario_code,
	shift_start
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON STAGE

GO 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (226,'7.1.226') 
