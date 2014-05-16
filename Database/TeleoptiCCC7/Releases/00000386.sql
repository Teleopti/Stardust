SET NOCOUNT ON
PRINT '-----------------------'
PRINT 'Release 386 will re-design the schedule/shift tables and convert a lot of data.'
PRINT 'This will need some extra time to finish. Please be patient'
PRINT 'Start time: ' + CONVERT(VARCHAR(24),GETDATE(),113)
PRINT '-----------------------'

----------------  
--Name: Anders Forsberg
--Date: 2014-01-24
--Desc: #26710 Need to remove unwanted duplicates from schedule before converting to the new PersonAssignment
--Duplicates, with same minimum, but maybe different updatedon
delete PersonAssignment
from PersonAssignment paD inner join
	(select pa.id,
	--We want to keep the duplicate that was created first, because that is how 384 displays in Schedules
	--If multiple modified same time keep first one (lowest id)
	ROW_NUMBER()OVER(PARTITION BY pa.person, pa.scenario, pa.minimum ORDER BY pa.updatedon, pa.id) as 'rn'
	from PersonAssignment pa) inside on paD.id = inside.id and inside.rn <> 1

--Overlapping, with different minimum, Schedules in 384 displays the one with earliest start = minimum
delete PersonAssignment
from PersonAssignment pD
where exists (
	select 1
	from PersonAssignment pa
	where pD.Scenario = pa.Scenario
	and pD.Person = pa.Person
	and pD.Minimum between pa.Minimum and pa.Maximum
	and pD.id <> pa.id
	and not (pD.Minimum = pa.Minimum))
GO

----------------  
--Name: MickeD
--Date: 2014-05-16
--Desc: Bug #27785 Need to delete person assignments that does not have a main-, personal- or overtime-shift
---------------- 
--this index is added by Security.exe. If the UQ-index doesn't exist, try fix the data before converting data to new PA-structure
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'UQ_PersonAssignment_Date_Scenario_Person')
BEGIN
            delete [dbo].[PersonAssignment]
            from [dbo].[PersonAssignment]
            left join [dbo].[MainShift] on [dbo].[PersonAssignment].[Id] = [dbo].[MainShift].[Id]
            left join [dbo].[PersonalShift] on [dbo].[PersonAssignment].[Id] = [dbo].[PersonalShift].[Id]
            left join [dbo].[OvertimeShift] on [dbo].[PersonAssignment].[Id] = [dbo].[OvertimeShift].[Id]
            where [dbo].[MainShift].[Id] is null
            and [dbo].[PersonalShift].[Id] is null
            and [dbo].[OvertimeShift].[Id] is null
END
GO

----------------  
--Name: Roger Kratz
--Date: 2013-04-15
--Desc: Adding date for person assignment. Hard coded to 1800-1-1. Will be replaced by .net code
ALTER TABLE dbo.PersonAssignment
ADD [Date] datetime
GO

declare @Date datetime
set @Date = '1800-01-01T00:00:00'
update dbo.PersonAssignment
set [Date] = @Date
GO

ALTER TABLE dbo.PersonAssignment
ALTER COLUMN [Date] datetime not null
GO

----------------  
--Name: Roger Kratz
--Date: 2013-05-15

---------------- MAIN TABLES --------------------------

--Add shiftcategory to personassignment
ALTER TABLE dbo.PersonAssignment
add ShiftCategory uniqueidentifier

ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_ShiftCategory] FOREIGN KEY([ShiftCategory])
REFERENCES [dbo].[ShiftCategory] ([Id])
GO

ALTER TABLE [dbo].[PersonAssignment] CHECK CONSTRAINT [FK_PersonAssignment_ShiftCategory]
GO

--move data for ShiftCategory
UPDATE pa
SET pa.ShiftCategory = ms.ShiftCategory
FROM dbo.PersonAssignment pa
INNER JOIN dbo.MainShift ms
	ON pa.Id = ms.Id

--add mainshiftlayers from personassignment
ALTER TABLE [dbo].[MainShiftActivityLayer] DROP CONSTRAINT [FK_MainShiftActivityLayer_MainShift]
GO

ALTER TABLE [dbo].[MainShiftActivityLayer]  WITH CHECK ADD  CONSTRAINT [FK_MainShiftActivityLayer_PersonAssignment] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonAssignment] ([Id])
on delete cascade
GO

