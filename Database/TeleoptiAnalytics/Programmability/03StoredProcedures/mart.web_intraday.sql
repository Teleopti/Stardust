IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday]
GO

-- =============================================
-- Author:		Jonas & Maria S
-- Create date: 2016-02-24
-- Description:	Load queue statistics and forecast data both by interval and by day. Used by web intraday.
-- =============================================
-- EXEC [mart].[web_intraday] 'W. Europe Standard Time', '2016-04-13', 'F08D75B3-FDB4-484A-AE4C-9F0800E2F753'
-- EXEC [mart].[web_intraday] 'W. Europe Standard Time', '2016-04-13', 'F08D75B3-FDB4-484A-AE4C-9F0800E2F753,C5FFFC8F-BCD6-47F7-9352-9F0800E39578'
CREATE PROCEDURE [mart].[web_intraday]
@time_zone_code nvarchar(100),
@today smalldatetime,
@skill_list nvarchar(max)

AS
BEGIN
	SET NOCOUNT ON;
            
	DECLARE @time_zone_id as int
	DECLARE @default_scenario_id int
	
	SELECT @default_scenario_id = scenario_id FROM mart.dim_scenario WHERE default_scenario = 1

	CREATE TABLE #skills(id uniqueidentifier)
	CREATE TABLE #queues(queue_id int)
	CREATE TABLE #result(
	interval_id smallint,
	forecasted_calls decimal(28,4), 
	forecasted_handle_time_s decimal(28,4), 
	offered_calls decimal(19,0), 
	handle_time_s decimal(19,0))

	SELECT @time_zone_id = time_zone_id FROM mart.dim_time_zone WHERE time_zone_code = @time_zone_code

	INSERT INTO #skills
	SELECT * FROM mart.SplitStringGuid(@skill_list)
                         
	INSERT INTO #queues
	SELECT DISTINCT qw.queue_id 
	FROM mart.bridge_queue_workload qw
	INNER JOIN mart.dim_skill ds ON qw.skill_id = ds.skill_id
	INNER JOIN #skills s ON ds.skill_code = s.id

	-- Forecast
	INSERT INTO #result(interval_id, forecasted_calls, forecasted_handle_time_s)
	SELECT 
		i.interval_id,
		SUM(ISNULL(fw.forecasted_calls, 0)),
		SUM(ISNULL(fw.forecasted_handling_time_s, 0))
	FROM
		mart.fact_forecast_workload fw
		INNER JOIN mart.dim_skill ds ON fw.skill_id = ds.skill_id
		INNER JOIN #skills s ON ds.skill_code = s.id
		INNER JOIN mart.bridge_time_zone bz ON fw.date_id = bz.date_id AND fw.interval_id = bz.interval_id
		INNER JOIN mart.dim_date d ON bz.local_date_id = d.date_id
		INNER JOIN mart.dim_interval i ON bz.local_interval_id = i.interval_id
	WHERE
		fw.scenario_id = @default_scenario_id
		AND bz.time_zone_id = @time_zone_id
		AND d.date_date = @today
	GROUP BY
		i.interval_id


	-- Queue stats - update result
	UPDATE r
	SET 
		offered_calls = fq.offered_calls,
		handle_time_s = fq.handle_time_s
	FROM 
		mart.fact_queue fq
		INNER JOIN #queues q ON fq.queue_id = q.queue_id
		INNER JOIN mart.bridge_time_zone bz ON fq.date_id = bz.date_id AND fq.interval_id = bz.interval_id
		INNER JOIN mart.dim_date d ON bz.local_date_id = d.date_id
		INNER JOIN mart.dim_interval i ON bz.local_interval_id = i.interval_id
		INNER JOIN #result r ON i.interval_id = r.interval_id
	WHERE
		bz.time_zone_id = @time_zone_id 
		AND d.date_date = @today


	-- Return data by interval
	SELECT
		interval_id AS IntervalId,
		forecasted_calls AS ForecastedCalls,
		forecasted_handle_time_s AS ForecastedHandleTime,
		CASE ISNULL(forecasted_calls,0)
			WHEN 0 THEN 0
			ELSE 
				ISNULL(forecasted_handle_time_s,0) / ISNULL(forecasted_calls,0)
		END AS ForecastedAverageHandleTime,
		offered_calls AS OfferedCalls,
		handle_time_s AS HandleTime,
		CASE ISNULL(offered_calls,0)
			WHEN 0 THEN 0
			ELSE 
				ISNULL(handle_time_s,0) / ISNULL(offered_calls,0)
		END AS AverageHandleTime
	FROM #result 
	ORDER BY interval_id
END

GO

