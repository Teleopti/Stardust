----------------  
--Name: tamasb, kunning
--Date: 2013-03-25 
--Desc: add new appliation function MyTimeWeb/TeamSchedule/ViewAllGroupPages
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
SELECT @ParentForeignId = '0072'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0084' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ViewAllGroupPages' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxViewAllGroupPages' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [CreatedBy], [UpdatedBy], [CreatedOn], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, @SuperUserId, getdate(), getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO
----------------  
--Name: Robin Karlsson
--Date: 2013-04-24
--Desc: Bug #23047. Empty read models to consider deleted schedule when done.
----------------  
--TRUNCATE TABLE [ReadModel].[ScheduleDay] nope! dont => #23288 //DavidJ
TRUNCATE TABLE [ReadModel].[ScheduleProjectionReadOnly]
GO
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

	SET NOCOUNT OFF
END
GO