ALTER TABLE [dbo].[MainShiftActivityLayer] CHECK CONSTRAINT [FK_MainShiftActivityLayer_PersonAssignment]
GO

--drop mainshift table
DROP TABLE dbo.MainShift
GO

----------------  
--Name: Kunning Mao
--Date: 2013-05-20
--Desc: Empty read models in order to re-fill it with compressed shifts
----------------  
TRUNCATE TABLE [ReadModel].[PersonScheduleDay]
GO

ALTER TABLE [ReadModel].[PersonScheduleDay] ALTER COLUMN [Shift] nvarchar(2000)
GO


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
	[BusinessUnit] uniqueidentifier NOT NULL,
	[Date] datetime NOT NULL
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

------------------
-- Name: David
-- Date: 2013-05-31
-- Desc: #23740 - rename PKs to match table name
-------------------------
DECLARE @PKName nvarchar(1000)
SELECT @PKName= '[Auditing].[Security].['+name+']' FROM sys.indexes
WHERE OBJECT_NAME(object_id) = 'Security'
AND index_id = 1
AND is_primary_key = 1
SELECT @PKName
EXEC sp_rename @PKName, N'PK_Security', N'INDEX'
GO
DECLARE @PKName nvarchar(1000)
SELECT @PKName= '[ReadModel].[ScheduleDay].['+name+']' FROM sys.indexes
WHERE OBJECT_NAME(object_id) = 'ScheduleDay'
AND is_primary_key = 1
SELECT @PKName
EXEC sp_rename @PKName, N'PK_ScheduleDay', N'INDEX'
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
		[Id] uniqueidentifier NOT NULL,
		[Person] uniqueidentifier NOT NULL,
		[Parent] uniqueidentifier NOT NULL,
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

------------------
-- Name: Ola
-- Date: 2013-06-14
-- Add permission to view the active agents
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
SELECT @ParentForeignId = '0023'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0086' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ViewActiveAgents' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxViewActiveAgents' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')



----------------  
--Name: David Jonsson
--Date: 2013-06-12
--Desc: #23812 - Migrate PersonalShift and OvertimeShift to PersonAssignment
---------------- 

--1) [dbo].[PersonalShiftActivityLayer] 

--drop FKs
ALTER TABLE [dbo].[PersonalShift] DROP CONSTRAINT [FK_PersonalShift_PersonAssignment]
ALTER TABLE [dbo].[PersonalShiftActivityLayer] DROP CONSTRAINT [FK_PersonalShiftActivityLayer_Activity]
ALTER TABLE [dbo].[PersonalShiftActivityLayer] DROP CONSTRAINT [FK_PersonalShiftActivityLayer_PersonalShift]
GO

--Create new layer table
CREATE TABLE [dbo].[PersonalShiftActivityLayer_new](
	[Id] uniqueidentifier NOT NULL,
	[payLoad] uniqueidentifier NOT NULL,
	[Minimum] datetime NOT NULL,
	[Maximum] datetime NOT NULL,
	[Parent] uniqueidentifier NOT NULL,
	[OrderIndex] int NOT NULL,
 CONSTRAINT [PK_PersonalShiftActivityLayer_new] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)

CREATE CLUSTERED INDEX [CIX_PersonalShiftActivityLayer] ON [dbo].[PersonalShiftActivityLayer_new]
(
	[Parent] ASC
)
GO

--move data
INSERT INTO [dbo].[PersonalShiftActivityLayer_new] (Id, payLoad, Minimum, Maximum, Parent, OrderIndex)
select
L2.Id,
L2.payLoad,
L2.Minimum,
L2.Maximum,
L1.Parent,
NewOrderIndex = ROW_NUMBER() OVER(PARTITION BY L0.Id ORDER BY L1.OrderIndex,L2.OrderIndex) -1
from  dbo.PersonAssignment L0
inner join dbo.personalShift L1
	on L0.Id = L1.Parent
inner join dbo.PersonalShiftActivityLayer L2
	on L1.Id = L2.Parent
GO

ALTER TABLE [dbo].[PersonalShiftActivityLayer_new]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShiftActivityLayer_Activity] FOREIGN KEY([payLoad])
REFERENCES [dbo].[Activity] ([Id])
ALTER TABLE [dbo].[PersonalShiftActivityLayer_new] CHECK CONSTRAINT [FK_PersonalShiftActivityLayer_Activity]

