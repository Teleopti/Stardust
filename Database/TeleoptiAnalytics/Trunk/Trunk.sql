

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

----------------  
--Name: Karin
--Date: 2013-05-24
--Desc: New Report Control collection
-----------------
--ADD REPORT CONTROL COLLECTION
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where CollectionId='5F47A114-B3CE-4D49-AAD0-6E118E7EDE2F')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '25E41D20-9D92-485F-8C53-764C540820A0',	mart.report_control.Id,	'5F47A114-B3CE-4D49-AAD0-6E118E7EDE2F',	473,44,1,14,'0','ResScenarioColon',NULL,'@scenario_id',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=14

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '7C06ACC6-6BF3-4228-B6CB-5B8197D9CE05',mart.report_control.Id,	'5F47A114-B3CE-4D49-AAD0-6E118E7EDE2F',	474,44,2,1,'12:00','ResDateFromColon',NULL,'@date_from',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=1

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'F1F7C1C8-9368-4E07-8315-FB59D01D2C4F',mart.report_control.Id,	'5F47A114-B3CE-4D49-AAD0-6E118E7EDE2F',	475,44,3,2,	'12:00','ResDateToColon',NULL,'@date_to',474,NULL,NULL,	NULL,'7C06ACC6-6BF3-4228-B6CB-5B8197D9CE05',NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=2

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'B4EE1A94-43E9-4BE1-8BF7-AE602B24024B',mart.report_control.Id,	'5F47A114-B3CE-4D49-AAD0-6E118E7EDE2F',	476,44,4,29,'-2','ResGroupPageColon',NULL,'@group_page_code',NULL,NULL,NULL,	NULL,NULL,NULL,	NULL,NULL
	FROM mart.report_control WHERE control_id=29

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '1C703E88-E7E7-49E3-98D2-A1C3B3EBDB45',mart.report_control.Id,	'5F47A114-B3CE-4D49-AAD0-6E118E7EDE2F',	477,44,5,35,'-99','ResGroupPageGroupColon',NULL,'@group_page_group_set',476,NULL,NULL,NULL,'B4EE1A94-43E9-4BE1-8BF7-AE602B24024B',NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=35

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'F98BEF4B-7213-4B29-A3DE-024713B18291',mart.report_control.Id,	'5F47A114-B3CE-4D49-AAD0-6E118E7EDE2F',	478,44,6,37,'-2','ResAgentColon',NULL,'@group_page_agent_code',474,475,476,477,'7C06ACC6-6BF3-4228-B6CB-5B8197D9CE05','F1F7C1C8-9368-4E07-8315-FB59D01D2C4F','B4EE1A94-43E9-4BE1-8BF7-AE602B24024B','1C703E88-E7E7-49E3-98D2-A1C3B3EBDB45'
	FROM mart.report_control WHERE control_id=37

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '8B7F4D10-1F64-4D81-8DC1-23B5A3501F25',mart.report_control.Id,	'5F47A114-B3CE-4D49-AAD0-6E118E7EDE2F',	479,44,7,3,	'-2','ResSiteNameColon',NULL,'@site_id',474,475,476,NULL,'7C06ACC6-6BF3-4228-B6CB-5B8197D9CE05'	,'F1F7C1C8-9368-4E07-8315-FB59D01D2C4F','B4EE1A94-43E9-4BE1-8BF7-AE602B24024B',	NULL
	FROM mart.report_control WHERE control_id=3

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'B4ADA1E7-F81E-4F81-8DC2-51577245E76F',mart.report_control.Id,	'5F47A114-B3CE-4D49-AAD0-6E118E7EDE2F',	480,44,8,34,'-99','ResTeamNameColon',NULL,'@team_set',474,475,476,479,'7C06ACC6-6BF3-4228-B6CB-5B8197D9CE05','F1F7C1C8-9368-4E07-8315-FB59D01D2C4F','B4EE1A94-43E9-4BE1-8BF7-AE602B24024B','8B7F4D10-1F64-4D81-8DC1-23B5A3501F25'
	FROM mart.report_control WHERE control_id=34

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '14337BE0-A836-47BC-8BC1-660FCD1EFBD1',mart.report_control.Id,	'5F47A114-B3CE-4D49-AAD0-6E118E7EDE2F',	481,44,9,36,'-2','ResAgentsColon',NULL,	'@agent_code',474,475,479,480,'7C06ACC6-6BF3-4228-B6CB-5B8197D9CE05','F1F7C1C8-9368-4E07-8315-FB59D01D2C4F','8B7F4D10-1F64-4D81-8DC1-23B5A3501F25','B4ADA1E7-F81E-4F81-8DC2-51577245E76F'
	FROM mart.report_control WHERE control_id=36
END

----------------  
--Name: Ola
--Date: 2013-05-14
--Desc: new "Avaliability per Agent" report
-----------------
DECLARE @newreportid uniqueidentifier = 'A56B3EEF-17A2-4778-AA8A-D166232073D2'
DECLARE @CollectionId uniqueidentifier = (SELECT DISTINCT CollectionId FROM mart.report_control_collection WHERE collection_id=44)

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
		44,
		'~/Selection.aspx?ReportId=A56B3EEF-17A2-4778-AA8A-D166232073D2',
		'_blank',
		'Availability per Agent',
		'ResReportAvailabilityPerAgent',
		1,
		'~/Reports/CCC/report_availability_per_agent.rdlc',
		'mart.report_data_availability_per_agent',
		'f01:Report+AvailabilityPerAgent',
		'','','','',
		@CollectionId)
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
 CONSTRAINT [PK_stg_schedule_updated_ShiftStartDateUTC] PRIMARY KEY CLUSTERED 
(
	[shift_startdate_id] ASC,
	[person_id] ASC
)
)
GO

----------------  
--Name: Ola
--Date: 2013-08-06
--Desc: #19606 - Remove obsolete datamart tables
-----------------
alter table mart.dim_shift_length
drop DF_dim_shift_length_shift_length_group_id,  DF_dim_shift_length_shift_length_group_name
GO
alter table mart.dim_shift_length
drop column shift_length_group_id, shift_length_group_name
GO
DELETE FROM [mart].[report_control] WHERE control_id in(16,17)
GO
DROP TABLE [mart].[sys_shift_length_group]
GO
DROP TABLE [mart].[dim_person_category_type]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_person_category_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_person_category_get]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_person_category_type_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_control_person_category_type_get]
GO

----------------  
--Name: Erik S
--Date: 2013-08-13
--Desc: PBI12246
-----------------
TRUNCATE TABLE stage.stg_schedule_preference
ALTER TABLE stage.stg_schedule_preference
DROP COLUMN interval_id
GO

DECLARE @control_collection_id int
SET @control_collection_id = 
	(SELECT control_collection_id 
	FROM mart.report 
	WHERE report_id = 1)
DELETE FROM mart.report_control_collection
WHERE collection_id = @control_collection_id
	AND param_name = '@time_zone_id'
GO
----------------  
--Name: David
--Date: 2013-08-14
--Desc: #12446 - convert UTC date to agent local date
-----------------
update f
set
	date_id =  b.local_date_id,
	interval_id = 0
from mart.fact_schedule_preference f
inner join mart.dim_person p
	on p.person_id = f.person_id
inner join mart.bridge_time_zone b
	on b.date_id = f.date_id
	and b.interval_id = f.interval_id
	and p.time_zone_id = b.time_zone_id