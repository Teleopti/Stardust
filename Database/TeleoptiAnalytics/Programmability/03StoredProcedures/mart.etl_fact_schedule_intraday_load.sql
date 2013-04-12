IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_schedule_intraday_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_schedule_intraday_load]
GO

-- =============================================
-- Author:		DJ
-- Create date: 2013-04-11
-- Description:	Write schedule activities from staging table 'stg_schedule'
--				to data mart table 'fact_schedule'.
-- Updates:
------------------------------------------------
--	Date	Who		Why

-- =============================================
--exec mart.etl_fact_schedule_intraday_load '2009-02-02','2009-02-03'
--exec mart.etl_fact_schedule_intraday_load @start_date='2010-10-30 00:00:00',@end_date='2010-11-03 00:00:00'
CREATE PROCEDURE [mart].[etl_fact_schedule_intraday_load]
@business_unit_code uniqueidentifier,
@scenario_code uniqueidentifier
AS

--delete days changed from mart.fact_schedule (new, updated or deleted)
DELETE fs
FROM Stage.stg_schedule_changed stg
INNER JOIN
	mart.dim_person		dp
ON
	stg.person_code		=			dp.person_code
	AND --trim
		(
				(stg.shift_start	>= dp.valid_from_date)

			AND
				(stg.shift_start < dp.valid_to_date)
		)
INNER JOIN mart.dim_date dd
	ON dd.date_date = stg.schedule_date
INNER JOIN mart.dim_scenario ds
	ON stg.scenario_code = ds.scenario_code
INNER JOIN mart.fact_schedule fs
	ON dd.date_id = fs.schedule_date_id
	AND dp.person_id = fs.person_id
	AND stg.interval_id = fs.interval_id
	AND ds.scenario_id = fs.scenario_id
WHERE stg.business_unit_code = @business_unit_code
AND stg.scenario_code = @scenario_code

--insert new and updated
INSERT INTO mart.fact_schedule
	(
	schedule_date_id, 
	person_id, 
	interval_id, 
	activity_starttime, 
	scenario_id, 
	activity_id, 
	absence_id, 
	activity_startdate_id, 
	activity_enddate_id, 
	activity_endtime, 
	shift_startdate_id, 
	shift_starttime, 
	shift_enddate_id, 
	shift_endtime, 
	shift_startinterval_id, 
	shift_category_id, 
	shift_length_id, 
	scheduled_time_m, 
	scheduled_time_absence_m,
	scheduled_time_activity_m,
	scheduled_contract_time_m,
	scheduled_contract_time_activity_m,
	scheduled_contract_time_absence_m,
	scheduled_work_time_m, 
	scheduled_work_time_activity_m,
	scheduled_work_time_absence_m,
	scheduled_over_time_m, 
	scheduled_ready_time_m,
	scheduled_paid_time_m,
	scheduled_paid_time_activity_m,
	scheduled_paid_time_absence_m,
	last_publish, 
	business_unit_id,
	datasource_id, 
	datasource_update_date,
	overtime_id
	)
SELECT * FROM Stage.v_stg_Schedule_load
GO
