IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadTeamSchedulesWithFilters]') AND type in (N'P', N'PC'))
DROP PROCEDURE  [ReadModel].[LoadTeamSchedulesWithFilters]
GO

-- =============================================
-- Author:		feng.gao
-- Create date: 2018-08-22
-- Description:	This sp is used for new team schedule search function
-- =============================================

CREATE PROCEDURE [ReadModel].[LoadTeamSchedulesWithFilters]
@scheduleDate smalldatetime,
@personList varchar(max),
@skip int = 0,
@take int = 20,
@isDayOff bit =0,
@filterStartTimes varchar(max) = NUll,
@filterEndTimes varchar(max) = NULL,
@hasFilter bit =0

AS
BEGIN

	SET NOCOUNT ON;
	DECLARE @maxIndex INT;
	DECLARE @scheduleDateStart smalldatetime;
	DECLARE @scheduleDateStartMax smalldatetime;
	DECLARE @scheduleDateEnd smalldatetime;

	SET @maxIndex = @skip + @take;
	SET @scheduleDateStart = @scheduleDate;		

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

	INSERT INTO @PersonIdList
	SELECT * FROM dbo.SplitStringString(@personList)

	INSERT INTO @filterStartTimeList(startTimeStart,startTimeEnd)
	SELECT startTime,endTime
	FROM [dbo].[SplitAndMergeTimeInterval](@filterStartTimes)

	INSERT INTO @filterEndTimeList(endTimeStart, endTimeEnd)
	SELECT startTime,endTime
	FROM [dbo].[SplitAndMergeTimeInterval](@filterEndTimes)

	if(@hasFilter = 0)
		begin
			WITH ScheduleResultSet AS
	(
	  SELECT DISTINCT 
	         *,ROW_NUMBER() OVER (ORDER BY IsEmptySchedule, IsFullDayAbsence, DayOffFlag, Start)
			
		    AS 'RowNumber'

         FROM (SELECT DISTINCT p.Person as PersonId,
			 team.Id as TeamID, 
			 sit.Id as SiteId, 
			 sit.BusinessUnit as BusinessUnitId, 
			 person.FirstName as FirstName,
			 person.LastName as LastName,
			 isnull(sd.BelongsToDate, @scheduleDate) as BelongsToDate, 
			 sd.Start, 
			 sd.[End], 
			 sd.Model,
			 sd.IsDayOff AS DayOffFlag,
			 Case When Start IS NOT NULL AND pa.Id IS NOT NULL THEN 1 ELSE 0 END AS IsFullDayAbsence,
			 CASE WHEN Start IS NULL THEN 1 ELSE 0 END As IsEmptySchedule
			 FROM @PersonIdList p 
			 JOIN dbo.Person person ON p.Person = person.Id 
			 LEFT JOIN ReadModel.PersonScheduleDay sd  ON sd.PersonId = p.Person and sd.BelongsToDate = @scheduleDate
			 LEFT JOIN dbo.PersonAbsence pa ON p.Person = pa.Person AND sd.Start >= pa.Minimum AND sd.[End] <= pa.Maximum 
			 LEFT JOIN PersonPeriod psp ON psp.Parent = person.Id AND psp.StartDate <= @scheduleDate AND @scheduleDate < psp.EndDate
			 LEFT JOIN Team team ON psp.Team = team.Id
			 LEFT JOIn [Site] sit ON team.[Site] = sit.Id
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
	WHERE RowNumber > @skip AND RowNumber <= @maxIndex AND TeamID IS NOT NULL
	UNION
	SELECT	PersonId, 
			t.Id, 
			s.Id, 
			s.BusinessUnit, 
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
	FROM ScheduleResultSet rs
	JOIN PersonPeriod pp on pp.Parent = rs.PersonId AND rs.TeamId IS NULL
	join Team t on t.Id = pp.Team
	join [Site] s on s.Id = t.Site
 	WHERE RowNumber > @skip AND RowNumber <= @maxIndex
		AND pp.StartDate = (select max(startdate) from PersonPeriod
							where startdate <= @scheduleDate and Parent = rs.PersonId) 
		end
	else if (@isDayOff = 1)
		begin
			WITH ScheduleResultSet AS
	(
	  SELECT DISTINCT 
	         *,ROW_NUMBER() OVER (ORDER BY IsEmptySchedule, IsFullDayAbsence, DayOffFlag, Start)
			
		    AS 'RowNumber'

         FROM (SELECT DISTINCT p.Person as PersonId,
			 team.Id as TeamID, 
			 sit.Id as SiteId, 
			 sit.BusinessUnit as BusinessUnitId, 
			 person.FirstName as FirstName,
			 person.LastName as LastName,
			 isnull(sd.BelongsToDate, @scheduleDate) as BelongsToDate, 
			 sd.Start, 
			 sd.[End], 
			 sd.Model,
			 sd.IsDayOff AS DayOffFlag,
			 Case When Start IS NOT NULL AND pa.Id IS NOT NULL THEN 1 ELSE 0 END AS IsFullDayAbsence,
			 CASE WHEN Start IS NULL THEN 1 ELSE 0 END As IsEmptySchedule
			 FROM @PersonIdList p 
			 JOIN dbo.Person person ON p.Person = person.Id 
			 LEFT JOIN ReadModel.PersonScheduleDay sd  ON sd.PersonId = p.Person and sd.BelongsToDate = @scheduleDate
			 LEFT JOIN dbo.PersonAbsence pa ON p.Person = pa.Person AND sd.Start >= pa.Minimum AND sd.[End] <= pa.Maximum 
			 LEFT JOIN PersonPeriod psp ON psp.Parent = person.Id AND psp.StartDate <= @scheduleDate AND @scheduleDate < psp.EndDate
			 LEFT JOIN Team team ON psp.Team = team.Id
			 LEFT JOIn [Site] sit ON team.[Site] = sit.Id
			 WHERE sd.IsDayOff = @isDayOff		    
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
	WHERE RowNumber > @skip AND RowNumber <= @maxIndex AND TeamID IS NOT NULL
	UNION
	SELECT	PersonId, 
			t.Id, 
			s.Id, 
			s.BusinessUnit, 
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
	FROM ScheduleResultSet rs
	JOIN PersonPeriod pp on pp.Parent = rs.PersonId AND rs.TeamId IS NULL
	join Team t on t.Id = pp.Team
	join [Site] s on s.Id = t.Site
 	WHERE RowNumber > @skip AND RowNumber <= @maxIndex
		AND pp.StartDate = (select max(startdate) from PersonPeriod
							where startdate <= @scheduleDate and Parent = rs.PersonId) 
        end  
	else
		begin
		 WITH ScheduleResultSet AS
	(
	  SELECT DISTINCT 
	         *,ROW_NUMBER() OVER (ORDER BY IsEmptySchedule, IsFullDayAbsence, DayOffFlag, Start)
			
		    AS 'RowNumber'

         FROM (SELECT DISTINCT p.Person as PersonId,
			 team.Id as TeamID, 
			 sit.Id as SiteId, 
			 sit.BusinessUnit as BusinessUnitId, 
			 person.FirstName as FirstName,
			 person.LastName as LastName,
			 isnull(sd.BelongsToDate, @scheduleDate) as BelongsToDate, 
			 sd.Start, 
			 sd.[End], 
			 sd.Model,
			 sd.IsDayOff AS DayOffFlag,
			 Case When Start IS NOT NULL AND pa.Id IS NOT NULL THEN 1 ELSE 0 END AS IsFullDayAbsence,
			 CASE WHEN Start IS NULL THEN 1 ELSE 0 END As IsEmptySchedule
			 FROM @PersonIdList p 
			 JOIN dbo.Person person ON p.Person = person.Id 
			 LEFT JOIN ReadModel.PersonScheduleDay sd  ON sd.PersonId = p.Person and sd.BelongsToDate = @scheduleDate
			 LEFT JOIN dbo.PersonAbsence pa ON p.Person = pa.Person AND sd.Start >= pa.Minimum AND sd.[End] <= pa.Maximum 
			 LEFT JOIN PersonPeriod psp ON psp.Parent = person.Id AND psp.StartDate <= @scheduleDate AND @scheduleDate < psp.EndDate
			 LEFT JOIN Team team ON psp.Team = team.Id
			 LEFT JOIn [Site] sit ON team.[Site] = sit.Id	
			  WHERE 			
			 (
			      sd.IsDayOff = @isDayOff
			     and 
				 (  					
					(isnull(@filterStartTimes, '') = '' OR EXISTS ( select * from @filterStartTimeList fstl where sd.Start between fstl.startTimeStart and fstl.startTimeEnd))
				    AND 
					(isnull(@filterEndTimes, '') = '' OR EXISTS ( select * from @filterEndTimeList fetl where sd.[End] between fetl.endTimeStart and fetl.endTimeEnd))				 
				  ) 									
			)	    
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
	WHERE RowNumber > @skip AND RowNumber <= @maxIndex AND TeamID IS NOT NULL
	UNION
	SELECT	PersonId, 
			t.Id, 
			s.Id, 
			s.BusinessUnit, 
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
	FROM ScheduleResultSet rs
	JOIN PersonPeriod pp on pp.Parent = rs.PersonId AND rs.TeamId IS NULL
	join Team t on t.Id = pp.Team
	join [Site] s on s.Id = t.Site
 	WHERE RowNumber > @skip AND RowNumber <= @maxIndex
		AND pp.StartDate = (select max(startdate) from PersonPeriod
							where startdate <= @scheduleDate and Parent = rs.PersonId) 
		end
	
	DECLARE @lastPage bit 
	SET @lastPage = 0
	IF ((SELECT COUNT(*) FROM #tmpOut) < @take)
		SET @lastPage = 1

	DECLARE @thisLastRow int
	SET @thisLastRow = (SELECT MAX(Rownumber) FROM #tmpOut)
	
	IF (SELECT MAX(Total) FROM #tmpOut) = @thisLastRow
		SET @lastPage = 1

	SELECT *, @lastPage AS IsLastPage FROM #tmpOut ORDER BY RowNumber

    DROP TABLE #tmpOut
END