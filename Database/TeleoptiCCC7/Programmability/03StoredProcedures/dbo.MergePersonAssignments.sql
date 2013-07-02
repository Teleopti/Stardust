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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'UQ_PersonAssignment_Person_Date_Scenario')
BEGIN

	--merge any duplicates
	CREATE TABLE #mergeUs (KeepMe uniqueidentifier, MergeUs uniqueidentifier)
	CREATE TABLE #PersonAssignment (Id uniqueidentifier, Person uniqueidentifier,[Date] datetime,Scenario uniqueidentifier)
	INSERT INTO #PersonAssignment
	SELECT
		NULL,[Person],[Date],[Scenario]
	FROM [dbo].[PersonAssignment]
	GROUP BY [Person],[Date],[Scenario]
	HAVING COUNT(*) > 1

	--Pick Id to keep	
	UPDATE tmp
	SET tmp.Id = pa.Id -- will give a random Id
	FROM #PersonAssignment tmp
	INNER JOIN [dbo].[PersonAssignment] pa
		ON  pa.Person	= tmp.Person
		AND pa.[Date]	= tmp.[Date]
		AND pa.Scenario	= tmp.Scenario

	--Get Ids to merge
	INSERT INTO #mergeUs
	SELECT tmp.Id,pa.Id
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
	WHERE Id IN (SELECT mergeUs FROM #mergeUs WHERE KeepMe<>MergeUs)

	--fix messed up OrderIndex
	-- .... ToDo

	--And finally add Unique constraint
	ALTER TABLE [dbo].[PersonAssignment] ADD  CONSTRAINT [UQ_PersonAssignment_Person_Date_Scenario] UNIQUE NONCLUSTERED 
	(
		[Person] ASC,
		[Date] ASC,
		[Scenario] ASC
	)

END

GO


