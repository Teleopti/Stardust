/* 
BuildTime is: 
2009-02-13 
17:22
*/ 
/*-----------------------------
Adding MSG schema and objects
-----------------------------*/
PRINT 'Adding RTA schema and objects ...'
GO
CREATE SCHEMA RTA AUTHORIZATION [dbo]
GO
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [RTA].[ExternalAgentState](
	[Id] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_ExternalAgentState_Id]  DEFAULT (newid()),
	[LogOn] [nvarchar](50) NOT NULL,
	[StateCode] [nvarchar](50) NOT NULL,
	[TimeInState] [bigint] NOT NULL,
	[TimestampValue] [datetime] NOT NULL,
	[PlatformTypeId] [uniqueidentifier] NULL,
	[LogObjectId] [int] NULL,
	[BatchId] [datetime] NULL,
	[IsSnapshot] [bit] NOT NULL CONSTRAINT [DF_ExternalAgentState_IsSnapshot]  DEFAULT ((0))
) ON [RTA]
GO
/*-----------------------------
Adding MSG schema and objects
-----------------------------*/
PRINT 'Adding MSG schema and objects ...'
GO
CREATE SCHEMA msg AUTHORIZATION [dbo]
GO
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Address](
	[MessageBrokerId] [int] NOT NULL,
	[MulticastAddress] [nvarchar](15) NOT NULL,
	[Port] [int] NOT NULL,
	[Direction] [nvarchar](10) NOT NULL
) ON [MSG]
GO
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [msg].[Configuration](
	[ConfigurationId] [int] NOT NULL,
	[ConfigurationType] [nvarchar](50) NOT NULL,
	[ConfigurationName] [nvarchar](50) NOT NULL,
	[ConfigurationValue] [nvarchar](255) NOT NULL,
	[ConfigurationDataType] [nvarchar](50) NOT NULL,
	[ChangedBy] [nvarchar](1000) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL
) ON [MSG]

GO
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--SET ANSI_PADDING ON
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
	[DomainObjectId] [uniqueidentifier] NOT NULL,
	[DomainObjectType] [nvarchar](255) NOT NULL,
	[DomainUpdateType] [int] NOT NULL,
	[DomainObject] [varbinary](1024) NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL
) ON [MSG]
GO
--SET ANSI_PADDING OFF
GO
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [msg].[Filter](
	[FilterId] [uniqueidentifier] NOT NULL,
	[SubscriberId] [uniqueidentifier] NOT NULL,
	[DomainObjectId] [uniqueidentifier] NOT NULL,
	[DomainObjectType] [nvarchar](255) NOT NULL,
	[EventStartDate] [datetime] NOT NULL,
	[EventEndDate] [datetime] NOT NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL
) ON [MSG]
GO
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [msg].[Heartbeat](
	[HeartbeatId] [uniqueidentifier] NOT NULL,
	[ProcessId] [int] NOT NULL,
	[ChangedBy] [nvarchar](50) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL
) ON [MSG]
GO
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Log](
	[LogId] [uniqueidentifier] NOT NULL,
	[ProcessId] [int] NOT NULL,
	[Description] [text] NOT NULL,
	[Exception] [text] NOT NULL,
	[Message] [text] NOT NULL,
	[StackTrace] [text] NOT NULL,
	[ChangedBy] [text] NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL
) ON [MSG] TEXTIMAGE_ON [MSG]
GO
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Receipt](
	[ReceiptId] [uniqueidentifier] NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	[ProcessId] [int] NOT NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL
) ON [MSG]
GO
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Subscriber](
	[SubscriberId] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[ProcessId] [int] NOT NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL
) ON [MSG]
GO
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [msg].[UpdateType](
	[UpdateType] [int] NOT NULL,
	[UpdateDescription] [nvarchar](15) NOT NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL
) ON [MSG]

--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [msg].[Users](
	[UserId] [int] NOT NULL,
	[Domain] [nvarchar](50) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[ChangedBy] [nvarchar](10) NOT NULL,
	[ChangedDateTime] [datetime] NOT NULL
) ON [MSG]

GO

