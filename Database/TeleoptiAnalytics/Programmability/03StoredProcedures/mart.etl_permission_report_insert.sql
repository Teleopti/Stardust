IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_bridge_group_page_person_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_bridge_group_page_person_insert]
GO

-- =============================================
-- Description:	Insert bridge group page person
-- =============================================
CREATE PROCEDURE [mart].[etl_bridge_group_page_person_insert]
@person_codes nvarchar(max),
@group_page_code uniqueidentifier

AS
BEGIN

  declare @group_page_id int = (select group_page_id from [mart].[dim_group_page] where group_code = @group_page_code)
	insert into [mart].[bridge_group_page_person]
	select @group_page_id, p.person_id, 1, GETUTCDATE() 
	from mart.SplitStringGuid(@person_codes) join [mart].dim_person p on p.person_code = id
END

GO



