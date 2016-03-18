IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_delete_all]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_delete_all]
GO

-- =============================================
-- Description:	Delete all people in specific group pages
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_delete_all]
@group_page_codes nvarchar(max)

AS
BEGIN
	delete bgpp from [mart].[bridge_group_page_person] bgpp
	join [mart].[dim_group_page] p on p.group_page_id = bgpp.group_page_id
	where p.group_page_code in (select * from mart.SplitStringGuid(@group_page_codes))
END

GO



