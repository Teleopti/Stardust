
/****** Object:  StoredProcedure [ReadModel].[LoadShiftTradeBulletinSchedules]    Script Date: 2013-11-14 13:35:09 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadShiftTradeBulletinSchedules]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadShiftTradeBulletinSchedules]
GO

-- =============================================
-- Author:		Mingdi
-- Create date: 2014-11-20
-- Update date: 2015-01-27 
-- Description:	Load Bulletin Schedules for shift trades
-- =============================================
/*
exec [ReadModel].[LoadShiftTradeBulletinSchedules]
@shiftExchangeOfferIdList = '65CABE14-E655-4E7C-96F8-A42900AE83FE,681A6923-6B09-44C2-A4AC-A42900CBC3E7,5156A5A4-3CD9-4D5B-B591-A42900EC1004,EF138B59-3CC1-468B-99AF-A42C00A24BA5,71B9DD9A-2118-4804-956E-A42C00A909D2',
@skip = 2,
@take = 2
*/
CREATE PROCEDURE [ReadModel].[LoadShiftTradeBulletinSchedules]
@shiftExchangeOfferIdList varchar(max),
@skip int,
@take int

AS

	SET NOCOUNT ON;
	--Declares
	DECLARE @TempList table
	(
	Request uniqueidentifier
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
	SELECT * FROM dbo.SplitStringString(@shiftExchangeOfferIdList)

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
				seo.Request AS ShiftExchangeOffer
			FROM ReadModel.PersonScheduleDay sd 
			INNER JOIN dbo.ShiftExchangeOffer seo ON sd.PersonId = seo.Person AND sd.BelongsToDate = seo.Date
			INNER JOIN @TempList t ON seo.Request = t.Request
			INNER JOIN dbo.Request req ON seo.Request = req.Id
			INNER JOIN dbo.PersonRequest pre ON pre.Id = req.Parent AND pre.IsDeleted = 0			
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

