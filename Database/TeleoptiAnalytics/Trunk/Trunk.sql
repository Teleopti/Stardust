----------------  
--Name: David Jonsson
--Date: 2013-12-18
--Desc: Bug #26329 - Adding index for faster report permissions
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dim_person_to_be_deleted')
CREATE NONCLUSTERED INDEX IX_dim_person_to_be_deleted
ON [mart].[dim_person] ([to_be_deleted])
INCLUDE ([person_id],[team_id],[business_unit_code])
GO
 
----------------  
--Name: David Jonsson
--Date: 2013-12-16
--Desc: bug #26204 - make hard join on null guid instead of isnull
----------------
--make sure we use a zero guid instead of dbNull
update mart.dim_absence
set absence_code='00000000-0000-0000-0000-000000000000'
where absence_id=-1

update mart.dim_activity
set activity_code='00000000-0000-0000-0000-000000000000'
where activity_id=-1

update mart.dim_overtime
set overtime_code='00000000-0000-0000-0000-000000000000'
where overtime_id=-1

update mart.dim_shift_category
set shift_category_code='00000000-0000-0000-0000-000000000000'
where shift_category_id =-1

--add supporting indexes to the stg_schedule view
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_activity]') AND name = N'IX_activity_code')
CREATE NONCLUSTERED INDEX [IX_activity_code] ON [mart].[dim_activity]
(
	[activity_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_absence]') AND name = N'IX_absence_code')
CREATE NONCLUSTERED INDEX [IX_absence_code] ON [mart].[dim_absence]
(
	[absence_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_scenario]') AND name = N'IX_scenario_code')
CREATE NONCLUSTERED INDEX [IX_scenario_code] ON [mart].[dim_scenario]
(
	[scenario_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_shift_category]') AND name = N'IX_shift_category_code')
CREATE NONCLUSTERED INDEX [IX_shift_category_code] ON [mart].[dim_shift_category]
(
	[shift_category_code] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_shift_length]') AND name = N'IX_shift_length_m')
CREATE NONCLUSTERED INDEX [IX_shift_length_m] ON [mart].[dim_shift_length]
(
	[shift_length_m] ASC
)

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_overtime]') AND name = N'IX_dim_overtime_code')
CREATE NONCLUSTERED INDEX [IX_dim_overtime_code] ON [mart].[dim_overtime]
(
	[overtime_code] ASC
)

GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_scenario_id')
DROP INDEX [IX_fact_schedule_scenario_id] ON [mart].[fact_schedule]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'IX_fact_schedule_scenario_shift_category')
DROP INDEX [IX_fact_schedule_scenario_shift_category] ON [mart].[fact_schedule]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND name = N'idx_fact_schedule_schedule_date_id')
DROP INDEX [idx_fact_schedule_schedule_date_id] ON [mart].[fact_schedule]
GO



----------------  
--Name: David Jonsson
--Date: 2013-12-18
--Desc: Bug #26329 - Adding index for faster report permissions
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dim_person_to_be_deleted')
CREATE NONCLUSTERED INDEX IX_dim_person_to_be_deleted
ON [mart].[dim_person] ([to_be_deleted])
INCLUDE ([person_id],[team_id],[business_unit_code])
GO
----------------  
--Name: Karin
--Date: 2013-11-07
--Desc: #24978 New report and report collection and control
-----------------
--ADD NEW CONTROL
IF NOT EXISTS(SELECT 1 FROM mart.report_control where control_id=44)
BEGIN
	INSERT mart.report_control(Id, control_id, control_name, fill_proc_name)
	SELECT NEWID(), 44, 'twolistStateGroup','mart.report_control_twolist_state_group_get'
END

