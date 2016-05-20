IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_shift_category_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_shift_category_update]
GO

CREATE PROCEDURE [mart].[etl_dim_shift_category_update]
	@shift_category_code uniqueidentifier,
	@shift_category_name nvarchar(100),
	@shift_category_shortname nvarchar(50),
	@display_color int,
	@business_unit_id int,
	@datasource_id smallint,
	@datasource_update_date smalldatetime,
	@is_deleted bit
AS
BEGIN
UPDATE [mart].[dim_shift_category]
   SET [shift_category_name] = @shift_category_name
           ,[shift_category_shortname] = @shift_category_shortname
		   ,[display_color] = @display_color
           ,[datasource_id] = @datasource_id
           ,[update_date] = GETUTCDATE()
		   ,[datasource_update_date] = @datasource_update_date
           ,[is_deleted] = @is_deleted
	WHERE shift_category_code = @shift_category_code AND
		  business_unit_id = @business_unit_id
END
GO
