IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Stage].[etl_stg_schedule_updated_special_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stage].[etl_stg_schedule_updated_special_load]
GO


CREATE PROCEDURE [stage].[etl_stg_schedule_updated_special_load]
AS
BEGIN

SET NOCOUNT ON;

declare @minDate smalldatetime
declare @maxDate smalldatetime
create table #DimPersonLocalized(person_id int, valid_from_date_local smalldatetime,valid_to_date_local smalldatetime)
insert into #DimPersonLocalized
select * from mart.DimPersonLocalized('1900-01-01','2059-12-24')

--create local person periods dates
INSERT INTO Stage.stg_schedule_updated_personLocal
SELECT DISTINCT
person_id				= dp.person_id,
time_zone_id			= p.time_zone_id,
person_code				= p.person_code,
valid_from_date_local	= dp.valid_from_date_local,
valid_to_date_local		= dp.valid_to_date_local
FROM #DimPersonLocalized dp
INNER JOIN mart.dim_person p
	ON dp.person_id = p.person_id
INNER JOIN Stage.stg_schedule_changed stg
	ON stg.person_code = p.person_code
	
--get the UTC day via local date on person and bridge time zone
INSERT INTO Stage.stg_schedule_updated_ShiftStartDateUTC
SELECT DISTINCT
	person_id			= p.person_id,
	shift_startdate_id	= btz.date_id  --UTC
FROM Stage.stg_schedule_updated_personLocal dp
INNER JOIN stage.stg_schedule_changed stg
	ON stg.person_code = dp.person_code
INNER JOIN mart.dim_date dd
	ON dd.date_date = stg.schedule_date
INNER JOIN mart.dim_person p
	ON stg.person_code	= p.person_code
INNER JOIN	#DimPersonLocalized dp_loc
	ON dp_loc.person_id = p.person_id
	AND	 --trim to person valid in this range
		(
				(stg.schedule_date	>= dp_loc.valid_from_date_local)

			AND
				(stg.schedule_date <= dp_loc.valid_to_date_local)
		)
INNER JOIN mart.bridge_time_zone btz
	ON	btz.local_date_id = dd.date_id
	AND btz.time_zone_id = dp.time_zone_id
END

GO

