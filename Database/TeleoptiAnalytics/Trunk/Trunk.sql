
----------------  
--Name: David J
--Date: 2013-09-30
--Desc: PBI23113 - Show execution time for different scheduling options
-----------------
SET NOCOUNT ON
declare @reportId uniqueidentifier
declare @controlId uniqueidentifier

declare @url nvarchar(500)

set @controlId = 'BFD593F0-39BF-403D-ACBF-F6957FF1804C'
set @reportId = 'F7F3AF97-EC24-4EA8-A2C7-5175879C7ACC'
set @url = '~/Selection.aspx?ReportId=' + cast(@reportId as varchar(36))

-- new controls
IF NOT EXISTS (select 1 from mart.report_control where control_id=42)
BEGIN
	INSERT INTO  mart.report_control
	select @controlId,42,'cboSchedulingType','mart.report_control_schedulingtype_get'
END

--ADD REPORT CONTROL COLLECTION
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where CollectionId='D0CC5826-A320-4210-A91C-67710A4DBBEB')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
		SELECT 'EB792517-B698-40A2-B264-B64E68C810E2',mart.report_control.Id,'D0CC5826-A320-4210-A91C-67710A4DBBEB',482,45,1,42,'0','ResSchedulingType',NULL,'@scheduling_type_id',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=42

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
		SELECT 'D2917206-4647-4BC6-BA72-B15348234A3B',mart.report_control.Id,'D0CC5826-A320-4210-A91C-67710A4DBBEB',483,45,2,1,'12:00','ResDateFromColon',NULL,'@date_from',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=1

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
		SELECT '9898193F-90E6-4339-9F9D-09C55560949D',mart.report_control.Id,'D0CC5826-A320-4210-A91C-67710A4DBBEB',484,45,3,2,'12:00','ResDateToColon',NULL,'@date_to',483,NULL,NULL,NULL,'D2917206-4647-4BC6-BA72-B15348234A3B',NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=2

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
		SELECT 'E0D0D8F8-2290-4337-AC95-3D93789BA838',mart.report_control.Id,'D0CC5826-A320-4210-A91C-67710A4DBBEB',485,45,4,11,'4','ResIntervalType',NULL,'@interval_type',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=11

END

--ADD REPORT
IF NOT EXISTS(SELECT 1 FROM mart.Report WHERE Id=@reportId)
BEGIN
	INSERT INTO mart.Report(Id, report_id, control_collection_id, url, target, report_name, report_name_resource_key, visible, rpt_file_name, proc_name, help_key, sub1_name, sub1_proc_name, sub2_name, sub2_proc_name, ControlCollectionId)
	VALUES (@reportId,30,45,@url,'_blank','Scheduling Metrics per Period','ResReportSchedulingMetricsPerPeriod',0,'~/Reports/CCC/report_scheduling_metrics_per_period.rdlc','mart.report_data_scheduling_metrics_per_period','f01_Report_SchedulingMetricsPerPeriod.html','','','','','D0CC5826-A320-4210-A91C-67710A4DBBEB')
END	

GO

----------------  
--Name: David J
--Date: 2013-09-25
--Desc: bug #23113 - Advanced logging
-----------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdvancedLoggingService]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[AdvancedLoggingService](
	[LogDate] [datetime] NOT NULL,
	[Message] [nvarchar](100) NULL,
	[BU] [nvarchar](50) NULL,
	[BUId] [uniqueidentifier] NULL,
	[DataSource] [nvarchar](200) NULL,
	[WindowsIdentity] [nvarchar](200) NULL,
	[HostIP] [varchar](30) NULL,
	[BlockOptions] [nvarchar](500) NULL,
	[TeamOptions] [nvarchar](500) NULL,
	[GeneralOptions] [nvarchar](500) NULL,
	[SkillDays] [int] NULL,
	[Agents] [int] NULL,
	[ExecutionTime] [int] NULL
	) 
	CREATE CLUSTERED INDEX [CIX_AdvancedLoggingService_LogDate_HostIP] ON [dbo].[AdvancedLoggingService]
	(
		[LogDate] ASC,
		[HostIP] ASC
	)
END
GO

