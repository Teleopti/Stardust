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
--exec mart.etl_fact_schedule_day_count_load '2013-02-04','2013-03-03','928DD0BC-BF40-412E-B970-9B5E015AADEA'

CREATE PROCEDURE [mart].[etl_fact_schedule_day_count_load] 
@start_date smalldatetime,
@end_date smalldatetime,
@business_unit_code uniqueidentifier
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
WHERE shift_startdate_local_id between @start_date_id AND @end_date_id
	AND business_unit_id = @business_unit_id

-----------------------------------------------------------------------------------
--SHIFTS
INSERT INTO mart.fact_schedule_day_count
	(
	shift_startdate_local_id, --day the absence start, the shift starts or the day off starts
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
	shift_startdate_local_id= shift_startdate_local_id, 
	person_id				= person_id, 
	scenario_id				= scenario_id, 
	starttime				= max(shift_starttime),
	shift_category_id		= max(shift_category_id), 
	day_off_id				= -1, 
	absence_id				= -1, 
	day_count				= 1,
	business_unit_id		= @business_unit_id, 
	datasource_id			= 1, --hard coded. If you start grouping on this on, make new index to support it!
	datasource_update_date	= getdate()
FROM
	mart.fact_schedule
WHERE shift_category_id<>-1
	AND shift_startdate_local_id BETWEEN @start_date_id AND @end_date_id
	AND business_unit_id = @business_unit_id
GROUP BY shift_startdate_local_id,person_id,scenario_id

--WHOLE DAY ABSENCES
/*
INSERT INTO mart.fact_schedule_day_count
	(
	shift_startdate_local_id, --day the absence start, the shift starts or the day off starts
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
	day_count				= 1,
	business_unit_id		= dp.business_unit_id, 
	datasource_id			= stg.datasource_id, 
	datasource_update_date	= getdate()
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
*/
--DAY OFF 
INSERT INTO mart.fact_schedule_day_count
	(
	shift_startdate_local_id, --day the absence start, the shift starts or the day off starts
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
	shift_startdate_local_id= dsd.date_id, 
	person_id				= dp.person_id, 
	scenario_id				= ds.scenario_id, 
	starttime				= stg.starttime,
	shift_category_id		= -1, 
	day_off_id				= dd.day_off_id, --is now available in domain, use it!
	absence_id				= -1, 
	day_count				= 1, 
	business_unit_id		= @business_unit_id, 
	datasource_id			= stg.datasource_id, 
	datasource_update_date	= stg.datasource_update_date
FROM (SELECT * FROM Stage.stg_schedule_day_off_count WHERE schedule_date_local between @start_date and @end_date) stg
JOIN
	mart.dim_person		dp
ON
	stg.person_code	= dp.person_code	AND
	stg.schedule_date_local BETWEEN dp.valid_from_date_local AND dp.valid_to_date_local  --Is person valid in this range	
JOIN
	mart.dim_day_off dd ON
	stg.day_off_name = dd.day_off_name AND
	dd.business_unit_id = dp.business_unit_id AND
	dd.business_unit_id = @business_unit_id
LEFT JOIN
	mart.dim_date dsd
ON
	stg.schedule_date_local = dsd.date_date
LEFT JOIN
	mart.dim_scenario	ds
ON
	stg.scenario_code = ds.scenario_code
GO