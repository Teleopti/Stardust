--#21539
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriod]') AND name = N'IX_PersonPeriod_Team_Id_Parent')
CREATE NONCLUSTERED INDEX IX_PersonPeriod_Team_Id_Parent
ON [dbo].[PersonPeriod] ([Team])
INCLUDE ([Id],[Parent])
GO

-----------------  
--Name: TamasB
--Date: 2012-12-17
--Desc: #bugfix 21764 - Fix invalid text
----------------  
UPDATE [dbo].PersonRequest
SET DenyReason = 'RequestDenyReasonSupervisor'
WHERE DenyReason = 'xxRequestDenyReasonSupervisor'
GO

----------------  
--Name: David Jonsson
--Date: 2013-01-08
--Desc: Bug #21786. Remove dubplicates in PersonSkill
----------------  
--remove duplicates, but keep one.
--note: We don't consider "Active" it will be random
WITH Dubplicates AS
(
	select Id, Parent,Skill,ROW_NUMBER() OVER(PARTITION BY Parent,Skill ORDER BY Id) as rownumber
	from PersonSkill
) 
DELETE ps
FROM PersonSkill ps
INNER JOIN Dubplicates d
ON ps.Id = d.Id
WHERE d.rownumber >1;
GO

--add constraint to block new duplicates
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonSkill]') AND name = N'UC_Parent_Skill')
ALTER TABLE PersonSkill ADD CONSTRAINT UC_Parent_Skill UNIQUE (Parent,Skill)
GO

----------------  
--Name: David Jonsson
--Date: 2013-05-04
--Desc: Bug #23349 Speed up rotations
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonRotation]') AND name = N'IX_PersonRotation_Person_IsDeleted_BusinessUnit_StartDate')
CREATE NONCLUSTERED INDEX [IX_PersonRotation_Person_IsDeleted_BusinessUnit_StartDate]
ON [dbo].[PersonRotation] ([Person],[IsDeleted],[BusinessUnit],[StartDate])
GO

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
delete from PersonAbsence
where id not in 

(
	--get the single one that we want to keep from the duplicates, count(*) > 1
	select id
	from PersonAbsence t1
	inner join (
		select person, Minimum, PayLoad, max(Maximum) Maximum
		from PersonAbsence
		group by person, Minimum,PayLoad
		having count(*) > 1
	) t2
	on t1.Person = t2.Person
		and t1.Minimum = t2.Minimum
		and t1.PayLoad = t2.PayLoad
		and t1.Maximum = t2.Maximum
	union all
	
	--get all the correct ones, count(*) = 1
	select Id
	from PersonAbsence t1
	inner join (
		select person, Minimum, PayLoad, max(Maximum) Maximum
		from PersonAbsence
		group by person, Minimum,PayLoad
		having count(*) = 1
	) t2
	on t1.Person = t2.Person
		and t1.Minimum = t2.Minimum
		and t1.PayLoad = t2.PayLoad
		and t1.Maximum = t2.Maximum
)

--finally delete any duplicates left
--cast GUID to string and back to handle SQL 2005
delete from PersonAbsence
where id not in 
(select cast(min(cast(id as varchar(36))) as uniqueidentifier) from PersonAbsence group by person, PayLoad, Minimum, Maximum)
GO