----------------  
--Name: David J
--Date: 2013-10-08
--Desc: PBI #24349 - tables and SPs to collect IO data
-----------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DBA_VirtualFileStats]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DBA_VirtualFileStats](
	[database_id] int NOT NULL,
	[file_id] int NOT NULL,
	[ServerName] [varchar](255) NOT NULL,
	[DatabaseName] [varchar](255) NOT NULL,
	[PhysicalName] [varchar](255) NOT NULL,
	[num_of_reads] [bigint] NULL,
	[num_of_reads_from_start] [bigint] NULL,
	[num_of_writes] [bigint] NULL,
	[num_of_writes_from_start] [bigint] NULL,
	[num_of_bytes_read] [bigint] NULL,
	[num_of_bytes_read_from_start] [bigint] NULL,
	[num_of_bytes_written] [bigint] NULL,
	[num_of_bytes_written_from_start] [bigint] NULL,
	[io_stall] [bigint] NULL,
	[io_stall_from_start] [bigint] NULL,
	[io_stall_read_ms] [bigint] NULL,
	[io_stall_read_ms_from_start] [bigint] NULL,
	[io_stall_write_ms] [bigint] NULL,
	[io_stall_write_ms_from_start] [bigint] NULL,
	[RecordedDateTime] [datetime] NULL,
	[interval_ms] [bigint] NULL,
	[FirstMeasureFromStart] [bit] NULL
)
CREATE CLUSTERED INDEX [CIX_DBA_VirtualFileStats_RecordedDateTime] ON [dbo].[DBA_VirtualFileStats]
(
	[RecordedDateTime] ASC
)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DBA_VirtualFileStatsHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DBA_VirtualFileStatsHistory](
	[RecordID] [int] IDENTITY(1,1) NOT NULL,
	[database_id] int NOT NULL,
	[file_id] int NOT NULL,
	[ServerName] [varchar](255) NOT NULL,
	[DatabaseName] [varchar](255) NOT NULL,
	[PhysicalName] [varchar](255) NOT NULL,
	[num_of_reads] [bigint] NULL,
	[num_of_reads_from_start] [bigint] NULL,
	[num_of_writes] [bigint] NULL,
	[num_of_writes_from_start] [bigint] NULL,
	[num_of_bytes_read] [bigint] NULL,
	[num_of_bytes_read_from_start] [bigint] NULL,
	[num_of_bytes_written] [bigint] NULL,
	[num_of_bytes_written_from_start] [bigint] NULL,
	[io_stall] [bigint] NULL,
	[io_stall_from_start] [bigint] NULL,
	[io_stall_read_ms] [bigint] NULL,
	[io_stall_read_ms_from_start] [bigint] NULL,
	[io_stall_write_ms] [bigint] NULL,
	[io_stall_write_ms_from_start] [bigint] NULL,
	[RecordedDateTime] [datetime] NULL,
	[interval_ms] [bigint] NULL,
	[FirstMeasureFromStart] [bit] NULL
)
CREATE CLUSTERED INDEX [CIX_DBA_VirtualFileStatsHistory_RecordedDateTime] ON [dbo].[DBA_VirtualFileStatsHistory]
(
	[RecordedDateTime] ASC
)
END
GO

----------------  
--Name: David Jonsson
--Date: 2013-11-05
--Desc: Bug #25601 - Missing index to support Megamart integration
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_acd_login]') AND name = N'IX_dim_acd_login_time_zone_id')
CREATE NONCLUSTERED INDEX [IX_dim_acd_login_time_zone_id]
ON [mart].[dim_acd_login] ([time_zone_id])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND name = N'IX_dim_person_time_zone_id')
CREATE NONCLUSTERED INDEX [IX_dim_person_time_zone_id]
ON [mart].[dim_person] ([time_zone_id])
GO
----------------------
--- Name: Talha M
--- Ddate: 2013-10-25
--- Desc : PBI  #25059: Add new column Active to stage.stg_agent_skill
----------------------
TRUNCATE TABLE [stage].[stg_agent_skill]
GO
ALTER TABLE [stage].[stg_agent_skill]
ADD [Active] bit NULL
GO

----------------  
--Name: Karin
--Date: 2013-10-30
--Desc: #25058 New Fact Table Agent Skills
-----------------

CREATE TABLE [mart].[fact_agent_skill](
	[person_id] [int] NOT NULL,
	[skill_id] [int] NOT NULL,
	[has_skill] [int] NULL,
	[active] [bit] NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL
 CONSTRAINT [PK_fact_agent_skill] PRIMARY KEY CLUSTERED 
(
	[person_id] ASC,
	[skill_id] ASC
)
)

GO

ALTER TABLE [mart].[fact_agent_skill] ADD  CONSTRAINT [DF_fact_agent_skill_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_agent_skill] ADD  CONSTRAINT [DF_fact_agent_has_skill]  DEFAULT ((1)) FOR [has_skill]
GO

ALTER TABLE [mart].[fact_agent_skill]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_skill_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO

