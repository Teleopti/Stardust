IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_job_schedule_toggle_enable_state]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Mart].[etl_job_schedule_toggle_enable_state]
GO

CREATE PROCEDURE [mart].[etl_job_schedule_toggle_enable_state]
@schedule_id int
AS
BEGIN
SET NOCOUNT ON

UPDATE mart.etl_job_schedule
SET enabled = enabled ^ 1
WHERE schedule_id = @schedule_id

END
GO