ALTER TABLE [dbo].[PersonalShiftActivityLayer_new]  WITH CHECK ADD  CONSTRAINT [FK_PersonalShiftActivityLayer_PersonAssignment] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonAssignment] ([Id])
ON DELETE CASCADE
ALTER TABLE [dbo].[PersonalShiftActivityLayer_new] CHECK CONSTRAINT [FK_PersonalShiftActivityLayer_PersonAssignment]

DROP TABLE PersonalShiftActivityLayer
DROP TABLE PersonalShift
GO

EXEC sp_rename N'[dbo].[PersonalShiftActivityLayer_new]', N'PersonalShiftActivityLayer', N'OBJECT'
EXEC sp_rename N'[dbo].[PersonalShiftActivityLayer].[PK_PersonalShiftActivityLayer_new]', N'PK_PersonalShiftActivityLayer', N'INDEX'
GO

--2) [dbo].[OvertimeShiftActivityLayer] 

--drop FKs
ALTER TABLE [dbo].[OvertimeShift] DROP CONSTRAINT [FK_OvertimeShift_PersonAssignment]
ALTER TABLE [dbo].[OvertimeShiftActivityLayer] DROP CONSTRAINT [FK_OvertimeShiftActivityLayer_Activity]
ALTER TABLE [dbo].[OvertimeShiftActivityLayer] DROP CONSTRAINT [FK_OvertimeShiftActivityLayer_OvertimeShift]
GO

--Create new layer table
CREATE TABLE [dbo].[OvertimeShiftActivityLayer_new](
	[Id] uniqueidentifier NOT NULL,
	[payLoad] uniqueidentifier NOT NULL,
	[Minimum] datetime NOT NULL,
	[Maximum] datetime NOT NULL,
	[DefinitionSet] uniqueidentifier NOT NULL,
	[Parent] uniqueidentifier NOT NULL,
	[OrderIndex] int NOT NULL,
 CONSTRAINT [PK_OvertimeShiftActivityLayer_new] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)

CREATE CLUSTERED INDEX [CIX_OvertimeShiftActivityLayer] ON [dbo].[OvertimeShiftActivityLayer_new]
(
	[Parent] ASC
)
GO

--move data
INSERT INTO [dbo].[OvertimeShiftActivityLayer_new] (Id, payLoad, Minimum, Maximum, DefinitionSet, Parent, OrderIndex)
select
L2.Id,
L2.payLoad,
L2.Minimum,
L2.Maximum,
L2.DefinitionSet,
L1.Parent,
NewOrderIndex = ROW_NUMBER() OVER(PARTITION BY L0.Id ORDER BY L1.OrderIndex,L2.OrderIndex) -1
from  dbo.PersonAssignment L0
inner join dbo.OvertimeShift L1
	on L0.Id = L1.Parent
inner join dbo.OvertimeShiftActivityLayer L2
	on L1.Id = L2.Parent
GO

ALTER TABLE [dbo].[OvertimeShiftActivityLayer_new]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_Activity] FOREIGN KEY([payLoad])
REFERENCES [dbo].[Activity] ([Id])
ALTER TABLE [dbo].[OvertimeShiftActivityLayer_new] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_Activity]

ALTER TABLE [dbo].[OvertimeShiftActivityLayer_new]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_PersonAssignment] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonAssignment] ([Id])
ON DELETE CASCADE
ALTER TABLE [dbo].[OvertimeShiftActivityLayer_new] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_PersonAssignment]

DROP TABLE OvertimeShiftActivityLayer
DROP TABLE OvertimeShift
GO

EXEC sp_rename N'[dbo].[OvertimeShiftActivityLayer_new]', N'OvertimeShiftActivityLayer', N'OBJECT'
EXEC sp_rename N'[dbo].[OvertimeShiftActivityLayer].[PK_OvertimeShiftActivityLayer_new]', N'PK_OvertimeShiftActivityLayer', N'INDEX'
GO

----------------  
--Name: David J
--Date: 2013-06-24
--Desc: PBI #21978 - New Audit layout
----------------  
CREATE SCHEMA [Auditing_Old] AUTHORIZATION [dbo]
GO
ALTER SCHEMA [Auditing_Old]
    TRANSFER [Auditing].[PersonAssignment_AUD];
