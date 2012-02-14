/* 
Trunk initiated: 
2011-01-04 
14:03
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Jonas n
--Date: 2011-01-05  
--Desc: Remove scenario as a parameter from three reports  
----------------  
DELETE FROM mart.report_control_collection WHERE collection_id = 8 AND control_id = 14
DELETE FROM mart.report_control_collection WHERE collection_id = 10 AND control_id = 14
DELETE FROM mart.report_control_collection WHERE collection_id = 18 AND control_id = 14

UPDATE mart.report_control_collection
SET print_order = print_order - 1
WHERE collection_id IN (8,10,18)



----------------  
--Name: Jonas n
--Date: 2011-01-11  
--Desc: Add person_name column to stage.stg_person 
----------------  
TRUNCATE TABLE stage.stg_person
GO
ALTER TABLE stage.stg_person
	DROP CONSTRAINT DF_stg_person_valid_from_interval_id
GO
ALTER TABLE stage.stg_person
	DROP CONSTRAINT DF_stg_person_valid_to_interval_id
GO
ALTER TABLE stage.stg_person
	DROP CONSTRAINT DF_stg_person_datasource_id
GO
ALTER TABLE stage.stg_person
	DROP CONSTRAINT DF_stg_person_insert_date
GO
ALTER TABLE stage.stg_person
	DROP CONSTRAINT DF_stg_person_update_date
GO
ALTER TABLE stage.stg_person
	DROP CONSTRAINT DF_stg_person_datasource_update_date
GO
CREATE TABLE stage.Tmp_stg_person
	(
	person_code uniqueidentifier NOT NULL,
	valid_from_date smalldatetime NOT NULL,
	valid_to_date smalldatetime NOT NULL,
	valid_from_interval_id int NOT NULL,
	valid_to_interval_id int NOT NULL,
	valid_to_interval_start smalldatetime NULL,
	person_period_code uniqueidentifier NULL,
	person_name nvarchar(200) NOT NULL,
	person_first_name nvarchar(25) NOT NULL,
	person_last_name nvarchar(25) NOT NULL,
	team_code uniqueidentifier NOT NULL,
	team_name nvarchar(50) NOT NULL,
	scorecard_code uniqueidentifier NULL,
	site_code uniqueidentifier NOT NULL,
	site_name nvarchar(50) NOT NULL,
	business_unit_code uniqueidentifier NOT NULL,
	business_unit_name nvarchar(50) NOT NULL,
	email nvarchar(200) NULL,
	note nchar(1024) NULL,
	employment_number nvarchar(50) NULL,
	employment_start_date smalldatetime NOT NULL,
	employment_end_date smalldatetime NOT NULL,
	time_zone_code nvarchar(50) NULL,
	is_agent bit NULL,
	is_user bit NULL,
	contract_code uniqueidentifier NULL,
	contract_name nvarchar(50) NULL,
	parttime_code uniqueidentifier NULL,
	parttime_percentage nvarchar(50) NULL,
	employment_type nvarchar(50) NULL,
	datasource_id smallint NOT NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL,
	datasource_update_date smalldatetime NOT NULL
	)  ON STAGE
GO

ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_valid_from_interval_id DEFAULT ((0)) FOR valid_from_interval_id
GO
ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_valid_to_interval_id DEFAULT ((0)) FOR valid_to_interval_id
GO
ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_datasource_id DEFAULT ((1)) FOR datasource_id
GO
ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_update_date DEFAULT (getdate()) FOR update_date
GO
ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_datasource_update_date DEFAULT (getdate()) FOR datasource_update_date
GO
DROP TABLE stage.stg_person
GO
EXECUTE sp_rename N'stage.Tmp_stg_person', N'stg_person', 'OBJECT' 
GO
ALTER TABLE stage.stg_person ADD CONSTRAINT
	PK_stg_person PRIMARY KEY CLUSTERED 
	(
	person_code,
	valid_from_date
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON STAGE
 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (313,'7.1.313') 
