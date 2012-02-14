IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_job_get_schedules]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Mart].[etl_job_get_schedules]
GO


-- =============================================
-- Author:		JN
-- Create date: 2008-06-03
-- Description:	Get job schedules
-- =============================================
CREATE PROCEDURE [mart].[etl_job_get_schedules]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [schedule_id]
		,[schedule_name]
		,[enabled]
		,[schedule_type]
		,[occurs_daily_at]
		,[occurs_every_minute]
		,[recurring_starttime]
		,[recurring_endtime]
		,[etl_job_name]
		,[etl_relative_period_start]
		,[etl_relative_period_end]
		,[etl_datasource_id]
		,[description]
	FROM Mart.[etl_job_schedule]
	WHERE schedule_id > -1
END


GO