ALTER TABLE [mart].[fact_agent_skill]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_skill_dim_skill] FOREIGN KEY([skill_id])
REFERENCES [mart].[dim_skill] ([skill_id])
GO

----------------  
--Name: Karin
--Date: 2013-10-30
--Desc: #25061 New Report Control collection
-----------------
--ADD NEW CONTROL
IF NOT EXISTS(SELECT 1 FROM mart.report_control where control_id=43)
BEGIN
	INSERT mart.report_control(Id, control_id, control_name, fill_proc_name)
	SELECT NEWID(), 43, 'chkActive','1'
END


--ADD REPORT CONTROL COLLECTION
IF NOT EXISTS(SELECT 1 FROM mart.report_control_collection where CollectionId='AD5D0F25-32A9-4433-AC7F-3B926CE5FFD0')
BEGIN
	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'FD57025C-5818-458C-8999-6571E3F72B68',	mart.report_control.Id,	'AD5D0F25-32A9-4433-AC7F-3B926CE5FFD0',486,46,1,6,'12:00','ResDateColon',NULL,'@date_from',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=6

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'E6818734-D9A9-4A7B-AA80-C4489C4458DF',mart.report_control.Id,	'AD5D0F25-32A9-4433-AC7F-3B926CE5FFD0',487,46,2,29,'-2','ResGroupPageColon',NULL,'@group_page_code',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=29

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'E15DF32C-3BF3-4D25-BEE7-28E4602B9B52',mart.report_control.Id,	'AD5D0F25-32A9-4433-AC7F-3B926CE5FFD0',488,46,3,35,'-99','ResGroupPageGroupColon',NULL,'@group_page_group_set',487,NULL,NULL,	NULL,'E6818734-D9A9-4A7B-AA80-C4489C4458DF',NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=35

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '1D2C9295-E251-4D46-AB34-BFE4847796FC',mart.report_control.Id,	'AD5D0F25-32A9-4433-AC7F-3B926CE5FFD0',489,46,4,37,'-2','ResAgentColon',NULL,'@group_page_agent_code',486,487,488,	NULL,'FD57025C-5818-458C-8999-6571E3F72B68', 'E6818734-D9A9-4A7B-AA80-C4489C4458DF','E15DF32C-3BF3-4D25-BEE7-28E4602B9B52',NULL
	FROM mart.report_control WHERE control_id=37

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '64FE4A56-78AF-4AAE-8EFD-478FB86959E0',mart.report_control.Id,'AD5D0F25-32A9-4433-AC7F-3B926CE5FFD0',490,46,5,3,'-2','ResSiteNameColon',NULL,'@site_id',486,487,NULL,NULL,'FD57025C-5818-458C-8999-6571E3F72B68','E6818734-D9A9-4A7B-AA80-C4489C4458DF',NULL,NULL
	FROM mart.report_control WHERE control_id=3

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'DB97F211-9F62-4C06-933C-C6CE67391446',mart.report_control.Id,'AD5D0F25-32A9-4433-AC7F-3B926CE5FFD0',491,46,6,34,'-99','ResTeamNameColon',NULL,'@team_set',486,487,490,NULL,'FD57025C-5818-458C-8999-6571E3F72B68', 'E6818734-D9A9-4A7B-AA80-C4489C4458DF','64FE4A56-78AF-4AAE-8EFD-478FB86959E0',NULL
	FROM mart.report_control WHERE control_id=34

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'BEB00922-AF40-4816-8AC6-2B1287E6BC98',mart.report_control.Id,'AD5D0F25-32A9-4433-AC7F-3B926CE5FFD0',492,46,7,36,'-2','ResAgentsColon',NULL,'@agent_person_code',486,490,491,NULL,'FD57025C-5818-458C-8999-6571E3F72B68'	,'64FE4A56-78AF-4AAE-8EFD-478FB86959E0','DB97F211-9F62-4C06-933C-C6CE67391446',	NULL
	FROM mart.report_control WHERE control_id=36

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'C306C730-5A4A-4BE7-91D4-632A5F49DF02',mart.report_control.Id,'AD5D0F25-32A9-4433-AC7F-3B926CE5FFD0',493,46,8,15,'-99','ResSkillColon',NULL,'@skill_set',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=15

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT '44452980-96D8-4B67-8D80-8BBCFFF966DD',mart.report_control.Id,'AD5D0F25-32A9-4433-AC7F-3B926CE5FFD0',494,46,9,43,'1','ResOnlyActiveSkillColon',NULL,	'@active',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=43

	INSERT INTO mart.report_control_collection(Id, ControlId, CollectionId, control_collection_id, collection_id, print_order, control_id, default_value, control_name_resource_key, fill_proc_param, param_name, depend_of1, depend_of2, depend_of3, depend_of4, DependOf1, DependOf2, DependOf3, DependOf4)
	SELECT 'CC62C6CC-2816-496F-9345-D02BC84D4204',mart.report_control.Id,'AD5D0F25-32A9-4433-AC7F-3B926CE5FFD0',495,46,10,22,'-95','ResTimeZoneColon',NULL,	'@time_zone_id',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
	FROM mart.report_control WHERE control_id=22
