
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadPersonSchedule]') AND type in (N'P', N'PC'))
DROP PROCEDURE  [ReadModel].[LoadPersonSchedule] 
GO


/****** Object:  StoredProcedure [ReadModel].[LoadPersonSchedule]    Script Date: 2/25/2015 2:28:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Fan Zhang, Zhiping Lan, Yanyi Wan
-- Create date: 2015-02-10
-- Description:	This sp merges "LoadPossibleShiftTradeSchedulesWithTimeFilter" 
--              and "LoadPossibleShiftTradeSchedulesWithEmptyDay" because the user may request both at the same time.
-- =============================================

CREATE PROCEDURE [ReadModel].[LoadPersonSchedule]
@scheduleDate smalldatetime,
@personList varchar(max),
@timeSortOrder varchar(max) = NULL,
@skip int = 0,
@take int = 20,

-- These are complementary filtering criteria, i.e. the result should be the union of the results for each individual criteria
@isEmptyDay bit = 1,
@isDayOff bit = 1,
@isWorkingDay bit = 1,

-- The following filters are effective only when isWorkingDay = 1
@filterStartTimes varchar(max) = NUll,
@filterEndTimes varchar(max) = NULL


AS
BEGIN

	SET NOCOUNT ON;
	--Declares
	DECLARE @maxIndex INT;
	SET @maxIndex = @skip + @take;

	DECLARE @PersonIdList table
	(
		Person uniqueidentifier
	)
	DECLARE @filterStartTimeList table
	(
		startTimeStart smalldatetime,
		startTimeEnd smalldatetime
	)

	DECLARE @filterEndTimeList table
	(
		endTimeStart smalldatetime,
		endTimeEnd smalldatetime
	)

	CREATE TABLE #tmpOut(
		PersonId uniqueidentifier,
		TeamId uniqueidentifier, 
		SiteId uniqueidentifier, 
		BusinessUnitId uniqueidentifier,
		FirstName nvarchar(max),
		LastName nvarchar(max),
		[Date] datetime, 
		Start datetime, 
		[End] datetime,		
		Model nvarchar(max), 
		IsDayOff bit,
		IsEmptyDay bit,		 
		MinStart datetime, 
		Total int, 
		RowNumber int)

	--Init:  Digest Input Parameters
	INSERT INTO @PersonIdList
	SELECT * FROM dbo.SplitStringString(@personList)

	INSERT INTO @filterStartTimeList(startTimeStart,startTimeEnd)
	SELECT startTime,endTime
	FROM [dbo].[SplitAndMergeTimeInterval](@filterStartTimes)

	INSERT INTO @filterEndTimeList(endTimeStart, endTimeEnd)
	SELECT startTime,endTime
	FROM [dbo].[SplitAndMergeTimeInterval](@filterEndTimes);

	WITH ScheduleResultSet AS
	(
	  SELECT DISTINCT 
	         *,
			CASE @timeSortOrder
			WHEN 'start asc'  THEN ROW_NUMBER() OVER (ORDER BY IsEmptySchedule, DayOffFlag, Start)
			WHEN 'start desc' THEN ROW_NUMBER() OVER (ORDER BY IsEmptySchedule, DayOffFlag, Start DESC) 		
			WHEN 'end asc' THEN ROW_NUMBER() OVER (ORDER BY IsEmptySchedule, DayOffFlag, [End]) 		
			WHEN 'end desc' THEN ROW_NUMBER() OVER (ORDER BY IsEmptySchedule, DayOffFlag, [End] DESC)
			ELSE  ROW_NUMBER() OVER (ORDER BY IsEmptySchedule, DayOffFlag, Start)
			End
		    AS 'RowNumber'

        FROM (select p.Person as PersonId,
			 isnull(sd.TeamId, t.Id) as TeamID, 
			 isnull(sd.SiteId, s.Id) as SiteId, 
			 isnull(sd.BusinessUnitId, s.BusinessUnit) as BusinessUnitId, 
			 person.FirstName as FirstName,
			 person.LastName as LastName,
			 isnull(sd.BelongsToDate, @scheduleDate) as BelongsToDate, 
			 sd.Start, 
			 sd.[End], 
			 sd.Model,
			 sd.IsDayOff AS DayOffFlag,
			 CASE WHEN Start IS NULL THEN 1 ELSE 0 END As IsEmptySchedule
			 FROM @PersonIdList p 
			 JOIN dbo.Person person ON p.Person = person.Id 
			 LEFT JOIN ReadModel.PersonScheduleDay sd  ON sd.PersonId = p.Person and sd.BelongsToDate = @scheduleDate		 
			 
			 -- These joins are used to support backward compatibility with incomplete ReadModel.PersonScheduleDay
			 JOIN PersonPeriod pp on pp.Parent = p.Person
			 join Team t on t.Id = pp.Team
			 join [Site] s on s.Id = t.Site 
			 ------

			 WHERE 			
			 -- The following enclosed conditions should be "Or"-ed such that the result is a union of each individual condition
			 (
				-- Condition when day-off filter is set
			     (  @isDayOff = 1 and sd.IsDayOff = @isDayOff )					
				-- Condition when empty-day filter is set
			     OR ( @isEmptyDay =1 and sd.Start is null)
				-- Condition if time filters are set
			     OR ( 
				    @isWorkingDay = 1 
				    AND
					(isnull(@filterStartTimes, '') = '' OR EXISTS ( select * from @filterStartTimeList fstl where sd.Start between fstl.startTimeStart and fstl.startTimeEnd))
				    AND 
					(isnull(@filterEndTimes, '') = '' OR EXISTS ( select * from @filterEndTimeList fetl where sd.[End] between fetl.endTimeStart and fetl.endTimeEnd))				 
				 ) 									
			)
			-- Condition to support backward compatibility with incomplete ReadModel.PersonScheduleDay
			 AND pp.StartDate = (select max(startdate) from PersonPeriod
								where startdate <= @scheduleDate 
									and Parent = p.Person)
				
							    
		) as resultSet		
	) 


	INSERT INTO #tmpOut
	SELECT	PersonId, 
			TeamId, 
			SiteId, 
			BusinessUnitId, 
			FirstName,
			LastName,
			BelongsToDate AS [Date], 
			Start, 
			[End],
			Model,
			DayOffFlag,
			IsEmptySchedule, 			 
			(SELECT MIN(Start) FROM ScheduleResultSet)  As 'MinStart',
			(SELECT COUNT(*) FROM ScheduleResultSet)  As 'Total', 
			RowNumber
	FROM ScheduleResultSet 
	WHERE RowNumber > @skip AND RowNumber <= @maxIndex
	
	DECLARE @lastPage bit 
	SET @lastPage = 0
	IF ((SELECT COUNT(*) FROM #tmpOut) < @take)
		SET @lastPage = 1

	DECLARE @thisLastRow int
	SET @thisLastRow = (SELECT MAX(Rownumber) FROM #tmpOut)
	
	IF (SELECT MAX(Total) FROM #tmpOut) = @thisLastRow
		SET @lastPage = 1

	SELECT *, @lastPage AS IsLastPage FROM #tmpOut ORDER BY RowNumber


	

END
