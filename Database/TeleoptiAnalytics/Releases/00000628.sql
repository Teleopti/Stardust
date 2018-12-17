IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_fact_kpi_team_targets_date_tabular]'))
DROP VIEW [mart].[v_fact_kpi_team_targets_date_tabular]
GO

IF NOT EXISTS(SELECT 1 FROM mart.sys_configuration WHERE [key]='TimeZoneCodeInsights')
BEGIN
	INSERT mart.sys_configuration([key], [value])
	SELECT	[key]='TimeZoneCodeInsights',
			[value]= ISNULL(time_zone_code,'UTC')
	FROM mart.dim_time_zone WHERE default_zone = 1
END
GO
