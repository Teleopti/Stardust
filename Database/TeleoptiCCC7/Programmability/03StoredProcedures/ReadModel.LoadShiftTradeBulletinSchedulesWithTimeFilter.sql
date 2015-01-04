IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadShiftTradeBulletinSchedulesWithTimeFilter]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadShiftTradeBulletinSchedulesWithTimeFilter]
GO

-- =============================================
-- Author:		Mingdi
-- Create date: 2014-11-26
-- Description:	Load Bulletin Schedules for shift trades
-- =============================================
/*
ReadModel.LoadShiftTradeBulletinSchedulesWithTimeFilter
'2014-11-28',
'11610FE4-0130-4568-97DE-9B5E015B2564,b46a2588-8861-42e3-ab03-9b5e015b257c,47a3d4aa-3cd8-4235-a7eb-9b5e015b2560,88be31b0-9c70-4076-9743-9b5e015b2577,9d42c9bf-f766-473f-970c-9b5e015b2564,94329a0e-b3c5-4b1f-beb9-9b5e015b2564',
'2014-11-28 10:00',
'2014-11-28 17:00',
'2014-08-08 10:00;2014-08-08 12:00,2014-08-08 12:00;2014-08-08 14:00',
'2014-08-08 20:00;2014-08-08 22:00,2014-08-09 00:00;2014-08-09 01:00',
true,0,20
*/
CREATE PROCEDURE ReadModel.LoadShiftTradeBulletinSchedulesWithTimeFilter
@shiftTradeDate smalldatetime,
@personList varchar(max),
@currentScheduleStart smalldatetime,
@currentScheduleEnd smalldatetime,
@filterStartTimes varchar(max),
@filterEndTimes varchar(max),
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
		[ShiftExchangeOffer] [uniqueidentifier] NUll,
		[MinStart] [datetime] NULL,
		[Total] [int] NULL,
		[RowNumber] [bigint] NULL
	)

	--Init
	INSERT INTO @TempList
	SELECT * FROM dbo.SplitStringString(@personList)
	
	INSERT INTO @filterStartTimeList(startTimeStart,startTimeEnd)
	SELECT startTime,endTime
	FROM [dbo].[SplitAndMergeTimeInterval](@filterStartTimes)

	INSERT INTO @filterEndTimeList(endTimeStart, endTimeEnd)
	SELECT startTime,endTime
	FROM [dbo].[SplitAndMergeTimeInterval](@filterEndTimes)

	DECLARE @BulletinResult table
	(
		Person uniqueidentifier,
		ShiftExchangeOffer uniqueidentifier
	)

	INSERT INTO @BulletinResult
	SELECT
		Person,
		Request
	FROM (
			--Shifts
  			SELECT DISTINCT
				Request,
				seo.Person,
				[Date],
				MyShiftStartDateTime,
				MyShiftEndDateTime,
				[Status]
			FROM dbo.ShiftExchangeOffer seo
			INNER JOIN @TempList t
				ON t.Person = seo.Person
			INNER JOIN dbo.Request req
				ON seo.Request = req.Id
			INNER JOIN dbo.PersonRequest preq
				ON req.Parent = preq.Id
				AND preq.IsDeleted = 0
			WHERE [Date] = @shiftTradeDate
			AND @currentScheduleStart between seo.ShiftWithinStartDateTime and seo.ShiftWithinEndDateTime
			AND @currentScheduleEnd between seo.ShiftWithinStartDateTime and seo.ShiftWithinEndDateTime
			AND ValidTo >= CONVERT(date, GETUTCDATE())
			AND [Status] = 0
	)bulletin

	SET ROWCOUNT @take;
	WITH Ass AS
	(
		SELECT *,
		ROW_NUMBER() OVER (ORDER BY a.Start) AS 'RowNumber'
		FROM (
			--Shifts
  			SELECT
				sd.PersonId,
				sd.TeamId,
				sd.SiteId,
				sd.BusinessUnitId,
				sd.BelongsToDate,
				sd.Start,
				sd.[End],
				sd.Model,
				br.ShiftExchangeOffer
			FROM ReadModel.PersonScheduleDay sd, @BulletinResult br, @filterStartTimeList fs, @filterEndTimeList fe
			WHERE  br.Person = sd.PersonId
			AND [BelongsToDate] = @shiftTradeDate
			AND sd.Start between fs.startTimeStart and fs.startTimeEnd
			AND sd.[End] between fe.endTimeStart and fe.endTimeEnd
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
				Model,
				br.ShiftExchangeOffer
			FROM ReadModel.PersonScheduleDay sd, @BulletinResult br
			WHERE br.Person = sd.PersonId
			AND [BelongsToDate] = @shiftTradeDate
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
		ShiftExchangeOffer,
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

