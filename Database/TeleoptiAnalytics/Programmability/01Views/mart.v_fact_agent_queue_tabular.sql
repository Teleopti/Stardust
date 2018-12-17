IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_fact_agent_queue_tabular]'))
DROP VIEW [mart].[v_fact_agent_queue_tabular]
GO

CREATE VIEW [mart].[v_fact_agent_queue_tabular]
AS
SELECT         d.date_id, i.interval_id, f.queue_id, f.acd_login_id, f.talk_time_s, f.after_call_work_time_s, f.answered_calls, f.transfered_calls
FROM            mart.fact_agent_queue AS f INNER JOIN
                         mart.bridge_time_zone AS b ON f.interval_id = b.interval_id AND f.date_id = b.date_id INNER JOIN
                         mart.dim_date AS d ON b.local_date_id = d.date_id INNER JOIN
                         mart.dim_interval AS i ON b.local_interval_id = i.interval_id INNER JOIN
                         mart.dim_time_zone AS tz ON tz.time_zone_id = b.time_zone_id
WHERE        (tz.time_zone_code COLLATE DATABASE_DEFAULT = (SELECT TRIM([value]) AS 'TimeZoneCodeInsights' FROM mart.sys_configuration WHERE [key]='TimeZoneCodeInsights'))
GO