IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[web_intraday_latest_interval]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[web_intraday_latest_interval]
GO

-- =============================================
-- Author:		Jonas & Maria S
-- Create date: 2016-08-16
-- Description:	Get the latest queue stats interval_id for given day.
-- =============================================
-- EXEC [mart].[web_intraday_latest_interval] 'W. Europe Standard Time', '2013-03-05', 'F08D75B3-FDB4-484A-AE4C-9F0800E2F753'
-- EXEC [mart].[web_intraday_latest_interval] 'W. Europe Standard Time', '2016-04-13', 'F08D75B3-FDB4-484A-AE4C-9F0800E2F753,C5FFFC8F-BCD6-47F7-9352-9F0800E39578'
-- EXEC [mart].[web_intraday_latest_interval] 'Eastern Standard Time', '2018-11-26', '6E2F21D9-CEFF-45A3-974E-A7E7011F7E2B'
CREATE PROCEDURE [mart].[web_intraday_latest_interval]
@today smalldatetime,
@skill_list nvarchar(max)

AS
BEGIN
	SET NOCOUNT ON;
            
	DECLARE @default_scenario_id int
	DECLARE @bu_id int
	DECLARE @return_value int
	
	CREATE TABLE #skills(id uniqueidentifier)	
	CREATE TABLE #queues(queue_id int)	
	CREATE TABLE #queue_stats(date_id int, utc_interval_id smallint)

	CREATE TABLE #result(
	interval_id smallint,
	offered_calls decimal(19,0))

	INSERT INTO #skills
	SELECT * FROM mart.SplitStringGuid(@skill_list)

	SELECT @bu_id = business_unit_id FROM mart.dim_skill WHERE skill_code = (SELECT TOP 1 id FROM #skills)

	SELECT @default_scenario_id = scenario_id 
	FROM mart.dim_scenario 
	WHERE business_unit_id = @bu_id
		AND default_scenario = 1
                         
	INSERT INTO #queues
	SELECT DISTINCT qw.queue_id 
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
		date_id = fq.date_id,
		utc_interval_id = interval_id
	FROM 
		#queues q
		INNER JOIN mart.fact_queue fq ON q.queue_id = fq.queue_id
	WHERE
		date_id = @current_date_id
		AND (offered_calls > 0 OR overflow_in_calls > 0)

	SELECT
		@return_value = MAX(i.interval_id)
	FROM
		#queue_stats qs
		INNER JOIN mart.dim_date d ON qs.date_id = d.date_id
		INNER JOIN mart.dim_interval i ON qs.utc_interval_id = i.interval_id

	IF(@return_value IS NULL)
		SET @return_value = -1

	SELECT @return_value
END

GO

