IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_group_page_delete_unused]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_group_page_delete_unused]
GO

-- =============================================
-- Description:	Delete unused group pages
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_group_page_delete_unused]
@business_unit_code uniqueidentifier

AS
BEGIN
  DELETE from [mart].[dim_group_page]
  WHERE business_unit_code = @business_unit_code
  AND group_page_id NOT IN (SELECT group_page_id FROM mart.bridge_group_page_person)

END

GO



