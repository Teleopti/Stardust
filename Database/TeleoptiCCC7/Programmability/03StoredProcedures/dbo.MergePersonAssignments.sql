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
CREATE PROCEDURE [dbo].[MergePersonAssignments]
AS

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'UQ_PersonAssignment_Date_Scenario_Person')
BEGIN

	--temp tables
	CREATE TABLE #mergeUs (KeepMe uniqueidentifier, MergeUs uniqueidentifier)
	CREATE TABLE #PersonAssignment (Id uniqueidentifier, Person uniqueidentifier,[Date] datetime,Scenario uniqueidentifier, ShiftCategory uniqueidentifier)

	--find duplicates, Id = NULL for now
	INSERT INTO #PersonAssignment
	SELECT
		NULL,[Person],[Date],[Scenario],cast(max(cast(ShiftCategory AS binary(16))) as uniqueidentifier)
	FROM [dbo].[PersonAssignment]
	GROUP BY [Person],[Date],[Scenario]
	HAVING COUNT(*) > 1
	ORDER BY cast(max(cast(ShiftCategory AS binary(16))) as uniqueidentifier) DESC --bug #27661 - favor the ShiftCategory Guid before NULL when a mix of PersonalShifts and Activies

	--Set any Id = "the one to keep"
	UPDATE tmp
	SET tmp.Id = pa.Id -- will give a random Id
	FROM #PersonAssignment tmp
	INNER JOIN [dbo].[PersonAssignment] pa
		ON  pa.Person	= tmp.Person
		AND pa.[Date]	= tmp.[Date]
		AND pa.Scenario	= tmp.Scenario
		AND pa.ShiftCategory = tmp.ShiftCategory

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

	--And finally add Unique constraint
	ALTER TABLE [dbo].[PersonAssignment] ADD  CONSTRAINT [UQ_PersonAssignment_Date_Scenario_Person] UNIQUE NONCLUSTERED 
	(
		[Date] ASC,
		[Scenario] ASC,
		[Person] ASC
	)

END

GO


