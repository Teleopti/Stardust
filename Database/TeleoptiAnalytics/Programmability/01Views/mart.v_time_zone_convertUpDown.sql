IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[mart].[v_time_zone_convertUpDown]'))
DROP VIEW [mart].[v_time_zone_convertUpDown]
GO

CREATE View [mart].[v_time_zone_convertUpDown]
AS
SELECT
	time_zone_id,
	CASE 
		WHEN utc_conversion < 0 THEN -1
		ELSE 1
	END AS 'conversion'
FROM mart.dim_time_zone

GO