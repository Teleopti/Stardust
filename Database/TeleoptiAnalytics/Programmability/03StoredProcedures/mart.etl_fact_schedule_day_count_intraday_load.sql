IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_day_count_intraday_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_day_count_intraday_load]
GO

-- =============================================
-- Author:		David
-- Create date: 2013-05-16
-- Description:	Write scheduled shift_category,day_off,absence.
--				For Schedules we make the delete based on the Shift_StartTime
--				The stage table must only contain "Full UTC-day" loaded from ETL-tool.
-- Interface:	smalldatetime, with only datepart! No time allowed
-- =============================================
CREATE PROCEDURE [mart].[etl_fact_schedule_day_count_intraday_load] 
--exec [mart].[etl_fact_schedule_day_count_intraday_load]  @business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA',@scenario_code='E21D813C-238C-4C3F-9B49-9B5E015AB432'
@business_unit_code uniqueidentifier,
@scenario_code uniqueidentifier
AS

CREATE TABLE #stg_schedule_changed(
	person_id int,
	date_id int,
	scenario_id int,
	business_unit_id int
	)

CREATE TABLE #fact_schedule_day_off_count (
	date_id int, 
	start_interval_id int, 
	person_id int, 
	scenario_id int,
	date smalldatetime, 
	person_code uniqueidentifier, 
	scenario_code uniqueidentifier,
	)

--Verify this is the default scenario, if not RAISERROR
if (select count(*)
	from mart.dim_scenario
	where business_unit_code = @business_unit_code
	and scenario_code = @scenario_code
	) <> 1
BEGIN
	DECLARE @ErrorMsg nvarchar(4000)
	SELECT @ErrorMsg  = 'This is not a default scenario, or muliple default scenarios exists!'
	RAISERROR (@ErrorMsg,16,1)
END

--Get BU id
DECLARE @business_unit_id int
SELECT @business_unit_id = business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code

--Create a tmp table with Id instead of code
INSERT INTO #stg_schedule_changed
SELECT dp.person_id,dd.date_id,ds.scenario_id,bu.business_unit_id
FROM Stage.stg_schedule_changed stg
INNER JOIN mart.dim_date dd
	ON dd.date_date = stg.schedule_date
INNER JOIN mart.dim_person dp
	ON dp.person_code = stg.person_code
	AND --trim
		(
				(stg.schedule_date	>= dp.valid_from_date)

			AND
				(stg.schedule_date < dp.valid_to_date)
		)
INNER JOIN mart.dim_scenario ds
	ON ds.scenario_code = stg.scenario_code
	AND ds.scenario_code = @scenario_code
INNER JOIN mart.dim_business_unit bu
	ON bu.business_unit_code = stg.business_unit_code
	AND bu.business_unit_code = @business_unit_code

--delete days changed from mart.fact_schedule_day_count (new, updated or deleted)
DELETE fc
FROM #stg_schedule_changed stg
INNER JOIN mart.fact_schedule_day_count fc
	ON stg.date_id = fc.date_id
	AND stg.person_id = fc.person_id
	AND stg.business_unit_id = fc.business_unit_id
	AND stg.scenario_id = fc.scenario_id

----------------------------------------------------------------------------------
-- Insert shift_category_count rows
-- Take a look at performance on the select
INSERT INTO mart.fact_schedule_day_count
	(
	date_id, --day the absence start, the shift starts or the day off starts
	start_interval_id,--interval_id that the the absence start, the shift starts or the day off starts
	person_id, 
	scenario_id, 
	starttime,
	shift_category_id, 
	day_off_id, 
	absence_id, 
	day_count, 
	business_unit_id, 
	datasource_id, 
	datasource_update_date
	)

SELECT
	date_id					= f.shift_startdate_id, 
	start_interval_id		= f.shift_startinterval_id,
	person_id				= f.person_id, 
	scenario_id				= f.scenario_id, 
	starttime				= MAX(f.shift_starttime),
	shift_category_id		= MAX(f.shift_category_id), 
	day_off_id				= -1, 
	absence_id				= -1, 
	day_count				= COUNT(DISTINCT f.shift_category_id) , 
	business_unit_id		= f.business_unit_id, 
	datasource_id			= f.datasource_id, 
	datasource_update_date	= MAX(datasource_update_date)
FROM
	mart.fact_schedule f
INNER JOIN #stg_schedule_changed stg
	ON f.shift_startdate_id = stg.date_id
	AND f.person_id = stg.person_id
	AND f.business_unit_id  = @business_unit_id
WHERE shift_category_id<>-1
GROUP BY f.shift_startdate_id,f.shift_startinterval_id,f.person_id,f.scenario_id,f.business_unit_id,f.datasource_id

--WHOLE DAY ABSENCES
INSERT INTO mart.fact_schedule_day_count
	(
	date_id, --day the absence start, the shift starts or the day off starts
	start_interval_id,--interval_id that the the absence start, the shift starts or the day off starts
	person_id, 
	scenario_id, 
	starttime,
	shift_category_id, 
	day_off_id, 
	absence_id, 
	day_count, 
	business_unit_id, 
	datasource_id
	)
