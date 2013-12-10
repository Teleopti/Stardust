

----------------  
--Name: Robin Karlsson
--Date: 2013-09-26
--Desc: Bug #24859 - Fix messed up values in shifts
---------------- 
update ActivityExtender
set earlystart = 0 where EarlyStart = 1

update ActivityExtender
set LateStart = 0 where LateStart = 1
GO

----------------  
--Name: David Jonsson
--Date: 2013-10-02
--Desc: Bug #24989 - Make database compatible with SQL 2005
---------------- 
ALTER TABLE [dbo].[OvertimeAvailability]
ALTER COLUMN [DateOfOvertime] [datetime] NOT NULL


----------------  
--Name: Erik Sundberg
--Date: 2013-10-03
--Desc: Bug #25008 - RTA does not differentiate between no scheduled activity and no alarm
---------------- 
ALTER TABLE dbo.StateGroupActivityAlarm ALTER COLUMN AlarmType uniqueidentifier NULL
GO

----------------  
--Name:CS
--Date: 2013-10-11
--Desc: Bug #25079 - Unclear permission setting for overtime availability
---------------- 
update dbo.ApplicationFunction
set FunctionCode = 'ModifyAvailabilities' where ForeignId = '0087'

update dbo.ApplicationFunction
set FunctionDescription = 'xxModifyAvailabilities' where ForeignId = '0087'
GO

----------------  
--Name: David J
--Date: 2013-10-22
--Desc: Bug #24969: Timeout during upgrade when running EXEC [dbo].[DayOffConverter]
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'IX_PersonAssignment_Scenario_UpdatedBy_UpdatedOn')
CREATE NONCLUSTERED INDEX [IX_PersonAssignment_Scenario_UpdatedBy_UpdatedOn]
ON [dbo].[PersonAssignment] ([Scenario])
INCLUDE ([UpdatedBy],[UpdatedOn])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonDayOff]') AND name = N'IX_PersonDayOff_BU_Name_Anchor_Person_Scenario')
CREATE NONCLUSTERED INDEX [IX_PersonDayOff_BU_Name_Anchor_Person_Scenario]
ON [dbo].[PersonDayOff] ([BusinessUnit],[Name])
INCLUDE ([Anchor],[Person],[Scenario])
GO

----------------  
--Name: David Jonsson
--Date: 2013-10-29
--Desc: Bug #25370 - re-design clustered key on dbo.OvertimeShiftActivityLayer
----------------
--No, not on this version. Tables is removed as part of "0000000386.sql"
GO

----------------  
--Name: David Jonsson
--Date: 2013-11-01
--Desc: Bug #25310 - Crash In ETL tool on stg_request, timout
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Request]') AND name = N'IX_Request_StartDateTime_Id_Parent')
CREATE NONCLUSTERED INDEX [IX_Request_StartDateTime_Id_Parent] ON [dbo].[Request]
(
	[StartDateTime] ASC
)
INCLUDE (
[Id],
[Parent]
)
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Request]') AND name = N'IX_Request_Parent')
CREATE NONCLUSTERED INDEX [IX_Request_Parent] ON [dbo].[Request]
(
	[Parent] ASC
)
INCLUDE ( 	[Id],
	[StartDateTime],
	[EndDateTime]
	)
GO

----------------  
--Name: David Jonsson
--Date: 2013-11-05
--Desc: Bug #25599 - ReadModel.GetNextActivityStartTime is super sloooow
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ReadModel].[ScheduleProjectionReadOnly]') AND name = N'IX_ScheduleProjectionReadOnly_PersonId_EndDateTime_StartDateTime')
CREATE NONCLUSTERED INDEX IX_ScheduleProjectionReadOnly_PersonId_EndDateTime_StartDateTime
ON [ReadModel].[ScheduleProjectionReadOnly]
(
	[PersonId],
	[EndDateTime]
)
INCLUDE
(
	[StartDateTime]
)
GO

