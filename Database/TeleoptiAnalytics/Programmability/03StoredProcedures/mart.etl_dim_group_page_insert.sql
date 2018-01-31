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
  FROM [mart].[dim_business_unit] bu
  WHERE business_unit_code = @business_unit_code
  AND NOT EXISTS (SELECT 1 
                     FROM [mart].[dim_group_page] WITH (NOLOCK)
                    WHERE group_code = @group_code and business_unit_id = bu.business_unit_id)

END

GO



