IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_agent_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_agent_load]
GO


-- =============================================
-- Author:		KJ
-- Create date: 2008-04-09
-- Description:	Loads fact_agent from agent_logg. 
-- Agent values not bound to queue.
-- Update date: 2009-02-11
-- 2009-02-11 New mart schema KJ
-- 2009-02-09 Stage moved to mart db, removed view KJ
-- 2009-05-25 Changed from MIN to MAX on values from agent_logg KJ
-- 2011-01-29
-- 2011-07-20 Dual Agg DJ
-- =============================================
--EXEC [mart].[etl_fact_agent_load] '2013-03-04','2013-03-04', -2
--EXEC [mart].[etl_fact_agent_load] '2018-12-26','2018-12-27', 2
CREATE PROCEDURE [mart].[etl_fact_agent_load] 
@start_date smalldatetime,
@end_date smalldatetime,
@datasource_id int,
@is_delayed_job bit = 0	
AS
SET NOCOUNT ON
DECLARE @internal bit
DECLARE @sqlstring nvarchar(4000)
SET @sqlstring = ''

--Execute one delayed jobs, if any
if (@datasource_id=-2 AND @is_delayed_job=0) --called from ETL
	EXEC mart.etl_execute_delayed_job @stored_procedure='mart.etl_fact_agent_load'

--------------------------------------------------------------------------
--If we get All = -2 loop existing log objects and call this SP in a cursor for each log object
--------------------------------------------------------------------------
IF @datasource_id = -2 --All
BEGIN
	DECLARE DataSouceCursor CURSOR FOR
	SELECT datasource_id FROM mart.sys_datasource WHERE datasource_id NOT IN (-1,1) AND time_zone_id IS NOT NULL AND inactive = 0 
	OPEN DataSouceCursor

	FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC [mart].[etl_fact_agent_load] @start_date, @end_date, @datasource_id
		FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	END
	CLOSE DataSouceCursor
	DEALLOCATE DataSouceCursor