--ADD REPORT CONTROL COLLECTION
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where CollectionId='F775ED72-5B41-4FEA-87DB-04AD347D4537')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '0405D0BA-37C2-49BF-8E4F-B18EEB82A8AF',	mart.report_control.Id,	'F775ED72-5B41-4FEA-87DB-04AD347D4537',496,47,1,1,'12:00','ResDateFromColon',NULL,'@date_from',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=1

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '3BF4D1B4-6DBA-4F9B-B5F5-631D1FAB725D',	mart.report_control.Id,	'F775ED72-5B41-4FEA-87DB-04AD347D4537',497,47,2,2,'12:00','ResDateToColon',NULL,'@date_to',496,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=2


	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '040C01EE-7596-4F2C-A5C8-DA62DFF1F599',mart.report_control.Id,	'F775ED72-5B41-4FEA-87DB-04AD347D4537',498,47,3,29,'-2','ResGroupPageColon',NULL,'@group_page_code',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=29

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '6D3A4EA0-CBEB-42A7-B37C-15E009308D8A',mart.report_control.Id,	'F775ED72-5B41-4FEA-87DB-04AD347D4537',499,47,4,35,'-99','ResGroupPageGroupColon',NULL,'@group_page_group_set',498,NULL,NULL,	NULL,'040C01EE-7596-4F2C-A5C8-DA62DFF1F599',NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=35

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'A0906C71-4202-4469-88C1-364FCA39CFB7',mart.report_control.Id,	'F775ED72-5B41-4FEA-87DB-04AD347D4537',500,47,5,37,'-2','ResAgentColon',NULL,'@group_page_agent_code',496,497,498,499,'0405D0BA-37C2-49BF-8E4F-B18EEB82A8AF', '3BF4D1B4-6DBA-4F9B-B5F5-631D1FAB725D','040C01EE-7596-4F2C-A5C8-DA62DFF1F599','6D3A4EA0-CBEB-42A7-B37C-15E009308D8A'
	FROM mart.report_control WHERE control_id=37

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '475CE887-4C4A-4C44-9D21-7D68DADC77B9',mart.report_control.Id,'F775ED72-5B41-4FEA-87DB-04AD347D4537',500,47,6,3,'-2','ResSiteNameColon',NULL,'@site_id',496,497,498,NULL,'0405D0BA-37C2-49BF-8E4F-B18EEB82A8AF', '3BF4D1B4-6DBA-4F9B-B5F5-631D1FAB725D','040C01EE-7596-4F2C-A5C8-DA62DFF1F599',NULL
	FROM mart.report_control WHERE control_id=3

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '0B726A61-DDD9-4E01-A4E1-55FE6A877E17',mart.report_control.Id,'F775ED72-5B41-4FEA-87DB-04AD347D4537',501,47,7,34,'-99','ResTeamNameColon',NULL,'@team_set',496,497,498,500,'0405D0BA-37C2-49BF-8E4F-B18EEB82A8AF', '3BF4D1B4-6DBA-4F9B-B5F5-631D1FAB725D','040C01EE-7596-4F2C-A5C8-DA62DFF1F599','475CE887-4C4A-4C44-9D21-7D68DADC77B9'
	FROM mart.report_control WHERE control_id=34

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'FDF243DB-C6F0-4E15-B520-D6A50C2862C8',mart.report_control.Id,'F775ED72-5B41-4FEA-87DB-04AD347D4537',502,47,8,36,'-2','ResAgentsColon',NULL,'@agent_person_code',496,497,500,501,'0405D0BA-37C2-49BF-8E4F-B18EEB82A8AF', '3BF4D1B4-6DBA-4F9B-B5F5-631D1FAB725D','475CE887-4C4A-4C44-9D21-7D68DADC77B9','0B726A61-DDD9-4E01-A4E1-55FE6A877E17'
	FROM mart.report_control WHERE control_id=36

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'A5E64353-0866-450D-A9E5-27169160DA6F',mart.report_control.Id,'F775ED72-5B41-4FEA-87DB-04AD347D4537',503,47,9,44,'-99','ResStateGroupColon',NULL,'@state_group_set',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=44

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '7D54C15F-E46E-42ED-99B8-9C90B32E0EB8',mart.report_control.Id,'F775ED72-5B41-4FEA-87DB-04AD347D4537',504,47,10,22,'-95','ResTimeZoneColon',NULL,	'@time_zone_id',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=22
END

----ADD REPORT
DECLARE @newreportid uniqueidentifier
SET  @newreportid= 'BB8C21BA-0756-4DDC-8B26-C9D5715A3443'
DECLARE @CollectionId uniqueidentifier
SELECT DISTINCT @CollectionId = CollectionId FROM mart.report_control_collection WHERE collection_id=47
IF NOT EXISTS(SELECT 1 FROM mart.report where Id='BB8C21BA-0756-4DDC-8B26-C9D5715A3443')
BEGIN
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
		32,
		47,
		'~/Selection.aspx?ReportId=BB8C21BA-0756-4DDC-8B26-C9D5715A3443',
		'_blank',
		'Time in State per Agent',
		'ResReportTimeInStatePerAgent',
		1,
		'~/Reports/CCC/CCCV8_report_time_in_state_per_agent.rdlc',
		'mart.report_data_time_in_state_per_agent',
		'f01:Report+TimeInStatePerAgent',
		'','','','',
		@CollectionId)
END
GO

----------------  
--Name: David J
--Date: 2013-11-07
--Desc: new tables for RTA state report
----------------
--trim datatypes in current live table
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN [StateCode] nvarchar(25)
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN [State] nvarchar(50)
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN [Scheduled] nvarchar(50)
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN [ScheduledNext] nvarchar(50)
ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN [AlarmName] nvarchar(50)

--new stage table
CREATE TABLE [RTA].[ActualAgentState_History](
	id int identity(1, 1) NOT NULL,
	[StateStart] datetime NOT NULL,
	[person_code] uniqueidentifier NOT NULL,
	[time_in_state_s] int NOT NULL,
	[state_group_code] uniqueidentifier NOT NULL,
	[days_cross_midnight] smallint NOT NULL
)

