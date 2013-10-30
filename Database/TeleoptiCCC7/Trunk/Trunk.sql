

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
--Desc: Bug #25358 - re-design clustered key on ReadModel.PersonScheduleDay
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ReadModel].[PersonScheduleDay]') AND name = N'CIX_PersonScheduleDay')
BEGIN
	CREATE TABLE [ReadModel].[New_PersonScheduleDay](
		[Id] [uniqueidentifier] NOT NULL,
		[PersonId] [uniqueidentifier] NOT NULL,
		[TeamId] [uniqueidentifier] NOT NULL,
		[BelongsToDate] [smalldatetime] NOT NULL,
		[ShiftStart] [datetime] NULL,
		[ShiftEnd] [datetime] NULL,
		[SiteId] [uniqueidentifier] NOT NULL,
		[BusinessUnitId] [uniqueidentifier] NOT NULL,
		[InsertedOn] [datetime] NOT NULL,
		[Shift] [nvarchar](max) NOT NULL
	)

	ALTER TABLE [ReadModel].[New_PersonScheduleDay] ADD  CONSTRAINT [new_PK_PersonScheduleDay] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)

	CREATE CLUSTERED INDEX [CIX_PersonScheduleDay] ON [ReadModel].[New_PersonScheduleDay]
	(
		[PersonId] ASC,
		[BelongsToDate] ASC
	)

	ALTER TABLE [ReadModel].[PersonScheduleDay] DROP CONSTRAINT [DF_PersonScheduleDay_InsertedOn]
	ALTER TABLE [ReadModel].[New_PersonScheduleDay] ADD  CONSTRAINT [DF_PersonScheduleDay_InsertedOn]  DEFAULT (getutcdate()) FOR [InsertedOn]

	--Re-create data
	INSERT INTO [ReadModel].[New_PersonScheduleDay]
	SELECT * FROM [ReadModel].[PersonScheduleDay]

	--drop old table
	DROP TABLE [ReadModel].[PersonScheduleDay]

	EXEC dbo.sp_rename @objname = N'[ReadModel].[New_PersonScheduleDay]', @newname = N'PersonScheduleDay', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[ReadModel].[PersonScheduleDay].[new_PK_PersonScheduleDay]', @newname = N'PK_PersonScheduleDay', @objtype =N'INDEX'
END
GO

----------------  
--Name: David Jonsson
--Date: 2013-10-29
--Desc: Bug #25370 - re-design clustered key on dbo.OvertimeShiftActivityLayer
----------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OvertimeShiftActivityLayer]') AND name = N'CIX_OvertimeShiftActivityLayer_Parent')
BEGIN
	ALTER TABLE [dbo].[OvertimeShiftActivityLayer] DROP CONSTRAINT [FK_OvertimeShiftActivityLayer_Activity]
	ALTER TABLE [dbo].[OvertimeShiftActivityLayer] DROP CONSTRAINT [FK_OvertimeShiftActivityLayer_DefinitionSet]
	ALTER TABLE [dbo].[OvertimeShiftActivityLayer] DROP CONSTRAINT [FK_OvertimeShiftActivityLayer_OvertimeShift]

	CREATE TABLE [dbo].[New_OvertimeShiftActivityLayer](
		[Id] [uniqueidentifier] NOT NULL,
		[payLoad] [uniqueidentifier] NOT NULL,
		[Minimum] [datetime] NOT NULL,
		[Maximum] [datetime] NOT NULL,
		[DefinitionSet] [uniqueidentifier] NOT NULL,
		[Parent] [uniqueidentifier] NOT NULL,
		[OrderIndex] [int] NOT NULL,
	CONSTRAINT [New_PK_OvertimeShiftActivityLayer] PRIMARY KEY NONCLUSTERED 
	(
		[Id]
	)
	)

	CREATE CLUSTERED INDEX [CIX_OvertimeShiftActivityLayer_Parent] ON [dbo].[New_OvertimeShiftActivityLayer]
	(
		[Parent] ASC
	)

	INSERT INTO [dbo].[New_OvertimeShiftActivityLayer]
	SELECT * FROM [dbo].[OvertimeShiftActivityLayer]

	DROP TABLE [dbo].[OvertimeShiftActivityLayer]

	EXEC dbo.sp_rename @objname = N'[dbo].[New_OvertimeShiftActivityLayer]', @newname = N'OvertimeShiftActivityLayer', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[dbo].[OvertimeShiftActivityLayer].[New_PK_OvertimeShiftActivityLayer]', @newname = N'PK_OvertimeShiftActivityLayer', @objtype =N'INDEX'


	ALTER TABLE [dbo].[OvertimeShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_Activity] FOREIGN KEY([payLoad])
	REFERENCES [dbo].[Activity] ([Id])
	ALTER TABLE [dbo].[OvertimeShiftActivityLayer] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_Activity]

	ALTER TABLE [dbo].[OvertimeShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_DefinitionSet] FOREIGN KEY([DefinitionSet])
	REFERENCES [dbo].[MultiplicatorDefinitionSet] ([Id])
	ALTER TABLE [dbo].[OvertimeShiftActivityLayer] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_DefinitionSet]


	ALTER TABLE [dbo].[OvertimeShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_OvertimeShift] FOREIGN KEY([Parent])
	REFERENCES [dbo].[OvertimeShift] ([Id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[OvertimeShiftActivityLayer] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_OvertimeShift]
END
GO