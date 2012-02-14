SET NOCOUNT ON
GO
CREATE SCHEMA [mart] AUTHORIZATION [dbo]
GO
CREATE SCHEMA [msg] AUTHORIZATION [dbo]
GO
CREATE SCHEMA [RTA] AUTHORIZATION [dbo]
GO
CREATE SCHEMA [stage] AUTHORIZATION [dbo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Receipt](
	[ReceiptId] [uniqueidentifier] NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	[ProcessId] [int] NOT NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Receipt] PRIMARY KEY NONCLUSTERED 
(
	[ReceiptId] ASC
)
)
GO
CREATE CLUSTERED INDEX [IX_Receipt_ChangedDateTime] ON [msg].[Receipt] 
(
	[ChangedDateTime] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Receipt_ChangedBy] ON [msg].[Receipt] 
(
	[ChangedBy] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[pm_user](
	[user_name] [nvarchar](256) NOT NULL,
	[is_windows_logon] [bit] NOT NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL
)
GO
CREATE CLUSTERED INDEX [CIX_pm_user] ON [mart].[pm_user] 
(
	[user_name] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [mart].[report_control](
	[control_id] [int] NOT NULL,
	[control_name] [varchar](50) NULL,
	[fill_proc_name] [varchar](200) NOT NULL,
	[attribute_id] [int] NULL,
 CONSTRAINT [PK_report_control] PRIMARY KEY CLUSTERED 
(
	[control_id] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (1, N'dateFrom', N'1', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (2, N'dateTo', N'1', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (3, N'cboSite', N'mart.report_control_site_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (4, N'cboTeam', N'mart.report_control_team_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (5, N'cboAgent', N'mart.report_control_agent_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (6, N'dateDate', N'1', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (7, N'cboIntervalHourFrom', N'mart.report_control_hour_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (8, N'cboIntervalHourTo', N'mart.report_control_hour_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (9, N'cboSkill', N'mart.report_control_skill_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (10, N'twolistWorkload', N'mart.report_control_workload_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (11, N'cboIntervalType', N'mart.report_control_interval_type_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (12, N'cboIntervalFrom', N'mart.report_control_interval_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (13, N'cboIntervalTo', N'mart.report_control_interval_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (14, N'cboScenario', N'mart.report_control_scenario_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (15, N'twolistSkill', N'mart.report_control_twolist_skill_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (16, N'cboPersonCatType', N'mart.report_control_person_category_type_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (17, N'cboPersonCat', N'mart.report_control_person_category_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (18, N'twolistAgent', N'mart.report_control_twolist_agent_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (19, N'cboAdherenceCalc', N'mart.report_control_adherence_calculation_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (20, N'cboServiceLevelCalc', N'mart.report_control_service_level_calculation_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (21, N'cboSortAdhRep', N'mart.report_control_sort_by_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (22, N'cboTimeZone', N'mart.report_control_time_zone', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (23, N'twolistQueue', N'mart.report_control_twolist_queue_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (24, N'twolistoptActivity', N'mart.report_control_twolist_activity_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (25, N'twolistoptAbsence', N'mart.report_control_twolist_absence_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (26, N'twolistShiftCat', N'mart.report_control_twolist_shift_category_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (27, N'twolistoptDayOff', N'mart.report_control_twolist_day_off_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (28, N'twolistoptShiftCat', N'mart.report_control_twolist_shift_category_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (29, N'cboGroupPage', N'mart.report_control_group_page_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (30, N'cboGroupPageGroup', N'mart.report_control_group_page_group_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (31, N'cboGroupPageAgent', N'mart.report_control_group_page_agent_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (32, N'twolistGroupPageAgent', N'mart.report_control_twolist_group_page_agent_get', NULL)
INSERT [mart].[report_control] ([control_id], [control_name], [fill_proc_name], [attribute_id]) VALUES (33, N'twolistOvertime', N'mart.report_control_twolist_overtime_get', NULL)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [mart].[Reports](
	[Definition] [image] NULL,
	[ReportId] [int] NOT NULL,
	[ReportName] [nchar](150) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[FolderId] [int] NOT NULL,
	[CompanyId] [int] NULL,
	[Inherited] [char](1) NULL,
	[CreateDate] [datetime] NOT NULL,
	[OwnerId] [int] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[ModifiedId] [int] NOT NULL,
 CONSTRAINT [PK_Reports] PRIMARY KEY CLUSTERED 
(
	[ReportId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [mart].[report_user_setting](
	[person_code] [uniqueidentifier] NOT NULL,
	[report_id] [int] NOT NULL,
	[param_name] [varchar](50) NOT NULL,
	[saved_name_id] [int] NOT NULL,
	[control_setting] [varchar](max) NULL,
 CONSTRAINT [PK_report_user_setting] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC,
	[report_id] ASC,
	[param_name] ASC,
	[saved_name_id] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[report_group](
	[group_id] [int] NOT NULL,
	[group_name] [nvarchar](500) NOT NULL,
	[group_name_resource_key] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_report_group] PRIMARY KEY CLUSTERED 
(
	[group_id] ASC
)
)
GO
INSERT [mart].[report_group] ([group_id], [group_name], [group_name_resource_key]) VALUES (1, N'Group 1', N'ResourceGroup1')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Users](
	[UserId] [int] NOT NULL,
	[Domain] [nvarchar](50) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_Users_ChangedBy] ON [msg].[Users] 
(
	[ChangedBy] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Users_ChangedDateTime] ON [msg].[Users] 
(
	[ChangedDateTime] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[UpdateType](
	[UpdateType] [int] NOT NULL,
	[UpdateDescription] [nvarchar](15) NOT NULL,
	[ChangedBy] [nvarchar](1000) NULL,
	[ChangedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_UpdateType] PRIMARY KEY CLUSTERED 
(
	[UpdateType] ASC
)
)
GO
INSERT [msg].[UpdateType] ([UpdateType], [UpdateDescription], [ChangedBy], [ChangedDateTime]) VALUES (0, N'Insert', N'Wise', CAST(0x00009EFD00BF7C91 AS DateTime))
INSERT [msg].[UpdateType] ([UpdateType], [UpdateDescription], [ChangedBy], [ChangedDateTime]) VALUES (1, N'Update', N'Wise', CAST(0x00009EFD00BF7C91 AS DateTime))
INSERT [msg].[UpdateType] ([UpdateType], [UpdateDescription], [ChangedBy], [ChangedDateTime]) VALUES (2, N'Delete', N'Wise', CAST(0x00009EFD00BF7C91 AS DateTime))
INSERT [msg].[UpdateType] ([UpdateType], [UpdateDescription], [ChangedBy], [ChangedDateTime]) VALUES (3, N'NotApplicable', N'Wise', CAST(0x00009EFD00BF7C91 AS DateTime))
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[sys_shift_length_group](
	[shift_length_group_id] [int] NOT NULL,
	[shift_length_group_name] [nvarchar](100) NULL,
	[min_length_m] [int] NULL,
	[max_length_m] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_sys_shift_length_group] PRIMARY KEY CLUSTERED 
(
	[shift_length_group_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[sys_datasource](
	[datasource_id] [smallint] IDENTITY(1,1) NOT NULL,
	[datasource_name] [nvarchar](100) NULL,
	[log_object_id] [int] NULL,
	[log_object_name] [nvarchar](100) NULL,
	[datasource_database_id] [int] NULL,
	[datasource_database_name] [nvarchar](100) NULL,
	[datasource_type_name] [nvarchar](100) NULL,
	[time_zone_id] [int] NULL,
	[inactive] [bit] NOT NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[source_id] [nvarchar](50) NULL,
 CONSTRAINT [PK_sys_datasource] PRIMARY KEY CLUSTERED 
(
	[datasource_id] ASC
)
)
GO
SET IDENTITY_INSERT [mart].[sys_datasource] ON
INSERT [mart].[sys_datasource] ([datasource_id], [datasource_name], [log_object_id], [log_object_name], [datasource_database_id], [datasource_database_name], [datasource_type_name], [time_zone_id], [inactive], [insert_date], [update_date], [source_id]) VALUES (-1, N'Not Defined', -1, N'Not Defined', -1, N'Not Defined', N'Not Defined', NULL, 0, CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime), N'-1')
INSERT [mart].[sys_datasource] ([datasource_id], [datasource_name], [log_object_id], [log_object_name], [datasource_database_id], [datasource_database_name], [datasource_type_name], [time_zone_id], [inactive], [insert_date], [update_date], [source_id]) VALUES (1, N'TeleoptiCCC', -1, N'Not Defined', 1, N'Raptor Default', N'Raptor Default', NULL, 0, CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime), N'-1')
SET IDENTITY_INSERT [mart].[sys_datasource] OFF
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [mart].[sys_crossdatabaseview_target](
	[target_id] [int] NOT NULL,
	[target_customName] [varchar](100) NOT NULL,
	[target_defaultName] [varchar](100) NOT NULL,
	[confirmed] [bit] NOT NULL,
 CONSTRAINT [PK_sys_crossdatabaseview_target] PRIMARY KEY CLUSTERED 
(
	[target_id] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
INSERT [mart].[sys_crossdatabaseview_target] ([target_id], [target_customName], [target_defaultName], [confirmed]) VALUES (4, N'TeleoptiCCCAgg', N'TeleoptiCCCAgg', 0)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[aspnet_applications](
	[ApplicationName] [nvarchar](256) NOT NULL,
	[LoweredApplicationName] [nvarchar](256) NOT NULL,
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](256) NULL,
 CONSTRAINT [PK_aspnet_applications] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC
),
 CONSTRAINT [UQ_aspnet_applications_ApplicationName] UNIQUE NONCLUSTERED 
(
	[ApplicationName] ASC
),
 CONSTRAINT [UQ_aspnet_applications_LoweredApplicationName] UNIQUE NONCLUSTERED 
(
	[LoweredApplicationName] ASC
)
)
GO
INSERT [dbo].[aspnet_applications] ([ApplicationName], [LoweredApplicationName], [ApplicationId], [Description]) VALUES (N'/', N'/', N'196a4451-8580-46bd-807a-1dbf027f970a', NULL)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[adherence_calculation](
	[adherence_id] [int] IDENTITY(1,1) NOT NULL,
	[adherence_name] [nvarchar](100) NULL,
	[resource_key] [nvarchar](500) NULL,
 CONSTRAINT [PK_adherence_calculation] PRIMARY KEY CLUSTERED 
(
	[adherence_id] ASC
)
)
GO
SET IDENTITY_INSERT [mart].[adherence_calculation] ON
INSERT [mart].[adherence_calculation] ([adherence_id], [adherence_name], [resource_key]) VALUES (1, N'Ready Time vs. Scheduled Ready Time', N'ResReadyTimeVsScheduledReadyTime')
INSERT [mart].[adherence_calculation] ([adherence_id], [adherence_name], [resource_key]) VALUES (2, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', N'ResReadyTimeVsScheduledTime')
INSERT [mart].[adherence_calculation] ([adherence_id], [adherence_name], [resource_key]) VALUES (3, N'Ready Time vs. Contracted Schedule Time', N'ResReadyTimeVsContractScheduleTime')
SET IDENTITY_INSERT [mart].[adherence_calculation] OFF
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Address](
	[AddressId] [int] NOT NULL,
	[Address] [nvarchar](255) NOT NULL,
	[Port] [int] NOT NULL,
 CONSTRAINT [PK_Address] PRIMARY KEY CLUSTERED 
(
	[AddressId] ASC
)
)
GO
INSERT [msg].[Address] ([AddressId], [Address], [Port]) VALUES (1, N'235.235.235.234', 9090)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[aspnet_SchemaVersions](
	[Feature] [nvarchar](128) NOT NULL,
	[CompatibleSchemaVersion] [nvarchar](128) NOT NULL,
	[IsCurrentVersion] [bit] NOT NULL,
 CONSTRAINT [PK_aspnet_SchemaVersions] PRIMARY KEY CLUSTERED 
(
	[Feature] ASC,
	[CompatibleSchemaVersion] ASC
)
)
GO
INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'common', N'1', 1)
INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'membership', N'1', 1)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_skillset](
	[skillset_id] [int] IDENTITY(1,1) NOT NULL,
	[skillset_code] [nvarchar](4000) NOT NULL,
	[skillset_name] [nvarchar](4000) NOT NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [datetime] NOT NULL,
 CONSTRAINT [PK_dim_skillset] PRIMARY KEY CLUSTERED 
(
	[skillset_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_skill](
	[skill_id] [int] IDENTITY(1,1) NOT NULL,
	[skill_code] [uniqueidentifier] NULL,
	[skill_name] [nvarchar](100) NOT NULL,
	[time_zone_id] [int] NOT NULL,
	[forecast_method_code] [uniqueidentifier] NULL,
	[forecast_method_name] [nvarchar](100) NOT NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [datetime] NOT NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_dim_skill] PRIMARY KEY CLUSTERED 
(
	[skill_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_shift_length](
	[shift_length_id] [int] IDENTITY(1,1) NOT NULL,
	[shift_length_m] [int] NOT NULL,
	[shift_length_h] [decimal](10, 2) NULL,
	[shift_length_group_id] [int] NULL,
	[shift_length_group_name] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_shift_length] PRIMARY KEY CLUSTERED 
(
	[shift_length_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_shift_category](
	[shift_category_id] [int] IDENTITY(1,1) NOT NULL,
	[shift_category_code] [uniqueidentifier] NULL,
	[shift_category_name] [nvarchar](100) NOT NULL,
	[shift_category_shortname] [nvarchar](50) NOT NULL,
	[display_color] [int] NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_dim_shift_category] PRIMARY KEY CLUSTERED 
(
	[shift_category_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_scorecard](
	[scorecard_id] [int] IDENTITY(1,1) NOT NULL,
	[scorecard_code] [uniqueidentifier] NULL,
	[scorecard_name] [nvarchar](255) NOT NULL,
	[period] [int] NOT NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_scorecard] PRIMARY KEY CLUSTERED 
(
	[scorecard_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_scenario](
	[scenario_id] [smallint] IDENTITY(1,1) NOT NULL,
	[scenario_code] [uniqueidentifier] NULL,
	[scenario_name] [nvarchar](50) NOT NULL,
	[default_scenario] [bit] NULL,
	[business_unit_id] [int] NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_dim_scenario] PRIMARY KEY CLUSTERED 
(
	[scenario_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_queue_excluded](
	[queue_original_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
 CONSTRAINT [PK_dim_queue_excluded] PRIMARY KEY CLUSTERED 
(
	[queue_original_id] ASC,
	[datasource_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_queue](
	[queue_id] [int] IDENTITY(1,1) NOT NULL,
	[queue_agg_id] [int] NULL,
	[queue_original_id] [nvarchar](50) NULL,
	[queue_name] [nvarchar](100) NOT NULL,
	[queue_description] [nvarchar](100) NULL,
	[log_object_name] [nvarchar](100) NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_queue] PRIMARY KEY CLUSTERED 
(
	[queue_id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_aggId_datasource] ON [mart].[dim_queue] 
(
	[queue_agg_id] ASC,
	[datasource_id] ASC
)
INCLUDE ( [queue_id]) 
GO
CREATE NONCLUSTERED INDEX [IX_datasource] ON [mart].[dim_queue] 
(
	[datasource_id] ASC
)
INCLUDE ( [queue_id],
[queue_agg_id]) 
GO
CREATE NONCLUSTERED INDEX [IX_dim_queue] ON [mart].[dim_queue] 
(
	[queue_original_id] ASC,
	[queue_id] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_preference_type](
	[preference_type_id] [int] NOT NULL,
	[preference_type_name] [nvarchar](50) NOT NULL,
	[resource_key] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_dim_preference_type] PRIMARY KEY CLUSTERED 
(
	[preference_type_id] ASC
)
)
GO
INSERT [mart].[dim_preference_type] ([preference_type_id], [preference_type_name], [resource_key]) VALUES (1, N'Shift Category', N'ResShiftCategory')
INSERT [mart].[dim_preference_type] ([preference_type_id], [preference_type_name], [resource_key]) VALUES (2, N'Day Off', N'ResDayOff')
INSERT [mart].[dim_preference_type] ([preference_type_id], [preference_type_name], [resource_key]) VALUES (3, N'Extended', N'ResExtendedPreference')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_person_category_type](
	[person_category_type_id] [int] IDENTITY(1,1) NOT NULL,
	[person_category_type_name] [nvarchar](100) NOT NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_person_category_type] PRIMARY KEY CLUSTERED 
(
	[person_category_type_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_overtime](
	[overtime_id] [int] IDENTITY(1,1) NOT NULL,
	[overtime_code] [uniqueidentifier] NULL,
	[overtime_name] [nvarchar](100) NOT NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_dim_overtime] PRIMARY KEY CLUSTERED 
(
	[overtime_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_kpi](
	[kpi_id] [int] IDENTITY(1,1) NOT NULL,
	[kpi_code] [uniqueidentifier] NOT NULL,
	[kpi_name] [nvarchar](50) NOT NULL,
	[resource_key] [nvarchar](50) NOT NULL,
	[target_value_type] [int] NOT NULL,
	[default_target_value] [float] NOT NULL,
	[default_min_value] [float] NOT NULL,
	[default_max_value] [float] NOT NULL,
	[default_between_color] [int] NOT NULL,
	[default_lower_than_min_color] [int] NOT NULL,
	[default_higher_than_max_color] [int] NOT NULL,
	[decreasing_value_is_positive] [bit] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_kpi] PRIMARY KEY CLUSTERED 
(
	[kpi_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_interval](
	[interval_id] [smallint] NOT NULL,
	[interval_name] [nvarchar](20) NULL,
	[halfhour_name] [nvarchar](50) NULL,
	[hour_name] [nvarchar](50) NULL,
	[interval_start] [smalldatetime] NULL,
	[interval_end] [smalldatetime] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_interval] PRIMARY KEY CLUSTERED 
(
	[interval_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_group_page](
	[group_page_id] [int] IDENTITY(1,1) NOT NULL,
	[group_page_code] [uniqueidentifier] NOT NULL,
	[group_page_name] [nvarchar](100) NULL,
	[group_page_name_resource_key] [nvarchar](100) NULL,
	[group_id] [int] NOT NULL,
	[group_code] [uniqueidentifier] NOT NULL,
	[group_name] [nvarchar](1024) NOT NULL,
	[group_is_custom] [bit] NOT NULL,
	[business_unit_id] [int] NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [int] NOT NULL,
	[insert_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_group_page] PRIMARY KEY CLUSTERED 
(
	[group_page_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_day_off](
	[day_off_id] [int] IDENTITY(1,1) NOT NULL,
	[day_off_code] [uniqueidentifier] NULL,
	[day_off_name] [nvarchar](50) NOT NULL,
	[display_color] [int] NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_day_off] PRIMARY KEY CLUSTERED 
(
	[day_off_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_date](
	[date_id] [int] IDENTITY(1,1) NOT NULL,
	[date_date] [smalldatetime] NOT NULL,
	[year] [int] NOT NULL,
	[year_month] [int] NOT NULL,
	[month] [int] NOT NULL,
	[month_name] [nvarchar](20) NOT NULL,
	[month_resource_key] [nvarchar](100) NULL,
	[day_in_month] [int] NOT NULL,
	[weekday_number] [int] NOT NULL,
	[weekday_name] [nvarchar](20) NOT NULL,
	[weekday_resource_key] [nvarchar](100) NULL,
	[week_number] [int] NOT NULL,
	[year_week] [nvarchar](6) NOT NULL,
	[quarter] [nvarchar](6) NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_dim_date] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_dim_date] ON [mart].[dim_date] 
(
	[date_id] ASC
)
INCLUDE ( [date_date]) 
GO
CREATE NONCLUSTERED INDEX [IX_dim_date_date_date] ON [mart].[dim_date] 
(
	[date_date] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_business_unit](
	[business_unit_id] [int] IDENTITY(1,1) NOT NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_business_unit] PRIMARY KEY CLUSTERED 
(
	[business_unit_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_activity](
	[activity_id] [int] IDENTITY(1,1) NOT NULL,
	[activity_code] [uniqueidentifier] NULL,
	[activity_name] [nvarchar](100) NOT NULL,
	[display_color] [int] NOT NULL,
	[in_ready_time] [bit] NOT NULL,
	[in_ready_time_name] [nvarchar](50) NULL,
	[in_contract_time] [bit] NULL,
	[in_contract_time_name] [nvarchar](50) NULL,
	[in_paid_time] [bit] NULL,
	[in_paid_time_name] [nvarchar](50) NULL,
	[in_work_time] [bit] NULL,
	[in_work_time_name] [nvarchar](50) NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_dim_activity] PRIMARY KEY CLUSTERED 
(
	[activity_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_acd_login](
	[acd_login_id] [int] IDENTITY(1,1) NOT NULL,
	[acd_login_agg_id] [int] NULL,
	[acd_login_original_id] [nvarchar](50) NULL,
	[acd_login_name] [nvarchar](100) NULL,
	[log_object_name] [nvarchar](100) NULL,
	[is_active] [bit] NULL,
	[time_zone_id] [int] NOT NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_acd_login] PRIMARY KEY CLUSTERED 
(
	[acd_login_id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_acd_datasource] ON [mart].[dim_acd_login] 
(
	[acd_login_agg_id] ASC,
	[datasource_id] ASC
)
INCLUDE ( [acd_login_id]) 
GO
CREATE NONCLUSTERED INDEX [IX_datasource] ON [mart].[dim_acd_login] 
(
	[datasource_id] ASC
)
INCLUDE ( [acd_login_id],
[acd_login_agg_id]) 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_absence](
	[absence_id] [int] IDENTITY(1,1) NOT NULL,
	[absence_code] [uniqueidentifier] NULL,
	[absence_name] [nvarchar](100) NOT NULL,
	[display_color] [int] NOT NULL,
	[in_contract_time] [bit] NULL,
	[in_contract_time_name] [nvarchar](50) NULL,
	[in_paid_time] [bit] NULL,
	[in_paid_time_name] [nvarchar](50) NULL,
	[in_work_time] [bit] NULL,
	[in_work_time_name] [nvarchar](50) NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_dim_absence] PRIMARY KEY CLUSTERED 
(
	[absence_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Configuration](
	[ConfigurationId] [int] NOT NULL,
	[ConfigurationType] [nvarchar](50) NOT NULL,
	[ConfigurationName] [nvarchar](50) NOT NULL,
	[ConfigurationValue] [nvarchar](255) NOT NULL,
	[ConfigurationDataType] [nvarchar](50) NOT NULL,
	[ChangedBy] [nvarchar](1000) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Configuration] PRIMARY KEY CLUSTERED 
(
	[ConfigurationId] ASC
)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Configuration_ConfigurationName] ON [msg].[Configuration] 
(
	[ConfigurationName] ASC
)
GO
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (1, N'TeleoptiBrokerService', N'Port', N'9080', N'System.Int32', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C1A AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (2, N'TeleoptiBrokerService', N'Server', N'127.0.0.1', N'System.String', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C1A AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (3, N'TeleoptiBrokerService', N'Threads', N'1', N'System.Int32', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C1A AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (4, N'TeleoptiBrokerService', N'Intervall', N'60000', N'System.Double', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C1A AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (6, N'TeleoptiBrokerService', N'GeneralThreadPoolThreads', N'3', N'System.Int32', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C1A AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (7, N'TeleoptiBrokerService', N'DatabaseThreadPoolThreads', N'3', N'System.Int32', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C1A AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (8, N'TeleoptiBrokerService', N'ReceiptThreadPoolThreads', N'3', N'System.Int32', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C1B AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (9, N'TeleoptiBrokerService', N'HeartbeatThreadPoolThreads', N'1', N'System.Int32', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C1B AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (10, N'TeleoptiBrokerService', N'RestartTime', N'180000', N'System.Int32', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C75 AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (11, N'TeleoptiBrokerService', N'ClientThrottle', N'5000', N'System.Int32', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C75 AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (12, N'TeleoptiBrokerService', N'ServerThrottle', N'5', N'System.Int32', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C75 AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (14, N'TeleoptiBrokerService', N'MessagingProtocol', N'ClientTcpIp', N'System.String', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C75 AS DateTime))
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime]) VALUES (15, N'TeleoptiBrokerService', N'TimeToLive', N'1', N'System.Int32', N'TOPTINET\davidj', CAST(0x00009EFD00BF7C75 AS DateTime))
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [mart].[etl_job](
	[job_id] [int] NOT NULL,
	[job_name] [varchar](50) NOT NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_etl_job] PRIMARY KEY CLUSTERED 
(
	[job_id] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (1, N'Initial', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (2, N'Schedule', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (3, N'Queue Statistics', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (4, N'Forecast', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (5, N'Agent Statistics', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (6, N'KPI', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (7, N'Permission', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (8, N'Person Skill', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (9, N'Intraday', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (10, N'Workload Queues', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (11, N'Nightly', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (12, N'Queue and Agent login synchronization', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (13, N'Cleanup', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_job] ([job_id], [job_name], [insert_date], [update_date]) VALUES (14, N'Process Cube', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_workload](
	[workload_id] [int] IDENTITY(1,1) NOT NULL,
	[workload_code] [uniqueidentifier] NULL,
	[workload_name] [nvarchar](100) NOT NULL,
	[skill_id] [int] NULL,
	[skill_code] [uniqueidentifier] NULL,
	[skill_name] [nvarchar](100) NOT NULL,
	[time_zone_id] [int] NOT NULL,
	[forecast_method_code] [uniqueidentifier] NULL,
	[forecast_method_name] [nvarchar](100) NOT NULL,
	[percentage_offered] [float] NOT NULL,
	[percentage_overflow_in] [float] NOT NULL,
	[percentage_overflow_out] [float] NOT NULL,
	[percentage_abandoned] [float] NOT NULL,
	[percentage_abandoned_short] [float] NOT NULL,
	[percentage_abandoned_within_service_level] [float] NOT NULL,
	[percentage_abandoned_after_service_level] [float] NOT NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [datetime] NOT NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_dim_workload] PRIMARY KEY CLUSTERED 
(
	[workload_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_time_zone](
	[time_zone_id] [smallint] IDENTITY(1,1) NOT NULL,
	[time_zone_code] [nvarchar](50) NOT NULL,
	[time_zone_name] [nvarchar](100) NOT NULL,
	[default_zone] [bit] NULL,
	[utc_conversion] [int] NULL,
	[utc_conversion_dst] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_time_zone] PRIMARY KEY CLUSTERED 
(
	[time_zone_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[etl_job_schedule](
	[schedule_id] [int] IDENTITY(1,1) NOT NULL,
	[schedule_name] [nvarchar](150) NULL,
	[enabled] [bit] NOT NULL,
	[schedule_type] [int] NOT NULL,
	[occurs_daily_at] [int] NOT NULL,
	[occurs_every_minute] [int] NOT NULL,
	[recurring_starttime] [int] NOT NULL,
	[recurring_endtime] [int] NOT NULL,
	[etl_job_name] [nvarchar](150) NOT NULL,
	[etl_relative_period_start] [int] NULL,
	[etl_relative_period_end] [int] NULL,
	[etl_datasource_id] [int] NULL,
	[description] [nvarchar](500) NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_etl_job_schedule] PRIMARY KEY CLUSTERED 
(
	[schedule_id] ASC
)
)
GO
SET IDENTITY_INSERT [mart].[etl_job_schedule] ON
INSERT [mart].[etl_job_schedule] ([schedule_id], [schedule_name], [enabled], [schedule_type], [occurs_daily_at], [occurs_every_minute], [recurring_starttime], [recurring_endtime], [etl_job_name], [etl_relative_period_start], [etl_relative_period_end], [etl_datasource_id], [description], [insert_date], [update_date]) VALUES (-1, N'Manual', 1, 0, 0, 0, 0, 0, N'Not Defined', NULL, NULL, -1, NULL, CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
SET IDENTITY_INSERT [mart].[etl_job_schedule] OFF
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[etl_jobstep_error](
	[jobstep_error_id] [int] IDENTITY(1,1) NOT NULL,
	[error_exception_message] [text] NULL,
	[error_exception_stacktrace] [text] NULL,
	[inner_error_exception_message] [text] NULL,
	[inner_error_exception_stacktrace] [text] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_etl_jobstep_error] PRIMARY KEY CLUSTERED 
(
	[jobstep_error_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[etl_jobstep](
	[jobstep_id] [int] NOT NULL,
	[jobstep_name] [nvarchar](200) NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_etl_jobstep] PRIMARY KEY CLUSTERED 
(
	[jobstep_id] ASC
)
)
GO
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (1, N'dim_interval', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (2, N'stg_date', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (3, N'dim_date', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (4, N'stg_time_zone', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (5, N'dim_time_zone', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (6, N'stg_time_zone_bridge', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (7, N'bridge_time_zone', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (8, N'stg_business_unit', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (9, N'dim_queue', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (10, N'dim_acd_login', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (11, N'TeleoptiCCC7.QueueSource', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (12, N'TeleoptiCCC7.ExternalLogOn', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (13, N'stg_person', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (14, N'stg_agent_skill', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (15, N'stg_activity', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (16, N'stg_absence', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (17, N'stg_scenario', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (18, N'stg_shift_category', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (19, N'stg_schedule, stg_schedule_day_absence_count', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (20, N'stg_schedule_forecast_skill', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (21, N'stg_schedule_day_off_count', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (22, N'stg_workload, stg_queue_workload', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (23, N'stg_forecast_workload', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (24, N'stg_kpi', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (25, N'stg_scorecard', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (26, N'stg_scorecard_kpi', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (27, N'stg_kpi_targets_team', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (28, N'stg_permission', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (29, N'stg_user', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (30, N'dim_business_unit', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (31, N'dim_site', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (32, N'dim_team', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (33, N'dim_skill', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (34, N'dim_skillset', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (35, N'dim_person', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (36, N'dim_activity', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (37, N'dim_absence', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (38, N'dim_scenario', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (39, N'dim_shift_category', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (40, N'dim_shift_length', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (41, N'dim_day_off', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (42, N'dim_workload', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (43, N'dim_kpi', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (44, N'dim_scorecard', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (45, N'scorecard_kpi', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (46, N'bridge_skillset_skill', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (47, N'bridge_acd_login_person', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (48, N'bridge_queue_workload', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (49, N'aspnet_Users, aspnet_Membership', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (50, N'fact_schedule', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (51, N'fact_schedule_day_count', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (52, N'fact_schedule_forecast_skill', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (53, N'fact_queue', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (54, N'fact_forecast_workload', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (55, N'fact_agent', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (56, N'fact_agent_queue', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (57, N'fact_schedule_deviation', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (58, N'fact_kpi_targets_team', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (59, N'permission_report', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (60, N'dim_person delete data', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (61, N'dim_person trim data', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (62, N'dim_scenario delete data', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (63, N'Performance Manager permissions', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (64, N'Process Cube', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (65, N'stg_skill', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (66, N'stg_schedule_preference', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (67, N'stg_group_page_person', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (68, N'stg_overtime', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (69, N'dim_group_page', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (70, N'dim_overtime', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (71, N'fact_schedule_preference', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (72, N'Performance Manager user check', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (73, N'Maintenance', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name], [insert_date], [update_date]) VALUES (74, N'bridge_group_page_person', CAST(0x9EFD02B9 AS SmallDateTime), CAST(0x9EFD02B9 AS SmallDateTime))
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[period_type](
	[period_type_id] [int] IDENTITY(1,1) NOT NULL,
	[period_type_name] [nvarchar](100) NULL,
	[resource_key] [nvarchar](500) NULL,
 CONSTRAINT [PK_period_type] PRIMARY KEY CLUSTERED 
(
	[period_type_id] ASC
)
)
GO
SET IDENTITY_INSERT [mart].[period_type] ON
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [resource_key]) VALUES (1, N'Interval', N'ResInterval')
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [resource_key]) VALUES (2, N'Half hour', N'ResHalfHour')
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [resource_key]) VALUES (3, N'Hour', N'ResHour')
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [resource_key]) VALUES (4, N'Day', N'ResDay')
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [resource_key]) VALUES (5, N'Week', N'ResWeek')
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [resource_key]) VALUES (6, N'Month', N'ResMonth')
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [resource_key]) VALUES (7, N'Weekday', N'ResWeekday')
SET IDENTITY_INSERT [mart].[period_type] OFF
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Pending](
	[SubscriberId] [uniqueidentifier] NOT NULL
)
GO
CREATE CLUSTERED INDEX [IX_SubscriberId] ON [msg].[Pending] 
(
	[SubscriberId] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Log](
	[LogId] [uniqueidentifier] NOT NULL,
	[ProcessId] [int] NOT NULL,
	[Description] [text] NOT NULL,
	[Exception] [text] NOT NULL,
	[Message] [text] NOT NULL,
	[StackTrace] [text] NOT NULL,
	[ChangedBy] [text] NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Log] PRIMARY KEY NONCLUSTERED 
(
	[LogId] ASC
)
)
GO
CREATE CLUSTERED INDEX [IX_Log_ChangedDateTime] ON [msg].[Log] 
(
	[ChangedDateTime] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [mart].[language_translation](
	[Culture] [varchar](10) NOT NULL,
	[language_id] [int] NOT NULL,
	[ResourceKey] [varchar](500) NOT NULL,
	[term_language] [nvarchar](1000) NOT NULL,
	[term_english] [nvarchar](1000) NULL,
 CONSTRAINT [PK_language_translation] PRIMARY KEY CLUSTERED 
(
	[Culture] ASC,
	[ResourceKey] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Heartbeat](
	[HeartbeatId] [uniqueidentifier] NOT NULL,
	[SubscriberId] [uniqueidentifier] NOT NULL,
	[ProcessId] [int] NOT NULL,
	[ChangedBy] [nvarchar](50) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Heartbeat] PRIMARY KEY NONCLUSTERED 
(
	[HeartbeatId] ASC
)
)
GO
CREATE CLUSTERED INDEX [IX_ChangeDateTime] ON [msg].[Heartbeat] 
(
	[ChangedDateTime] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Heartbeat_ChangedBy] ON [msg].[Heartbeat] 
(
	[ChangedBy] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Heartbeat_SubscriberId] ON [msg].[Heartbeat] 
(
	[SubscriberId] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [mart].[Folders](
	[FolderId] [int] NOT NULL,
	[FolderName] [nchar](150) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[ParentFolderId] [int] NULL,
	[CompanyId] [int] NULL,
	[Inherited] [char](1) NULL,
	[CreateDate] [datetime] NOT NULL,
	[OwnerId] [int] NOT NULL,
	[LastModifiedDate] [datetime] NOT NULL,
	[ModifiedId] [int] NOT NULL,
 CONSTRAINT [PK_Folders] PRIMARY KEY CLUSTERED 
(
	[FolderId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Filter](
	[FilterId] [uniqueidentifier] NOT NULL,
	[SubscriberId] [uniqueidentifier] NOT NULL,
	[ReferenceObjectId] [uniqueidentifier] NOT NULL,
	[ReferenceObjectType] [nvarchar](255) NOT NULL,
	[DomainObjectId] [uniqueidentifier] NOT NULL,
	[DomainObjectType] [nvarchar](255) NOT NULL,
	[EventStartDate] [datetime] NOT NULL,
	[EventEndDate] [datetime] NOT NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Filter] PRIMARY KEY CLUSTERED 
(
	[FilterId] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [RTA].[ExternalAgentState](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[LogOn] [nvarchar](50) NOT NULL,
	[StateCode] [nvarchar](50) NOT NULL,
	[TimeInState] [bigint] NOT NULL,
	[TimestampValue] [datetime] NOT NULL,
	[PlatformTypeId] [uniqueidentifier] NULL,
	[DataSourceId] [int] NULL,
	[BatchId] [datetime] NULL,
	[IsSnapshot] [bit] NOT NULL
)
GO
CREATE CLUSTERED INDEX [IXC_ExternalAgentState_LogOn_Timestamp] ON [RTA].[ExternalAgentState] 
(
	[LogOn] ASC,
	[TimestampValue] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [msg].[Event](
	[EventId] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
	[ProcessId] [int] NOT NULL,
	[ModuleId] [uniqueidentifier] NOT NULL,
	[PackageSize] [int] NOT NULL,
	[IsHeartbeat] [bit] NOT NULL,
	[ReferenceObjectId] [uniqueidentifier] NOT NULL,
	[ReferenceObjectType] [nvarchar](255) NOT NULL,
	[DomainObjectId] [uniqueidentifier] NOT NULL,
	[DomainObjectType] [nvarchar](255) NOT NULL,
	[DomainUpdateType] [int] NOT NULL,
	[DomainObject] [varbinary](1024) NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED 
(
	[EventId] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[etl_maintenance_configuration](
	[configuration_id] [smallint] NOT NULL,
	[configuration_name] [nvarchar](50) NOT NULL,
	[configuration_value] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_etl_maintenance_configuration] PRIMARY KEY CLUSTERED 
(
	[configuration_id] ASC
)
)
GO
INSERT [mart].[etl_maintenance_configuration] ([configuration_id], [configuration_name], [configuration_value]) VALUES (1, N'daysToKeepETLError', N'60')
INSERT [mart].[etl_maintenance_configuration] ([configuration_id], [configuration_name], [configuration_value]) VALUES (2, N'daysToKeepETLExecution', N'45')
INSERT [mart].[etl_maintenance_configuration] ([configuration_id], [configuration_name], [configuration_value]) VALUES (3, N'daysToKeepRTAEvents', N'2')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Subscriber](
	[SubscriberId] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[ProcessId] [int] NOT NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL,
	[IpAddress] [nvarchar](15) NOT NULL,
	[Port] [int] NOT NULL,
 CONSTRAINT [PK_Subscriber] PRIMARY KEY NONCLUSTERED 
(
	[SubscriberId] ASC
)
)
GO
CREATE CLUSTERED INDEX [IX_Subscriber_ChangedDateTime] ON [msg].[Subscriber] 
(
	[ChangedDateTime] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_Subscriber_ChangedBy] ON [msg].[Subscriber] 
(
	[ChangedBy] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_workload](
	[workload_code] [uniqueidentifier] NOT NULL,
	[workload_name] [nvarchar](100) NOT NULL,
	[skill_code] [uniqueidentifier] NULL,
	[skill_name] [nvarchar](100) NOT NULL,
	[time_zone_code] [nvarchar](50) NOT NULL,
	[forecast_method_code] [uniqueidentifier] NULL,
	[forecast_method_name] [nvarchar](100) NOT NULL,
	[percentage_offered] [float] NOT NULL,
	[percentage_overflow_in] [float] NOT NULL,
	[percentage_overflow_out] [float] NOT NULL,
	[percentage_abandoned] [float] NOT NULL,
	[percentage_abandoned_short] [float] NOT NULL,
	[percentage_abandoned_within_service_level] [float] NOT NULL,
	[percentage_abandoned_after_service_level] [float] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [datetime] NOT NULL,
	[skill_is_deleted] [bit] NOT NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_stg_workload] PRIMARY KEY CLUSTERED 
(
	[workload_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_user](
	[person_code] [uniqueidentifier] NOT NULL,
	[person_first_name] [nvarchar](25) NOT NULL,
	[person_last_name] [nvarchar](25) NOT NULL,
	[application_logon_name] [nvarchar](50) NULL,
	[windows_logon_name] [nvarchar](50) NULL,
	[windows_domain_name] [nvarchar](50) NULL,
	[password] [nvarchar](50) NULL,
	[email] [nvarchar](200) NULL,
	[language_id] [int] NULL,
	[language_name] [nvarchar](50) NULL,
	[culture] [nvarchar](50) NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_stg_user] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_time_zone_bridge](
	[date] [smalldatetime] NOT NULL,
	[interval_id] [int] NOT NULL,
	[time_zone_code] [nvarchar](50) NOT NULL,
	[local_date] [smalldatetime] NULL,
	[local_interval_id] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_time_zone_bridge] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[interval_id] ASC,
	[time_zone_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_time_zone](
	[time_zone_code] [nvarchar](50) NOT NULL,
	[time_zone_name] [nvarchar](100) NOT NULL,
	[default_zone] [bit] NULL,
	[utc_conversion] [int] NULL,
	[utc_conversion_dst] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_time_zone] PRIMARY KEY CLUSTERED 
(
	[time_zone_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_skill](
	[skill_code] [uniqueidentifier] NOT NULL,
	[skill_name] [nvarchar](100) NOT NULL,
	[time_zone_code] [nvarchar](50) NOT NULL,
	[forecast_method_code] [uniqueidentifier] NULL,
	[forecast_method_name] [nvarchar](100) NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [datetime] NOT NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_stg_skill] PRIMARY KEY CLUSTERED 
(
	[skill_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_shift_category](
	[shift_category_code] [uniqueidentifier] NOT NULL,
	[shift_category_name] [nvarchar](100) NOT NULL,
	[shift_category_shortname] [nvarchar](50) NOT NULL,
	[display_color] [int] NOT NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](100) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_stg_shift_category] PRIMARY KEY CLUSTERED 
(
	[shift_category_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_scorecard_kpi](
	[scorecard_code] [uniqueidentifier] NOT NULL,
	[kpi_code] [uniqueidentifier] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_scorecard_kpi] PRIMARY KEY CLUSTERED 
(
	[scorecard_code] ASC,
	[kpi_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_scorecard](
	[scorecard_code] [uniqueidentifier] NOT NULL,
	[scorecard_name] [nvarchar](255) NOT NULL,
	[period] [int] NOT NULL,
	[period_name] [nvarchar](50) NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_scorecard] PRIMARY KEY CLUSTERED 
(
	[scorecard_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_schedule_preference] PRIMARY KEY CLUSTERED 
(
	[person_restriction_code] ASC,
	[scenario_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_schedule_forecast_skill](
	[date] [smalldatetime] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[skill_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[forecasted_resources_m] [decimal](28, 4) NULL,
	[forecasted_resources] [decimal](28, 4) NULL,
	[forecasted_resources_incl_shrinkage_m] [decimal](28, 4) NULL,
	[forecasted_resources_incl_shrinkage] [decimal](28, 4) NULL,
	[scheduled_resources_m] [decimal](28, 4) NULL,
	[scheduled_resources] [decimal](28, 4) NULL,
	[scheduled_resources_incl_shrinkage_m] [decimal](28, 4) NULL,
	[scheduled_resources_incl_shrinkage] [decimal](28, 4) NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_schedule_forecast_skill] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[interval_id] ASC,
	[skill_code] ASC,
	[scenario_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_schedule_day_off_count](
	[date] [smalldatetime] NOT NULL,
	[start_interval_id] [smallint] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[starttime] [smalldatetime] NULL,
	[day_off_code] [uniqueidentifier] NULL,
	[day_off_name] [nvarchar](50) NOT NULL,
	[day_count] [int] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_schedule_day_off_count] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[start_interval_id] ASC,
	[person_code] ASC,
	[scenario_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_schedule_day_absence_count](
	[date] [smalldatetime] NOT NULL,
	[start_interval_id] [smallint] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[starttime] [smalldatetime] NULL,
	[absence_code] [uniqueidentifier] NOT NULL,
	[day_count] [int] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_schedule_day_absence_count] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[start_interval_id] ASC,
	[person_code] ASC,
	[scenario_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_schedule](
	[schedule_date] [datetime] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[interval_id] [int] NOT NULL,
	[activity_start] [smalldatetime] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[activity_code] [uniqueidentifier] NULL,
	[absence_code] [uniqueidentifier] NULL,
	[activity_end] [smalldatetime] NOT NULL,
	[shift_start] [smalldatetime] NOT NULL,
	[shift_end] [smalldatetime] NOT NULL,
	[shift_startinterval_id] [int] NOT NULL,
	[shift_category_code] [uniqueidentifier] NULL,
	[shift_length_m] [int] NOT NULL,
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
	[last_publish] [smalldatetime] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
	[overtime_code] [uniqueidentifier] NULL,
 CONSTRAINT [PK_stg_schedule] PRIMARY KEY CLUSTERED 
(
	[schedule_date] ASC,
	[person_code] ASC,
	[interval_id] ASC,
	[activity_start] ASC,
	[scenario_code] ASC,
	[shift_start] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_scenario](
	[scenario_code] [uniqueidentifier] NOT NULL,
	[scenario_name] [nvarchar](50) NOT NULL,
	[default_scenario] [bit] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_stg_scenario] PRIMARY KEY CLUSTERED 
(
	[scenario_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [stage].[stg_queue_workload](
	[queue_code] [nvarchar](50) NOT NULL,
	[workload_code] [uniqueidentifier] NOT NULL,
	[log_object_data_source_id] [int] NOT NULL,
	[log_object_name] [varchar](50) NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [int] NOT NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_queue_workload] PRIMARY KEY CLUSTERED 
(
	[queue_code] ASC,
	[workload_code] ASC,
	[log_object_data_source_id] ASC,
	[log_object_name] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_queue](
	[date] [datetime] NOT NULL,
	[interval] [nvarchar](50) NOT NULL,
	[queue_code] [int] NULL,
	[queue_name] [nvarchar](50) NOT NULL,
	[offd_direct_call_cnt] [int] NULL,
	[overflow_in_call_cnt] [int] NULL,
	[aband_call_cnt] [int] NULL,
	[overflow_out_call_cnt] [int] NULL,
	[answ_call_cnt] [int] NULL,
	[queued_and_answ_call_dur] [int] NULL,
	[queued_and_aband_call_dur] [int] NULL,
	[talking_call_dur] [int] NULL,
	[wrap_up_dur] [int] NULL,
	[queued_answ_longest_que_dur] [int] NULL,
	[queued_aband_longest_que_dur] [int] NULL,
	[avg_avail_member_cnt] [int] NULL,
	[ans_servicelevel_cnt] [int] NULL,
	[wait_dur] [int] NULL,
	[aband_short_call_cnt] [int] NULL,
	[aband_within_sl_cnt] [int] NULL
)
GO
CREATE CLUSTERED INDEX [CIX_stg_queue_date] ON [stage].[stg_queue] 
(
	[date] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_person](
	[person_code] [uniqueidentifier] NOT NULL,
	[valid_from_date] [smalldatetime] NOT NULL,
	[valid_to_date] [smalldatetime] NOT NULL,
	[valid_from_interval_id] [int] NOT NULL,
	[valid_to_interval_id] [int] NOT NULL,
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
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_stg_person] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC,
	[valid_from_date] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_permission_report](
	[person_code] [uniqueidentifier] NULL,
	[report_id] [int] NULL,
	[team_id] [uniqueidentifier] NULL,
	[my_own] [bit] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL
)
GO
CREATE CLUSTERED INDEX [CIX_stg_permisssion_report] ON [stage].[stg_permission_report] 
(
	[person_code] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_overtime](
	[overtime_code] [uniqueidentifier] NULL,
	[overtime_name] [nvarchar](100) NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[is_deleted] [bit] NOT NULL
)
GO
CREATE CLUSTERED INDEX [CIX_stg_overtime] ON [stage].[stg_overtime] 
(
	[overtime_code] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_kpi_targets_team](
	[kpi_code] [uniqueidentifier] NOT NULL,
	[team_code] [uniqueidentifier] NOT NULL,
	[target_value] [float] NOT NULL,
	[min_value] [float] NOT NULL,
	[max_value] [float] NOT NULL,
	[between_color] [int] NOT NULL,
	[lower_than_min_color] [int] NOT NULL,
	[higher_than_max_color] [int] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_kpi_targets_team] PRIMARY KEY CLUSTERED 
(
	[kpi_code] ASC,
	[team_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_kpi](
	[kpi_code] [uniqueidentifier] NOT NULL,
	[kpi_name] [nvarchar](50) NOT NULL,
	[resource_key] [nvarchar](50) NOT NULL,
	[target_value_type] [int] NOT NULL,
	[default_target_value] [float] NOT NULL,
	[default_min_value] [float] NOT NULL,
	[default_max_value] [float] NOT NULL,
	[default_between_color] [int] NOT NULL,
	[default_lower_than_min_color] [int] NOT NULL,
	[default_higher_than_max_color] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_kpi] PRIMARY KEY CLUSTERED 
(
	[kpi_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_group_page_person](
	[group_page_code] [uniqueidentifier] NOT NULL,
	[group_page_name] [nvarchar](100) NULL,
	[group_page_name_resource_key] [nvarchar](100) NULL,
	[group_code] [uniqueidentifier] NOT NULL,
	[group_name] [nvarchar](1024) NOT NULL,
	[group_is_custom] [bit] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [int] NOT NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_group_page_person] PRIMARY KEY CLUSTERED 
(
	[group_page_code] ASC,
	[group_code] ASC,
	[person_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_forecast_workload](
	[date] [smalldatetime] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[start_time] [smalldatetime] NOT NULL,
	[workload_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[end_time] [smalldatetime] NOT NULL,
	[skill_code] [uniqueidentifier] NULL,
	[forecasted_calls] [decimal](28, 4) NULL,
	[forecasted_emails] [decimal](28, 4) NULL,
	[forecasted_backoffice_tasks] [decimal](28, 4) NULL,
	[forecasted_campaign_calls] [decimal](28, 4) NULL,
	[forecasted_calls_excl_campaign] [decimal](28, 4) NULL,
	[forecasted_talk_time_s] [decimal](28, 4) NULL,
	[forecasted_campaign_talk_time_s] [decimal](28, 4) NULL,
	[forecasted_talk_time_excl_campaign_s] [decimal](28, 4) NULL,
	[forecasted_after_call_work_s] [decimal](28, 4) NULL,
	[forecasted_campaign_after_call_work_s] [decimal](28, 4) NULL,
	[forecasted_after_call_work_excl_campaign_s] [decimal](28, 4) NULL,
	[forecasted_handling_time_s] [decimal](28, 4) NULL,
	[forecasted_campaign_handling_time_s] [decimal](28, 4) NULL,
	[forecasted_handling_time_excl_campaign_s] [decimal](28, 4) NULL,
	[period_length_min] [decimal](28, 4) NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](50) NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_forecast_workload] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[interval_id] ASC,
	[start_time] ASC,
	[workload_code] ASC,
	[scenario_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_day_off](
	[day_off_code] [uniqueidentifier] NULL,
	[day_off_name] [nvarchar](50) NOT NULL,
	[display_color] [int] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_day_off] PRIMARY KEY CLUSTERED 
(
	[day_off_name] ASC,
	[business_unit_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_date](
	[date_date] [smalldatetime] NOT NULL,
	[year] [int] NOT NULL,
	[year_month] [int] NOT NULL,
	[month] [int] NOT NULL,
	[month_name] [nvarchar](20) NOT NULL,
	[month_resource_key] [nvarchar](100) NULL,
	[day_in_month] [int] NOT NULL,
	[weekday_number] [int] NOT NULL,
	[weekday_name] [nvarchar](20) NOT NULL,
	[weekday_resource_key] [nvarchar](100) NULL,
	[week_number] [int] NOT NULL,
	[year_week] [nvarchar](6) NOT NULL,
	[quarter] [nvarchar](6) NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_stg_date] PRIMARY KEY CLUSTERED 
(
	[date_date] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_business_unit](
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_business_unit] PRIMARY KEY CLUSTERED 
(
	[business_unit_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_agent_skillset](
	[person_code] [uniqueidentifier] NOT NULL,
	[skill_id] [int] NOT NULL,
	[date_from] [datetime] NOT NULL,
	[skillset_id] [int] NOT NULL,
	[date_to] [datetime] NULL,
	[skill_name] [nvarchar](100) NULL,
	[skill_sum_code] [nvarchar](4000) NULL,
	[skill_sum_name] [nvarchar](4000) NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [datetime] NOT NULL,
 CONSTRAINT [PK_stg_agent_skillset] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC,
	[skill_id] ASC,
	[date_from] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_agent_skill](
	[skill_date] [datetime] NOT NULL,
	[interval_id] [int] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[skill_code] [uniqueidentifier] NOT NULL,
	[date_from] [datetime] NULL,
	[date_to] [datetime] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_agent_skill] PRIMARY KEY CLUSTERED 
(
	[skill_date] ASC,
	[interval_id] ASC,
	[person_code] ASC,
	[skill_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_activity](
	[activity_code] [uniqueidentifier] NOT NULL,
	[activity_name] [nvarchar](50) NOT NULL,
	[display_color] [int] NOT NULL,
	[in_ready_time] [bit] NOT NULL,
	[in_contract_time] [bit] NULL,
	[in_paid_time] [bit] NULL,
	[in_work_time] [bit] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_stg_activity] PRIMARY KEY CLUSTERED 
(
	[activity_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_acd_login_person](
	[acd_login_code] [nvarchar](50) NULL,
	[person_code] [uniqueidentifier] NULL,
	[start_date] [smalldatetime] NULL,
	[end_date] [smalldatetime] NULL,
	[person_period_code] [uniqueidentifier] NULL,
	[log_object_datasource_id] [int] NULL,
	[log_object_name] [nvarchar](50) NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL
)
GO
CREATE CLUSTERED INDEX [CIX_stg_acd_login_person] ON [stage].[stg_acd_login_person] 
(
	[acd_login_code] ASC
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [stage].[stg_absence](
	[absence_code] [uniqueidentifier] NOT NULL,
	[absence_name] [nvarchar](100) NOT NULL,
	[display_color] [int] NOT NULL,
	[in_contract_time] [bit] NULL,
	[in_paid_time] [bit] NULL,
	[in_work_time] [bit] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[is_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_stg_absence] PRIMARY KEY CLUSTERED 
(
	[absence_code] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[service_level_calculation](
	[service_level_id] [int] IDENTITY(1,1) NOT NULL,
	[service_level_name] [nvarchar](100) NULL,
	[resource_key] [nvarchar](500) NULL,
 CONSTRAINT [PK_service_level_calculation] PRIMARY KEY CLUSTERED 
(
	[service_level_id] ASC
)
)
GO
SET IDENTITY_INSERT [mart].[service_level_calculation] ON
INSERT [mart].[service_level_calculation] ([service_level_id], [service_level_name], [resource_key]) VALUES (1, N'Answered Calls Within Service Level Threshold /Offered Calls ', N'ResAnsweredCallsWithinSLPerOfferedCalls')
INSERT [mart].[service_level_calculation] ([service_level_id], [service_level_name], [resource_key]) VALUES (2, N'Answered and Abandoned Calls Within Service Level Threshold /Offered Calls', N'ResAnsweredAndAbndCallsWithinSLPerOfferedCalls')
INSERT [mart].[service_level_calculation] ([service_level_id], [service_level_name], [resource_key]) VALUES (3, N'Answered Calls Within Service Level Threshold / Answered Calls', N'ResAnswCallsWithinSLPerAnswCalls')
SET IDENTITY_INSERT [mart].[service_level_calculation] OFF
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[scorecard_kpi](
	[scorecard_id] [int] NOT NULL,
	[kpi_id] [int] NOT NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_scorecard_kpi] PRIMARY KEY CLUSTERED 
(
	[scorecard_id] ASC,
	[kpi_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[fact_forecast_workload](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[start_time] [smalldatetime] NOT NULL,
	[workload_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[end_time] [smalldatetime] NOT NULL,
	[skill_id] [int] NULL,
	[forecasted_calls] [decimal](28, 4) NULL,
	[calculated_calls] [decimal](28, 4) NULL,
	[forecasted_emails] [decimal](28, 4) NULL,
	[forecasted_backoffice_tasks] [decimal](28, 4) NULL,
	[forecasted_campaign_calls] [decimal](28, 4) NULL,
	[forecasted_calls_excl_campaign] [decimal](28, 4) NULL,
	[forecasted_talk_time_s] [decimal](28, 4) NULL,
	[forecasted_campaign_talk_time_s] [decimal](28, 4) NULL,
	[forecasted_talk_time_excl_campaign_s] [decimal](28, 4) NULL,
	[forecasted_after_call_work_s] [decimal](28, 4) NULL,
	[forecasted_campaign_after_call_work_s] [decimal](28, 4) NULL,
	[forecasted_after_call_work_excl_campaign_s] [decimal](28, 4) NULL,
	[forecasted_handling_time_s] [decimal](28, 4) NULL,
	[forecasted_campaign_handling_time_s] [decimal](28, 4) NULL,
	[forecasted_handling_time_excl_campaign_s] [decimal](28, 4) NULL,
	[period_length_min] [decimal](28, 4) NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_forecast_workload] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[start_time] ASC,
	[workload_id] ASC,
	[scenario_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[fact_agent_queue](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[queue_id] [int] NOT NULL,
	[acd_login_id] [int] NOT NULL,
	[local_date_id] [int] NULL,
	[local_interval_id] [smallint] NULL,
	[talk_time_s] [decimal](20, 2) NULL,
	[after_call_work_time_s] [decimal](20, 2) NULL,
	[answered_calls] [int] NULL,
	[transfered_calls] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_agent_queue] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[queue_id] ASC,
	[acd_login_id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_datasource_localdate_date_interval_acd] ON [mart].[fact_agent_queue] 
(
	[datasource_id] ASC,
	[local_date_id] ASC
)
INCLUDE ( [date_id],
[interval_id],
[queue_id],
[acd_login_id]) 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[fact_agent](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[acd_login_id] [int] NOT NULL,
	[local_date_id] [int] NULL,
	[local_interval_id] [smallint] NULL,
	[ready_time_s] [decimal](20, 2) NULL,
	[logged_in_time_s] [decimal](20, 2) NULL,
	[not_ready_time_s] [decimal](20, 2) NULL,
	[idle_time_s] [decimal](20, 2) NULL,
	[direct_outbound_calls] [int] NULL,
	[direct_outbound_talk_time_s] [decimal](20, 2) NULL,
	[direct_incoming_calls] [int] NULL,
	[direct_incoming_calls_talk_time_s] [decimal](20, 2) NULL,
	[admin_time_s] [decimal](20, 2) NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_agent] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[acd_login_id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_datasource_localdate_date_interval_acd] ON [mart].[fact_agent] 
(
	[datasource_id] ASC,
	[local_date_id] ASC
)
INCLUDE ( [date_id],
[interval_id],
[acd_login_id]) 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[fact_queue](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[queue_id] [int] NOT NULL,
	[local_date_id] [int] NULL,
	[local_interval_id] [smallint] NULL,
	[offered_calls] [decimal](24, 5) NULL,
	[answered_calls] [decimal](24, 5) NULL,
	[answered_calls_within_SL] [decimal](24, 5) NULL,
	[abandoned_calls] [decimal](24, 5) NULL,
	[abandoned_calls_within_SL] [decimal](24, 5) NULL,
	[abandoned_short_calls] [decimal](18, 0) NULL,
	[overflow_out_calls] [decimal](24, 5) NULL,
	[overflow_in_calls] [decimal](24, 5) NULL,
	[talk_time_s] [decimal](24, 5) NULL,
	[after_call_work_s] [decimal](24, 5) NULL,
	[handle_time_s] [decimal](24, 5) NULL,
	[speed_of_answer_s] [decimal](24, 5) NULL,
	[time_to_abandon_s] [decimal](24, 5) NULL,
	[longest_delay_in_queue_answered_s] [decimal](24, 5) NULL,
	[longest_delay_in_queue_abandoned_s] [decimal](24, 5) NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_fact_queue] PRIMARY KEY CLUSTERED 
(
	[queue_id] ASC,
	[interval_id] ASC,
	[date_id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_datasource_localdate_date_interval_queue] ON [mart].[fact_queue] 
(
	[datasource_id] ASC,
	[local_date_id] ASC
)
INCLUDE ( [date_id],
[interval_id],
[queue_id]) 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[etl_job_schedule_period](
	[schedule_id] [int] NOT NULL,
	[job_id] [int] NOT NULL,
	[relative_period_start] [int] NOT NULL,
	[relative_period_end] [int] NOT NULL,
 CONSTRAINT [PK_etl_job_schedule_period] PRIMARY KEY CLUSTERED 
(
	[schedule_id] ASC,
	[job_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[etl_job_execution](
	[job_execution_id] [int] IDENTITY(1,1) NOT NULL,
	[job_id] [int] NULL,
	[schedule_id] [int] NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](100) NULL,
	[job_start_time] [datetime] NULL,
	[job_end_time] [datetime] NULL,
	[duration_s] [int] NULL,
	[affected_rows] [int] NULL,
	[job_execution_success] [bit] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_etl_job_execution] PRIMARY KEY CLUSTERED 
(
	[job_execution_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[bridge_time_zone](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[time_zone_id] [smallint] NOT NULL,
	[local_date_id] [int] NULL,
	[local_interval_id] [smallint] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_bridge_time_zone] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[time_zone_id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_bridge_time_zone_local_date_id_local_interval_id] ON [mart].[bridge_time_zone] 
(
	[local_date_id] ASC
)
INCLUDE ( [local_interval_id]) 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[bridge_skillset_skill](
	[skillset_id] [int] NOT NULL,
	[skill_id] [int] NOT NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_bridge_skillset_skill] PRIMARY KEY CLUSTERED 
(
	[skillset_id] ASC,
	[skill_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[bridge_queue_workload](
	[queue_id] [int] NOT NULL,
	[workload_id] [int] NOT NULL,
	[skill_id] [int] NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_bridge_queue_workload] PRIMARY KEY CLUSTERED 
(
	[queue_id] ASC,
	[workload_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_person](
	[person_id] [int] IDENTITY(1,1) NOT NULL,
	[person_code] [uniqueidentifier] NULL,
	[valid_from_date] [smalldatetime] NOT NULL,
	[valid_to_date] [smalldatetime] NOT NULL,
	[valid_from_date_id] [int] NOT NULL,
	[valid_from_interval_id] [int] NOT NULL,
	[valid_to_date_id] [int] NOT NULL,
	[valid_to_interval_id] [int] NOT NULL,
	[person_period_code] [uniqueidentifier] NULL,
	[person_name] [nvarchar](200) NOT NULL,
	[first_name] [nvarchar](30) NOT NULL,
	[last_name] [nvarchar](30) NOT NULL,
	[employment_number] [nvarchar](50) NULL,
	[employment_type_code] [int] NULL,
	[employment_type_name] [nvarchar](50) NOT NULL,
	[contract_code] [uniqueidentifier] NULL,
	[contract_name] [nvarchar](50) NULL,
	[parttime_code] [uniqueidentifier] NULL,
	[parttime_percentage] [nvarchar](50) NULL,
	[team_id] [int] NULL,
	[team_code] [uniqueidentifier] NULL,
	[team_name] [nvarchar](50) NOT NULL,
	[site_id] [int] NULL,
	[site_code] [uniqueidentifier] NULL,
	[site_name] [nvarchar](50) NOT NULL,
	[business_unit_id] [int] NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[skillset_id] [int] NULL,
	[email] [nvarchar](200) NULL,
	[note] [nvarchar](1024) NULL,
	[employment_start_date] [smalldatetime] NULL,
	[employment_end_date] [smalldatetime] NULL,
	[time_zone_id] [int] NULL,
	[is_agent] [bit] NULL,
	[is_user] [bit] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[to_be_deleted] [bit] NOT NULL,
 CONSTRAINT [PK_dim_person] PRIMARY KEY CLUSTERED 
(
	[person_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_site](
	[site_id] [int] IDENTITY(1,1) NOT NULL,
	[site_code] [uniqueidentifier] NULL,
	[site_name] [nvarchar](100) NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_site] PRIMARY KEY CLUSTERED 
(
	[site_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[aspnet_Users](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[LoweredUserName] [nvarchar](256) NOT NULL,
	[MobileAlias] [nvarchar](16) NULL,
	[IsAnonymous] [bit] NOT NULL,
	[LastActivityDate] [datetime] NOT NULL,
	[AppLoginName] [nvarchar](256) NOT NULL,
	[FirstName] [nvarchar](30) NOT NULL,
	[LastName] [nvarchar](30) NOT NULL,
	[LanguageId] [int] NOT NULL,
	[CultureId] [int] NOT NULL,
 CONSTRAINT [PK_aspnet_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [mart].[sys_crossdatabaseview](
	[view_id] [int] IDENTITY(1,1) NOT NULL,
	[view_name] [varchar](100) NOT NULL,
	[view_definition] [varchar](4000) NOT NULL,
	[target_id] [int] NOT NULL,
 CONSTRAINT [pk_sys_crossdatabaseview] PRIMARY KEY CLUSTERED 
(
	[view_id] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] ON
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (1, N'v_log_object', N'SELECT * FROM [$$$target$$$].dbo.log_object', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (2, N'v_agent_logg', N'SELECT * FROM [$$$target$$$].dbo.agent_logg WITH (NOLOCK)', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (3, N'v_agent_info', N'SELECT * FROM [$$$target$$$].dbo.agent_info', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (4, N'v_queues', N'SELECT * FROM [$$$target$$$].dbo.queues', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (5, N'v_queue_logg', N'SELECT * FROM [$$$target$$$].dbo.queue_logg  WITH (NOLOCK)', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (6, N'v_ccc_system_info', N'SELECT * FROM [$$$target$$$].dbo.ccc_system_info', 4)
SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] OFF
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [mart].[report_control_collection](
	[control_collection_id] [int] NOT NULL,
	[collection_id] [int] NOT NULL,
	[print_order] [int] NOT NULL,
	[control_id] [int] NOT NULL,
	[default_value] [nvarchar](4000) NOT NULL,
	[control_name_resource_key] [nvarchar](50) NOT NULL,
	[fill_proc_param] [varchar](100) NULL,
	[param_name] [varchar](50) NULL,
	[depend_of1] [int] NULL,
	[depend_of2] [int] NULL,
	[depend_of3] [int] NULL,
	[depend_of4] [int] NULL,
 CONSTRAINT [PK_report_control_collection] PRIMARY KEY CLUSTERED 
(
	[control_collection_id] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (1, 1, 0, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (2, 1, 1, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 1, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (3, 1, 2, 3, N'1', N'ResSiteNameColon', NULL, N'@site_id', 1, 2, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (4, 1, 3, 4, N'1', N'ResTeamNameColon', NULL, N'@team_id', 1, 2, 3, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (5, 2, 0, 6, N'12:00', N'ResDateColon', NULL, N'@date', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (6, 2, 1, 7, N'8', N'ResIntervalColon', NULL, N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (7, 2, 2, 8, N'17', N'ResIntervalColon', NULL, N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (8, 2, 3, 9, N'1', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (9, 2, 4, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 8, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (10, 3, 0, 6, N'12:00', N'ResDateColon', NULL, N'@date', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (11, 3, 1, 11, N'3', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (12, 3, 2, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (13, 3, 3, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (14, 3, 4, 9, N'-2', N'ResSkillColon', NULL, N'@skill_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (15, 3, 5, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 14, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (17, 4, 1, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (18, 4, 2, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 17, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (19, 4, 3, 11, N'4', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (20, 4, 4, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (21, 4, 5, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 20, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (22, 4, 6, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (23, 4, 7, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (24, 5, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (25, 5, 2, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (26, 5, 3, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (27, 5, 4, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 26, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (28, 5, 5, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (29, 5, 6, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (30, 6, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (31, 6, 2, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (32, 6, 3, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 31, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (33, 6, 4, 11, N'4', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (34, 6, 5, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (35, 6, 6, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 34, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (36, 6, 7, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (37, 6, 8, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (38, 7, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (39, 7, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (40, 7, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 39, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (41, 7, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (42, 7, 5, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (43, 7, 6, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 39, 40, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (44, 7, 7, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 39, 40, 43, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (47, 8, 1, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (48, 8, 2, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 47, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (49, 8, 3, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (50, 8, 4, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (51, 8, 8, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 47, 48, 200, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (52, 8, 9, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 47, 48, 51, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (53, 8, 12, 18, N'-99', N'ResAgentsColon', NULL, N'@agent_set', 47, 48, 51, 52)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (54, 8, 13, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (55, 9, 1, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (56, 9, 2, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 55, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (57, 9, 3, 11, N'4', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (58, 9, 4, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (59, 9, 5, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 58, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (60, 9, 6, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (61, 9, 7, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (62, 9, 8, 20, N'1', N'ResServiceLevelCalcColon', NULL, N'@sl_calc_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (64, 10, 1, 6, N'12:00', N'ResDateColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (65, 10, 5, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 64, 203, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (66, 10, 6, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 64, 65, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (67, 10, 8, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (68, 10, 9, 21, N'1', N'ResSortByColon', NULL, N'@sort_by', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (69, 8, 14, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (70, 11, 1, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (71, 11, 2, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 70, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (72, 11, 3, 23, N'-99', N'ResQueueColon', NULL, N'@queue_set', 70, 71, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (73, 11, 4, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (74, 11, 5, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 73, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (75, 11, 6, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (76, 11, 7, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (77, 11, 8, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (78, 10, 10, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (79, 9, 9, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (80, 4, 8, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (81, 5, 7, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (82, 6, 9, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (83, 12, 1, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (84, 12, 2, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 83, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (85, 12, 3, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (86, 12, 4, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 85, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (87, 12, 5, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (88, 12, 6, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (89, 12, 10, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 85, 86, 206, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (90, 12, 11, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 85, 86, 89, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (91, 12, 12, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_code', 85, 86, 89, 90)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (92, 12, 13, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (93, 13, 1, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (94, 13, 2, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 93, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (95, 13, 3, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (96, 13, 4, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (97, 13, 8, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 93, 94, 209, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (98, 13, 9, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 93, 94, 97, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (99, 13, 10, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_code', 93, 94, 97, 98)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (100, 13, 11, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (101, 14, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (102, 14, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (103, 14, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 102, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (104, 14, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (105, 14, 5, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
GO
print 'Processed 100 total records'
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (106, 14, 9, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 102, 103, 212, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (107, 14, 10, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 102, 103, 106, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (108, 14, 11, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_code', 102, 103, 106, 107)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (111, 14, 13, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (112, 15, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (113, 15, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (114, 15, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 113, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (115, 15, 7, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 113, 114, 218, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (116, 15, 8, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 113, 114, 115, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (117, 15, 9, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_code', 113, 114, 115, 116)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (118, 15, 10, 26, N'-99', N'ResShiftCategoryColon', NULL, N'@shift_category_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (119, 15, 11, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (120, 16, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (121, 16, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (122, 16, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 121, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (123, 16, 7, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 121, 122, 221, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (124, 16, 8, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 121, 122, 123, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (125, 16, 9, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_code', 121, 122, 123, 124)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (126, 16, 10, 28, N'-99', N'ResShiftCategoryColon', NULL, N'@shift_category_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (127, 16, 11, 27, N'-1', N'ResDayOffColon', NULL, N'@day_off_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (128, 16, 12, 25, N'-1', N'ResAbsenceColon', NULL, N'@absence_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (129, 16, 13, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (130, 17, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (131, 17, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 130, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (132, 17, 9, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 130, 131, 215, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (133, 17, 10, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 130, 131, 132, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (134, 17, 11, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_code', 130, 131, 132, 133)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (135, 17, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (136, 17, 13, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (137, 17, 12, 24, N'-99', N'ResActivityColon', NULL, N'@activity_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (138, 17, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (139, 17, 5, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (140, 18, 1, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (141, 18, 2, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 140, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (142, 18, 8, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 140, 141, 197, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (143, 18, 9, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 140, 141, 142, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (146, 18, 11, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (147, 18, 10, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (148, 18, 3, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (149, 18, 4, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (150, 19, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (151, 19, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (152, 19, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 151, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (153, 19, 7, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 151, 152, 188, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (154, 19, 8, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 151, 152, 153, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (155, 19, 9, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_code', 151, 152, 153, 154)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (156, 19, 10, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (157, 20, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (158, 20, 2, 6, N'12:00', N'ResDateColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (159, 20, 3, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (160, 20, 4, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (161, 20, 8, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 158, 224, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (162, 20, 9, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 158, 161, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (164, 20, 11, 24, N'-99', N'ResActivityColon', NULL, N'@activity_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (165, 20, 12, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (166, 21, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (167, 21, 2, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (168, 21, 3, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 167, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (169, 21, 4, 11, N'4', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (170, 21, 5, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (171, 21, 6, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 170, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (172, 21, 7, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (173, 21, 8, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (174, 21, 12, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 170, 171, 191, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (175, 21, 13, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 170, 171, 174, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (176, 21, 14, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (177, 21, 15, 20, N'1', N'ResServiceLevelCalcColon', NULL, N'@sl_calc_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (178, 21, 16, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (179, 22, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (180, 22, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (181, 22, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 180, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (182, 22, 7, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 180, 181, 194, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (183, 22, 8, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 180, 181, 182, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (184, 22, 9, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_code', 180, 181, 182, 183)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (185, 22, 10, 25, N'-1', N'ResAbsenceColon', NULL, N'@absence_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (186, 22, 11, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (187, 10, 7, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_person_code', 64, 65, 66, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (188, 19, 4, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (189, 19, 5, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 188, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (190, 19, 6, 31, N'-2', N'ResAgentColon', NULL, N'@group_page_agent_code', 151, 152, 188, 189)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (191, 21, 9, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (192, 21, 10, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 191, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (194, 22, 4, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (195, 22, 5, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 194, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (196, 22, 6, 31, N'-2', N'ResAgentColon', NULL, N'@group_page_agent_code', 180, 181, 194, 195)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (197, 18, 5, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (198, 18, 6, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 197, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (200, 8, 5, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (201, 8, 6, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 200, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (202, 8, 7, 32, N'-99', N'ResAgentColon', NULL, N'@group_page_agent_set', 47, 48, 200, 201)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (203, 10, 2, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (204, 10, 3, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 203, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (205, 10, 4, 31, N'-2', N'ResAgentColon', NULL, N'@group_page_agent_code', 64, 203, 204, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (206, 12, 7, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (207, 12, 8, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 206, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (208, 12, 9, 31, N'-2', N'ResAgentColon', NULL, N'@group_page_agent_code', 85, 86, 206, 207)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (209, 13, 5, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (210, 13, 6, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 209, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (211, 13, 7, 31, N'-2', N'ResAgentColon', NULL, N'@group_page_agent_code', 93, 94, 209, 210)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (212, 14, 6, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (213, 14, 7, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 212, NULL, NULL, NULL)
GO
print 'Processed 200 total records'
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (214, 14, 8, 31, N'-2', N'ResAgentColon', NULL, N'@group_page_agent_code', 102, 103, 212, 213)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (215, 17, 6, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (216, 17, 7, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 215, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (217, 17, 8, 31, N'-2', N'ResAgentColon', NULL, N'@group_page_agent_code', 130, 131, 215, 216)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (218, 15, 4, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (219, 15, 5, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 218, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (220, 15, 6, 31, N'-2', N'ResAgentColon', NULL, N'@group_page_agent_code', 113, 114, 218, 219)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (221, 16, 4, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (222, 16, 5, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 221, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (223, 16, 6, 31, N'-2', N'ResAgentColon', NULL, N'@group_page_agent_code', 121, 122, 221, 222)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (224, 20, 5, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (225, 20, 6, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 224, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (226, 23, 1, 14, N'0', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (227, 23, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (228, 23, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 227, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (229, 23, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (230, 23, 5, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (231, 23, 9, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 227, 228, 235, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (232, 23, 10, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 227, 228, 231, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (233, 23, 11, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_code', 227, 228, 231, 232)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (234, 23, 13, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (235, 23, 6, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (236, 23, 7, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 235, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (237, 23, 8, 31, N'-2', N'ResAgentColon', NULL, N'@group_page_agent_code', 227, 228, 235, 236)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (238, 23, 12, 33, N'-1', N'ResOvertimeColon', NULL, N'@overtime_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (239, 24, 1, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (240, 24, 2, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 239, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (241, 24, 3, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (242, 24, 4, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (243, 24, 5, 29, N'-2', N'ResGroupPageColon', NULL, N'@group_page_code', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (244, 24, 8, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 239, 240, 243, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (245, 24, 9, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 239, 240, 244, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (246, 24, 10, 18, N'-2', N'ResAgentsColon', NULL, N'@agent_set', 239, 240, 244, 245)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (247, 24, 6, 30, N'-2', N'ResGroupPageGroupColon', NULL, N'@group_page_group_id', 243, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (248, 24, 7, 32, N'-2', N'ResAgentColon', NULL, N'@group_page_agent_set', 239, 240, 243, 247)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (249, 24, 11, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [mart].[report](
	[report_id] [int] NOT NULL,
	[control_collection_id] [int] NOT NULL,
	[report_group_id] [int] NULL,
	[url] [nvarchar](500) NULL,
	[target] [nvarchar](50) NULL,
	[report_name] [nvarchar](500) NULL,
	[report_name_resource_key] [nvarchar](50) NOT NULL,
	[visible] [bit] NOT NULL,
	[rpt_file_name] [varchar](100) NOT NULL,
	[text_id] [int] NULL,
	[proc_name] [varchar](100) NOT NULL,
	[help_key] [nvarchar](500) NULL,
	[sub1_name] [varchar](50) NOT NULL,
	[sub1_proc_name] [varchar](50) NOT NULL,
	[sub2_name] [varchar](50) NOT NULL,
	[sub2_proc_name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_report] PRIMARY KEY CLUSTERED 
(
	[report_id] ASC
)
)
GO
SET ANSI_PADDING OFF
GO
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (1, 19, 1, N'~/Selection.aspx?ReportID=1', N'_blank', N'Preferences per Day', N'ResReportPreferencesPerDay', 1, N'~/Reports/CCC/report_schedule_preferences_per_day.rdlc', 1000, N'mart.report_data_schedule_preferences_per_day', N'f01_Report_PreferencesPerDay.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (2, 19, 1, N'~/Selection.aspx?ReportID=2', N'_blank', N'Preferences Per Agent', N'ResReportPreferencesPerAgent', 1, N'~/Reports/CCC/report_schedule_preferences_per_agent.rdlc', 1000, N'mart.report_data_schedule_preferences_per_agent', N'f01_Report_PreferencesPerAgent.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (3, 21, 1, N'~/Selection.aspx?ReportID=3', N'_blank', N'IMPROVE', N'ResReportImprove', 1, N'~/Reports/CCC/report_IMPROVE.rdlc', 1000, N'mart.report_data_IMPROVE', N'f01_Report_ImproveReport.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (4, 22, 1, N'~/Selection.aspx?ReportID=4', N'_blank', N'Absence per Agent', N'ResReportAbsenceTimePerAgent', 1, N'~/Reports/CCC/report_absence_time_per_agent.rdlc', 1000, N'mart.report_data_absence_time_per_agent', N'f01_Report_AbsenceTimeperAgent.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (6, 1, 1, N'~/Reports/CCC/AgentScorecard.aspx', N'main', N'Agent Scorecard', N'ResReportAgentScorecard', 0, N'', 1000, N'', N'', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (7, 5, 1, N'~/Selection.aspx?ReportID=7', N'_blank', N'Forecast vs Scheduled Hours', N'ResReportForecastvsScheduledHours', 1, N'~/Reports/CCC/report_forecast_vs_scheduled_hours.rdlc', 1000, N'mart.report_data_forecast_vs_scheduled_hours', N'f01_Report_ForecastvsScheduledHours.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (8, 4, 1, N'~/Selection.aspx?ReportID=8', N'_blank', N'Abandonment and Speed of Answer', N'ResReportAbandonmentAndSpeedOfAnswer', 1, N'~/Reports/CCC/report_queue_stat_abnd.rdlc', 1000, N'mart.report_data_queue_stat_abnd', N'f01_Report_AbandonmentAndSpeedOfAnswer.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (9, 9, 1, N'~/Selection.aspx?ReportID=9', N'_blank', N'Service Level and Agents Ready', N'ResReportServiceLevelAndAgentsReady', 1, N'~/Reports/CCC/report_service_level_agents_ready.rdlc', 1000, N'mart.report_data_service_level_agents_ready', N'f01_Report_ServiceLevelAndAgentsReady.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (10, 6, 1, N'~/Selection.aspx?ReportID=10', N'_blank', N'Forecast vs Actual Workload', N'ResReportForecastvsActualWorkload', 1, N'~/Reports/CCC/report_forecast_vs_actual_workload.rdlc', 1000, N'mart.report_data_forecast_vs_actual_workload', N'f01_Report_ForecastvsActualWorkload.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (11, 18, 1, N'~/Selection.aspx?ReportID=11', N'_blank', N'Team Metrics', N'ResReportTeamMetrics', 1, N'~/Reports/CCC/report_team_metrics.rdlc', 1000, N'mart.report_data_team_metrics', N'f01_Report_TeamMetrics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (12, 8, 1, N'~/Selection.aspx?ReportID=12', N'_blank', N'Agent Metrics', N'ResReportAgentScheduleResult', 1, N'~/Reports/CCC/report_agent_schedule_result.rdlc', 1000, N'mart.report_data_agent_schedule_result', N'f01_Report_AgentScheduleResult.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (13, 10, 1, N'~/Selection.aspx?ReportID=13', N'_blank', N'Agent Schedule Adherence', N'ResReportAgentScheduleAdherence', 1, N'~/Reports/CCC/report_agent_schedule_adherence.aspx', 1000, N'mart.report_data_agent_schedule_adherence', N'f01_Report_AgentScheduleAdherence.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (14, 11, 1, N'~/Selection.aspx?ReportID=14', N'_blank', N'Queue Statistics', N'ResReportQueueStatistics', 1, N'~/Reports/CCC/report_queue_stat_raw.rdlc', 1000, N'mart.report_data_queue_stat_raw', N'f01_Report_QueueStatistics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (15, 12, 1, N'~/Selection.aspx?reportID=15', N'_blank', N'Agent Queue Statistics', N'ResReportAgentQueueStatistics', 1, N'~/Reports/CCC/report_agent_queue_stat_raw.rdlc', 1000, N'mart.report_data_agent_queue_stat_raw', N'f01_Report_AgentQueueStatistics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (16, 13, 1, N'~/Selection.aspx?ReportID=16', N'_blank', N'Agent Statistics', N'ResReportAgentStatistics', 1, N'~/Reports/CCC/report_agent_stat_raw.rdlc', 1000, N'mart.report_data_agent_stat_raw', N'f01_Report_AgentStatistics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (17, 14, 1, N'~/Selection.aspx?ReportID=17', N'_blank', N'Scheduled Time per Agent', N'ResReportScheduledTimePerAgent', 1, N'~/Reports/CCC/report_scheduled_time_per_agent.rdlc', 1000, N'mart.report_data_scheduled_time_per_agent', N'f01_Report_ScheduledTimePerAgent.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (18, 17, 1, N'~/Selection.aspx?ReportID=18', N'_blank', N'Scheduled Time per Activity', N'ResReportScheduledTimePerActivity', 1, N'~/Reports/CCC/report_scheduled_time_per_activity.rdlc', 1000, N'mart.report_data_scheduled_time_per_activity', N'f01_Report_ScheduledTimePerActivity.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (19, 15, 1, N'~/Selection.aspx?ReportID=19', N'_blank', N'Shift Category per Day', N'ResReportShiftCategoryPerDay', 1, N'~/Reports/CCC/report_shift_category_per_day.rdlc', 1000, N'mart.report_data_shift_category_per_day', N'f01_Report_ShiftCategoryPerDay.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (20, 16, 1, N'~/Selection.aspx?ReportID=20', N'_blank', N'Shift Category and Full Day Absences per Agent', N'ResReportShiftCategoryAndDayAbsencePerAgent', 1, N'~/Reports/CCC/report_shift_category_and_day_absences_per_agent.rdlc', 1000, N'mart.report_data_shift_cat_and_day_abs_per_agent', N'f01_Report_ShiftCategoryAndFullDayAbsencePerAgent.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (21, 20, 1, N'~/Selection.aspx?ReportID=21', N'_blank', N'Scheduled Agents per Interval and Team', N'ResReportScheduledAgentsPerIntervalAndTeam', 1, N'~/Reports/CCC/report_scheduled_agents_per_interval_and_team.rdlc', 1000, N'mart.report_data_scheduled_agents_per_interval_and_team', N'f01_Report_ScheduledAgentsPerIntervalAndTeam.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (22, 22, 1, N'~/Selection.aspx?ReportID=22', N'_self', N'Absence time per absence', N'ResReportAbsenceTimePerAbsence', 1, N'~/Reports/CCC/report_absence_time_per_absence.rdlc', 1000, N'mart.report_data_absence_time_per_agent', N'f01_Report_AbsenceTimeperAbsence.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (23, 23, 1, N'~/Selection.aspx?ReportID=23', N'_blank', N'Schedule overtime', N'ResReportScheduledOvertimePerAgent', 1, N'~/Reports/CCC/report_scheduled_overtime_per_agent.rdlc', 1000, N'mart.report_data_scheduled_overtime_per_agent', N'f01_Report_ScheduleOvertime.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (24, 24, 1, N'~/Selection.aspx?ReportID=24', N'_blank', N'Agent Queue Metrics', N'ResReportAgentQueueMetrics', 1, N'~/Reports/CCC/report_agent_queue_metrics.rdlc', 1000, N'mart.report_data_agent_queue_metrics', N'f01_Report_AgentQueueMetrics.html', N'', N'', N'', N'')
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[fact_schedule_forecast_skill](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[skill_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[forecasted_resources_m] [decimal](28, 4) NULL,
	[forecasted_resources] [decimal](28, 4) NULL,
	[forecasted_resources_incl_shrinkage_m] [decimal](28, 4) NULL,
	[forecasted_resources_incl_shrinkage] [decimal](28, 4) NULL,
	[scheduled_resources_m] [decimal](28, 4) NULL,
	[scheduled_resources] [decimal](28, 4) NULL,
	[scheduled_resources_incl_shrinkage_m] [decimal](28, 4) NULL,
	[scheduled_resources_incl_shrinkage] [decimal](28, 4) NULL,
	[intraday_deviation_m] [decimal](28, 4) NULL,
	[relative_difference] [decimal](28, 4) NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_schedule_forecast_skill] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[skill_id] ASC,
	[scenario_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[permission_report](
	[report_id] [int] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[team_id] [int] NOT NULL,
	[my_own] [bit] NOT NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_permission_report] PRIMARY KEY CLUSTERED 
(
	[report_id] ASC,
	[person_code] ASC,
	[team_id] ASC,
	[my_own] ASC,
	[business_unit_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[fact_schedule_deviation](
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
	[datasource_update_date] [smalldatetime] NULL,
	[is_logged_in] [bit] NOT NULL,
 CONSTRAINT [PK_fact_schedule_deviation] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[person_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[fact_schedule_day_count](
	[date_id] [int] NOT NULL,
	[start_interval_id] [smallint] NOT NULL,
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
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_schedule_day_count] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[start_interval_id] ASC,
	[person_id] ASC,
	[scenario_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[fact_schedule](
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
	[last_publish] [smalldatetime] NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
	[datasource_update_date] [smalldatetime] NULL,
	[overtime_id] [int] NOT NULL,
 CONSTRAINT [PK_fact_schedule] PRIMARY KEY CLUSTERED 
(
	[schedule_date_id] ASC,
	[person_id] ASC,
	[interval_id] ASC,
	[activity_starttime] ASC,
	[scenario_id] ASC
)
)
GO
CREATE NONCLUSTERED INDEX [IX_person_scenario_schdate_interval] ON [mart].[fact_schedule] 
(
	[person_id] ASC,
	[scenario_id] ASC,
	[schedule_date_id] ASC,
	[interval_id] ASC
)
INCLUDE ( [scheduled_ready_time_m]) 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[aspnet_Membership](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Password] [nvarchar](128) NOT NULL,
	[PasswordFormat] [int] NOT NULL,
	[PasswordSalt] [nvarchar](128) NOT NULL,
	[MobilePIN] [nvarchar](16) NULL,
	[Email] [nvarchar](256) NULL,
	[LoweredEmail] [nvarchar](256) NULL,
	[PasswordQuestion] [nvarchar](256) NULL,
	[PasswordAnswer] [nvarchar](128) NULL,
	[IsApproved] [bit] NOT NULL,
	[IsLockedOut] [bit] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastLoginDate] [datetime] NOT NULL,
	[LastPasswordChangedDate] [datetime] NOT NULL,
	[LastLockoutDate] [datetime] NOT NULL,
	[FailedPasswordAttemptCount] [int] NOT NULL,
	[FailedPasswordAttemptWindowStart] [datetime] NOT NULL,
	[FailedPasswordAnswerAttemptCount] [int] NOT NULL,
	[FailedPasswordAnswerAttemptWindowStart] [datetime] NOT NULL,
	[Comment] [ntext] NULL,
 CONSTRAINT [PK_aspnet_Membership] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[bridge_group_page_person](
	[group_page_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[datasource_id] [int] NOT NULL,
	[insert_date] [smalldatetime] NULL,
 CONSTRAINT [PK_bridge_group_page_person] PRIMARY KEY CLUSTERED 
(
	[group_page_id] ASC,
	[person_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[dim_team](
	[team_id] [int] IDENTITY(1,1) NOT NULL,
	[team_code] [uniqueidentifier] NULL,
	[team_name] [nvarchar](100) NULL,
	[scorecard_id] [int] NULL,
	[site_id] [int] NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_team] PRIMARY KEY CLUSTERED 
(
	[team_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[fact_schedule_preference](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[person_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL,
	[preference_type_id] [int] NOT NULL,
	[shift_category_id] [int] NOT NULL,
	[day_off_id] [int] NOT NULL,
	[preferences_requested_count] [int] NULL,
	[preferences_accepted_count] [int] NULL,
	[preferences_declined_count] [int] NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_schedule_preference] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[person_id] ASC,
	[scenario_id] ASC,
	[preference_type_id] ASC,
	[shift_category_id] ASC,
	[day_off_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[etl_jobstep_execution](
	[jobstep_execution_id] [int] IDENTITY(1,1) NOT NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](100) NULL,
	[duration_s] [int] NULL,
	[rows_affected] [int] NULL,
	[job_execution_id] [int] NULL,
	[jobstep_error_id] [int] NULL,
	[jobstep_id] [int] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_etl_jobstep_execution] PRIMARY KEY CLUSTERED 
(
	[jobstep_execution_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[fact_kpi_targets_team](
	[kpi_id] [int] NOT NULL,
	[team_id] [int] NOT NULL,
	[business_unit_id] [int] NOT NULL,
	[target_value] [float] NOT NULL,
	[min_value] [float] NOT NULL,
	[max_value] [float] NOT NULL,
	[between_color] [int] NOT NULL,
	[lower_than_min_color] [int] NOT NULL,
	[higher_than_max_color] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_kpi_targets_team] PRIMARY KEY CLUSTERED 
(
	[kpi_id] ASC,
	[team_id] ASC
)
)
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [mart].[bridge_acd_login_person](
	[acd_login_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[team_id] [int] NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_bridge_acd_login_person] PRIMARY KEY CLUSTERED 
(
	[acd_login_id] ASC,
	[person_id] ASC
)
)
GO
ALTER TABLE [dbo].[aspnet_applications] ADD  DEFAULT (newid()) FOR [ApplicationId]
GO
ALTER TABLE [dbo].[aspnet_Membership] ADD  DEFAULT ((0)) FOR [PasswordFormat]
GO
ALTER TABLE [dbo].[aspnet_Users] ADD  CONSTRAINT [DF__aspnet_Us__UserI__60DD3190]  DEFAULT (newid()) FOR [UserId]
GO
ALTER TABLE [dbo].[aspnet_Users] ADD  CONSTRAINT [DF__aspnet_Us__Mobil__61D155C9]  DEFAULT (NULL) FOR [MobileAlias]
GO
ALTER TABLE [dbo].[aspnet_Users] ADD  CONSTRAINT [DF__aspnet_Us__IsAno__62C57A02]  DEFAULT ((0)) FOR [IsAnonymous]
GO
ALTER TABLE [dbo].[aspnet_Users] ADD  CONSTRAINT [DF_aspnet_Users_AppLoginName]  DEFAULT ('') FOR [AppLoginName]
GO
ALTER TABLE [dbo].[aspnet_Users] ADD  CONSTRAINT [DF_aspnet_Users_FirstName]  DEFAULT ('') FOR [FirstName]
GO
ALTER TABLE [dbo].[aspnet_Users] ADD  CONSTRAINT [DF_aspnet_Users_LastName]  DEFAULT ('') FOR [LastName]
GO
ALTER TABLE [dbo].[aspnet_Users] ADD  CONSTRAINT [DF_aspnet_Users_LanguageId]  DEFAULT ((1033)) FOR [LanguageId]
GO
ALTER TABLE [dbo].[aspnet_Users] ADD  CONSTRAINT [DF_aspnet_Users_UICultureId]  DEFAULT ((1033)) FOR [CultureId]
GO
ALTER TABLE [mart].[bridge_acd_login_person] ADD  CONSTRAINT [DF_bridge_acd_login_person_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[bridge_acd_login_person] ADD  CONSTRAINT [DF_bridge_acd_login_person_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[bridge_acd_login_person] ADD  CONSTRAINT [DF_bridge_acd_login_person_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[bridge_queue_workload] ADD  CONSTRAINT [DF_bridge_queue_workload_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[bridge_queue_workload] ADD  CONSTRAINT [DF_bridge_queue_workload_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[bridge_queue_workload] ADD  CONSTRAINT [DF_bridge_queue_workload_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[bridge_queue_workload] ADD  CONSTRAINT [DF_bridge_queue_workload_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
GO
ALTER TABLE [mart].[bridge_skillset_skill] ADD  CONSTRAINT [DF_bridge_skillset_skill_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[bridge_skillset_skill] ADD  CONSTRAINT [DF_bridge_skillset_skill_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[bridge_skillset_skill] ADD  CONSTRAINT [DF_bridge_skillset_skill_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[bridge_skillset_skill] ADD  CONSTRAINT [DF_bridge_skillset_skill_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
GO
ALTER TABLE [mart].[dim_absence] ADD  CONSTRAINT [DF_dim_absence_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_absence] ADD  CONSTRAINT [DF_dim_absence_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_absence] ADD  CONSTRAINT [DF_dim_absence_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_absence] ADD  CONSTRAINT [DF_dim_absence_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [mart].[dim_acd_login] ADD  CONSTRAINT [DF_dim_agent_time_zone_id]  DEFAULT ((-1)) FOR [time_zone_id]
GO
ALTER TABLE [mart].[dim_acd_login] ADD  CONSTRAINT [DF_dim_agent_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_acd_login] ADD  CONSTRAINT [DF_dim_agent_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_acd_login] ADD  CONSTRAINT [DF_dim_agent_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_acd_login] ADD  CONSTRAINT [DF_dim_agent_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
GO
ALTER TABLE [mart].[dim_activity] ADD  CONSTRAINT [DF_dim_activity_activity_name]  DEFAULT ('Not Defined') FOR [activity_name]
GO
ALTER TABLE [mart].[dim_activity] ADD  CONSTRAINT [DF_dim_activity_display_color]  DEFAULT ((-1)) FOR [display_color]
GO
ALTER TABLE [mart].[dim_activity] ADD  CONSTRAINT [DF_dim_activity_in_ready_time]  DEFAULT ((0)) FOR [in_ready_time]
GO
ALTER TABLE [mart].[dim_activity] ADD  CONSTRAINT [DF_dim_activity_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_activity] ADD  CONSTRAINT [DF_dim_activity_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_activity] ADD  CONSTRAINT [DF_dim_activity_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_activity] ADD  CONSTRAINT [DF_dim_activity_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [mart].[dim_business_unit] ADD  CONSTRAINT [DF_dim_business_unit_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_business_unit] ADD  CONSTRAINT [DF_dim_business_unit_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_business_unit] ADD  CONSTRAINT [DF_dim_business_unit_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_date] ADD  CONSTRAINT [DF_dim_date_inserted]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_day_off] ADD  CONSTRAINT [DF_dim_day_off_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_day_off] ADD  CONSTRAINT [DF_dim_day_off_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_day_off] ADD  CONSTRAINT [DF_dim_day_off_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_kpi] ADD  CONSTRAINT [DF_dim_kpi_increasing_value_is_negative]  DEFAULT ((0)) FOR [decreasing_value_is_positive]
GO
ALTER TABLE [mart].[dim_kpi] ADD  CONSTRAINT [DF_dim_kpi_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_kpi] ADD  CONSTRAINT [DF_dim_kpi_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_kpi] ADD  CONSTRAINT [DF_dim_kpi_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_overtime] ADD  CONSTRAINT [DF_dim_multiplicator_definition_set_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_overtime] ADD  CONSTRAINT [DF_dim_multiplicator_definition_set_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_overtime] ADD  CONSTRAINT [DF_dim_multiplicator_definition_set_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_overtime] ADD  CONSTRAINT [DF_dim_multiplicator_definition_set_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_valid_to_date]  DEFAULT (((2059)-(12))-(31)) FOR [valid_to_date]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_valid_from_date_id]  DEFAULT ((-1)) FOR [valid_from_date_id]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_valid_from_interval_id]  DEFAULT ((0)) FOR [valid_from_interval_id]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_valid_from_date_id1]  DEFAULT ((-1)) FOR [valid_to_date_id]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_valid_from_interval_id1]  DEFAULT ((0)) FOR [valid_to_interval_id]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_person_name]  DEFAULT (N'Not Defined') FOR [person_name]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_first_name]  DEFAULT (N'Not Defined') FOR [first_name]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_last_name]  DEFAULT (N'Not Defined''') FOR [last_name]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_employment_type_name]  DEFAULT (N'Not Defined') FOR [employment_type_name]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_tema_id]  DEFAULT ((-1)) FOR [team_id]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_team_name]  DEFAULT (N'Not Defined') FOR [team_name]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_site_id]  DEFAULT ((-1)) FOR [site_id]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_site_name]  DEFAULT (N'Not Defined') FOR [site_name]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_business_unit_name]  DEFAULT (N'Not Defined') FOR [business_unit_name]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_person] ADD  CONSTRAINT [DF_dim_person_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_queue_name]  DEFAULT ('Not Defined') FOR [queue_name]
GO
ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
GO
ALTER TABLE [mart].[dim_scenario] ADD  CONSTRAINT [DF_dim_scenario_scenario_name]  DEFAULT (N'Not Defined') FOR [scenario_name]
GO
ALTER TABLE [mart].[dim_scenario] ADD  CONSTRAINT [DF_dim_scenario_business_unit_name]  DEFAULT (N'Not Defined') FOR [business_unit_name]
GO
ALTER TABLE [mart].[dim_scenario] ADD  CONSTRAINT [DF_dim_scenario_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_scenario] ADD  CONSTRAINT [DF_dim_scenario_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_scenario] ADD  CONSTRAINT [DF_dim_scenario_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_scenario] ADD  CONSTRAINT [DF_dim_scenario_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [mart].[dim_scorecard] ADD  CONSTRAINT [DF_dim_scorecard_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_scorecard] ADD  CONSTRAINT [DF_dim_scorecard_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_scorecard] ADD  CONSTRAINT [DF_dim_scorecard_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_shift_category] ADD  CONSTRAINT [DF_dim_shift_category_shift_category_name]  DEFAULT ('Not Defined') FOR [shift_category_name]
GO
ALTER TABLE [mart].[dim_shift_category] ADD  CONSTRAINT [DF_dim_shift_category_shift_category_shortname]  DEFAULT ('Not Defined') FOR [shift_category_shortname]
GO
ALTER TABLE [mart].[dim_shift_category] ADD  CONSTRAINT [DF_dim_shift_category_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_shift_category] ADD  CONSTRAINT [DF_dim_shift_category_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_shift_category] ADD  CONSTRAINT [DF_dim_shift_category_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_shift_category] ADD  CONSTRAINT [DF_dim_shift_category_datasource_update_date]  DEFAULT ('2059-12-31') FOR [datasource_update_date]
GO
ALTER TABLE [mart].[dim_shift_category] ADD  CONSTRAINT [DF_dim_shift_category_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [mart].[dim_shift_length] ADD  CONSTRAINT [DF_dim_shift_length_shift_length_m]  DEFAULT ((-1)) FOR [shift_length_m]
GO
ALTER TABLE [mart].[dim_shift_length] ADD  CONSTRAINT [DF_dim_shift_length_shift_length_h]  DEFAULT ((0)) FOR [shift_length_h]
GO
ALTER TABLE [mart].[dim_shift_length] ADD  CONSTRAINT [DF_dim_shift_length_shift_length_group_id]  DEFAULT ((-1)) FOR [shift_length_group_id]
GO
ALTER TABLE [mart].[dim_shift_length] ADD  CONSTRAINT [DF_dim_shift_length_shift_length_group_name]  DEFAULT (N'Not Defined') FOR [shift_length_group_name]
GO
ALTER TABLE [mart].[dim_shift_length] ADD  CONSTRAINT [DF_dim_shift_length_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_shift_length] ADD  CONSTRAINT [DF_dim_shift_length_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_shift_length] ADD  CONSTRAINT [DF_dim_shift_length_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_site] ADD  CONSTRAINT [DF_dim_site_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_site] ADD  CONSTRAINT [DF_dim_site_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_site] ADD  CONSTRAINT [DF_dim_site_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_skill] ADD  CONSTRAINT [DF_dim_skill_name]  DEFAULT ('Not Defined''') FOR [skill_name]
GO
ALTER TABLE [mart].[dim_skill] ADD  CONSTRAINT [DF_dim_skill_time_zone_id]  DEFAULT ((-1)) FOR [time_zone_id]
GO
ALTER TABLE [mart].[dim_skill] ADD  CONSTRAINT [DF_dim_skill_forecast_method_name]  DEFAULT ('Not Defined') FOR [forecast_method_name]
GO
ALTER TABLE [mart].[dim_skill] ADD  CONSTRAINT [DF_dim_skill_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]
GO
ALTER TABLE [mart].[dim_skill] ADD  CONSTRAINT [DF_dim_skill_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_skill] ADD  CONSTRAINT [DF_dim_skill_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_skill] ADD  CONSTRAINT [DF_dim_skill_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_skill] ADD  CONSTRAINT [DF_dim_skill_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
GO
ALTER TABLE [mart].[dim_skill] ADD  CONSTRAINT [DF_dim_skill_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [mart].[dim_skillset] ADD  CONSTRAINT [DF_dim_skillset_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]
GO
ALTER TABLE [mart].[dim_skillset] ADD  CONSTRAINT [DF_dim_skillset_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_skillset] ADD  CONSTRAINT [DF_dim_skillset_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_skillset] ADD  CONSTRAINT [DF_dim_skillset_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_skillset] ADD  CONSTRAINT [DF_dim_skillset_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
GO
ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_scorecard_id]  DEFAULT ((-1)) FOR [scorecard_id]
GO
ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_site_id]  DEFAULT ((-1)) FOR [site_id]
GO
ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]
GO
ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_team] ADD  CONSTRAINT [DF_dim_team_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_time_zone] ADD  CONSTRAINT [DF_dim_time_zone_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_time_zone] ADD  CONSTRAINT [DF_dim_time_zone_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_time_zone] ADD  CONSTRAINT [DF_dim_time_zone_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_workload_name]  DEFAULT ('Not Defined') FOR [workload_name]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_skill_id]  DEFAULT ((-1)) FOR [skill_id]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_skill_name]  DEFAULT ('Not Defined') FOR [skill_name]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_time_zone_id]  DEFAULT ((-1)) FOR [time_zone_id]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_forecast_method_name]  DEFAULT ('Not Defined') FOR [forecast_method_name]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_percentage_offered]  DEFAULT ((1)) FOR [percentage_offered]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_percentage_overflow_in]  DEFAULT ((1)) FOR [percentage_overflow_in]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_percentage_overflow_out]  DEFAULT ((-1)) FOR [percentage_overflow_out]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_percentage_abandoned]  DEFAULT ((-1)) FOR [percentage_abandoned]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_percentage_abandoned_short]  DEFAULT ((0)) FOR [percentage_abandoned_short]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_abandoned_within_service_level]  DEFAULT ((1)) FOR [percentage_abandoned_within_service_level]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_abandoned_after_service_level]  DEFAULT ((1)) FOR [percentage_abandoned_after_service_level]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
GO
ALTER TABLE [mart].[dim_workload] ADD  CONSTRAINT [DF_dim_workload_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [mart].[etl_job] ADD  CONSTRAINT [DF_etl_job_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[etl_job] ADD  CONSTRAINT [DF_etl_job_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[etl_job_execution] ADD  CONSTRAINT [DF_etl_job_execution_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[etl_job_execution] ADD  CONSTRAINT [DF_etl_job_execution_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[etl_job_schedule] ADD  CONSTRAINT [DF_job_schedule_enabled]  DEFAULT ((1)) FOR [enabled]
GO
ALTER TABLE [mart].[etl_job_schedule] ADD  CONSTRAINT [DF_etl_job_schedule_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[etl_job_schedule] ADD  CONSTRAINT [DF_etl_job_schedule_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[etl_jobstep] ADD  CONSTRAINT [DF_etl_jobstep_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[etl_jobstep] ADD  CONSTRAINT [DF_etl_jobstep_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[etl_jobstep_error] ADD  CONSTRAINT [DF_etl_jobstep_error_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[etl_jobstep_error] ADD  CONSTRAINT [DF_etl_jobstep_error_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[etl_jobstep_execution] ADD  CONSTRAINT [DF_etl_jobstep_execution_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[etl_jobstep_execution] ADD  CONSTRAINT [DF_etl_jobstep_execution_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_agent] ADD  CONSTRAINT [DF_fact_agent_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_agent] ADD  CONSTRAINT [DF_fact_agent_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_agent] ADD  CONSTRAINT [DF_fact_agent_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_agent_queue] ADD  CONSTRAINT [DF_fact_agent_queue_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_agent_queue] ADD  CONSTRAINT [DF_fact_agent_queue_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_agent_queue] ADD  CONSTRAINT [DF_fact_agent_queue_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_forecast_workload] ADD  CONSTRAINT [DF_fact_forecast_startdate_id]  DEFAULT ((-1)) FOR [date_id]
GO
ALTER TABLE [mart].[fact_forecast_workload] ADD  CONSTRAINT [DF_fact_forecast_interval_id]  DEFAULT ((-1)) FOR [interval_id]
GO
ALTER TABLE [mart].[fact_forecast_workload] ADD  CONSTRAINT [DF_fact_forecast_queue_id]  DEFAULT ((-1)) FOR [workload_id]
GO
ALTER TABLE [mart].[fact_forecast_workload] ADD  CONSTRAINT [DF_fact_forecast_scenario_id]  DEFAULT ((-1)) FOR [scenario_id]
GO
ALTER TABLE [mart].[fact_forecast_workload] ADD  CONSTRAINT [DF_fact_forecast_workload_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]
GO
ALTER TABLE [mart].[fact_forecast_workload] ADD  CONSTRAINT [DF_fact_forecast_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_forecast_workload] ADD  CONSTRAINT [DF_fact_forecast_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_forecast_workload] ADD  CONSTRAINT [DF_fact_forecast_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_kpi_targets_team] ADD  CONSTRAINT [DF_fact_kpi_targets_team_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_kpi_targets_team] ADD  CONSTRAINT [DF_fact_kpi_targets_team_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_kpi_targets_team] ADD  CONSTRAINT [DF_fact_kpi_targets_team_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
GO
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
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_category_id]  DEFAULT ((-1)) FOR [shift_category_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_length_id]  DEFAULT ((-1)) FOR [shift_length_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_last_publish]  DEFAULT (((1900)-(1))-(1)) FOR [last_publish]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_schedule] ADD  CONSTRAINT [DF_fact_schedule_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_schedule] ADD  DEFAULT ((-1)) FOR [overtime_id]
GO
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_date_id]  DEFAULT ((-1)) FOR [date_id]
GO
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_person_id]  DEFAULT ((-1)) FOR [person_id]
GO
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_scenario_id]  DEFAULT ((-1)) FOR [scenario_id]
GO
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_shift_category_id]  DEFAULT ((-1)) FOR [shift_category_id]
GO
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_absence_id]  DEFAULT ((-1)) FOR [absence_id]
GO
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_schedule_day_count] ADD  CONSTRAINT [DF_fact_schedule_day_count_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_schedule_deviation] ADD  CONSTRAINT [DF_fact_schedule_deviation_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_schedule_deviation] ADD  CONSTRAINT [DF_fact_schedule_deviation_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_schedule_deviation] ADD  CONSTRAINT [DF_fact_schedule_deviation_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] ADD  CONSTRAINT [DF_fact_forecast_skill_startdate_id]  DEFAULT ((-1)) FOR [date_id]
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] ADD  CONSTRAINT [DF_fact_forecast_skill_interval_id]  DEFAULT ((-1)) FOR [interval_id]
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] ADD  CONSTRAINT [DF_fact_forecast_skill_queue_id]  DEFAULT ((-1)) FOR [skill_id]
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] ADD  CONSTRAINT [DF_fact_forecast_skill_scenario_id]  DEFAULT ((-1)) FOR [scenario_id]
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] ADD  CONSTRAINT [DF_fact_forecast_skill_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] ADD  CONSTRAINT [DF_fact_forecast_skill_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] ADD  CONSTRAINT [DF_fact_forecast_skill_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[fact_schedule_preference] ADD  CONSTRAINT [DF_fact_preferences_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[fact_schedule_preference] ADD  CONSTRAINT [DF_fact_preferences_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[fact_schedule_preference] ADD  CONSTRAINT [DF_fact_preferences_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[permission_report] ADD  CONSTRAINT [DF_permission_report_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]
GO
ALTER TABLE [mart].[permission_report] ADD  CONSTRAINT [DF_permission_report_data_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[permission_report] ADD  CONSTRAINT [DF_permission_report_data_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[pm_user] ADD  CONSTRAINT [DF_pm_user_is_windows_logon]  DEFAULT ((1)) FOR [is_windows_logon]
GO
ALTER TABLE [mart].[pm_user] ADD  CONSTRAINT [DF_pm_user_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[pm_user] ADD  CONSTRAINT [DF_pm_user_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[report] ADD  CONSTRAINT [DF_report_visible]  DEFAULT ((1)) FOR [visible]
GO
ALTER TABLE [mart].[scorecard_kpi] ADD  CONSTRAINT [DF_scorecard_kpi_business_unit_id]  DEFAULT ((-1)) FOR [business_unit_id]
GO
ALTER TABLE [mart].[scorecard_kpi] ADD  CONSTRAINT [DF_scorecard_kpi_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
GO
ALTER TABLE [mart].[scorecard_kpi] ADD  CONSTRAINT [DF_scorecard_kpi_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[scorecard_kpi] ADD  CONSTRAINT [DF_scorecard_kpi_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [mart].[sys_datasource] ADD  CONSTRAINT [DF_sys_datasource_log_object_id]  DEFAULT ((-1)) FOR [log_object_id]
GO
ALTER TABLE [mart].[sys_datasource] ADD  CONSTRAINT [DF_sys_datasource_datasource_type_name]  DEFAULT ('Not Defined') FOR [datasource_type_name]
GO
ALTER TABLE [mart].[sys_datasource] ADD  CONSTRAINT [DF_sys_datasource_inactive]  DEFAULT ((0)) FOR [inactive]
GO
ALTER TABLE [mart].[sys_datasource] ADD  CONSTRAINT [DF_sys_datasource_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [mart].[sys_datasource] ADD  CONSTRAINT [DF_sys_datasource_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [msg].[Subscriber] ADD  CONSTRAINT [DF_Subscriber_IPAddress]  DEFAULT (N'169.0.0.1') FOR [IpAddress]
GO
ALTER TABLE [msg].[Subscriber] ADD  CONSTRAINT [DF_Subscriber_Port]  DEFAULT ((9080)) FOR [Port]
GO
ALTER TABLE [RTA].[ExternalAgentState] ADD  CONSTRAINT [DF_ExternalAgentState_IsSnapshot]  DEFAULT ((0)) FOR [IsSnapshot]
GO
ALTER TABLE [stage].[stg_acd_login_person] ADD  CONSTRAINT [DF_acd_login_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_acd_login_person] ADD  CONSTRAINT [DF_acd_login_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_acd_login_person] ADD  CONSTRAINT [DF_acd_login_datasource_update_date]  DEFAULT (getdate()) FOR [datasource_update_date]
GO
ALTER TABLE [stage].[stg_activity] ADD  CONSTRAINT [DF_stg_activity_in_ready_time]  DEFAULT ((0)) FOR [in_ready_time]
GO
ALTER TABLE [stage].[stg_activity] ADD  CONSTRAINT [DF_stg_activity_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_activity] ADD  CONSTRAINT [DF_stg_activity_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_agent_skill] ADD  CONSTRAINT [DF_agent_skill_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_agent_skill] ADD  CONSTRAINT [DF_agent_skill_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_business_unit] ADD  CONSTRAINT [DF_dim_business_unit_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_business_unit] ADD  CONSTRAINT [DF_dim_business_unit_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_date] ADD  CONSTRAINT [DF_stg_date_inserted]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_group_page_person] ADD  CONSTRAINT [DF_stg_group_page_person_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_group_page_person] ADD  CONSTRAINT [DF_stg_group_page_person_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_overtime] ADD  CONSTRAINT [DF_stg_multiplicator_definition_set_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [stage].[stg_overtime] ADD  CONSTRAINT [DF_stg_multiplicator_definition_set_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_overtime] ADD  CONSTRAINT [DF_stg_multiplicator_definition_set_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_overtime] ADD  CONSTRAINT [DF_stg_multiplicator_definition_set_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [stage].[stg_permission_report] ADD  CONSTRAINT [DF_stg_permission_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [stage].[stg_permission_report] ADD  CONSTRAINT [DF_stg_permission_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_permission_report] ADD  CONSTRAINT [DF_stg_permission_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_permission_report] ADD  CONSTRAINT [DF_stg_permission_report_datasource_update_date]  DEFAULT (getdate()) FOR [datasource_update_date]
GO
ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_valid_from_interval_id]  DEFAULT ((0)) FOR [valid_from_interval_id]
GO
ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_valid_to_interval_id]  DEFAULT ((0)) FOR [valid_to_interval_id]
GO
ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_person] ADD  CONSTRAINT [DF_stg_person_datasource_update_date]  DEFAULT (getdate()) FOR [datasource_update_date]
GO
ALTER TABLE [stage].[stg_queue_workload] ADD  CONSTRAINT [DF_stg_queue_workload_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [stage].[stg_queue_workload] ADD  CONSTRAINT [DF_stg_queue_workload_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_queue_workload] ADD  CONSTRAINT [DF_stg_queue_workload_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_queue_workload] ADD  CONSTRAINT [DF_stg_queue_workload_datasource_update_date]  DEFAULT (getdate()) FOR [datasource_update_date]
GO
ALTER TABLE [stage].[stg_scenario] ADD  CONSTRAINT [DF_stg_scenario_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [stage].[stg_scenario] ADD  CONSTRAINT [DF_stg_scenario_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_scenario] ADD  CONSTRAINT [DF_stg_scenario_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_schedule] ADD  CONSTRAINT [DF_stg_schedule_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [stage].[stg_schedule] ADD  CONSTRAINT [DF_stg_schedule_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_schedule] ADD  CONSTRAINT [DF_stg_schedule_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_schedule_day_absence_count] ADD  CONSTRAINT [DF_stg_schedule_day_absence_count_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [stage].[stg_schedule_day_absence_count] ADD  CONSTRAINT [DF_stg_schedule_day_absence_count_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_schedule_day_absence_count] ADD  CONSTRAINT [DF_stg_schedule_day_absence_count_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_schedule_day_off_count] ADD  CONSTRAINT [DF_stg_schedule_day_off_count_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [stage].[stg_schedule_day_off_count] ADD  CONSTRAINT [DF_stg_schedule_day_off_count_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_schedule_day_off_count] ADD  CONSTRAINT [DF_stg_schedule_day_off_count_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_schedule_preference] ADD  CONSTRAINT [DF_stg_schedule_preference_datasource_update_date]  DEFAULT (getdate()) FOR [datasource_update_date]
GO
ALTER TABLE [stage].[stg_shift_category] ADD  CONSTRAINT [DF_stg_shift_category_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [stage].[stg_shift_category] ADD  CONSTRAINT [DF_stg_shift_category_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_shift_category] ADD  CONSTRAINT [DF_stg_shift_category_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_shift_category] ADD  CONSTRAINT [DF_stg_shift_category_datasource_update_date]  DEFAULT (getdate()) FOR [datasource_update_date]
GO
ALTER TABLE [stage].[stg_skill] ADD  CONSTRAINT [DF_stg_skill_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [stage].[stg_user] ADD  CONSTRAINT [DF_stg_user_datasource_id]  DEFAULT ((1)) FOR [datasource_id]
GO
ALTER TABLE [stage].[stg_user] ADD  CONSTRAINT [DF_stg_user_insert_date]  DEFAULT (getdate()) FOR [insert_date]
GO
ALTER TABLE [stage].[stg_user] ADD  CONSTRAINT [DF_stg_user_update_date]  DEFAULT (getdate()) FOR [update_date]
GO
ALTER TABLE [stage].[stg_workload] ADD  CONSTRAINT [DF_stg_workload_skill_is_deleted]  DEFAULT ((0)) FOR [skill_is_deleted]
GO
ALTER TABLE [stage].[stg_workload] ADD  CONSTRAINT [DF_stg_workload_is_deleted]  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[aspnet_Membership]  WITH CHECK ADD  CONSTRAINT [FK_aspnet_Membership_aspnet_applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[aspnet_applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[aspnet_Membership] CHECK CONSTRAINT [FK_aspnet_Membership_aspnet_applications]
GO
ALTER TABLE [dbo].[aspnet_Membership]  WITH CHECK ADD  CONSTRAINT [FK_aspnet_Membership_aspnet_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[aspnet_Users] ([UserId])
GO
ALTER TABLE [dbo].[aspnet_Membership] CHECK CONSTRAINT [FK_aspnet_Membership_aspnet_Users]
GO
ALTER TABLE [dbo].[aspnet_Users]  WITH CHECK ADD  CONSTRAINT [FK_aspnet_User_aspnet_applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[aspnet_applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[aspnet_Users] CHECK CONSTRAINT [FK_aspnet_User_aspnet_applications]
GO
ALTER TABLE [mart].[bridge_acd_login_person]  WITH CHECK ADD  CONSTRAINT [FK_bridge_acd_login_person_dim_acd_login] FOREIGN KEY([acd_login_id])
REFERENCES [mart].[dim_acd_login] ([acd_login_id])
GO
ALTER TABLE [mart].[bridge_acd_login_person] CHECK CONSTRAINT [FK_bridge_acd_login_person_dim_acd_login]
GO
ALTER TABLE [mart].[bridge_acd_login_person]  WITH CHECK ADD  CONSTRAINT [FK_bridge_acd_login_person_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[bridge_acd_login_person] CHECK CONSTRAINT [FK_bridge_acd_login_person_dim_person]
GO
ALTER TABLE [mart].[bridge_acd_login_person]  WITH CHECK ADD  CONSTRAINT [FK_bridge_acd_login_person_dim_team] FOREIGN KEY([team_id])
REFERENCES [mart].[dim_team] ([team_id])
GO
ALTER TABLE [mart].[bridge_acd_login_person] CHECK CONSTRAINT [FK_bridge_acd_login_person_dim_team]
GO
ALTER TABLE [mart].[bridge_group_page_person]  WITH CHECK ADD  CONSTRAINT [FK_bridge_group_page_person_dim_group_page] FOREIGN KEY([group_page_id])
REFERENCES [mart].[dim_group_page] ([group_page_id])
GO
ALTER TABLE [mart].[bridge_group_page_person] CHECK CONSTRAINT [FK_bridge_group_page_person_dim_group_page]
GO
ALTER TABLE [mart].[bridge_group_page_person]  WITH CHECK ADD  CONSTRAINT [FK_bridge_group_page_person_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[bridge_group_page_person] CHECK CONSTRAINT [FK_bridge_group_page_person_dim_person]
GO
ALTER TABLE [mart].[bridge_queue_workload]  WITH CHECK ADD  CONSTRAINT [FK_bridge_queue_workload_dim_queue] FOREIGN KEY([queue_id])
REFERENCES [mart].[dim_queue] ([queue_id])
GO
ALTER TABLE [mart].[bridge_queue_workload] CHECK CONSTRAINT [FK_bridge_queue_workload_dim_queue]
GO
ALTER TABLE [mart].[bridge_queue_workload]  WITH CHECK ADD  CONSTRAINT [FK_bridge_queue_workload_dim_skill] FOREIGN KEY([skill_id])
REFERENCES [mart].[dim_skill] ([skill_id])
GO
ALTER TABLE [mart].[bridge_queue_workload] CHECK CONSTRAINT [FK_bridge_queue_workload_dim_skill]
GO
ALTER TABLE [mart].[bridge_queue_workload]  WITH CHECK ADD  CONSTRAINT [FK_bridge_queue_workload_dim_workload] FOREIGN KEY([workload_id])
REFERENCES [mart].[dim_workload] ([workload_id])
GO
ALTER TABLE [mart].[bridge_queue_workload] CHECK CONSTRAINT [FK_bridge_queue_workload_dim_workload]
GO
ALTER TABLE [mart].[bridge_skillset_skill]  WITH CHECK ADD  CONSTRAINT [FK_bridge_skillset_skill_dim_skill] FOREIGN KEY([skill_id])
REFERENCES [mart].[dim_skill] ([skill_id])
GO
ALTER TABLE [mart].[bridge_skillset_skill] CHECK CONSTRAINT [FK_bridge_skillset_skill_dim_skill]
GO
ALTER TABLE [mart].[bridge_skillset_skill]  WITH CHECK ADD  CONSTRAINT [FK_bridge_skillset_skill_dim_skillset] FOREIGN KEY([skillset_id])
REFERENCES [mart].[dim_skillset] ([skillset_id])
GO
ALTER TABLE [mart].[bridge_skillset_skill] CHECK CONSTRAINT [FK_bridge_skillset_skill_dim_skillset]
GO
ALTER TABLE [mart].[bridge_time_zone]  WITH CHECK ADD  CONSTRAINT [FK_bridge_time_zone_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[bridge_time_zone] CHECK CONSTRAINT [FK_bridge_time_zone_dim_date]
GO
ALTER TABLE [mart].[bridge_time_zone]  WITH CHECK ADD  CONSTRAINT [FK_bridge_time_zone_dim_date1] FOREIGN KEY([local_date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[bridge_time_zone] CHECK CONSTRAINT [FK_bridge_time_zone_dim_date1]
GO
ALTER TABLE [mart].[bridge_time_zone]  WITH CHECK ADD  CONSTRAINT [FK_bridge_time_zone_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[bridge_time_zone] CHECK CONSTRAINT [FK_bridge_time_zone_dim_interval]
GO
ALTER TABLE [mart].[bridge_time_zone]  WITH CHECK ADD  CONSTRAINT [FK_bridge_time_zone_dim_interval1] FOREIGN KEY([local_interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[bridge_time_zone] CHECK CONSTRAINT [FK_bridge_time_zone_dim_interval1]
GO
ALTER TABLE [mart].[bridge_time_zone]  WITH CHECK ADD  CONSTRAINT [FK_bridge_time_zone_dim_time_zone] FOREIGN KEY([time_zone_id])
REFERENCES [mart].[dim_time_zone] ([time_zone_id])
GO
ALTER TABLE [mart].[bridge_time_zone] CHECK CONSTRAINT [FK_bridge_time_zone_dim_time_zone]
GO
ALTER TABLE [mart].[dim_person]  WITH CHECK ADD  CONSTRAINT [FK_dim_person_dim_skillset] FOREIGN KEY([skillset_id])
REFERENCES [mart].[dim_skillset] ([skillset_id])
GO
ALTER TABLE [mart].[dim_person] CHECK CONSTRAINT [FK_dim_person_dim_skillset]
GO
ALTER TABLE [mart].[dim_site]  WITH CHECK ADD  CONSTRAINT [FK_dim_site_dim_business_unit] FOREIGN KEY([business_unit_id])
REFERENCES [mart].[dim_business_unit] ([business_unit_id])
GO
ALTER TABLE [mart].[dim_site] CHECK CONSTRAINT [FK_dim_site_dim_business_unit]
GO
ALTER TABLE [mart].[dim_team]  WITH CHECK ADD  CONSTRAINT [FK_dim_team_dim_scorecard] FOREIGN KEY([scorecard_id])
REFERENCES [mart].[dim_scorecard] ([scorecard_id])
GO
ALTER TABLE [mart].[dim_team] CHECK CONSTRAINT [FK_dim_team_dim_scorecard]
GO
ALTER TABLE [mart].[dim_team]  WITH CHECK ADD  CONSTRAINT [FK_dim_team_dim_site] FOREIGN KEY([site_id])
REFERENCES [mart].[dim_site] ([site_id])
GO
ALTER TABLE [mart].[dim_team] CHECK CONSTRAINT [FK_dim_team_dim_site]
GO
ALTER TABLE [mart].[etl_job_execution]  WITH CHECK ADD  CONSTRAINT [FK_etl_job_execution_etl_job_definition] FOREIGN KEY([job_id])
REFERENCES [mart].[etl_job] ([job_id])
GO
ALTER TABLE [mart].[etl_job_execution] CHECK CONSTRAINT [FK_etl_job_execution_etl_job_definition]
GO
ALTER TABLE [mart].[etl_job_execution]  WITH CHECK ADD  CONSTRAINT [FK_etl_job_execution_etl_job_schedule] FOREIGN KEY([schedule_id])
REFERENCES [mart].[etl_job_schedule] ([schedule_id])
GO
ALTER TABLE [mart].[etl_job_execution] CHECK CONSTRAINT [FK_etl_job_execution_etl_job_schedule]
GO
ALTER TABLE [mart].[etl_job_schedule_period]  WITH CHECK ADD  CONSTRAINT [FK_etl_job_schedule_period_etl_job] FOREIGN KEY([job_id])
REFERENCES [mart].[etl_job] ([job_id])
GO
ALTER TABLE [mart].[etl_job_schedule_period] CHECK CONSTRAINT [FK_etl_job_schedule_period_etl_job]
GO
ALTER TABLE [mart].[etl_job_schedule_period]  WITH CHECK ADD  CONSTRAINT [FK_etl_job_schedule_period_etl_job_schedule] FOREIGN KEY([schedule_id])
REFERENCES [mart].[etl_job_schedule] ([schedule_id])
GO
ALTER TABLE [mart].[etl_job_schedule_period] CHECK CONSTRAINT [FK_etl_job_schedule_period_etl_job_schedule]
GO
ALTER TABLE [mart].[etl_jobstep_execution]  WITH CHECK ADD  CONSTRAINT [FK_etl_jobstep_execution_etl_job_execution] FOREIGN KEY([job_execution_id])
REFERENCES [mart].[etl_job_execution] ([job_execution_id])
GO
ALTER TABLE [mart].[etl_jobstep_execution] CHECK CONSTRAINT [FK_etl_jobstep_execution_etl_job_execution]
GO
ALTER TABLE [mart].[etl_jobstep_execution]  WITH CHECK ADD  CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_error] FOREIGN KEY([jobstep_error_id])
REFERENCES [mart].[etl_jobstep_error] ([jobstep_error_id])
GO
ALTER TABLE [mart].[etl_jobstep_execution] CHECK CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_error]
GO
ALTER TABLE [mart].[etl_jobstep_execution]  WITH CHECK ADD  CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_execution] FOREIGN KEY([jobstep_id])
REFERENCES [mart].[etl_jobstep] ([jobstep_id])
GO
ALTER TABLE [mart].[etl_jobstep_execution] CHECK CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_execution]
GO
ALTER TABLE [mart].[fact_agent]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_dim_agent] FOREIGN KEY([acd_login_id])
REFERENCES [mart].[dim_acd_login] ([acd_login_id])
GO
ALTER TABLE [mart].[fact_agent] CHECK CONSTRAINT [FK_fact_agent_dim_agent]
GO
ALTER TABLE [mart].[fact_agent]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_dim_date1] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_agent] CHECK CONSTRAINT [FK_fact_agent_dim_date1]
GO
ALTER TABLE [mart].[fact_agent]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_agent] CHECK CONSTRAINT [FK_fact_agent_dim_interval]
GO
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_agent] FOREIGN KEY([acd_login_id])
REFERENCES [mart].[dim_acd_login] ([acd_login_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_agent]
GO
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_date]
GO
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_interval]
GO
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_queue] FOREIGN KEY([queue_id])
REFERENCES [mart].[dim_queue] ([queue_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_queue]
GO
ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_date]
GO
ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_interval]
GO
ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO
ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_scenario]
GO
ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_skill] FOREIGN KEY([skill_id])
REFERENCES [mart].[dim_skill] ([skill_id])
GO
ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_skill]
GO
ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_workload] FOREIGN KEY([workload_id])
REFERENCES [mart].[dim_workload] ([workload_id])
GO
ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_workload]
GO
ALTER TABLE [mart].[fact_kpi_targets_team]  WITH CHECK ADD  CONSTRAINT [FK_fact_kpi_targets_team_dim_business_unit] FOREIGN KEY([business_unit_id])
REFERENCES [mart].[dim_business_unit] ([business_unit_id])
GO
ALTER TABLE [mart].[fact_kpi_targets_team] CHECK CONSTRAINT [FK_fact_kpi_targets_team_dim_business_unit]
GO
ALTER TABLE [mart].[fact_kpi_targets_team]  WITH CHECK ADD  CONSTRAINT [FK_fact_kpi_targets_team_dim_kpi] FOREIGN KEY([kpi_id])
REFERENCES [mart].[dim_kpi] ([kpi_id])
GO
ALTER TABLE [mart].[fact_kpi_targets_team] CHECK CONSTRAINT [FK_fact_kpi_targets_team_dim_kpi]
GO
ALTER TABLE [mart].[fact_kpi_targets_team]  WITH CHECK ADD  CONSTRAINT [FK_fact_kpi_targets_team_dim_team] FOREIGN KEY([team_id])
REFERENCES [mart].[dim_team] ([team_id])
GO
ALTER TABLE [mart].[fact_kpi_targets_team] CHECK CONSTRAINT [FK_fact_kpi_targets_team_dim_team]
GO
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_date]
GO
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_interval]
GO
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_queue] FOREIGN KEY([queue_id])
REFERENCES [mart].[dim_queue] ([queue_id])
GO
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_queue]
GO
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_absence] FOREIGN KEY([absence_id])
REFERENCES [mart].[dim_absence] ([absence_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_absence]
GO
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_activity] FOREIGN KEY([activity_id])
REFERENCES [mart].[dim_activity] ([activity_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_activity]
GO
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date] FOREIGN KEY([schedule_date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date]
GO
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date1] FOREIGN KEY([activity_startdate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date1]
GO
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date2] FOREIGN KEY([activity_enddate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date2]
GO
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date3] FOREIGN KEY([shift_startdate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date3]
GO
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date4] FOREIGN KEY([shift_enddate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date4]
GO
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_interval]
GO
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_interval1] FOREIGN KEY([shift_startinterval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_interval1]
GO
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_person]
GO
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_scenario]
GO
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_absence] FOREIGN KEY([absence_id])
REFERENCES [mart].[dim_absence] ([absence_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_absence]
GO
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_date]
GO
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_day_off] FOREIGN KEY([day_off_id])
REFERENCES [mart].[dim_day_off] ([day_off_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_day_off]
GO
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_interval] FOREIGN KEY([start_interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_interval]
GO
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_person]
GO
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_scenario]
GO
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_shift_category] FOREIGN KEY([shift_category_id])
REFERENCES [mart].[dim_shift_category] ([shift_category_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_shift_category]
GO
ALTER TABLE [mart].[fact_schedule_deviation]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_deviation_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule_deviation] CHECK CONSTRAINT [FK_fact_schedule_deviation_dim_date]
GO
ALTER TABLE [mart].[fact_schedule_deviation]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_deviation_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule_deviation] CHECK CONSTRAINT [FK_fact_schedule_deviation_dim_interval]
GO
ALTER TABLE [mart].[fact_schedule_deviation]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_deviation_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[fact_schedule_deviation] CHECK CONSTRAINT [FK_fact_schedule_deviation_dim_person]
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_forecast_skill_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] CHECK CONSTRAINT [FK_fact_schedule_forecast_skill_dim_date]
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_forecast_skill_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] CHECK CONSTRAINT [FK_fact_schedule_forecast_skill_dim_interval]
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_forecast_skill_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] CHECK CONSTRAINT [FK_fact_schedule_forecast_skill_dim_scenario]
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_forecast_skill_dim_skill] FOREIGN KEY([skill_id])
REFERENCES [mart].[dim_skill] ([skill_id])
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] CHECK CONSTRAINT [FK_fact_schedule_forecast_skill_dim_skill]
GO
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_date]
GO
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_day_off] FOREIGN KEY([day_off_id])
REFERENCES [mart].[dim_day_off] ([day_off_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_day_off]
GO
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_interval]
GO
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_person]
GO
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_preference_type] FOREIGN KEY([preference_type_id])
REFERENCES [mart].[dim_preference_type] ([preference_type_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_preference_type]
GO
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_scenario]
GO
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_shift_category] FOREIGN KEY([shift_category_id])
REFERENCES [mart].[dim_shift_category] ([shift_category_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_shift_category]
GO
ALTER TABLE [mart].[permission_report]  WITH CHECK ADD  CONSTRAINT [FK_permission_report_data_report] FOREIGN KEY([report_id])
REFERENCES [mart].[report] ([report_id])
GO
ALTER TABLE [mart].[permission_report] CHECK CONSTRAINT [FK_permission_report_data_report]
GO
ALTER TABLE [mart].[report]  WITH CHECK ADD  CONSTRAINT [FK_report_report_group] FOREIGN KEY([report_group_id])
REFERENCES [mart].[report_group] ([group_id])
GO
ALTER TABLE [mart].[report] CHECK CONSTRAINT [FK_report_report_group]
GO
ALTER TABLE [mart].[report_control_collection]  WITH CHECK ADD  CONSTRAINT [FK_report_control_collection_control] FOREIGN KEY([control_id])
REFERENCES [mart].[report_control] ([control_id])
GO
ALTER TABLE [mart].[report_control_collection] CHECK CONSTRAINT [FK_report_control_collection_control]
GO
ALTER TABLE [mart].[scorecard_kpi]  WITH CHECK ADD  CONSTRAINT [FK_scorecard_kpi_dim_kpi] FOREIGN KEY([kpi_id])
REFERENCES [mart].[dim_kpi] ([kpi_id])
GO
ALTER TABLE [mart].[scorecard_kpi] CHECK CONSTRAINT [FK_scorecard_kpi_dim_kpi]
GO
ALTER TABLE [mart].[scorecard_kpi]  WITH CHECK ADD  CONSTRAINT [FK_scorecard_kpi_dim_scorecard] FOREIGN KEY([scorecard_id])
REFERENCES [mart].[dim_scorecard] ([scorecard_id])
GO
ALTER TABLE [mart].[scorecard_kpi] CHECK CONSTRAINT [FK_scorecard_kpi_dim_scorecard]
GO
ALTER TABLE [mart].[sys_crossdatabaseview]  WITH CHECK ADD  CONSTRAINT [fk_sys_crossdatabaseview_sys_crossdatabaseview_target] FOREIGN KEY([target_id])
REFERENCES [mart].[sys_crossdatabaseview_target] ([target_id])
GO
ALTER TABLE [mart].[sys_crossdatabaseview] CHECK CONSTRAINT [fk_sys_crossdatabaseview_sys_crossdatabaseview_target]
GO
SET NOCOUNT OFF

-- ==============================================
--Agg
-- ==============================================
CREATE TABLE [dbo].[acd_type](
	[acd_type_id] [int] NOT NULL,
	[acd_type_desc] [varchar](50) NOT NULL,
 CONSTRAINT [PK_acd_type] PRIMARY KEY CLUSTERED 
(
	[acd_type_id] ASC
)
)
--
CREATE TABLE [dbo].[log_object](
	[log_object_id] [int] NOT NULL,
	[acd_type_id] [int] NOT NULL,
	[log_object_desc] [varchar](50) NOT NULL,
	[logDB_name] [varchar](50) NOT NULL,
	[intervals_per_day] [int] NOT NULL,
	[default_service_level_sec] [int] NULL,
	[default_short_call_treshold] [int] NULL,
 CONSTRAINT [PK_log_object] PRIMARY KEY CLUSTERED 
(
	[log_object_id] ASC
)
)

ALTER TABLE [dbo].[log_object]  WITH CHECK ADD  CONSTRAINT [FK_log_object_acd_type] FOREIGN KEY([acd_type_id])
REFERENCES [dbo].[acd_type] ([acd_type_id])
GO
ALTER TABLE [dbo].[log_object] CHECK CONSTRAINT [FK_log_object_acd_type]
--
CREATE TABLE [dbo].[agent_info](
	[Agent_id] [int] NOT NULL,
	[Agent_name] [varchar](50) NOT NULL,
	[is_active] [bit] NULL,
	[log_object_id] [int] NOT NULL,
	[orig_agent_id] [varchar](50) NOT NULL,
 CONSTRAINT [PK_agent_info] PRIMARY KEY CLUSTERED 
(
	[Agent_id] ASC
)
)

ALTER TABLE [dbo].[agent_info]  WITH CHECK ADD  CONSTRAINT [FK_agent_info_log_object] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])
GO
ALTER TABLE [dbo].[agent_info] CHECK CONSTRAINT [FK_agent_info_log_object]
--
CREATE TABLE [dbo].[ccc_system_info](
	[id] [int] NOT NULL,
	[desc] [varchar](50) NOT NULL,
	[int_value] [int] NULL,
	[varchar_value] [char](10) NULL,
 CONSTRAINT [PK_ccc_system_info] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
)
--
CREATE TABLE [dbo].[queues](
	[queue] [int] NOT NULL,
	[orig_desc] [varchar](50) NULL,
	[log_object_id] [int] NOT NULL,
	[orig_queue_id] [int] NULL,
	[display_desc] [varchar](50) NULL,
 CONSTRAINT [PK_queues] PRIMARY KEY CLUSTERED 
(
	[queue] ASC
)
)
ALTER TABLE [dbo].[queues]  WITH CHECK ADD  CONSTRAINT [FK_queues_log_object] FOREIGN KEY([log_object_id])
REFERENCES [dbo].[log_object] ([log_object_id])
GO
ALTER TABLE [dbo].[queues] CHECK CONSTRAINT [FK_queues_log_object]
--
CREATE TABLE [dbo].[agent_logg](
	[queue] [int] NOT NULL,
	[date_from] [smalldatetime] NOT NULL,
	[interval] [int] NOT NULL,
	[agent_id] [int] NOT NULL,
	[agent_name] [nvarchar](50) NULL,
	[avail_dur] [int] NULL,
	[tot_work_dur] [int] NULL,
	[talking_call_dur] [int] NULL,
	[pause_dur] [int] NULL,
	[wait_dur] [int] NULL,
	[wrap_up_dur] [int] NULL,
	[answ_call_cnt] [int] NULL,
	[direct_out_call_cnt] [int] NULL,
	[direct_out_call_dur] [int] NULL,
	[direct_in_call_cnt] [int] NULL,
	[direct_in_call_dur] [int] NULL,
	[transfer_out_call_cnt] [int] NULL,
	[admin_dur] [int] NULL,
 CONSTRAINT [PK_agent_logg] PRIMARY KEY CLUSTERED 
(
	[queue] ASC,
	[date_from] ASC,
	[interval] ASC,
	[agent_id] ASC
)
)

GO
ALTER TABLE [dbo].[agent_logg]  WITH CHECK ADD  CONSTRAINT [FK_agent_logg_agent_info] FOREIGN KEY([agent_id])
REFERENCES [dbo].[agent_info] ([Agent_id])
GO
ALTER TABLE [dbo].[agent_logg] CHECK CONSTRAINT [FK_agent_logg_agent_info]
GO
ALTER TABLE [dbo].[agent_logg]  WITH CHECK ADD  CONSTRAINT [FK_agent_logg_queues] FOREIGN KEY([queue])
REFERENCES [dbo].[queues] ([queue])
GO
ALTER TABLE [dbo].[agent_logg] CHECK CONSTRAINT [FK_agent_logg_queues]
--
CREATE TABLE [dbo].[queue_logg](
	[queue] [int] NOT NULL,
	[date_from] [smalldatetime] NOT NULL,
	[interval] [int] NOT NULL,
	[offd_direct_call_cnt] [int] NULL,
	[overflow_in_call_cnt] [int] NULL,
	[aband_call_cnt] [int] NULL,
	[overflow_out_call_cnt] [int] NULL,
	[answ_call_cnt] [int] NULL,
	[queued_and_answ_call_dur] [int] NULL,
	[queued_and_aband_call_dur] [int] NULL,
	[talking_call_dur] [int] NULL,
	[wrap_up_dur] [int] NULL,
	[queued_answ_longest_que_dur] [int] NULL,
	[queued_aband_longest_que_dur] [int] NULL,
	[avg_avail_member_cnt] [int] NULL,
	[ans_servicelevel_cnt] [int] NULL,
	[wait_dur] [int] NULL,
	[aband_short_call_cnt] [int] NULL,
	[aband_within_sl_cnt] [int] NULL,
 CONSTRAINT [PK_queue_logg] PRIMARY KEY CLUSTERED 
(
	[queue] ASC,
	[date_from] ASC,
	[interval] ASC
)
)

GO
ALTER TABLE [dbo].[queue_logg]  WITH CHECK ADD  CONSTRAINT [FK_queue_logg_queues] FOREIGN KEY([queue])
REFERENCES [dbo].[queues] ([queue])
GO
ALTER TABLE [dbo].[queue_logg] CHECK CONSTRAINT [FK_queue_logg_queues]
GO

----------------  
--Name: Anders F  
--Date: 2009-03-20  
--Desc: Correct initial data for new aggs  
----------------  
SET NOCOUNT ON
INSERT INTO acd_type
VALUES (0,'Default')

INSERT INTO acd_type
VALUES (1,'Avaya Definity CMS')

INSERT INTO acd_type
VALUES (2,'Nortel Symposium 3-4.0 Skillset')

INSERT INTO acd_type
VALUES (3,'Nortel Symposium 1.5 Skillset')

INSERT INTO acd_type
VALUES (4,'Nortel Symposium 3-4.0 Application')

INSERT INTO acd_type
VALUES (5,'Nortel Symposium 1.5 Application')

INSERT INTO acd_type
VALUES (6,'Siemens ProCenter Advanced')

INSERT INTO acd_type
VALUES (7,'Siemens ProCenter Entry')

INSERT INTO acd_type
VALUES (8,'Ericsson Solidus E-Care')

INSERT INTO acd_type
VALUES (9,'Ericsson CCM')

INSERT INTO acd_type
VALUES (10,'Interactive Intelligence Interaction center')

INSERT INTO acd_type
VALUES (11,'Telia VCC 7.5')

INSERT INTO acd_type
VALUES (12,'ClearIT MCC')

INSERT INTO acd_type
VALUES (13,'WebTrump - ccBridge')

INSERT INTO acd_type
VALUES (14,'Nokia DX200')

INSERT INTO acd_type
VALUES (15,'Cisco ICM5')

INSERT INTO acd_type
VALUES (16,'Advoco')

INSERT INTO acd_type
VALUES (17,'TeliaSonera CallGuide 4')

INSERT INTO acd_type
VALUES (18,'Alcatel')

INSERT INTO acd_type
VALUES (19,'Telia VCC 7.6')

INSERT INTO acd_type
VALUES (20,'Telia VCC 8')

INSERT INTO acd_type
VALUES (21,'CDS')

INSERT INTO acd_type
VALUES (22,'Wicom')

INSERT INTO acd_type
VALUES (23,'Altitude 6.2')

INSERT INTO acd_type
VALUES (24,'Wicomrt')

--intervals = 96
INSERT INTO ccc_system_info
VALUES (1, 'CCC intervals per day', 96,NULL)

--A dummy log object
INSERT INTO [dbo].[log_object]
           ([log_object_id]
           ,[acd_type_id]
           ,[log_object_desc]
           ,[logDB_name]
           ,[intervals_per_day]
           ,[default_service_level_sec]
           ,[default_short_call_treshold])
     VALUES
           (1
           ,1
           ,'Default log object'
           ,db_name()
           ,96
           ,20
           ,5)
GO
SET NOCOUNT OFF

--Create stub
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossDatabaseView_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_crossDatabaseView_load]
GO

CREATE PROCEDURE [mart].[sys_crossDatabaseView_load]
AS
SET NOCOUNT ON
RETURN(0)

GO

GO
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (329,'7.1.329') 