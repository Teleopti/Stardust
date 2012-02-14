/* 
Trunk initiated: 
2009-12-16 
20:35
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: Anders  
--Date: 2010-01-07  
--Desc: Sockets live for 240 seconds by default... Let's not send heartbeats more often than that! 
----------------  

update msg.configuration
set ConfigurationValue = 250000
where ConfigurationId = 4

update msg.configuration
set ConfigurationValue = 600000
where ConfigurationId = 10

----------------
--Name: David J 
--Date: 2010-01-18
--Desc:  Add Day Off for reporting
----------------
--New stage table
CREATE TABLE [stage].[stg_day_off](
	[day_off_code] [uniqueidentifier] NULL,
	[day_off_name] [nvarchar](50) NOT NULL,
	[display_color] [int] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL
	)
GO
--PK
ALTER TABLE [stage].[stg_day_off] ADD  CONSTRAINT [PK_stg_day_off] PRIMARY KEY CLUSTERED 
(
	[day_off_name] ASC,
	[business_unit_code]
) ON [STAGE]
GO

--Set all current days off as "Not Defined" in fact-table
UPDATE mart.fact_schedule_preference
SET day_off_id = -1

--Set all current days off as "Not Defined" in fact-table
UPDATE mart.fact_schedule_day_count
SET day_off_id = -1

--Delete all current day off from dim-table
DELETE FROM mart.dim_day_off
WHERE day_off_id <> -1
GO

--Prepare for re-design of stage tables
DROP TABLE [stage].[stg_schedule_preference]

--Re-design
CREATE TABLE [stage].[stg_schedule_preference](
	[person_restriction_code] [uniqueidentifier] NOT NULL,
	[restriction_date] [datetime] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[interval_id] [int] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[shift_category_code] [uniqueidentifier] NULL,
	[day_off_code] [uniqueidentifier] NULL,
	[day_off_name] [nvarchar](50) NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[endTimeMinimum] [bigint] NULL,
	[endTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
	[preference_accepted] [int] NULL,
	[preference_declined] [int] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL
	) ON [STAGE]

--Re-create PK	
ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [PK_stg_schedule_preference] PRIMARY KEY CLUSTERED 
(
	[person_restriction_code] ASC
) ON [STAGE]
GO

--Re-create DF
ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO

ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_datasource_update_date]  DEFAULT (getdate()) FOR [datasource_update_date]
GO

--Prepare for re-design
DROP TABLE [stage].[stg_schedule_day_off_count]

--Re-design
CREATE TABLE [stage].[stg_schedule_day_off_count](
	[date] [smalldatetime] NOT NULL,
	[start_interval_id] [smallint] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[starttime] [smalldatetime] NULL,
	[day_off_code] [uniqueidentifier] NULL,
	[day_off_name]  [nvarchar](50) NOT NULL,
	[day_count] [int] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL
) ON [STAGE]
GO

ALTER TABLE [stage].[stg_schedule_day_off_count] ADD  CONSTRAINT [PK_stg_schedule_day_off_count] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[start_interval_id] ASC,
	[person_code] ASC,
	[scenario_code] ASC
) ON [STAGE]
GO

--Re-create DF
ALTER TABLE [stage].[stg_schedule_day_off_count] ADD  CONSTRAINT [DF_stg_schedule_day_off_count_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO

ALTER TABLE [stage].[stg_schedule_day_off_count] ADD  CONSTRAINT [DF_stg_schedule_day_off_count_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [stage].[stg_schedule_day_off_count] ADD  CONSTRAINT [DF_stg_schedule_day_off_count_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

--Run ETL to:
-- 1) re-load mart.dim_day_off
-- 2) re-load day_off_id in the fact-tables


----------------  
--Name: Jonas 
--Date: 2010-01-29  
--Desc: Add Business unit columns to mart.etl_job_execution and mart.etl_jobstep_execution
----------------  
/*** ADD BUSINESS UNIT COLUMNS business_unit_code AND business_unit_name ***/
ALTER TABLE mart.etl_job_execution
	DROP CONSTRAINT FK_etl_job_execution_etl_job_schedule
GO
--ALTER TABLE mart.etl_job_schedule SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE mart.etl_job_execution
	DROP CONSTRAINT FK_etl_job_execution_etl_job_definition
GO
--ALTER TABLE mart.etl_job SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE mart.etl_job_execution
	DROP CONSTRAINT DF_etl_job_execution_insert_date
GO
ALTER TABLE mart.etl_job_execution
	DROP CONSTRAINT DF_etl_job_execution_update_date
GO
CREATE TABLE mart.Tmp_etl_job_execution
	(
	job_execution_id int NOT NULL IDENTITY (1, 1),
	job_id int NULL,
	schedule_id int NULL,
	business_unit_code uniqueidentifier NULL,
	business_unit_name nvarchar(100) NULL,
	job_start_time datetime NULL,
	job_end_time datetime NULL,
	duration_s int NULL,
	affected_rows int NULL,
	job_execution_success bit NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL
	)  ON MART
GO
--ALTER TABLE mart.Tmp_etl_job_execution SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE mart.Tmp_etl_job_execution ADD CONSTRAINT
	DF_etl_job_execution_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE mart.Tmp_etl_job_execution ADD CONSTRAINT
	DF_etl_job_execution_update_date DEFAULT (getdate()) FOR update_date
