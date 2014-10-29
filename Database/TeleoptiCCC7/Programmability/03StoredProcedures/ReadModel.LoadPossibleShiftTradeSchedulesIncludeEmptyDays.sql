
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
-- Author:		Ola
-- Create date: 2013-11-14
-- Description:	Load Schedule for possible shift trades
-- =============================================
-- ReadModel.LoadPossibleShiftTradeSchedulesIncludeEmptyDays '2008-07-11', '47A3D4AA-3CD8-4235-A7EB-9B5E015B2560,B0E35119-4661-4A1B-8772-9B5E015B2564,9D42C9BF-F766-473F-970C-9B5E015B2564,27F2C260-C262-40B3-BE5D-ABAA48EB8286,27F2C260-C262-40B3-BE5D-ABAA48EB8286', 00,50

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
	--WITH Ass AS
	--(
	--  SELECT TOP (1000000)
	--         *,
	--	     ROW_NUMBER() OVER (ORDER BY DayOffFlag, Start) AS 'RowNumber'
 --       FROM (SELECT *,
	--	             CASE DATEDIFF(MINUTE, sd.Start, sd.[End])
	--				   WHEN 1440 THEN 1
	--				   ELSE 0
	--				 END AS DayOffFlag
	--	        FROM ReadModel.PersonScheduleDay sd
	--	       INNER JOIN @TempList t ON t.Person = sd.PersonId
			   
	--	       WHERE [BelongsToDate] = @shiftTradeDate
	--	         --AND DATEDIFF(MINUTE,Start, [End] ) <= 1440
	--			 ) as s
	--	ORDER BY DayOffFlag, Start
	--) 


	WITH Ass AS
	(
	  SELECT TOP (1000000)
	         *,
		     ROW_NUMBER() OVER (ORDER BY DayOffFlag, Start) AS 'RowNumber'
        FROM (select p.Person as PersonId,
			 isnull(sd.TeamId, t.Id) as TeamID, 
			 isnull(sd.SiteId, s.Id) as SiteId, 
			 isnull(sd.BusinessUnitId, s.BusinessUnit) as BusinessUnitId, 
			 sd.BelongsToDate, sd.Start, sd.[End], sd.Model,
			 CASE DATEDIFF(MINUTE, sd.Start, sd.[End])
								   WHEN 1440 THEN 1
								   ELSE 0
								 END AS DayOffFlag
			--t.Id, s.Id, s.BusinessUnit
			FROM @TempList p left join (select * from ReadModel.PersonScheduleDay where [BelongsToDate] = @shiftTradeDate) sd on sd.PersonId=p.Person
			join PersonPeriod pp on pp.Parent=p.Person 
			join Team t on t.Id =pp.Team
			join [Site] s on s.Id = t.Site
			where  pp.StartDate=(select max(startdate) from PersonPeriod
									where startdate <= @shiftTradeDate 
									and Parent=p.Person
									group by parent)
		) as s
		ORDER BY DayOffFlag, Start
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


