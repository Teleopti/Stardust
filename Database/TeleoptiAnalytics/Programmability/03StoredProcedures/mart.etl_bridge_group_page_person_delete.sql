IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_delete]
GO

-- =============================================
-- Description:	Delete all people in specific group pages
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_delete]
@person_codes nvarchar(max),
@group_page_code uniqueidentifier
AS
BEGIN
	declare @group_page_id int = (select group_page_id from [mart].[dim_group_page] where group_code = @group_page_code)

	delete bgpp from [mart].[bridge_group_page_person] bgpp
	where bgpp.group_page_id = @group_page_id
	and bgpp.person_id in (select person_id from mart.SplitStringGuid(@person_codes) join [mart].dim_person p on p.person_code = id)
END

GO



