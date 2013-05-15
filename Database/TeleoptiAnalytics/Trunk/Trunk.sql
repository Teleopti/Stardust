

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[permission_report]'))
DROP VIEW [mart].[permission_report]
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[mart].[permission_report]'))
CREATE TABLE [mart].[permission_report](
	[person_code] [uniqueidentifier] NOT NULL,
	[team_id] [int] NOT NULL,
	[my_own] [bit] NOT NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[ReportId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_permission_report] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC,
	[team_id] ASC,
	[my_own] ASC,
	[business_unit_id] ASC,
	[ReportId] ASC
)
) 

GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = N'DF_permission_report_business_unit_id')
ALTER TABLE [mart].[permission_report] ADD  CONSTRAINT [DF_permission_report_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_permission_report]'))
DROP VIEW [mart].[v_permission_report]
GO

CREATE VIEW [mart].[v_permission_report]
AS
SELECT person_code, team_id, my_own, business_unit_id, datasource_id, datasource_update_date, ReportId
  FROM [mart].[permission_report] 
GO


IF  EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[mart].[permission_report_A]'))
DROP TABLE [mart].[permission_report_A]
GO

IF  EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[mart].[permission_report_B]'))
DROP TABLE [mart].[permission_report_B]
GO

IF  EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[mart].[permission_report_active]'))
DROP TABLE [mart].[permission_report_active]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_permission_report_switch_active]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_permission_report_switch_active]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_permission_report_truncate_nonactive]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_permission_report_truncate_nonactive]
GO

----------------  
--Name: David
--Date: 2013-05-10
--Desc: #23416 - dubplicate key error
-----------------
IF EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[Stage].[stg_schedule_updated_personLocal]'))
DROP TABLE [Stage].[stg_schedule_updated_personLocal]
GO
CREATE TABLE Stage.stg_schedule_updated_personLocal (
	person_id int not null,
	time_zone_id int not null,
	person_code uniqueidentifier not null,
	valid_from_date_local smalldatetime not null,
	valid_to_date_local smalldatetime not null
CONSTRAINT [PK_stg_schedule_updated_personLocal] PRIMARY KEY CLUSTERED 
(
	[person_id],
	[valid_from_date_local],
	[valid_to_date_local]
)
)

IF EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[Stage].[stg_schedule_updated_ShiftStartDateUTC]'))
DROP TABLE [Stage].[stg_schedule_updated_ShiftStartDateUTC]
GO
CREATE TABLE Stage.stg_schedule_updated_ShiftStartDateUTC (
	person_id int not null,
	shift_startdate_id int not null
CONSTRAINT [PK_stg_schedule_updated_ShiftStartDateUTC] PRIMARY KEY CLUSTERED 
(
	[person_id],
	[shift_startdate_id]
)
)
GO
----------------  
--Name: Ola
--Date: 2013-05-14
--Desc: new "Avaliability per Agent" report
-----------------
DECLARE @newreportid uniqueidentifier = 'A56B3EEF-17A2-4778-AA8A-D166232073D2'
INSERT INTO mart.report (
		Id, 
		report_id, 
		control_collection_id, 
		url, 
		target, 
		report_name, 
		report_name_resource_key, 
		visible, 
		rpt_file_name, 
		proc_name, 
		help_key, 
		sub1_name, 
		sub1_proc_name, 
		sub2_name, 
		sub2_proc_name, 
		ControlCollectionId)
		VALUES(
		@newreportid,
		0,
		28,
		'~/Selection.aspx?ReportId=A56B3EEF-17A2-4778-AA8A-D166232073D2',
		'_blank',
		'Availability per Agent',
		'ResReportAvailabilityPerAgent',
		1,
		'~/Reports/CCC/report_availability_per_agent.rdlc',
		'mart.report_data_availability_per_agent',
		'f01:Report+AvailabilityPerAgent',
		'','','','',
		'077F50D3-CE9C-4EF6-B52F-13975C4F83EA')
GO

----------------  
--Name: Karin
--Date: 2013-05-15
--Desc: New Stage Table Hourly Availablity
-----------------
CREATE TABLE [stage].[stg_hourly_availability](
	[restriction_date] [smalldatetime] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[available_time_m] [int] NOT NULL,
	[scheduled_time_m] [int] NOT NULL,
	[scheduled] [smallint] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NOT NULL

 CONSTRAINT [PK_stg_hourly_availability] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC,
	[restriction_date] ASC,
	[scenario_code] ASC
)
)

GO

----------------  
--Name: Karin
--Date: 2013-05-15
--Desc: New Fact Table Hourly Availablity
-----------------

CREATE TABLE [mart].[fact_hourly_availability](
	[date_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[available_time_m] [int] NULL,
	[available_days] [int] NULL,
	[scheduled_time_m] [int] NULL,
	[scheduled_days] [int] NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL
 CONSTRAINT [PK_fact_hourly_availability] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[person_id] ASC,
	[scenario_id] ASC
)
)

GO

ALTER TABLE [mart].[fact_hourly_availability] ADD  CONSTRAINT [DF_fact_hourly_availability_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO


ALTER TABLE [mart].[fact_hourly_availability]  WITH CHECK ADD  CONSTRAINT [FK_fact_hourly_availability_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO

ALTER TABLE [mart].[fact_hourly_availability] CHECK CONSTRAINT [FK_fact_hourly_availability_dim_date]
GO


ALTER TABLE [mart].[fact_hourly_availability]  WITH CHECK ADD  CONSTRAINT [FK_fact_hourly_availability_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO

ALTER TABLE [mart].[fact_hourly_availability] CHECK CONSTRAINT [FK_fact_hourly_availability_dim_person]
GO


ALTER TABLE [mart].[fact_hourly_availability]  WITH CHECK ADD  CONSTRAINT [FK_fact_hourly_availability_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO

ALTER TABLE [mart].[fact_hourly_availability] CHECK CONSTRAINT [FK_fact_hourly_availability_dim_scenario]
GO
