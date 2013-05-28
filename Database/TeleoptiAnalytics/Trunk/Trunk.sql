

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
	shift_startdate_id int not null,
	interval_id int not null
CONSTRAINT [PK_stg_schedule_updated_ShiftStartDateUTC] PRIMARY KEY CLUSTERED 
(
	[person_id],
	[shift_startdate_id],
	[interval_id]
)
)
GO
GO

----------------  
--Name: David
--Date: 2013-05-27
--Desc: #23520 - New indexes to support delete by Scenario 
-----------------
--[mart].[fact_schedule]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_scenario_id')
CREATE NONCLUSTERED INDEX IX_fact_schedule_scenario_id
ON [mart].[fact_schedule] ([scenario_id])
INCLUDE ([schedule_date_id],[person_id],[interval_id],[activity_starttime],[shift_startdate_id])
GO

--[stage].[stg_schedule_updated_ShiftStartDateUTC]
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_schedule_updated_ShiftStartDateUTC]') AND type in (N'U'))
DROP TABLE [stage].[stg_schedule_updated_ShiftStartDateUTC]
GO
CREATE TABLE [stage].[stg_schedule_updated_ShiftStartDateUTC](
	[person_id] [int] NOT NULL,
	[shift_startdate_id] [int] NOT NULL,
	[interval_id] [int] NOT NULL,
 CONSTRAINT [PK_stg_schedule_updated_ShiftStartDateUTC] PRIMARY KEY CLUSTERED 
(
	[shift_startdate_id] ASC,
	[interval_id] ASC,
	[person_id] ASC
)
)
GO