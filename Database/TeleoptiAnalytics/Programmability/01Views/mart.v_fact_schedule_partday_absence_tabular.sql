IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_fact_schedule_partday_absence_tabular]'))
DROP VIEW [mart].[v_fact_schedule_partday_absence_tabular]
GO

CREATE VIEW [mart].[v_fact_schedule_partday_absence_tabular]
AS
SELECT f.shift_startdate_local_id,f.person_id,f.scenario_id,f.absence_id,f.business_unit_id,sum([scheduled_contract_time_absence_m]) as 'scheduled_contract_time_absence_m',1 as 'part_day_absence_count'
FROM mart.fact_schedule f
LEFT JOIN  mart.fact_schedule_day_count dc on 
		f.shift_startdate_local_id=dc.shift_startdate_local_id 
		AND f.person_id=dc.person_id 
		AND f.scenario_id=dc.scenario_id
		AND f.absence_id=dc.absence_id
WHERE f.absence_id<>-1 
AND dc.shift_startdate_local_id IS NULL
GROUP BY f.shift_startdate_local_id,f.person_id,f.scenario_id,f.absence_id ,f.business_unit_id

GO