----------------  
--Name: David Jonsson
--Date: 2013-11-05
--Desc: Bug #25359 - Prepare (redesign) the tables and optimize for the initial load of [ReadModel].[ScheduledResources] and [ReadModel].[ActivitySkillCombination]
-- Intial load of theese are disabled until we need decided on when to deploy "AnyWhere"
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ReadModel].[ScheduledResources]') AND name = N'CIX_ScheduledResources')
BEGIN
	CREATE TABLE [ReadModel].[ScheduledResources_new](
		[Id] [bigint] IDENTITY(1,1) NOT NULL,
		[ActivitySkillCombinationId] [int] NOT NULL,
		[Resources] [float] NOT NULL,
		[Heads] [float] NOT NULL,
		[PeriodStart] [datetime] NOT NULL,
		[PeriodEnd] [datetime] NOT NULL,
	 CONSTRAINT [PK_ScheduledResources_new] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)
	)

	CREATE CLUSTERED INDEX [CIX_ScheduledResources] ON [ReadModel].[ScheduledResources_new]
	(
		[ActivitySkillCombinationId] ASC,
		[PeriodStart] ASC
	)

	ALTER TABLE [ReadModel].[ScheduledResources] DROP CONSTRAINT [DF_ScheduledResources_Resources]
	ALTER TABLE [ReadModel].[ScheduledResources_new] ADD  CONSTRAINT [DF_ScheduledResources_Resources]  DEFAULT ((0)) FOR [Resources]

	ALTER TABLE [ReadModel].[ScheduledResources] DROP CONSTRAINT [DF_ScheduledResources_Heads]
	ALTER TABLE [ReadModel].[ScheduledResources_new] ADD  CONSTRAINT [DF_ScheduledResources_Heads]  DEFAULT ((0)) FOR [Heads]

	--Re-create data
	SET IDENTITY_INSERT [ReadModel].[ScheduledResources_new] ON
	INSERT INTO [ReadModel].[ScheduledResources_new](Id, ActivitySkillCombinationId, Resources, Heads, PeriodStart, PeriodEnd)
	SELECT Id, ActivitySkillCombinationId, Resources, Heads, PeriodStart, PeriodEnd
	FROM [ReadModel].[ScheduledResources]
	SET IDENTITY_INSERT [ReadModel].[ScheduledResources_new] OFF
	
	--drop old table
	DROP TABLE [ReadModel].[ScheduledResources]

	EXEC dbo.sp_rename @objname = N'[ReadModel].[ScheduledResources_new]', @newname = N'ScheduledResources', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[ReadModel].[ScheduledResources].[PK_ScheduledResources_new]', @newname = N'PK_ScheduledResources', @objtype =N'INDEX'
END
GO
--And a supporting index on [ReadModel].[ActivitySkillCombination]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ReadModel].[ActivitySkillCombination]') AND name = N'IX_ActivitySkillCombination')
	CREATE NONCLUSTERED INDEX [IX_ActivitySkillCombination] ON [ReadModel].[ActivitySkillCombination]
	(
		[Activity] ASC
	)
	INCLUDE ([Skills]) 
GO

