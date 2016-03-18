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

  declare @counter int
  select @counter = ISNULL(MAX(group_id), 0) from [mart].[dim_group_page]
  set @counter = @counter + 1

  insert into [mart].[dim_group_page]
  select 
            @group_page_code
            ,@group_page_name
            ,@group_page_name_resource_key
            ,@counter
            ,@group_code
            ,@group_name
            ,@group_is_custom
            ,bu.business_unit_id
            ,@business_unit_code
            ,bu.business_unit_name
            ,bu.datasource_id
            ,GETUTCDATE()
            ,GETUTCDATE()
  from [mart].[dim_business_unit] bu
  where business_unit_code = @business_unit_code

END

GO



