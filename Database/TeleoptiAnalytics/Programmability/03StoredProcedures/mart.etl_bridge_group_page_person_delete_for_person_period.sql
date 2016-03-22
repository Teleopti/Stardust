IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_delete_for_person_period]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_delete_for_person_period]
GO

-- =============================================
-- Description:	Delete all group mappings for a specific person_period
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_delete_for_person_period]
@person_period_code uniqueidentifier,
@group_codes nvarchar(max)
AS
BEGIN
	declare @person_id int = (select person_id from [mart].[dim_person] where person_period_code = @person_period_code)

	delete bgpp from [mart].[bridge_group_page_person] bgpp
	where bgpp.person_id = @person_id
	and bgpp.group_page_id in (select group_page_id from mart.SplitStringGuid(@group_codes) join [mart].dim_group_page gp on gp.group_code = id)
END

GO



