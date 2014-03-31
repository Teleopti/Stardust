IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[LastChangedDateOnStep]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[LastChangedDateOnStep]
GO

-- =============================================
-- Author:		Ola
-- Create date: 2013-04-10
-- Description:	Returns the latest time that ETL has updated the schedules for 
-- =============================================
-- Date			Who	Description
-- =============================================
-- exec [mart].[LastChangedDateOnStep] 'Forecast','928DD0BC-BF40-412E-B970-9B5E015AADEA', '2000-01-01', '2013-06-30'
CREATE PROCEDURE [mart].[LastChangedDateOnStep]
@stepName nvarchar(500),
@buId uniqueidentifier,
@start datetime = '2000-01-01',
@end datetime  = '2100-01-01'
AS

DECLARE @thisTime datetime
DECLARE @lastTime datetime
DECLARE @maxDayToGoBackForScheduleUpdates int
SET @maxDayToGoBackForScheduleUpdates=14 --Number of days to go back for updated Schedule data

--insert missing stepName for each BU
INSERT INTO mart.LastUpdatedPerStep(StepName,BusinessUnit,[Date])
SELECT @StepName,bu.Id,GETUTCDATE()
FROM dbo.BusinessUnit bu
WHERE NOT EXISTS (
	SELECT * FROM mart.LastUpdatedPerStep etl
	WHERE etl.stepName = @StepName
	AND etl.BusinessUnit = bu.id
	)
AND bu.IsDeleted = 0
AND bu.Id = @buId

--Get ETL last execution time
SELECT @lastTime= [Date]
FROM mart.LastUpdatedPerStep a
WHERE BusinessUnit = @buId
AND StepName = @StepName

CREATE TABLE #persons (PersonId uniqueidentifier NOT NULL)
CREATE TABLE #PersonsUpdated
	(PersonId uniqueidentifier NOT NULL,
	BelongsToDate smalldatetime NOT NULL,
	InsertedOn datetime NOT NULL
	)

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

--Get the time for last update on this particular object; updated after last ETL run
IF @stepName = 'Forecast'
BEGIN
	DECLARE @scenarioId uniqueidentifier
	SELECT @scenarioId = ID FROM Scenario WHERE BusinessUnit =  @buId AND DefaultScenario = 1
	SELECT @thisTime = MAX(UpdatedOn) FROM SkillDay WHERE UpdatedOn >= @lastTime
	AND UpdatedOn BETWEEN @start AND @end
	AND Scenario = @scenarioId
END
IF @stepName = 'Preferences'
BEGIN
	SELECT @thisTime = MAX(UpdatedOn) FROM PreferenceDay WHERE UpdatedOn >= @lastTime
END
IF @stepName = 'Availability'
BEGIN
	SELECT @thisTime = MAX(UpdatedOn) FROM StudentAvailabilityDay WHERE UpdatedOn >= @lastTime
END
IF @stepName = 'Schedules'
BEGIN
	INSERT INTO #PersonsUpdated (PersonId, BelongsToDate, InsertedOn)
	SELECT s.PersonId, s.BelongsToDate, s.InsertedOn
	FROM [ReadModel].[ScheduleDay] s
	INNER JOIN #persons p ON p.PersonId = s.PersonId
	WHERE InsertedOn > @lastTime --InsertedOn includes milliseconds, @lastTime does not => we will always catch the last changed schedule over and over again
	AND InsertedOn > DATEADD(DAY,-@maxDayToGoBackForScheduleUpdates,GETUTCDATE())  --Limit the number of days we go back. #27450

	--get max for this BU
	SET @thisTime = (SELECT max(InsertedOn) from #PersonsUpdated)

END
IF @stepName = 'Permissions'
BEGIN
	SELECT @thisTime = MAX(LastUpdate)
	FROM (
		--Persons getting changed Role
		SELECT MAX(pr.InsertedOn) as LastUpdate
		FROM dbo.AvailablePersonsInApplicationRole pr
		INNER JOIN #persons p
			ON p.PersonId = pr.AvailablePerson
		UNION ALL

		--Role getting changed ApplicationFunction
		SELECT MAX(afir.InsertedOn) as LastUpdate
		FROM dbo.ApplicationFunctionInRole afir
		INNER JOIN dbo.ApplicationFunction af
			ON afir.ApplicationFunction = af.Id
		INNER JOIN dbo.ApplicationRole ar
			ON afir.ApplicationRole=ar.Id
			AND ar.BusinessUnit = @buId
		UNION ALL

		--Role getting changed data
		SELECT MAX(ad.UpdatedOn) as LastUpdate
		FROM dbo.ApplicationRole ar
		INNER JOIN dbo.AvailableData ad
			ON ar.Id = ad.ApplicationRole
			AND ar.BusinessUnit = @buId
		) a
END

IF @stepName = 'Requests'
BEGIN
	SELECT @thisTime = MAX(LastUpdate)
	FROM (
		--Persons getting changed Role
		SELECT MAX(pr.UpdatedOn) as LastUpdate
		FROM dbo.PersonRequest pr
		INNER JOIN #persons p
			ON p.PersonId = pr.person
		) a
END

--Handle case, "now rows detected"
IF @thisTime IS NULL
SET @thisTime = @lastTime

--Handle case, "ETL is running and detect older changes"
IF @thisTime < @lastTime
SET @thisTime = @lastTime

--return 'thisTime' to ETL. To be used later if everything is successfull
SELECT @thisTime as 'ThisTime', @lastTime as 'LastTime'
GO