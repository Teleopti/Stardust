IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Mart].[etl_job_get_schedule_periods]') AND type in (N'P', N'PC'))
DROP PROCEDURE [Mart].[etl_job_get_schedule_periods]
GO


-- =============================================
-- Author:		JN
-- Create date: 2008-10-15
-- Description:	Get job schedule periods
-- =============================================
CREATE PROCEDURE [mart].[etl_job_get_schedule_periods]
@schedule_id int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		p.schedule_id
		,p.job_id
		,j.job_name
		,p.relative_period_start
		,p.relative_period_end
	FROM 
		Mart.etl_job_schedule_period p
	INNER JOIN
		Mart.etl_job j
	ON
		p.job_id = j.job_id
	WHERE schedule_id = @schedule_id
END


GO

