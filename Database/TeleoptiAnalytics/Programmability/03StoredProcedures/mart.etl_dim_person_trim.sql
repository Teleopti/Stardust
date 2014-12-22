IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_person_trim]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_person_trim]
GO

-- =============================================
-- Author:		Jonas Nordh
-- Create date: 200-09-25
-- Description:	Data that is bound to mart.dim_person is trimmed so that data only exists inside the person periods.
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_person_trim]
	
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

	/* mart.fact_schedule: Trim invalid fact data before period start */
	--SELECT p.valid_from_date, p.valid_to_date, s.shift_starttime, p.person_name,* 
	DELETE FROM mart.fact_schedule
	FROM mart.dim_person p
		INNER JOIN mart.fact_schedule s
	ON p.person_id = s.person_id
		WHERE p.valid_from_date > s.shift_starttime

	--/* mart.fact_contract: Trim invalid fact data after period end */
	--/* TODO: If shift starts in period and ends outside period schedule data will be left. That is correct. But fact_contract data is not handled */
	--/* TODO: same way. Contract data left is the one only inside period. Could then be diff between schedule and contract data */
	----SELECT p.person_id, p.valid_from_date, p.valid_to_date, p.valid_from_date_id, p.valid_from_interval_id, p.valid_to_date_id, p.valid_to_interval_id, c.date_id, c.interval_id, * 
	--DELETE FROM mart.fact_contract
	--FROM mart.dim_person p
	--INNER JOIN mart.fact_contract c
	--	ON p.person_id = c.person_id
	--WHERE ((p.valid_to_date_id < c.date_id)
	--	OR (p.valid_to_date_id = c.date_id AND p.valid_to_interval_id < c.interval_id))
	--	AND p.valid_to_date_id <> -2

	--/* mart.fact_contract: Trim invalid fact data before period start */
	----SELECT p.person_id, p.valid_from_date, p.valid_to_date, p.valid_from_date_id, p.valid_from_interval_id, p.valid_to_date_id, p.valid_to_interval_id, c.date_id, c.interval_id, * 
	--DELETE FROM mart.fact_contract
	--FROM mart.dim_person p
	--INNER JOIN mart.fact_contract c
	--	ON p.person_id = c.person_id
	--WHERE (p.valid_from_date_id > c.date_id)
	--	OR (p.valid_from_date_id = c.date_id AND p.valid_from_interval_id > c.interval_id)


	/* TODO: Trim mart.fact_schedule_preference, It is not in use as of today 2009-09-25 */

	 
	/* mart.fact_schedule_day_count: Trim invalid fact data after period end */
	--SELECT p.person_id, p.valid_from_date, p.valid_to_date, p.valid_from_date_id, p.valid_from_interval_id, p.valid_to_date_id, p.valid_to_interval_id, sdc.date_id, * 
	DELETE FROM mart.fact_schedule_day_count
	FROM mart.dim_person p
	INNER JOIN mart.fact_schedule_day_count sdc
		ON p.person_id = sdc.person_id
	WHERE p.valid_to_date < sdc.starttime 
		AND p.valid_to_date_id <> -2

	/* mart.fact_schedule_day_count: Trim invalid fact data before period start */
	--SELECT p.person_id, p.valid_from_date, p.valid_to_date, p.valid_from_date_id, p.valid_from_interval_id, p.valid_to_date_id, p.valid_to_interval_id, sdc.date_id, * 
	DELETE FROM mart.fact_schedule_day_count
	FROM mart.dim_person p
	INNER JOIN mart.fact_schedule_day_count sdc
		ON p.person_id = sdc.person_id
	WHERE p.valid_from_date > sdc.starttime


	/* mart.fact_schedule_deviation: Trim invalid fact data after period end */
	--SELECT p.person_id, p.valid_from_date, p.valid_to_date, p.valid_from_date_id, p.valid_from_interval_id, p.valid_to_date_id, p.valid_to_interval_id, sd.date_id, sd.interval_id, * 
	DELETE FROM mart.fact_schedule_deviation
	FROM mart.dim_person p
	INNER JOIN mart.fact_schedule_deviation sd
		ON p.person_id = sd.person_id
	WHERE ((p.valid_to_date_id < sd.shift_startdate_id)
		OR (p.valid_to_date_id = sd.shift_startdate_id AND p.valid_to_interval_id < sd.shift_startinterval_id))
		AND p.valid_to_date_id <> -2

	--/* mart.fact_schedule_deviation: Trim invalid fact data before period start */
	--SELECT p.person_id, p.valid_from_date, p.valid_to_date, p.valid_from_date_id, p.valid_from_interval_id, p.valid_to_date_id, p.valid_to_interval_id, sd.date_id, sd.interval_id, * 
	DELETE FROM mart.fact_schedule_deviation
	FROM mart.dim_person p
	INNER JOIN mart.fact_schedule_deviation sd
		ON p.person_id = sd.person_id
	WHERE (p.valid_from_date_id > sd.shift_startdate_id)
		OR (p.valid_from_date_id = sd.shift_startdate_id AND p.valid_from_interval_id > sd.shift_startinterval_id)
END

GO