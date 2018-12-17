IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_fact_schedule_forecast_skill_tabular]'))
DROP VIEW [mart].[v_fact_schedule_forecast_skill_tabular]
GO
CREATE VIEW [mart].[v_fact_schedule_forecast_skill_tabular]
AS
SELECT         d.date_id, i.interval_id, f.skill_id, f.scenario_id, f.forecasted_resources_m, f.forecasted_resources, f.forecasted_resources_incl_shrinkage_m, f.forecasted_resources_incl_shrinkage, 
                         f.scheduled_resources_m, f.scheduled_resources, f.scheduled_resources_incl_shrinkage_m, f.scheduled_resources_incl_shrinkage, f.intraday_deviation_m, f.relative_difference, f.business_unit_id, f.forecasted_tasks, 
                         f.estimated_tasks_answered_within_sl, f.forecasted_tasks_incl_shrinkage, f.estimated_tasks_answered_within_sl_incl_shrinkage
FROM            mart.fact_schedule_forecast_skill AS f INNER JOIN
                         mart.bridge_time_zone AS b ON f.interval_id = b.interval_id AND f.date_id = b.date_id INNER JOIN
                         mart.dim_date AS d ON b.local_date_id = d.date_id INNER JOIN
                         mart.dim_interval AS i ON b.local_interval_id = i.interval_id INNER JOIN
                         mart.dim_time_zone AS tz ON tz.time_zone_id = b.time_zone_id
WHERE        (tz.time_zone_code COLLATE DATABASE_DEFAULT = (SELECT TRIM([value]) AS 'TimeZoneCodeInsights' FROM mart.sys_configuration WHERE [key]='TimeZoneCodeInsights'))
GO