ALTER SCHEMA [Auditing_Old]
    TRANSFER [Auditing].[MainShiftActivityLayer_AUD];
ALTER SCHEMA [Auditing_Old]
    TRANSFER [Auditing].[MainShift_AUD];
ALTER SCHEMA [Auditing_Old]
    TRANSFER [Auditing].[OvertimeShiftActivityLayer_AUD]
ALTER SCHEMA [Auditing_Old]
    TRANSFER [Auditing].[OvertimeShift_AUD]
ALTER SCHEMA [Auditing_Old]
    TRANSFER [Auditing].[PersonalShiftActivityLayer_AUD]
ALTER SCHEMA [Auditing_Old]
    TRANSFER [Auditing].[PersonalShift_AUD]
ALTER SCHEMA [Auditing_Old]
    TRANSFER [Auditing].[PersonDayOff_AUD]
ALTER SCHEMA [Auditing_Old]
	TRANSFER [Auditing].[PersonAbsence_AUD]
ALTER SCHEMA [Auditing_Old]
	TRANSFER [Auditing].[Revision]
GO

CREATE TABLE [Auditing].[Revision](
	[Id] bigint IDENTITY(1,1) NOT NULL,
	[ModifiedAt] datetime NULL,
	[ModifiedBy] uniqueidentifier NOT NULL,
 CONSTRAINT [PK_Revision] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)
ALTER TABLE [Auditing].[Revision]  WITH CHECK ADD  CONSTRAINT [FK_Revision_Person] FOREIGN KEY([ModifiedBy])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [Auditing].[Revision] CHECK CONSTRAINT [FK_Revision_Person]
GO

