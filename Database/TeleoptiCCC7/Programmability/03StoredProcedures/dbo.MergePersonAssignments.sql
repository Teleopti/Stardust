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
	CREATE TABLE #PersonAssignment (Id uniqueidentifier, Person uniqueidentifier,[Date] datetime,Scenario uniqueidentifier)

	--find duplicates, Id = NULL for now
	INSERT INTO #PersonAssignment
	SELECT
		NULL,[Person],[Date],[Scenario]
	FROM [dbo].[PersonAssignment]
	GROUP BY [Person],[Date],[Scenario]
	HAVING COUNT(*) > 1

	--Set any Id = "the one to keep"
	UPDATE tmp
	SET tmp.Id = pa.Id -- will give a random Id
	FROM #PersonAssignment tmp
	INNER JOIN [dbo].[PersonAssignment] pa
		ON  pa.Person	= tmp.Person
		AND pa.[Date]	= tmp.[Date]
		AND pa.Scenario	= tmp.Scenario

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

	--bug #27661 - missing ShiftCategory values
	create table #tmp_missing_shift_category(paID uniqueidentifier, buID uniqueidentifier)
	create table #tmp_bu(buID uniqueidentifier)

	declare @SuperUserId uniqueidentifier
	declare @ShortName nvarchar(25)
	declare @Name nvarchar(50)
	set @ShortName='?!'
	set @Name='missing category'
	set @SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

	insert #tmp_missing_shift_category
	select distinct
		pa.Id,
		sc.BusinessUnit 
	from ShiftLayer sl
	inner join PersonAssignment pa
		on pa.Id = sl.Parent
	inner join Scenario sc
		on pa.Scenario = sc.Id
	where LayerType = 1
	and pa.ShiftCategory is null

	insert #tmp_bu
	select cast(max(cast(buId AS binary(16))) as uniqueidentifier)
	from #tmp_missing_shift_category
	group by buID

	if exists (select * from #tmp_missing_shift_category)
	begin
		--insert a new ShiftCategory for each BU where missing
		insert into ShiftCategory(id, Version, UpdatedBy, UpdatedOn, Name, ShortName, DisplayColor, BusinessUnit, IsDeleted)
		select 
		newid(), 
		1, 
		@SuperUserId, 
		getutcdate(),
		@Name,
		@ShortName,
		0,
		buID, 
		1
		from #tmp_bu
 
		--update all PersonAssignment with missing ShiftCategory
		update pa
		set ShiftCategory = sc.Id
		from shiftCategory sc
		inner join #tmp_missing_shift_category tmp 
			on sc.BusinessUnit = tmp.buID
		inner join PersonAssignment pa
			on pa.Id = tmp.paID
		where sc.Name = @Name
		and sc.ShortName = @ShortName
	end

	--And finally add Unique constraint
	ALTER TABLE [dbo].[PersonAssignment] ADD  CONSTRAINT [UQ_PersonAssignment_Date_Scenario_Person] UNIQUE NONCLUSTERED 
	(
		[Date] ASC,
		[Scenario] ASC,
		[Person] ASC
	)

END

GO


