
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
--Date: 2013-04-15
--Desc: Adding date for person assignment audit table. Hard coded to 1800-1-1. Will be replaced by .net code
ALTER TABLE Auditing.PersonAssignment_AUD
ADD [Date] datetime
GO

declare @Date datetime
set @Date = '1800-01-01T00:00:00'

update Auditing.PersonAssignment_AUD
set [Date] = @Date
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

------------------ AUDIT TABLES ----------------------------
--Add shiftcategory to personassignment
ALTER TABLE auditing.PersonAssignment_AUD
add ShiftCategory uniqueidentifier
GO

--move data for ShiftCategory
UPDATE pa
SET pa.ShiftCategory = ms.ShiftCategory
FROM auditing.PersonAssignment_AUD pa
INNER JOIN auditing.MainShift_AUD ms
	ON pa.Id = ms.Id

--drop mainshift table
drop table auditing.mainshift_aud
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
---------------- 
--first remove duplicates but keep the Absence with the biggest time span
create table #PersonAbsenceRemove (Id uniqueidentifier)

insert into #PersonAbsenceRemove (Id)
select Id from PersonAbsence
where id not in
(
	--keep longest Abscence with lowest Guid from the duplicates, count(*) > 1
	select t1.Id
	from PersonAbsence t1
	inner join (
		select cast(min(cast(id as varchar(36))) as uniqueidentifier) Id, person, Scenario, Minimum, PayLoad, max(Maximum) Maximum
		from PersonAbsence
		group by person, Scenario, Minimum,PayLoad
		having count(*) > 1
	) t2
	on t1.Person = t2.Person
		and t1.Id = t2.Id --t2.Id is lowest one -> manipulated string/Id cast above
		and t1.Minimum = t2.Minimum
		and t1.PayLoad = t2.PayLoad
		and t1.Maximum = t2.Maximum --t2.Maximum is the longest one
		and t1.Scenario= t2.Scenario

	union all
	
	--get all the correct ones, count(*) = 1
	select t1.Id
	from PersonAbsence t1
	inner join (
		select person, Scenario, Minimum, PayLoad, max(Maximum) Maximum
		from PersonAbsence
		group by person, Scenario, Minimum,PayLoad
		having count(*) = 1
	) t2
	on t1.Person = t2.Person
		and t1.Minimum = t2.Minimum
		and t1.PayLoad = t2.PayLoad
		and t1.Maximum = t2.Maximum
		and t1.Scenario= t2.Scenario
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