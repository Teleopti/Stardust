IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_group_page_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_group_page_update]
GO

-- =============================================
-- Description:	Update group page
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_group_page_update]
@group_page_code uniqueidentifier,
@group_page_name nvarchar(100),
@group_page_name_resource_key nvarchar(100),
@group_code uniqueidentifier,
@group_name nvarchar(1024),
@group_is_custom bit

AS
BEGIN

  update [mart].[dim_group_page]
  set 
	group_page_name = @group_page_name
	,group_page_name_resource_key = @group_page_name_resource_key
	,group_name=@group_name
	,group_is_custom=@group_is_custom
	,datasource_update_date=GETUTCDATE()
  where group_page_code = @group_page_code and group_code = @group_code 

END

GO