CREATE UNIQUE CLUSTERED INDEX [CIX_ActualAgentState_History] ON [RTA].[ActualAgentState_History]
(
	days_cross_midnight desc,
	id asc
)

CREATE TABLE [stage].[stg_agent_state](
	id int identity(1, 1) NOT NULL,
	[StateStart] datetime NOT NULL,
	[person_code] uniqueidentifier NOT NULL,
	[time_in_state_s] int NOT NULL,
	[state_group_code] uniqueidentifier NOT NULL,
	[days_cross_midnight] smallint NOT NULL
)

CREATE UNIQUE CLUSTERED INDEX [CIX_stg_agent_state] ON [stage].[stg_agent_state]
(
	days_cross_midnight desc,
	id asc
)

--new stage table
CREATE TABLE [stage].[stg_state_group](
	[state_group_code] uniqueidentifier NOT NULL,
	[state_group_name] nvarchar(50) NOT NULL,
	[business_unit_code] uniqueidentifier NULL,
	[datasource_id] smallint NOT NULL,
	[insert_date] smalldatetime NOT NULL,
	[update_date] smalldatetime NOT NULL,
	[datasource_update_date] smalldatetime NOT NULL,
	[is_deleted] [bit] NOT NULL,
	[is_log_out_state] bit NOT NULL
)
ALTER TABLE [stage].[stg_state_group] ADD  CONSTRAINT [PK_stg_state_group] PRIMARY KEY CLUSTERED
(
	[state_group_code] ASC
)

--new dimension table
CREATE TABLE [mart].[dim_state_group](
	[state_group_id] int IDENTITY(1,1) NOT NULL,
	[state_group_code] uniqueidentifier NULL,
	[state_group_name] nvarchar(50) NOT NULL,
	[business_unit_id] int NULL,
	[datasource_id] smallint NOT NULL,
	[insert_date] smalldatetime NOT NULL,
	[update_date] smalldatetime NOT NULL,
	[datasource_update_date] smalldatetime NOT NULL,
	[is_deleted] [bit] NOT NULL,
	[is_log_out_state] bit NOT NULL
)

ALTER TABLE [mart].[dim_state_group] ADD  CONSTRAINT [PK_dim_state_group] PRIMARY KEY CLUSTERED 
(
	[state_group_id] ASC
)

ALTER TABLE [mart].[dim_state_group]  WITH NOCHECK ADD  CONSTRAINT [FK_dim_state_group_dim_business_unit] FOREIGN KEY([business_unit_id])
REFERENCES [mart].[dim_business_unit] ([business_unit_id])
ALTER TABLE [mart].[dim_state_group] CHECK CONSTRAINT [FK_dim_state_group_dim_business_unit]
GO

--new fact table
CREATE TABLE [mart].[fact_agent_state](
	[date_id] int NOT NULL,
	[person_id] int NOT NULL,
	[state_group_id] int NOT NULL,
	[time_in_state_s] int NOT NULL,
	[datasource_id] smallint NOT NULL,
	[insert_date] smalldatetime NOT NULL
)

ALTER TABLE [mart].[fact_agent_state] ADD CONSTRAINT [PK_fact_agent_state] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[person_id] ASC,
	[state_group_id] ASC
)

--A number table
CREATE TABLE mart.sys_numbers (n smallint not null)
ALTER TABLE mart.sys_numbers ADD CONSTRAINT
PK_sys_numbers PRIMARY KEY CLUSTERED
(
	n
)

INSERT INTO mart.sys_numbers
SELECT top(100) ROW_NUMBER() OVER (ORDER BY [object_id])-1 FROM sys.all_columns

GO
ALTER TABLE [mart].[fact_agent_state]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_state_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_agent_state]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_state_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[fact_agent_state]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_state_dim_state_group] FOREIGN KEY([state_group_id])
REFERENCES [mart].[dim_state_group] ([state_group_id])
GO

----------------  
--Name: DJ
--Date: 2013-11-08
--Desc: #26422 - New jobsteps for stg_state_group and fact_agent_state
----------------
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'stg_state_group' AND jobstep_id=84)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(84,N'stg_state_group')
GO
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'dim_state_group' AND jobstep_id=85)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(85,N'dim_state_group')
GO
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'fact_agent_state' AND jobstep_id=86)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(86,N'fact_agent_state')
GO