ALTER TABLE [msg].[Address] ADD  CONSTRAINT [PK_Address] PRIMARY KEY CLUSTERED 
(
	[MessageBrokerId] ASC,
	[MulticastAddress] ASC,
	[Port] ASC,
	[Direction] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

ALTER TABLE [msg].[Configuration] ADD  CONSTRAINT [PK_Configuration] PRIMARY KEY CLUSTERED 
(
	[ConfigurationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

ALTER TABLE [msg].[Event] ADD  CONSTRAINT [PK_Event] PRIMARY KEY NONCLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

ALTER TABLE [msg].[Filter] ADD  CONSTRAINT [PK_Filter] PRIMARY KEY NONCLUSTERED 
(
	[FilterId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

ALTER TABLE [msg].[Heartbeat] ADD  CONSTRAINT [PK_Heartbeat] PRIMARY KEY NONCLUSTERED 
(
	[HeartbeatId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

ALTER TABLE [msg].[Log] ADD  CONSTRAINT [PK_Log] PRIMARY KEY NONCLUSTERED 
(
	[LogId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

ALTER TABLE [msg].[Receipt] ADD  CONSTRAINT [PK_Receipt] PRIMARY KEY NONCLUSTERED 
(
	[ReceiptId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

ALTER TABLE [msg].[Subscriber] ADD  CONSTRAINT [PK_Subscriber] PRIMARY KEY NONCLUSTERED 
(
	[SubscriberId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

ALTER TABLE [msg].[UpdateType] ADD  CONSTRAINT [PK_UpdateType] PRIMARY KEY CLUSTERED 
(
	[UpdateType] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

ALTER TABLE [msg].[Users] ADD  CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]


CREATE CLUSTERED INDEX [IX_Event_ChangedDateTime] ON [msg].[Event] 
(
	[ChangedDateTime] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = ON, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]


CREATE NONCLUSTERED INDEX [IX_Event_ChangedBy] ON [msg].[Event] 
(
	[ChangedBy] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

CREATE CLUSTERED INDEX [IX_Filter_ChangedDateTime] ON [msg].[Filter] 
(
	[ChangedDateTime] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

CREATE NONCLUSTERED INDEX [IX_Filter_ChangedBy] ON [msg].[Filter] 
(
	[ChangedBy] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

CREATE NONCLUSTERED INDEX [IX_Heartbeat_ChangedDateTime] ON [msg].[Heartbeat] 
(
	[ChangedDateTime] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

CREATE NONCLUSTERED INDEX [IX_Heartbeat_ChangedBy] ON [msg].[Heartbeat] 
(
	[ChangedBy] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

CREATE CLUSTERED INDEX [IX_Log_ChangedDateTime] ON [msg].[Log] 
(
	[ChangedDateTime] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

CREATE CLUSTERED INDEX [IX_Receipt_ChangedDateTime] ON [msg].[Receipt] 
(
	[ChangedDateTime] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

CREATE NONCLUSTERED INDEX [IX_Receipt_ChangedBy] ON [msg].[Receipt] 
(
	[ChangedBy] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

CREATE CLUSTERED INDEX [IX_Subscriber_ChangedDateTime] ON [msg].[Subscriber] 
(
	[ChangedDateTime] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

CREATE NONCLUSTERED INDEX [IX_Subscriber_ChangedBy] ON [msg].[Subscriber] 
(
	[ChangedBy] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

CREATE NONCLUSTERED INDEX [IX_Users_ChangedDateTime] ON [msg].[Users] 
(
	[ChangedDateTime] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

CREATE NONCLUSTERED INDEX [IX_Users_ChangedBy] ON [msg].[Users] 
(
	[ChangedBy] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MSG]

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[msg].[vCurrentUsers]'))
DROP VIEW [msg].[vCurrentUsers]
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Configuration_ConfigurationName] ON [msg].[Configuration] 
(
	[ConfigurationName] ASC
)WITH (STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = OFF) ON [MSG]
GO

/*-----------------------------
Adding STAGE schema and objects
-----------------------------*/
PRINT 'Adding STAGE schema and objects ...'
GO
CREATE SCHEMA stage AUTHORIZATION [dbo]
GO
----------------  
--Name: KJ 
--Date: 2009-02-09
--Desc: New schema and moved tables from Stage db 
----------------


/****** Object:  Table [stage].[stg_acd_login_person]    Script Date: 02/06/2009 14:14:32 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_acd_login_person]') AND type in (N'U'))
----BEGIN
CREATE TABLE [stage].[stg_acd_login_person](
	[acd_login_code] [nvarchar](50) NULL,
	[person_code] [uniqueidentifier] NULL,
	[start_date] [smalldatetime] NULL,
	[end_date] [smalldatetime] NULL,
	[log_object_datasource_id] [int] NULL,
	[log_object_name] [nvarchar](50) NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_acd_login_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_acd_login_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL CONSTRAINT [DF_acd_login_datasource_update_date]  DEFAULT (getdate())
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_schedule_day_off_count]    Script Date: 02/06/2009 14:14:34 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_schedule_day_off_count]') AND type in (N'U'))
----BEGIN
CREATE TABLE [stage].[stg_schedule_day_off_count](
	[date] [smalldatetime] NOT NULL,
	[start_interval_id] [smallint] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[starttime] [smalldatetime] NULL,
	[day_off_code] [uniqueidentifier] NULL,
	[day_count] [int] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_stg_schedule_day_off_count_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_stg_schedule_day_off_count_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_schedule_day_off_count_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_schedule_day_off_count] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[start_interval_id] ASC,
	[person_code] ASC,
	[scenario_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_agent_skill]    Script Date: 02/06/2009 14:14:32 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_agent_skill]') AND type in (N'U'))
----BEGIN
CREATE TABLE [stage].[stg_agent_skill](
	[skill_date] [datetime] NOT NULL,
	[interval_id] [int] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[skill_code] [uniqueidentifier] NOT NULL,
	[date_from] [datetime] NULL,
	[date_to] [datetime] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_agent_skill_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_agent_skill_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_stg_agent_skill_1] PRIMARY KEY CLUSTERED 
(
	[skill_date] ASC,
	[interval_id] ASC,
	[person_code] ASC,
	[skill_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_agent_skillset]    Script Date: 02/06/2009 14:14:32 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_agent_skillset]') AND type in (N'U'))
----BEGIN
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_business_unit]    Script Date: 02/06/2009 14:14:32 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_business_unit]') AND type in (N'U'))
----BEGIN
CREATE TABLE [stage].[stg_business_unit](
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_business_unit_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_business_unit_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_business_unit] PRIMARY KEY CLUSTERED 
(
	[business_unit_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_schedule]    Script Date: 02/06/2009 14:14:34 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_schedule]') AND type in (N'U'))
----BEGIN
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
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_stg_schedule_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_schedule_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_schedule_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_stg_schedule] PRIMARY KEY CLUSTERED 
(
	[schedule_date] ASC,
	[person_code] ASC,
	[interval_id] ASC,
	[activity_start] ASC,
	[scenario_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_kpi]    Script Date: 02/06/2009 14:14:33 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_kpi]') AND type in (N'U'))
----BEGIN
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
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL,
	[update_date] [smalldatetime] NOT NULL,
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_kpi] PRIMARY KEY CLUSTERED 
(
	[kpi_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_schedule_preference]    Script Date: 02/06/2009 14:14:35 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_schedule_preference]') AND type in (N'U'))
----BEGIN
CREATE TABLE [stage].[stg_schedule_preference](
	[person_restriction_code] [uniqueidentifier] NOT NULL,
	[restriction_date] [datetime] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[interval_id] [int] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[shift_category_code] [uniqueidentifier] NULL,
	[day_off_code] [uniqueidentifier] NULL,
	[StartTimeMinimum] [bigint] NULL,
	[StartTimeMaximum] [bigint] NULL,
	[endTimeMinimum] [bigint] NULL,
	[endTimeMaximum] [bigint] NULL,
	[WorkTimeMinimum] [bigint] NULL,
	[WorkTimeMaximum] [bigint] NULL,
	[preference_accepted] [int] NULL,
	[preference_declined] [int] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_stg_schedule_preference_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_schedule_preference_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_schedule_preference_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_stg_schedule_preference] PRIMARY KEY CLUSTERED 
(
	[person_restriction_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_kpi_targets_team]    Script Date: 02/06/2009 14:14:33 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_kpi_targets_team]') AND type in (N'U'))
----BEGIN
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_person]    Script Date: 02/06/2009 14:14:34 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_person]') AND type in (N'U'))
----BEGIN
CREATE TABLE [stage].[stg_person](
	[person_code] [uniqueidentifier] NOT NULL,
	[valid_from_date] [smalldatetime] NOT NULL,
	[valid_to_date] [smalldatetime] NOT NULL,
	[person_first_name] [nvarchar](25) NOT NULL,
	[person_last_name] [nvarchar](25) NOT NULL,
	[team_code] [uniqueidentifier] NOT NULL,
	[team_name] [nvarchar](50) NOT NULL,
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
 CONSTRAINT [PK_stg_person] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC,
	[valid_from_date] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_user]    Script Date: 02/06/2009 14:14:35 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_user]') AND type in (N'U'))
----BEGIN
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
	[datasource_id] [smallint] NULL CONSTRAINT [DF_stg_user_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_stg_user_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_user_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_stg_user] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_scenario]    Script Date: 02/06/2009 14:14:34 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_scenario]') AND type in (N'U'))
----BEGIN
CREATE TABLE [stage].[stg_scenario](
	[scenario_code] [uniqueidentifier] NOT NULL,
	[scenario_name] [nvarchar](50) NOT NULL,
	[default_scenario] [bit] NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_stg_scenario_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_scenario_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_scenario_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_stg_scenario] PRIMARY KEY CLUSTERED 
(
	[scenario_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_schedule_forecast_skill]    Script Date: 02/06/2009 14:14:34 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_schedule_forecast_skill]') AND type in (N'U'))
--BEGIN
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
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_forecast_skill_1] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[interval_id] ASC,
	[skill_code] ASC,
	[scenario_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_scorecard]    Script Date: 02/06/2009 14:14:35 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_scorecard]') AND type in (N'U'))
--BEGIN
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_shift_category]    Script Date: 02/06/2009 14:14:35 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_shift_category]') AND type in (N'U'))
--BEGIN
CREATE TABLE [stage].[stg_shift_category](
	[shift_category_code] [uniqueidentifier] NOT NULL,
	[shift_category_name] [nvarchar](100) NOT NULL,
	[shift_category_shortname] [nvarchar](50) NOT NULL,
	[display_color] [int] NOT NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](100) NOT NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_stg_shift_category_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_shift_category_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_shift_category_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_shift_category_datasource_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_stg_shift_category] PRIMARY KEY CLUSTERED 
(
	[shift_category_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_time_zone]    Script Date: 02/06/2009 14:14:35 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_time_zone]') AND type in (N'U'))
--BEGIN
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_time_zone_bridge]    Script Date: 02/06/2009 14:14:35 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_time_zone_bridge]') AND type in (N'U'))
--BEGIN
CREATE TABLE [stage].[stg_time_zone_bridge](
	[date] [smalldatetime] NOT NULL,
	[interval_id] [int] NOT NULL,
	[time_zone_code] [nvarchar](50) NOT NULL,
	[local_date] [smalldatetime] NULL,
	[local_interval_id] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_convert_time_zone] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[interval_id] ASC,
	[time_zone_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_workload]    Script Date: 02/06/2009 14:14:35 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_workload]') AND type in (N'U'))
--BEGIN
CREATE TABLE [stage].[stg_workload](
	[workload_code] [uniqueidentifier] NOT NULL,
	[workload_name] [nvarchar](100) NOT NULL,
	[skill_code] [uniqueidentifier] NULL,
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
 CONSTRAINT [PK_stg_workload] PRIMARY KEY CLUSTERED 
(
	[workload_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_permission_report]    Script Date: 02/06/2009 14:14:33 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_permission_report]') AND type in (N'U'))
--BEGIN
CREATE TABLE [stage].[stg_permission_report](
	[person_code] [uniqueidentifier] NULL,
	[report_id] [int] NULL,
	[team_id] [uniqueidentifier] NULL,
	[my_own] [bit] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_stg_permission_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_stg_permission_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_permission_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_permission_report_datasource_update_date]  DEFAULT (getdate())
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_forecast_workload]    Script Date: 02/06/2009 14:14:33 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_forecast_workload]') AND type in (N'U'))
--BEGIN
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
 CONSTRAINT [PK_stg_forecast] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[interval_id] ASC,
	[start_time] ASC,
	[workload_code] ASC,
	[scenario_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_schedule_day_absence_count]    Script Date: 02/06/2009 14:14:34 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_schedule_day_absence_count]') AND type in (N'U'))
--BEGIN
CREATE TABLE [stage].[stg_schedule_day_absence_count](
	[date] [smalldatetime] NOT NULL,
	[start_interval_id] [smallint] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[scenario_code] [uniqueidentifier] NOT NULL,
	[starttime] [smalldatetime] NULL,
	[absence_code] [uniqueidentifier] NOT NULL,
	[day_count] [int] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_stg_schedule_day_absence_count_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_stg_schedule_day_absence_count_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_schedule_day_absence_count_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_schedule_day_absence_count] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[start_interval_id] ASC,
	[person_code] ASC,
	[scenario_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_contract]    Script Date: 02/06/2009 14:14:33 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_contract]') AND type in (N'U'))
--BEGIN
CREATE TABLE [stage].[stg_contract](
	[date] [smalldatetime] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[scheduled_contract_time_m] [int] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_stg_contract_time] PRIMARY KEY CLUSTERED 
(
	[date] ASC,
	[person_code] ASC,
	[interval_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_queue_workload]    Script Date: 02/06/2009 14:14:34 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--SET ANSI_PADDING ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_queue_workload]') AND type in (N'U'))
--BEGIN
CREATE TABLE [stage].[stg_queue_workload](
	[queue_code] [nvarchar](50) NOT NULL,
	[workload_code] [uniqueidentifier] NOT NULL,
	[log_object_data_source_id] [int] NOT NULL,
	[log_object_name] [varchar](50) NOT NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [int] NOT NULL CONSTRAINT [DF_stg_queue_workload_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_stg_queue_workload_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_queue_workload_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL CONSTRAINT [DF_stg_queue_workload_datasource_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_stg_queue_workload] PRIMARY KEY CLUSTERED 
(
	[queue_code] ASC,
	[workload_code] ASC,
	[log_object_data_source_id] ASC,
	[log_object_name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
--SET ANSI_PADDING OFF
GO
/****** Object:  Table [stage].[stg_date]    Script Date: 02/06/2009 14:14:33 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_date]') AND type in (N'U'))
--BEGIN
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
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_date_inserted]  DEFAULT (getdate()),
 CONSTRAINT [PK_stg_date] PRIMARY KEY CLUSTERED 
(
	[date_date] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_scorecard_kpi]    Script Date: 02/06/2009 14:14:35 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_scorecard_kpi]') AND type in (N'U'))
--BEGIN
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_activity]    Script Date: 02/06/2009 14:14:32 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_activity]') AND type in (N'U'))
--BEGIN
CREATE TABLE [stage].[stg_activity](
	[activity_code] [uniqueidentifier] NOT NULL,
	[activity_name] [nvarchar](50) NOT NULL,
	[display_color] [int] NOT NULL,
	[in_ready_time] [bit] NOT NULL CONSTRAINT [DF_stg_activity_in_ready_time]  DEFAULT ((0)),
	[in_contract_time] [bit] NULL,
	[in_paid_time] [bit] NULL,
	[in_work_time] [bit] NULL,
	[business_unit_code] [uniqueidentifier] NOT NULL,
	[business_unit_name] [nvarchar](50) NOT NULL,
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_activity_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_stg_activity_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_stg_activity] PRIMARY KEY CLUSTERED 
(
	[activity_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO
/****** Object:  Table [stage].[stg_absence]    Script Date: 02/06/2009 14:14:32 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_absence]') AND type in (N'U'))
----BEGIN
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
 CONSTRAINT [PK_stg_absence] PRIMARY KEY CLUSTERED 
(
	[absence_code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [STAGE]
) ON [STAGE]
--END
GO

PRINT 'Adding MART schema and objects ...'
GO
CREATE SCHEMA mart AUTHORIZATION [dbo]
GO
----------------  
--Name: KJ 
--Date: 2009-02-09 
--Desc: New schema "Mart" and moved tables from Stage database 
----------------  
/****** Object:  Table [mart].[etl_job]    Script Date: 02/06/2009 13:41:50 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--SET ANSI_PADDING ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_job]') AND type in (N'U'))
--BEGIN
CREATE TABLE [mart].[etl_job](
	[job_id] [int] NOT NULL,
	[job_name] [varchar](50) NOT NULL,
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_etl_job_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_etl_job_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_etl_job_definition] PRIMARY KEY CLUSTERED 
(
	[job_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART]
--END
GO
--SET ANSI_PADDING OFF
GO
/****** Object:  Table [mart].[etl_jobstep]    Script Date: 02/06/2009 13:41:38 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_jobstep]') AND type in (N'U'))
--BEGIN
CREATE TABLE [mart].[etl_jobstep](
	[jobstep_id] [int] NOT NULL,
	[jobstep_name] [nvarchar](200) NULL,
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_etl_jobstep_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_etl_jobstep_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_etl_jobstep_definition] PRIMARY KEY CLUSTERED 
(
	[jobstep_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART]
--END
GO
/****** Object:  Table [mart].[etl_jobstep_error]    Script Date: 02/06/2009 13:41:36 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_jobstep_error]') AND type in (N'U'))
--BEGIN
CREATE TABLE [mart].[etl_jobstep_error](
	[jobstep_error_id] [int] IDENTITY(1,1) NOT NULL,
	[error_exception_message] [text] NULL,
	[error_exception_inner] [text] NULL,
	[error_exception_stacktrace] [text] NULL,
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_etl_jobstep_error_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_etl_jobstep_error_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_etl_jobstep_error] PRIMARY KEY CLUSTERED 
(
	[jobstep_error_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART] TEXTIMAGE_ON [MART]
--END
GO
/****** Object:  Table [mart].[etl_job_schedule]    Script Date: 02/06/2009 13:41:44 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_job_schedule]') AND type in (N'U'))
--BEGIN
CREATE TABLE [mart].[etl_job_schedule](
	[schedule_id] [int] IDENTITY(1,1) NOT NULL,
	[schedule_name] [nvarchar](150) NULL,
	[enabled] [bit] NOT NULL CONSTRAINT [DF_job_schedule_enabled]  DEFAULT ((1)),
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
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_etl_job_schedule_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_etl_job_schedule_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_etl_job_schedule] PRIMARY KEY CLUSTERED 
(
	[schedule_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART]
--END
GO
/****** Object:  Table [mart].[etl_job_schedule_period]    Script Date: 02/06/2009 13:41:39 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_job_schedule_period]') AND type in (N'U'))
--BEGIN
CREATE TABLE [mart].[etl_job_schedule_period](
	[schedule_id] [int] NOT NULL,
	[job_id] [int] NOT NULL,
	[relative_period_start] [int] NOT NULL,
	[relative_period_end] [int] NOT NULL,
 CONSTRAINT [PK_etl_job_schedule_period] PRIMARY KEY CLUSTERED 
(
	[schedule_id] ASC,
	[job_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART]
--END
GO
/****** Object:  Table [mart].[etl_job_execution]    Script Date: 02/06/2009 13:41:48 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_job_execution]') AND type in (N'U'))
--BEGIN
CREATE TABLE [mart].[etl_job_execution](
	[job_execution_id] [int] IDENTITY(1,1) NOT NULL,
	[job_id] [int] NULL,
	[schedule_id] [int] NULL,
	[job_start_time] [datetime] NULL,
	[job_end_time] [datetime] NULL,
	[duration_s] [int] NULL,
	[affected_rows] [int] NULL,
	[job_execution_success] [bit] NULL,
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_etl_job_execution_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_etl_job_execution_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_etl_job_execution] PRIMARY KEY CLUSTERED 
(
	[job_execution_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART]
--END
GO
/****** Object:  Table [mart].[etl_jobstep_execution]    Script Date: 02/06/2009 13:41:33 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_jobstep_execution]') AND type in (N'U'))
--BEGIN
CREATE TABLE [mart].[etl_jobstep_execution](
	[jobstep_execution_id] [int] IDENTITY(1,1) NOT NULL,
	[duration_s] [int] NULL,
	[rows_affected] [int] NULL,
	[job_execution_id] [int] NULL,
	[jobstep_error_id] [int] NULL,
	[jobstep_id] [int] NULL,
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_etl_jobstep_execution_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_etl_jobstep_execution_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_etl_jobstep_execution] PRIMARY KEY CLUSTERED 
(
	[jobstep_execution_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART]
--END
GO
/****** Object:  ForeignKey [FK_etl_jobstep_execution_etl_job_execution]    Script Date: 02/06/2009 13:41:34 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_etl_jobstep_execution_etl_job_execution]') AND parent_object_id = OBJECT_ID(N'[mart].[etl_jobstep_execution]'))
ALTER TABLE [mart].[etl_jobstep_execution]  WITH CHECK ADD  CONSTRAINT [FK_etl_jobstep_execution_etl_job_execution] FOREIGN KEY([job_execution_id])
REFERENCES [mart].[etl_job_execution] ([job_execution_id])
GO
ALTER TABLE [mart].[etl_jobstep_execution] CHECK CONSTRAINT [FK_etl_jobstep_execution_etl_job_execution]
GO
/****** Object:  ForeignKey [FK_etl_jobstep_execution_etl_jobstep_error]    Script Date: 02/06/2009 13:41:34 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_etl_jobstep_execution_etl_jobstep_error]') AND parent_object_id = OBJECT_ID(N'[mart].[etl_jobstep_execution]'))
ALTER TABLE [mart].[etl_jobstep_execution]  WITH CHECK ADD  CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_error] FOREIGN KEY([jobstep_error_id])
REFERENCES [mart].[etl_jobstep_error] ([jobstep_error_id])
GO
ALTER TABLE [mart].[etl_jobstep_execution] CHECK CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_error]
GO
/****** Object:  ForeignKey [FK_etl_jobstep_execution_etl_jobstep_execution]    Script Date: 02/06/2009 13:41:34 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_etl_jobstep_execution_etl_jobstep_execution]') AND parent_object_id = OBJECT_ID(N'[mart].[etl_jobstep_execution]'))
ALTER TABLE [mart].[etl_jobstep_execution]  WITH CHECK ADD  CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_execution] FOREIGN KEY([jobstep_id])
REFERENCES [mart].[etl_jobstep] ([jobstep_id])
GO
ALTER TABLE [mart].[etl_jobstep_execution] CHECK CONSTRAINT [FK_etl_jobstep_execution_etl_jobstep_execution]
GO
/****** Object:  ForeignKey [FK_etl_job_schedule_period_etl_job]    Script Date: 02/06/2009 13:41:39 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_etl_job_schedule_period_etl_job]') AND parent_object_id = OBJECT_ID(N'[mart].[etl_job_schedule_period]'))
ALTER TABLE [mart].[etl_job_schedule_period]  WITH CHECK ADD  CONSTRAINT [FK_etl_job_schedule_period_etl_job] FOREIGN KEY([job_id])
REFERENCES [mart].[etl_job] ([job_id])
GO
ALTER TABLE [mart].[etl_job_schedule_period] CHECK CONSTRAINT [FK_etl_job_schedule_period_etl_job]
GO
/****** Object:  ForeignKey [FK_etl_job_schedule_period_etl_job_schedule]    Script Date: 02/06/2009 13:41:40 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_etl_job_schedule_period_etl_job_schedule]') AND parent_object_id = OBJECT_ID(N'[mart].[etl_job_schedule_period]'))
ALTER TABLE [mart].[etl_job_schedule_period]  WITH CHECK ADD  CONSTRAINT [FK_etl_job_schedule_period_etl_job_schedule] FOREIGN KEY([schedule_id])
REFERENCES [mart].[etl_job_schedule] ([schedule_id])
GO
ALTER TABLE [mart].[etl_job_schedule_period] CHECK CONSTRAINT [FK_etl_job_schedule_period_etl_job_schedule]
GO
/****** Object:  ForeignKey [FK_etl_job_execution_etl_job_definition]    Script Date: 02/06/2009 13:41:48 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_etl_job_execution_etl_job_definition]') AND parent_object_id = OBJECT_ID(N'[mart].[etl_job_execution]'))
ALTER TABLE [mart].[etl_job_execution]  WITH CHECK ADD  CONSTRAINT [FK_etl_job_execution_etl_job_definition] FOREIGN KEY([job_id])
REFERENCES [mart].[etl_job] ([job_id])
GO
ALTER TABLE [mart].[etl_job_execution] CHECK CONSTRAINT [FK_etl_job_execution_etl_job_definition]
GO
/****** Object:  ForeignKey [FK_etl_job_execution_etl_job_schedule]    Script Date: 02/06/2009 13:41:48 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_etl_job_execution_etl_job_schedule]') AND parent_object_id = OBJECT_ID(N'[mart].[etl_job_execution]'))
ALTER TABLE [mart].[etl_job_execution]  WITH CHECK ADD  CONSTRAINT [FK_etl_job_execution_etl_job_schedule] FOREIGN KEY([schedule_id])
REFERENCES [mart].[etl_job_schedule] ([schedule_id])
GO
ALTER TABLE [mart].[etl_job_execution] CHECK CONSTRAINT [FK_etl_job_execution_etl_job_schedule]
GO
/****** Object:  Table [mart].[sys_datasource]    Script Date: 02/06/2009 13:47:17 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_datasource]') AND type in (N'U'))
--BEGIN
CREATE TABLE [mart].[sys_datasource](
	[datasource_id] [smallint] IDENTITY(1,1) NOT NULL,
	[datasource_name] [nvarchar](100) NULL,
	[log_object_id] [int] NULL CONSTRAINT [DF_sys_datasource_log_object_id]  DEFAULT ((-1)),
	[log_object_name] [nvarchar](100) NULL,
	[datasource_database_id] [int] NULL,
	[datasource_database_name] [nvarchar](100) NULL,
	[datasource_type_name] [nvarchar](100) NULL CONSTRAINT [DF_sys_datasource_datasource_type_name]  DEFAULT ('Not Defined'),
	[time_zone_id] [int] NULL,
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_sys_datasource_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_sys_datasource_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_sys_datasource] PRIMARY KEY CLUSTERED 
(
	[datasource_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART]
--END
GO
/****** Object:  Table [mart].[sys_shift_length_group]    Script Date: 02/06/2009 13:47:19 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_shift_length_group]') AND type in (N'U'))
--BEGIN
CREATE TABLE [mart].[sys_shift_length_group](
	[shift_length_group_id] [int] NOT NULL,
	[shift_length_group_name] [nvarchar](100) NULL,
	[min_length_m] [int] NULL,
	[max_length_m] [int] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK__shift_length_group] PRIMARY KEY CLUSTERED 
(
	[shift_length_group_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART]
--END
GO
/****** Object:  Table [mart].[Reports]    Script Date: 02/06/2009 14:07:45 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--SET ANSI_PADDING OFF
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[Reports]') AND type in (N'U'))
--BEGIN
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
 CONSTRAINT [Reports_PK] PRIMARY KEY CLUSTERED 
(
	[ReportId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART] TEXTIMAGE_ON [MART]
--END
GO
--SET ANSI_PADDING OFF
GO
/****** Object:  Table [mart].[Folders]    Script Date: 02/06/2009 14:07:41 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--SET ANSI_PADDING OFF
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[Folders]') AND type in (N'U'))
--BEGIN
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
 CONSTRAINT [Folders_PK] PRIMARY KEY CLUSTERED 
(
	[FolderId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [MART]
) ON [MART]
--END
GO
--SET ANSI_PADDING OFF
GO


----------------  
--Name: KJ  
--Date: 2009-02-11  
--Desc: New stage table for handling Queue data from File Import  
----------------  
/****** Object:  Table [dbo].[stg_queue]    Script Date: 02/02/2009 10:41:51 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Stage].[stg_queue](
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
) ON [Stage]

GO

----------------  
--Name: KJ  
--Date: 2009-02-11  
--Desc: New schema and moved tables to Mart schema
----------------  
/****** Object:  Table [mart].[dim_preference_type]    Script Date: 02/11/2009 10:25:12 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_preference_type]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_preference_type](
	[preference_type_id] [int] IDENTITY(1,1) NOT NULL,
	[preference_type_name] [nvarchar](50) NOT NULL,
	[term_id] [int] NULL,
 CONSTRAINT [PK_dim_preference_type] PRIMARY KEY CLUSTERED 
(
	[preference_type_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_kpi]    Script Date: 02/11/2009 10:24:57 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_kpi]') AND type in (N'U'))
BEGIN
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
	[decreasing_value_is_positive] [bit] NULL CONSTRAINT [DF_dim_kpi_increasing_value_is_negative]  DEFAULT ((0)),
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_kpi_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_kpi_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_kpi_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_kpi] PRIMARY KEY CLUSTERED 
(
	[kpi_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_acd_login]    Script Date: 02/11/2009 10:24:32 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_acd_login]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_acd_login](
	[acd_login_id] [int] IDENTITY(1,1) NOT NULL,
	[acd_login_agg_id] [int] NULL,
	[acd_login_code] [nvarchar](50) NULL,
	[acd_login_name] [nvarchar](100) NULL,
	[log_object_name] [nvarchar](100) NULL,
	[is_active] [bit] NULL,
	[time_zone_id] [int] NOT NULL CONSTRAINT [DF_dim_agent_time_zone_id]  DEFAULT ((-1)),
	[datasource_id] [smallint] NULL CONSTRAINT [DF_dim_agent_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_dim_agent_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_dim_agent_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL CONSTRAINT [DF_dim_agent_datasource_update_date]  DEFAULT ('1900-01-01'),
 CONSTRAINT [PK_dim_agent] PRIMARY KEY CLUSTERED 
(
	[acd_login_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_business_unit]    Script Date: 02/11/2009 10:24:40 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_business_unit]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_business_unit](
	[business_unit_id] [int] IDENTITY(1,1) NOT NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](100) NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_business_unit_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_business_unit_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_business_unit_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_business_unit] PRIMARY KEY CLUSTERED 
(
	[business_unit_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_date]    Script Date: 02/11/2009 10:24:45 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_date]') AND type in (N'U'))
BEGIN
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
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_date_inserted]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_date] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
--IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_date]') AND name = N'IX_dim_date')
CREATE NONCLUSTERED INDEX [IX_dim_date] ON [mart].[dim_date] 
(
	[date_id] ASC
)
INCLUDE ( [date_date]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
GO
--IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_date]') AND name = N'IX_dim_date_date_date')
CREATE NONCLUSTERED INDEX [IX_dim_date_date_date] ON [mart].[dim_date] 
(
	[date_date] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
GO
/****** Object:  Table [mart].[dim_interval]    Script Date: 02/11/2009 10:24:51 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_interval]') AND type in (N'U'))
BEGIN
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_person_category_type]    Script Date: 02/11/2009 10:25:10 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_person_category_type]') AND type in (N'U'))
BEGIN
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_queue]    Script Date: 02/11/2009 10:25:15 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_queue]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_queue](
	[queue_id] [int] IDENTITY(1,1) NOT NULL,
	[queue_agg_id] [int] NULL,
	[queue_code] [nvarchar](50) NULL,
	[queue_name] [nvarchar](100) NOT NULL CONSTRAINT [DF_dim_queue_queue_name]  DEFAULT ('Not Defined'),
	[log_object_name] [nvarchar](100) NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_dim_queue_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_dim_queue_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_dim_queue_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL CONSTRAINT [DF_dim_queue_datasource_update_date]  DEFAULT ('1900-01-01'),
 CONSTRAINT [PK_dim_queue] PRIMARY KEY CLUSTERED 
(
	[queue_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
--IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[dim_queue]') AND name = N'IX_dim_queue')
CREATE NONCLUSTERED INDEX [IX_dim_queue] ON [mart].[dim_queue] 
(
	[queue_code] ASC,
	[queue_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
GO
/****** Object:  Table [mart].[dim_scenario]    Script Date: 02/11/2009 10:25:19 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_scenario]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_scenario](
	[scenario_id] [smallint] IDENTITY(1,1) NOT NULL,
	[scenario_code] [uniqueidentifier] NULL,
	[scenario_name] [nvarchar](50) NOT NULL CONSTRAINT [DF_dim_scenario_scenario_name]  DEFAULT (N'Not Defined'),
	[default_scenario] [bit] NULL,
	[business_unit_id] [int] NULL,
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](50) NOT NULL CONSTRAINT [DF_dim_scenario_business_unit_name]  DEFAULT (N'Not Defined'),
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_scenario_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_scenario_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_scenario_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_scenario] PRIMARY KEY CLUSTERED 
(
	[scenario_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_scorecard]    Script Date: 02/11/2009 10:25:22 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_scorecard]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_scorecard](
	[scorecard_id] [int] IDENTITY(1,1) NOT NULL,
	[scorecard_code] [uniqueidentifier] NULL,
	[scorecard_name] [nvarchar](255) NOT NULL,
	[period] [int] NOT NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_scorecard_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_scorecard_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_scorecard_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_scorecard] PRIMARY KEY CLUSTERED 
(
	[scorecard_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[language]    Script Date: 02/11/2009 10:27:02 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[language]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[language](
	[language_id] [int] NOT NULL,
	[language_name] [nvarchar](100) NULL,
 CONSTRAINT [PK_language] PRIMARY KEY CLUSTERED 
(
	[language_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[language_term]    Script Date: 02/11/2009 10:27:03 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[language_term]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[language_term](
	[term_id] [int] IDENTITY(1,1) NOT NULL,
	[term_name_english] [nvarchar](100) NULL,
 CONSTRAINT [PK_language_term] PRIMARY KEY CLUSTERED 
(
	[term_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[language_translation]    Script Date: 02/11/2009 10:27:04 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[language_translation]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[language_translation](
	[language_id] [int] NOT NULL,
	[term_id] [int] NOT NULL,
	[term_english] [nvarchar](100) NULL,
	[term_language] [nvarchar](100) NULL,
 CONSTRAINT [PK_term_translation] PRIMARY KEY CLUSTERED 
(
	[language_id] ASC,
	[term_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[period_type]    Script Date: 02/11/2009 10:27:06 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[period_type]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[period_type](
	[period_type_id] [int] IDENTITY(1,1) NOT NULL,
	[period_type_name] [nvarchar](100) NULL,
	[term_id] [int] NULL,
 CONSTRAINT [PK_period_type] PRIMARY KEY CLUSTERED 
(
	[period_type_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_shift_category]    Script Date: 02/11/2009 10:25:26 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_shift_category]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_shift_category](
	[shift_category_id] [int] IDENTITY(1,1) NOT NULL,
	[shift_category_code] [uniqueidentifier] NULL,
	[shift_category_name] [nvarchar](100) NOT NULL CONSTRAINT [DF_dim_shift_category_shift_category_name]  DEFAULT ('Not Defined'),
	[shift_category_shortname] [nvarchar](50) NOT NULL CONSTRAINT [DF_dim_shift_category_shift_category_shortname]  DEFAULT ('Not Defined'),
	[display_color] [int] NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_shift_category_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_shift_category_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_shift_category_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_shift_category_datasource_update_date]  DEFAULT ('2059-12-31'),
 CONSTRAINT [PK_dim_shift_category] PRIMARY KEY CLUSTERED 
(
	[shift_category_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_absence]    Script Date: 02/11/2009 10:24:28 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_absence]') AND type in (N'U'))
BEGIN
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
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_absence_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_absence_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_absence_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_absence] PRIMARY KEY CLUSTERED 
(
	[absence_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_shift_length]    Script Date: 02/11/2009 10:25:29 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_shift_length]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_shift_length](
	[shift_length_id] [int] IDENTITY(1,1) NOT NULL,
	[shift_length_m] [int] NOT NULL CONSTRAINT [DF_dim_shift_length_shift_length_m]  DEFAULT ((-1)),
	[shift_length_h] [decimal](10, 2) NULL CONSTRAINT [DF_dim_shift_length_shift_length_h]  DEFAULT ((0)),
	[shift_length_group_id] [int] NULL CONSTRAINT [DF_dim_shift_length_shift_length_group_id]  DEFAULT ((-1)),
	[shift_length_group_name] [nvarchar](100) NULL CONSTRAINT [DF_dim_shift_length_shift_length_group_name]  DEFAULT (N'Not Defined'),
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_shift_length_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_shift_length_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_dim_shift_length_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_shift_length] PRIMARY KEY CLUSTERED 
(
	[shift_length_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_activity]    Script Date: 02/11/2009 10:24:38 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_activity]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_activity](
	[activity_id] [int] IDENTITY(1,1) NOT NULL,
	[activity_code] [uniqueidentifier] NULL,
	[activity_name] [nvarchar](100) NOT NULL CONSTRAINT [DF_dim_activity_activity_name]  DEFAULT ('Not Defined'),
	[display_color] [int] NOT NULL CONSTRAINT [DF_dim_activity_display_color]  DEFAULT ((-1)),
	[in_ready_time] [bit] NOT NULL CONSTRAINT [DF_dim_activity_in_ready_time]  DEFAULT ((0)),
	[in_ready_time_name] [nvarchar](50) NULL,
	[in_contract_time] [bit] NULL,
	[in_contract_time_name] [nvarchar](50) NULL,
	[in_paid_time] [bit] NULL,
	[in_paid_time_name] [nvarchar](50) NULL,
	[in_work_time] [bit] NULL,
	[in_work_time_name] [nvarchar](50) NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_activity_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_activity_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_activity_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_activity] PRIMARY KEY CLUSTERED 
(
	[activity_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[report_group]    Script Date: 02/11/2009 10:27:20 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_group]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[report_group](
	[group_id] [int] NOT NULL,
	[group_name] [nvarchar](500) NOT NULL,
	[group_name_resource_key] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_report_group] PRIMARY KEY CLUSTERED 
(
	[group_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[report_user_setting]    Script Date: 02/11/2009 10:27:22 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--SET ANSI_PADDING ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_user_setting]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[report_user_setting](
	[person_code] [uniqueidentifier] NOT NULL,
	[report_id] [int] NOT NULL,
	[param_name] [varchar](50) NOT NULL,
	[saved_name_id] [int] NOT NULL,
	[control_setting] [nvarchar](4000) NULL,
 CONSTRAINT [PK_report_user_setting] PRIMARY KEY CLUSTERED 
(
	[person_code] ASC,
	[report_id] ASC,
	[param_name] ASC,
	[saved_name_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
--SET ANSI_PADDING OFF
GO
/****** Object:  Table [mart].[service_level_calculation]    Script Date: 02/11/2009 10:27:26 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[service_level_calculation]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[service_level_calculation](
	[service_level_id] [int] IDENTITY(1,1) NOT NULL,
	[service_level_name] [nvarchar](100) NULL,
	[term_id] [int] NULL,
 CONSTRAINT [PK_service_level_calculation] PRIMARY KEY CLUSTERED 
(
	[service_level_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[report_control]    Script Date: 02/11/2009 10:27:15 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--SET ANSI_PADDING ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[report_control](
	[control_id] [int] NOT NULL,
	[control_name] [varchar](20) NOT NULL,
	[fill_proc_name] [varchar](200) NOT NULL,
	[attribute_id] [int] NULL,
 CONSTRAINT [PK_report_controls] PRIMARY KEY CLUSTERED 
(
	[control_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
--SET ANSI_PADDING OFF
GO
/****** Object:  Table [mart].[dim_skill]    Script Date: 02/11/2009 10:25:36 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_skill]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_skill](
	[skill_id] [int] IDENTITY(1,1) NOT NULL,
	[skill_code] [uniqueidentifier] NULL,
	[skill_name] [nvarchar](100) NOT NULL CONSTRAINT [DF_dim_skill_name]  DEFAULT ('Not Defined'''),
	[time_zone_id] [int] NOT NULL CONSTRAINT [DF_dim_skill_time_zone_id]  DEFAULT ((-1)),
	[forecast_method_code] [uniqueidentifier] NULL,
	[forecast_method_name] [nvarchar](100) NOT NULL CONSTRAINT [DF_dim_skill_forecast_method_name]  DEFAULT ('Not Defined'),
	[business_unit_id] [int] NULL CONSTRAINT [DF_dim_skill_business_unit_id]  DEFAULT ((-1)),
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_skill_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_skill_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_skill_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [datetime] NOT NULL CONSTRAINT [DF_dim_skill_datasource_update_date]  DEFAULT ('1900-01-01'),
 CONSTRAINT [PK_dim_skill] PRIMARY KEY CLUSTERED 
(
	[skill_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[adherence_calculation]    Script Date: 02/11/2009 10:23:56 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[adherence_calculation]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[adherence_calculation](
	[adherence_id] [int] IDENTITY(1,1) NOT NULL,
	[adherence_name] [nvarchar](100) NULL,
	[term_id] [int] NULL,
 CONSTRAINT [PK_adherence_calculation] PRIMARY KEY CLUSTERED 
(
	[adherence_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_day_off]    Script Date: 02/11/2009 10:24:48 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_day_off]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_day_off](
	[day_off_id] [int] IDENTITY(1,1) NOT NULL,
	[day_off_code] [uniqueidentifier] NULL,
	[day_off_name] [nvarchar](50) NOT NULL,
	[display_color] [int] NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_day_off_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_day_off_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_day_off_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_day_off] PRIMARY KEY CLUSTERED 
(
	[day_off_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_time_zone]    Script Date: 02/11/2009 10:25:46 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_time_zone]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_time_zone](
	[time_zone_id] [smallint] IDENTITY(1,1) NOT NULL,
	[time_zone_code] [nvarchar](50) NOT NULL,
	[time_zone_name] [nvarchar](100) NOT NULL,
	[default_zone] [bit] NULL,
	[utc_conversion] [int] NULL,
	[utc_conversion_dst] [int] NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_dim_time_zone_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_dim_time_zone_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_dim_time_zone_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_dim_time_zone] PRIMARY KEY CLUSTERED 
(
	[time_zone_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_skillset]    Script Date: 02/11/2009 10:25:39 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_skillset]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_skillset](
	[skillset_id] [int] IDENTITY(1,1) NOT NULL,
	[skillset_code] [nvarchar](4000) NOT NULL,
	[skillset_name] [nvarchar](4000) NOT NULL,
	[business_unit_id] [int] NULL CONSTRAINT [DF_dim_skillset_business_unit_id]  DEFAULT ((-1)),
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_skillset_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_skillset_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_skillset_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [datetime] NOT NULL CONSTRAINT [DF_dim_skillset_datasource_update_date]  DEFAULT ('1900-01-01'),
 CONSTRAINT [PK_dim_skillset] PRIMARY KEY CLUSTERED 
(
	[skillset_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[sys_crossdatabaseview_target]    Script Date: 02/11/2009 10:27:29 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--SET ANSI_PADDING OFF
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossdatabaseview_target]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[sys_crossdatabaseview_target](
	[target_id] [int] NOT NULL,
	[target_customName] [varchar](100) NOT NULL,
	[target_defaultName] [varchar](100) NOT NULL,
	[confirmed] [bit] NOT NULL,
 CONSTRAINT [PK_sys_crossdatabaseview_target] PRIMARY KEY CLUSTERED 
(
	[target_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
--SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[aspnet_applications]    Script Date: 02/11/2009 10:23:58 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[aspnet_applications]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[aspnet_applications](
	[ApplicationName] [nvarchar](256) NOT NULL,
	[LoweredApplicationName] [nvarchar](256) NOT NULL,
	[ApplicationId] [uniqueidentifier] NOT NULL DEFAULT (newid()),
	[Description] [nvarchar](256) NULL,
 CONSTRAINT [PK_aspnet_applications] PRIMARY KEY NONCLUSTERED 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart],
 CONSTRAINT [UQ_aspnet_applications_ApplicationName] UNIQUE NONCLUSTERED 
(
	[ApplicationName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart],
 CONSTRAINT [UQ_aspnet_applications_LoweredApplicationName] UNIQUE NONCLUSTERED 
(
	[LoweredApplicationName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_workload]    Script Date: 02/11/2009 10:25:51 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_workload]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_workload](
	[workload_id] [int] IDENTITY(1,1) NOT NULL,
	[workload_code] [uniqueidentifier] NULL,
	[workload_name] [nvarchar](100) NOT NULL CONSTRAINT [DF_dim_workload_workload_name]  DEFAULT ('Not Defined'),
	[skill_id] [int] NULL CONSTRAINT [DF_dim_workload_skill_id]  DEFAULT ((-1)),
	[skill_code] [uniqueidentifier] NULL,
	[skill_name] [nvarchar](100) NOT NULL CONSTRAINT [DF_dim_workload_skill_name]  DEFAULT ('Not Defined'),
	[time_zone_id] [int] NOT NULL CONSTRAINT [DF_dim_workload_time_zone_id]  DEFAULT ((-1)),
	[forecast_method_code] [uniqueidentifier] NULL,
	[forecast_method_name] [nvarchar](100) NOT NULL CONSTRAINT [DF_dim_workload_forecast_method_name]  DEFAULT ('Not Defined'),
	[business_unit_id] [int] NULL CONSTRAINT [DF_dim_workload_business_unit_id]  DEFAULT ((-1)),
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_workload_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_workload_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_workload_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [datetime] NOT NULL CONSTRAINT [DF_dim_workload_datasource_update_date]  DEFAULT ('1900-01-01'),
 CONSTRAINT [PK_dim_workload] PRIMARY KEY CLUSTERED 
(
	[workload_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [dbo].[aspnet_SchemaVersions]    Script Date: 02/11/2009 10:24:06 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[aspnet_SchemaVersions]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[aspnet_SchemaVersions](
	[Feature] [nvarchar](128) NOT NULL,
	[CompatibleSchemaVersion] [nvarchar](128) NOT NULL,
	[IsCurrentVersion] [bit] NOT NULL,
 CONSTRAINT [PK_aspnet_SchemaVersions] PRIMARY KEY CLUSTERED 
(
	[Feature] ASC,
	[CompatibleSchemaVersion] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[fact_kpi_targets_team]    Script Date: 02/11/2009 10:26:14 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_kpi_targets_team]') AND type in (N'U'))
BEGIN
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
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_kpi_targets_team_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_kpi_targets_team_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_kpi_targets_team_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_kpi_targets_team_1] PRIMARY KEY CLUSTERED 
(
	[kpi_id] ASC,
	[team_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[scorecard_kpi]    Script Date: 02/11/2009 10:27:25 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[scorecard_kpi]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[scorecard_kpi](
	[scorecard_id] [int] NOT NULL,
	[kpi_id] [int] NOT NULL,
	[business_unit_id] [int] NOT NULL CONSTRAINT [DF_scorecard_kpi_business_unit_id]  DEFAULT ((-1)),
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_scorecard_kpi_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_scorecard_kpi_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_scorecard_kpi_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_scorecard_kpi] PRIMARY KEY CLUSTERED 
(
	[scorecard_id] ASC,
	[kpi_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[bridge_acd_login_person]    Script Date: 02/11/2009 10:24:13 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[bridge_acd_login_person]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[bridge_acd_login_person](
	[acd_login_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[team_id] [int] NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_bridge_acd_login_person_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_bridge_acd_login_person_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_bridge_acd_login_person_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_bridge_acd_login_person] PRIMARY KEY CLUSTERED 
(
	[acd_login_id] ASC,
	[person_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[fact_agent_queue]    Script Date: 02/11/2009 10:25:56 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_agent_queue]') AND type in (N'U'))
BEGIN
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
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_agent_queue_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_agent_queue_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_agent_queue_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_agent_queue] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[queue_id] ASC,
	[acd_login_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [Mart].[fact_agent]    Script Date: 02/11/2009 10:27:35 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[fact_agent]') AND type in (N'U'))
BEGIN
CREATE TABLE [Mart].[fact_agent](
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
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_agent_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_agent_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_agent_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_agent] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[acd_login_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[fact_schedule_deviation]    Script Date: 02/11/2009 10:26:48 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule_deviation]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[fact_schedule_deviation](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[person_id] [int] NOT NULL,
	[scheduled_ready_time_m] [int] NULL,
	[ready_time_m] [int] NULL,
	[contract_time_m] [int] NULL,
	[deviation_schedule_m] [decimal](18, 4) NULL,
	[deviation_schedule_ready_m] [decimal](18, 4) NULL,
	[deviation_contract_m] [decimal](18, 4) NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_fact_schedule_deviation_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_deviation_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_deviation_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_schedule_deviation] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[person_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[fact_contract]    Script Date: 02/11/2009 10:26:00 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_contract]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[fact_contract](
	[date_id] [int] NOT NULL CONSTRAINT [DF_fact_contract_time_schedule_date_id]  DEFAULT ((-1)),
	[person_id] [int] NOT NULL CONSTRAINT [DF_fact_contract_time_person_id]  DEFAULT ((-1)),
	[interval_id] [smallint] NOT NULL CONSTRAINT [DF_fact_contract_time_interval_id]  DEFAULT ((-1)),
	[scheduled_contract_time_m] [int] NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_fact_contract_time_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_fact_contract_time_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_fact_contract_time_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_contract_time] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[person_id] ASC,
	[interval_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[fact_schedule]    Script Date: 02/11/2009 10:26:36 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[fact_schedule](
	[schedule_date_id] [int] NOT NULL CONSTRAINT [DF_fact_schedule_schedule_date_id]  DEFAULT ((-1)),
	[person_id] [int] NOT NULL CONSTRAINT [DF_fact_schedule_person_id]  DEFAULT ((-1)),
	[interval_id] [smallint] NOT NULL CONSTRAINT [DF_fact_schedule_interval_id]  DEFAULT ((-1)),
	[activity_starttime] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_schedule_activity_starttime]  DEFAULT (((1900)-(1))-(1)),
	[scenario_id] [smallint] NOT NULL CONSTRAINT [DF_fact_schedule_scenario_id]  DEFAULT ((-1)),
	[activity_id] [int] NULL CONSTRAINT [DF_fact_schedule_activity_id]  DEFAULT ((-1)),
	[absence_id] [int] NULL CONSTRAINT [DF_fact_schedule_absence_id]  DEFAULT ((-1)),
	[activity_startdate_id] [int] NULL CONSTRAINT [DF_fact_schedule_activity_startdate_id]  DEFAULT ((-1)),
	[activity_enddate_id] [int] NULL CONSTRAINT [DF_fact_schedule_activity_enddate_id]  DEFAULT ((-1)),
	[activity_endtime] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_activity_endtime]  DEFAULT (((2059)-(12))-(31)),
	[shift_startdate_id] [int] NULL CONSTRAINT [DF_fact_schedule_startdate_id]  DEFAULT ((-1)),
	[shift_starttime] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_starttime]  DEFAULT (((1900)-(1))-(1)),
	[shift_enddate_id] [int] NULL CONSTRAINT [DF_fact_schedule_enddate_id]  DEFAULT ((-1)),
	[shift_endtime] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_endtime]  DEFAULT (((2059)-(12))-(31)),
	[shift_startinterval_id] [smallint] NULL CONSTRAINT [DF_fact_schedule_startinterval_id]  DEFAULT ((-1)),
	[shift_category_id] [int] NULL CONSTRAINT [DF_fact_schedule_category_id]  DEFAULT ((-1)),
	[shift_length_id] [int] NULL CONSTRAINT [DF_fact_schedule_length_id]  DEFAULT ((-1)),
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
	[last_publish] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_last_publish]  DEFAULT (((1900)-(1))-(1)),
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_fact_schedule_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_fact_schedule_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_schedule_1] PRIMARY KEY CLUSTERED 
(
	[schedule_date_id] ASC,
	[person_id] ASC,
	[interval_id] ASC,
	[activity_starttime] ASC,
	[scenario_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[fact_schedule_preference]    Script Date: 02/11/2009 10:27:00 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule_preference]') AND type in (N'U'))
BEGIN
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
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_preferences_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_preferences_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_preferences_update_date]  DEFAULT (getdate()),
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[fact_schedule_day_count]    Script Date: 02/11/2009 10:26:43 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule_day_count]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[fact_schedule_day_count](
	[date_id] [int] NOT NULL CONSTRAINT [DF_fact_schedule_day_count_date_id]  DEFAULT ((-1)),
	[start_interval_id] [smallint] NOT NULL,
	[person_id] [int] NOT NULL CONSTRAINT [DF_fact_schedule_day_count_person_id]  DEFAULT ((-1)),
	[scenario_id] [smallint] NOT NULL CONSTRAINT [DF_fact_schedule_day_count_scenario_id]  DEFAULT ((-1)),
	[starttime] [smalldatetime] NULL,
	[shift_category_id] [int] NOT NULL CONSTRAINT [DF_fact_schedule_day_count_shift_category_id]  DEFAULT ((-1)),
	[day_off_id] [int] NOT NULL,
	[absence_id] [int] NOT NULL CONSTRAINT [DF_fact_schedule_day_count_absence_id]  DEFAULT ((-1)),
	[day_count] [int] NULL,
	[business_unit_id] [int] NOT NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_schedule_day_count_datasource_id]  DEFAULT ((1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_schedule_day_count_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_schedule_day_count_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_schedule_day_count] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[start_interval_id] ASC,
	[person_id] ASC,
	[scenario_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_site]    Script Date: 02/11/2009 10:25:32 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_site]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_site](
	[site_id] [int] IDENTITY(1,1) NOT NULL,
	[site_code] [uniqueidentifier] NULL,
	[site_name] [nvarchar](100) NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_site_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_site_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_site_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_site] PRIMARY KEY CLUSTERED 
(
	[site_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [dbo].[aspnet_Membership]    Script Date: 02/11/2009 10:24:04 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[aspnet_Membership]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[aspnet_Membership](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Password] [nvarchar](128) NOT NULL,
	[PasswordFormat] [int] NOT NULL DEFAULT ((0)),
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
 CONSTRAINT [PK_aspnet_Membership] PRIMARY KEY NONCLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart] TEXTIMAGE_ON [Mart]
END
GO
/****** Object:  Table [mart].[bridge_time_zone]    Script Date: 02/11/2009 10:24:23 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[bridge_time_zone]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[bridge_time_zone](
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NOT NULL,
	[time_zone_id] [smallint] NOT NULL,
	[local_date_id] [int] NULL,
	[local_interval_id] [smallint] NULL,
	[datasource_id] [smallint] NULL,
	[insert_date] [smalldatetime] NULL,
	[update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_convert_time_zone] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[time_zone_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
--IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[mart].[bridge_time_zone]') AND name = N'IX_bridge_time_zone_local_date_id_local_interval_id')
CREATE NONCLUSTERED INDEX [IX_bridge_time_zone_local_date_id_local_interval_id] ON [mart].[bridge_time_zone] 
(
	[local_date_id] ASC
)
INCLUDE ( [local_interval_id]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
GO
/****** Object:  Table [mart].[fact_queue]    Script Date: 02/11/2009 10:26:22 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_queue]') AND type in (N'U'))
BEGIN
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
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_fact_queue_statistics_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_queue_statistics_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_queue_statistics_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_fact_queue_statistics_datasource_update_date]  DEFAULT ('1900-01-01'),
 CONSTRAINT [PK_fact_queue] PRIMARY KEY CLUSTERED 
(
	[queue_id] ASC,
	[interval_id] ASC,
	[date_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[fact_forecast_workload]    Script Date: 02/11/2009 10:26:09 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_forecast_workload]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[fact_forecast_workload](
	[date_id] [int] NOT NULL CONSTRAINT [DF_fact_forecast_startdate_id]  DEFAULT ((-1)),
	[interval_id] [smallint] NOT NULL CONSTRAINT [DF_fact_forecast_interval_id]  DEFAULT ((-1)),
	[start_time] [smalldatetime] NOT NULL,
	[workload_id] [int] NOT NULL CONSTRAINT [DF_fact_forecast_queue_id]  DEFAULT ((-1)),
	[scenario_id] [smallint] NOT NULL CONSTRAINT [DF_fact_forecast_scenario_id]  DEFAULT ((-1)),
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
	[business_unit_id] [int] NULL CONSTRAINT [DF_fact_forecast_workload_business_unit_id]  DEFAULT ((-1)),
	[datasource_id] [smallint] NULL CONSTRAINT [DF_fact_forecast_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_fact_forecast_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_fact_forecast_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_fact_forecast] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[start_time] ASC,
	[workload_id] ASC,
	[scenario_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[fact_schedule_forecast_skill]    Script Date: 02/11/2009 10:26:54 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_schedule_forecast_skill]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[fact_schedule_forecast_skill](
	[date_id] [int] NOT NULL CONSTRAINT [DF_fact_forecast_skill_startdate_id]  DEFAULT ((-1)),
	[interval_id] [smallint] NOT NULL CONSTRAINT [DF_fact_forecast_skill_interval_id]  DEFAULT ((-1)),
	[skill_id] [int] NOT NULL CONSTRAINT [DF_fact_forecast_skill_queue_id]  DEFAULT ((-1)),
	[scenario_id] [smallint] NOT NULL CONSTRAINT [DF_fact_forecast_skill_scenario_id]  DEFAULT ((-1)),
	[forecasted_resources_m] [decimal](28, 4) NULL,
	[forecasted_resources] [decimal](28, 4) NULL,
	[forecasted_resources_incl_shrinkage_m] [decimal](28, 4) NULL,
	[forecasted_resources_incl_shrinkage] [decimal](28, 4) NULL,
	[scheduled_resources_m] [decimal](28, 4) NULL,
	[scheduled_resources] [decimal](28, 4) NULL,
	[intraday_deviation_m] [decimal](28, 4) NULL,
	[relative_difference] [decimal](28, 4) NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_fact_forecast_skill_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_fact_forecast_skill_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_fact_forecast_skill_update_date]  DEFAULT (getdate()),
 CONSTRAINT [PK_fact_schedule_forecast_skill] PRIMARY KEY CLUSTERED 
(
	[date_id] ASC,
	[interval_id] ASC,
	[skill_id] ASC,
	[scenario_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[bridge_queue_workload]    Script Date: 02/11/2009 10:24:17 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[bridge_queue_workload]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[bridge_queue_workload](
	[queue_id] [int] NOT NULL,
	[workload_id] [int] NOT NULL,
	[skill_id] [int] NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_bridge_queue_workload_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_bridge_queue_workload_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_bridge_queue_workload_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL CONSTRAINT [DF_bridge_queue_workload_datasource_update_date]  DEFAULT ('1900-01-01'),
 CONSTRAINT [PK_bridge_queue_workload] PRIMARY KEY CLUSTERED 
(
	[queue_id] ASC,
	[workload_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_team]    Script Date: 02/11/2009 10:25:42 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_team]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_team](
	[team_id] [int] IDENTITY(1,1) NOT NULL,
	[team_code] [uniqueidentifier] NULL,
	[team_name] [nvarchar](100) NULL,
	[site_id] [int] NULL CONSTRAINT [DF_dim_team_site_id]  DEFAULT ((-1)),
	[business_unit_id] [int] NULL CONSTRAINT [DF_dim_team_business_unit_id]  DEFAULT ((-1)),
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_team_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_team_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_team_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_team] PRIMARY KEY CLUSTERED 
(
	[team_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[report]    Script Date: 02/11/2009 10:27:14 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--SET ANSI_PADDING ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[report](
	[report_id] [int] NOT NULL,
	[control_collection_id] [int] NOT NULL,
	[report_group_id] [int] NULL,
	[url] [nvarchar](500) NULL,
	[target] [nvarchar](50) NULL,
	[report_name] [nvarchar](500) NULL,
	[report_name_resource_key] [nvarchar](50) NOT NULL,
	[visible] [bit] NOT NULL CONSTRAINT [DF_report_visible]  DEFAULT ((1)),
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
--SET ANSI_PADDING OFF
GO
/****** Object:  Table [mart].[report_control_collection]    Script Date: 02/11/2009 10:27:19 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--SET ANSI_PADDING ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_control_collection]') AND type in (N'U'))
BEGIN
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
 CONSTRAINT [PK_report_control_collection] PRIMARY KEY NONCLUSTERED 
(
	[control_collection_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
--SET ANSI_PADDING OFF
GO
/****** Object:  Table [mart].[bridge_skillset_skill]    Script Date: 02/11/2009 10:24:20 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[bridge_skillset_skill]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[bridge_skillset_skill](
	[skillset_id] [int] NOT NULL,
	[skill_id] [int] NOT NULL,
	[business_unit_id] [int] NULL,
	[datasource_id] [smallint] NULL CONSTRAINT [DF_bridge_skillset_skill_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NULL CONSTRAINT [DF_bridge_skillset_skill_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NULL CONSTRAINT [DF_bridge_skillset_skill_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL CONSTRAINT [DF_bridge_skillset_skill_datasource_update_date]  DEFAULT ('1900-01-01'),
 CONSTRAINT [PK_bridge_skillset_skill] PRIMARY KEY CLUSTERED 
(
	[skillset_id] ASC,
	[skill_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[dim_person]    Script Date: 02/11/2009 10:25:08 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[dim_person](
	[person_id] [int] IDENTITY(1,1) NOT NULL,
	[person_code] [uniqueidentifier] NULL,
	[valid_from_date] [smalldatetime] NOT NULL,
	[valid_to_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_person_valid_to_date]  DEFAULT (((2059)-(12))-(31)),
	[person_name] [nvarchar](200) NOT NULL CONSTRAINT [DF_dim_person_person_name]  DEFAULT (N'Not Defined'),
	[first_name] [nvarchar](30) NOT NULL CONSTRAINT [DF_dim_person_first_name]  DEFAULT (N'Not Defined'),
	[last_name] [nvarchar](30) NOT NULL CONSTRAINT [DF_dim_person_last_name]  DEFAULT (N'Not Defined'''),
	[employment_number] [nvarchar](50) NULL,
	[employment_type_code] [int] NULL,
	[employment_type_name] [nvarchar](50) NOT NULL CONSTRAINT [DF_dim_person_employment_type_name]  DEFAULT (N'Not Defined'),
	[contract_code] [uniqueidentifier] NULL,
	[contract_name] [nvarchar](50) NULL,
	[parttime_code] [uniqueidentifier] NULL,
	[parttime_percentage] [nvarchar](50) NULL,
	[team_id] [int] NULL CONSTRAINT [DF_dim_person_tema_id]  DEFAULT ((-1)),
	[team_code] [uniqueidentifier] NULL,
	[team_name] [nvarchar](50) NOT NULL CONSTRAINT [DF_dim_person_team_name]  DEFAULT (N'Not Defined'),
	[site_id] [int] NULL CONSTRAINT [DF_dim_person_site_id]  DEFAULT ((-1)),
	[site_code] [uniqueidentifier] NULL,
	[site_name] [nvarchar](50) NOT NULL CONSTRAINT [DF_dim_person_site_name]  DEFAULT (N'Not Defined'),
	[business_unit_id] [int] NULL CONSTRAINT [DF_dim_person_business_unit_id]  DEFAULT ((-1)),
	[business_unit_code] [uniqueidentifier] NULL,
	[business_unit_name] [nvarchar](50) NOT NULL CONSTRAINT [DF_dim_person_business_unit_name]  DEFAULT (N'Not Defined'),
	[skillset_id] [int] NULL,
	[email] [nvarchar](200) NULL,
	[note] [nvarchar](1024) NULL,
	[employment_start_date] [smalldatetime] NULL,
	[employment_end_date] [smalldatetime] NULL,
	[time_zone_id] [int] NULL,
	[is_agent] [bit] NULL,
	[is_user] [bit] NULL,
	[datasource_id] [smallint] NOT NULL CONSTRAINT [DF_dim_person_datasource_id]  DEFAULT ((-1)),
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_person_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_dim_person_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_dim_person] PRIMARY KEY CLUSTERED 
(
	[person_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[sys_crossdatabaseview]    Script Date: 02/11/2009 10:27:27 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--SET ANSI_PADDING OFF
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_crossdatabaseview]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[sys_crossdatabaseview](
	[view_id] [int] IDENTITY(1,1) NOT NULL,
	[view_name] [varchar](100) NOT NULL,
	[view_definition] [varchar](4000) NOT NULL,
	[target_id] [int] NOT NULL,
 CONSTRAINT [pk_sys_crossdatabaseview] PRIMARY KEY CLUSTERED 
(
	[view_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
--SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[aspnet_Users]    Script Date: 02/11/2009 10:24:10 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[aspnet_Users]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[aspnet_Users](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL CONSTRAINT [DF__aspnet_Us__UserI__60DD3190]  DEFAULT (newid()),
	[UserName] [nvarchar](256) NOT NULL,
	[LoweredUserName] [nvarchar](256) NOT NULL,
	[MobileAlias] [nvarchar](16) NULL CONSTRAINT [DF__aspnet_Us__Mobil__61D155C9]  DEFAULT (NULL),
	[IsAnonymous] [bit] NOT NULL CONSTRAINT [DF__aspnet_Us__IsAno__62C57A02]  DEFAULT ((0)),
	[LastActivityDate] [datetime] NOT NULL,
	[AppLoginName] [nvarchar](256) NOT NULL CONSTRAINT [DF_aspnet_Users_AppLoginName]  DEFAULT (''),
	[FirstName] [nvarchar](30) NOT NULL CONSTRAINT [DF_aspnet_Users_FirstName]  DEFAULT (''),
	[LastName] [nvarchar](30) NOT NULL CONSTRAINT [DF_aspnet_Users_LastName]  DEFAULT (''),
	[LanguageId] [int] NOT NULL CONSTRAINT [DF_aspnet_Users_LanguageId]  DEFAULT ((1033)),
	[CultureId] [int] NOT NULL CONSTRAINT [DF_aspnet_Users_UICultureId]  DEFAULT ((1033)),
 CONSTRAINT [PK__aspnet_Users__5EF4E91E] PRIMARY KEY NONCLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  Table [mart].[permission_report]    Script Date: 02/11/2009 10:27:09 ******/
--ANSI_NULLS ON
GO
--SET QUOTED_IDENTIFIER ON
GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[permission_report]') AND type in (N'U'))
BEGIN
CREATE TABLE [mart].[permission_report](
	[report_id] [int] NOT NULL,
	[person_code] [uniqueidentifier] NOT NULL,
	[team_id] [int] NOT NULL,
	[my_own] [bit] NOT NULL,
	[business_unit_id] [int] NOT NULL CONSTRAINT [DF_permission_report_business_unit_id]  DEFAULT ((-1)),
	[datasource_id] [smallint] NOT NULL,
	[insert_date] [smalldatetime] NOT NULL CONSTRAINT [DF_permission_report_data_insert_date]  DEFAULT (getdate()),
	[update_date] [smalldatetime] NOT NULL CONSTRAINT [DF_permission_report_data_update_date]  DEFAULT (getdate()),
	[datasource_update_date] [smalldatetime] NULL,
 CONSTRAINT [PK_permission_report_data_test] PRIMARY KEY CLUSTERED 
(
	[report_id] ASC,
	[person_code] ASC,
	[team_id] ASC,
	[my_own] ASC,
	[business_unit_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [Mart]
) ON [Mart]
END
GO
/****** Object:  ForeignKey [FK__aspnet_Me__Appli__1E6F845E]    Script Date: 02/11/2009 10:24:04 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK__aspnet_Me__Appli__1E6F845E]') AND parent_object_id = OBJECT_ID(N'[dbo].[aspnet_Membership]'))
ALTER TABLE [dbo].[aspnet_Membership]  WITH CHECK ADD FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[aspnet_applications] ([ApplicationId])
GO
/****** Object:  ForeignKey [FK__aspnet_Me__UserI__71139959]    Script Date: 02/11/2009 10:24:05 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK__aspnet_Me__UserI__71139959]') AND parent_object_id = OBJECT_ID(N'[dbo].[aspnet_Membership]'))
ALTER TABLE [dbo].[aspnet_Membership]  WITH CHECK ADD  CONSTRAINT [FK__aspnet_Me__UserI__71139959] FOREIGN KEY([UserId])
REFERENCES [dbo].[aspnet_Users] ([UserId])
GO
ALTER TABLE [dbo].[aspnet_Membership] CHECK CONSTRAINT [FK__aspnet_Me__UserI__71139959]
GO
/****** Object:  ForeignKey [FK__aspnet_Us__Appli__5FE90D57]    Script Date: 02/11/2009 10:24:10 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK__aspnet_Us__Appli__5FE90D57]') AND parent_object_id = OBJECT_ID(N'[dbo].[aspnet_Users]'))
ALTER TABLE [dbo].[aspnet_Users]  WITH CHECK ADD  CONSTRAINT [FK__aspnet_Us__Appli__5FE90D57] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[aspnet_applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[aspnet_Users] CHECK CONSTRAINT [FK__aspnet_Us__Appli__5FE90D57]
GO
/****** Object:  ForeignKey [FK_bridge_acd_login_person_dim_acd_login]    Script Date: 02/11/2009 10:24:13 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_acd_login_person_dim_acd_login]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_acd_login_person]'))
ALTER TABLE [mart].[bridge_acd_login_person]  WITH CHECK ADD  CONSTRAINT [FK_bridge_acd_login_person_dim_acd_login] FOREIGN KEY([acd_login_id])
REFERENCES [mart].[dim_acd_login] ([acd_login_id])
GO
ALTER TABLE [mart].[bridge_acd_login_person] CHECK CONSTRAINT [FK_bridge_acd_login_person_dim_acd_login]
GO
/****** Object:  ForeignKey [FK_bridge_acd_login_person_dim_person]    Script Date: 02/11/2009 10:24:14 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_acd_login_person_dim_person]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_acd_login_person]'))
ALTER TABLE [mart].[bridge_acd_login_person]  WITH CHECK ADD  CONSTRAINT [FK_bridge_acd_login_person_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[bridge_acd_login_person] CHECK CONSTRAINT [FK_bridge_acd_login_person_dim_person]
GO
/****** Object:  ForeignKey [FK_bridge_acd_login_person_dim_team]    Script Date: 02/11/2009 10:24:14 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_acd_login_person_dim_team]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_acd_login_person]'))
ALTER TABLE [mart].[bridge_acd_login_person]  WITH CHECK ADD  CONSTRAINT [FK_bridge_acd_login_person_dim_team] FOREIGN KEY([team_id])
REFERENCES [mart].[dim_team] ([team_id])
GO
ALTER TABLE [mart].[bridge_acd_login_person] CHECK CONSTRAINT [FK_bridge_acd_login_person_dim_team]
GO
/****** Object:  ForeignKey [FK_bridge_queue_workload_dim_queue]    Script Date: 02/11/2009 10:24:17 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_queue_workload_dim_queue]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_queue_workload]'))
ALTER TABLE [mart].[bridge_queue_workload]  WITH CHECK ADD  CONSTRAINT [FK_bridge_queue_workload_dim_queue] FOREIGN KEY([queue_id])
REFERENCES [mart].[dim_queue] ([queue_id])
GO
ALTER TABLE [mart].[bridge_queue_workload] CHECK CONSTRAINT [FK_bridge_queue_workload_dim_queue]
GO
/****** Object:  ForeignKey [FK_bridge_queue_workload_dim_skill]    Script Date: 02/11/2009 10:24:17 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_queue_workload_dim_skill]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_queue_workload]'))
ALTER TABLE [mart].[bridge_queue_workload]  WITH CHECK ADD  CONSTRAINT [FK_bridge_queue_workload_dim_skill] FOREIGN KEY([skill_id])
REFERENCES [mart].[dim_skill] ([skill_id])
GO
ALTER TABLE [mart].[bridge_queue_workload] CHECK CONSTRAINT [FK_bridge_queue_workload_dim_skill]
GO
/****** Object:  ForeignKey [FK_bridge_queue_workload_dim_workload]    Script Date: 02/11/2009 10:24:17 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_queue_workload_dim_workload]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_queue_workload]'))
ALTER TABLE [mart].[bridge_queue_workload]  WITH CHECK ADD  CONSTRAINT [FK_bridge_queue_workload_dim_workload] FOREIGN KEY([workload_id])
REFERENCES [mart].[dim_workload] ([workload_id])
GO
ALTER TABLE [mart].[bridge_queue_workload] CHECK CONSTRAINT [FK_bridge_queue_workload_dim_workload]
GO
/****** Object:  ForeignKey [FK_bridge_skillset_skill_dim_skill]    Script Date: 02/11/2009 10:24:20 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_skillset_skill_dim_skill]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_skillset_skill]'))
ALTER TABLE [mart].[bridge_skillset_skill]  WITH CHECK ADD  CONSTRAINT [FK_bridge_skillset_skill_dim_skill] FOREIGN KEY([skill_id])
REFERENCES [mart].[dim_skill] ([skill_id])
GO
ALTER TABLE [mart].[bridge_skillset_skill] CHECK CONSTRAINT [FK_bridge_skillset_skill_dim_skill]
GO
/****** Object:  ForeignKey [FK_bridge_skillset_skill_dim_skillset]    Script Date: 02/11/2009 10:24:20 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_skillset_skill_dim_skillset]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_skillset_skill]'))
ALTER TABLE [mart].[bridge_skillset_skill]  WITH CHECK ADD  CONSTRAINT [FK_bridge_skillset_skill_dim_skillset] FOREIGN KEY([skillset_id])
REFERENCES [mart].[dim_skillset] ([skillset_id])
GO
ALTER TABLE [mart].[bridge_skillset_skill] CHECK CONSTRAINT [FK_bridge_skillset_skill_dim_skillset]
GO
/****** Object:  ForeignKey [FK_bridge_time_zone_dim_date]    Script Date: 02/11/2009 10:24:23 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_time_zone_dim_date]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_time_zone]'))
ALTER TABLE [mart].[bridge_time_zone]  WITH CHECK ADD  CONSTRAINT [FK_bridge_time_zone_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[bridge_time_zone] CHECK CONSTRAINT [FK_bridge_time_zone_dim_date]
GO
/****** Object:  ForeignKey [FK_bridge_time_zone_dim_date1]    Script Date: 02/11/2009 10:24:23 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_time_zone_dim_date1]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_time_zone]'))
ALTER TABLE [mart].[bridge_time_zone]  WITH CHECK ADD  CONSTRAINT [FK_bridge_time_zone_dim_date1] FOREIGN KEY([local_date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[bridge_time_zone] CHECK CONSTRAINT [FK_bridge_time_zone_dim_date1]
GO
/****** Object:  ForeignKey [FK_bridge_time_zone_dim_interval]    Script Date: 02/11/2009 10:24:23 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_time_zone_dim_interval]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_time_zone]'))
ALTER TABLE [mart].[bridge_time_zone]  WITH CHECK ADD  CONSTRAINT [FK_bridge_time_zone_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[bridge_time_zone] CHECK CONSTRAINT [FK_bridge_time_zone_dim_interval]
GO
/****** Object:  ForeignKey [FK_bridge_time_zone_dim_interval1]    Script Date: 02/11/2009 10:24:24 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_time_zone_dim_interval1]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_time_zone]'))
ALTER TABLE [mart].[bridge_time_zone]  WITH CHECK ADD  CONSTRAINT [FK_bridge_time_zone_dim_interval1] FOREIGN KEY([local_interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[bridge_time_zone] CHECK CONSTRAINT [FK_bridge_time_zone_dim_interval1]
GO
/****** Object:  ForeignKey [FK_bridge_time_zone_dim_time_zone]    Script Date: 02/11/2009 10:24:24 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_bridge_time_zone_dim_time_zone]') AND parent_object_id = OBJECT_ID(N'[mart].[bridge_time_zone]'))
ALTER TABLE [mart].[bridge_time_zone]  WITH CHECK ADD  CONSTRAINT [FK_bridge_time_zone_dim_time_zone] FOREIGN KEY([time_zone_id])
REFERENCES [mart].[dim_time_zone] ([time_zone_id])
GO
ALTER TABLE [mart].[bridge_time_zone] CHECK CONSTRAINT [FK_bridge_time_zone_dim_time_zone]
GO
/****** Object:  ForeignKey [FK_dim_person_dim_skillset]    Script Date: 02/11/2009 10:25:08 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_dim_person_dim_skillset]') AND parent_object_id = OBJECT_ID(N'[mart].[dim_person]'))
ALTER TABLE [mart].[dim_person]  WITH CHECK ADD  CONSTRAINT [FK_dim_person_dim_skillset] FOREIGN KEY([skillset_id])
REFERENCES [mart].[dim_skillset] ([skillset_id])
GO
ALTER TABLE [mart].[dim_person] CHECK CONSTRAINT [FK_dim_person_dim_skillset]
GO
/****** Object:  ForeignKey [FK_dim_site_dim_business_unit]    Script Date: 02/11/2009 10:25:32 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_dim_site_dim_business_unit]') AND parent_object_id = OBJECT_ID(N'[mart].[dim_site]'))
ALTER TABLE [mart].[dim_site]  WITH CHECK ADD  CONSTRAINT [FK_dim_site_dim_business_unit] FOREIGN KEY([business_unit_id])
REFERENCES [mart].[dim_business_unit] ([business_unit_id])
GO
ALTER TABLE [mart].[dim_site] CHECK CONSTRAINT [FK_dim_site_dim_business_unit]
GO
/****** Object:  ForeignKey [FK_dim_team_dim_site]    Script Date: 02/11/2009 10:25:43 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_dim_team_dim_site]') AND parent_object_id = OBJECT_ID(N'[mart].[dim_team]'))
ALTER TABLE [mart].[dim_team]  WITH CHECK ADD  CONSTRAINT [FK_dim_team_dim_site] FOREIGN KEY([site_id])
REFERENCES [mart].[dim_site] ([site_id])
GO
ALTER TABLE [mart].[dim_team] CHECK CONSTRAINT [FK_dim_team_dim_site]
GO
/****** Object:  ForeignKey [FK_fact_agent_queue_dim_agent]    Script Date: 02/11/2009 10:25:56 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_agent_queue_dim_agent]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_agent_queue]'))
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_agent] FOREIGN KEY([acd_login_id])
REFERENCES [mart].[dim_acd_login] ([acd_login_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_agent]
GO
/****** Object:  ForeignKey [FK_fact_agent_queue_dim_date]    Script Date: 02/11/2009 10:25:56 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_agent_queue_dim_date]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_agent_queue]'))
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_date]
GO
/****** Object:  ForeignKey [FK_fact_agent_queue_dim_interval]    Script Date: 02/11/2009 10:25:56 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_agent_queue_dim_interval]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_agent_queue]'))
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_interval]
GO
/****** Object:  ForeignKey [FK_fact_agent_queue_dim_queue]    Script Date: 02/11/2009 10:25:56 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_agent_queue_dim_queue]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_agent_queue]'))
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_queue] FOREIGN KEY([queue_id])
REFERENCES [mart].[dim_queue] ([queue_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_queue]
GO
/****** Object:  ForeignKey [FK_fact_agent_queue_dim_queue1]    Script Date: 02/11/2009 10:25:56 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_agent_queue_dim_queue1]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_agent_queue]'))
ALTER TABLE [mart].[fact_agent_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_queue_dim_queue1] FOREIGN KEY([queue_id])
REFERENCES [mart].[dim_queue] ([queue_id])
GO
ALTER TABLE [mart].[fact_agent_queue] CHECK CONSTRAINT [FK_fact_agent_queue_dim_queue1]
GO
/****** Object:  ForeignKey [FK_fact_contract_time_dim_date]    Script Date: 02/11/2009 10:26:00 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_contract_time_dim_date]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_contract]'))
ALTER TABLE [mart].[fact_contract]  WITH CHECK ADD  CONSTRAINT [FK_fact_contract_time_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_contract] CHECK CONSTRAINT [FK_fact_contract_time_dim_date]
GO
/****** Object:  ForeignKey [FK_fact_contract_time_dim_interval]    Script Date: 02/11/2009 10:26:00 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_contract_time_dim_interval]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_contract]'))
ALTER TABLE [mart].[fact_contract]  WITH CHECK ADD  CONSTRAINT [FK_fact_contract_time_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_contract] CHECK CONSTRAINT [FK_fact_contract_time_dim_interval]
GO
/****** Object:  ForeignKey [FK_fact_contract_time_dim_person]    Script Date: 02/11/2009 10:26:00 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_contract_time_dim_person]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_contract]'))
ALTER TABLE [mart].[fact_contract]  WITH CHECK ADD  CONSTRAINT [FK_fact_contract_time_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[fact_contract] CHECK CONSTRAINT [FK_fact_contract_time_dim_person]
GO
/****** Object:  ForeignKey [FK_fact_forecast_workload_dim_date]    Script Date: 02/11/2009 10:26:10 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_forecast_workload_dim_date]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_forecast_workload]'))
ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_date]
GO
/****** Object:  ForeignKey [FK_fact_forecast_workload_dim_interval]    Script Date: 02/11/2009 10:26:10 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_forecast_workload_dim_interval]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_forecast_workload]'))
ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_interval]
GO
/****** Object:  ForeignKey [FK_fact_forecast_workload_dim_scenario]    Script Date: 02/11/2009 10:26:10 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_forecast_workload_dim_scenario]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_forecast_workload]'))
ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO
ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_scenario]
GO
/****** Object:  ForeignKey [FK_fact_forecast_workload_dim_skill]    Script Date: 02/11/2009 10:26:10 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_forecast_workload_dim_skill]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_forecast_workload]'))
ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_skill] FOREIGN KEY([skill_id])
REFERENCES [mart].[dim_skill] ([skill_id])
GO
ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_skill]
GO
/****** Object:  ForeignKey [FK_fact_forecast_workload_dim_workload]    Script Date: 02/11/2009 10:26:10 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_forecast_workload_dim_workload]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_forecast_workload]'))
ALTER TABLE [mart].[fact_forecast_workload]  WITH CHECK ADD  CONSTRAINT [FK_fact_forecast_workload_dim_workload] FOREIGN KEY([workload_id])
REFERENCES [mart].[dim_workload] ([workload_id])
GO
ALTER TABLE [mart].[fact_forecast_workload] CHECK CONSTRAINT [FK_fact_forecast_workload_dim_workload]
GO
/****** Object:  ForeignKey [FK_fact_kpi_targets_team_dim_business_unit]    Script Date: 02/11/2009 10:26:15 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_kpi_targets_team_dim_business_unit]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_kpi_targets_team]'))
ALTER TABLE [mart].[fact_kpi_targets_team]  WITH CHECK ADD  CONSTRAINT [FK_fact_kpi_targets_team_dim_business_unit] FOREIGN KEY([business_unit_id])
REFERENCES [mart].[dim_business_unit] ([business_unit_id])
GO
ALTER TABLE [mart].[fact_kpi_targets_team] CHECK CONSTRAINT [FK_fact_kpi_targets_team_dim_business_unit]
GO
/****** Object:  ForeignKey [FK_fact_kpi_targets_team_dim_kpi]    Script Date: 02/11/2009 10:26:15 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_kpi_targets_team_dim_kpi]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_kpi_targets_team]'))
ALTER TABLE [mart].[fact_kpi_targets_team]  WITH CHECK ADD  CONSTRAINT [FK_fact_kpi_targets_team_dim_kpi] FOREIGN KEY([kpi_id])
REFERENCES [mart].[dim_kpi] ([kpi_id])
GO
ALTER TABLE [mart].[fact_kpi_targets_team] CHECK CONSTRAINT [FK_fact_kpi_targets_team_dim_kpi]
GO
/****** Object:  ForeignKey [FK_fact_kpi_targets_team_dim_team]    Script Date: 02/11/2009 10:26:15 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_kpi_targets_team_dim_team]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_kpi_targets_team]'))
ALTER TABLE [mart].[fact_kpi_targets_team]  WITH CHECK ADD  CONSTRAINT [FK_fact_kpi_targets_team_dim_team] FOREIGN KEY([team_id])
REFERENCES [mart].[dim_team] ([team_id])
GO
ALTER TABLE [mart].[fact_kpi_targets_team] CHECK CONSTRAINT [FK_fact_kpi_targets_team_dim_team]
GO
/****** Object:  ForeignKey [FK_fact_queue_dim_date]    Script Date: 02/11/2009 10:26:22 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_queue_dim_date]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_queue]'))
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_date]
GO
/****** Object:  ForeignKey [FK_fact_queue_dim_interval]    Script Date: 02/11/2009 10:26:23 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_queue_dim_interval]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_queue]'))
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_interval]
GO
/****** Object:  ForeignKey [FK_fact_queue_dim_queue]    Script Date: 02/11/2009 10:26:23 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_queue_dim_queue]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_queue]'))
ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_queue] FOREIGN KEY([queue_id])
REFERENCES [mart].[dim_queue] ([queue_id])
GO
ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_queue]
GO
/****** Object:  ForeignKey [FK_fact_schedule_dim_absence]    Script Date: 02/11/2009 10:26:36 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_dim_absence]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule]'))
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_absence] FOREIGN KEY([absence_id])
REFERENCES [mart].[dim_absence] ([absence_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_absence]
GO
/****** Object:  ForeignKey [FK_fact_schedule_dim_activity]    Script Date: 02/11/2009 10:26:36 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_dim_activity]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule]'))
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_activity] FOREIGN KEY([activity_id])
REFERENCES [mart].[dim_activity] ([activity_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_activity]
GO
/****** Object:  ForeignKey [FK_fact_schedule_dim_date]    Script Date: 02/11/2009 10:26:36 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_dim_date]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule]'))
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date] FOREIGN KEY([schedule_date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date]
GO
/****** Object:  ForeignKey [FK_fact_schedule_dim_date1]    Script Date: 02/11/2009 10:26:37 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_dim_date1]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule]'))
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date1] FOREIGN KEY([activity_startdate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date1]
GO
/****** Object:  ForeignKey [FK_fact_schedule_dim_date2]    Script Date: 02/11/2009 10:26:37 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_dim_date2]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule]'))
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date2] FOREIGN KEY([activity_enddate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date2]
GO
/****** Object:  ForeignKey [FK_fact_schedule_dim_date3]    Script Date: 02/11/2009 10:26:37 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_dim_date3]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule]'))
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date3] FOREIGN KEY([shift_startdate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date3]
GO
/****** Object:  ForeignKey [FK_fact_schedule_dim_date4]    Script Date: 02/11/2009 10:26:37 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_dim_date4]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule]'))
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_date4] FOREIGN KEY([shift_enddate_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_date4]
GO
/****** Object:  ForeignKey [FK_fact_schedule_dim_interval]    Script Date: 02/11/2009 10:26:37 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_dim_interval]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule]'))
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_interval]
GO
/****** Object:  ForeignKey [FK_fact_schedule_dim_interval1]    Script Date: 02/11/2009 10:26:37 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_dim_interval1]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule]'))
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_interval1] FOREIGN KEY([shift_startinterval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_interval1]
GO
/****** Object:  ForeignKey [FK_fact_schedule_dim_person]    Script Date: 02/11/2009 10:26:38 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_dim_person]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule]'))
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_person]
GO
/****** Object:  ForeignKey [FK_fact_schedule_dim_scenario]    Script Date: 02/11/2009 10:26:38 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_dim_scenario]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule]'))
ALTER TABLE [mart].[fact_schedule]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO
ALTER TABLE [mart].[fact_schedule] CHECK CONSTRAINT [FK_fact_schedule_dim_scenario]
GO
/****** Object:  ForeignKey [FK_fact_schedule_day_count_dim_absence]    Script Date: 02/11/2009 10:26:43 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_day_count_dim_absence]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_day_count]'))
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_absence] FOREIGN KEY([absence_id])
REFERENCES [mart].[dim_absence] ([absence_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_absence]
GO
/****** Object:  ForeignKey [FK_fact_schedule_day_count_dim_date]    Script Date: 02/11/2009 10:26:43 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_day_count_dim_date]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_day_count]'))
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_date]
GO
/****** Object:  ForeignKey [FK_fact_schedule_day_count_dim_day_off]    Script Date: 02/11/2009 10:26:43 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_day_count_dim_day_off]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_day_count]'))
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_day_off] FOREIGN KEY([day_off_id])
REFERENCES [mart].[dim_day_off] ([day_off_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_day_off]
GO
/****** Object:  ForeignKey [FK_fact_schedule_day_count_dim_interval]    Script Date: 02/11/2009 10:26:43 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_day_count_dim_interval]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_day_count]'))
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_interval] FOREIGN KEY([start_interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_interval]
GO
/****** Object:  ForeignKey [FK_fact_schedule_day_count_dim_person]    Script Date: 02/11/2009 10:26:44 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_day_count_dim_person]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_day_count]'))
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_person]
GO
/****** Object:  ForeignKey [FK_fact_schedule_day_count_dim_scenario]    Script Date: 02/11/2009 10:26:44 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_day_count_dim_scenario]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_day_count]'))
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_scenario]
GO
/****** Object:  ForeignKey [FK_fact_schedule_day_count_dim_shift_category]    Script Date: 02/11/2009 10:26:44 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_day_count_dim_shift_category]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_day_count]'))
ALTER TABLE [mart].[fact_schedule_day_count]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_day_count_dim_shift_category] FOREIGN KEY([shift_category_id])
REFERENCES [mart].[dim_shift_category] ([shift_category_id])
GO
ALTER TABLE [mart].[fact_schedule_day_count] CHECK CONSTRAINT [FK_fact_schedule_day_count_dim_shift_category]
GO
/****** Object:  ForeignKey [FK_fact_schedule_deviation_dim_date]    Script Date: 02/11/2009 10:26:49 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_deviation_dim_date]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_deviation]'))
ALTER TABLE [mart].[fact_schedule_deviation]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_deviation_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule_deviation] CHECK CONSTRAINT [FK_fact_schedule_deviation_dim_date]
GO
/****** Object:  ForeignKey [FK_fact_schedule_deviation_dim_interval]    Script Date: 02/11/2009 10:26:49 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_deviation_dim_interval]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_deviation]'))
ALTER TABLE [mart].[fact_schedule_deviation]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_deviation_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule_deviation] CHECK CONSTRAINT [FK_fact_schedule_deviation_dim_interval]
GO
/****** Object:  ForeignKey [FK_fact_schedule_deviation_dim_person]    Script Date: 02/11/2009 10:26:49 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_deviation_dim_person]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_deviation]'))
ALTER TABLE [mart].[fact_schedule_deviation]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_deviation_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[fact_schedule_deviation] CHECK CONSTRAINT [FK_fact_schedule_deviation_dim_person]
GO
/****** Object:  ForeignKey [FK_fact_schedule_forecast_skill_dim_date]    Script Date: 02/11/2009 10:26:55 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_forecast_skill_dim_date]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_forecast_skill]'))
ALTER TABLE [mart].[fact_schedule_forecast_skill]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_forecast_skill_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] CHECK CONSTRAINT [FK_fact_schedule_forecast_skill_dim_date]
GO
/****** Object:  ForeignKey [FK_fact_schedule_forecast_skill_dim_interval]    Script Date: 02/11/2009 10:26:55 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_forecast_skill_dim_interval]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_forecast_skill]'))
ALTER TABLE [mart].[fact_schedule_forecast_skill]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_forecast_skill_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] CHECK CONSTRAINT [FK_fact_schedule_forecast_skill_dim_interval]
GO
/****** Object:  ForeignKey [FK_fact_schedule_forecast_skill_dim_scenario]    Script Date: 02/11/2009 10:26:55 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_forecast_skill_dim_scenario]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_forecast_skill]'))
ALTER TABLE [mart].[fact_schedule_forecast_skill]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_forecast_skill_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] CHECK CONSTRAINT [FK_fact_schedule_forecast_skill_dim_scenario]
GO
/****** Object:  ForeignKey [FK_fact_schedule_forecast_skill_dim_skill]    Script Date: 02/11/2009 10:26:55 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_forecast_skill_dim_skill]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_forecast_skill]'))
ALTER TABLE [mart].[fact_schedule_forecast_skill]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_forecast_skill_dim_skill] FOREIGN KEY([skill_id])
REFERENCES [mart].[dim_skill] ([skill_id])
GO
ALTER TABLE [mart].[fact_schedule_forecast_skill] CHECK CONSTRAINT [FK_fact_schedule_forecast_skill_dim_skill]
GO
/****** Object:  ForeignKey [FK_fact_schedule_preference_dim_date]    Script Date: 02/11/2009 10:27:00 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_preference_dim_date]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_preference]'))
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_date] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_date]
GO
/****** Object:  ForeignKey [FK_fact_schedule_preference_dim_day_off]    Script Date: 02/11/2009 10:27:00 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_preference_dim_day_off]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_preference]'))
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_day_off] FOREIGN KEY([day_off_id])
REFERENCES [mart].[dim_day_off] ([day_off_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_day_off]
GO
/****** Object:  ForeignKey [FK_fact_schedule_preference_dim_interval]    Script Date: 02/11/2009 10:27:01 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_preference_dim_interval]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_preference]'))
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_interval]
GO
/****** Object:  ForeignKey [FK_fact_schedule_preference_dim_person]    Script Date: 02/11/2009 10:27:01 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_preference_dim_person]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_preference]'))
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_person] FOREIGN KEY([person_id])
REFERENCES [mart].[dim_person] ([person_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_person]
GO
/****** Object:  ForeignKey [FK_fact_schedule_preference_dim_preference_type]    Script Date: 02/11/2009 10:27:01 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_preference_dim_preference_type]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_preference]'))
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_preference_type] FOREIGN KEY([preference_type_id])
REFERENCES [mart].[dim_preference_type] ([preference_type_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_preference_type]
GO
/****** Object:  ForeignKey [FK_fact_schedule_preference_dim_scenario]    Script Date: 02/11/2009 10:27:01 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_preference_dim_scenario]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_preference]'))
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_scenario] FOREIGN KEY([scenario_id])
REFERENCES [mart].[dim_scenario] ([scenario_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_scenario]
GO
/****** Object:  ForeignKey [FK_fact_schedule_preference_dim_shift_category]    Script Date: 02/11/2009 10:27:01 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_fact_schedule_preference_dim_shift_category]') AND parent_object_id = OBJECT_ID(N'[mart].[fact_schedule_preference]'))
ALTER TABLE [mart].[fact_schedule_preference]  WITH CHECK ADD  CONSTRAINT [FK_fact_schedule_preference_dim_shift_category] FOREIGN KEY([shift_category_id])
REFERENCES [mart].[dim_shift_category] ([shift_category_id])
GO
ALTER TABLE [mart].[fact_schedule_preference] CHECK CONSTRAINT [FK_fact_schedule_preference_dim_shift_category]
GO
/****** Object:  ForeignKey [FK_permission_report_data_report]    Script Date: 02/11/2009 10:27:09 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_permission_report_data_report]') AND parent_object_id = OBJECT_ID(N'[mart].[permission_report]'))
ALTER TABLE [mart].[permission_report]  WITH CHECK ADD  CONSTRAINT [FK_permission_report_data_report] FOREIGN KEY([report_id])
REFERENCES [mart].[report] ([report_id])
GO
ALTER TABLE [mart].[permission_report] CHECK CONSTRAINT [FK_permission_report_data_report]
GO
/****** Object:  ForeignKey [FK_report_report_group]    Script Date: 02/11/2009 10:27:14 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_report_report_group]') AND parent_object_id = OBJECT_ID(N'[mart].[report]'))
ALTER TABLE [mart].[report]  WITH CHECK ADD  CONSTRAINT [FK_report_report_group] FOREIGN KEY([report_group_id])
REFERENCES [mart].[report_group] ([group_id])
GO
ALTER TABLE [mart].[report] CHECK CONSTRAINT [FK_report_report_group]
GO
/****** Object:  ForeignKey [FK_report_control_collection_control]    Script Date: 02/11/2009 10:27:19 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_report_control_collection_control]') AND parent_object_id = OBJECT_ID(N'[mart].[report_control_collection]'))
ALTER TABLE [mart].[report_control_collection]  WITH CHECK ADD  CONSTRAINT [FK_report_control_collection_control] FOREIGN KEY([control_id])
REFERENCES [mart].[report_control] ([control_id])
GO
ALTER TABLE [mart].[report_control_collection] CHECK CONSTRAINT [FK_report_control_collection_control]
GO
/****** Object:  ForeignKey [FK_scorecard_kpi_dim_kpi]    Script Date: 02/11/2009 10:27:25 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_scorecard_kpi_dim_kpi]') AND parent_object_id = OBJECT_ID(N'[mart].[scorecard_kpi]'))
ALTER TABLE [mart].[scorecard_kpi]  WITH CHECK ADD  CONSTRAINT [FK_scorecard_kpi_dim_kpi] FOREIGN KEY([kpi_id])
REFERENCES [mart].[dim_kpi] ([kpi_id])
GO
ALTER TABLE [mart].[scorecard_kpi] CHECK CONSTRAINT [FK_scorecard_kpi_dim_kpi]
GO
/****** Object:  ForeignKey [FK_scorecard_kpi_dim_scorecard]    Script Date: 02/11/2009 10:27:25 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[FK_scorecard_kpi_dim_scorecard]') AND parent_object_id = OBJECT_ID(N'[mart].[scorecard_kpi]'))
ALTER TABLE [mart].[scorecard_kpi]  WITH CHECK ADD  CONSTRAINT [FK_scorecard_kpi_dim_scorecard] FOREIGN KEY([scorecard_id])
REFERENCES [mart].[dim_scorecard] ([scorecard_id])
GO
ALTER TABLE [mart].[scorecard_kpi] CHECK CONSTRAINT [FK_scorecard_kpi_dim_scorecard]
GO
/****** Object:  ForeignKey [fk_sys_crossdatabaseview_sys_crossdatabaseview_target]    Script Date: 02/11/2009 10:27:28 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[mart].[fk_sys_crossdatabaseview_sys_crossdatabaseview_target]') AND parent_object_id = OBJECT_ID(N'[mart].[sys_crossdatabaseview]'))
ALTER TABLE [mart].[sys_crossdatabaseview]  WITH CHECK ADD  CONSTRAINT [fk_sys_crossdatabaseview_sys_crossdatabaseview_target] FOREIGN KEY([target_id])
REFERENCES [mart].[sys_crossdatabaseview_target] ([target_id])
GO
ALTER TABLE [mart].[sys_crossdatabaseview] CHECK CONSTRAINT [fk_sys_crossdatabaseview_sys_crossdatabaseview_target]
GO
/****** Object:  ForeignKey [FK_fact_agent_dim_agent]    Script Date: 02/11/2009 10:27:35 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Mart].[FK_fact_agent_dim_agent]') AND parent_object_id = OBJECT_ID(N'[Mart].[fact_agent]'))
ALTER TABLE [Mart].[fact_agent]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_dim_agent] FOREIGN KEY([acd_login_id])
REFERENCES [mart].[dim_acd_login] ([acd_login_id])
GO
ALTER TABLE [Mart].[fact_agent] CHECK CONSTRAINT [FK_fact_agent_dim_agent]
GO
/****** Object:  ForeignKey [FK_fact_agent_dim_date1]    Script Date: 02/11/2009 10:27:35 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Mart].[FK_fact_agent_dim_date1]') AND parent_object_id = OBJECT_ID(N'[Mart].[fact_agent]'))
ALTER TABLE [Mart].[fact_agent]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_dim_date1] FOREIGN KEY([date_id])
REFERENCES [mart].[dim_date] ([date_id])
GO
ALTER TABLE [Mart].[fact_agent] CHECK CONSTRAINT [FK_fact_agent_dim_date1]
GO
/****** Object:  ForeignKey [FK_fact_agent_dim_interval]    Script Date: 02/11/2009 10:27:35 ******/
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Mart].[FK_fact_agent_dim_interval]') AND parent_object_id = OBJECT_ID(N'[Mart].[fact_agent]'))
ALTER TABLE [Mart].[fact_agent]  WITH CHECK ADD  CONSTRAINT [FK_fact_agent_dim_interval] FOREIGN KEY([interval_id])
REFERENCES [mart].[dim_interval] ([interval_id])
GO
ALTER TABLE [Mart].[fact_agent] CHECK CONSTRAINT [FK_fact_agent_dim_interval]
GO
 
GO 
 
GO

/****** Object:  Table [mart].[etl_job]    Script Date: 02/09/2009 14:14:54 ******/
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (1, N'Initial' )
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (2, N'Schedule')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (3, N'Queue Statistics')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (4, N'Forecast')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (5, N'Agent Statistics')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (6, N'KPI')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (7, N'Permission')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (8, N'Person Skill')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (9, N'Main')
INSERT [mart].[etl_job] ([job_id], [job_name]) VALUES (10, N'Workload Queues')

/****** Object:  Table [mart].[etl_jobstep]    Script Date: 02/09/2009 14:14:54 ******/
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (1, N'stg_date')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (2, N'stg_person')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (3, N'stg_agent_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (4, N'stg_activity')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (5, N'stg_absence')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (6, N'stg_scenario')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (7, N'stg_shift_category')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (8, N'stg_schedule, stg_contract, stg_schedule_day_absence_count')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (9, N'stg_workload, stg_queue_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (10, N'stg_forecast_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (11, N'stg_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (12, N'stg_scorecard')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (13, N'stg_scorecard_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (14, N'stg_kpi_targets_team')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (15, N'stg_permission')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (16, N'dim_business_unit')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (17, N'dim_date')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (18, N'dim_site')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (19, N'dim_team')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (20, N'dim_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (21, N'dim_skillset')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (22, N'dim_person')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (23, N'dim_activity')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (24, N'dim_absence')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (25, N'dim_scenario')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (26, N'dim_shift_category')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (27, N'dim_shift_length')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (28, N'dim_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (29, N'dim_queue')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (30, N'dim_acd_login')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (31, N'dim_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (32, N'dim_scorecard')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (33, N'scorecard_kpi')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (34, N'bridge_skillset_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (35, N'bridge_acd_login_person')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (36, N'bridge_queue_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (37, N'fact_schedule')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (38, N'fact_contract')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (39, N'fact_queue')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (40, N'fact_forecast_workload')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (41, N'fact_agent')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (42, N'fact_agent_queue')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (43, N'fact_kpi_targets_team')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (44, N'permission_report')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (45, N'stg_business_unit')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (46, N'fact_schedule_day_count')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (47, N'bridge_time_zone')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (48, N'dim_interval')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (49, N'dim_time_zone')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (50, N'fact_schedule_deviation')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (51, N'fact_schedule_forecast_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (52, N'stg_time_zone_bridge')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (53, N'stg_time_zone')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (54, N'fact_schedule_day_count')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (55, N'dim_day_off')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (56, N'stg_schedule_day_off_count')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (57, N'stg_schedule_forecast_skill')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (58, N'stg_user')
INSERT [mart].[etl_jobstep] ([jobstep_id], [jobstep_name]) VALUES (59, N'aspnet_Users, aspnet_Membership')

/****** Object:  Table [mart].[etl_job_schedule]    Script Date: 02/09/2009 14:14:54 ******/
SET IDENTITY_INSERT [mart].[etl_job_schedule] ON
INSERT [mart].[etl_job_schedule] ([schedule_id], [schedule_name], [enabled], [schedule_type], [occurs_daily_at], [occurs_every_minute], [recurring_starttime], [recurring_endtime], [etl_job_name], [etl_relative_period_start], [etl_relative_period_end], [etl_datasource_id], [description])
VALUES (-1, N'Manual', 1, 0, 0, 0, 0, 0, N'Not Defined', NULL, NULL, -1, NULL)
SET IDENTITY_INSERT [mart].[etl_job_schedule] OFF

/****** Object:  Table [mart].[service_level_calculation]    Script Date: 02/11/2009 11:00:50 ******/
SET IDENTITY_INSERT [mart].[service_level_calculation] ON
INSERT [mart].[service_level_calculation] ([service_level_id], [service_level_name], [term_id]) VALUES (1, N'Answered Calls Within Service Level Threshold /Offered Calls ', 12)
INSERT [mart].[service_level_calculation] ([service_level_id], [service_level_name], [term_id]) VALUES (2, N'Answered and Abandoned Calls Within Service Level Threshold /Offered Calls', 13)
INSERT [mart].[service_level_calculation] ([service_level_id], [service_level_name], [term_id]) VALUES (3, N'Answered Calls Within Service Level Threshold / Answered Calls', 14)
SET IDENTITY_INSERT [mart].[service_level_calculation] OFF
/****** Object:  Table [mart].[adherence_calculation]    Script Date: 02/11/2009 11:00:50 ******/
SET IDENTITY_INSERT [mart].[adherence_calculation] ON
INSERT [mart].[adherence_calculation] ([adherence_id], [adherence_name], [term_id]) VALUES (1, N'Ready Time vs. Scheduled Ready Time', 9)
INSERT [mart].[adherence_calculation] ([adherence_id], [adherence_name], [term_id]) VALUES (2, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', 10)
INSERT [mart].[adherence_calculation] ([adherence_id], [adherence_name], [term_id]) VALUES (3, N'Ready Time vs. Contracted Schedule Time', 11)
SET IDENTITY_INSERT [mart].[adherence_calculation] OFF
/****** Object:  Table [mart].[report_group]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [mart].[report_group] ([group_id], [group_name], [group_name_resource_key]) VALUES (1, N'Group 1', N'ResourceGroup1')
/****** Object:  Table [mart].[period_type]    Script Date: 02/11/2009 11:00:50 ******/
SET IDENTITY_INSERT [mart].[period_type] ON
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (1, N'Interval', 5)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (2, N'Half hour', 4)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (3, N'Hour', 3)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (4, N'Day', 6)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (5, N'Week', 7)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (6, N'Month', 8)
INSERT [mart].[period_type] ([period_type_id], [period_type_name], [term_id]) VALUES (7, N'Weekday', 31)
SET IDENTITY_INSERT [mart].[period_type] OFF
/****** Object:  Table [mart].[aspnet_applications]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [dbo].[aspnet_applications] ([ApplicationName], [LoweredApplicationName], [ApplicationId], [Description]) VALUES (N'/', N'/', N'196a4451-8580-46bd-807a-1dbf027f970a', NULL)
/****** Object:  Table [mart].[report_control]    Script Date: 02/11/2009 11:00:50 ******/
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
/****** Object:  Table [mart].[aspnet_SchemaVersions]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'common', N'1', 1)
INSERT [dbo].[aspnet_SchemaVersions] ([Feature], [CompatibleSchemaVersion], [IsCurrentVersion]) VALUES (N'membership', N'1', 1)
/****** Object:  Table [mart].[language]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [mart].[language] ([language_id], [language_name]) VALUES (1025, N'Arabic')
INSERT [mart].[language] ([language_id], [language_name]) VALUES (1033, N'English')
INSERT [mart].[language] ([language_id], [language_name]) VALUES (1040, N'Italian')
INSERT [mart].[language] ([language_id], [language_name]) VALUES (1053, N'Swedish')
/****** Object:  Table [mart].[language_term]    Script Date: 02/11/2009 11:00:50 ******/
SET IDENTITY_INSERT [mart].[language_term] ON
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (1, N'All')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (2, N'Not Defined')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (3, N'Hour')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (4, N'Half Hour')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (5, N'Interval')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (6, N'Day')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (7, N'Week')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (8, N'Month')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (9, N'Ready Time vs Scheduled Ready Time')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (11, N'Ready Time vs. Contracted Schedule Time')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (12, N'Answered Calls Within Service Level Threshold /Offered Calls ')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (13, N'Answered and Abandoned Calls Within Service Level Threshold /Offered Calls')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (14, N'Answered Calls Within Service Level Threshold / Answered Calls')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (15, N'First Name')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (16, N'Last Name')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (17, N'Shift Start Time')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (18, N'Adherence')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (19, N'Monday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (20, N'Tuesday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (21, N'Wednesday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (22, N'Thursday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (23, N'Friday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (24, N'Saturday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (25, N'Sunday')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (26, N'Activity')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (27, N'Absence')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (28, N'Day Off')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (29, N'Shift Category')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (30, N'Extended Preference')
INSERT [mart].[language_term] ([term_id], [term_name_english]) VALUES (31, N'Weekday')
SET IDENTITY_INSERT [mart].[language_term] OFF
/****** Object:  Table [mart].[language_translation]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 1, N'All', N'????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 2, N'Not Defined', N'??? ????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 3, N'Hour', N'????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 4, N'Half Hour', N'??? ????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 5, N'Interval', N'?????? ??????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 6, N'Day', N'???')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 7, N'Week', N'?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 8, N'Month', N'???')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 9, N'Ready Time vs Scheduled Ready Time', N'??? ????????? ????? ??? ????????? ???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', N'??? ????????? ????? ????? ??????? (??????? ????? ??? ??? ???? ????? ?????)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 11, N'Ready Time �vs. Scheduled Contract Time (i.e excluding lunch)', N'??? ????????? ????? ??? ????? ???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 12, N'Answered Calls Within Service Level Threshold / Offered Calls', N'???? ????? ?????? ???? ????????? ???? ?? ???? ????? / ????????? ???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 13, N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls', N'???? ????? ?????? ???? ????????? ???? ?? ???? ????? ?????????? ??? ???????? / ????????? ???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 14, N'Answered Calls Within Service Level Threshold / Answered Calls', N'???? ????? ?????? ???? ????????? ???? ?? ???? ????? / ????????? ???? ?? ???? ?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 15, N'First Name', N'????? ?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 16, N'Last Name', N'??? ???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 17, N'Shift Start Time', N'??? ??? ???? ???')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 18, N'Adherence', N'????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 19, N'Monday', N'???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 20, N'Tuesday', N'????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 21, N'Wednesday', N'????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 22, N'Thursday', N'??????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 23, N'Friday', N'??????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 24, N'Saturday', N'?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 25, N'Sunday', N'?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 26, N'Activity', N'????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 27, N'Absence', N'????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1025, 28, N'Day Off', N'????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 1, N'All', N'All')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 2, N'Not Defined', N'Not Defined')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 3, N'Hour', N'Hour')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 4, N'Half Hour', N'Half Hour')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 5, N'Interval', N'Interval')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 6, N'Day', N'Day')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 7, N'Week', N'Week')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 8, N'Month', N'Month')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 9, N'Ready Time vs Scheduled Ready Time', N'Ready Time vs Scheduled Ready Time')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 11, N'Ready Time  vs. Scheduled Contract Time (i.e excluding lunch)', N'Ready Time vs. Contracted Schedule Time')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 12, N'Answered Calls Within Service Level Threshold / Offered Calls ', N'Answered Calls Within Service Level Threshold / Offered Calls ')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 13, N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls', N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 14, N'Answered Calls Within Service Level Threshold / Answered Calls', N'Answered Calls Within Service Level Threshold / Answered Calls')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 15, N'First Name', N'First Name')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 16, N'Last Name', N'Last Name')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 17, N'Shift Start Time', N'Shift Start Time')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 18, N'Adherence', N'Adherence')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 19, N'Monday', N'Monday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 20, N'Tuesday', N'Tuesday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 21, N'Wednesday', N'Wednesday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 22, N'Thursday', N'Thursday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 23, N'Friday', N'Friday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 24, N'Saturday', N'Saturday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 25, N'Sunday', N'Sunday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 26, N'Activity', N'Activity')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 27, N'Absence', N'Absence')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 28, N'Day Off', N'Day Off')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 29, N'Shift Category', N'Shift Category')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 30, N'Extended Preference', N'Extended Preference')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1033, 31, N'Weekday', N'Weekday')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 1, N'All', N'Tutti')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 2, N'Not Defined', N'Non Definito')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 3, N'Hour', N'Ora')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 4, N'Half Hour', N'Mezz''ora')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 5, N'Interval', N'Intervallo')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 6, N'Day', N'Giorno')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 7, N'Week', N'Settimana')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 8, N'Month', N'Mese')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 9, N'Ready Time vs Scheduled Ready Time', N'Tempo Pronto / Tempo Pronto Programmato')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', N'Tempo Pronto / Tempo Programmato (incl. prima e dopo inizio turno)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 11, N'Ready Time �vs. Scheduled Contract Time (i.e excluding lunch)', N'Tempo Pronto / Tempo Programmato')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 12, N'Answered Calls Within Service Level Threshold / Offered Calls', N'Chiamate Risposte entro Soglia Livello di Servizio / Chiamate Offerte')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 13, N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls', N'Chiamate Risposte e Abbandonate entro Soglia Livello di Servizio / Chiamate Offerte')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 14, N'Answered Calls Within Service Level Threshold / Answered Calls', N'Chiamate Risposte entro Soglia Livello di Servizio / Risposte')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 15, N'First Name', N'Nome')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 16, N'Last Name', N'Cognome')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 17, N'Shift Start Time', N'Ora Inizio Turno')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 18, N'Adherence', N'Presenza')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 19, N'Monday', N'Luned�')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 20, N'Tuesday', N'Marted�')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 21, N'Wednesday', N'Mercoled�')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 22, N'Thursday', N'Gioved�')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 23, N'Friday', N'Venerd�')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 24, N'Saturday', N'Sabato')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 25, N'Sunday', N'Domenica')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 26, N'Activity', N'Attivit�')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 27, N'Absence', N'Assenza')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1040, 28, N'Day Off', N'Giorno Libero')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 1, N'All', N'Alla')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 2, N'Not Defined', N'Ej Definierad')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 3, N'Hour', N'Timme')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 4, N'Half Hour', N'Halvtimme')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 5, N'Interval', N'Intervall')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 6, N'Day', N'Dag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 7, N'Week', N'Vecka')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 8, N'Month', N'M�nad')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 9, N'Ready Time vs Scheduled Ready Time', N'Ready tid vs Schemalagd ready tid')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', N'Ready tid vs Schemalagd tid (inkl tid f�re och efter skiftstart)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 11, N'Ready Time  vs. Scheduled Contract Time (i.e excluding lunch)', N'Ready tid vs Schemalagd konstraktstid')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 12, N'Answered Calls Within Service Level Threshold / Offered Calls ', N'Besvarade samtal inom svarsm�l / Inkommande samtal')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 13, N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls', N'Besvarade och tappade samtal inom svarsm�l / Inkommande samtal')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 14, N'Answered Calls Within Service Level Threshold / Answered Calls', N'Besvarade samtal inom svarsm�l / Besvarade samtal')

INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 15, N'First Name', N'F�rnamn')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 16, N'Last Name', N'Efternamn')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 17, N'Shift Start Time', N'Skiftstart')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 18, N'Adherence', N'F�ljsamhet')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 19, N'Monday', N'm�ndag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 20, N'Tuesday', N'tisdag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 21, N'Wednesday', N'onsdag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 22, N'Thursday', N'torsdag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 23, N'Friday', N'fredag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 24, N'Saturday', N'l�rdag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 25, N'Sunday', N's�ndag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 26, N'Activity', N'Aktivitet')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 27, N'Absence', N'Fr�nvaro')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 28, N'Day Off', N'Fridag')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 29, N'Shift Category', N'Skiftkategori')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 30, N'Extended Preference', N'Ut�kat �nskem�l')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1053, 31, N'Weekday', N'Veckodag')
/****** Object:  Table [mart].[report]    Script Date: 02/11/2009 11:00:50 ******/
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (1, 19, 1, N'~/Selection.aspx?ReportID=1', N'_self', N'Preferences per Day', N'ResReportPreferencesPerDay', 1, N'~/Reports/CCC/report_schedule_preferences_per_day.rdlc', 1000, N'mart.report_data_schedule_preferences_per_day', N'f01_Report_PreferencesPerDay.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (2, 19, 1, N'~/Selection.aspx?ReportID=2', N'_self', N'Preferences Per Agent', N'ResReportPreferencesPerAgent', 1, N'~/Reports/CCC/report_schedule_preferences_per_agent.rdlc', 1000, N'mart.report_data_schedule_preferences_per_agent', N'f01_Report_PreferencesPerAgent.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (3, 21, 1, N'~/Selection.aspx?ReportID=3', N'_self', N'IMPROVE', N'ResReportImprove', 1, N'~/Reports/CCC/report_IMPROVE.rdlc', 1000, N'mart.report_data_IMPROVE', N'/f01:Report+ImproveReport', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (5, 1, 1, N'~/Reports/CCC/AgentScorecardDefault.aspx', NULL, N'Agent Scorecard Compact', N'ResreportAgentScoercard', 1, N'', 1000, N'', N'', N' ', N' ', N' ', N' ')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (6, 1, 1, N'~/Reports/CCC/AgentScorecard.aspx', N'main', N'Agent Scorecard', N'ResReportAgentScorecard', 1, N'', 1000, N'', N'', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (7, 5, 1, N'~/Selection.aspx?ReportID=7', N'_self', N'Forecast vs Scheduled Hours', N'ResReportForecastvsScheduledHours', 1, N'~/Reports/CCC/report_forecast_vs_scheduled_hours.rdlc', 1000, N'mart.report_data_forecast_vs_scheduled_hours', N'f01_Report_ForecastvsScheduledHours.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (8, 4, 1, N'~/Selection.aspx?ReportID=8', N'_self', N'Abandonment and Speed of Answer', N'ResReportAbandonmentAndSpeedOfAnswer', 1, N'~/Reports/CCC/report_queue_stat_abnd.rdlc', 1000, N'mart.report_data_queue_stat_abnd', N'f01_Report_AbandonmentAndSpeedOfAnswer.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (9, 9, 1, N'~/Selection.aspx?ReportID=9', N'_self', N'Service Level and Agents Ready', N'ResReportServiceLevelAndAgentsReady', 1, N'~/Reports/CCC/report_service_level_agents_ready.rdlc', 1000, N'mart.report_data_service_level_agents_ready', N'f01_Report_ServiceLevelAndAgentsReady.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (10, 6, 1, N'~/Selection.aspx?ReportID=10', N'_self', N'Forecast vs Actual Workload', N'ResReportForecastvsActualWorkload', 1, N'~/Reports/CCC/report_forecast_vs_actual_workload.rdlc', 1000, N'mart.report_data_forecast_vs_actual_workload', N'f01_Report_ForecastvsActualWorkload.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (11, 18, 1, N'~/Selection.aspx?ReportID=11', N'_self', N'Team Metrics', N'ResReportTeamMetrics', 1, N'~/Reports/CCC/report_team_metrics.rdlc', 1000, N'mart.report_data_team_metrics', N'f01_Report_TeamMetrics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (12, 8, 1, N'~/Selection.aspx?ReportID=12', N'_self', N'Agent Metrics', N'ResReportAgentScheduleResult', 1, N'~/Reports/CCC/report_agent_schedule_result.rdlc', 1000, N'mart.report_data_agent_schedule_result', N'f01_Report_AgentScheduleResult.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (13, 10, 1, N'~/Selection.aspx?ReportID=13', N'_self', N'Agent Schedule Adherence', N'ResReportAgentScheduleAdherence', 1, N'~/Reports/CCC/report_agent_schedule_adherence.aspx', 1000, N'mart.report_data_agent_schedule_adherence', N'f01_Report_AgentScheduleAdherence.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (14, 11, 1, N'~/Selection.aspx?ReportID=14', N'_self', N'Queue Statistics', N'ResReportQueueStatistics', 1, N'~/Reports/CCC/report_queue_stat_raw.rdlc', 1000, N'mart.report_data_queue_stat_raw', N'f01_Report_QueueStatistics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (15, 12, 1, N'~/Selection.aspx?reportID=15', N'_self', N'Agent Queue Statistics', N'ResReportAgentQueueStatistics', 1, N'~/Reports/CCC/report_agent_queue_stat_raw.rdlc', 1000, N'mart.report_data_agent_queue_stat_raw', N'f01_Report_AgentQueueStatistics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (16, 13, 1, N'~/Selection.aspx?ReportID=16', N'_self', N'Agent Statistics', N'ResReportAgentStatistics', 1, N'~/Reports/CCC/report_agent_stat_raw.rdlc', 1000, N'mart.report_data_agent_stat_raw', N'f01_Report_AgentStatistics.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (17, 14, 1, N'~/Selection.aspx?ReportID=17', N'_self', N'Scheduled Time per Agent', N'ResReportScheduledTimePerAgent', 1, N'~/Reports/CCC/report_scheduled_time_per_agent.rdlc', 1000, N'mart.report_data_scheduled_time_per_agent', N'f01_Report_ScheduledTimePerAgent.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (18, 17, 1, N'~/Selection.aspx?ReportID=18', N'_self', N'Scheduled Time per Activity', N'ResReportScheduledTimePerActivity', 1, N'~/Reports/CCC/report_scheduled_time_per_activity.rdlc', 1000, N'mart.report_data_scheduled_time_per_activity', N'f01_Report_ScheduledTimePerActivity.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (19, 15, 1, N'~/Selection.aspx?ReportID=19', N'_self', N'Shift Category per Day', N'ResReportShiftCategoryPerDay', 1, N'~/Reports/CCC/report_shift_category_per_day.rdlc', 1000, N'mart.report_data_shift_category_per_day', N'f01_Report_ShiftCategoryPerDay.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (20, 16, 1, N'~/Selection.aspx?ReportID=20', N'_self', N'Shift Category and Full Day Absences per Agent', N'ResReportShiftCategoryAndDayAbsencePerAgent', 1, N'~/Reports/CCC/report_shift_category_and_day_absences_per_agent.rdlc', 1000, N'mart.report_data_shift_cat_and_day_abs_per_agent', N'f01_Report_ShiftCategoryAndFullDayAbsencePerAgent.html', N'', N'', N'', N'')
INSERT [mart].[report] ([report_id], [control_collection_id], [report_group_id], [url], [target], [report_name], [report_name_resource_key], [visible], [rpt_file_name], [text_id], [proc_name], [help_key], [sub1_name], [sub1_proc_name], [sub2_name], [sub2_proc_name]) VALUES (21, 20, 1, N'~/Selection.aspx?ReportID=21', N'_self', N'Scheduled Agents per Interval and Team', N'ResReportScheduledAgentsPerIntervalAndTeam', 1, N'~/Reports/CCC/report_scheduled_agents_per_interval_and_team.rdlc', 1000, N'mart.report_data_scheduled_agents_per_interval_and_team', N'f01_Report_ScheduledAgentsPerIntervalAndTeam.html', N'', N'', N'', N'')
/****** Object:  Table [mart].[report_control_collection]    Script Date: 02/11/2009 11:00:50 ******/
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
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (24, 5, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (25, 5, 2, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (26, 5, 3, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (27, 5, 4, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 26, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (28, 5, 5, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (29, 5, 6, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (30, 6, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (31, 6, 2, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (32, 6, 3, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 31, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (33, 6, 4, 11, N'4', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (34, 6, 5, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (35, 6, 6, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 34, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (36, 6, 7, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (37, 6, 8, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (38, 7, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (39, 7, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (40, 7, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 39, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (41, 7, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (42, 7, 5, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (43, 7, 6, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 39, 40, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (44, 7, 7, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 39, 40, 43, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (46, 8, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (47, 8, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (48, 8, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 47, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (49, 8, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (50, 8, 5, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (51, 8, 6, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 47, 48, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (52, 8, 7, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 47, 48, 51, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (53, 8, 10, 18, N'-99', N'ResAgentsColon', NULL, N'@agent_set', 47, 48, 51, 52)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (54, 8, 11, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (55, 9, 1, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (56, 9, 2, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 55, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (57, 9, 3, 11, N'4', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (58, 9, 4, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (59, 9, 5, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 58, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (60, 9, 6, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (61, 9, 7, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (62, 9, 8, 20, N'1', N'ResServiceLevelCalcColon', NULL, N'@sl_calc_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (63, 10, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (64, 10, 2, 6, N'12:00', N'ResDateColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (65, 10, 3, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 64, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (66, 10, 4, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 64, 65, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (67, 10, 5, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (68, 10, 6, 21, N'1', N'ResSortByColon', NULL, N'@sort_by', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (69, 8, 12, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (70, 11, 1, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (71, 11, 2, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 70, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (72, 11, 3, 23, N'-99', N'ResQueueColon', NULL, N'@queue_set', 70, 71, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (73, 11, 4, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (74, 11, 5, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 73, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (75, 11, 6, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (76, 11, 7, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (77, 11, 8, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (78, 10, 7, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
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
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (89, 12, 7, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 85, 86, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (90, 12, 8, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 85, 86, 89, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (91, 12, 9, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 85, 86, 89, 90)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (92, 12, 10, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (93, 13, 1, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (94, 13, 2, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 93, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (95, 13, 3, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (96, 13, 4, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (97, 13, 5, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 93, 94, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (98, 13, 6, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 93, 94, 97, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (99, 13, 7, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 93, 94, 97, 98)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (100, 13, 8, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (101, 14, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (102, 14, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (103, 14, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 102, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (104, 14, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (105, 14, 5, 13, N'287', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (106, 14, 6, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 102, 103, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (107, 14, 7, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 102, 103, 106, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (108, 14, 8, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 102, 103, 106, 107)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (111, 14, 10, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (112, 15, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (113, 15, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (114, 15, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 113, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (115, 15, 4, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 113, 114, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (116, 15, 5, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 113, 114, 115, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (117, 15, 6, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 113, 114, 115, 116)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (118, 15, 7, 26, N'-99', N'ResShiftCategoryColon', NULL, N'@shift_category_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (119, 15, 8, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (120, 16, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (121, 16, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (122, 16, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 121, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (123, 16, 4, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 121, 122, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (124, 16, 5, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 121, 122, 123, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (125, 16, 6, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 121, 122, 123, 124)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (126, 16, 7, 28, N'-99', N'ResShiftCategoryColon', NULL, N'@shift_category_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (127, 16, 8, 27, N'-1', N'ResDayOffColon', NULL, N'@day_off_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (128, 16, 9, 25, N'-1', N'ResAbsenceColon', NULL, N'@absence_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (129, 16, 10, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (130, 17, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (131, 17, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 130, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (132, 17, 6, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 130, 131, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (133, 17, 7, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 130, 131, 132, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (134, 17, 8, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 130, 131, 132, 133)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (135, 17, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (136, 17, 10, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (137, 17, 9, 24, N'-99', N'ResActivityColon', NULL, N'@activity_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (138, 17, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (139, 17, 5, 13, N'287', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (140, 18, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (141, 18, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 140, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (142, 18, 6, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 140, 141, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (143, 18, 7, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 140, 141, 142, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (145, 18, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (146, 18, 9, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (147, 18, 8, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (148, 18, 4, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (149, 18, 5, 13, N'287', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (150, 19, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (151, 19, 2, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (152, 19, 3, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 151, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (153, 19, 4, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 151, 152, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (154, 19, 5, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 151, 152, 153, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (155, 19, 6, 5, N'-2', N'ResAgentsColon', NULL, N'@agent_id', 151, 152, 153, 154)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (156, 19, 7, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (157, 20, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (158, 20, 2, 6, N'12:00', N'ResDateColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (159, 20, 3, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (160, 20, 4, 13, N'287', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (161, 20, 5, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 158, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (162, 20, 6, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 158, 161, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (164, 20, 8, 24, N'-99', N'ResActivityColon', NULL, N'@activity_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (165, 20, 9, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (166, 21, 1, 14, N'-99', N'ResScenarioColon', NULL, N'@scenario_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (167, 21, 2, 15, N'-99', N'ResSkillColon', NULL, N'@skill_set', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (168, 21, 3, 10, N'-99', N'ResWorkloadColon', NULL, N'@workload_set', 167, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (169, 21, 4, 11, N'4', N'ResIntervalType', NULL, N'@interval_type', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (170, 21, 5, 1, N'12:00', N'ResDateFromColon', NULL, N'@date_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (171, 21, 6, 2, N'12:00', N'ResDateToColon', NULL, N'@date_to', 170, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (172, 21, 7, 12, N'0', N'ResIntervalFromColon', N'1', N'@interval_from', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (173, 21, 8, 13, N'-99', N'ResIntervalToColon', N'2', N'@interval_to', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (174, 21, 9, 3, N'-2', N'ResSiteNameColon', NULL, N'@site_id', 170, 171, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (175, 21, 10, 4, N'-2', N'ResTeamNameColon', NULL, N'@team_id', 170, 171, 174, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (176, 21, 11, 19, N'1', N'ResAdherenceCalculationColon', NULL, N'@adherence_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (177, 21, 12, 20, N'1', N'ResServiceLevelCalcColon', NULL, N'@sl_calc_id', NULL, NULL, NULL, NULL)
INSERT [mart].[report_control_collection] ([control_collection_id], [collection_id], [print_order], [control_id], [default_value], [control_name_resource_key], [fill_proc_param], [param_name], [depend_of1], [depend_of2], [depend_of3], [depend_of4]) VALUES (178, 21, 13, 22, N'-1', N'ResTimeZoneColon', NULL, N'@time_zone_id', NULL, NULL, NULL, NULL)
GO

--Crossdatabase view inserts
--KJ 20090212
INSERT INTO mart.sys_crossdatabaseview_target
VALUES (4,'TeleoptiCCCAgg','TeleoptiCCCAgg', 0)
GO
SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] ON
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (1, N'v_log_object', N'SELECT * FROM $$$target$$$.dbo.log_object', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (2, N'v_agent_logg', N'SELECT * FROM $$$target$$$.dbo.agent_logg', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (3, N'v_agent_info', N'SELECT * FROM $$$target$$$.dbo.agent_info', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (4, N'v_queues', N'SELECT * FROM $$$target$$$.dbo.queues', 4)
INSERT [mart].[sys_crossdatabaseview] ([view_id], [view_name], [view_definition], [target_id]) VALUES (5, N'v_queue_logg', N'SELECT * FROM $$$target$$$.dbo.queue_logg', 4)
SET IDENTITY_INSERT [mart].[sys_crossdatabaseview] OFF
GO

-- Not Defined data source or manual data source

SET IDENTITY_INSERT [mart].[sys_datasource] ON

INSERT INTO [mart].[sys_datasource]
	(
	datasource_id, 
	datasource_name, 
	log_object_id,
	log_object_name,
	datasource_database_id,
	datasource_database_name,
	datasource_type_name
	)
SELECT 
	datasource_id			=-1, 
	datasource_name			='Not Defined',
	log_object_id			= -1,
	log_object_name			='Not Defined',
	datasource_database_id	= -1,
	datasource_database_name= 'Not Defined',
	datasource_type_name	= 'Not Defined'
WHERE NOT EXISTS (SELECT * FROM [mart].[sys_datasource] where datasource_id = -1)

----------------------------------------------------------------------------
-- Insert TeleoptiCCC as a data source.
INSERT INTO [mart].[sys_datasource]
	(
	datasource_id, 
	datasource_name, 
	log_object_id,
	log_object_name,
	datasource_database_id,
	datasource_database_name,
	datasource_type_name
	)
SELECT 
	datasource_id			=1, 
	datasource_name			= 'TeleoptiCCC',
	log_object_id			= -1,
	log_object_name			= 'Not Defined',
	datasource_database_id	= 1,
	datasource_database_name= 'Raptor Default',
	datasource_type_name	= 'Raptor Default'
WHERE NOT EXISTS (SELECT * FROM [mart].[sys_datasource] where datasource_id = 1)

SET IDENTITY_INSERT [mart].[sys_datasource] OFF
GO

SET NOCOUNT ON
INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (1, N'TeleoptiBrokerService', N'Port', N'9080', N'System.Int32',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (2, N'TeleoptiBrokerService', N'Server', N'127.0.0.1', N'System.String',suser_sname(),getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (3, N'TeleoptiBrokerService', N'Threads', N'1', N'System.Int32',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (4, N'TeleoptiBrokerService', N'Intervall', N'10000', N'System.Double',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (5, N'TeleoptiBrokerService', N'ConnectionString', N'Data Source=server\instanceName;Initial Catalog=Messaging;Persist Security Info=True;User ID=sa;Password=somepassword', N'System.String',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (6, N'TeleoptiBrokerService', N'GeneralThreadPoolThreads', N'3', N'System.Int32',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (7, N'TeleoptiBrokerService', N'DatabaseThreadPoolThreads', N'3', N'System.Int32',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (8, N'TeleoptiBrokerService', N'ReceiptThreadPoolThreads', N'3', N'System.Int32',suser_sname(), getdate())

INSERT [msg].[Configuration] ([ConfigurationId], [ConfigurationType], [ConfigurationName], [ConfigurationValue], [ConfigurationDataType], [ChangedBy], [ChangedDateTime])
VALUES (9, N'TeleoptiBrokerService', N'HeartbeatThreadPoolThreads', N'1', N'System.Int32',suser_sname(), getdate())

INSERT INTO [msg].[Address] ([MessageBrokerId],[MulticastAddress],[Port],[Direction])
VALUES (1,'235.235.235.234',9090,'Client')

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (67,'7.0.67') 
