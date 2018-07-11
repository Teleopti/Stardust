IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_group_page_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_group_page_insert]
GO

-- =============================================
-- Description:	Insert new group page
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_group_page_insert]
@group_page_code uniqueidentifier,
@group_page_name nvarchar(100),
@group_page_name_resource_key nvarchar(100),
@group_code uniqueidentifier,
@group_name nvarchar(1024),
@group_is_custom bit,
@business_unit_code uniqueidentifier

AS
BEGIN 
	DECLARE @business_unit_id int, @business_unit_name nvarchar(100)

	SELECT 
		@business_unit_id = business_unit_id,
		@business_unit_name = business_unit_name
	FROM mart.dim_business_unit WITH (NOLOCK)
	WHERE business_unit_code = @business_unit_code

	MERGE mart.dim_group_page AS target  
    USING (SELECT 
				@group_page_code,
				@group_page_name,
				@group_page_name_resource_key,
				@group_code,
				@group_name,
				@group_is_custom,
				@business_unit_code
				) AS src 
					(
						group_page_code,
						group_page_name,
						group_page_name_resource_key,
						group_code,
						group_name,
						group_is_custom,
						business_unit_code
					)
		ON (
			target.group_page_code = src.group_page_code
			and target.group_name COLLATE DATABASE_DEFAULT = src.group_name COLLATE DATABASE_DEFAULT
			and target.business_unit_code = src.business_unit_code
			)
	WHEN NOT MATCHED THEN  
		INSERT (
					[group_page_code], 
					[group_page_name], 
					[group_page_name_resource_key], 
					[group_code], 
					[group_name], 
					[group_is_custom], 
					[business_unit_id], 
					[business_unit_code], 
					[business_unit_name], 
					[datasource_id], 
					[insert_date], 
					[datasource_update_date]
				)
		VALUES (
					@group_page_code,
					@group_page_name,
					@group_page_name_resource_key,
					@group_code,
					@group_name,
					@group_is_custom,
					@business_unit_id,
					@business_unit_code,
					@business_unit_name,
					1,
					GETUTCDATE(),
					GETUTCDATE()
				);

END

GO



