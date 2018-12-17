IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_fact_schedule_deviation_tabular]'))
DROP VIEW [mart].[v_fact_schedule_deviation_tabular]
GO
CREATE VIEW [mart].[v_fact_schedule_deviation_tabular]
AS
SELECT         f.shift_startdate_local_id,d.date_id, i.interval_id, f.person_id, f.scheduled_ready_time_s, f.ready_time_s, f.contract_time_s, f.deviation_schedule_s, f.deviation_schedule_ready_s, f.deviation_contract_s, f.business_unit_id, 
                         f.is_logged_in
FROM            mart.fact_schedule_deviation AS f INNER JOIN
                         mart.bridge_time_zone AS b ON f.interval_id = b.interval_id AND f.date_id = b.date_id INNER JOIN
                         mart.dim_date AS d ON b.local_date_id = d.date_id INNER JOIN
                         mart.dim_interval AS i ON b.local_interval_id = i.interval_id INNER JOIN
                         mart.dim_time_zone AS tz ON tz.time_zone_id = b.time_zone_id
WHERE        (tz.time_zone_code COLLATE DATABASE_DEFAULT = (SELECT TRIM([value]) AS 'TimeZoneCodeInsights' FROM mart.sys_configuration WHERE [key]='TimeZoneCodeInsights'))
GO