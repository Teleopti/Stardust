IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_delete]
GO

-- =============================================
-- Description:	Delete all people in specific group pages
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_delete]
@person_codes nvarchar(max),
@group_page_code uniqueidentifier,
@business_unit_code uniqueidentifier
AS
BEGIN
	delete bgpp from [mart].[bridge_group_page_person] bgpp
	join (select group_page_id 
			from [mart].[dim_group_page] 
			where group_code = @group_page_code and business_unit_code = @business_unit_code) groups
		on groups.group_page_id = bgpp.group_page_id
	join (select person_id 
			from mart.SplitStringGuid(@person_codes) 
			join [mart].dim_person p  WITH (NOLOCK)
				on p.person_code = id 
			where p.business_unit_code = @business_unit_code) person
		on person.person_id = bgpp.person_id
END

GO



