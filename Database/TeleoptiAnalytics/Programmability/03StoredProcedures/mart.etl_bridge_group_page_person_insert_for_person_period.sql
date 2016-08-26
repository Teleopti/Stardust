IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_insert_for_person_period]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_insert_for_person_period]
GO

-- =============================================
-- Description:	Insert bridge group page person entries for a specific person period
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_insert_for_person_period]
@person_period_code uniqueidentifier,
@group_codes nvarchar(max),
@business_unit_code uniqueidentifier

AS
BEGIN

	insert into [mart].[bridge_group_page_person]
	select gp.group_page_id, p.person_id, 1, GETUTCDATE() 
	from mart.SplitStringGuid(@group_codes) 
	join [mart].dim_group_page gp 
		on gp.group_code = id AND gp.business_unit_code = @business_unit_code
	join [mart].[dim_person] p
		on p.person_period_code = @person_period_code AND p.business_unit_code = @business_unit_code
END

GO



