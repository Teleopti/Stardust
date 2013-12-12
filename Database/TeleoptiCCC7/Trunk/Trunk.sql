
----------------  
--Name: Erik Sundberg
--Date: 2013-05-20
--Desc: Bug #23519 Duplicate OrderIndex in ActivityExtender
----------------  

SET NOCOUNT ON
-------------------
--Create table to hold error layers
-------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityExtenderWrongOrderIndex]') AND type in (N'U'))
BEGIN 
	CREATE TABLE dbo.ActivityExtenderWrongOrderIndex
	(
		Batch int NOT NULL,
		UpdatedOn smalldatetime NOT NULL,
		Id uniqueidentifier NOT NULL,
		Parent uniqueidentifier NOT NULL,
		OrderIndexOld int NOT NULL,
		OrderIndexNew int NOT NULL
	)

	ALTER TABLE dbo.ActivityExtenderWrongOrderIndex 
	ADD CONSTRAINT PK_ActivityExtenderWrongOrderIndex PRIMARY KEY (Batch, Id)
END

-------------------
--Save error layers
-------------------
DECLARE @Batch int
DECLARE @UpdatedOn smalldatetime
SELECT @UpdatedOn = GETDATE()
SELECT @Batch = ISNULL(MAX(Batch),0) FROM ActivityExtenderWrongOrderIndex
SET @Batch = @Batch + 1

INSERT INTO dbo.ActivityExtenderWrongOrderIndex
SELECT
		@Batch,
		@UpdatedOn,
		ps1.Id,
		ps1.Parent,
		ps1.OrderIndex as OrderIndexOld,
		ActivityExtenderIndexChange.rn-1 as OrderindexNew
	FROM ActivityExtender ps1
	INNER JOIN
	(
		SELECT ps2.id, ps2.parent, ps2.orderindex, ROW_NUMBER()OVER(PARTITION BY ps2.parent ORDER BY ps2.orderindex) rn
		FROM ActivityExtender ps2
	) ActivityExtenderIndexChange
	ON ps1.id=ActivityExtenderIndexChange.id
	WHERE ActivityExtenderIndexChange.OrderIndex <> ActivityExtenderIndexChange.rn-1

-------------------
--Update error layers
-------------------
UPDATE ActivityExtender
SET 
	OrderIndex	= fix.OrderindexNew
FROM dbo.ActivityExtenderWrongOrderIndex fix
WHERE fix.Id = ActivityExtender.Id AND fix.Parent = ActivityExtender.Parent
AND fix.Batch = @Batch

-------------------
--Report error layers
-------------------
IF (SELECT COUNT(1) FROM dbo.ActivityExtenderWrongOrderIndex WHERE Batch=@Batch)> 0
PRINT 'Shifts have been updated'

SET NOCOUNT OFF
GO

----------------  
--Name: David
--Date: 2013-05-31
--Desc: #23741 - Add clustered index
-----------------
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[LastUpdatedPerStep]') AND type in (N'U'))
DROP TABLE [mart].[LastUpdatedPerStep]
GO

CREATE TABLE [mart].[LastUpdatedPerStep](
	[StepName] [varchar](500) NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL
)
GO

ALTER TABLE [mart].[LastUpdatedPerStep] ADD  CONSTRAINT [PK_LastUpdatedPerStep] PRIMARY KEY CLUSTERED 
(
	[StepName] ASC,
	[BusinessUnit] ASC
)
GO

----------------  
--Name: David
--Date: 2013-05-31
--Desc: #23675 - Force ETL permission to run once
-----------------
DECLARE @isoDate datetime
SET @isoDate = '2001-01-01T00:00:00.000'

INSERT INTO [mart].[LastUpdatedPerStep]
SELECT
	[StepName] =  'Permissions',
	[BusinessUnit] = bu.Id,
	[Date] = @isoDate
FROM  dbo.BusinessUnit bu
WHERE NOT EXISTS (
			SELECT *
			FROM [mart].[LastUpdatedPerStep] a
			WHERE bu.Id = a.BusinessUnit
			)
GO

------------------
-- Name: Ola
-- Date: 2013-05-31
-- #23691 - missing permission function for Intraday reforecasting
-------------------------
SET NOCOUNT ON
	
--declarations
DECLARE @SuperUserId as uniqueidentifier
DECLARE @FunctionId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

--insert to super user if not exist
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

