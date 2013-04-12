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
-- DELETE FROM mart.LastUpdatedPerStep
-- SELECT * FROM mart.LastUpdatedPerStep
-- exec mart.[ChangedDataOnStep] 'stg_schedule_day_off_count, stg_day_off, dim_day_off', '2013-03-03', '2013-03-09', 'fc2f309c-3e3c-4cfb-9c11-a1570077b92b'
DECLARE @lastTime datetime
SELECT @lastTime = '1900-01-01'

-- do the same check on fact step as on stage step
IF @stepName = 'fact_schedule' SELECT @stepName = 'stg_schedule, stg_schedule_day_absence_count'
IF @stepName = 'fact_schedule_day_count' SELECT @stepName = 'stg_schedule_day_off_count, stg_day_off, dim_day_off'
IF @stepName = 'fact_schedule_preference' SELECT @stepName = 'stg_schedule_preference, stg_day_off, dim_day_off'

IF EXISTS (SELECT * FROM mart.LastUpdatedPerStep WHERE StepName = @stepName AND BusinessUnit = @buId)
	SELECT @lastTime = Date FROM LastUpdatedPerStep WHERE StepName = @stepName AND BusinessUnit = @buId

DECLARE @thisTime datetime
SET @thisTime = @lastTime

select Parent INTO #persons
FROM PersonPeriodWithEndDate
INNER JOIN Contract ON Contract.Id = Contract
WHERE EndDate = '2059-12-31'
AND Contract.BusinessUnit = @buId

SELECT PersonId, BelongsToDate, InsertedOn
INTO #temp
FROM [ReadModel].[ScheduleDay] s
INNER JOIN #persons p ON p.Parent = s.PersonId
WHERE InsertedOn >= @lastTime -- >= (not > ) to be sure we do not miss any
AND BelongsToDate BETWEEN @startDate AND @endDate


IF EXISTS (SELECT * FROM #temp) 
	set @thisTime = (SELECT max(InsertedOn) from #temp)


IF @thisTime <> @lastTime
BEGIN
	IF EXISTS (SELECT * FROM mart.LastUpdatedPerStep WHERE StepName = @stepName AND BusinessUnit = @buId)
		UPDATE mart.LastUpdatedPerStep SET Date = @thisTime WHERE StepName = @stepName AND BusinessUnit = @buId
	ELSE
		INSERT mart.LastUpdatedPerStep VALUES(@stepName, @buId, @thisTime) 
END


	SELECT DISTINCT PersonId AS Person, BelongsToDate AS [Date] FROM #temp
	ORDER BY BelongsToDate

GO