IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Stage].[etl_stg_schedule_day_absence_count_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Stage].[etl_stg_schedule_day_absence_count_delete]
GO


CREATE PROCEDURE [stage].[etl_stg_schedule_day_absence_count_delete]
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;

    TRUNCATE TABLE Stage.stg_schedule_day_absence_count
END

GO

