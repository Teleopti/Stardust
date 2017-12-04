IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday]
GO

-- =============================================
-- Author:		Jonas & Maria S
-- Create date: 2016-02-24
-- Description:	Load queue statistics and forecast data both by interval and by day. Used by web intraday.
-- =============================================
-- EXEC [mart].[web_intraday] 'W. Europe Standard Time', '2017-01-13', 'F08D75B3-FDB4-484A-AE4C-9F0800E2F753'
-- EXEC [mart].[web_intraday] 'W. Europe Standard Time', '2016-04-13', 'F08D75B3-FDB4-484A-AE4C-9F0800E2F753,C5FFFC8F-BCD6-47F7-9352-9F0800E39578'
-- EXEC mart.web_intraday 'GMT Standard Time', '2016-11-01', '32411FDD-5A63-45B2-98AE-A59B00B9CDF3'
CREATE PROCEDURE [mart].[web_intraday]
@time_zone_code nvarchar(100),
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
		date_id int,
		interval_id smallint,
		calculated_calls decimal(19,0), 
		answered_calls decimal(19,0), 
		answered_calls_within_SL decimal(19,0), 
		handle_time_s decimal(19,0),
		abandoned_calls decimal(19,0),
		speed_of_answer decimal (19,0)
	)

	CREATE TABLE #result(
		[date] smalldatetime,
		interval_id smallint,
		forecasted_calls decimal(28,4), 
		forecasted_handle_time_s decimal(28,4), 
		calculated_calls decimal(19,0), 
		answered_calls decimal(19,0),
		answered_calls_within_SL decimal(19,0),
		handle_time_s decimal(19,0),
		abandoned_calls decimal(19,0),
		speed_of_answer decimal(19,0)
	)

	SELECT @time_zone_id = time_zone_id 
	FROM mart.dim_time_zone WITH (NOLOCK)
	WHERE time_zone_code = @time_zone_code

	INSERT INTO #skills
	SELECT * FROM mart.SplitStringGuid(@skill_list)

	SELECT @bu_id = business_unit_id 
	FROM mart.dim_skill WITH (NOLOCK) 
	WHERE skill_code = (SELECT TOP 1 id FROM #skills)

	SELECT @default_scenario_id = scenario_id 
	FROM mart.dim_scenario WITH (NOLOCK)
	WHERE business_unit_id = @bu_id
		AND default_scenario = 1
                         
	INSERT INTO #queues
	SELECT DISTINCT 
		qw.queue_id,
		w.percentage_offered,
		percentage_overflow_in,
		percentage_overflow_out,
		percentage_abandoned,
		percentage_abandoned_short,
		percentage_abandoned_within_service_level,
		percentage_abandoned_after_service_level
	FROM 
		mart.bridge_queue_workload qw WITH (NOLOCK)
		INNER JOIN mart.dim_workload w WITH (NOLOCK) 
			ON qw.workload_id = w.workload_id
		INNER JOIN mart.dim_skill ds WITH (NOLOCK) 
			ON qw.skill_id = ds.skill_id
		INNER JOIN #skills s 
			ON ds.skill_code = s.id
	WHERE 
		w.is_deleted = 0

	-- Forecast
	INSERT INTO #result([date], interval_id, forecasted_calls, forecasted_handle_time_s)
	SELECT 
		d.date_date,
		i.interval_id,
		SUM(ISNULL(fw.forecasted_calls, 0)),
		SUM(ISNULL(fw.forecasted_handling_time_s, 0))
	FROM
		mart.fact_forecast_workload fw WITH (NOLOCK) 
		INNER JOIN mart.dim_skill ds WITH (NOLOCK) 
			ON fw.skill_id = ds.skill_id
		INNER JOIN #skills s 
			ON ds.skill_code = s.id
		INNER JOIN mart.bridge_time_zone bz WITH (NOLOCK) 
			ON fw.date_id = bz.date_id AND fw.interval_id = bz.interval_id
		INNER JOIN mart.dim_date d WITH (NOLOCK) 
			ON bz.local_date_id = d.date_id
		INNER JOIN mart.dim_interval i WITH (NOLOCK) 
			ON bz.local_interval_id = i.interval_id
		INNER JOIN mart.dim_workload wl WITH (NOLOCK)
			ON wl.workload_id = fw.workload_id
	WHERE
		fw.scenario_id = @default_scenario_id
		AND bz.time_zone_id = @time_zone_id
		AND d.date_date = @today
		AND wl.is_deleted = 0
	GROUP BY
		d.date_date,
		i.interval_id

	-- Prepare Queue stats
	DECLARE @current_date_id int
	SELECT @current_date_id = date_id FROM mart.dim_date WHERE date_date = @today

	INSERT INTO #queue_stats
	SELECT
		date_id,
		interval_id,
		mart.CalculateQueueStatistics(
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
			ISNULL(fq.overflow_in_calls, 0)) AS calculated_calls,
		answered_calls,
		answered_calls_within_SL,
		handle_time_s,
		abandoned_calls,
		speed_of_answer_s
	FROM 
		#queues q
		INNER JOIN mart.fact_queue fq 
			ON q.queue_id = fq.queue_id
	WHERE
		date_id between @current_date_id - 1 and @current_date_id + 1
	ORDER BY date_id,
		interval_id,
		fq.queue_id

		
	-- Queue stats - update result
	UPDATE r
	SET 
		calculated_calls = fq.calculated_calls,
		answered_calls = fq.answered_calls,
		answered_calls_within_SL = fq.answered_calls_within_SL,
		handle_time_s = fq.handle_time_s,
		abandoned_calls = fq.abandoned_calls,
		speed_of_answer = fq.speed_of_answer
	FROM 
			(SELECT 
				date_id,
				interval_id,
				SUM(ISNULL(calculated_calls, 0)) as calculated_calls,
				SUM(ISNULL(answered_calls, 0)) as answered_calls,
				SUM(ISNULL(answered_calls_within_SL, 0)) as answered_calls_within_SL,
				SUM(ISNULL(handle_time_s, 0)) as handle_time_s,
				SUM(ISNULL(abandoned_calls, 0)) as abandoned_calls,
				SUM(ISNULL(speed_of_answer, 0)) as speed_of_answer
			FROM 
				#queue_stats
			GROUP BY
				date_id,
				interval_id
		) AS fq
		INNER JOIN mart.bridge_time_zone bz WITH (NOLOCK) 
			ON fq.date_id = bz.date_id AND fq.interval_id = bz.interval_id
		INNER JOIN mart.dim_date d WITH (NOLOCK) 
			ON bz.local_date_id = d.date_id
		INNER JOIN mart.dim_interval i WITH (NOLOCK) 
			ON bz.local_interval_id = i.interval_id
		INNER JOIN #result r 
			ON i.interval_id = r.interval_id
	WHERE
		bz.time_zone_id = @time_zone_id 
		AND d.date_date = @today


	-- Return data by interval
	SELECT
		[date] AS IntervalDate,
		interval_id AS IntervalId,
		forecasted_calls AS ForecastedCalls,
		forecasted_handle_time_s AS ForecastedHandleTime,
		CASE ISNULL(forecasted_calls,0)
			WHEN 0 THEN 0
			ELSE 
				ISNULL(forecasted_handle_time_s,0) / ISNULL(forecasted_calls,0)
		END AS ForecastedAverageHandleTime,
		calculated_calls AS CalculatedCalls,
		handle_time_s AS HandleTime,
		CASE ISNULL(answered_calls,0)
			WHEN 0 THEN 0
			ELSE 
				ISNULL(handle_time_s,0) / ISNULL(answered_calls,0)
		END AS AverageHandleTime,
		answered_calls AS AnsweredCalls,
		answered_calls_within_SL AS AnsweredCallsWithinSL,
		CASE ISNULL(calculated_calls,0)
			WHEN 0 THEN 0
			ELSE 
				ISNULL(answered_calls_within_SL,0) / ISNULL(calculated_calls,0)
		END AS ServiceLevel,
		abandoned_calls AS AbandonedCalls,
		CASE ISNULL(calculated_calls,0)
			WHEN 0 THEN 0
			ELSE 
				ISNULL(abandoned_calls,0) / ISNULL(calculated_calls,0)
		END AS AbandonedRate,
		speed_of_answer AS SpeedOfAnswer,
		CASE ISNULL(answered_calls,0)
			WHEN 0 THEN 0
			ELSE 
				ISNULL(speed_of_answer,0) / ISNULL(answered_calls,0)
		END AS AverageSpeedOfAnswer

	FROM #result 
	ORDER BY interval_id
END



GO