----------------  
--Name: DJ
--Date: 2014-02-03
--Desc: #26422 - re-factor mart do match new PersonAssignment
----------------
DROP TABLE [stage].[stg_schedule]
GO
CREATE TABLE [stage].[stg_schedule](
	[schedule_date_local] smalldatetime NOT NULL,
	[schedule_date_utc] smalldatetime NOT NULL,
	[person_code] uniqueidentifier NOT NULL,
	[interval_id] smallint NOT NULL,
	[activity_start] smalldatetime NOT NULL,
	[scenario_code] uniqueidentifier NOT NULL,
	[activity_code] uniqueidentifier NULL,
	[absence_code] uniqueidentifier NULL,
	[activity_end] smalldatetime NOT NULL,
	[shift_start] smalldatetime NOT NULL,
	[shift_end] smalldatetime NOT NULL,
	[shift_startinterval_id] smallint NOT NULL,
	[shift_endinterval_id] smallint NOT NULL,
	[shift_category_code] uniqueidentifier NULL,
	[shift_length_m] int NOT NULL,
	[scheduled_time_m] int NULL,
	[scheduled_time_absence_m] int NULL,
	[scheduled_time_activity_m] int NULL,
	[scheduled_contract_time_m] int NULL,
	[scheduled_contract_time_activity_m] int NULL,
	[scheduled_contract_time_absence_m] int NULL,
	[scheduled_work_time_m] int NULL,
	[scheduled_work_time_activity_m] int NULL,
	[scheduled_work_time_absence_m] int NULL,
	[scheduled_over_time_m] int NULL,
	[scheduled_ready_time_m] int NULL,
	[scheduled_paid_time_m] int NULL,
	[scheduled_paid_time_activity_m] int NULL,
	[scheduled_paid_time_absence_m] int NULL,
	[business_unit_code] uniqueidentifier NOT NULL,
	[business_unit_name] nvarchar(50) NOT NULL,
	[datasource_id] smallint NOT NULL,
	[insert_date] smalldatetime NOT NULL,
	[update_date] smalldatetime NOT NULL,
	[datasource_update_date] smalldatetime NOT NULL,
	[overtime_code] uniqueidentifier NULL
)

ALTER TABLE [stage].[stg_schedule] ADD  CONSTRAINT [PK_stg_schedule] PRIMARY KEY CLUSTERED
(
	[schedule_date_local] ASC,
	[schedule_date_utc] ASC,
	[person_code] ASC,
	[interval_id] ASC,
	[activity_start] ASC,
	[scenario_code] ASC,
	[shift_start] ASC
)

ALTER TABLE [stage].[stg_schedule] ADD  CONSTRAINT [DF_stg_schedule_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
ALTER TABLE [stage].[stg_schedule] ADD  CONSTRAINT [DF_stg_schedule_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [stage].[stg_schedule] ADD  CONSTRAINT [DF_stg_schedule_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
DROP TABLE [stage].[stg_schedule_changed]
GO
CREATE TABLE [stage].[stg_schedule_changed](
	[schedule_date_local] smalldatetime NOT NULL,
	[person_code] uniqueidentifier NOT NULL,
	[scenario_code] uniqueidentifier NOT NULL,
	[business_unit_code] uniqueidentifier NOT NULL,
	[datasource_id] smallint NOT NULL,
	[datasource_update_date] smalldatetime NOT NULL
)

ALTER TABLE [stage].[stg_schedule_changed] ADD CONSTRAINT [PK_stg_schedule_changed] PRIMARY KEY CLUSTERED
(
	[schedule_date_local] ASC,
	[person_code] ASC,
	[scenario_code] ASC
)

----------------  
--Name: KJ
--Date: 2014-02-04
--Desc: #26422 - New column fact_schedule for local date
----------------
--drop all constraints
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_scenario]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_person]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_interval1]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_interval]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_date4]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_date3]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_date2]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_date1]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_date]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_activity]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [FK_fact_schedule_dim_absence]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_update_date]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_insert_date]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_datasource_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_last_publish]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_length_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_category_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_startinterval_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_endtime]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_enddate_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_starttime]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_startdate_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_activity_endtime]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_activity_enddate_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_activity_startdate_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_absence_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_activity_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_scenario_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_activity_starttime]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_interval_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_person_id]
GO
ALTER TABLE [mart].[fact_schedule] DROP CONSTRAINT [DF_fact_schedule_schedule_date_id]
GO
--RENAME old TABLE
EXEC sp_rename 'mart.fact_schedule', 'fact_schedule_old'
GO
--RENAME TMP KEY
EXEC sp_rename 'mart.PK_fact_schedule', 'PK_fact_schedule_old'
GO

