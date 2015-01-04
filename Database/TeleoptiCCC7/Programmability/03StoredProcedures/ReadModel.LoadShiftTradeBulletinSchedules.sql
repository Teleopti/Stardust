
/****** Object:  StoredProcedure [ReadModel].[LoadShiftTradeBulletinSchedules]    Script Date: 2013-11-14 13:35:09 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadShiftTradeBulletinSchedules]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadShiftTradeBulletinSchedules]
GO

/****** Object:  StoredProcedure [ReadModel].[LoadShiftTradeBulletinSchedules]    Script Date: 2013-11-14 13:35:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Mingdi
-- Create date: 2014-11-20
-- Description:	Load Bulletin Schedules for shift trades
-- =============================================
/*
ReadModel.LoadShiftTradeBulletinSchedules
'2014-12-12',
'11610FE4-0130-4568-97DE-9B5E015B2564,b46a2588-8861-42e3-ab03-9b5e015b257c,47a3d4aa-3cd8-4235-a7eb-9b5e015b2560,88be31b0-9c70-4076-9743-9b5e015b2577,9d42c9bf-f766-473f-970c-9b5e015b2564,94329a0e-b3c5-4b1f-beb9-9b5e015b2564',
'2014-12-12 10:00',
'2014-12-12 17:00',
0,20
*/
CREATE PROCEDURE [ReadModel].[LoadShiftTradeBulletinSchedules]
@shiftTradeDate smalldatetime,
@personList varchar(max),
@currentScheduleStart smalldatetime,
@currentScheduleEnd smalldatetime,
@skip int,
@take int

AS

	SET NOCOUNT ON;
	--Declares
	DECLARE @TempList table
	(
	Person uniqueidentifier
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
			AND [Status] = 0
			AND @currentScheduleStart between seo.ShiftWithinStartDateTime and seo.ShiftWithinEndDateTime
			AND @currentScheduleEnd between seo.ShiftWithinStartDateTime and seo.ShiftWithinEndDateTime
			AND ValidTo >= CONVERT(date, GETUTCDATE())
	)bulletin

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
				Model,
				br.ShiftExchangeOffer
			FROM ReadModel.PersonScheduleDay sd, @BulletinResult br
			WHERE br.Person = sd.PersonId
			AND [BelongsToDate] = @shiftTradeDate
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

