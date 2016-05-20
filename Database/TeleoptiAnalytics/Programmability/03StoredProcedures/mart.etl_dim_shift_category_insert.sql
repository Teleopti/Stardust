IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_shift_category_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_shift_category_insert]
GO

CREATE PROCEDURE [mart].[etl_dim_shift_category_insert]
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
	INSERT INTO [mart].[dim_shift_category]
           ([shift_category_code]
           ,[shift_category_name]
		   ,[shift_category_shortname]
           ,[display_color]
           ,[business_unit_id]
           ,[datasource_id]
           ,[insert_date]
           ,[update_date]
		   ,[datasource_update_date]
           ,[is_deleted])
	SELECT 
            @shift_category_code,
			@shift_category_name,
			@shift_category_shortname,
			@display_color,
			@business_unit_id,
			@datasource_id,
			GETUTCDATE(),
			GETUTCDATE(),
			@datasource_update_date,
			@is_deleted
	WHERE NOT EXISTS (SELECT * FROM [mart].[dim_shift_category] WHERE shift_category_code = @shift_category_code AND
			business_unit_id = @business_unit_id)
END
GO