CREATE TABLE [Auditing].[ShiftLayer_AUD](
	[Id] uniqueidentifier NOT NULL,
	[REV] bigint NOT NULL,
	[REVTYPE] tinyint NOT NULL,
	[Minimum] datetime NULL,
	[Maximum] datetime NULL,
	[OrderIndex] int NULL,
	[Payload] uniqueidentifier NULL,
	[LayerType] tinyint NULL,
	[DefinitionSet] uniqueidentifier NULL,
	[Parent] uniqueidentifier NULL,
 CONSTRAINT [PK_ShiftLayer_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
)

ALTER TABLE [Auditing].[ShiftLayer_AUD]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeShiftActivityLayer_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[ShiftLayer_AUD] CHECK CONSTRAINT [FK_OvertimeShiftActivityLayer_REV]
GO

CREATE TABLE [Auditing].[PersonAssignment_AUD](
	[Id] uniqueidentifier NOT NULL,
	[REV] bigint NOT NULL,
	[REVTYPE] tinyint NOT NULL,
	[Version] int NULL,
	[Minimum] datetime NULL,
	[Maximum] datetime NULL,
	[Person] uniqueidentifier NULL,
	[Scenario] uniqueidentifier NULL,
	[BusinessUnit] uniqueidentifier NULL,
	[ShiftCategory] uniqueidentifier NULL,
	[Date] datetime NULL
 CONSTRAINT [PK_PersonAssignment_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
)
ALTER TABLE [Auditing].[PersonAssignment_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Auditing].[FK_PersonAssignment_REV]') AND parent_object_id = OBJECT_ID(N'[Auditing].[PersonAssignment_AUD]'))
ALTER TABLE [Auditing].[PersonAssignment_AUD] CHECK CONSTRAINT [FK_PersonAssignment_REV]


GO

CREATE TABLE [Auditing].[PersonAbsence_AUD](
	[Id] uniqueidentifier NOT NULL,
	[REV] bigint NOT NULL,
	[REVTYPE] tinyint NOT NULL,
	[Version] int NULL,
	[LastChange] datetime NULL,
	[Minimum] datetime NULL,
	[Maximum] datetime NULL,
	[Person] uniqueidentifier NULL,
	[Scenario] uniqueidentifier NULL,
	[Payload] uniqueidentifier NULL,
	[BusinessUnit] uniqueidentifier NULL,
 CONSTRAINT [PK_PersonAbsence_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
)

ALTER TABLE [Auditing].[PersonAbsence_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonalAbsence_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[PersonAbsence_AUD] CHECK CONSTRAINT [FK_PersonalAbsence_REV]


GO
CREATE TABLE [Auditing].[PersonDayOff_AUD](
	[Id] uniqueidentifier NOT NULL,
	[REV] bigint NOT NULL,
	[REVTYPE] tinyint NOT NULL,
	[Version] int NULL,
	[Anchor] datetime NULL,
	[TargetLength] bigint NULL,
	[Flexibility] bigint NULL,
	[DisplayColor] int NULL,
	[PayrollCode] nvarchar(20) NULL,
	[Name] nvarchar(50) NULL,
	[ShortName] nvarchar(25) NULL,
	[Person] uniqueidentifier NULL,
	[Scenario] uniqueidentifier NULL,
	[BusinessUnit] uniqueidentifier NULL,
 CONSTRAINT [PK_PersonDayOff_AUD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[REV] ASC
)
)
ALTER TABLE [Auditing].[PersonDayOff_AUD]  WITH CHECK ADD  CONSTRAINT [FK_PersonDayOff_REV] FOREIGN KEY([REV])
REFERENCES [Auditing].[Revision] ([Id])
ON DELETE CASCADE

ALTER TABLE [Auditing].[PersonDayOff_AUD] CHECK CONSTRAINT [FK_PersonDayOff_REV]
GO

----------------  
--Name: Asad Mirza
--Date: 2013-06-10
--Desc: Added a new table for overtime availability day
----------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OvertimeAvailability]') AND type in (N'U'))
BEGIN

	CREATE TABLE [dbo].[OvertimeAvailability](
		[Id] [uniqueidentifier] NOT NULL,
		[Person] [uniqueidentifier] NOT NULL,
		[DateOfOvertime] [datetime] NOT NULL,
		[StartTime] [bigint] NULL,
		[EndTime] [bigint] NULL,
		[BusinessUnit] [uniqueidentifier] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		[UpdatedBy] [uniqueidentifier] NOT NULL,
		[CreatedOn] [datetime] NOT NULL,
		[UpdatedOn] [datetime] NOT NULL,
		CONSTRAINT [PK_OvertimeAvailability] PRIMARY KEY NONCLUSTERED
(
	[Id] ASC
))
	ALTER TABLE [dbo].[OvertimeAvailability]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeAvailability_BusinessUnit] FOREIGN KEY([BusinessUnit])
	REFERENCES [dbo].[BusinessUnit] ([Id])
	
	ALTER TABLE [dbo].[OvertimeAvailability]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeAvailability_Person] FOREIGN KEY([Person])
	REFERENCES [dbo].[Person] ([Id])
	
	CREATE CLUSTERED INDEX [CIX_OvertimeAvailability] ON [dbo].[OvertimeAvailability]
	(
		[Person] ASC
	)

END


------------------
-- Name: CS
-- Date: 2013-06-11
-- application function for Overtime availability
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
SELECT @ParentForeignId = '0023'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0087' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'OvertimeAvailability' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxOvertimeAvailability' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO
----------------  
--Name: Ola Håkansson
--Date: 2013-06-13
--Desc: Adding the new skill type Retail to the database.
----------------  
DECLARE @creator uniqueidentifier
SELECT TOP 1 @creator=CreatedBy FROM SkillType
IF @creator IS NULL
	BEGIN
		SELECT TOP 1 @creator=Id FROM Person
	END
INSERT INTO SkillType (Id,ForecastType,Version,CreatedBy,UpdatedBy,CreatedOn,UpdatedOn,Name,ShortName,ForecastSource,IsDeleted)
VALUES (NEWID(),0,1,@creator,@creator,GETUTCDATE(),GETUTCDATE(),N'SkillTypeChat',null,9,0)

GO
----------------  
--Name: Ola Håkansson
--Date: 2013-06-13
--Desc: Adding MaxParallelTasks to Skill.
---------------- 
ALTER TABLE Skill
ADD MaxParallelTasks int NULL
GO
UPDATE Skill SET MaxParallelTasks = 1
ALTER TABLE Skill ALTER COLUMN MaxParallelTasks int not null
GO

----------------  
--Name: David Jonsson
--Date: 2013-07-01
--Desc: PBI #21978 - merging all shiftLayer tables into one
----------------  
CREATE TABLE [dbo].[ShiftLayer](
	[Id] uniqueidentifier NOT NULL,
	[payLoad] uniqueidentifier NOT NULL,
	[Minimum] datetime NOT NULL,
	[Maximum] datetime NOT NULL,
	[Parent] uniqueidentifier NOT NULL,
	[OrderIndex] int NOT NULL,
	[LayerType] tinyint NOT NULL,
	[DefinitionSet] uniqueidentifier NULL,
	CONSTRAINT [PK_ShiftLayer] PRIMARY KEY NONCLUSTERED 	
	(	
		[Id] ASC
	)	
)
CREATE CLUSTERED INDEX [CIX_ShiftLayer_Parent] ON [dbo].[ShiftLayer]
(
            [Parent] ASC
)

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ShiftLayer_Activity]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShiftLayer]'))
ALTER TABLE [dbo].[ShiftLayer]  WITH NOCHECK ADD  CONSTRAINT [FK_ShiftLayer_Activity] FOREIGN KEY([payLoad])
REFERENCES [dbo].[Activity] ([Id])

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ShiftLayer_Activity]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShiftLayer]'))
ALTER TABLE [dbo].[ShiftLayer] CHECK CONSTRAINT [FK_ShiftLayer_Activity]

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ShiftLayer_PersonAssignment]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShiftLayer]'))
ALTER TABLE [dbo].[ShiftLayer]  WITH CHECK ADD  CONSTRAINT [FK_ShiftLayer_PersonAssignment] FOREIGN KEY([Parent])
REFERENCES [dbo].[PersonAssignment] ([Id])
ON DELETE CASCADE

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ShiftLayer_PersonAssignment]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShiftLayer]'))
ALTER TABLE [dbo].[ShiftLayer] CHECK CONSTRAINT [FK_ShiftLayer_PersonAssignment]

