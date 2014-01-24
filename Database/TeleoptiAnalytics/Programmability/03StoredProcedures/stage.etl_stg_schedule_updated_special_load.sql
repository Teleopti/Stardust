IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Stage].[etl_stg_schedule_updated_special_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stage].[etl_stg_schedule_updated_special_load]
GO


CREATE PROCEDURE [stage].[etl_stg_schedule_updated_special_load]
AS
BEGIN

SET NOCOUNT ON;
-- REM full procedure!

END

GO

