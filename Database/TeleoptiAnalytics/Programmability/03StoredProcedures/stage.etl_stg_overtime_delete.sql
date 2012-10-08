IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[stage].[etl_stg_overtime_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [stage].[etl_stg_overtime_delete]
GO

CREATE PROCEDURE [stage].[etl_stg_overtime_delete]
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;

    TRUNCATE TABLE Stage.stg_overtime
END
GO