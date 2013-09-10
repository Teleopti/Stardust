declare @sqltext nvarchar(4000)
set @sqltext = ''

SELECT @sqltext = 'CREATE SCHEMA [Stage] AUTHORIZATION [dbo]'
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Stage')
EXEC sp_executesql @sqltext
set @sqltext = ''

SELECT @sqltext = 'CREATE SCHEMA [Mart] AUTHORIZATION [dbo]'
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Mart')
EXEC sp_executesql @sqltext
set @sqltext = ''

SELECT @sqltext = 'CREATE SCHEMA [ReadModel] AUTHORIZATION [dbo]'
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'ReadModel')
EXEC sp_executesql @sqltext
set @sqltext = ''

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_date]') AND type in (N'U'))
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
		[insert_date] [smalldatetime] NOT NULL,
	 CONSTRAINT [PK_dim_date] PRIMARY KEY CLUSTERED 
	(
		[date_id] ASC
	)
	)

	ALTER TABLE [mart].[dim_date] ADD  CONSTRAINT [DF_dim_date_inserted]  DEFAULT (getdate()) FOR [insert_date]
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_interval]') AND type in (N'U'))
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
	)
	)
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_queue]') AND type in (N'U'))
BEGIN
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

	ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_queue_name]  DEFAULT ('Not Defined') FOR [queue_name]
	ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
	ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_insert_date]  DEFAULT (getdate()) FOR [insert_date]
	ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_update_date]  DEFAULT (getdate()) FOR [update_date]
	ALTER TABLE [mart].[dim_queue] ADD  CONSTRAINT [DF_dim_queue_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_queue]') AND type in (N'U'))
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
		[datasource_id] [smallint] NOT NULL,
		[insert_date] [smalldatetime] NOT NULL,
		[update_date] [smalldatetime] NOT NULL,
		[datasource_update_date] [smalldatetime] NOT NULL,
	 CONSTRAINT [PK_fact_queue] PRIMARY KEY CLUSTERED 
	(
		[date_id] ASC,
		[interval_id] ASC,
		[queue_id] ASC
	)
	)
	
	ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_date] FOREIGN KEY([date_id])
	REFERENCES [mart].[dim_date] ([date_id])
	ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_date]
	ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_interval] FOREIGN KEY([interval_id])
	REFERENCES [mart].[dim_interval] ([interval_id])
	ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_interval]
	ALTER TABLE [mart].[fact_queue]  WITH CHECK ADD  CONSTRAINT [FK_fact_queue_dim_queue] FOREIGN KEY([queue_id])
	REFERENCES [mart].[dim_queue] ([queue_id])
	ALTER TABLE [mart].[fact_queue] CHECK CONSTRAINT [FK_fact_queue_dim_queue]
	ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_datasource_id]  DEFAULT ((-1)) FOR [datasource_id]
	ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_insert_date]  DEFAULT (getdate()) FOR [insert_date]
	ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_update_date]  DEFAULT (getdate()) FOR [update_date]
	ALTER TABLE [mart].[fact_queue] ADD  CONSTRAINT [DF_fact_queue_statistics_datasource_update_date]  DEFAULT ('1900-01-01') FOR [datasource_update_date]
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[stg_queue]') AND type in (N'U'))
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

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[GroupingReadOnly]') AND type in (N'U'))
BEGIN
CREATE TABLE [ReadModel].[GroupingReadOnly](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [uniqueidentifier] NOT NULL,
	[StartDate] [smalldatetime] NOT NULL,
	[TeamId] [uniqueidentifier] NULL,
	[SiteId] [uniqueidentifier] NULL,
	[BusinessUnitId] [uniqueidentifier] NULL,
	[GroupId] [uniqueidentifier] NULL,
	[GroupName] [nvarchar](50) NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[PageId] [uniqueidentifier] NULL,
	[PageName] [nvarchar](50) NULL,
	[EmploymentNumber] [nvarchar](50) NULL,
	[EndDate] [smalldatetime] NULL,
	[LeavingDate] [smalldatetime] NULL,
 CONSTRAINT [PK_GroupingReadOnly] PRIMARY KEY CLUSTERED 
	(
	[Id] ASC
	)
	)
	
END

--on delete cascade på audittabeller
ALTER TABLE [Auditing].[MainShift_AUD] DROP CONSTRAINT [FK_MainShift_AUD_Revision]
ALTER TABLE [Auditing].[MainShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_MainShift_AUD_Revision] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
on delete cascade
ALTER TABLE [Auditing].[MainShift_AUD] CHECK CONSTRAINT [FK_MainShift_AUD_Revision]


