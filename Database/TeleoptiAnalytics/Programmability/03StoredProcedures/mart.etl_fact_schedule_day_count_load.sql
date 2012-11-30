IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_day_count_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_day_count_load]
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-10-03
-- Description:	Write scheduled shift_category,day_off,absence.
--				For Schedules we make the delete based on the Shift_StartTime
--				The stage table must only contain "Full UTC-day" loaded from ETL-tool.
-- Interface:	smalldatetime, with only datepart! No time allowed
-- Updates:		2008108 Added columns start_interval_id and starttime and changed groupings since new PK KJ
--				20081027 Added load from v_stg_schedule_day_off_count KJ
--				20081031 Added load from v_stg_schedule_day_absence_count KJ
--				2009-02-09 Stage moved to mart db, removed view KJ
--              2009-02-11 New mart schema KJ
--				2010-01-19 Adding Day Off Name as PK
--				2012-11-21 Adding new columns for display_color_html and day_off_shortname
-- =============================================
--exec etl_fact_schedule_day_count_load '2005-12-01','2006-01-05'

CREATE PROCEDURE [mart].[etl_fact_schedule_day_count_load] 
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier
WITH EXECUTE AS OWNER
AS

--DECLARES
DECLARE @start_date_id	INT
DECLARE @end_date_id	INT
DECLARE @business_unit_id int

--There must not be any timevalue on the interface, since that will mess things up around midnight!
SET @start_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), @start_date, 112)) --ISO yyyymmdd
SET @end_date	= CONVERT(smalldatetime,CONVERT(nvarchar(30), @end_date, 112))

SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

-- Delete rows matching dates
DELETE FROM mart.fact_schedule_day_count 
WHERE date_id between @start_date_id AND @end_date_id
	AND business_unit_id = @business_unit_id

-----------------------------------------------------------------------------------
-- Insert shift_category_count rows

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
	date_id					= shift_startdate_id, 
	start_interval_id		= shift_startinterval_id,
	person_id				= person_id, 
	scenario_id				= scenario_id, 
	starttime				= MAX(shift_starttime),
	shift_category_id		= MAX(shift_category_id), 
	day_off_id				= -1, 
	absence_id				= -1, 
	day_count				= COUNT( DISTINCT shift_category_id) , 
	business_unit_id		= business_unit_id, 
	datasource_id			= datasource_id, 
	datasource_update_date	= MAX(datasource_update_date)
FROM
	mart.fact_schedule
WHERE shift_category_id<>-1
	AND shift_startdate_id BETWEEN @start_date_id AND @end_date_id
	AND business_unit_id = @business_unit_id
GROUP BY shift_startdate_id,shift_startinterval_id,person_id,scenario_id,business_unit_id,datasource_id
ORDER BY shift_startdate_id,shift_startinterval_id,person_id,scenario_id,business_unit_id,datasource_id


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
	day_off_id				= -1,
	absence_id				= max(da.absence_id), 
	day_count				= max(stg.day_count), 
	business_unit_id		= dp.business_unit_id, 
	datasource_id			= stg.datasource_id, 
	datasource_update_date	= max(stg.datasource_update_date)
FROM (SELECT * FROM Stage.stg_schedule_day_absence_count WHERE date between @start_date and @end_date) stg
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
ORDER BY dsd.date_id,di.interval_id,dp.person_id,ds.scenario_id,dp.business_unit_id,stg.datasource_id





--DAY OFF 

--(START: Special handling to increase start_interval_id for day off if already shift category or absence on same PK.)
CREATE TABLE #fact_schedule_day_off_count (
											date_id int, 
											start_interval_id int, 
											person_id int, 
											scenario_id int,
											date smalldatetime, 
											person_code uniqueidentifier, 
											scenario_code uniqueidentifier,
											)
INSERT INTO #fact_schedule_day_off_count
SELECT
	date_id				= dsd.date_id, 
	start_interval_id	= di.interval_id,
	person_id			= dp.person_id, 
	scenario_id			= ds.scenario_id,
	date				= stg.date,
	person_code			= stg.person_code,
	scenario_code		= stg.scenario_code
FROM (SELECT * FROM Stage.stg_schedule_day_off_count WHERE date between @start_date and @end_date) stg
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
ORDER BY dsd.date_id,di.interval_id,dp.person_id,ds.scenario_id,dd.day_off_id,dp.business_unit_id,stg.datasource_id

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
FROM (SELECT * FROM Stage.stg_schedule_day_off_count WHERE date between @start_date and @end_date) stg
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
ORDER BY dsd.date_id,di.interval_id,dp.person_id,ds.scenario_id,dd.day_off_id,dp.business_unit_id,stg.datasource_id