CREATE TABLE [mart].[fact_schedule](
	[shift_startdate_local_id] [int] NOT NULL,
	[schedule_date_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[activity_starttime] [smalldatetime] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[activity_id] [int] NULL,
	[absence_id] [int] NULL,
	[activity_startdate_id] [int] NULL,
	[activity_enddate_id] [int] NULL,
	[activity_endtime] [smalldatetime] NULL,
	[shift_startdate_id] [int] NULL,
	[shift_starttime] [smalldatetime] NULL,
	[shift_enddate_id] [int] NULL,
	[shift_endtime] [smalldatetime] NULL,
	[shift_startinterval_id] [smallint] NULL,
	[shift_endinterval_id] smallint NULL,
	[shift_category_id] [int] NULL,
	[shift_length_id] [int] NULL,
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
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[overtime_id] [int] NOT NULL
)

ALTER TABLE [mart].[fact_schedule] ADD CONSTRAINT [PK_fact_schedule] PRIMARY KEY CLUSTERED
(
	[shift_startdate_local_id] ASC,
	[scenario_id] ASC,
	[person_id] ASC,
	[schedule_date_id] ASC,
	[interval_id] ASC,
	[activity_starttime] ASC
)
GO

--Prepare intervals for new column
PRINT '----'
PRINT 'new data into: [mart].[fact_schedule]. Working ...'
GO
DECLARE @date_min smalldatetime
SET @date_min='1900-01-01'

CREATE TABLE #intervals
(
	interval_id smallint not null,
	interval_start smalldatetime null,
	interval_end smalldatetime null
)

INSERT #intervals(interval_id,interval_start,interval_end)
SELECT interval_id= interval_id,
	interval_start= interval_start,
	interval_end = interval_end
FROM mart.dim_interval
ORDER BY interval_id
--remove one minute from last interval to be able to join shifts ending at UTC midnight
update #intervals 
set interval_end=dateadd(minute,-1,interval_end) 
where interval_end=dateadd(day,1,@date_min)

--INSERT DATA FROM OLD FACT_SCHEDULE
INSERT [mart].[fact_schedule] WITH(TABLOCK)
(shift_startdate_local_id, schedule_date_id, person_id, interval_id, activity_starttime, scenario_id, activity_id, absence_id, activity_startdate_id, activity_enddate_id, activity_endtime, shift_startdate_id, shift_starttime, shift_enddate_id, shift_endtime, shift_startinterval_id, shift_endinterval_id, shift_category_id, shift_length_id, scheduled_time_m, scheduled_time_absence_m, scheduled_time_activity_m, scheduled_contract_time_m, scheduled_contract_time_activity_m, scheduled_contract_time_absence_m, scheduled_work_time_m, scheduled_work_time_activity_m, scheduled_work_time_absence_m, scheduled_over_time_m, scheduled_ready_time_m, scheduled_paid_time_m, scheduled_paid_time_activity_m, scheduled_paid_time_absence_m, business_unit_id, datasource_id, insert_date, update_date, datasource_update_date, overtime_id)
SELECT btz.local_date_id, f.schedule_date_id, f.person_id, f.interval_id, f.activity_starttime, f.scenario_id, f.activity_id, f.absence_id, f.activity_startdate_id, f.activity_enddate_id, f.activity_endtime, f.shift_startdate_id, f.shift_starttime, f.shift_enddate_id, f.shift_endtime, f.shift_startinterval_id, di.interval_id, f.shift_category_id, f.shift_length_id, f.scheduled_time_m, f.scheduled_time_absence_m, f.scheduled_time_activity_m, f.scheduled_contract_time_m, f.scheduled_contract_time_activity_m, f.scheduled_contract_time_absence_m, f.scheduled_work_time_m, f.scheduled_work_time_activity_m, f.scheduled_work_time_absence_m, f.scheduled_over_time_m, f.scheduled_ready_time_m, f.scheduled_paid_time_m, f.scheduled_paid_time_activity_m, f.scheduled_paid_time_absence_m, f.business_unit_id, f.datasource_id, f.insert_date, f.update_date, f.datasource_update_date, f.overtime_id
FROM [mart].[fact_schedule_old] f
INNER JOIN mart.bridge_time_zone btz 
	ON f.shift_startdate_id=btz.date_id 
	AND f.shift_startinterval_id=btz.interval_id
INNER JOIN mart.dim_person dp
	ON f.person_id=dp.person_id
	AND btz.time_zone_id=dp.time_zone_id
INNER JOIN #intervals di
	ON	dateadd(hour,DATEPART(hour,f.shift_endtime),@date_min)+ dateadd(minute,DATEPART(minute,f.shift_endtime),@date_min) > di.interval_start
	AND	dateadd(hour,DATEPART(hour,f.shift_endtime),@date_min)+ dateadd(minute,DATEPART(minute,f.shift_endtime),@date_min) <= di.interval_end
GO
PRINT 'new data into: [mart].[fact_schedule]. Done!'
GO

--ADD ALL CONSTRAINTS
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_schedule_date_id]  DEFAULT ((-1)) FOR [schedule_date_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_person_id]  DEFAULT ((-1)) FOR [person_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_interval_id]  DEFAULT ((-1)) FOR [interval_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_activity_starttime]  DEFAULT (((1900)-(1))-(1)) FOR [activity_starttime]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_scenario_id]  DEFAULT ((-1)) FOR [scenario_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_activity_id]  DEFAULT ((-1)) FOR [activity_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_absence_id]  DEFAULT ((-1)) FOR [absence_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_activity_startdate_id]  DEFAULT ((-1)) FOR [activity_startdate_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_activity_enddate_id]  DEFAULT ((-1)) FOR [activity_enddate_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_activity_endtime]  DEFAULT (((2059)-(12))-(31)) FOR [activity_endtime]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_startdate_id]  DEFAULT ((-1)) FOR [shift_startdate_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_starttime]  DEFAULT (((1900)-(1))-(1)) FOR [shift_starttime]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_enddate_id]  DEFAULT ((-1)) FOR [shift_enddate_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_endtime]  DEFAULT (((2059)-(12))-(31)) FOR [shift_endtime]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_startinterval_id]  DEFAULT ((-1)) FOR [shift_startinterval_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_endinterval_id]  DEFAULT ((-1)) FOR [shift_endinterval_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_category_id]  DEFAULT ((-1)) FOR [shift_category_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_length_id]  DEFAULT ((-1)) FOR [shift_length_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_schedule] ADD  DEFAULT ((-1)) FOR [overtime_id]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_absence] FOREIGN KEY([absence_id])
REFERENCES [mart].[dim_absence] ([absence_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_absence]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_activity] FOREIGN KEY([activity_id])
REFERENCES [mart].[dim_activity] ([activity_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_activity]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date] FOREIGN KEY([schedule_date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date1] FOREIGN KEY([activity_startdate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date1]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date2] FOREIGN KEY([activity_enddate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date2]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date3] FOREIGN KEY([shift_startdate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date3]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date4] FOREIGN KEY([shift_enddate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date4]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_interval]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_interval1] FOREIGN KEY([shift_startinterval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_interval1]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_interval2] FOREIGN KEY([shift_endinterval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_interval2]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_person]
GO
ALTER TABLE [mart].[fact_schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_scenario]
GO

