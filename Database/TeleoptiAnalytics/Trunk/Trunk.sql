
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