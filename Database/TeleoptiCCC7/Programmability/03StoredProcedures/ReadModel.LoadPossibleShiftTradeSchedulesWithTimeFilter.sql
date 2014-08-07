IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter]
GO

-- =============================================
-- Author:		Mingdi
-- Create date: 2014-08-05
-- Description:	Load Schedule for possible shift trades with filtered times
-- =============================================
-- ReadModel. LoadPossibleShiftTradeSchedulesWithTimeFilter '2014-08-05', '88be31b0-9c70-4076-9743-9b5e015b2577,b46a2588-8861-42e3-ab03-9b5e015b257c,fdb75a4e-5765-4857-b213-9b5e015b2564,9d42c9bf-f766-473f-970c-9b5e015b2564,94329a0e-b3c5-4b1f-beb9-9b5e015b2564','6:00-8:00',00,20

CREATE PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter]
@shiftTradeDate smalldatetime,
@personList varchar(max),
@filteredStartTimeList varchar(max),
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
	startHour int,
	endHour int,
	timestring varchar(24),
	Processed bit
	)

	--Init
	INSERT INTO @TempList
	SELECT * FROM dbo.SplitStringString(@personList)

	--Init
	INSERT INTO @filterStartTimeList(timestring, Processed)
	SELECT *,0 FROM dbo.SplitStringString(@filteredStartTimeList)

	While (Select Count(*) From @filterStartTimeList Where Processed = 0) > 0
	Begin
		DECLARE @startHour int, @endHour int, @timestring varchar(24), @tempstring varchar(8), @substring varchar(8)
		Select Top 1 @timestring = timestring From @filterStartTimeList Where Processed = 0

		--Do some processing here
		SET @tempstring = SUBSTRING(@timestring, 0, CHARINDEX('-', @timestring))
		SET @substring = SUBSTRING(@tempstring, 0, CHARINDEX(':', @tempstring))
		SET @startHour =  CONVERT(INT, @substring) 
		SET @tempstring = SUBSTRING(@timestring, CHARINDEX('-', @timestring)+1, LEN(@tempstring))
		SET @substring = SUBSTRING(@tempstring, 0, CHARINDEX(':', @tempstring))
		SET @endHour =  CONVERT(INT, @substring)

		INSERT INTO @filterStartTimeList(startHour, endHour)
		VALUES(@startHour, @endHour)

		Update @filterStartTimeList Set Processed = 1 Where timestring = @timestring 
	End

	SET ROWCOUNT @take;
	WITH Ass AS
	(
		SELECT TOP (1000000) *, 
		ROW_NUMBER() OVER (ORDER BY sd.Start) AS 'RowNumber'
		FROM ReadModel.PersonScheduleDay sd
		INNER JOIN @TempList t ON t.Person = sd.PersonId
		INNER JOIN @filterStartTimeList fs ON fs.startHour <= DATEPART(hh, sd.Start) AND fs.endHour*100 > DATEPART(hh, sd.Start)*100 +DATEPART(mi, sd.Start)
		WHERE [BelongsToDate] = @shiftTradeDate
		AND sd.Start IS NOT NULL
		AND DATEDIFF(MINUTE,Start, [End] ) <= 1440
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