--Re-design indexes on [dbo].[PersonAbsence]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAbsence]') AND name = N'CIX_PersonAbsence')
BEGIN
	CREATE TABLE [dbo].[PersonAbsence_new](
		[Id] [uniqueidentifier] NOT NULL,
		[Version] [int] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		[UpdatedBy] [uniqueidentifier] NOT NULL,
		[CreatedOn] [datetime] NOT NULL,
		[UpdatedOn] [datetime] NOT NULL,
		[LastChange] [datetime] NULL,
		[Person] [uniqueidentifier] NOT NULL,
		[Scenario] [uniqueidentifier] NOT NULL,
		[PayLoad] [uniqueidentifier] NOT NULL,
		[Minimum] [datetime] NOT NULL,
		[Maximum] [datetime] NOT NULL,
		[BusinessUnit] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_PersonAbsence_new] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)
	)

	CREATE CLUSTERED INDEX [CIX_PersonAbsence] ON [dbo].[PersonAbsence_new]
	(
		[Person] ASC
	)

	--Re-create data
	INSERT INTO [dbo].[PersonAbsence_new]
	SELECT * FROM [dbo].[PersonAbsence]
	
	--drop old table
	DROP TABLE [dbo].[PersonAbsence]

	EXEC dbo.sp_rename @objname = N'[dbo].[PersonAbsence_new]', @newname = N'PersonAbsence', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[dbo].[PersonAbsence].[PK_PersonAbsence_new]', @newname = N'PK_PersonAbsence', @objtype =N'INDEX'

	--Add additional indexes
	CREATE NONCLUSTERED INDEX [IX_PersonAbsence_BusinessUnit] ON [dbo].[PersonAbsence]
	(
		[BusinessUnit] ASC
	)

	CREATE NONCLUSTERED INDEX [IX_PersonAbsence_Scenario] ON [dbo].[PersonAbsence]
	(
		[Scenario] ASC
	)

	ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Absence] FOREIGN KEY([PayLoad])
	REFERENCES [dbo].[Absence] ([Id])
	ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Absence]

	ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_BusinessUnit] FOREIGN KEY([BusinessUnit])
	REFERENCES [dbo].[BusinessUnit] ([Id])
	ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_BusinessUnit]

	ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Person_CreatedBy] FOREIGN KEY([CreatedBy])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Person_CreatedBy]

	ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Person_UpdatedBy]

	ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Person3] FOREIGN KEY([Person])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Person3]

	ALTER TABLE [dbo].[PersonAbsence]  WITH CHECK ADD  CONSTRAINT [FK_PersonAbsence_Scenario] FOREIGN KEY([Scenario])
	REFERENCES [dbo].[Scenario] ([Id])
	ALTER TABLE [dbo].[PersonAbsence] CHECK CONSTRAINT [FK_PersonAbsence_Scenario]
END
GO

----------------  
--Name: Robin Karlsson
--Date: 2013-11-25
--Desc: Bug #25580 - Remove invalid negative values for tasks and campaign tasks (most likely from statistics calculation failure)
---------------- 
UPDATE dbo.TemplateTaskPeriod SET Tasks = 0 WHERE Tasks<0
UPDATE dbo.TemplateTaskPeriod SET CampaignTasks = -1 WHERE CampaignTasks<-1
GO

----------------  
--Name: David Jonsson
--Date: 2013-12-10
--Desc: Bug #26202 - ETL fails due to duplicated rows in readmodel
---------------- 
--remove potential duplicates
delete from a
from
(select PersonId,BelongsToDate
       ,ROW_NUMBER() over (partition by  PersonId,BelongsToDate
                           order by InsertedOn desc) RowNumber --keep last updated
from readmodel.ScheduleDay) as a
where a.RowNumber > 1
go

----------------  
--Name: David Jonsson
--Date: 2013-12-10
--Desc: Bug #26244 - Remove obsolete/never used PK => Id (guid) from readmodel
---------------- 
--Re-design tables and clustered index
if exists(select * from sys.columns 
            where Name = N'Id' and Object_ID = Object_ID(N'[ReadModel].[ScheduleDay]'))
