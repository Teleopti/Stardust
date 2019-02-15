IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday_calls_per_skill_interval]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday_calls_per_skill_interval]
GO

-- =============================================
-- Author:		Jonas & Maria S & Bharath M
-- Create date: 2016-09-05
-- Description:	Get the offered calls & aht per skill for the given day. 
-- =============================================
-- EXEC [mart].[web_intraday_calls_per_skill_interval] 'Eastern Standard Time', '2018-11-26', '6E2F21D9-CEFF-45A3-974E-A7E7011F7E2B'
CREATE PROCEDURE [mart].[web_intraday_calls_per_skill_interval]
@today smalldatetime,
@skill_list nvarchar(max)

AS
BEGIN
	SET NOCOUNT ON;
            
	DECLARE @time_zone_id as int
	DECLARE @default_scenario_id int
	DECLARE @bu_id int
	
	CREATE TABLE #skills(id uniqueidentifier)	
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
	CREATE TABLE #queue_stats(
		skill_code uniqueidentifier,
		workload_code uniqueidentifier,
		date_id int, 
		utc_interval_id smallint,
		calculated_calls int,
		answered_calls int,
		handle_time decimal(19,0)
	)

	INSERT INTO #skills
	SELECT * FROM mart.SplitStringGuid(@skill_list)

	SELECT @bu_id = business_unit_id FROM mart.dim_skill WHERE skill_code = (SELECT TOP 1 id FROM #skills)

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
	INNER JOIN #skills s ON ds.skill_code = s.id
	WHERE w.is_deleted = 0
	
	-- Prepare Queue stats
	DECLARE @current_date_id int
	SELECT @current_date_id = date_id FROM mart.dim_date WHERE date_date = @today

	INSERT INTO #queue_stats
	SELECT 
		skill_code = q.skill_code,
		workload_code = q.workload_code,
		date_id = fq.date_id,
		utc_interval_id = interval_id,
		calculated_calls = mart.CalculateQueueStatistics(
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
			ISNULL(fq.overflow_in_calls, 0)),
		answered_calls =  ISNULL(fq.answered_calls, 0),
		handle_time = ISNULL(fq.handle_time_s, 0)			
	FROM 
		#queues q
		INNER JOIN mart.fact_queue fq ON q.queue_id = fq.queue_id
	WHERE
		date_id = @current_date_id
		AND 
			(
				(offered_calls IS NOT NULL AND offered_calls > 0)
				OR
				(overflow_in_calls IS NOT NULL AND overflow_in_calls > 0)
			)

	SELECT
		qs.skill_code AS SkillId,
		qs.workload_code AS WorkloadId,
		DATEADD(mi, DATEDIFF(MINUTE, DATEADD(DAY, DATEDIFF(DAY, 0, i.interval_start), 0), i.interval_start), d.date_date) AS StartTime,
		SUM(qs.calculated_calls) AS Calls,
		CASE SUM(qs.answered_calls)
				WHEN 
					0 THEN 0
				ELSE 
					SUM(qs.handle_time) / SUM(qs.answered_calls)
			END AS AverageHandleTime,
		SUM(qs.answered_calls) AS AnsweredCalls,
		SUM(qs.handle_time) AS HandleTime
	FROM
		#queue_stats qs
		INNER JOIN mart.dim_date d ON qs.date_id = d.date_id
		INNER JOIN mart.dim_interval i ON qs.utc_interval_id = i.interval_id
	GROUP BY
		qs.skill_code,
		qs.workload_code,
		d.date_date,
		i.interval_start
	ORDER BY
		qs.skill_code,
		d.date_date,
		i.interval_start
END

GO