----------------  
--Name: KJ
--Date: 2014-02-04
--Desc: #26422 - New column fact_schedule_deviation for local date
----------------
--drop constraints
ALTER TABLE [mart].[fact_schedule_deviation] DROP CONSTRAINT [FK_fact_schedule_deviation_dim_person]
GO

ALTER TABLE [mart].[fact_schedule_deviation] DROP CONSTRAINT [FK_fact_schedule_deviation_dim_interval]
GO

ALTER TABLE [mart].[fact_schedule_deviation] DROP CONSTRAINT [FK_fact_schedule_deviation_dim_date]
GO

ALTER TABLE [mart].[fact_schedule_deviation] DROP CONSTRAINT [DF_fact_schedule_deviation_update_date]
GO

ALTER TABLE [mart].[fact_schedule_deviation] DROP CONSTRAINT [DF_fact_schedule_deviation_insert_date]
GO

ALTER TABLE [mart].[fact_schedule_deviation] DROP CONSTRAINT [DF_fact_schedule_deviation_datasource_id]
GO

--RENAME old TABLE
EXEC sp_rename 'mart.fact_schedule_deviation', 'fact_schedule_deviation_old'
GO
--RENAME TMP KEY
EXEC sp_rename 'mart.PK_fact_schedule_deviation', 'PK_fact_schedule_deviation_old'
GO
PRINT '----'
PRINT 'new data into: [mart].[fact_schedule_deviation]. Working ...'
GO
CREATE TABLE [mart].[fact_schedule_deviation](
	[shift_startdate_local_id] [int] NOT NULL,
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[person_id] [int] NOT NULL,
	[scheduled_ready_time_s] [int] NULL,
	[ready_time_s] [int] NULL,
	[contract_time_s] [int] NULL,
	[deviation_schedule_s] [decimal](18, 4) NULL,
	[deviation_schedule_ready_s] [decimal](18, 4) NULL,
	[deviation_contract_s] [decimal](18, 4) NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[is_logged_in] [bit] NOT NULL,
	[shift_startdate_id] [int] NULL,
	[shift_startinterval_id] [smallint] NULL
)

ALTER TABLE [mart].[fact_schedule_deviation] ADD CONSTRAINT [PK_fact_schedule_deviation] PRIMARY KEY CLUSTERED 
(
	[shift_startdate_local_id] ASC,
	[date_id] ASC,
	[interval_id] ASC,
	[person_id] ASC
)
GO

--move data
--INSERT DATA FROM OLD FACT_SCHEDULE_DEVIATION
INSERT [mart].[fact_schedule_deviation] WITH (TABLOCK)
(shift_startdate_local_id,date_id, interval_id, person_id, scheduled_ready_time_s, ready_time_s, contract_time_s, deviation_schedule_s, deviation_schedule_ready_s, deviation_contract_s, business_unit_id, datasource_id, insert_date, update_date, is_logged_in, shift_startdate_id, shift_startinterval_id )
SELECT btz.local_date_id,f.date_id, f.interval_id,f.person_id, scheduled_ready_time_s, ready_time_s, contract_time_s, deviation_schedule_s, deviation_schedule_ready_s, deviation_contract_s, f.business_unit_id, f.datasource_id, f.insert_date, f.update_date, is_logged_in, shift_startdate_id, shift_startinterval_id
FROM [mart].[fact_schedule_deviation_old] f
INNER JOIN mart.bridge_time_zone btz 
	ON f.shift_startdate_id=btz.date_id 
	AND f.shift_startinterval_id=btz.interval_id
