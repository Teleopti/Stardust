IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_time_zone_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_time_zone_load]
GO


-- =============================================
-- Author:                                      ChLu
-- Create date: 2008-01-30
-- Description:                             Loads date from stg_time_zone to dim_time_zone
-- Update date: 2009-02-11
-- 2009-02-11 KJ New mart schema
-- 2009-02-09 KJ Stage moved to mart db, removed view
-- 2009-04-01 KJ Change in inserts, first UTC time zone and then all other with default timezone first
-- 2011-09-21 DJ - "there can be only one"
-- 2011-09-21 DJ mark obsolete time zones with "to_be_deleted"

-- =============================================
CREATE PROCEDURE [mart].[etl_dim_time_zone_load] 
                             
AS

--reset default_zone and mark for delete on time zone that are not available any more
UPDATE mart.dim_time_zone
SET		default_zone	= 0,
		to_be_deleted	= 1
FROM mart.dim_time_zone d 
WHERE NOT EXISTS (SELECT time_zone_code
					FROM Stage.stg_time_zone s
					WHERE d.time_zone_code = s.time_zone_code
					)

-- Update time zone
UPDATE mart.dim_time_zone
SET
	time_zone_name		= s.time_zone_name, 
	default_zone		= s.default_zone, 
	utc_conversion		= s.utc_conversion, 
	utc_conversion_dst	= s.utc_conversion_dst, 
	update_date			= getdate(),
	to_be_deleted		= 0,
	utc_in_use			= s.utc_in_use
FROM
	Stage.stg_time_zone s 
WHERE
	mart.dim_time_zone.time_zone_code = s.time_zone_code

-- Insert UTC time zone first
INSERT INTO mart.dim_time_zone
	 (
	 time_zone_code, 
	 time_zone_name, 
	 default_zone, 
	 utc_conversion, 
	 utc_conversion_dst, 
	 datasource_id, 
	 insert_date, 
	 update_date,
	 to_be_deleted,
	 utc_in_use
	 )
SELECT 
	time_zone_code		= s.time_zone_code, 
	time_zone_name		= s.time_zone_name, 
	default_zone		= s.default_zone, 
	utc_conversion		= s.utc_conversion, 
	utc_conversion_dst	= s.utc_conversion_dst, 
	datasource_id		= -1, 
	insert_date			= getdate(), 
	update_date			= getdate(),
	to_be_deleted		= 0,
	utc_in_use			= s.utc_in_use
FROM
	Stage.stg_time_zone s
WHERE
	NOT EXISTS (SELECT time_zone_code FROM mart.dim_time_zone d WHERE d.time_zone_code = s.time_zone_code)
AND s.time_zone_name='UTC'

----------------------------------------------------------------------------
-- Insert new time zone
INSERT INTO mart.dim_time_zone
	 (
	 time_zone_code, 
	 time_zone_name, 
	 default_zone, 
	 utc_conversion, 
	 utc_conversion_dst, 
	 datasource_id, 
	 insert_date, 
	 update_date,
	 to_be_deleted
	 )
SELECT
	 time_zone_code		= s.time_zone_code, 
	 time_zone_name		= s.time_zone_name, 
	 default_zone		= s.default_zone, 
	 utc_conversion		= s.utc_conversion, 
	 utc_conversion_dst	= s.utc_conversion_dst, 
	 datasource_id		= -1, 
	 insert_date		= getdate(), 
	 update_date		= getdate(),
	 to_be_deleted		= 0
FROM
	Stage.stg_time_zone s
WHERE 
	NOT EXISTS (SELECT time_zone_code FROM mart.dim_time_zone d WHERE d.time_zone_code = s.time_zone_code)
ORDER BY s.default_zone DESC,s.time_zone_name

GO