GO

INSERT INTO [dbo].[ShiftLayer]
SELECT
	[Id]		= Id,
	[payLoad]	= [payLoad],
	[Minimum]	= [Minimum],
	[Maximum]	= [Maximum],
	[Parent]	= [Parent],
	[OrderIndex]= [OrderIndex],
	[LayerType]	= 1,
	[DefinitionSet] = NULL
FROM dbo.MainShiftActivityLayer

INSERT INTO [dbo].[ShiftLayer]
SELECT
	[Id]		= o.[Id],
	[payLoad]	= o.[payLoad],
	[Minimum]	= o.[Minimum],
	[Maximum]	= o.[Maximum],
	[Parent]	= o.[Parent] ,
	[OrderIndex]=
		CASE WHEN s.Parent IS NULL THEN o.[OrderIndex]
			ELSE o.[OrderIndex] + s.OrderIndex + 1
		END,
	[LayerType]	= 2,
	[DefinitionSet] = o.DefinitionSet
FROM dbo.OvertimeShiftActivityLayer o
LEFT OUTER JOIN
	(
	SELECT Parent,MAX(OrderIndex) AS OrderIndex
	FROM [dbo].[ShiftLayer]
	GROUP BY Parent
	) s
	ON s.Parent = o.Parent

INSERT INTO [dbo].[ShiftLayer]
SELECT
	[Id]		= p.[Id],
	[payLoad]	= p.[payLoad],
	[Minimum]	= p.[Minimum],
	[Maximum]	= p.[Maximum],
	[Parent]	= p.[Parent] ,
	[OrderIndex]=
		CASE WHEN s.Parent IS NULL THEN p.[OrderIndex]
			ELSE p.[OrderIndex] + s.OrderIndex + 1
		END,

	[LayerType]	= 3,
	[DefinitionSet] = NULL
FROM dbo.PersonalShiftActivityLayer p
LEFT OUTER JOIN
	(
	SELECT Parent,MAX(OrderIndex) AS OrderIndex
	FROM [dbo].[ShiftLayer]
	GROUP BY Parent
	) s
	ON s.Parent = p.Parent
GO

DROP TABLE dbo.MainShiftActivityLayer
DROP TABLE dbo.OvertimeShiftActivityLayer
DROP TABLE dbo.PersonalShiftActivityLayer
GO

------
-- dummy change to force a new db schema when running test
------

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
--Date: 2013-07-18
--Desc: Remove unused column for soft deletes
---------------- 
ALTER TABLE dbo.MultisiteDay DROP COLUMN IsDeleted
GO

