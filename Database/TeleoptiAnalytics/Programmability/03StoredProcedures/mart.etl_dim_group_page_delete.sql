IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_group_page_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_group_page_delete]
GO

-- =============================================
-- Description:	Delete group pages
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_group_page_delete]
@group_page_codes nvarchar(max)

AS
BEGIN
  delete from [mart].[dim_group_page]
  where group_page_code in (select * from mart.SplitStringGuid(@group_page_codes))

END

GO



