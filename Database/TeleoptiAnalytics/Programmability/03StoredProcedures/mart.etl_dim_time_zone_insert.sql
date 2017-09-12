IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_time_zone_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_time_zone_insert]
GO

-- =============================================
-- Description:	Insert new time zone
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_time_zone_insert]
@time_zone_code nvarchar(50),
@time_zone_name nvarchar(100),
@default_zone bit,
@utc_conversion int,
@utc_conversion_dst int

AS
BEGIN

  INSERT INTO [mart].[dim_time_zone]
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
            @time_zone_code
            ,@time_zone_name
            ,@default_zone
            ,@utc_conversion
            ,@utc_conversion_dst
            ,1
            ,GETUTCDATE()
            ,GETUTCDATE()
			,0

END

GO