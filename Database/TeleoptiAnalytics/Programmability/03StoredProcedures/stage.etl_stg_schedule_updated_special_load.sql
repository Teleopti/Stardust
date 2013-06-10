IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Stage].[etl_stg_schedule_updated_special_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stage].[etl_stg_schedule_updated_special_load]
GO


CREATE PROCEDURE [stage].[etl_stg_schedule_updated_special_load]
AS
BEGIN

SET NOCOUNT ON;

--@variables used to create local dates
declare @minutesPerInterval int
declare @maxInterval int
select @maxInterval=max(interval_id) from mart.dim_interval
select @minutesPerInterval=1440/@maxInterval

--create local person periods dates
INSERT INTO Stage.stg_schedule_updated_personLocal
SELECT DISTINCT
person_id				= dp.person_id,
time_zone_id			= dp.time_zone_id,
person_code				= dp.person_code,
valid_from_date_local	= dateadd(MINUTE,UpDown.conversion*btz.interval_id*@minutesPerInterval,dp.valid_from_date),
valid_to_date_local		= dateadd(MINUTE,UpDown.conversion*btz.interval_id*@minutesPerInterval,dp.valid_to_date)
FROM mart.dim_date dd
INNER JOIN mart.bridge_time_zone btz
	ON	btz.local_date_id = dd.date_id
INNER JOIN mart.v_time_zone_convertUpDown UpDown
	ON UpDown.time_zone_id = btz.time_zone_id
INNER JOIN mart.dim_interval di
	ON btz.local_interval_id = di.interval_id 
INNER JOIN mart.dim_person		dp
	ON btz.time_zone_id =	dp.time_zone_id
	AND dp.valid_from_date_id = btz.date_id
	AND dp.valid_from_interval_id = btz.interval_id
INNER JOIN Stage.stg_schedule_changed stg
	ON stg.person_code = dp.person_code

--create utc shift_date_id for local agent schedule day
INSERT INTO Stage.stg_schedule_updated_ShiftStartDateUTC
SELECT
	person_id			= dp.person_id,
	shift_startdate_id	= btz.date_id
FROM Stage.stg_schedule_updated_personLocal dp
INNER JOIN stage.stg_schedule_changed stg
	ON stg.person_code = dp.person_code
INNER JOIN mart.dim_date dd
	ON dd.date_date = stg.schedule_date
INNER JOIN mart.bridge_time_zone btz
	ON	btz.local_date_id = dd.date_id
	AND btz.time_zone_id =	dp.time_zone_id
END

GO