ALTER TABLE [Auditing].[MainShiftActivityLayer_AUD] DROP CONSTRAINT [FK_MainShiftActivityLayer_AUD_Revision]
ALTER TABLE [Auditing].[MainShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftActivityLayer_AUD_Revision] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
on delete cascade
ALTER TABLE [Auditing].[MainShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_MainShiftActivityLayer_AUD_Revision]


ALTER TABLE [Auditing].[OvertimeShift_AUD] DROP CONSTRAINT [FK_OvertimeShift_AUD_Revision]
ALTER TABLE [Auditing].[OvertimeShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShift_AUD_Revision] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
on delete cascade
ALTER TABLE [Auditing].[OvertimeShift_AUD] CHECK CONSTRAINT [FK_OvertimeShift_AUD_Revision]


ALTER TABLE [Auditing].[OvertimeShiftActivityLayer_AUD] DROP CONSTRAINT [FK_OvertimeShiftActivityLayer_AUD_Revision]
ALTER TABLE [Auditing].[OvertimeShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_AUD_Revision] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
on delete cascade
ALTER TABLE [Auditing].[OvertimeShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_AUD_Revision]


ALTER TABLE [Auditing].[PersonAbsence_AUD] DROP CONSTRAINT [FK_PersonAbsence_AUD_Revision]
ALTER TABLE [Auditing].[PersonAbsence_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_AUD_Revision] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
on delete cascade
ALTER TABLE [Auditing].[PersonAbsence_AUD] CHECK CONSTRAINT [FK_PersonAbsence_AUD_Revision]


ALTER TABLE [Auditing].[MainShift_AUD] DROP CONSTRAINT [FK_MainShift_AUD_Revision]
ALTER TABLE [Auditing].[MainShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_MainShift_AUD_Revision] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
on delete cascade
ALTER TABLE [Auditing].[MainShift_AUD] CHECK CONSTRAINT [FK_MainShift_AUD_Revision]


ALTER TABLE [Auditing].[PersonalShift_AUD] DROP CONSTRAINT [FK_PersonalShift_AUD_Revision]
ALTER TABLE [Auditing].[PersonalShift_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShift_AUD_Revision] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
on delete cascade
ALTER TABLE [Auditing].[PersonalShift_AUD] CHECK CONSTRAINT [FK_PersonalShift_AUD_Revision]


ALTER TABLE [Auditing].[PersonalShiftActivityLayer_AUD] DROP CONSTRAINT [FK_PersonalShiftActivityLayer_AUD_Revision]
ALTER TABLE [Auditing].[PersonalShiftActivityLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShiftActivityLayer_AUD_Revision] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
on delete cascade
ALTER TABLE [Auditing].[PersonalShiftActivityLayer_AUD] CHECK CONSTRAINT [FK_PersonalShiftActivityLayer_AUD_Revision]


ALTER TABLE [Auditing].[PersonAssignment_AUD] DROP CONSTRAINT [FK_PersonAssignment_AUD_Revision]
ALTER TABLE [Auditing].[PersonAssignment_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_AUD_Revision] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
on delete cascade
ALTER TABLE [Auditing].[PersonAssignment_AUD] CHECK CONSTRAINT [FK_PersonAssignment_AUD_Revision]


ALTER TABLE [Auditing].[PersonDayOff_AUD] DROP CONSTRAINT [FK_PersonDayOff_AUD_Revision]
ALTER TABLE [Auditing].[PersonDayOff_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonDayOff_AUD_Revision] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
on delete cascade
ALTER TABLE [Auditing].[PersonDayOff_AUD] CHECK CONSTRAINT [FK_PersonDayOff_AUD_Revision]




---This needs to be manually put here to prevent crash when proc
---is scripted in when running tests locally
------------
--Setting table
------------
CREATE TABLE [dbo].[AuditTrailSetting](
	[Id] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[DaysToKeep] [int] NOT NULL,
	[IsRunning] [bit] NOT NULL)
ON [PRIMARY]

ALTER TABLE dbo.AuditTrailSetting ADD CONSTRAINT
	PK_AuditTrailSetting PRIMARY KEY CLUSTERED 
	(
	Id
	)

