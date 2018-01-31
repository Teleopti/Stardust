IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[sys_get_timezones_used_by_datasource_and_base_config]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[sys_get_timezones_used_by_datasource_and_base_config]
GO



CREATE PROCEDURE [mart].[sys_get_timezones_used_by_datasource_and_base_config]
AS
BEGIN
	
	SET NOCOUNT ON;	

	SELECT 
		t.time_zone_id AS TimeZoneId, 
		t.time_zone_code AS TimeZoneCode, 
		t.utc_in_use AS IsUtcInUse, 
		t.to_be_deleted AS ToBeDeleted  
	FROM 
		mart.dim_time_zone t WITH (NOLOCK)
		INNER JOIN mart.sys_datasource d  WITH (NOLOCK)
			ON d.time_zone_id = t.time_zone_id
	WHERE
		d.time_zone_id IS NOT NULL
	GROUP BY 
		t.time_zone_id, 
		t.time_zone_code, 
		t.utc_in_use, 
		t.to_be_deleted
	UNION
	SELECT
		t.time_zone_id AS TimeZoneId, 
		t.time_zone_code AS TimeZoneCode, 
		t.utc_in_use AS IsUtcInUse, 
		t.to_be_deleted AS ToBeDeleted   
	FROM 
		mart.dim_time_zone t WITH (NOLOCK)
		INNER JOIN mart.sys_configuration c WITH (NOLOCK)
			ON t.time_zone_code = c.[value]
	WHERE 
		c.[key] = 'TimeZoneCode'
END


GO

