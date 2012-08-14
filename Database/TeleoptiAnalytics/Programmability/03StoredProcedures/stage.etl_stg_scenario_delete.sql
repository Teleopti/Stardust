IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Stage].[etl_stg_scenario_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stage].[etl_stg_scenario_delete]
GO


CREATE PROCEDURE [stage].[etl_stg_scenario_delete]
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;

    TRUNCATE TABLE Stage.stg_scenario
END


GO

