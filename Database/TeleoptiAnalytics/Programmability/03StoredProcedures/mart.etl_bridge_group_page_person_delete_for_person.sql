IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_delete_for_person]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_delete_for_person]
GO

-- =============================================
-- Description:	Delete given people in a specific group page
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_delete_for_person]
@group_page_code uniqueidentifier,
@person_code uniqueidentifier,
@business_unit_code uniqueidentifier
AS
BEGIN
	delete bgpp from [mart].[bridge_group_page_person] bgpp
	join (select group_page_id 
			from [mart].[dim_group_page] WITH (NOLOCK)
			where group_page_code = @group_page_code and business_unit_code = @business_unit_code) groups
		on groups.group_page_id = bgpp.group_page_id
	join (select person_id 
			from [mart].dim_person  WITH (NOLOCK)
			where person_code = @person_code 
			and business_unit_code = @business_unit_code) person
		on person.person_id = bgpp.person_id
END

GO



