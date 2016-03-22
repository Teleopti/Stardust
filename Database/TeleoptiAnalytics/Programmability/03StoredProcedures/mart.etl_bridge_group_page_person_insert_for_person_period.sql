IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_insert_for_person_period]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_insert_for_person_period]
GO

-- =============================================
-- Description:	Insert bridge group page person entries for a specific person period
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_insert_for_person_period]
@person_period_code uniqueidentifier,
@group_codes nvarchar(max)

AS
BEGIN

    declare @person_id int = (select person_id from [mart].[dim_person] where person_period_code = @person_period_code)

	insert into [mart].[bridge_group_page_person]
	select gp.group_page_id, @person_id, 1, GETUTCDATE() 
	from mart.SplitStringGuid(@group_codes) join [mart].dim_group_page gp on gp.group_code = id
END

GO



