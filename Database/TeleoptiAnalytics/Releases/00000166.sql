/* 
Trunk initiated: 
2009-10-20 
09:28
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Jonas N  
--Date: 2009-10-22  
--Desc: New column in stage.stg_person  
----------------  
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
	person_first_name nvarchar(25) NOT NULL,
	person_last_name nvarchar(25) NOT NULL,
	team_code uniqueidentifier NOT NULL,
	team_name nvarchar(50) NOT NULL,
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
--ALTER TABLE stage.Tmp_stg_person SET (LOCK_ESCALATION = TABLE)
--GO
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
IF EXISTS(SELECT * FROM stage.stg_person)
	 EXEC('INSERT INTO stage.Tmp_stg_person (person_code, valid_from_date, valid_to_date, valid_from_interval_id, valid_to_interval_id, person_period_code, person_first_name, person_last_name, team_code, team_name, site_code, site_name, business_unit_code, business_unit_name, email, note, employment_number, employment_start_date, employment_end_date, time_zone_code, is_agent, is_user, contract_code, contract_name, parttime_code, parttime_percentage, employment_type, datasource_id, insert_date, update_date, datasource_update_date)
		SELECT person_code, valid_from_date, valid_to_date, valid_from_interval_id, valid_to_interval_id, person_period_code, person_first_name, person_last_name, team_code, team_name, site_code, site_name, business_unit_code, business_unit_name, email, note, employment_number, employment_start_date, employment_end_date, time_zone_code, is_agent, is_user, contract_code, contract_name, parttime_code, parttime_percentage, employment_type, datasource_id, insert_date, update_date, datasource_update_date FROM stage.stg_person WITH (HOLDLOCK TABLOCKX)')
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

EXEC mart.sys_crossdatabaseview_load  
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (166,'7.0.166') 
