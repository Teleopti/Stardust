IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_group_page_delete_by_group_codes]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_group_page_delete_by_group_codes]
GO

-- =============================================
-- Description:	Delete group pages by group codes
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_group_page_delete_by_group_codes]
@group_codes nvarchar(max)

AS
BEGIN
  delete from [mart].[dim_group_page]
  where group_code in (select * from mart.SplitStringGuid(@group_codes))

END

GO



