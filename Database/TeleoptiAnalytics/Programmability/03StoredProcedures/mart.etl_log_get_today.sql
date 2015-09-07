IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_log_get_today]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_log_get_today]
GO


CREATE PROCEDURE [mart].[etl_log_get_today]
AS
BEGIN
	SET NOCOUNT ON;

    SELECT [schedule_id]
      ,max([job_start_time]) as job_start_time
      ,max([job_end_time]) as job_end_time
	FROM Mart.etl_job_execution
	WHERE
		[schedule_id] IS NOT NULL AND
		[job_start_time] IS NOT NULL AND
		[job_end_time] IS NOT NULL
	GROUP BY [schedule_id]
END


GO

