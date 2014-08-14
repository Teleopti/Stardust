IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter]
GO

-- =============================================
-- Author:		Mingdi
-- Create date: 2014-08-05
-- Description:	Load Schedule for possible shift trades with filtered times
-- =============================================
-- ReadModel. LoadPossibleShiftTradeSchedulesWithTimeFilter '2014-08-08', 'b46a2588-8861-42e3-ab03-9b5e015b257c,47a3d4aa-3cd8-4235-a7eb-9b5e015b2560,88be31b0-9c70-4076-9743-9b5e015b2577,9d42c9bf-f766-473f-970c-9b5e015b2564,94329a0e-b3c5-4b1f-beb9-9b5e015b2564',
--'2014-08-08 10:00','2014-08-08 12:00', '2014-08-08 20:00','2014-08-08 22:00',
-- false,00,20

CREATE PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter]
@shiftTradeDate smalldatetime,
@personList varchar(max),
@filterStartTimeStarts varchar(max),
@filterStartTimeEnds varchar(max),
@filterEndTimeStarts varchar(max),
@filterEndTimeEnds varchar(max),
@isDayOff bit,
@skip int,
@take int

AS

	SET NOCOUNT ON;
	--Declares
	DECLARE @TempList table
	(
	Person uniqueidentifier
	)

	DECLARE @filterStartTimeList table
	(
	startTimeStart varchar(24),
	startTimeEnd varchar(24)
	)

	DECLARE @filterEndTimeList table
	(
	endTimeStart varchar(24),
	endTimeEnd varchar(24)
	)

	--Init
	INSERT INTO @TempList
	SELECT * FROM dbo.SplitStringString(@personList)

	INSERT INTO @filterStartTimeList(startTimeStart, startTimeEnd)
	SELECT startTime.string, endTime.string FROM 
		(SELECT *, ROW_NUMBER()  over(order by string) as id FROM dbo.SplitStringString(@filterStartTimeStarts)) startTime 
	JOIN
		(SELECT *, ROW_NUMBER() over(order by string) as id FROM dbo.SplitStringString(@filterStartTimeEnds)) endTime
	ON startTime.id = endTime.id

	INSERT INTO @filterEndTimeList(endTimeStart, endTimeEnd)
	SELECT startTime.string, endTime.string FROM 
		(SELECT *, ROW_NUMBER()  over(order by string) as id FROM dbo.SplitStringString(@filterEndTimeStarts)) startTime 
	JOIN
		(SELECT *, ROW_NUMBER() over(order by string) as id FROM dbo.SplitStringString(@filterEndTimeEnds)) endTime
	ON startTime.id = endTime.id

	SET ROWCOUNT @take;
	WITH Ass AS
	(
		SELECT TOP (1000000) *, 
		ROW_NUMBER() OVER (ORDER BY sd.Start) AS 'RowNumber'
		FROM ReadModel.PersonScheduleDay sd
		INNER JOIN @TempList t ON t.Person = sd.PersonId
		INNER JOIN @filterStartTimeList fs ON fs.startTimeStart <= sd.Start
		AND fs.startTimeEnd > sd.Start
		INNER JOIN @filterEndTimeList fe ON fe.endTimeStart <= sd.[End] 
		AND fe.endTimeEnd >sd.[End]
		WHERE [BelongsToDate] = @shiftTradeDate
		AND sd.Start IS NOT NULL
		AND DATEDIFF(MINUTE,Start, [End] ) < 1440
		OR sd.IsDayOff = @isDayOff
		ORDER BY sd.Start
	) 
	
	SELECT PersonId, TeamId, SiteId, BusinessUnitId, BelongsToDate AS [Date], Start, [End], Model, (SELECT MIN(Start) FROM Ass)  As 'MinStart',(SELECT COUNT(*) FROM Ass)  As 'Total', RowNumber
	INTO #tmpOut
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
