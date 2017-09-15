IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_delete_for_person_period]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_delete_for_person_period]
GO

-- =============================================
-- Description:	Delete all group mappings for a specific person_period
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_delete_for_person_period]
@person_id int,
@group_codes nvarchar(max),
@business_unit_code uniqueidentifier
AS
BEGIN

	delete bgpp from [mart].[bridge_group_page_person] bgpp
	join (select group_page_id from mart.SplitStringGuid(@group_codes) 
			join [mart].dim_group_page gp with (nolock)
				on gp.group_code = id AND gp.business_unit_code = @business_unit_code) group_page
		on group_page.group_page_id = bgpp.group_page_id
	where bgpp.person_id=@person_id
END

GO



