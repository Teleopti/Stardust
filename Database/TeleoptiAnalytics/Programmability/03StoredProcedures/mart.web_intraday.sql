IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday]
GO

-- =============================================
-- Author:		Jonas & Maria S
-- Create date: 2016-02-24
-- Description:	Load queue statistics for web intraday
-- =============================================
-- EXEC [mart].[web_intraday] 'FLE Standard Time', '2016-03-01', 'C5FFFC8F-BCD6-47F7-9352-9F0800E39578'
CREATE PROCEDURE [mart].[web_intraday]
@time_zone_code nvarchar(100),
@today smalldatetime,
@skill_list nvarchar(max)

AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @time_zone_id as int

	CREATE TABLE #skills(id uniqueidentifier)
	CREATE TABLE #queues(queue_id int)
	CREATE TABLE #result(
		forecasted_calls decimal(28,4), 
		offered_calls decimal(19,0), 
		latest_stats_time smalldatetime)

	SELECT @time_zone_id = time_zone_id FROM mart.dim_time_zone WHERE time_zone_code = @time_zone_code

	INSERT INTO #skills
		SELECT * FROM mart.SplitStringGuid(@skill_list)
		
	INSERT INTO #queues
		SELECT DISTINCT qw.queue_id 
		FROM mart.bridge_queue_workload qw
		INNER JOIN mart.dim_skill ds ON qw.skill_id = ds.skill_id
		INNER JOIN #skills s ON ds.skill_code = s.id

	INSERT INTO #result(offered_calls, latest_stats_time)
		SELECT 
			SUM(ISNULL(fq.offered_calls, 0)), 
			MAX(i.interval_end)
		FROM
			mart.fact_queue fq
			INNER JOIN #queues q ON fq.queue_id = q.queue_id
			INNER JOIN mart.bridge_time_zone bz ON fq.date_id = bz.date_id AND fq.interval_id = bz.interval_id
			INNER JOIN mart.dim_date d ON bz.local_date_id = d.date_id
			INNER JOIN mart.dim_interval i ON bz.local_interval_id = i.interval_id
		WHERE
			bz.time_zone_id = @time_zone_id AND d.date_date = @today

		
	INSERT INTO #result(forecasted_calls)
		SELECT 
			SUM(ISNULL(fw.forecasted_calls, 0)) AS ForecastedCalls
		FROM
			mart.fact_forecast_workload fw
			INNER JOIN mart.dim_skill ds ON fw.skill_id = ds.skill_id
			INNER JOIN #skills s ON ds.skill_code = s.id
			INNER JOIN mart.bridge_time_zone bz ON fw.date_id = bz.date_id AND fw.interval_id = bz.interval_id
			INNER JOIN mart.dim_date d ON bz.local_date_id = d.date_id
			INNER JOIN mart.dim_interval i ON bz.local_interval_id = i.interval_id
		WHERE
			d.date_date = @today 
			AND i.interval_end <= (SELECT latest_stats_time FROM #result) 
			 
			 
	SELECT 
		SUM(ISNULL(forecasted_calls,0)) AS ForecastedCalls,
		SUM(ISNULL(offered_calls,0)) AS OfferedCalls,
		MAX(latest_stats_time) AS LatestStatsTime,
		CASE SUM(ISNULL(forecasted_calls,0))
			WHEN 0
			THEN -99
			ELSE
				ABS(SUM(ISNULL(offered_calls,0)) - SUM(ISNULL(forecasted_calls,0))) / SUM(ISNULL(forecasted_calls,0)) * 100
		END AS ForecastedActualCallsDiff
	FROM #result
END

GO




