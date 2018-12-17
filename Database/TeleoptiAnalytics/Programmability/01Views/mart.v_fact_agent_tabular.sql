IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_fact_agent_tabular]'))
DROP VIEW [mart].[v_fact_agent_tabular]
GO
CREATE VIEW [mart].[v_fact_agent_tabular]
AS
SELECT         d.date_id, i.interval_id, f.acd_login_id, f.ready_time_s, f.logged_in_time_s, f.not_ready_time_s, f.idle_time_s, f.direct_outbound_calls, f.direct_outbound_talk_time_s, f.direct_incoming_calls, 
                         f.direct_incoming_calls_talk_time_s, f.admin_time_s
FROM            mart.fact_agent AS f INNER JOIN
                         mart.bridge_time_zone AS b ON f.interval_id = b.interval_id AND f.date_id = b.date_id INNER JOIN
                         mart.dim_date AS d ON b.local_date_id = d.date_id INNER JOIN
                         mart.dim_interval AS i ON b.local_interval_id = i.interval_id INNER JOIN
                         mart.dim_time_zone AS tz ON tz.time_zone_id = b.time_zone_id
WHERE        (tz.time_zone_code COLLATE DATABASE_DEFAULT = (SELECT TRIM([value]) AS 'TimeZoneCodeInsights' FROM mart.sys_configuration WHERE [key]='TimeZoneCodeInsights'))
GO