-- check for the existence of super user role
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1,@SuperUserId, @SuperUserId, getdate(), getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0018'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0085' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ReForecasting' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxReforecast' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: David Jonsson
--Date: 2013-06-04
--Desc: Bug #23760 rearrange and add indexes for better performance
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MeetingPerson]') AND name = N'CIX_MeetingPerson_Parent')
BEGIN
	SET NOCOUNT ON

	CREATE TABLE [dbo].[MeetingPerson_new](
		[Id] [uniqueidentifier] NOT NULL,
		[Person] [uniqueidentifier] NOT NULL,
		[Parent] [uniqueidentifier] NOT NULL,
		[Optional] [bit] NOT NULL,
	 CONSTRAINT [PK_MeetingPerson_new] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)
	)

	CREATE CLUSTERED INDEX [CIX_MeetingPerson_Parent] ON [dbo].[MeetingPerson_new]
	(
		[Parent] ASC
	)

	INSERT INTO [dbo].[MeetingPerson_new] ([Id],[Person],[Parent],[Optional])
	SELECT [Id],[Person],[Parent],[Optional]
	FROM [dbo].[MeetingPerson]

	DROP TABLE [dbo].[MeetingPerson]

	CREATE NONCLUSTERED INDEX [IX_MeetingPerson_Person_Parent] ON [dbo].[MeetingPerson_new]
	(
		[Person] ASC
	)
	INCLUDE ([Parent])

	EXEC dbo.sp_rename @objname = N'[dbo].[MeetingPerson_new]', @newname = N'MeetingPerson', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[dbo].[MeetingPerson].[PK_MeetingPerson_new]', @newname = N'PK_MeetingPerson', @objtype =N'INDEX'

	ALTER TABLE [dbo].[MeetingPerson]  WITH CHECK ADD  CONSTRAINT [FK_MeetingPerson_Meeting] FOREIGN KEY([Parent])
	REFERENCES [dbo].[Meeting] ([Id])
	ALTER TABLE [dbo].[MeetingPerson] CHECK CONSTRAINT [FK_MeetingPerson_Meeting]

	ALTER TABLE [dbo].[MeetingPerson]  WITH CHECK ADD  CONSTRAINT [FK_MeetingPerson_Person] FOREIGN KEY([Person])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[MeetingPerson] CHECK CONSTRAINT [FK_MeetingPerson_Person]

	SET NOCOUNT OFF
END
GO

----------------  
--Name: David Jonsson
--Date: 2013-06-05
--Desc: Bug #23770 - try to reduce some of the I/O
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriod]') AND name = N'IX_PersonPeriod_StartDate')
CREATE NONCLUSTERED INDEX [IX_PersonPeriod_StartDate] ON [dbo].[PersonPeriod]
(
	[StartDate] ASC
)
INCLUDE ([Parent],[Team])
GO

----------------  
--Name: David Jonsson
--Date: 2013-06-12
--Desc: Bug #23815 - remove duplicates from PersonAbsence
--Desc: Bug #24721 - patch deletes absences incorrect from PersonAbsence
---------------- 
create table #PersonAbsenceRemove (Id uniqueidentifier)
CREATE TABLE #PersonAbsence (
	[Id] [uniqueidentifier] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[PayLoad] [uniqueidentifier] NOT NULL,
	[Minimum] [datetime] NOT NULL,
	[Maximum] [datetime] NOT NULL)

--First get all correct (unique) rows keeping only the longest Absence
--note: The [Id] column will still represent some duplicated rows as [Id] is not included in the GROUP BY
insert into #PersonAbsence (Id, Person, Scenario, PayLoad, Minimum, Maximum)
select t1.Id, t1.Person, t1.Scenario, t1.PayLoad, t1.Minimum, t1.Maximum
from PersonAbsence t1
inner join (
	select person, Scenario, Minimum, PayLoad, max(Maximum) Maximum
	from PersonAbsence
	group by person, Scenario, Minimum,PayLoad
) t2
on t1.Person = t2.Person
	and t1.Minimum = t2.Minimum
	and t1.PayLoad = t2.PayLoad
	and t1.Maximum = t2.Maximum
	and t1.Scenario= t2.Scenario

--Secondly, we group again, now by lowest GUID ("random")
INSERT INTO #PersonAbsenceRemove
SELECT Id FROM PersonAbsence
where Id not in (
	select cast(min(cast(id as varchar(36))) as uniqueidentifier) Id
	from #PersonAbsence
	group by person, Scenario, Minimum,PayLoad,Maximum
)

--delete PersonAbsence_AUD
delete aud
from Auditing.PersonAbsence_AUD aud
inner join #PersonAbsenceRemove t
on aud.Id = t.Id

--delete PersonAbsence
delete pa
from dbo.PersonAbsence pa
inner join #PersonAbsenceRemove t
on pa.Id = t.Id
GO


----------------  
--Name: Robin Karlsson
--Date: 2013-07-15
--Desc: Bug #24018 - Fix messed up default values for Contract
---------------- 
UPDATE dbo.Contract SET AvgWorkTimePerDay = 288000000000,Version = Version+1 WHERE AvgWorkTimePerDay = 0
GO

UPDATE dbo.Contract SET MaxTimePerWeek=1728000000000,NightlyRest=396000000000,WeeklyRest=1296000000000,Version = Version+1 WHERE MaxTimePerWeek=0 AND NightlyRest=0 AND WeeklyRest=0
GO

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
--Name: Erik Sundberg
--Date: 2013-10-03
--Desc: Bug #25008 - RTA does not differentiate between no scheduled activity and no alarm
---------------- 
ALTER TABLE dbo.StateGroupActivityAlarm ALTER COLUMN AlarmType uniqueidentifier NULL
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
--Name: Jonas Nordh
--Date: 2013-12-12
--Desc: Bug #26054 - Add unique constraint to prevent duplicates in ReadModel.GroupingReadOnly
---------------- 
TRUNCATE TABLE [ReadModel].[GroupingReadOnly] 
IF NOT EXISTS (
	SELECT * FROM sys.indexes
	WHERE object_id = OBJECT_ID(N'[ReadModel].[GroupingReadOnly]')
	AND name = N'UC_GroupingReadOnly'
	)
ALTER TABLE [ReadModel].[GroupingReadOnly] ADD CONSTRAINT UC_GroupingReadOnly UNIQUE (PersonId, StartDate, GroupId)
GO
