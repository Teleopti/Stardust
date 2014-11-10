
/****** Object:  StoredProcedure [ReadModel].[LoadPossibleShiftTradeSchedulesIncludeEmptyDays]    Script Date: 2013-11-14 13:35:09 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadPossibleShiftTradeSchedulesIncludeEmptyDays]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedulesIncludeEmptyDays]
GO

/****** Object:  StoredProcedure [ReadModel].[LoadPossibleShiftTradeSchedulesIncludeEmptyDays]    Script Date: 2013-11-14 13:35:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Chundan Xu, Jianguang Fang
-- Create date: 2014-10-25
-- Description:	Load Schedule for possible shift trades including empty days
-- =============================================

CREATE PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedulesIncludeEmptyDays]
@shiftTradeDate smalldatetime,
@personList varchar(max),
@skip int,
@take int

AS

	SET NOCOUNT ON;
	--Declares
	DECLARE @TempList table
	(
	Person uniqueidentifier
	)

	CREATE TABLE #tmpOut(PersonId uniqueidentifier, TeamId uniqueidentifier, SiteId uniqueidentifier, BusinessUnitId uniqueidentifier,[Date] datetime, Start datetime, [End] datetime, 
	Model nvarchar(max), MinStart datetime, Total int, RowNumber int)

	--Init
	INSERT INTO @TempList
	SELECT * FROM dbo.SplitStringString(@personList)
	--loop tempt list
	SET ROWCOUNT @take;

	WITH Ass AS
	(
	  SELECT DISTINCT TOP (1000000)
	         *,
		     ROW_NUMBER() OVER (ORDER BY IsEmptySchedule, DayOffFlag, Start) AS 'RowNumber'
        FROM (select p.Person as PersonId,
			 isnull(sd.TeamId, t.Id) as TeamID, 
			 isnull(sd.SiteId, s.Id) as SiteId, 
			 isnull(sd.BusinessUnitId, s.BusinessUnit) as BusinessUnitId, 
			 isnull(sd.BelongsToDate, @shiftTradeDate) as BelongsToDate, 
			 sd.Start, sd.[End], sd.Model,
			 CASE DATEDIFF(MINUTE, sd.Start, sd.[End])
								   WHEN 1440 THEN 1
								   ELSE 0
								 END AS DayOffFlag,
			 CASE WHEN Start IS NULL THEN 1 ELSE 0 END As IsEmptySchedule
			FROM @TempList p left join (select * from ReadModel.PersonScheduleDay where [BelongsToDate] = @shiftTradeDate) sd on sd.PersonId=p.Person
			join PersonPeriod pp on pp.Parent = p.Person 
			join Team t on t.Id = pp.Team
			join [Site] s on s.Id = t.Site
			where  pp.StartDate = (select max(startdate) from PersonPeriod
									where startdate <= @shiftTradeDate 
									and Parent = p.Person
									group by parent)
		) as s
		ORDER BY IsEmptySchedule, DayOffFlag, Start
	) 

	
	INSERT INTO #tmpOut
	SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS [Date], Start, [End], Model, (SELECT MIN(Start) FROM Ass)  As 'MinStart',(SELECT COUNT(*) FROM Ass)  As 'Total', RowNumber
	--INTO #tmpOut
	FROM Ass 
	WHERE RowNumber > @skip
	
	DECLARE @lastPage bit 
	SET @lastPage = 0
	IF ((SELECT COUNT(*) FROM #tmpOut) < @take)
		SET @lastPage = 1

	DECLARE @thisLastRow int
	SET @thisLastRow = (SELECT MAX(Rownumber) FROM #tmpOut)
	
	IF (SELECT MAX(Total) FROM #tmpOut) = @thisLastRow
		SET @lastPage = 1

	SELECT *, @lastPage AS IsLastPage FROM #tmpOut
GO


