IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday_simulator_get_workloads]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday_simulator_get_workloads]
GO

-- =============================================
-- Author:		Jonas
-- Create date: 2016-03-31
-- Description:	Load workloads and its queues. Used by web intraday queue stats simulator.
-- =============================================
-- EXEC mart.web_intraday_simulator_get_workloads
CREATE PROCEDURE [mart].[web_intraday_simulator_get_workloads]
AS 
BEGIN
	DECLARE @time_zone_id as int

	CREATE TABLE #result(
		WorkloadId int,
		WorkloadCode uniqueidentifier,
		QueueId int,
		QueueName nvarchar(100),
		DatasourceId int,
		SkillName nvarchar(100)
	)
	CREATE TABLE #queues(
		QueueId int,
	)

	INSERT INTO #result (WorkloadId, WorkloadCode, QueueId, QueueName, DatasourceId, SkillName)
		SELECT
			qw.workload_id, 
			w.workload_code,
			qw.queue_id, 
			q.queue_name,
			q.datasource_id,
			s.skill_name
		FROM 
			mart.bridge_queue_workload qw
			INNER JOIN mart.dim_queue q ON qw.queue_id = q.queue_id
			INNER JOIN mart.dim_workload w ON qw.workload_id = w.workload_id
			INNER JOIN mart.dim_skill s ON qw.skill_id = s.skill_id
		WHERE 
			qw.workload_id <> -1

	INSERT INTO #queues
		SELECT DISTINCT QueueId from #result
	
	SELECT
		WorkloadId, 
		WorkloadCode,
		QueueId, 
		QueueName,
		DatasourceId, 
		SkillName
	FROM 
		#result
	ORDER BY 
		WorkloadId, 
		QueueId, 
		DatasourceId
END

GO

