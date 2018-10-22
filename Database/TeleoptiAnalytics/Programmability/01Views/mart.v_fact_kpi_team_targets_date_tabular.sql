IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_fact_kpi_team_targets_date_tabular]'))
DROP VIEW [mart].[v_fact_kpi_team_targets_date_tabular]
GO
CREATE VIEW [mart].[v_fact_kpi_team_targets_date_tabular]
AS
SELECT        vb.date_id, vb.team_id, f.kpi_id, f.target_value, f.min_value, f.max_value, vb.business_unit_id
FROM            mart.v_bridge_acd_login_person_date AS vb INNER JOIN
                         mart.fact_kpi_targets_team AS f ON f.team_id = vb.team_id
GO