IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_job_execution_history]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_job_execution_history]
GO

-- =============================================
-- Author:		Jonas N
-- Create date: 2012-03-29
-- Description:	Return ETL Job history for the given period
-- =============================================
CREATE PROCEDURE [mart].[etl_job_execution_history]
	@start_date smalldatetime, 
	@end_date smalldatetime,
	@business_unit_id uniqueidentifier,
	@show_only_errors bit
AS
BEGIN
	SET NOCOUNT ON;
	
	SET @start_date = CONVERT(smalldatetime,CONVERT(nvarchar(30), @start_date, 112)) --ISO yyyymmdd
	SET @end_date	= CONVERT(smalldatetime,CONVERT(nvarchar(30), @end_date, 112))
	SET @end_date	= DATEADD(d, 1, @end_date)

    IF @show_only_errors = 1
    BEGIN

		SELECT
			je.job_execution_id,
			j.job_name,
			je.business_unit_name,
			je.job_start_time,
			je.job_end_time,
			je.duration_s AS job_duration_s,
			je.affected_rows AS job_affected_rows,
			je.schedule_id,
			sch.schedule_name,
			jse.jobstep_execution_id,
			js.jobstep_name,
			jse.duration_s AS jobstep_duration_s,
			jse.rows_affected AS jobstep_affected_rows,
			er.error_exception_message AS exception_msg,
			er.error_exception_stacktrace AS exception_trace,
			er.inner_error_exception_message AS inner_exception_msg,
			er.inner_error_exception_stacktrace AS inner_exception_trace
		FROM 
			mart.etl_job_execution je
		INNER JOIN
			mart.etl_job_schedule sch
		ON
			je.schedule_id = sch.schedule_id
		INNER JOIN
			mart.etl_jobstep_execution jse
		ON
			je.job_execution_id = jse.job_execution_id
		INNER JOIN
			mart.etl_job j
		ON
			je.job_id = j.job_id
		INNER JOIN
			mart.etl_jobstep js
		ON
			jse.jobstep_id = js.jobstep_id
		LEFT OUTER JOIN
			mart.etl_jobstep_error er
		ON
			jse.jobstep_error_id = er.jobstep_error_id	
		WHERE
			je.job_execution_id IN	(SELECT je.job_execution_id
									FROM
										mart.etl_job_execution je
									INNER JOIN 
										mart.etl_jobstep_execution jse
									ON
										je.job_execution_id = jse.job_execution_id
									LEFT OUTER JOIN
										mart.etl_jobstep_error er
									ON
										jse.jobstep_error_id = er.jobstep_error_id	
									WHERE
										(je.job_start_time >= @start_date)
										AND
										(je.job_end_time < @end_date)
										AND
										(je.business_unit_code = @business_unit_id 
										Or
										@business_unit_id = '00000000-0000-0000-0000-000000000002')
										AND 
										jse.jobstep_error_id IS NOT NULL)
		ORDER BY 
		job_execution_id desc,
		jobstep_execution_id
    END
    ELSE
    BEGIN

		SELECT 
			je.job_execution_id,
			j.job_name,
			je.business_unit_name,
			je.job_start_time,
			je.job_end_time,
			je.duration_s AS job_duration_s,
			je.affected_rows AS job_affected_rows,
			je.schedule_id,
			sch.schedule_name,
			jse.jobstep_execution_id,
			js.jobstep_name,
			jse.duration_s AS jobstep_duration_s,
			jse.rows_affected AS jobstep_affected_rows,
			er.error_exception_message AS exception_msg,
			er.error_exception_stacktrace AS exception_trace,
			er.inner_error_exception_message AS inner_exception_msg,
			er.inner_error_exception_stacktrace AS inner_exception_trace
		FROM 
			mart.etl_job_execution je
		INNER JOIN
			mart.etl_job_schedule sch
		ON
			je.schedule_id = sch.schedule_id
		INNER JOIN
			mart.etl_jobstep_execution jse
		ON
			je.job_execution_id = jse.job_execution_id
		INNER JOIN
			mart.etl_job j
		ON
			je.job_id = j.job_id
		INNER JOIN
			mart.etl_jobstep js
		ON
			jse.jobstep_id = js.jobstep_id
		LEFT OUTER JOIN
			mart.etl_jobstep_error er
		ON
			jse.jobstep_error_id = er.jobstep_error_id	
		WHERE
			(je.job_start_time >= @start_date)
			AND
			(je.job_end_time < @end_date)
			AND
			(je.business_unit_code = @business_unit_id 
			Or
			@business_unit_id = '00000000-0000-0000-0000-000000000002')
		ORDER BY 
			je.job_execution_id desc,
			jse.jobstep_execution_id
    END

END

GO