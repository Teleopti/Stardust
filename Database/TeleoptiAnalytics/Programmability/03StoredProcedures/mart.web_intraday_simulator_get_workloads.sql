IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday_simulator_get_workloads]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday_simulator_get_workloads]
GO

-- =============================================
-- Author:		Jonas
-- Create date: 2016-03-31
-- Description:	Load workloads and its queues. Used by web intraday queue stats simulator.
-- =============================================
-- EXEC mart.web_intraday_simulator_get_workloads '2016-04-01', 'W. Europe Standard Time'
CREATE PROCEDURE [mart].[web_intraday_simulator_get_workloads]
	@today smalldatetime,
	@time_zone_code nvarchar(100)
AS
BEGIN
	DECLARE @time_zone_id as int

	CREATE TABLE #result(
		WorkloadId int,
		QueueId int,
		HasQueueStats bit,
		DatasourceId int
	)
	CREATE TABLE #queues(
		QueueId int,
	)

	SELECT @time_zone_id = time_zone_id FROM mart.dim_time_zone WHERE time_zone_code = @time_zone_code

	INSERT INTO #result (WorkloadId, QueueId, DatasourceId, HasQueueStats)
		SELECT 
			qw.workload_id, 
			qw.queue_id, 
			q.datasource_id,
			0
		FROM mart.bridge_queue_workload qw
			INNER JOIN mart.dim_queue q ON qw.queue_id = q.queue_id
		WHERE qw.workload_id <> -1

	INSERT INTO #queues
		SELECT DISTINCT QueueId from #result
	
	UPDATE #result
	SET HasQueueStats = 1
	FROM #queues q
		INNER JOIN #result r ON q.QueueId = r.QueueId
		INNER JOIN mart.fact_queue fq ON q.QueueId = fq.queue_id
		INNER JOIN mart.bridge_time_zone bz ON fq.date_id = bz.date_id AND fq.interval_id = bz.interval_id
		INNER JOIN mart.dim_date d ON bz.local_date_id = d.date_id
		INNER JOIN mart.dim_interval i ON bz.local_interval_id = i.interval_id
	WHERE d.date_date = @today
		AND bz.time_zone_id = @time_zone_id

	SELECT * FROM #result
	ORDER BY WorkloadId, QueueId
END

GO

