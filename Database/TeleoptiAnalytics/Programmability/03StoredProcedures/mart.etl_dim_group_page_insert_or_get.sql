IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_group_page_insert_or_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_group_page_insert_or_get]
GO

-- =============================================
-- Description:	Insert new group page
-- =============================================
-- exec [mart].[etl_dim_group_page_insert_or_get] '8CA8734E-C950-4314-BB76-20206A8426B2', 'Contract Schedule', 'ContractSchedule', '462F97C8-AEB1-4A27-9C05-9B5E015B2400', 'Sultimess 5 sdfsagdlys', 0, '928DD0BC-BF40-412E-B970-9B5E015AADEA'
CREATE PROCEDURE [mart].[etl_dim_group_page_insert_or_get]
@group_page_code uniqueidentifier,
@group_page_name nvarchar(100),
@group_page_name_resource_key nvarchar(100),
@group_code uniqueidentifier,
@group_name nvarchar(1024),
@group_is_custom bit,
@business_unit_code uniqueidentifier

AS
BEGIN
	CREATE TABLE #existing_group_page(
		group_page_code uniqueidentifier,
		group_page_name nvarchar(100),
		group_page_name_resource_key nvarchar(100),
		group_code uniqueidentifier,
		group_name nvarchar(100),
		group_is_custom bit,
		business_unit_code uniqueidentifier
	)

	INSERT INTO #existing_group_page
	SELECT
		group_page_code,
		group_page_name,
		group_page_name_resource_key,
		group_code,
		group_name,
		group_is_custom,
		gp.business_unit_code
	FROM [mart].[dim_group_page] gp WITH (NOLOCK)
	INNER JOIN mart.dim_business_unit bu WITH (NOLOCK) on gp.business_unit_code = bu.business_unit_code
	WHERE group_page_code = @group_page_code and group_name = @group_name and gp.business_unit_id = bu.business_unit_id

	IF EXISTS(SELECT 1 FROM #existing_group_page)
	BEGIN
		SELECT
			group_page_code AS GroupPageCode,
			group_page_name AS GroupPageName,
			group_page_name_resource_key AS GroupPageNameResourceKey,
			group_code AS GroupCode,
			group_name AS GroupName,
			group_is_custom AS GroupIsCustom,
			business_unit_code AS BusinessUnitCode
		FROM #existing_group_page
	END
	ELSE
	BEGIN
		INSERT INTO [mart].[dim_group_page] (
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
			[datasource_update_date])
		SELECT 
			@group_page_code
			,@group_page_name
			,@group_page_name_resource_key
			,@group_code
			,@group_name
			,@group_is_custom
			,bu.business_unit_id
			,@business_unit_code
			,bu.business_unit_name
			,bu.datasource_id
			,GETUTCDATE()
			,GETUTCDATE()
		FROM [mart].[dim_business_unit] bu WITH (NOLOCK)
		WHERE business_unit_code = @business_unit_code
	END
END

GO



