IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[DataOnStepHasChanged]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[DataOnStepHasChanged]
GO

CREATE PROCEDURE [mart].[DataOnStepHasChanged]
@stepName nvarchar(500),
@startDate datetime,
@endDate datetime,
@buId uniqueidentifier 
-- =============================================
-- Author:		Ola
-- Create date: 2013-04-02
-- Description:	Detects if a ETL step needs to run
-- =============================================
-- Date			Who	Description
-- =============================================
AS
-- exec mart.DataOnStepHasChanged 'fact_schedule', '2013-03-03', '2013-03-09', 'fc2f309c-3e3c-4cfb-9c11-a1570077b92b'
DECLARE @lastTime bigint
SELECT @lastTime = 0

-- do the same check on fact step as on stage step
IF @stepName = 'fact_schedule' SELECT @stepName = 'stg_schedule, stg_schedule_day_absence_count'
IF @stepName = 'fact_schedule_day_count' SELECT @stepName = 'stg_schedule_day_off_count, stg_day_off, dim_day_off'
IF @stepName = 'fact_schedule_preference' SELECT @stepName = 'stg_schedule_preference, stg_day_off, dim_day_off'

IF EXISTS (SELECT * FROM mart.LastUpdatedPerStep WHERE StepName = @stepName AND BusinessUnit = @buId)
	SELECT @lastTime = LastChecksum FROM LastUpdatedPerStep WHERE StepName = @stepName AND BusinessUnit = @buId

DECLARE @thisTime bigint

IF @stepName = 'stg_schedule, stg_schedule_day_absence_count' 
BEGIN
	SELECT @thisTime = sum(cast(checksum(Version,id) as bigint)) from dbo.PersonAssignment
	where minimum between @startDate AND @endDate
	AND BusinessUnit = @buId

	SELECT @thisTime = @thisTime + sum(cast(checksum(Version,id) as bigint)) from dbo.PersonAbsence
	where minimum between @startDate AND @endDate -- AND  maximum ??
	AND BusinessUnit = @buId

	SELECT @thisTime = @thisTime + sum(cast(checksum(Version,id) as bigint)) from dbo.PersonDayOff
	where Anchor between @startDate AND @endDate
	AND BusinessUnit = @buId
END

IF @stepName = 'stg_schedule_day_off_count, stg_day_off, dim_day_off' 
	SELECT @thisTime = sum(cast(checksum(Version,id) as bigint)) from dbo.PersonDayOff
	where Anchor between @startDate AND @endDate
	AND BusinessUnit = @buId

IF @stepName = 'stg_schedule_preference, stg_day_off, dim_day_off'
BEGIN
	SELECT @thisTime = sum(cast(checksum(Version,id) as bigint)) from dbo.PersonAssignment
	where minimum between @startDate AND @endDate
	AND BusinessUnit = @buId

	SELECT @thisTime = @thisTime + sum(cast(checksum(Version,id) as bigint)) from dbo.PreferenceDay
	where RestrictionDate between @startDate AND @endDate -- AND  maximum ??
	AND BusinessUnit = @buId

	SELECT @thisTime = @thisTime + sum(cast(checksum(Version,id) as bigint)) from dbo.PersonDayOff
	where Anchor between @startDate AND @endDate
	AND BusinessUnit = @buId
END

-- add check to more steps later??

-- stg_permission ??
-- [dbo].[AvailableData], [dbo].[Person], [dbo].[ApplicationRole]

IF @thisTime <> @lastTime
BEGIN
	IF EXISTS (SELECT * FROM mart.LastUpdatedPerStep WHERE StepName = @stepName AND BusinessUnit = @buId)
		UPDATE mart.LastUpdatedPerStep SET LastChecksum = @thisTime WHERE StepName = @stepName AND BusinessUnit = @buId
	ELSE
		INSERT mart.LastUpdatedPerStep VALUES(@stepName, @buId, @thisTime) 

	SELECT 1
END

ELSE
	SELECT 0


GO