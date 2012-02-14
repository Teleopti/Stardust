/* 
Trunk initiated: 
2009-09-18 
10:38
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: David Jonsson
--Date: 2009-09-21
--Desc: Adding new column to handle delete functions on dim_person(period)  
----------------  
ALTER TABLE mart.dim_person ADD
	to_be_deleted bit NULL
GO

UPDATE mart.dim_person
SET to_be_deleted = 0
GO

ALTER TABLE mart.dim_person ALTER COLUMN
	to_be_deleted bit NOT NULL
GO

----------------  
--Name: Jonas Nordh
--Date: 2009-09-23
--Desc: Adding new columns to mart.dim_person and stage.stg_person
----------------  

/* Add columns to mart.dim_person */
/* (valid_from_date_id int, valid_from_interval_id int, valid_to_date_id int, valid_to_interval_id) */
ALTER TABLE mart.dim_person
	DROP CONSTRAINT FK_dim_person_dim_skillset
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_valid_to_date
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_person_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_first_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_last_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_employment_type_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_tema_id
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_team_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_site_id
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_site_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_business_unit_id
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_business_unit_name
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_datasource_id
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_insert_date
GO
ALTER TABLE mart.dim_person
	DROP CONSTRAINT DF_dim_person_update_date
GO
CREATE TABLE mart.Tmp_dim_person
	(
	person_id int NOT NULL IDENTITY (1, 1),
	person_code uniqueidentifier NULL,
	valid_from_date smalldatetime NOT NULL,
	valid_to_date smalldatetime NOT NULL,
	valid_from_date_id int NOT NULL,
	valid_from_interval_id int NOT NULL,
	valid_to_date_id int NOT NULL,
	valid_to_interval_id int NOT NULL,
	person_period_code uniqueidentifier NULL,
	person_name nvarchar(200) NOT NULL,
	first_name nvarchar(30) NOT NULL,
	last_name nvarchar(30) NOT NULL,
	employment_number nvarchar(50) NULL,
	employment_type_code int NULL,
	employment_type_name nvarchar(50) NOT NULL,
	contract_code uniqueidentifier NULL,
	contract_name nvarchar(50) NULL,
	parttime_code uniqueidentifier NULL,
	parttime_percentage nvarchar(50) NULL,
	team_id int NULL,
	team_code uniqueidentifier NULL,
	team_name nvarchar(50) NOT NULL,
	site_id int NULL,
	site_code uniqueidentifier NULL,
	site_name nvarchar(50) NOT NULL,
	business_unit_id int NULL,
	business_unit_code uniqueidentifier NULL,
	business_unit_name nvarchar(50) NOT NULL,
	skillset_id int NULL,
	email nvarchar(200) NULL,
	note nvarchar(1024) NULL,
	employment_start_date smalldatetime NULL,
	employment_end_date smalldatetime NULL,
	time_zone_id int NULL,
	is_agent bit NULL,
	is_user bit NULL,
	datasource_id smallint NOT NULL,
	insert_date smalldatetime NOT NULL,
	update_date smalldatetime NOT NULL,
	datasource_update_date smalldatetime NULL,
	to_be_deleted bit NOT NULL
	)  ON MART
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_valid_to_date DEFAULT (((2059)-(12))-(31)) FOR valid_to_date
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_valid_from_date_id DEFAULT -1 FOR valid_from_date_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_valid_from_interval_id DEFAULT 0 FOR valid_from_interval_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_valid_from_date_id1 DEFAULT -1 FOR valid_to_date_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_valid_from_interval_id1 DEFAULT 0 FOR valid_to_interval_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_person_name DEFAULT (N'Not Defined') FOR person_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_first_name DEFAULT (N'Not Defined') FOR first_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_last_name DEFAULT (N'Not Defined''') FOR last_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_employment_type_name DEFAULT (N'Not Defined') FOR employment_type_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_tema_id DEFAULT ((-1)) FOR team_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_team_name DEFAULT (N'Not Defined') FOR team_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_site_id DEFAULT ((-1)) FOR site_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_site_name DEFAULT (N'Not Defined') FOR site_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_business_unit_id DEFAULT ((-1)) FOR business_unit_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_business_unit_name DEFAULT (N'Not Defined') FOR business_unit_name
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_datasource_id DEFAULT ((-1)) FOR datasource_id
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE mart.Tmp_dim_person ADD CONSTRAINT
	DF_dim_person_update_date DEFAULT (getdate()) FOR update_date
