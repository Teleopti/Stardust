IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[Stage].[v_stg_schedule_load]'))
DROP VIEW [Stage].[v_stg_schedule_load]
GO
CREATE VIEW [Stage].[v_stg_schedule_load]
AS
SELECT
	shift_startdate_local_id			= dl.date_id,
	schedule_date_id					= dsd.date_id, 
	person_id							= dp.person_id, 
	interval_id							= di.interval_id, 
	activity_starttime					= f.activity_start,		
	scenario_id							= ds.scenario_id, 
	activity_id							= da.activity_id, 
	absence_id							= ab.absence_id,
	activity_startdate_id				= dad.date_id, 
	activity_enddate_id					= daed.date_id, 
	activity_endtime					= f.activity_end, 
	shift_startdate_id					= ssd.date_id, 
	shift_starttime						= f.shift_start, 
	shift_enddate_id					= sed.date_id, 
	shift_endtime						= f.shift_end, 
	shift_startinterval_id				= f.shift_startinterval_id,
	shift_endinterval_id				= f.shift_endinterval_id,
	shift_category_id					= sc.shift_category_id, 
	shift_length_id						= sl.shift_length_id, 
	scheduled_time_m					= f.scheduled_time_m, 
	scheduled_time_absence_m			= f.scheduled_time_absence_m, 
	scheduled_time_activity_m			= f.scheduled_time_activity_m, 
	scheduled_contract_time_m			= f.scheduled_contract_time_m,
	scheduled_contract_time_activity_m	= f.scheduled_contract_time_activity_m,
	scheduled_contract_time_absence_m	= f.scheduled_contract_time_absence_m,
	scheduled_work_time_m				= f.scheduled_work_time_m,
	scheduled_work_time_activity_m		= f.scheduled_work_time_activity_m,
	scheduled_work_time_absence_m		= f.scheduled_work_time_absence_m,
	scheduled_over_time_m				= f.scheduled_over_time_m, 
	scheduled_ready_time_m				= f.scheduled_ready_time_m,
	scheduled_paid_time_m				= f.scheduled_paid_time_m,
	scheduled_paid_time_activity_m		= f.scheduled_paid_time_activity_m,
	scheduled_paid_time_absence_m		= f.scheduled_paid_time_absence_m,
	business_unit_id					= dp.business_unit_id,
	datasource_id						= f.datasource_id, 
	datasource_update_date				= f.datasource_update_date,
	overtime_id							= ot.overtime_id
FROM Stage.stg_schedule f
INNER JOIN
	mart.dim_person		dp
ON
	f.person_code		=			dp.person_code
	AND --trim
		(
				(f.schedule_date_local	>= dp.valid_from_date_local)

			AND
				(f.schedule_date_local < DATEADD(DAY,1,dp.valid_to_date_local)) --include everything on LeavingDate
		)
INNER JOIN
	mart.dim_date		dl
ON
	f.schedule_date_local	= dl.date_date
INNER JOIN
	mart.dim_date		dsd
ON
	f.schedule_date_utc	= dsd.date_date
	
INNER JOIN
	mart.dim_interval	di
ON
	f.interval_id = di.interval_id
INNER JOIN
	mart.dim_interval	dist
ON
	f.shift_startinterval_id = dist.interval_id
INNER JOIN
	mart.dim_activity	da
ON
	f.activity_code = da.activity_code
INNER JOIN
	mart.dim_absence	ab
ON
	f.absence_code = ab.absence_code
INNER JOIN
	mart.dim_scenario	ds
ON
	f.scenario_code = ds.scenario_code
INNER JOIN
	mart.dim_date	dad --activity start
ON
	DATEADD(dd, 0, DATEDIFF(dd, 0,f.activity_start)) = dad.date_date
INNER JOIN
	mart.dim_date daed
ON
	DATEADD(dd, 0, DATEDIFF(dd, 0,f.activity_end)) = daed.date_date
INNER JOIN
	mart.dim_date ssd
ON
	DATEADD(dd, 0, DATEDIFF(dd, 0,f.shift_start )) = ssd.date_date
INNER JOIN
	mart.dim_date sed	
ON
	DATEADD(dd, 0, DATEDIFF(dd, 0,f.shift_end )) = sed.date_date
INNER JOIN
	mart.dim_shift_category sc
ON
	f.shift_category_code = sc.shift_category_code
INNER JOIN
	mart.dim_shift_length sl
ON 
	f.shift_length_m = sl.shift_length_m
INNER JOIN
	mart.dim_overtime ot
ON
	f.overtime_code = ot.overtime_code
GO


