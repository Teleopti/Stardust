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
	MERGE [mart].[dim_day_off] AS target  
		USING (
				SELECT 
					@day_off_code,
					@day_off_name,
					@display_color,
					@business_unit_id,
					@datasource_id,
					@datasource_update_date,
					@display_color_html,
					@day_off_shortname
				) AS src 
				(
					[day_off_code]
					,[day_off_name]
					,[display_color]
					,[business_unit_id]
					,[datasource_id]
					,[datasource_update_date]
					,[display_color_html]
					,[day_off_shortname]
				)
			ON (
				target.day_off_code = src.day_off_code
				AND target.business_unit_id = src.business_unit_id
				)
		WHEN MATCHED THEN
			UPDATE SET
					[day_off_name] = src.[day_off_name]
					,[display_color] = src.[display_color]
					,[update_date] = GETUTCDATE()
					,[datasource_update_date] = src.[datasource_update_date]
					,[display_color_html] =  src.[display_color_html]
					,[day_off_shortname] = src.[day_off_shortname]
		WHEN NOT MATCHED THEN
			INSERT (
					[day_off_code]
					,[day_off_name]
					,[display_color]
					,[business_unit_id]
					,[datasource_id]
					,[insert_date]
					,[update_date]
					,[datasource_update_date]
					,[display_color_html]
					,[day_off_shortname]
					)
			VALUES (
					@day_off_code,
					@day_off_name,
					@display_color,
					@business_unit_id,
					@datasource_id,
					GETUTCDATE(),
					GETUTCDATE(),
					@datasource_update_date,
					@display_color_html,
					@day_off_shortname
					);
END
GO
