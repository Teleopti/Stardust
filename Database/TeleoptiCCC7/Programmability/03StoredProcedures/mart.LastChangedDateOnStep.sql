IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[LastChangedDateOnStep]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[LastChangedDateOnStep]
GO

CREATE PROCEDURE [mart].[LastChangedDateOnStep]
@stepName nvarchar(500),
@buId uniqueidentifier
-- =============================================
-- Author:		Ola
-- Create date: 2013-04-10
-- Description:	Returns the latest time that ETL has updated the schedules for 
-- =============================================
-- Date			Who	Description
-- =============================================
-- exec [mart].[LastChangedDateOnStep] 'stg_schedule, stg_schedule_day_absence_count', 'fc2f309c-3e3c-4cfb-9c11-a1570077b92b'
AS

DECLARE @thisTime datetime
DECLARE @lastTime datetime

--Get ETL last execution time
SELECT @lastTime= [Date]
FROM LastUpdatedPerStep a
WHERE BusinessUnit = @buId
AND StepName = @StepName

--Handle case "ETL never executed".
IF @lastTime IS NULL
BEGIN
	SET @lastTime=dateadd(hour,-1,getdate())
END

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
IF @stepName = 'Preferences'
BEGIN
	SELECT @thisTime = MAX(UpdatedOn) FROM PreferenceDay WHERE UpdatedOn >= @lastTime
END
IF @stepName = 'Schedules'
BEGIN
	INSERT INTO #PersonsUpdated (PersonId, BelongsToDate, InsertedOn)
	SELECT s.PersonId, s.BelongsToDate, s.InsertedOn
	FROM [ReadModel].[ScheduleDay] s
	INNER JOIN #persons p ON p.PersonId = s.PersonId
	WHERE InsertedOn > @lastTime -- >= (not > ) to be sure we do not miss any

	--get max for this BU
	SET @thisTime = (SELECT max(InsertedOn) from #PersonsUpdated)
END
IF @stepName = 'Permissions'
BEGIN
	--fixar ikväll
	select @thisTime = getutcdate()
END

--Handle case, "now rows detected"
IF @thisTime IS NULL
SET @thisTime = @lastTime

--return 'thisTime' to ETL. To be used later if everything is successfull
SELECT @thisTime as 'ThisTime', @lastTime as 'LastTime'