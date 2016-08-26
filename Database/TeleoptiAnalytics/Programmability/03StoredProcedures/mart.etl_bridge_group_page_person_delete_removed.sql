IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_delete_removed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_delete_removed]
GO

-- =============================================
-- Description:	Delete all group mappings for a specific person where the person period code is not in the parameter list
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_delete_removed]
@person_code uniqueidentifier,
@person_period_codes nvarchar(max),
@business_unit_code uniqueidentifier
AS
BEGIN

	delete bgpp from [mart].[bridge_group_page_person] bgpp
	inner join mart.dim_person p
		on bgpp.person_id = p.person_id AND p.business_unit_code = @business_unit_code
	left outer join mart.SplitStringGuid(@person_period_codes) pp
		on pp.id = p.person_period_code
	where p.person_code=@person_code
	and pp.id is null

END

GO



