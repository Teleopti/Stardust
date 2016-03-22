IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_delete_removed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_delete_removed]
GO

-- =============================================
-- Description:	Delete all group mappings for a specific person where the person period code is not in the parameter list
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_delete_removed]
@person_code uniqueidentifier,
@person_period_codes nvarchar(max)
AS
BEGIN
	delete bgpp from [mart].[bridge_group_page_person] bgpp
	where bgpp.person_id in (
		select person_id from [mart].[dim_person] 
		where person_code = @person_code 
		and person_period_code not in (
			select id from mart.SplitStringGuid(@person_period_codes)))
END

GO



