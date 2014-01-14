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
CREATE TABLE [stage].[stg_agent_state](
	[person_code] uniqueidentifier NOT NULL,
	[state_group_name] nvarchar(50) NOT NULL,
	[state_group_code] uniqueidentifier NOT NULL,
	[StateStart] datetime NOT NULL,
--	[interval] smalldatetime NOT NULL,
	[StateEnd] datetime NOT NULL
)

CREATE CLUSTERED INDEX [CIX_stg_agent_state] ON [stage].[stg_agent_state]
(
	[person_code] ASC
)

/*
CREATE TABLE [stage].[stg_agent_state_loading](
	[person_code] uniqueidentifier NOT NULL,
	[state_group_code] uniqueidentifier NOT NULL,
	[time_in_state_s] int NOT NULL,
	[state_start] datetime NOT NULL
	)
CREATE CLUSTERED INDEX [CIX_stg_agent_state_loading] ON [stage].[stg_agent_state_loading]
(
	[person_code] ASC
)
*/

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
--	[interval_id] smallint NOT NULL,
	[state_group_id] int NOT NULL,
	[time_in_state_s] int NOT NULL,
	[datasource_id] smallint NOT NULL,
	[insert_date] smalldatetime NOT NULL
)

ALTER TABLE [mart].[fact_agent_state] ADD CONSTRAINT [PK_fact_agent_state] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[person_id] ASC,
--	[interval_id] ASC,
	[state_group_id] ASC
)

--A number table
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_numbers]') AND type in (N'U'))
DROP TABLE [mart].[sys_numbers]
GO
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
--ALTER TABLE [mart].[fact_agent_state]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_state_dim_interval] FOREIGN KEY([interval_id])
--REFERENCES [mart].[dim_interval] ([interval_id])
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
--Desc: New jobsteps for stg_state_group and fact_agent_state
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
/*
un-do
DROP TABLE [mart].[fact_agent_skill]
ALTER TABLE [stage].[stg_agent_skill] DROP COLUMN [Active]
DROP TABLE [mart].[fact_agent_state]
DROP TABLE [mart].[dim_state_group]
DROP TABLE [stage].[stg_state_group]
DROP TABLE [stage].[stg_agent_state_loading]
DROP TABLE [stage].[stg_agent_state]

*/