INNER JOIN mart.dim_person dp
	ON f.person_id=dp.person_id
	AND btz.time_zone_id=dp.time_zone_id
GO
PRINT '[mart].[fact_schedule_deviation]. Done!'
GO

--add constraints
ALTER TABLE [mart].[fact_schedule_deviation] ADD  CONSTRAINT [DF_fact_schedule_deviation_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO

ALTER TABLE [mart].[fact_schedule_deviation] ADD  CONSTRAINT [DF_fact_schedule_deviation_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [mart].[fact_schedule_deviation] ADD  CONSTRAINT [DF_fact_schedule_deviation_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

ALTER TABLE [mart].[fact_schedule_deviation]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_deviation_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO

ALTER TABLE [mart].[fact_schedule_deviation] CHECK CONSTRAINT [FK_fact_schedule_deviation_dim_date]
GO

ALTER TABLE [mart].[fact_schedule_deviation]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_deviation_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO

ALTER TABLE [mart].[fact_schedule_deviation] CHECK CONSTRAINT [FK_fact_schedule_deviation_dim_interval]
GO

ALTER TABLE [mart].[fact_schedule_deviation]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_deviation_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO

ALTER TABLE [mart].[fact_schedule_deviation] CHECK CONSTRAINT [FK_fact_schedule_deviation_dim_person]
GO

--Drop unused code
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[DimPersonAdapted]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[DimPersonAdapted]

--use local agent date on DayOff
DROP TABLE [stage].[stg_schedule_day_off_count]
GO
CREATE TABLE [stage].[stg_schedule_day_off_count](
	[schedule_date_local] [smalldatetime] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[starttime] [smalldatetime] NULL,
	[day_off_code] [uniqueidentifier] NULL,
	[day_off_name] [nvarchar](50) NOT NULL,
	[day_off_shortname] [nvarchar](25) NOT NULL,
	[day_count] [int] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL
)

ALTER TABLE [stage].[stg_schedule_day_off_count] ADD CONSTRAINT [PK_stg_schedule_day_off_count] PRIMARY KEY CLUSTERED
(
	[schedule_date_local] ASC,
	[person_code] ASC,
	[scenario_code] ASC
)

ALTER TABLE [stage].[stg_schedule_day_off_count] ADD  CONSTRAINT [DF_stg_schedule_day_off_count_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
ALTER TABLE [stage].[stg_schedule_day_off_count] ADD  CONSTRAINT [DF_stg_schedule_day_off_count_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [stage].[stg_schedule_day_off_count] ADD  CONSTRAINT [DF_stg_schedule_day_off_count_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

--old fact table
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [FK_fact_schedule_day_count_dim_absence]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [FK_fact_schedule_day_count_dim_date]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [FK_fact_schedule_day_count_dim_day_off]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [FK_fact_schedule_day_count_dim_interval]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [FK_fact_schedule_day_count_dim_person]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [FK_fact_schedule_day_count_dim_scenario]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [FK_fact_schedule_day_count_dim_shift_category]

ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [DF_fact_schedule_day_count_absence_id]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [DF_fact_schedule_day_count_datasource_id]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [DF_fact_schedule_day_count_date_id]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [DF_fact_schedule_day_count_insert_date]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [DF_fact_schedule_day_count_person_id]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [DF_fact_schedule_day_count_scenario_id]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [DF_fact_schedule_day_count_shift_category_id]
ALTER TABLE [mart].[fact_schedule_day_count] DROP CONSTRAINT [DF_fact_schedule_day_count_update_date]
GO

EXEC sp_rename 'mart.fact_schedule_day_count', 'fact_schedule_day_count_old'
GO
EXEC sp_rename 'mart.PK_fact_schedule_day_count', 'PK_fact_schedule_day_count_old'
GO

--new fact table
CREATE TABLE [mart].[fact_schedule_day_count](
	[shift_startdate_local_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[starttime] [smalldatetime] NULL,
	[shift_category_id] [int] NOT NULL,
	[day_off_id] [int] NOT NULL,
	[absence_id] [int] NOT NULL,
	[day_count] [int] NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL
)

ALTER  TABLE [mart].[fact_schedule_day_count] ADD CONSTRAINT [PK_fact_schedule_day_count] PRIMARY KEY CLUSTERED 
(
	[shift_startdate_local_id] ASC,
	[person_id] ASC,
	[scenario_id] ASC
)

ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_date_id]  DEFAULT ((-1)) FOR [shift_startdate_local_id]
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_person_id]  DEFAULT ((-1)) FOR [person_id]
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_scenario_id]  DEFAULT ((-1)) FOR [scenario_id]
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_shift_category_id]  DEFAULT ((-1)) FOR [shift_category_id]
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_absence_id]  DEFAULT ((-1)) FOR [absence_id]
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_update_date]  DEFAULT (getdate()) FOR [update_date]

--get old data
GO
PRINT '----'
PRINT 'new data into: [mart].[fact_schedule_day_count]. Working ...'
GO
INSERT INTO  [mart].[fact_schedule_day_count] WITH (TABLOCK)
(shift_startdate_local_id, person_id, scenario_id, starttime, shift_category_id, day_off_id, absence_id, day_count, business_unit_id, datasource_id, insert_date, update_date, datasource_update_date)
SELECT 
	btz.local_date_id,
	f.person_id,
	f.scenario_id,
	max(f.starttime),
	max(f.shift_category_id),
	max(f.day_off_id),
	max(f.absence_id),
	max(f.day_count),
	max(f.business_unit_id),
	max(f.datasource_id),
	max(f.insert_date),
	max(f.update_date),
	max(f.datasource_update_date)
FROM [mart].[fact_schedule_day_count_old] f
INNER JOIN mart.bridge_time_zone btz 
	ON f.date_id=btz.date_id 
	AND f.start_interval_id=btz.interval_id
INNER JOIN mart.dim_person dp
	ON f.person_id=dp.person_id
	AND btz.time_zone_id=dp.time_zone_id
GROUP BY
	btz.local_date_id,
	f.person_id,
	f.scenario_id
GO
PRINT '[mart].[fact_schedule_day_count]. Done!'
GO
ALTER TABLE [mart].[fact_schedule_day_count]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_absence] FOREIGN KEY([absence_id])
REFERENCES [mart].[dim_absence] ([absence_id])
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_absence]

