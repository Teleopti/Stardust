IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Stage].[etl_stg_schedule_updated_special_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stage].[etl_stg_schedule_updated_special_load]
GO


CREATE PROCEDURE [stage].[etl_stg_schedule_updated_special_load]
AS
BEGIN

SET NOCOUNT ON;

--create local person periods dates
INSERT INTO Stage.stg_schedule_updated_personLocal
SELECT DISTINCT
person_id				= dp.person_id,
time_zone_id			= dp.time_zone_id,
person_code				= dp.person_code,
valid_from_date_local	= dd.date_date,
valid_to_date_local		= '2059-12-31'
FROM mart.dim_date dd
INNER JOIN mart.bridge_time_zone btz
	ON	btz.local_date_id = dd.date_id
INNER JOIN mart.dim_person		dp
	ON btz.time_zone_id =	dp.time_zone_id
	AND dp.valid_from_date_id = btz.date_id
	AND dp.valid_from_interval_id = btz.interval_id
INNER JOIN Stage.stg_schedule_changed stg
	ON stg.person_code = dp.person_code

UPDATE Stage.stg_schedule_updated_personLocal 
SET valid_to_date_local		= dd.date_date 
FROM mart.dim_date dd
INNER JOIN mart.bridge_time_zone btz
	ON	btz.local_date_id = dd.date_id
INNER JOIN mart.dim_person	dp
	ON btz.time_zone_id =	dp.time_zone_id
	AND dp.valid_to_date_id = btz.date_id
	AND dp.valid_to_interval_id = btz.interval_id
INNER JOIN Stage.stg_schedule_updated_personLocal stg
	ON stg.person_id = dp.person_id	

--this table is namned UTC,
--... but for now we will go for "agent local date", later used for delete on [shift_startdate_id]
--Currently we can't tell on what UTC-day the shift starts.
--We need local shift start time in order to calculate correct UTC on shift_startdate_id
INSERT INTO Stage.stg_schedule_updated_ShiftStartDateUTC
SELECT DISTINCT
	person_id			= p.person_id,
	shift_startdate_id	= dd.date_id
FROM Stage.stg_schedule_updated_personLocal dp
INNER JOIN stage.stg_schedule_changed stg
	ON stg.person_code = dp.person_code
INNER JOIN mart.dim_date dd
	ON dd.date_date = stg.schedule_date
INNER JOIN	mart.dim_person p
ON stg.person_code	= p.person_code
	AND --trim to person valid in this range
		(
				(stg.schedule_date	>= p.valid_from_date)

			AND
				(stg.schedule_date < p.valid_to_date)
		)

END

GO