GO
SET IDENTITY_INSERT mart.Tmp_dim_person ON
GO
IF EXISTS(SELECT * FROM mart.dim_person)
	 EXEC('INSERT INTO mart.Tmp_dim_person (person_id, person_code, valid_from_date, valid_to_date, person_period_code, person_name, first_name, last_name, employment_number, employment_type_code, employment_type_name, contract_code, contract_name, parttime_code, parttime_percentage, team_id, team_code, team_name, site_id, site_code, site_name, business_unit_id, business_unit_code, business_unit_name, skillset_id, email, note, employment_start_date, employment_end_date, time_zone_id, is_agent, is_user, datasource_id, insert_date, update_date, datasource_update_date, to_be_deleted)
		SELECT person_id, person_code, valid_from_date, valid_to_date, person_period_code, person_name, first_name, last_name, employment_number, employment_type_code, employment_type_name, contract_code, contract_name, parttime_code, parttime_percentage, team_id, team_code, team_name, site_id, site_code, site_name, business_unit_id, business_unit_code, business_unit_name, skillset_id, email, note, employment_start_date, employment_end_date, time_zone_id, is_agent, is_user, datasource_id, insert_date, update_date, datasource_update_date, to_be_deleted FROM mart.dim_person WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT mart.Tmp_dim_person OFF
GO
ALTER TABLE mart.fact_schedule_day_count
	DROP CONSTRAINT FK_fact_schedule_day_count_dim_person
GO
ALTER TABLE mart.fact_schedule
	DROP CONSTRAINT FK_fact_schedule_dim_person
GO
ALTER TABLE mart.fact_contract
	DROP CONSTRAINT FK_fact_contract_time_dim_person
GO
ALTER TABLE mart.bridge_acd_login_person
	DROP CONSTRAINT FK_bridge_acd_login_person_dim_person
GO
ALTER TABLE mart.fact_schedule_preference
	DROP CONSTRAINT FK_fact_schedule_preference_dim_person
GO
ALTER TABLE mart.fact_schedule_deviation
	DROP CONSTRAINT FK_fact_schedule_deviation_dim_person
GO
DROP TABLE mart.dim_person
GO
EXECUTE sp_rename N'mart.Tmp_dim_person', N'dim_person', 'OBJECT' 
GO
ALTER TABLE mart.dim_person ADD CONSTRAINT
	PK_dim_person PRIMARY KEY CLUSTERED 
	(
	person_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
ALTER TABLE mart.dim_person ADD CONSTRAINT
	FK_dim_person_dim_skillset FOREIGN KEY
	(
	skillset_id
	) REFERENCES mart.dim_skillset
	(
	skillset_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.fact_schedule_deviation ADD CONSTRAINT
	FK_fact_schedule_deviation_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.fact_schedule_preference ADD CONSTRAINT
	FK_fact_schedule_preference_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.bridge_acd_login_person ADD CONSTRAINT
	FK_bridge_acd_login_person_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.fact_contract ADD CONSTRAINT
	FK_fact_contract_time_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.fact_schedule ADD CONSTRAINT
	FK_fact_schedule_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.fact_schedule_day_count ADD CONSTRAINT
	FK_fact_schedule_day_count_dim_person FOREIGN KEY
	(
	person_id
	) REFERENCES mart.dim_person
	(
	person_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_valid_from_interval_id DEFAULT 0 FOR valid_from_interval_id
GO
ALTER TABLE stage.Tmp_stg_person ADD CONSTRAINT
	DF_stg_person_valid_to_interval_id DEFAULT 0 FOR valid_to_interval_id
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
	 EXEC('INSERT INTO stage.Tmp_stg_person (person_code, valid_from_date, valid_to_date, person_period_code, person_first_name, person_last_name, team_code, team_name, site_code, site_name, business_unit_code, business_unit_name, email, note, employment_number, employment_start_date, employment_end_date, time_zone_code, is_agent, is_user, contract_code, contract_name, parttime_code, parttime_percentage, employment_type, datasource_id, insert_date, update_date, datasource_update_date)
		SELECT person_code, valid_from_date, valid_to_date, person_period_code, person_first_name, person_last_name, team_code, team_name, site_code, site_name, business_unit_code, business_unit_name, email, note, employment_number, employment_start_date, employment_end_date, time_zone_code, is_agent, is_user, contract_code, contract_name, parttime_code, parttime_percentage, employment_type, datasource_id, insert_date, update_date, datasource_update_date FROM stage.stg_person WITH (HOLDLOCK TABLOCKX)')
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
----------------  
--Name: David Jonsson
--Date: 2009-09-28
--Desc: Adding Clustered Index to Message Broker heap table
----------------  
-------------
--[Pending]
--Put a clustered index on SubscriberId
----------
--Temporay added at some customers!! IF EXISTS => Drop
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[msg].[Pending]') AND name = N'IX_SubscriberId')
DROP INDEX [IX_SubscriberId] ON [msg].[Pending]
GO

--Temporay added at some customers!! IF EXISTS => Drop
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[msg].[Pending]') AND name = N'IX_Pending')
DROP INDEX [IX_Pending] ON [msg].[Pending]
GO

--Add a clustered index to avoid heap table
CREATE CLUSTERED INDEX [IX_SubscriberId] ON [msg].[Pending] 
(
	[SubscriberId] ASC
)
GO

-------------
--[Heartbeat]
--Re-order indexes
--Put a clustered index on ChangeDateTime
----------
DROP INDEX [IX_Heartbeat_ChangedBy] ON [msg].[Heartbeat]
GO
DROP INDEX [IX_Heartbeat_ChangedDateTime] ON [msg].[Heartbeat]
GO
ALTER TABLE msg.Heartbeat
	DROP CONSTRAINT PK_Heartbeat
GO
CREATE CLUSTERED INDEX [IX_ChangeDateTime] ON [msg].[Heartbeat] 
(
	[ChangedDateTime] ASC
)
GO

ALTER TABLE msg.Heartbeat ADD CONSTRAINT
	PK_Heartbeat PRIMARY KEY NONCLUSTERED 
	(
	HeartbeatId
	)
GO

CREATE NONCLUSTERED INDEX [IX_Heartbeat_ChangedBy] ON [msg].[Heartbeat] 
(
	[ChangedBy] ASC
)
GO
 
GO 
 
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (154,'7.0.154') 
