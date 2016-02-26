IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday]
GO

-- =============================================
-- Author:		Jonas & Maria S
-- Create date: 2016-02-24
-- Description:	Load queue statistics for web intraday
-- =============================================
-- EXEC [mart].[web_intraday] 'FLE Standard Time', '2016-02-26', 'C5FFFC8F-BCD6-47F7-9352-9F0800E39578'
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

	SELECT @time_zone_id = time_zone_id FROM mart.dim_time_zone WHERE time_zone_code = @time_zone_code

	INSERT INTO #skills
		SELECT * FROM mart.SplitStringGuid(@skill_list)
		
	INSERT INTO #queues
		SELECT DISTINCT qw.queue_id 
		FROM mart.bridge_queue_workload qw
		INNER JOIN mart.dim_skill ds ON qw.skill_id = ds.skill_id
		INNER JOIN #skills s ON ds.skill_code = s.id
		
	SELECT 
		SUM(fq.offered_calls) AS OfferedCalls,
		MAX(i.interval_end) AS LatestStatsTime
	FROM
		mart.fact_queue fq
		INNER JOIN #queues q ON fq.queue_id = q.queue_id
		INNER JOIN mart.bridge_time_zone bz ON fq.date_id = bz.date_id AND fq.interval_id = bz.interval_id
		INNER JOIN mart.dim_date d ON bz.local_date_id = d.date_id
		INNER JOIN mart.dim_interval i ON bz.local_interval_id = i.interval_id
	WHERE
		bz.time_zone_id = @time_zone_id AND d.date_date = @today

END

GO

