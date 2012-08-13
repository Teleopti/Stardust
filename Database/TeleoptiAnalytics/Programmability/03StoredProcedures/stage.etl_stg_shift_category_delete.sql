IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Stage].[etl_stg_shift_category_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stage].[etl_stg_shift_category_delete]
GO


CREATE PROCEDURE [stage].[etl_stg_shift_category_delete]
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;

    TRUNCATE TABLE Stage.stg_shift_category
END


GO

