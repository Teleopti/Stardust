IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MergePersonAssignments]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MergePersonAssignments]
GO

-- =============================================
-- Author:		David Jonsson
-- Create date: 2013-07-02
-- Description:	Find duplicates and merge into one and only only ScheduleDay
-- =============================================
-- Changed		Who			Why
-- =============================================
-- yyyy-mm-dd	Mrx			Because
-- =============================================
--EXEC [dbo].[MergePersonAssignments]
CREATE PROCEDURE [dbo].[MergePersonAssignments]
AS

--our marker
DECLARE @bugNumber int
SET @bugNumber = -35191

--If "Done" then return
IF EXISTS (SELECT 1 FROM dbo.databaseVersion WHERE BuildNumber=@bugNumber)
RETURN 0

--Check if Realease 625 has been applied. If so we know the PA merge is OK => The unique key from early versions of Release 000000625.sql did work.
--but we never marked it as "Done!" at the time
DECLARE @ClusteredKeyIsUnique bit
SELECT @ClusteredKeyIsUnique=is_unique
FROM sys.indexes
WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]')
AND type_desc = 'CLUSTERED'

--if the Clustered key is not yet unique, apply merge and re-design the clustered key
IF @ClusteredKeyIsUnique=0
BEGIN
	--temp tables
	CREATE TABLE #mergeUs (KeepMe uniqueidentifier, MergeUs uniqueidentifier)
	CREATE TABLE #PersonAssignment (Id uniqueidentifier not null, Person uniqueidentifier not null,[Date] datetime not null, Scenario uniqueidentifier not null, ShiftCategory uniqueidentifier,DayOffTemplate uniqueidentifier)

	--find duplicates, Id = NULL for now
	INSERT INTO #PersonAssignment
	SELECT
		Id				= '00000000-0000-0000-0000-000000000000',
		Person			= [Person],
		[Date]			= [Date],
		Scenario		= [Scenario],
		ShiftCategory	= cast(max(cast(ShiftCategory AS binary(16))) as uniqueidentifier),
		DayOffTemplate	= cast(max(cast(DayOffTemplate AS binary(16))) as uniqueidentifier)
	FROM [dbo].[PersonAssignment]
	GROUP BY [Person],[Date],[Scenario]
	HAVING COUNT(*) > 1
	ORDER BY cast(max(cast(ShiftCategory AS binary(16))) as uniqueidentifier) DESC,	 --bug #27661 - favor the ShiftCategory Guid before NULL when a mix of PersonalShifts and Activies
	cast(max(cast(DayOffTemplate AS binary(16))) as uniqueidentifier) DESC

	--When a mix of PersonAssignments: Set any Id for the one "the one to keep"
	--1) shifts only
	UPDATE tmp
	SET tmp.Id = pa.Id -- will give a random Id
	FROM #PersonAssignment tmp
	INNER JOIN [dbo].[PersonAssignment] pa
		ON  pa.Person	= tmp.Person
		AND pa.[Date]	= tmp.[Date]
		AND pa.Scenario	= tmp.Scenario
		AND pa.ShiftCategory = tmp.ShiftCategory
		AND pa.DayOffTemplate IS NULL

	--2) DayOff only
	UPDATE tmp
	SET tmp.Id = pa.Id -- will give a random Id
	FROM #PersonAssignment tmp
	INNER JOIN [dbo].[PersonAssignment] pa
		ON  pa.Person	= tmp.Person
		AND pa.[Date]	= tmp.[Date]
		AND pa.Scenario	= tmp.Scenario
		AND pa.ShiftCategory IS NULL
		AND pa.DayOffTemplate = tmp.DayOffTemplate
	
	--3) handle Assignments with only Personal or Overtime activies. (neither Shift nor DayOff)
	UPDATE tmp
	SET tmp.Id = pa.Id -- will give a random Id
	FROM #PersonAssignment tmp
	INNER JOIN [dbo].[PersonAssignment] pa
		ON  pa.Person	= tmp.Person
		AND pa.[Date]	= tmp.[Date]
		AND pa.Scenario	= tmp.Scenario
		AND tmp.ShiftCategory IS NULL
		AND tmp.DayOffTemplate IS NULL

	--Get other Ids, the to merge
	INSERT INTO #mergeUs
	SELECT
		tmp.Id, --the one to keep
		pa.Id	--the ones to dump + the one to keep
	FROM [dbo].[PersonAssignment] pa
	INNER JOIN #PersonAssignment tmp
		ON  pa.Person	= tmp.Person
		AND pa.[Date]	= tmp.[Date]
		AND pa.Scenario	= tmp.Scenario

	--update layers to point to new parent
	UPDATE sl
	SET parent = tmp.KeepMe
	FROM ShiftLayer sl
	INNER JOIN #mergeUs tmp
		ON tmp.MergeUs = sl.Parent
		
	--remove obsolete PA Ids
	DELETE FROM dbo.PersonAssignment
	WHERE Id IN (SELECT mergeUs FROM #mergeUs WHERE KeepMe<>MergeUs);

	--fix messed up OrderIndex for the duplicates
	WITH Dubplicates AS
	(
		SELECT sl.Id, ROW_NUMBER() OVER(PARTITION BY sl.Parent ORDER BY LayerType,sl.Minimum) -1 as newOrderIndex
		FROM #PersonAssignment tmp
		INNER JOIN dbo.PersonAssignment pa
			ON pa.Id = tmp.Id
		INNER JOIN dbo.ShiftLayer sl
			ON sl.Parent = pa.Id
	)
	UPDATE sl
	SET OrderIndex = d.newOrderIndex
	FROM dbo.ShiftLayer sl
	INNER JOIN Dubplicates d
	ON sl.Id = d.Id;

	--re-design the table
	ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [FK_PersonAssignment_DayOffTemplate]
	ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [FK_PersonAssignment_Person]
	ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [FK_PersonAssignment_Person_UpdatedBy]
	ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [FK_PersonAssignment_Scenario]
	ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [FK_PersonAssignment_ShiftCategory]
	ALTER TABLE [dbo].[ShiftLayer] DROP CONSTRAINT [FK_ShiftLayer_PersonAssignment]
	--GO

	--create new table and clustered key
	CREATE TABLE [dbo].[PersonAssignment_new](
		[Id] [uniqueidentifier] NOT NULL,
		[Version] [int] NOT NULL,
		[UpdatedBy] [uniqueidentifier] NOT NULL,
		[UpdatedOn] [datetime] NOT NULL,
		[Person] [uniqueidentifier] NOT NULL,
		[Scenario] [uniqueidentifier] NOT NULL,
		[Date] [datetime] NOT NULL,
		[ShiftCategory] [uniqueidentifier] NULL,
		[DayOffTemplate] [uniqueidentifier] NULL,
		[Source] nvarchar(50) NULL
	 CONSTRAINT [PK_PersonAssignment_new] PRIMARY KEY NONCLUSTERED 
	(
		[Id] ASC
	)
	)

	CREATE UNIQUE CLUSTERED INDEX [CIX_PersonAssignment_PersonDate_Scenario] ON [dbo].[PersonAssignment_new]
	(
					[Person] ASC,
					[Date] ASC,
					[Scenario] ASC
	)

	--get data into new table
	INSERT INTO [dbo].[PersonAssignment_new]
	SELECT  Id, Version, UpdatedBy, UpdatedOn, Person, Scenario, Date, ShiftCategory, DayOffTemplate, null
	FROM [dbo].[PersonAssignment]

	--rename tables
	EXEC dbo.sp_rename @objname = N'[dbo].[PersonAssignment]', @newname = N'PersonAssignment_old', @objtype = N'OBJECT'
	EXEC sp_rename N'[dbo].[PersonAssignment_old].[PK_PersonAssignment]', N'PK_PersonAssignment_old', N'INDEX'
	EXEC dbo.sp_rename @objname = N'[dbo].[PersonAssignment_new]', @newname = N'PersonAssignment', @objtype = N'OBJECT'
	EXEC sp_rename N'[dbo].[PersonAssignment].[PK_PersonAssignment_new]', N'PK_PersonAssignment', N'INDEX'

	--re-create FKs
	ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_DayOffTemplate] FOREIGN KEY([DayOffTemplate])
	REFERENCES [dbo].[DayOffTemplate] ([Id])
	ALTER TABLE [dbo].[PersonAssignment]  CHECK CONSTRAINT [FK_PersonAssignment_DayOffTemplate]

	ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Person] FOREIGN KEY([Person])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[PersonAssignment]  CHECK CONSTRAINT [FK_PersonAssignment_Person]

	ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
	REFERENCES [dbo].[Person] ([Id])
	ALTER TABLE [dbo].[PersonAssignment]  CHECK CONSTRAINT [FK_PersonAssignment_Person_UpdatedBy]

	ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_Scenario] FOREIGN KEY([Scenario])
	REFERENCES [dbo].[Scenario] ([Id])
	ALTER TABLE [dbo].[PersonAssignment]  CHECK CONSTRAINT [FK_PersonAssignment_Scenario]

	ALTER TABLE [dbo].[PersonAssignment]  WITH CHECK ADD  CONSTRAINT [FK_PersonAssignment_ShiftCategory] FOREIGN KEY([ShiftCategory])
	REFERENCES [dbo].[ShiftCategory] ([Id])
	ALTER TABLE [dbo].[PersonAssignment]  CHECK CONSTRAINT [FK_PersonAssignment_ShiftCategory]

	ALTER TABLE [dbo].[ShiftLayer]  WITH CHECK ADD  CONSTRAINT [FK_ShiftLayer_PersonAssignment] FOREIGN KEY([Parent])
	REFERENCES [dbo].[PersonAssignment] ([Id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[ShiftLayer] CHECK CONSTRAINT [FK_ShiftLayer_PersonAssignment]

	DECLARE @SQLString nvarchar(500)
	SET @SQLString=N'DROP TABLE [dbo].[PersonAssignment_old]'
	EXECUTE sp_executesql @SQLString

END

--Mark as "done!"
INSERT INTO dbo.databaseVersion (BuildNumber,SystemVersion)
SELECT @bugNumber,'Merge of PA'


GO


