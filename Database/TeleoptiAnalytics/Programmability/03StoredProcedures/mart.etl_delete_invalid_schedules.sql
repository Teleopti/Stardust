IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_delete_invalid_schedules]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_delete_invalid_schedules]
GO

CREATE PROCEDURE [mart].[etl_delete_invalid_schedules]
@person_periodids nvarchar(max)
AS
BEGIN

	/* mart.fact_schedule: Trim invalid fact data after period end */
	--SELECT p.valid_from_date, p.valid_to_date, s.shift_starttime, p.person_name,* 
	DELETE FROM mart.fact_schedule
	FROM mart.dim_person p
	INNER JOIN mart.fact_schedule s
		ON p.person_id = s.person_id
	WHERE p.valid_to_date <= s.shift_starttime
		AND p.valid_to_date_id <> -2 
		AND p.person_id IN (select id from mart.SplitStringInt(@person_periodids))

	/* mart.fact_schedule: Trim invalid fact data before period start */
	--SELECT p.valid_from_date, p.valid_to_date, s.shift_starttime, p.person_name,* 
	DELETE FROM mart.fact_schedule
	FROM mart.dim_person p
		INNER JOIN mart.fact_schedule s
	ON p.person_id = s.person_id
		WHERE p.valid_from_date > s.shift_starttime 
		AND p.person_id IN (select id from mart.SplitStringInt(@person_periodids))

	/* mart.fact_schedule_day_count: Trim invalid fact data after period end */
	--SELECT p.person_id, p.valid_from_date, p.valid_to_date, p.valid_from_date_id, p.valid_from_interval_id, p.valid_to_date_id, p.valid_to_interval_id, sdc.date_id, * 
	DELETE FROM mart.fact_schedule_day_count
	FROM mart.dim_person p
	INNER JOIN mart.fact_schedule_day_count sdc
		ON p.person_id = sdc.person_id
	WHERE p.valid_to_date < sdc.starttime 
		AND p.valid_to_date_id <> -2 
		AND p.person_id IN (select id from mart.SplitStringInt(@person_periodids))

	/* mart.fact_schedule_day_count: Trim invalid fact data before period start */
	--SELECT p.person_id, p.valid_from_date, p.valid_to_date, p.valid_from_date_id, p.valid_from_interval_id, p.valid_to_date_id, p.valid_to_interval_id, sdc.date_id, * 
	DELETE FROM mart.fact_schedule_day_count
	FROM mart.dim_person p
	INNER JOIN mart.fact_schedule_day_count sdc
		ON p.person_id = sdc.person_id
	WHERE p.valid_from_date > sdc.starttime 
		AND p.person_id IN (select id from mart.SplitStringInt(@person_periodids))


	/* mart.fact_schedule_deviation: Trim invalid fact data after period end */
	--SELECT p.person_id, p.valid_from_date, p.valid_to_date, p.valid_from_date_id, p.valid_from_interval_id, p.valid_to_date_id, p.valid_to_interval_id, sd.date_id, sd.interval_id, * 
	DELETE FROM mart.fact_schedule_deviation
	FROM mart.dim_person p
	INNER JOIN mart.fact_schedule_deviation sd
		ON p.person_id = sd.person_id
	WHERE ((p.valid_to_date_id < sd.shift_startdate_id)
		OR (p.valid_to_date_id = sd.shift_startdate_id AND p.valid_to_interval_id < sd.shift_startinterval_id))
		AND p.valid_to_date_id <> -2
		AND p.person_id IN (select id from mart.SplitStringInt(@person_periodids))

	--/* mart.fact_schedule_deviation: Trim invalid fact data before period start */
	--SELECT p.person_id, p.valid_from_date, p.valid_to_date, p.valid_from_date_id, p.valid_from_interval_id, p.valid_to_date_id, p.valid_to_interval_id, sd.date_id, sd.interval_id, * 
	DELETE FROM mart.fact_schedule_deviation
	FROM mart.dim_person p
	INNER JOIN mart.fact_schedule_deviation sd
		ON p.person_id = sd.person_id
	WHERE ((p.valid_from_date_id > sd.shift_startdate_id)
		OR (p.valid_from_date_id = sd.shift_startdate_id AND p.valid_from_interval_id > sd.shift_startinterval_id))
		AND p.person_id IN (select id from mart.SplitStringInt(@person_periodids))

END

GO