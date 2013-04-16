IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[ChangedDataOnStep]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[ChangedDataOnStep]
GO

CREATE PROCEDURE [mart].[ChangedDataOnStep]
@stepName nvarchar(500),
@startDate datetime,
@endDate datetime,
@buId uniqueidentifier 
-- =============================================
-- Author:		Ola
-- Create date: 2013-04-10
-- Description:	Detects if a ETL step needs to run
-- =============================================
-- Date			Who	Description
-- =============================================
AS
CREATE TABLE #persons (PersonId uniqueidentifier NOT NULL)
CREATE TABLE #PersonsUpdated
	(PersonId uniqueidentifier NOT NULL,
	BelongsToDate smalldatetime NOT NULL,
	InsertedOn datetime NOT NULL
	)

DECLARE @thisTime datetime
DECLARE @lastTime datetime

--Get ETL last execution time
SELECT @lastTime= [Date]
FROM LastUpdatedPerStep a
WHERE BusinessUnit = @buId
AND StepName = @StepName

INSERT INTO #persons (PersonId)
SELECT DISTINCT Parent
FROM dbo.PersonPeriod pp
INNER JOIN dbo.team t
	ON t.Id = pp.team
INNER JOIN dbo.site s
	ON t.site = s.Id
INNER JOIN dbo.BusinessUnit bu
	ON bu.id = s.BusinessUnit
WHERE bu.id=@buId

--Handle case "ETL never executed".
IF @lastTime IS NULL
BEGIN
	SET @lastTime=dateadd(hour,-1,getdate())
	INSERT mart.LastUpdatedPerStep VALUES(@stepName, @buId, @lastTime)
END

--Get all Schedule days that are updated after last ETL run
INSERT INTO #PersonsUpdated (PersonId, BelongsToDate, InsertedOn)
SELECT s.PersonId, s.BelongsToDate, s.InsertedOn
FROM [ReadModel].[ScheduleDay] s
INNER JOIN #persons p ON p.PersonId = s.PersonId
WHERE InsertedOn >= @lastTime -- >= (not > ) to be sure we do not miss any
AND BelongsToDate BETWEEN @startDate AND @endDate

--get max for this BU
SET @thisTime = (SELECT max(InsertedOn) from #PersonsUpdated)

--Handle case, "now rows detected"
IF @thisTime IS NULL
SET @thisTime = @lastTime

--return 'thisTime' to ETL. To be used later if everything is successfull
SELECT @thisTime as 'thisTime'

--return Persons
SELECT PersonId AS Person, BelongsToDate AS [Date]
FROM #PersonsUpdated
ORDER BY BelongsToDate

GO