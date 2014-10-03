----------------  
--Name: Karin Jeppsson
--Date: 2014-09-17
--Desc: Bug #30657 Speed up load of dim_person to prevent deadlocks, add two new columns valid_from_date_local , valid_to_date_local
----------------

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_person]') AND type in (N'U'))
BEGIN
	DROP TABLE [stage].[stg_person]
END
GO

CREATE TABLE [stage].[stg_person](
	[person_code] [uniqueidentifier] NOT NULL,
	[valid_from_date] [smalldatetime] NOT NULL,
	[valid_to_date] [smalldatetime] NOT NULL,
	[valid_from_interval_id] [int] NOT NULL CONSTRAINT [DF_stg_person_valid_from_interval_id]  DEFAULT ((0)),
	[valid_to_interval_id] [int] NOT NULL CONSTRAINT [DF_stg_person_valid_to_interval_id]  DEFAULT ((0)),
	[valid_to_interval_start] [smalldatetime] NULL,
	[person_period_code] [uniqueidentifier] NULL,
	[person_name] [nvarchar](200) NOT NULL,
	[person_first_name] [nvarchar](25) NOT NULL,
	[person_last_name] [nvarchar](25) NOT NULL,
	[team_code] [uniqueidentifier] NOT NULL,
	[team_name] [nvarchar](50) NOT NULL,
	[scorecard_code] [uniqueidentifier] NULL,
	[site_code] [uniqueidentifier] NOT NULL,
	[site_name] [nvarchar](50) NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[email] [nvarchar](200) NULL,
	[note] [nchar](1024) NULL,
	[employment_number] [nvarchar](50) NULL,
	[employment_start_date] [smalldatetime] NOT NULL,
	[employment_end_date] [smalldatetime] NOT NULL,
	[time_zone_code] [nvarchar](50) NULL,
	[is_agent] [bit] NULL,
	[is_user] [bit] NULL,
	[contract_code] [uniqueidentifier] NULL,
	[contract_name] [nvarchar](50) NULL,
	[parttime_code] [uniqueidentifier] NULL,
	[parttime_percentage] [nvarchar](50) NULL,
	[employment_type] [nvarchar](50) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_stg_person_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_stg_person_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_person_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_person_datasource_update_date]  DEFAULT (getdate()),
	[windows_domain] [nvarchar](50) NULL,
	[windows_username] [nvarchar](50) NULL,
	[valid_from_date_local] [smalldatetime] NULL,
	[valid_to_date_local] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_person] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC,
	[valid_from_date] ASC
)
)

GO

----------------  
--Name: David Jonsson
--Date: 2014-10-02
--Desc: Bug #30779 Speed up re-load of fact_schedule
----------------
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule_convert]') AND type in (N'U'))
DROP TABLE [mart].[fact_schedule_convert]
GO

CREATE TABLE [mart].[fact_schedule_convert](
	[shift_startdate_local_id] [int] NOT NULL,
	[schedule_date_id] [int] NOT NULL CONSTRAINT [DF_fact_schedule_convert_schedule_date_id]  DEFAULT ((-1)),
	[person_id] [int] NOT NULL CONSTRAINT [DF_fact_schedule_convert_person_id]  DEFAULT ((-1)),
	[interval_id] [smallint] NOT NULL CONSTRAINT [DF_fact_schedule_convert_interval_id]  DEFAULT ((-1)),
	[activity_starttime] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_schedule_convert_activity_starttime]  DEFAULT (((1900)-(1))-(1)),
	[scenario_id] [smallint] NOT NULL CONSTRAINT [DF_fact_schedule_convert_scenario_id]  DEFAULT ((-1)),
	[activity_id] [int] NULL CONSTRAINT [DF_fact_schedule_convert_activity_id]  DEFAULT ((-1)),
	[absence_id] [int] NULL CONSTRAINT [DF_fact_schedule_convert_absence_id]  DEFAULT ((-1)),
	[activity_startdate_id] [int] NULL CONSTRAINT [DF_fact_schedule_convert_activity_startdate_id]  DEFAULT ((-1)),
	[activity_enddate_id] [int] NULL CONSTRAINT [DF_fact_schedule_convert_activity_enddate_id]  DEFAULT ((-1)),
	[activity_endtime] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_convert_activity_endtime]  DEFAULT (((2059)-(12))-(31)),
	[shift_startdate_id] [int] NULL CONSTRAINT [DF_fact_schedule_convert_startdate_id]  DEFAULT ((-1)),
	[shift_starttime] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_convert_starttime]  DEFAULT (((1900)-(1))-(1)),
	[shift_enddate_id] [int] NULL CONSTRAINT [DF_fact_schedule_convert_enddate_id]  DEFAULT ((-1)),
	[shift_endtime] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_convert_endtime]  DEFAULT (((2059)-(12))-(31)),
	[shift_startinterval_id] [smallint] NULL CONSTRAINT [DF_fact_schedule_convert_startinterval_id]  DEFAULT ((-1)),
	[shift_endinterval_id] [smallint] NULL CONSTRAINT [DF_fact_schedule_convert_endinterval_id]  DEFAULT ((-1)),
	[shift_category_id] [int] NULL CONSTRAINT [DF_fact_schedule_convert_category_id]  DEFAULT ((-1)),
	[shift_length_id] [int] NULL CONSTRAINT [DF_fact_schedule_convert_length_id]  DEFAULT ((-1)),
	[scheduled_time_m] [int] NULL,
	[scheduled_time_absence_m] [int] NULL,
	[scheduled_time_activity_m] [int] NULL,
	[scheduled_contract_time_m] [int] NULL,
	[scheduled_contract_time_activity_m] [int] NULL,
	[scheduled_contract_time_absence_m] [int] NULL,
	[scheduled_work_time_m] [int] NULL,
	[scheduled_work_time_activity_m] [int] NULL,
	[scheduled_work_time_absence_m] [int] NULL,
	[scheduled_over_time_m] [int] NULL,
	[scheduled_ready_time_m] [int] NULL,
	[scheduled_paid_time_m] [int] NULL,
	[scheduled_paid_time_activity_m] [int] NULL,
	[scheduled_paid_time_absence_m] [int] NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_fact_schedule_convert_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_convert_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_convert_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
	[overtime_id] [int] NOT NULL CONSTRAINT [DF_fact_schedule_convert_overtime_id]  DEFAULT ((-1)),
 CONSTRAINT [PK_fact_schedule_convert] PRIMARY KEY CLUSTERED 
(
	[shift_startdate_local_id] ASC,
	[schedule_date_id] ASC
)
)
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (406,'8.1.406') 
