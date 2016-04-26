IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_day_off_insert_or_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_day_off_insert_or_update]
GO

-- =============================================
-- Author:		Pontus
-- Description:	Insert or update day off information
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_day_off_insert_or_update]
	@day_off_code uniqueidentifier, 
	@day_off_name nvarchar(50), 
	@display_color int,
	@business_unit_id int,
	@datasource_id smallint,
	@datasource_update_date smalldatetime,
	@display_color_html char(7),
	@day_off_shortname nvarchar(25)
AS
BEGIN

-- Because the day_off_code has been null for all customers this
-- procedure need to be aware of that and fill in the code when possible.
-- Eventually all the day offs for all customers will have a code.

IF NOT EXISTS (SELECT * FROM mart.dim_day_off WHERE 
	(day_off_code IS NOT NULL AND day_off_code = @day_off_code AND business_unit_id = @business_unit_id)
	OR (day_off_code IS NULL AND day_off_name = @day_off_name AND business_unit_id = @business_unit_id))
	INSERT INTO [mart].[dim_day_off]
           ([day_off_code]
           ,[day_off_name]
           ,[display_color]
           ,[business_unit_id]
           ,[datasource_id]
           ,[insert_date]
           ,[update_date]
           ,[datasource_update_date]
           ,[display_color_html]
           ,[day_off_shortname])
     VALUES
           (@day_off_code,
		   @day_off_name,
		   @display_color,
		   @business_unit_id,
		   @datasource_id,
		   GETUTCDATE(),
		   GETUTCDATE(),
		   @datasource_update_date,
		   @display_color_html,
		   @day_off_shortname)
ELSE
	IF EXISTS (SELECT * FROM mart.dim_day_off WHERE 
	day_off_code IS NULL AND day_off_name = @day_off_name AND business_unit_id = @business_unit_id)
		UPDATE [mart].[dim_day_off]
		SET
		   [day_off_name] = @day_off_name
		  ,[day_off_code] = @day_off_code 
		  ,[display_color] = @display_color
		  ,[business_unit_id] = @business_unit_id
		  ,[datasource_id] = @datasource_id
		  ,[update_date] = GETUTCDATE()
		  ,[datasource_update_date] = @datasource_update_date
		  ,[display_color_html] =  @display_color_html
		  ,[day_off_shortname] = @day_off_shortname
		WHERE 
			day_off_name = @day_off_name AND
			business_unit_id = @business_unit_id
	ELSE
		UPDATE [mart].[dim_day_off]
		SET
		   [day_off_name] = @day_off_name
		  ,[display_color] = @display_color
		  ,[business_unit_id] = @business_unit_id
		  ,[datasource_id] = @datasource_id
		  ,[update_date] = GETUTCDATE()
		  ,[datasource_update_date] = @datasource_update_date
		  ,[display_color_html] =  @display_color_html
		  ,[day_off_shortname] = @day_off_shortname
		WHERE 
			day_off_code = @day_off_code AND
			business_unit_id = @business_unit_id
END
GO
