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
--exec mart.etl_fact_schedule_day_count_intraday_load @business_unit_code='928DD0BC-BF40-412E-B970-9B5E015AADEA',@scenario_code='E21D813C-238C-4C3F-9B49-9B5E015AB432'

@business_unit_code uniqueidentifier,
@scenario_code uniqueidentifier
AS
SET NOCOUNT ON

if (select count(*)
	from mart.dim_scenario
	where business_unit_code = @business_unit_code
	and scenario_code = @scenario_code
	) <> 1
BEGIN
	DECLARE @ErrorMsg nvarchar(4000)
	SELECT @ErrorMsg  = 'This is not a default scenario, or muliple default scenarios exists!'
	RAISERROR (@ErrorMsg,16,1)
	RETURN 0
END

CREATE TABLE #fact_schedule_day_off_count (
	shift_startdate_local_id int, 
	person_id int, 
	scenario_id int,
	date smalldatetime, 
	person_code uniqueidentifier, 
	scenario_code uniqueidentifier,
	)

CREATE TABLE #stg_schedule_changed(
	[person_id] [int] NOT NULL,
	[shift_startdate_id] [int] NOT NULL,
	[scenario_id] [smallint] NOT NULL
)
--Get scenario_id
DECLARE @scenario_id int 
SELECT @scenario_id = scenario_id FROM mart.dim_scenario WHERE scenario_code= @scenario_code
DECLARE @business_unit_id int
SET @business_unit_id = (SELECT business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code)

--return numbers of rows to ETL from here
SET NOCOUNT OFF

INSERT INTO #stg_schedule_changed
SELECT  p.person_id,
		dd.date_id,
		s.scenario_id
FROM stage.stg_schedule_changed ch
INNER JOIN mart.dim_person p
	ON p.person_code = ch.person_code
		AND --trim
		(
				(ch.schedule_date_local	>= p.valid_from_date_local)

			AND
				(ch.schedule_date_local <= p.valid_to_date_local)
		)
INNER JOIN mart.dim_date dd
	ON dd.date_date = ch.schedule_date_local
INNER JOIN mart.dim_scenario s
	ON ch.scenario_code=s.scenario_code

DELETE fs
FROM mart.fact_schedule_day_count fs
INNER JOIN #stg_schedule_changed a
	ON	a.person_id = fs.person_id
	AND a.shift_startdate_id = fs.shift_startdate_local_id
	AND a.scenario_id = fs.scenario_id

----------------------------------------------------------------------------------
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
	shift_startdate_local_id= f.shift_startdate_local_id, 
	person_id				= f.person_id, 
	scenario_id				= f.scenario_id, 
	starttime				= max(f.shift_starttime),
	shift_category_id		= max(f.shift_category_id), 
	day_off_id				= -1, 
	absence_id				= -1, 
	day_count				= 1,
	business_unit_id		= @business_unit_id, 
	datasource_id			= 1, 
	datasource_update_date	= max(f.datasource_update_date)
FROM mart.fact_schedule f
INNER JOIN #stg_schedule_changed stg
	ON f.shift_startdate_local_id = stg.shift_startdate_id
	AND f.person_id = stg.person_id
WHERE shift_category_id<>-1
AND f.scenario_id = @scenario_id 
--need to group this one (costly .....) since mart.fact_schedule contains a lot of intervals per day, but we are only interested on the one and only(?) shift_category_id
GROUP BY f.shift_startdate_local_id,f.person_id,f.scenario_id

--WHOLE DAY ABSENCES
INSERT INTO mart.fact_schedule_day_count
	(
	shift_startdate_local_id, --day the absence start on agent local date
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
	day_off_id				= -1,
	absence_id				= da.absence_id, 
	day_count				= 1,
	business_unit_id		= dp.business_unit_id, 
	datasource_id			= stg.datasource_id,
	datasource_update_date	= stg.datasource_update_date
FROM (SELECT * FROM Stage.stg_schedule_day_absence_count) stg
INNER JOIN mart.dim_person dp
	ON stg.person_code = dp.person_code
	AND --trim to person valid in this range
		(
				(stg.schedule_date_local	>= dp.valid_from_date_local)

			AND
				(stg.schedule_date_local < dp.valid_to_date_local)
		)
INNER JOIN mart.dim_absence da
	ON da.absence_code = stg.absence_code
INNER JOIN mart.dim_date dsd
	ON stg.schedule_date_local = dsd.date_date
LEFT JOIN mart.dim_scenario ds
	ON stg.scenario_code = ds.scenario_code
WHERE ds.scenario_id = @scenario_id 

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
	date_id					= dsd.date_id,
	person_id				= dp.person_id, 
	scenario_id				= ds.scenario_id, 
	starttime				= stg.starttime,
	shift_category_id		= -1, 
	day_off_id				= dd.day_off_id, 
	absence_id				= -1, 
	day_count				= 1, 
	business_unit_id		= dp.business_unit_id, 
	datasource_id			= stg.datasource_id, 
	datasource_update_date	= stg.datasource_update_date
FROM (SELECT * FROM Stage.stg_schedule_day_off_count) stg
INNER JOIN mart.dim_person dp
	ON stg.person_code = dp.person_code
	AND stg.schedule_date_local BETWEEN dp.valid_from_date_local AND dp.valid_to_date_local  --Is person valid in this range	
INNER JOIN mart.dim_day_off dd
	ON stg.day_off_name = dd.day_off_name
	AND dd.business_unit_id = dp.business_unit_id
INNER JOIN mart.dim_date dsd
	ON stg.schedule_date_local = dsd.date_date
LEFT JOIN mart.dim_scenario	ds
	ON stg.scenario_code = ds.scenario_code
	AND ds.scenario_id = @scenario_id
WHERE NOT EXISTS (select 1 from mart.fact_schedule_day_count sdc where sdc.date_id = dsd.date_id and sdc.person_id = dp.person_id and sdc.scenario_id = ds.scenario_id and sdc.start_interval_id = di.interval_id)
GO