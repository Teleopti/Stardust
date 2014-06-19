
/****** Object:  StoredProcedure [ReadModel].[LoadPossibleShiftTradeSchedules]    Script Date: 2013-11-14 13:35:09 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadPossibleShiftTradeSchedules]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedules]
GO

/****** Object:  StoredProcedure [ReadModel].[LoadPossibleShiftTradeSchedules]    Script Date: 2013-11-14 13:35:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Ola
-- Create date: 2013-11-14
-- Description:	Load Schedule for possible shift trades
-- =============================================
-- ReadModel.LoadPossibleShiftTradeSchedules '2008-07-11', '47A3D4AA-3CD8-4235-A7EB-9B5E015B2560,B0E35119-4661-4A1B-8772-9B5E015B2564,9D42C9BF-F766-473F-970C-9B5E015B2564,27F2C260-C262-40B3-BE5D-ABAA48EB8286,27F2C260-C262-40B3-BE5D-ABAA48EB8286', 00,50

CREATE PROCEDURE [ReadModel].[LoadPossibleShiftTradeSchedules]
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

	--Init
	INSERT INTO @TempList
	SELECT * FROM dbo.SplitStringString(@personList)

	SET ROWCOUNT @take;
	WITH Ass AS
	(
		SELECT TOP (1000000) *, 
		ROW_NUMBER() OVER (ORDER BY sd.Start) AS 'RowNumber'
		FROM ReadModel.PersonScheduleDay sd
		INNER JOIN @TempList t ON t.Person = sd.PersonId
		WHERE [BelongsToDate] = @shiftTradeDate
		AND sd.Start IS NOT NULL
		AND DATEDIFF(MINUTE,Start, [End] ) < 1440
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
GO


