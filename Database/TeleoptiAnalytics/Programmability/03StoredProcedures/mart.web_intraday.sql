IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday]
GO

-- =============================================
-- Author:		Jonas & Maria S
-- Create date: 2016-02-24
-- Changed by: Daniel S : 2018-06-26
-- Description:	Load queue statistics and forecast data both by interval and by day. Used by web intraday.
-- =============================================
-- EXEC [mart].[web_intraday] 'W. Europe Standard Time', '2017-01-13', 'F08D75B3-FDB4-484A-AE4C-9F0800E2F753'
-- EXEC [mart].[web_intraday] 'W. Europe Standard Time', '2016-04-13', 'F08D75B3-FDB4-484A-AE4C-9F0800E2F753,C5FFFC8F-BCD6-47F7-9352-9F0800E39578'
-- EXEC mart.web_intraday 'GMT Standard Time', '2018-11-27', '6E2F21D9-CEFF-45A3-974E-A7E7011F7E2B'
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
	
	CREATE TABLE #queue_stats(
		date_id int,
		interval_id smallint,
		skill_id int,
		calculated_calls decimal(19,0), 
		answered_calls decimal(19,0), 
		answered_calls_within_SL decimal(19,0), 
		handle_time_s decimal(19,0),
		abandoned_calls decimal(19,0),
		speed_of_answer decimal (19,0)
	)

	CREATE TABLE #forecast(
		[date] smalldatetime,
		interval_id smallint,
		skill_id int,
		forecasted_calls decimal(28,4), 
		forecasted_handle_time_s decimal(28,4), 
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
                  
	-- Prepare Queue stats
	DECLARE @current_date_id int,
	@current_date_id_minus1 int,
	@current_date_id_plus1 int

	SELECT @current_date_id = date_id FROM mart.dim_date WHERE date_date = @today
	SELECT @current_date_id_minus1 = date_id
		FROM mart.dim_date WHERE date_date = (SELECT DATEADD(d, -1, date_date) FROM mart.dim_date WHERE date_id = @current_date_id)
	SELECT @current_date_id_plus1 = date_id
		FROM mart.dim_date WHERE date_date = (SELECT DATEADD(d, 1, date_date) FROM mart.dim_date WHERE date_id = @current_date_id)

	-- Queue incoming statistics
	INSERT INTO #queue_stats
	SELECT DISTINCT 
		fq.date_id,
		fq.interval_id,
		ds.skill_id,
		SUM(ISNULL(mart.CalculateQueueStatistics(
			w.percentage_offered,
			w.percentage_overflow_in,
			w.percentage_overflow_out,
			w.percentage_abandoned,
			w.percentage_abandoned_short,
			w.percentage_abandoned_within_service_level,
			w.percentage_abandoned_after_service_level,
			ISNULL(fq.offered_calls, 0),
			ISNULL(fq.abandoned_calls, 0),
			ISNULL(fq.abandoned_calls_within_SL, 0),
			ISNULL(fq.abandoned_short_calls, 0),
			ISNULL(fq.overflow_out_calls, 0),
			ISNULL(fq.overflow_in_calls, 0)), 0)) as calculated_calls,
		SUM(ISNULL(answered_calls, 0)) as answered_calls,
		SUM(ISNULL(answered_calls_within_SL, 0)) as answered_calls_within_SL,
		SUM(ISNULL(handle_time_s, 0)) as handle_time_s,
		SUM(ISNULL(abandoned_calls, 0)) as abandoned_calls,
		SUM(ISNULL(speed_of_answer_s, 0)) as speed_of_answer
	FROM 
		mart.bridge_queue_workload qw WITH (NOLOCK)
		INNER JOIN mart.fact_queue fq ON qw.queue_id = fq.queue_id
		INNER JOIN mart.dim_workload w WITH (NOLOCK) ON qw.workload_id = w.workload_id
		INNER JOIN mart.dim_skill ds WITH (NOLOCK) ON qw.skill_id = ds.skill_id
		INNER JOIN #skills s ON ds.skill_code = s.id
	WHERE 
		fq.date_id between @current_date_id_minus1 and @current_date_id_plus1 AND
		w.is_deleted = 0
	GROUP BY
 		date_id,
		interval_id,
		ds.skill_id

	-- Forecast
	INSERT INTO #forecast([date], interval_id, skill_id, forecasted_calls, forecasted_handle_time_s)
	SELECT 
		d.date_date,
		i.interval_id,
		ds.skill_id,
		SUM(ISNULL(fw.forecasted_calls, 0)),
		SUM(ISNULL(fw.forecasted_handling_time_s, 0))
	FROM
		mart.fact_forecast_workload fw WITH (NOLOCK) 
		INNER JOIN mart.dim_skill ds WITH (NOLOCK) ON fw.skill_id = ds.skill_id
		INNER JOIN #skills s ON ds.skill_code = s.id
		INNER JOIN mart.bridge_time_zone bz WITH (NOLOCK) ON fw.date_id = bz.date_id AND fw.interval_id = bz.interval_id
		INNER JOIN mart.dim_date d WITH (NOLOCK) ON bz.local_date_id = d.date_id
		INNER JOIN mart.dim_interval i WITH (NOLOCK) ON bz.local_interval_id = i.interval_id
		INNER JOIN mart.dim_workload wl WITH (NOLOCK) ON wl.workload_id = fw.workload_id
	WHERE
		fw.scenario_id = @default_scenario_id
		AND bz.time_zone_id = @time_zone_id
		AND d.date_date = @today
		AND wl.is_deleted = 0
	GROUP BY
		d.date_date,
		i.interval_id,
		ds.skill_id

	-- Merge forecast and incoming
	INSERT INTO #result
	SELECT forecast.[date],
		forecast.interval_id,
		forecast.forecasted_calls, 
		forecast.forecasted_handle_time_s, 
		incoming.calculated_calls, 
		incoming.answered_calls,
		incoming.answered_calls_within_SL,
		incoming.handle_time_s,
		incoming.abandoned_calls,
		incoming.speed_of_answer 
	FROM
		(SELECT 
			f.[date],
			f.interval_id,
			SUM(ISNULL(f.forecasted_calls, 0)) AS forecasted_calls,
			SUM(ISNULL(f.forecasted_handle_time_s, 0)) AS forecasted_handle_time_s
		FROM
			#forecast f
		GROUP BY 
			[date],
			interval_id) AS forecast
	LEFT JOIN 	
		(SELECT 
			f.[date],
			f.interval_id,
			SUM(ISNULL(fq.calculated_calls, 0)) as calculated_calls,
			SUM(ISNULL(answered_calls, 0)) as answered_calls,
			SUM(ISNULL(answered_calls_within_SL, 0)) as answered_calls_within_SL,
			SUM(ISNULL(handle_time_s, 0)) as handle_time_s,
			SUM(ISNULL(abandoned_calls, 0)) as abandoned_calls,
			SUM(ISNULL(speed_of_answer, 0)) as speed_of_answer
		FROM 
			#queue_stats AS fq
			INNER JOIN mart.bridge_time_zone bz WITH (NOLOCK) ON fq.date_id = bz.date_id AND fq.interval_id = bz.interval_id
			INNER JOIN mart.dim_date d WITH (NOLOCK) ON bz.local_date_id = d.date_id
			INNER JOIN mart.dim_interval i WITH (NOLOCK) ON bz.local_interval_id = i.interval_id
			INNER JOIN #forecast f ON i.interval_id = f.interval_id AND f.skill_id = fq.skill_id
		WHERE
			bz.time_zone_id = @time_zone_id 
			AND d.date_date = @today
		GROUP BY 
			f.[date],
			f.interval_id
		) AS incoming ON incoming.[date] = forecast.[date] AND incoming.[interval_id] = forecast.[interval_id]
	
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