END

----ADD REPORT
DECLARE @newreportid uniqueidentifier
SET  @newreportid= '047B138C-DE3A-426A-99B0-00C5BA826AF2'
DECLARE @CollectionId uniqueidentifier
SELECT DISTINCT @CollectionId = CollectionId FROM mart.report_control_collection WHERE collection_id=46
IF NOT EXISTS(SELECT 1 FROM mart.report where Id='047B138C-DE3A-426A-99B0-00C5BA826AF2')
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
		31,
		46,
		'~/Selection.aspx?ReportId=047B138C-DE3A-426A-99B0-00C5BA826AF2',
		'_blank',
		'Agent Skills',
		'ResReportAgentSkills',
		1,
		'~/Reports/CCC/report_agent_skills.rdlc',
		'mart.report_data_agent_skills',
		'f01:Report+AgentSkills',
		'','','','',
		@CollectionId)
END
GO

----------------  
--Name: KJ
--Date: 2013-11-05
--Desc: New jobstep for mart.fact_agent_skill
----------------
IF NOT EXISTS (SELECT 1 FROM [mart].[etl_jobstep] WHERE jobstep_name=N'fact_agent_skill' AND jobstep_id=83)
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES(83,N'fact_agent_skill')
GO

----------------  
--Name: Karin
--Date: 2013-11-07
--Desc: #24978 New dimension and fact table for state groups and agent time in state
-----------------
CREATE TABLE [mart].[dim_state_group](
	[state_group_id] [int] IDENTITY(1,1) NOT NULL,
	[state_group_code] [uniqueidentifier] NULL,
	[state_group_name] [nvarchar](100) NOT NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[is_deleted] [bit] NOT NULL	
 CONSTRAINT [PK_dim_state_group] PRIMARY KEY CLUSTERED 
(
	[state_group_id] ASC
) )

GO

ALTER TABLE [mart].[dim_state_group] ADD  CONSTRAINT [DF_dim_state_group_state_group_name]  DEFAULT ('Not Defined') FOR [state_group_name]
GO

ALTER TABLE [mart].[dim_state_group] ADD  CONSTRAINT [DF_dim_state_group_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO

ALTER TABLE [mart].[dim_state_group] ADD  CONSTRAINT [DF_dim_state_group_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO

ALTER TABLE [mart].[dim_state_group] ADD  CONSTRAINT [DF_dim_state_group_update_date]  DEFAULT (getdate()) FOR [update_date]
GO

ALTER TABLE [mart].[dim_state_group] ADD  CONSTRAINT [DF_dim_state_group_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO

CREATE TABLE [mart].[fact_agent_state](
	[date_id] [int] NOT NULL,
	[interval_id][smallint] NOT NULL, 
	[person_id] [int] NOT NULL,
	[state_group_id] [int] NOT NULL,
	[time_in_state_s] [int] NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
 CONSTRAINT [PK_fact_agent_state] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[person_id] ASC,
	[state_group_id] ASC
)) 

GO

ALTER TABLE [mart].[fact_agent_state] ADD  CONSTRAINT [DF_fact_agent_state_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_agent_state]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_state_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_agent_state]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_state_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_agent_state]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_state_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
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
	SELECT '475CE887-4C4A-4C44-9D21-7D68DADC77B9',mart.report_control.Id,'F775ED72-5B41-4FEA-87DB-04AD347D4537',500,47,6,3,'-2','ResSiteNameColon',NULL,'@site_id',496,497,NULL,NULL,'0405D0BA-37C2-49BF-8E4F-B18EEB82A8AF', '3BF4D1B4-6DBA-4F9B-B5F5-631D1FAB725D',NULL,NULL
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
SELECT DISTINCT @CollectionId = CollectionId FROM mart.report_control_collection WHERE collection_id=46
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
		'~/Reports/CCC/report_time_in_state_per_agent.rdlc',
		'mart.report_data_time_in_state_per_agent',
		'f01:Report+TimeInStatePerAgent',
		'','','','',
		@CollectionId)
END
GO