END
ELSE
BEGIN
	--is this an internal or external datasource?
	SELECT @internal = internal FROM mart.sys_datasource WHERE datasource_id = @datasource_id
	
	--declare
	CREATE TABLE #bridge_time_zone(date_id int,interval_id int,time_zone_id int,local_date_id int,local_interval_id int)
	CREATE NONCLUSTERED INDEX [#IX_bridge_timezone]ON #bridge_time_zone ([local_date_id],[local_interval_id])INCLUDE ([date_id],[interval_id])
	CREATE TABLE #agg_acdlogin_ids (acd_login_agg_id int, mart_acd_login_id int)

	--Create system mindate
	DECLARE @UtcNow as smalldatetime
	SELECT @UtcNow=CAST(getutcdate() as smalldatetime)
	----------------------------------------------------------------------------------
	DECLARE @start_date_id	INT
	DECLARE @end_date_id	INT

	DECLARE @max_date smalldatetime
	DECLARE @min_date smalldatetime

	DECLARE @time_zone_id smallint
	DECLARE @from_date_id_utc int
	DECLARE @from_interval_id_utc smallint
	DECLARE @to_date_id_utc int
	DECLARE @to_interval_id_utc smallint
	DECLARE @from_date_id_utc_plus1 int
	DECLARE @to_date_id_utc_minus1 int

	SELECT 
		@time_zone_id = ds.time_zone_id
	FROM
		Mart.sys_datasource ds
	WHERE 
		ds.datasource_id= @datasource_id

	SET	@start_date = convert(smalldatetime,floor(convert(decimal(18,4),@start_date )))
	SET @end_date	= convert(smalldatetime,floor(convert(decimal(18,4),@end_date )))

	SET @start_date_id	=	(SELECT date_id FROM mart.dim_date WHERE @start_date = date_date)
	SET @end_date_id	=	(SELECT date_id FROM mart.dim_date WHERE @end_date = date_date)

	/*Optimize query against agg*/
	INSERT INTO #agg_acdlogin_ids
	SELECT
		acd_login_agg_id,
		acd_login_id
	FROM mart.dim_acd_login
	WHERE datasource_id = @datasource_id


	--prepare dates and intervals for this time_zone
	--note: Get date and intervals grouped so that we do not get duplicates at DST clock shifts
	INSERT #bridge_time_zone(date_id,time_zone_id,local_date_id,local_interval_id)
	SELECT	min(date_id),	time_zone_id, 	local_date_id,	local_interval_id
	FROM mart.bridge_time_zone 
	WHERE time_zone_id	= @time_zone_id	
	AND local_date_id BETWEEN @start_date_id AND @end_date_id
	GROUP BY time_zone_id, local_date_id,local_interval_id

	UPDATE temp
	SET interval_id= bt.interval_id
	FROM 
	(SELECT date_id,local_date_id,local_interval_id,interval_id= MIN(interval_id)
	FROM mart.bridge_time_zone
	WHERE time_zone_id=@time_zone_id
	GROUP BY date_id,local_date_id,local_interval_id)bt
	INNER JOIN #bridge_time_zone temp
		ON temp.local_interval_id=bt.local_interval_id
		AND temp.date_id=bt.date_id
		AND temp.local_date_id=bt.local_date_id
	--------------
	-- Delete rows
	--------------
	SELECT TOP 1
		@from_date_id_utc = date_id,
		@from_interval_id_utc = interval_id
	FROM #bridge_time_zone
	WHERE local_date_id = @start_date_id
	ORDER BY date_id ASC,interval_id ASC

	SELECT TOP 1
		@to_date_id_utc = date_id,
		@to_interval_id_utc = interval_id
	FROM #bridge_time_zone
	WHERE local_date_id = @end_date_id
	ORDER BY date_id DESC,interval_id DESC
	
	SELECT @from_date_id_utc_plus1 = date_id
		FROM mart.dim_date WHERE date_date = (SELECT DATEADD(d, 1, date_date) FROM mart.dim_date WHERE date_id = @from_date_id_utc)
		
	SELECT @to_date_id_utc_minus1 = date_id
		FROM mart.dim_date WHERE date_date = (SELECT DATEADD(d, -1, date_date) FROM mart.dim_date WHERE date_id = @to_date_id_utc)


	SET NOCOUNT OFF

	DELETE  f
    FROM mart.fact_agent f 
    WHERE EXISTS (select 1 from #agg_acdlogin_ids q where q.mart_acd_login_id = f.acd_login_id)
    AND date_id between @from_date_id_utc_plus1 AND @to_date_id_utc_minus1

    --first date_id 
    DELETE  f
	FROM mart.fact_agent f 
    WHERE EXISTS (select 1 from #agg_acdlogin_ids q where q.mart_acd_login_id = f.acd_login_id)
    AND date_id = @from_date_id_utc
    AND interval_id >= @from_interval_id_utc

    --last date_id
    DELETE  f
    FROM mart.fact_agent f 
    WHERE EXISTS (select 1 from #agg_acdlogin_ids q where q.mart_acd_login_id = f.acd_login_id)
    AND date_id = @to_date_id_utc
    AND interval_id <= @to_interval_id_utc
	--------------
	-- Insert rows
	--------------
	-- Same agent_id can occur on several intervals bound to different queues. 
	-- Important is to get the distinct(min/max) value.
	
	--prepare
	SELECT @sqlstring = 'INSERT INTO mart.fact_agent
		(
		date_id, 
		interval_id, 
		acd_login_id, 
		ready_time_s,
		logged_in_time_s,
		not_ready_time_s,
		idle_time_s,
		direct_outbound_calls,
		direct_outbound_talk_time_s,
		direct_incoming_calls,
		direct_incoming_calls_talk_time_s,
		admin_time_s,
		datasource_id, 
		insert_date
		)
	SELECT
		date_id						= MIN(bridge.date_id), 
		interval_id					= MIN(bridge.interval_id), 
		acd_login_id				= a.mart_acd_login_id, 
		ready_time_s				= MAX(avail_dur),
		logged_in_time_s			= MAX(tot_work_dur),
		not_ready_time_s			= MAX(pause_dur),
		idle_time_s					= MAX(wait_dur),
		direct_outbound_calls		= SUM(direct_out_call_cnt),
		direct_outbound_talk_time_s = SUM(direct_out_call_dur),
		direct_incoming_calls		= SUM(direct_in_call_cnt),
		direct_incoming_calls_talk_time_s = SUM(direct_in_call_dur),
		admin_time_s				= MAX(admin_dur),
		datasource_id				= ' + cast(@datasource_id as varchar(3)) + ', 
		insert_date					= getdate()
	FROM '
	+ 
	CASE @internal
		WHEN 0 THEN '	mart.v_agent_logg stg'
		WHEN 1 THEN '	dbo.agent_logg stg'
		ELSE NULL --Fail fast
	END
	+ 
	' INNER JOIN
		#agg_acdlogin_ids a
		ON a.acd_login_agg_id = stg.agent_id
	INNER JOIN
		mart.dim_date		d
	ON
		stg.date_from	= d.date_date
	INNER JOIN 
		#bridge_time_zone bridge 
	ON
		d.date_id		= bridge.local_date_id		AND
		stg.interval	= bridge.local_interval_id
	WHERE bridge.time_zone_id = '+CAST(@time_zone_id as nvarchar(10))+'
	AND date_from between '''+ CAST(@start_date as nvarchar(20))+''' and '''+ CAST(@end_date as nvarchar(20))+'''
	AND avail_dur IS NOT NULL
	AND tot_work_dur IS NOT NULL
	GROUP BY 
		a.mart_acd_login_id,
		d.date_id,
		stg.interval'

	EXEC sp_executesql @sqlstring

	SET NOCOUNT ON

	DROP TABLE #bridge_time_zone
END

GO


