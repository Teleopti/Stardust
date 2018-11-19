IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_group_page_insert_and_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_group_page_insert_and_get]
GO

-- =============================================
-- Description:	Insert new group page
-- =============================================
-- exec [mart].[etl_dim_group_page_insert_and_get] '8CA8734E-C950-4314-BB76-20206A8426B2', 'Contract Schedule', 'ContractSchedule', '462F97C8-AEB1-4A27-9C05-9B5E015B2400', 'Sultimess 5 sdfsagdlys', 0, '928DD0BC-BF40-412E-B970-9B5E015AADEA'
CREATE PROCEDURE [mart].[etl_dim_group_page_insert_and_get]
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
			and target.group_name COLLATE Latin1_General_CS_AS = src.group_name COLLATE Latin1_General_CS_AS
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

	SELECT
		group_page_code AS GroupPageCode,
		group_page_name AS GroupPageName,
		group_page_name_resource_key AS GroupPageNameResourceKey,
		group_code AS GroupCode,
		group_name AS GroupName,
		group_is_custom AS GroupIsCustom,
		business_unit_code AS BusinessUnitCode
	FROM 
		mart.dim_group_page WITH (NOLOCK)
	WHERE 
		group_page_code = @group_page_code and 
		group_name COLLATE Latin1_General_CS_AS = @group_name COLLATE Latin1_General_CS_AS and 
		business_unit_code = @business_unit_code
END

GO