SELECT
	date_id					= dsd.date_id, 
	start_interval_id		= di.interval_id,
	person_id				= dp.person_id, 
	scenario_id				= ds.scenario_id, 
	starttime				= max(stg.starttime),
	shift_category_id		= -1, 
	day_off_id				= -1,
	absence_id				= max(da.absence_id), 
	day_count				= max(stg.day_count), 
	business_unit_id		= dp.business_unit_id, 
	datasource_id			= stg.datasource_id
FROM (SELECT * FROM Stage.stg_schedule_day_absence_count) stg
JOIN
	mart.dim_person		dp

ON
	stg.person_code	= dp.person_code
	AND --trim to person valid in this range
		(
				(stg.starttime	>= dp.valid_from_date)

			AND
				(stg.starttime < dp.valid_to_date)
		)

JOIN
	mart.dim_absence da ON
	da.absence_code = stg.absence_code
LEFT JOIN
	mart.dim_date dsd
ON
	stg.date = dsd.date_date
LEFT JOIN
	mart.dim_interval	di
ON
	stg.start_interval_id = di.interval_id
LEFT JOIN
	mart.dim_scenario	ds
ON
	stg.scenario_code = ds.scenario_code
GROUP BY dsd.date_id,di.interval_id,dp.person_id,ds.scenario_id,dp.business_unit_id,stg.datasource_id

--DAY OFF 
--(START: Special handling to increase start_interval_id for day off if already shift category or absence on same PK.)
INSERT INTO #fact_schedule_day_off_count
SELECT
	date_id				= dsd.date_id, 
	start_interval_id	= di.interval_id,
	person_id			= dp.person_id, 
	scenario_id			= ds.scenario_id,
	date				= stg.date,
	person_code			= stg.person_code,
	scenario_code		= stg.scenario_code
FROM (SELECT * FROM Stage.stg_schedule_day_off_count) stg
JOIN
	mart.dim_person		dp
ON
	stg.person_code	= dp.person_code	AND
	stg.date BETWEEN dp.valid_from_date	AND dp.valid_to_date  --Is person valid in this range	
JOIN
	mart.dim_day_off dd ON
	stg.day_off_name = dd.day_off_name AND
	dd.business_unit_id = dp.business_unit_id
LEFT JOIN
	mart.dim_date dsd
ON
	stg.date = dsd.date_date
LEFT JOIN
	mart.dim_interval	di
ON
	stg.start_interval_id = di.interval_id
LEFT JOIN
	mart.dim_scenario	ds
ON
	stg.scenario_code = ds.scenario_code
GROUP BY dsd.date_id,stg.date,di.interval_id,dp.person_id,stg.person_code,ds.scenario_id,stg.scenario_code,dd.day_off_id,dp.business_unit_id,stg.datasource_id

UPDATE stage.stg_schedule_day_off_count
SET start_interval_id = stg.start_interval_id + 1
FROM stage.stg_schedule_day_off_count stg
	INNER JOIN #fact_schedule_day_off_count do
		ON stg.date = do.date
			AND stg.start_interval_id = do.start_interval_id
			AND stg.person_code = do.person_code
			AND stg.scenario_code = do.scenario_code
WHERE EXISTS (
			SELECT date_id, start_interval_id, person_id, scenario_id 
			FROM mart.fact_schedule_day_count
			WHERE date_id = do.date_id AND start_interval_id = do.start_interval_id AND person_id = do.person_id AND scenario_id = do.scenario_id
			)
--(END: Special handling to increase start_interval_id for day off if already shift category or absence on same PK.)

INSERT INTO mart.fact_schedule_day_count
	(
	date_id, --day the absence start, the shift starts or the day off starts
	start_interval_id,--interval_id that the the absence start, the shift starts or the day off starts
	person_id, 
	scenario_id, 
	starttime,
	shift_category_id, 
	day_off_id, 
	absence_id, 
	day_count, 
	business_unit_id, 
	datasource_id, 
	datasource_update_date
	)

SELECT
	date_id					= dsd.date_id, 
	start_interval_id		= di.interval_id,
	person_id				= dp.person_id, 
	scenario_id				= ds.scenario_id, 
	starttime				= max(stg.starttime),
	shift_category_id		= -1, 
	day_off_id				= dd.day_off_id, 
	absence_id				= -1, 
	day_count				= max(stg.day_count), 
	business_unit_id		= dp.business_unit_id, 
	datasource_id			= stg.datasource_id, 
	datasource_update_date	= max(stg.datasource_update_date)
FROM (SELECT * FROM Stage.stg_schedule_day_off_count) stg
JOIN
	mart.dim_person		dp
ON
	stg.person_code	= dp.person_code	AND
	stg.date BETWEEN dp.valid_from_date	AND dp.valid_to_date  --Is person valid in this range	
JOIN
	mart.dim_day_off dd ON
	stg.day_off_name = dd.day_off_name AND
	dd.business_unit_id = dp.business_unit_id
LEFT JOIN
	mart.dim_date dsd
ON
	stg.date = dsd.date_date
LEFT JOIN
	mart.dim_interval	di
ON
	stg.start_interval_id = di.interval_id
LEFT JOIN
	mart.dim_scenario	ds
ON
	stg.scenario_code = ds.scenario_code
GROUP BY dsd.date_id,di.interval_id,dp.person_id,ds.scenario_id,dd.day_off_id,dp.business_unit_id,stg.datasource_id
