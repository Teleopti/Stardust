IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday_email_backlog_per_workload]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday_email_backlog_per_workload]
GO

-- =============================================
-- Author:		Jonas & Bharath M
-- Create date: 2017-06-02
-- Description:	Get the email backlog for a given period
-- =============================================
-- EXEC [mart].[web_intraday_email_backlog_per_workload] 'A7092015-60D2-4D35-83BF-9F0801136943', '2017-06-02', '1900-01-01 15:00', '2017-06-05', '1900-01-01 06:00'
CREATE PROCEDURE [mart].[web_intraday_email_backlog_per_workload]
@workload_id uniqueidentifier,
@start_date smalldatetime,
@start_time smalldatetime,
@end_date smalldatetime,
@end_time smalldatetime

AS
BEGIN
	SET NOCOUNT ON;
    
	DECLARE @default_scenario_id int
	DECLARE @bu_id int
	
	CREATE TABLE #queues(
		skill_code uniqueidentifier,
		workload_code uniqueidentifier,
		queue_id int,
		percentage_offered float,
		percentage_overflow_in float,
		percentage_overflow_out float,
		percentage_abandoned float,
		percentage_abandoned_short float,
		percentage_abandoned_within_service_level float,
		percentage_abandoned_after_service_level float
		)
	
	SELECT @bu_id = business_unit_id FROM mart.dim_workload WHERE workload_code = @workload_id

	SELECT @default_scenario_id = scenario_id 
	FROM mart.dim_scenario 
	WHERE business_unit_id = @bu_id
		AND default_scenario = 1
                         
	INSERT INTO #queues
	SELECT 
		ds.skill_code,
		w.workload_code,
		qw.queue_id,
		w.percentage_offered,
		percentage_overflow_in,
		percentage_overflow_out,
		percentage_abandoned,
		percentage_abandoned_short,
		percentage_abandoned_within_service_level,
		percentage_abandoned_after_service_level 
	FROM mart.bridge_queue_workload qw
	INNER JOIN mart.dim_workload w ON qw.workload_id = w.workload_id
	INNER JOIN mart.dim_skill ds ON qw.skill_id = ds.skill_id
	WHERE w.is_deleted = 0 AND w.workload_code = @workload_id
	
	SELECT
		ISNULL(SUM(mart.CalculateQueueStatistics(
			q.percentage_offered,
			q.percentage_overflow_in,
			q.percentage_overflow_out,
			q.percentage_abandoned,
			q.percentage_abandoned_short,
			q.percentage_abandoned_within_service_level,
			q.percentage_abandoned_after_service_level,
			ISNULL(fq.offered_calls, 0),
			ISNULL(fq.abandoned_calls, 0),
			ISNULL(fq.abandoned_calls_within_SL, 0),
			ISNULL(fq.abandoned_short_calls, 0),
			ISNULL(fq.overflow_out_calls, 0),
			ISNULL(fq.overflow_in_calls, 0))), 0) AS Emails
	FROM 
		#queues q
		INNER JOIN mart.fact_queue fq ON q.queue_id = fq.queue_id
		INNER JOIN mart.dim_date d ON fq.date_id = d.date_id
		INNER JOIN mart.dim_interval i ON fq.interval_id = i.interval_id
	WHERE
			(
				(
					(
						(@start_date <> @end_date)
						AND
						(
							(d.date_date = @start_date AND i.interval_start >= @start_time)
							OR
							(d.date_date = @end_date AND i.interval_end <= @end_time)
						)
					)
					OR
					(
						(@start_date = @end_date)
						AND
						(d.date_date = @start_date AND (i.interval_start >= @start_time AND i.interval_end <= @end_time))
					)
				)
				OR
				(d.date_date > @start_date AND d.date_date < @end_date)
			)
		AND 
			(
				(offered_calls IS NOT NULL AND offered_calls > 0)
				OR
				(overflow_in_calls IS NOT NULL AND overflow_in_calls > 0)
			)
END

GO