ALTER TABLE [dbo].[AuditTrailSetting]  WITH CHECK ADD  CONSTRAINT [FK_AuditTrailSetting_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[AuditTrailSetting] CHECK CONSTRAINT [FK_AuditTrailSetting_Person_UpdatedBy]

---------------------------------

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[ScheduleProjectionReadOnly]') AND type in (N'U'))
BEGIN
	CREATE TABLE [ReadModel].[ScheduleProjectionReadOnly](
		[Id] [uniqueidentifier] NOT NULL,
		[ScenarioId] [uniqueidentifier] NOT NULL,
		[PersonId] [uniqueidentifier] NOT NULL,
		[BelongsToDate] [smalldatetime] NOT NULL,
		[PayloadId] [uniqueidentifier] NOT NULL,
		[StartDateTime] [datetime] NOT NULL,
		[EndDateTime] [datetime] NOT NULL,
		[WorkTime] [bigint] NOT NULL,
		[ContractTime] [bigint] NOT NULL,
		[Name] [nvarchar](50) NOT NULL,
		[ShortName] [nvarchar](25) NULL,
		[DisplayColor] [int] NOT NULL,
		[PayrollCode] [nvarchar](20) NULL,
		[InsertedOn] [datetime] NOT NULL
	)

	ALTER TABLE ReadModel.ScheduleProjectionReadOnly ADD CONSTRAINT
		PK_ScheduleProjectionReadOnly PRIMARY KEY NONCLUSTERED 
		(
		Id
		)

	CREATE CLUSTERED INDEX [CIX_ScheduleProjectionReadOnly] ON [ReadModel].[ScheduleProjectionReadOnly] 
	(
		[BelongsToDate] ASC,
		[PersonId] ASC,
		[ScenarioId] ASC
	)

	ALTER TABLE [ReadModel].[ScheduleProjectionReadOnly] ADD  CONSTRAINT [DF_ScheduleProjectionReadOnly_Id]  DEFAULT (newid()) FOR [Id]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DenormalizationQueue]') AND type in (N'U'))
BEGIN
	CREATE TABLE dbo.DenormalizationQueue
		(
		Id uniqueidentifier NOT NULL,
		BusinessUnit uniqueidentifier NOT NULL,
		Timestamp datetime NOT NULL,
		Message nvarchar(2000) NOT NULL,
		Type nvarchar(250) NOT NULL
		) 

	ALTER TABLE dbo.DenormalizationQueue ADD CONSTRAINT
		DF_DenormalizationQueue_Id DEFAULT newid() FOR Id

	ALTER TABLE dbo.DenormalizationQueue ADD CONSTRAINT
		PK_DenormalizationQueue PRIMARY KEY CLUSTERED 
		(
		Id
		) 
END
GO


------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[FindPerson]') AND type in (N'U'))
BEGIN
	CREATE TABLE [ReadModel].[FindPerson](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[PersonId] [uniqueidentifier] NOT NULL,
		[FirstName] [nvarchar](50) NOT NULL,
		[LastName] [nvarchar](50) NOT NULL,
		[EmploymentNumber] [nvarchar](50) NOT NULL,
		[Note] [nvarchar](1024) NOT NULL,
		[TerminalDate] [datetime] NULL,
		[SearchValue] [nvarchar](max) NULL,
		[SearchType] [nvarchar](200) NOT NULL,
		[TeamId] [uniqueidentifier]  NULL,
		[SiteId] [uniqueidentifier]  NULL,
		[BusinessUnitId] [uniqueidentifier]  NULL
		)

	ALTER TABLE ReadModel.[FindPerson] ADD CONSTRAINT
		PK_FindPerson PRIMARY KEY NONCLUSTERED 
		(
		Id
		)

	CREATE CLUSTERED INDEX [CIX_FindPerson] ON [ReadModel].[FindPerson] 
	(
		[TerminalDate] ASC
	)
END
GO


/*
Anders F
2012-02-06
Purge old data for app db.
*/

IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurgeSetting]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PurgeSetting](
	[Key] [nvarchar](50) NOT NULL,
	[KeepYears] [int] NOT NULL
	)


ALTER TABLE [dbo].[PurgeSetting] ADD CONSTRAINT PK_PurgeSetting PRIMARY KEY CLUSTERED 
(
	[Key] ASC
)

END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[PurgeSetting] WHERE [Key] ='Forecast')
	INSERT INTO [dbo].[PurgeSetting] ([Key], [KeepYears]) VALUES('Forecast',10)
IF NOT EXISTS (SELECT 1 FROM [dbo].[PurgeSetting] WHERE [Key] ='Schedule')
	INSERT INTO [dbo].[PurgeSetting] ([Key], [KeepYears]) VALUES('Schedule',10)
GO