BEGIN
	--remove potential duplicates
	delete from a
	from
		(
		select
			PersonId,
			BelongsToDate,
			ROW_NUMBER() over
				(partition by
					PersonId,
					BelongsToDate
				order by
					InsertedOn desc
				) RowNumber --keep last updated
		from readmodel.ScheduleDay
		) as a
	where a.RowNumber > 1

	CREATE TABLE [ReadModel].[ScheduleDay_new](
		[PersonId] [uniqueidentifier] NOT NULL,
		[BelongsToDate] [smalldatetime] NOT NULL,
		[StartDateTime] [datetime] NOT NULL,
		[EndDateTime] [datetime] NOT NULL,
		[Workday] [bit] NOT NULL,
		[WorkTime] [bigint] NOT NULL,
		[ContractTime] [bigint] NOT NULL,
		[Label] [nvarchar](50) NOT NULL,
		[DisplayColor] [int] NOT NULL,
		[InsertedOn] [datetime] NOT NULL,
		[NotScheduled] [bit] NOT NULL,
	 CONSTRAINT [PK_ScheduleDay_new] PRIMARY KEY NONCLUSTERED 
	(
		[PersonId] ASC,
		[BelongsToDate] ASC
	)
	)
	
	--save current data into new table structure
	INSERT INTO [ReadModel].[ScheduleDay_new](PersonId, BelongsToDate, StartDateTime, EndDateTime, Workday, WorkTime, ContractTime, Label, DisplayColor, InsertedOn, NotScheduled)
	SELECT PersonId, BelongsToDate, StartDateTime, EndDateTime, Workday, WorkTime, ContractTime, Label, DisplayColor, InsertedOn, NotScheduled
	FROM [ReadModel].[ScheduleDay]

	DROP TABLE [ReadModel].[ScheduleDay]
	
	EXEC dbo.sp_rename @objname = N'[ReadModel].[ScheduleDay_new]', @newname = N'ScheduleDay', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[ReadModel].[ScheduleDay].[PK_ScheduleDay_new]', @newname = N'PK_ScheduleDay', @objtype =N'INDEX'
	
	ALTER TABLE [ReadModel].[ScheduleDay] ADD  CONSTRAINT [DF_ScheduleDay_InsertedOn]  DEFAULT (getutcdate()) FOR [InsertedOn]

	ALTER TABLE [ReadModel].[ScheduleDay] ADD  CONSTRAINT [DF_ScheduleDay_NotScheduled]  DEFAULT ((0)) FOR [NotScheduled]
END
GO

if exists(select * from sys.columns 
            where Name = N'Id' and Object_ID = Object_ID(N'[ReadModel].[PersonScheduleDay]'))
BEGIN

	--remove potential duplicates
	delete from a
	from
		(
		select
			PersonId,
			BelongsToDate,
			ROW_NUMBER() over
				(partition by
					PersonId,
					BelongsToDate
				order by
					InsertedOn desc
				) RowNumber --keep last updated
		from readmodel.PersonScheduleDay
		) as a
	where a.RowNumber > 1

	CREATE TABLE [ReadModel].[PersonScheduleDay_new](
		[PersonId] [uniqueidentifier] NOT NULL,
		[TeamId] [uniqueidentifier] NOT NULL,
		[BelongsToDate] [smalldatetime] NOT NULL,
		[ShiftStart] [datetime] NULL,
		[ShiftEnd] [datetime] NULL,
		[SiteId] [uniqueidentifier] NOT NULL,
		[BusinessUnitId] [uniqueidentifier] NOT NULL,
		[InsertedOn] [datetime] NOT NULL,
		[Shift] [nvarchar](max) NOT NULL,
	 CONSTRAINT [PK_PersonScheduleDay_new] PRIMARY KEY NONCLUSTERED 
	(
		[PersonId] ASC,
		[BelongsToDate] ASC
	)
	)

	--save current data into new table structure
	INSERT INTO [ReadModel].[PersonScheduleDay_new](PersonId, TeamId, BelongsToDate, ShiftStart, ShiftEnd, SiteId, BusinessUnitId, InsertedOn, Shift)
	SELECT PersonId, TeamId, BelongsToDate, ShiftStart, ShiftEnd, SiteId, BusinessUnitId, InsertedOn, Shift
	FROM [ReadModel].[PersonScheduleDay]

	DROP TABLE [ReadModel].[PersonScheduleDay]
	
	EXEC dbo.sp_rename @objname = N'[ReadModel].[PersonScheduleDay_new]', @newname = N'PersonScheduleDay', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[ReadModel].[PersonScheduleDay].[PK_PersonScheduleDay_new]', @newname = N'PK_PersonScheduleDay', @objtype =N'INDEX'

	ALTER TABLE [ReadModel].[PersonScheduleDay] ADD  CONSTRAINT [DF_PersonScheduleDay_InsertedOn]  DEFAULT (getutcdate()) FOR [InsertedOn]
END
GO
