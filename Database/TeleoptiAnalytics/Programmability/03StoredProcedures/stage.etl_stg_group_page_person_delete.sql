IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[etl_stg_group_page_person_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [stage].[etl_stg_group_page_person_delete]
GO


CREATE PROCEDURE [stage].[etl_stg_group_page_person_delete]
AS
BEGIN
	SET NOCOUNT ON;

    TRUNCATE TABLE stage.stg_group_page_person
END

GO


