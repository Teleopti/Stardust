IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter_new]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter_new]
GO

-- =============================================
-- Author:		Mingdi
-- Create date: 2014-08-05
-- Description:	Load Schedule for possible shift trades with filtered times
-- =============================================
/*
ReadModel.LoadPossibleShiftTradeSchedulesWithTimeFilter_new
'2014-08-08',
'b46a2588-8861-42e3-ab03-9b5e015b257c,47a3d4aa-3cd8-4235-a7eb-9b5e015b2560,88be31b0-9c70-4076-9743-9b5e015b2577,9d42c9bf-f766-473f-970c-9b5e015b2564,94329a0e-b3c5-4b1f-beb9-9b5e015b2564',
'2014-08-08 10:00;2014-08-08 12:00,2014-08-08 10:00;2014-08-08 16:00',
'2014-08-08 20:00;2014-08-08 22:00,2014-08-09 00:00;2014-08-09 01:00',
true,00,20

ReadModel.LoadPossibleShiftTradeSchedulesWithTimeFilter_new
'2014-08-08',
'b46a2588-8861-42e3-ab03-9b5e015b257c,47a3d4aa-3cd8-4235-a7eb-9b5e015b2560,88be31b0-9c70-4076-9743-9b5e015b2577,9d42c9bf-f766-473f-970c-9b5e015b2564,94329a0e-b3c5-4b1f-beb9-9b5e015b2564',
'2014-08-08 10:00;2014-08-08 12:00',
'2014-08-08 20:00;2014-08-08 22:00',
true,00,20

 */
CREATE PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedulesWithTimeFilter_new]
@shiftTradeDate smalldatetime,
@personList varchar(max),
@filterStartTime varchar(max),
@filterEndTime varchar(max),
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
	startTimeStart smalldatetime,
	startTimeEnd smalldatetime
	)

	DECLARE @filterEndTimeList table
	(
	endTimeStart smalldatetime,
	endTimeEnd smalldatetime
	)

	DECLARE @output table
	(
		[PersonId] [uniqueidentifier] NOT NULL,
		[TeamId] [uniqueidentifier] NOT NULL,
		[SiteId] [uniqueidentifier] NOT NULL,
		[BusinessUnitId] [uniqueidentifier] NOT NULL,
		[Date] [smalldatetime] NOT NULL,
		[Start] [datetime] NULL,
		[End] [datetime] NULL,
		[Model] [nvarchar](max) NOT NULL,
		[MinStart] [datetime] NULL,
		[Total] [int] NULL,
		[RowNumber] [bigint] NULL
	)

	--Init
	INSERT INTO @TempList
	SELECT * FROM dbo.SplitStringString(@personList)

	INSERT INTO @filterStartTimeList(startTimeStart,startTimeEnd)
	SELECT startTime,endTime
	FROM [dbo].[SplitAndMergeTimeInterval](@filterStartTime)

	INSERT INTO @filterEndTimeList(endTimeStart, endTimeEnd)
	SELECT startTime,endTime
	FROM [dbo].[SplitAndMergeTimeInterval](@filterEndTime)

	SET ROWCOUNT @take;		
	WITH Ass AS
	(
		SELECT *,
		ROW_NUMBER() OVER (ORDER BY a.Start) AS 'RowNumber'
		FROM (
			--Shifts
  			SELECT
				PersonId,
				TeamId,
				SiteId,
				BusinessUnitId,
				BelongsToDate,
				Start,
				[End],
				Model
			FROM ReadModel.PersonScheduleDay sd
			INNER JOIN @TempList t
				ON t.Person = sd.PersonId
			INNER JOIN @filterStartTimeList fs
				ON sd.Start between fs.startTimeStart and fs.startTimeEnd
			INNER JOIN @filterEndTimeList fe
				ON sd.[End] between fe.endTimeStart and fe.endTimeEnd
			WHERE [BelongsToDate] = @shiftTradeDate
			AND IsDayOff = 0

			UNION ALL

			--DayOff
			SELECT
				PersonId,
				TeamId,
				SiteId,
				BusinessUnitId,
				BelongsToDate,
				Start,
				[End],
				Model
			FROM ReadModel.PersonScheduleDay sd
			INNER JOIN @TempList t
				ON t.Person = sd.PersonId
			WHERE [BelongsToDate] = @shiftTradeDate
			AND IsDayOff = 1
			AND IsDayOff = @isDayOff
			) a
	)
	INSERT INTO @output
	SELECT
		PersonId,
		TeamId,
		SiteId,
		BusinessUnitId,
		BelongsToDate AS [Date],
		Start,
		[End],
		Model,
		(SELECT MIN(Start) FROM Ass)  As 'MinStart',
		(SELECT COUNT(*) FROM Ass)  As 'Total',
		RowNumber
	FROM Ass
	WHERE Ass.RowNumber > @skip
	
	DECLARE @lastPage bit 
	SET @lastPage = 0
	IF ((SELECT COUNT(*) FROM @output) < @take)
		SET @lastPage = 1

	DECLARE @thisLastRow int
	SET @thisLastRow = (SELECT MAX(Rownumber) FROM @output)
	
	IF (SELECT MAX(Total) FROM @output) = @thisLastRow
		SET @lastPage = 1

	SELECT *, @lastPage AS IsLastPage FROM @output