---------------- 
--Name: tamasb, kunning
--Date: 2013-06-24 
--Desc: add new appliation function MyTimeWeb/ShareCalendar
----------------  
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
SELECT @ParentForeignId = '0065'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0088' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ShareCalendar' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxShareCalendar' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Roger Kratz
--Date: 2013-08-16
--Desc: PBI #21978 - dropping min,max columns from personassignment(AUD).
----------------  
DROP INDEX [IX_PersonAssignment_AK1] ON [dbo].[PersonAssignment]
DROP INDEX [IX_Scenario_Minimum_Maximum] ON [dbo].[PersonAssignment]
GO

ALTER TABLE [dbo].[PersonAssignment]
DROP COLUMN Minimum, Maximum
GO

ALTER TABLE [Auditing].[PersonAssignment_AUD]
DROP COLUMN Minimum, Maximum
GO

--remove assignments without shifts
delete from PersonAssignment
where id in 
(
select pa.id from personassignment pa
left outer join ShiftLayer sl on pa.Id=sl.Parent
where not exists(select 1 from ShiftLayer where Parent=pa.Id)
)


----------------  
--Name: David Jonsson
--Date: 2013-08-22
--Desc: PBI #21978 - re-factor clustered index + add column: DayOffTemplate
----------------  
--rename current table + PK
EXEC dbo.sp_rename @objname = N'[dbo].[PersonAssignment]', @newname = N'PersonAssignment_old', @objtype = N'OBJECT'
EXEC sp_rename N'[dbo].[PersonAssignment_old].[PK_PersonAssignment]', N'PK_PersonAssignment_old', N'INDEX'

--drop existing FK + UQ
ALTER TABLE [dbo].[PersonAssignment_old] DROP CONSTRAINT [FK_PersonAssignment_BusinessUnit]
ALTER TABLE [dbo].[PersonAssignment_old] DROP CONSTRAINT [FK_PersonAssignment_Person_CreatedBy]
ALTER TABLE [dbo].[PersonAssignment_old] DROP CONSTRAINT [FK_PersonAssignment_Person_UpdatedBy]
ALTER TABLE [dbo].[PersonAssignment_old] DROP CONSTRAINT [FK_PersonAssignment_Person3]
ALTER TABLE [dbo].[PersonAssignment_old] DROP CONSTRAINT [FK_PersonAssignment_Scenario]
ALTER TABLE [dbo].[PersonAssignment_old] DROP CONSTRAINT [FK_PersonAssignment_ShiftCategory]
ALTER TABLE [dbo].[ShiftLayer] DROP CONSTRAINT [FK_ShiftLayer_PersonAssignment]

--create new table with correct clustered key
CREATE TABLE [dbo].[PersonAssignment](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] uniqueidentifier NOT NULL,
	[UpdatedBy] uniqueidentifier NOT NULL,
	[CreatedOn] datetime NOT NULL,
	[UpdatedOn] datetime NOT NULL,
	[Person] uniqueidentifier NOT NULL,
	[Scenario] uniqueidentifier NOT NULL,
	[BusinessUnit] uniqueidentifier NOT NULL,
	[Date] datetime NOT NULL,
	[ShiftCategory] uniqueidentifier NULL,
	[DayOffTemplate] uniqueidentifier NULL,
 CONSTRAINT [PK_PersonAssignment] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)
)

CREATE CLUSTERED INDEX [CIX_PersonAssignment_PersonDate_Scenario] ON [dbo].[PersonAssignment]
(
	[Person] ASC,
	[Date] ASC,
	[Scenario] ASC
)

--Transfer data into new clustered index
INSERT INTO [dbo].[PersonAssignment] (Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, Person, Scenario, BusinessUnit, Date, ShiftCategory,DayOffTemplate)
SELECT Id, Version, CreatedBy, UpdatedBy, CreatedOn, UpdatedOn, Person, Scenario, BusinessUnit, Date, ShiftCategory,NULL
FROM [dbo].[PersonAssignment_old]

--drop old table
DROP TABLE [dbo].[PersonAssignment_old]
GO

--re-add FK + UQ
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_BusinessUnit] FOREIGN KEY([BusinessUnit]) REFERENCES [dbo].[BusinessUnit] ([Id])
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Person_CreatedBy] FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Person_UpdatedBy] FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Person] FOREIGN KEY([Person]) REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Scenario] FOREIGN KEY([Scenario]) REFERENCES [dbo].[Scenario] ([Id])
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_ShiftCategory] FOREIGN KEY([ShiftCategory]) REFERENCES [dbo].[ShiftCategory] ([Id])
ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_DayOffTemplate] FOREIGN KEY([DayOffTemplate]) REFERENCES dbo.DayOffTemplate([Id])
ALTER TABLE [dbo].[ShiftLayer]  WITH CHECK ADD  CONSTRAINT [FK_ShiftLayer_PersonAssignment] FOREIGN KEY([Parent]) REFERENCES [dbo].[PersonAssignment] ([Id]) ON DELETE CASCADE
GO


