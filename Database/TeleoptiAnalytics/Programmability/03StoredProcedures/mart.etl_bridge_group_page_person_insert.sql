IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_insert]
GO

-- =============================================
-- Description:	Insert bridge group page person
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_insert]
@person_codes nvarchar(max),
@group_page_code uniqueidentifier,
@business_unit_code uniqueidentifier

AS
BEGIN
	insert into [mart].[bridge_group_page_person]
	select gp.group_page_id, p.person_id, 1, GETUTCDATE() 
	from mart.SplitStringGuid(@person_codes) 
	join [mart].dim_person p  WITH (NOLOCK)
		on p.person_code = id AND p.business_unit_code = @business_unit_code
	join [mart].[dim_group_page] gp
		on gp.group_code = @group_page_code AND gp.business_unit_code = @business_unit_code
END

GO



