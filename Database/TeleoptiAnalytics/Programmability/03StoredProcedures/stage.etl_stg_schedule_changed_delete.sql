IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Stage].[stg_schedule_changed_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stage].[stg_schedule_changed_delete]
GO


CREATE PROCEDURE [stage].[stg_schedule_changed_delete]
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;

    TRUNCATE TABLE Stage.stg_schedule_changed
	--TRUNCATE TABLE Stage.stg_schedule_updated_personLocal
	--TRUNCATE TABLE Stage.stg_schedule_updated_ShiftStartDateUTC
END


GO