ALTER TABLE auditing.PersonAssignment_AUD ADD
            DayOffTemplate uniqueidentifier NULL
GO

--Adding index to support next operation
--DROP INDEX IX_PersonAssignment_Date ON [dbo].[PersonAssignment]
CREATE NONCLUSTERED INDEX IX_PersonAssignment_Date
ON [dbo].[PersonAssignment]
	(
	[Date]
	)

--DROP INDEX [IX_PersonDayOff_Anchor_Person] ON [dbo].[PersonDayOff]
CREATE NONCLUSTERED INDEX [IX_PersonDayOff_Anchor_Person]
ON [dbo].[PersonDayOff]
(
	[Anchor] ASC,
	[Person] ASC,
	[Name] ASC
)
INCLUDE
(
	[Id]
)



----------------  
--Name: Robin Karlsson
--Date: 2013-08-09
--Desc: Adding read model for resources
---------------- 
CREATE TABLE [ReadModel].[ActivitySkillCombination](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Activity] [uniqueidentifier] NOT NULL,
	[Skills] [nvarchar](1500) NULL,
	[ActivityRequiresSeat] [bit] NOT NULL,
 CONSTRAINT [PK_ActivitySkillCombination] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))

GO

CREATE TABLE [ReadModel].[PeriodSkillEfficiencies](
	[ParentId] [int] NOT NULL,
	[SkillId] [uniqueidentifier] NOT NULL,
	[Amount] [float] NOT NULL
 CONSTRAINT [PK_PeriodSkillEfficiencies] PRIMARY KEY CLUSTERED 
(
	[ParentId] ASC,
	[SkillId] ASC
))

GO

CREATE TABLE [ReadModel].[ScheduledResources](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ActivitySkillCombinationId] [int] NOT NULL,
	[Resources] [float] NOT NULL,
	[Heads] [float] NOT NULL,
	[PeriodStart] [datetime] NOT NULL,
	[PeriodEnd] [datetime] NOT NULL,
 CONSTRAINT [PK_ScheduledResources] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))

GO

ALTER TABLE [ReadModel].[ActivitySkillCombination] ADD  CONSTRAINT [DF_ActivitySkillCombination_ActivityRequiresSeat]  DEFAULT ((0)) FOR [ActivityRequiresSeat]
GO

ALTER TABLE [ReadModel].[PeriodSkillEfficiencies] ADD  CONSTRAINT [DF_PeriodSkillEfficiencies_Amount]  DEFAULT ((0)) FOR [Amount]
GO

ALTER TABLE [ReadModel].[ScheduledResources] ADD  CONSTRAINT [DF_ScheduledResources_Resources]  DEFAULT ((0)) FOR [Resources]
GO

ALTER TABLE [ReadModel].[ScheduledResources] ADD  CONSTRAINT [DF_ScheduledResources_Heads]  DEFAULT ((0)) FOR [Heads]
GO


----------------  
--Name: Robin Karlsson
--Date: 2013-08-28
--Desc: Truncate read model for projected layers to force load of scheduled resources
--Edit: Erik Sundberg -> Moved this truncate to 389(default) due to bug: 25359
---------------- 
--TRUNCATE TABLE [ReadModel].[ScheduleProjectionReadOnly]
--GO

----------------  
--Name: tamasb
--Date: 2013-08-30  
--Desc: Delete the following application function: ModifyPersionDayOff
----------------  
	
DECLARE @FunctionCode as varchar(255)
DECLARE @ForeignId as varchar(255)

--modify the following application function
SELECT @ForeignId = '0013' -- Foreign id of the function
SELECT @FunctionCode = 'ModifyDayOff' -- Name of the function

UPDATE ApplicationFunction
SET IsDeleted=1
WHERE ForeignSource='Raptor' 
	AND ForeignId Like(@ForeignId + '%') 
	AND FunctionCode=@FunctionCode 

GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (386,'7.4.386') 