ALTER TABLE [mart].[fact_schedule_day_count]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_date] FOREIGN KEY([shift_startdate_local_id])
REFERENCES [mart].[dim_date] ([date_id])
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_date]

ALTER TABLE [mart].[fact_schedule_day_count]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_day_off] FOREIGN KEY([day_off_id])
REFERENCES [mart].[dim_day_off] ([day_off_id])
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_day_off]

ALTER TABLE [mart].[fact_schedule_day_count]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_person]

ALTER TABLE [mart].[fact_schedule_day_count]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_scenario]

ALTER TABLE [mart].[fact_schedule_day_count]  WITH NOCHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_shift_category] FOREIGN KEY([shift_category_id])
REFERENCES [mart].[dim_shift_category] ([shift_category_id])
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_shift_category]
GO

DROP TABLE [stage].[stg_schedule_day_absence_count]
GO
CREATE TABLE [stage].[stg_schedule_day_absence_count](
	[schedule_date_local] [smalldatetime] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[starttime] [smalldatetime] NULL,
	[absence_code] [uniqueidentifier] NOT NULL,
	[day_count] [int] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL
)

ALTER TABLE [stage].[stg_schedule_day_absence_count] ADD CONSTRAINT [PK_stg_schedule_day_absence_count] PRIMARY KEY CLUSTERED 
(
	[schedule_date_local] ASC,
	[person_code] ASC,
	[scenario_code] ASC
)
ALTER TABLE [stage].[stg_schedule_day_absence_count] ADD  CONSTRAINT [DF_stg_schedule_day_absence_count_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
ALTER TABLE [stage].[stg_schedule_day_absence_count] ADD  CONSTRAINT [DF_stg_schedule_day_absence_count_insert_date]  DEFAULT (getdate()) FOR [insert_date]
ALTER TABLE [stage].[stg_schedule_day_absence_count] ADD  CONSTRAINT [DF_stg_schedule_day_absence_count_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

--drop dead code
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dimPersonPeriodSpanUTC]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[dimPersonPeriodSpanUTC]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[LocalDateIntervalToUTC]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[LocalDateIntervalToUTC]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[ReportAgents]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[ReportAgents]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[IntervalInfo]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [mart].[IntervalInfo]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Stage].[etl_stg_schedule_updated_special_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stage].[etl_stg_schedule_updated_special_load]
GO

----------------  
--Name: KJ + DJ
--Date: 2014-04-02
--Desc: #26422 - Remove time_zone from selection page since now using agent local time zone
----------------

DELETE from [mart].[report_user_setting] 
WHERE ReportId in ('C5B88862-F7BE-431B-A63F-3DD5FF8ACE54','D45A8874-57E1-4EB9-826D-E216A4CBC45B')
AND param_name='@time_zone_id' 

DELETE FROM  mart.report_control_collection where collection_id=25 AND param_name='@time_zone_id'
GO

DELETE FROM [mart].[report_user_setting] 
WHERE ReportId in ('2F222F0A-4571-4462-8FBE-0C747035994A','35649814-4DE8-4CB3-A51C-DDBA2A073E09')
AND param_name='@time_zone_id' 

DELETE FROM  mart.report_control_collection where collection_id=26 AND param_name='@time_zone_id'
GO

--messed up cross view target found in customer database
update mart.sys_crossdatabaseview_target
set target_defaultName='TeleoptiCCCAgg'
where target_id=4
GO