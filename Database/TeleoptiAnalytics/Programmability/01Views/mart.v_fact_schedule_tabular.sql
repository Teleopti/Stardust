IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_fact_schedule_tabular]'))
DROP VIEW [mart].[v_fact_schedule_tabular]
GO
CREATE VIEW [mart].[v_fact_schedule_tabular]
AS
SELECT         f.shift_startdate_local_id,d.date_id, i.interval_id, f.person_id, f.activity_starttime, f.scenario_id, f.activity_id, f.absence_id, f.scheduled_time_m, f.shift_category_id, f.scheduled_contract_time_m, f.scheduled_paid_time_m, 
                         f.scheduled_ready_time_m, f.scheduled_over_time_m, f.scheduled_work_time_m, f.scheduled_contract_time_absence_m, f.business_unit_id, f.overtime_id, f.planned_overtime_m,f.shift_length_id
FROM            mart.fact_schedule AS f INNER JOIN
                         mart.bridge_time_zone AS b ON f.interval_id = b.interval_id AND f.schedule_date_id = b.date_id INNER JOIN
                         mart.dim_date AS d ON b.local_date_id = d.date_id INNER JOIN
                         mart.dim_interval AS i ON b.local_interval_id = i.interval_id INNER JOIN
                         mart.dim_time_zone AS tz ON tz.time_zone_id = b.time_zone_id
WHERE        (tz.time_zone_code COLLATE DATABASE_DEFAULT = (SELECT TRIM([value]) AS 'TimeZoneCodeInsights' FROM mart.sys_configuration WHERE [key]='TimeZoneCodeInsights'))
GO