GO
SET IDENTITY_INSERT mart.Tmp_etl_job_execution ON
GO
IF EXISTS(SELECT * FROM mart.etl_job_execution)
	 EXEC('INSERT INTO mart.Tmp_etl_job_execution (job_execution_id, job_id, schedule_id, job_start_time, job_end_time, duration_s, affected_rows, job_execution_success, insert_date, update_date)
		SELECT job_execution_id, job_id, schedule_id, job_start_time, job_end_time, duration_s, affected_rows, job_execution_success, insert_date, update_date FROM mart.etl_job_execution WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT mart.Tmp_etl_job_execution OFF
GO
ALTER TABLE mart.etl_jobstep_execution
	DROP CONSTRAINT FK_etl_jobstep_execution_etl_job_execution
GO
DROP TABLE mart.etl_job_execution
GO
EXECUTE sp_rename N'mart.Tmp_etl_job_execution', N'etl_job_execution', 'OBJECT' 
GO
ALTER TABLE mart.etl_job_execution ADD CONSTRAINT
	PK_etl_job_execution PRIMARY KEY CLUSTERED 
	(
	job_execution_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
ALTER TABLE mart.etl_job_execution ADD CONSTRAINT
	FK_etl_job_execution_etl_job_definition FOREIGN KEY
	(
	job_id
	) REFERENCES mart.etl_job
	(
	job_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.etl_job_execution ADD CONSTRAINT
	FK_etl_job_execution_etl_job_schedule FOREIGN KEY
	(
	schedule_id
	) REFERENCES mart.etl_job_schedule
	(
	schedule_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.etl_jobstep_execution ADD CONSTRAINT
	FK_etl_jobstep_execution_etl_job_execution FOREIGN KEY
	(
	job_execution_id
	) REFERENCES mart.etl_job_execution
	(
	job_execution_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
--ALTER TABLE mart.etl_jobstep_execution SET (LOCK_ESCALATION = TABLE)
GO




/*** ADD BUSINESS UNIT COLUMNS business_unit_code AND business_unit_name ***/
ALTER TABLE mart.etl_jobstep_execution
	DROP CONSTRAINT FK_etl_jobstep_execution_etl_jobstep_execution
GO
--ALTER TABLE mart.etl_jobstep SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE mart.etl_jobstep_execution
	DROP CONSTRAINT FK_etl_jobstep_execution_etl_jobstep_error
GO
--ALTER TABLE mart.etl_jobstep_error SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE mart.etl_jobstep_execution
	DROP CONSTRAINT FK_etl_jobstep_execution_etl_job_execution
GO
--ALTER TABLE mart.etl_job_execution SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE mart.etl_jobstep_execution
	DROP CONSTRAINT DF_etl_jobstep_execution_insert_date
GO
ALTER TABLE mart.etl_jobstep_execution
	DROP CONSTRAINT DF_etl_jobstep_execution_update_date
GO
CREATE TABLE mart.Tmp_etl_jobstep_execution
	(
	jobstep_execution_id int NOT NULL IDENTITY (1, 1),
	business_unit_code uniqueidentifier NULL,
	business_unit_name nvarchar(100) NULL,
	duration_s int NULL,
	rows_affected int NULL,
	job_execution_id int NULL,
	jobstep_error_id int NULL,
	jobstep_id int NULL,
	insert_date smalldatetime NULL,
	update_date smalldatetime NULL
	)  ON MART
GO
--ALTER TABLE mart.Tmp_etl_jobstep_execution SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE mart.Tmp_etl_jobstep_execution ADD CONSTRAINT
	DF_etl_jobstep_execution_insert_date DEFAULT (getdate()) FOR insert_date
GO
ALTER TABLE mart.Tmp_etl_jobstep_execution ADD CONSTRAINT
	DF_etl_jobstep_execution_update_date DEFAULT (getdate()) FOR update_date
GO
SET IDENTITY_INSERT mart.Tmp_etl_jobstep_execution ON
GO
IF EXISTS(SELECT * FROM mart.etl_jobstep_execution)
	 EXEC('INSERT INTO mart.Tmp_etl_jobstep_execution (jobstep_execution_id, duration_s, rows_affected, job_execution_id, jobstep_error_id, jobstep_id, insert_date, update_date)
		SELECT jobstep_execution_id, duration_s, rows_affected, job_execution_id, jobstep_error_id, jobstep_id, insert_date, update_date FROM mart.etl_jobstep_execution WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT mart.Tmp_etl_jobstep_execution OFF
GO
DROP TABLE mart.etl_jobstep_execution
GO
EXECUTE sp_rename N'mart.Tmp_etl_jobstep_execution', N'etl_jobstep_execution', 'OBJECT' 
GO
ALTER TABLE mart.etl_jobstep_execution ADD CONSTRAINT
	PK_etl_jobstep_execution PRIMARY KEY CLUSTERED 
	(
	jobstep_execution_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON MART

GO
ALTER TABLE mart.etl_jobstep_execution ADD CONSTRAINT
	FK_etl_jobstep_execution_etl_job_execution FOREIGN KEY
	(
	job_execution_id
	) REFERENCES mart.etl_job_execution
	(
	job_execution_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.etl_jobstep_execution ADD CONSTRAINT
	FK_etl_jobstep_execution_etl_jobstep_error FOREIGN KEY
	(
	jobstep_error_id
	) REFERENCES mart.etl_jobstep_error
	(
	jobstep_error_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE mart.etl_jobstep_execution ADD CONSTRAINT
	FK_etl_jobstep_execution_etl_jobstep_execution FOREIGN KEY
	(
	jobstep_id
	) REFERENCES mart.etl_jobstep
	(
	jobstep_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 	
GO 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (196,'7.1.196') 
