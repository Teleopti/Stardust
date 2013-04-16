IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[Stage].[v_stg_schedule_load]'))
DROP VIEW [Stage].[v_stg_schedule_load]
GO
CREATE VIEW [Stage].[v_stg_schedule_load]
AS
SELECT
	schedule_date_id					= dsd.date_id, 
	person_id							= dp.person_id, 
	interval_id							= di.interval_id, 
	activity_starttime					= f.activity_start,		
	scenario_id							= ds.scenario_id, 
	activity_id							= min(isnull(da.activity_id,-1)), 
	absence_id							= min(isnull(ab.absence_id,-1)),
	activity_startdate_id				= min(dad.date_id), 
	activity_enddate_id					= min(daed.date_id), 
	activity_endtime					= min(f.activity_end), 
	shift_startdate_id					= min(ssd.date_id), 
	shift_starttime						= min(f.shift_start), 
	shift_enddate_id					= min(sed.date_id), 
	shift_endtime						= min(f.shift_end), 
	shift_startinterval_id				= min(f.shift_startinterval_id), 
	shift_category_id					= min(isnull(sc.shift_category_id,-1)), 
	shift_length_id						= min(sl.shift_length_id), 
	scheduled_time_m					= sum(f.scheduled_time_m), 
	scheduled_time_absence_m			= sum(f.scheduled_time_absence_m), 
	scheduled_time_activity_m			= sum(f.scheduled_time_activity_m), 
	scheduled_contract_time_m			= sum(f.scheduled_contract_time_m),
	scheduled_contract_time_activity_m	= sum(f.scheduled_contract_time_activity_m),
	scheduled_contract_time_absence_m	= sum(f.scheduled_contract_time_absence_m),
	scheduled_work_time_m				= sum(f.scheduled_work_time_m),
	scheduled_work_time_activity_m		= sum(f.scheduled_work_time_activity_m),
	scheduled_work_time_absence_m		= sum(f.scheduled_work_time_absence_m),
	scheduled_over_time_m				= sum(f.scheduled_over_time_m), 
	scheduled_ready_time_m				= sum(f.scheduled_ready_time_m),
	scheduled_paid_time_m				= sum(f.scheduled_paid_time_m),
	scheduled_paid_time_activity_m		= sum(f.scheduled_paid_time_activity_m),
	scheduled_paid_time_absence_m		= sum(f.scheduled_paid_time_absence_m),
	last_publish						= min(f.last_publish), 
	business_unit_id					= min(dp.business_unit_id),
	datasource_id						= min(f.datasource_id), 
	datasource_update_date				= min(f.datasource_update_date),
	overtime_id							= min(isnull(ot.overtime_id, -1))
FROM 
	(SELECT * FROM Stage.stg_schedule) f
JOIN
	mart.dim_person		dp
ON
	f.person_code		=			dp.person_code
	AND --trim
		(
				(f.shift_start	>= dp.valid_from_date)

			AND
				(f.shift_start < dp.valid_to_date)
		)

LEFT JOIN
	mart.dim_date		dsd
ON
	f.schedule_date	= dsd.date_date
	
LEFT JOIN
	mart.dim_interval	di
ON
	f.interval_id = di.interval_id
LEFT JOIN
	mart.dim_interval	dist
ON
	f.shift_startinterval_id = dist.interval_id
LEFT JOIN
	mart.dim_activity	da
ON
	f.activity_code = da.activity_code
LEFT JOIN
	mart.dim_absence	ab
ON
	f.absence_code = ab.absence_code
LEFT JOIN
	mart.dim_scenario	ds
ON
	f.scenario_code = ds.scenario_code
LEFT JOIN
	mart.dim_date	dad --activity start
ON
	convert(smalldatetime,floor(convert(decimal(18,8),f.activity_start ))) = dad.date_date
LEFT JOIN
	mart.dim_date daed
ON
	convert(smalldatetime,floor(convert(decimal(18,8),f.activity_end ))) = daed.date_date
LEFT JOIN
	mart.dim_date ssd
ON
	convert(smalldatetime,floor(convert(decimal(18,8),f.shift_start ))) = ssd.date_date
LEFT JOIN
	mart.dim_date sed	
ON
	convert(smalldatetime,floor(convert(decimal(18,8),f.shift_end ))) = sed.date_date
LEFT JOIN
	mart.dim_shift_category sc
ON
	f.shift_category_code = sc.shift_category_code
LEFT JOIN
	mart.dim_shift_length sl
ON 
	f.shift_length_m = sl.shift_length_m
LEFT JOIN
	mart.dim_overtime ot
ON
	f.overtime_code = ot.overtime_code
GROUP BY 
	dsd.date_id, dp.person_id, di.interval_id, f.activity_start, ds.scenario_id, overtime_id

GO


