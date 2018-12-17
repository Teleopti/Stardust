IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_fact_forecast_workload_tabular]'))
DROP VIEW [mart].[v_fact_forecast_workload_tabular]
GO
CREATE VIEW [mart].[v_fact_forecast_workload_tabular]
AS
SELECT         d.date_id, i.interval_id, f.start_time, f.workload_id, f.scenario_id, f.end_time, f.skill_id, f.forecasted_calls, f.forecasted_emails, f.forecasted_backoffice_tasks, f.forecasted_campaign_calls, 
                         f.forecasted_calls_excl_campaign, f.forecasted_talk_time_s, f.forecasted_campaign_talk_time_s, f.forecasted_talk_time_excl_campaign_s, f.forecasted_after_call_work_s, f.forecasted_campaign_after_call_work_s, 
                         f.forecasted_after_call_work_excl_campaign_s, f.forecasted_handling_time_s, f.forecasted_campaign_handling_time_s, f.forecasted_handling_time_excl_campaign_s, f.period_length_min, f.business_unit_id
FROM            mart.fact_forecast_workload AS f INNER JOIN
                         mart.bridge_time_zone AS b ON f.interval_id = b.interval_id AND f.date_id = b.date_id INNER JOIN
                         mart.dim_date AS d ON b.local_date_id = d.date_id INNER JOIN
                         mart.dim_interval AS i ON b.local_interval_id = i.interval_id INNER JOIN
                         mart.dim_time_zone AS tz ON tz.time_zone_id = b.time_zone_id
WHERE        (tz.time_zone_code COLLATE DATABASE_DEFAULT = (SELECT TRIM([value]) AS 'TimeZoneCodeInsights' FROM mart.sys_configuration WHERE [key]='TimeZoneCodeInsights'))
GO