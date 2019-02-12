IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_agent_queue_load_intraday]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_agent_queue_load_intraday]
GO

-- =============================================
-- Author:		Karin
-- Create date: 2014-10-24
-- Description:	Loads fact_agent_queue from agent_logg in the intraday job.
-- =============================================
--EXEC mart.etl_fact_agent_queue_load_intraday @datasource_id=-2
CREATE PROCEDURE [mart].[etl_fact_agent_queue_load_intraday] 
@datasource_id int,
@is_delayed_job bit = 0
AS
SET NOCOUNT ON

DECLARE @sqlstring nvarchar(4000)
SET @sqlstring = ''
DECLARE @detail_id int
DECLARE @mart_detail_id int
SET @detail_id = 2 --Agent Data in agg
SET @mart_detail_id = 3 --Agent Queue Data in mart


--Execute one delayed jobs, if any
if (@datasource_id=-2 AND @is_delayed_job=0) --called from ETL
	EXEC mart.etl_execute_delayed_job @stored_procedure='mart.etl_fact_agent_queue_load'

IF (@datasource_id=-1)--no available datasource
BEGIN
	SELECT 'No datasource available'
	RETURN 0
END
--make sure datasource exists in mart.etl_intraday_settings
IF NOT EXISTS(select * from [mart].[etl_job_intraday_settings] where detail_id=@detail_id and datasource_id=@datasource_id) AND @datasource_id<>-2
BEGIN	
	SELECT 'No datasource available'
	RETURN 0
END
--------------------------------------------------------------------------
--If we get All = -2 loop existing log objects and call this SP in a cursor for each log object
--------------------------------------------------------------------------
IF @datasource_id = -2 --All
BEGIN
	DECLARE DataSouceCursor CURSOR FOR
	SELECT ds.datasource_id
	FROM mart.sys_datasource ds
	INNER JOIN mart.etl_job_intraday_settings dsd
		ON ds.datasource_id = dsd.datasource_id
	WHERE ds.datasource_id NOT IN (-1,1)
	AND ds.time_zone_id IS NOT NULL
	AND ds.inactive = 0 
	AND dsd.detail_id=@mart_detail_id
	
	OPEN DataSouceCursor

	FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC [mart].[etl_fact_agent_queue_load_intraday] @datasource_id
		FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	END
	CLOSE DataSouceCursor
	DEALLOCATE DataSouceCursor
END
ELSE
BEGIN  --Single datasource_id
	
	--declare
	CREATE TABLE #bridge_time_zone(date_id int,interval_id int,time_zone_id int,local_date_id int,local_interval_id int)
	CREATE TABLE #agg_queue_ids (queue_agg_id int, queue_id int)
	CREATE TABLE #agg_acdlogin_ids (acd_login_agg_id int, acd_login_id int)

	DECLARE @UtcNow as smalldatetime
	DECLARE @time_zone_id smallint
	DECLARE @internal bit

	DECLARE @start_date_id	INT
	DECLARE @end_date_id	INT

	declare @source_date_id_utc int
	declare @source_date_id_utc_minus1 int
	declare @source_interval_id_utc int
	declare @source_date_local smalldatetime
	declare @source_interval_local int
	
	declare @target_date_id_utc int
	declare @target_date_id_utc_plus1 int
	declare @target_interval_id_utc smallint
	declare @target_date_local smalldatetime
	declare @target_interval_local smallint
	declare @intervals_back smallint

	--init
	SELECT @UtcNow=CAST(getutcdate() as smalldatetime)
	SELECT 
		@time_zone_id = ds.time_zone_id,
		@internal	  = ds.internal
	FROM
		Mart.sys_datasource ds
	WHERE 
		ds.datasource_id= @datasource_id

	SELECT
		@target_date_local=target_date,
		@target_interval_local=target_interval,
		@intervals_back=intervals_back
	FROM [mart].[etl_job_intraday_settings] ds WITH (TABLOCKX) --Block any other process from even reading this data. Wait until ETL is done processing!
	WHERE datasource_id = @datasource_id
	AND detail_id = @mart_detail_id

	--if any "go back number of intervals"
	SELECT
		@target_date_local		= date_from,
		@target_interval_local	= interval_id
	FROM [mart].[SubtractInterval](@target_date_local,@target_interval_local,@intervals_back)
	
	/*Optimize query against agg*/
	INSERT INTO #agg_queue_ids
	SELECT
		queue_agg_id,
		queue_id
	FROM mart.dim_queue 
	WHERE datasource_id = @datasource_id

	/*Optimize query against agg*/
	INSERT INTO #agg_acdlogin_ids
	SELECT
		acd_login_agg_id,
		acd_login_id
	FROM mart.dim_acd_login 
	WHERE datasource_id = @datasource_id


	if (select @internal) = 0
		select
			@source_date_id_utc		= b.date_id,
			@source_interval_id_utc	= b.interval_id,
			@source_date_local		= DATEDIFF(DD, 0, lod.date_value),
			@source_interval_local	= lod.int_value
		from mart.v_log_object_detail lod
		inner join mart.dim_date d
			on DATEDIFF(DD, 0, lod.date_value) = d.date_date
		inner join mart.sys_datasource ds
			on ds.log_object_id = lod.log_object_id
		inner join mart.bridge_time_zone b
			on b.time_zone_id = ds.time_zone_id
			and d.date_id = b.local_date_id
			and lod.int_value = b.local_interval_id
		WHERE lod.detail_id		= @detail_id
		AND ds.datasource_id	= @datasource_id
	else
		select
			@source_date_id_utc		= b.date_id,
			@source_interval_id_utc	= b.interval_id,
			@source_date_local		= DATEDIFF(DD, 0, lod.date_value),
			@source_interval_local	= lod.int_value
		from dbo.log_object_detail lod --Azure or a mix of seprate and internal Agg-tables
		inner join mart.dim_date d
			on DATEDIFF(DD, 0, lod.date_value) = d.date_date
		inner join mart.sys_datasource ds
			on ds.log_object_id = lod.log_object_id
		inner join mart.bridge_time_zone b
			on b.time_zone_id = ds.time_zone_id
			and d.date_id = b.local_date_id
			and lod.int_value = b.local_interval_id
		WHERE lod.detail_id		= @detail_id
		AND ds.datasource_id	= @datasource_id

	--Set date in utc
	SELECT
		@target_date_id_utc = bt.date_id,
		@target_interval_id_utc = bt.interval_id
	FROM  mart.dim_date d
	INNER JOIN --in case duplicate du to DST->standard time
	(SELECT date_id,local_date_id,local_interval_id,interval_id= MIN(interval_id)
	FROM mart.bridge_time_zone
	WHERE time_zone_id=@time_zone_id
	GROUP BY date_id,local_date_id,local_interval_id)bt 
	ON d.date_id = bt.local_date_id
	AND bt.local_interval_id=@target_interval_local
	WHERE d.date_date=@target_date_local
	
	--if missing intervals in bridge_time_zone, probably standard time->DST hour
	IF @target_date_id_utc IS NULL OR @target_interval_id_utc IS NULL
	BEGIN
		--try and go back some more
		SELECT
		@target_date_local		= date_from,
		@target_interval_local	= interval_id
		FROM [mart].[SubtractInterval](dateadd(dd,-1,@target_date_local),@target_interval_local,0)
		
		--and try set again
		SELECT
		@target_date_id_utc = b.date_id,
		@target_interval_id_utc = b.interval_id
		FROM  mart.dim_date d
		INNER JOIN mart.bridge_time_zone b
			ON d.date_id = b.local_date_id
			AND b.time_zone_id=@time_zone_id
			AND b.local_interval_id=@target_interval_local
		WHERE d.date_date=@target_date_local
	END
	
	--If Mart is ahead of Agg, bail out
	IF (@target_date_id_utc-@source_date_id_utc>0
		AND
		@target_interval_id_utc-@source_interval_id_utc>0
		)
	BEGIN
		SELECT 'Mart is ahead of Agg, bail out'
		RETURN 0
	END

	--If Dates and Intervals are the same in Agg <-> Mart do nothing
	IF (@source_date_id_utc-@target_date_id_utc=0
		AND
		@source_interval_id_utc-@target_interval_id_utc=0)
	BEGIN
		SELECT 'Dates and Intervals are the same in Agg <-> Mart do nothing'
		RETURN 0
	END

	
	IF @source_date_id_utc>@target_date_id_utc
	BEGIN
		--If Agg is way ahead of Mart limit fetch to 5 days.
		IF (@source_date_id_utc-@target_date_id_utc > 5)
		BEGIN
			SELECT 'Agg is way ahead of Mart limit fetch to 5 days'
			SET @source_date_local = DATEADD(DAY,5,@target_date_local)
			SET @source_interval_local = (select max(interval_id) from mart.dim_interval)

			SELECT	@source_date_id_utc		= b.date_id,
					@source_interval_id_utc	= b.interval_id
			FROM  mart.dim_date d
			INNER JOIN mart.bridge_time_zone b
			ON d.date_id = b.local_date_id
			AND  b.local_interval_id = @source_interval_local
			AND b.time_zone_id = @time_zone_id
			AND d.date_date = @source_date_local
		END
		ELSE
		BEGIN
			--reset @source_interval_id_utc only for delete
			--make sure complete day is deleted BUT keep max source interval if detail_setting date<> max date in agg table to avoid duplicate key error
			SELECT	@source_date_id_utc		= b.date_id,
					@source_interval_id_utc	= b.interval_id
			FROM  mart.dim_date d
			INNER JOIN mart.bridge_time_zone b
			ON d.date_id = b.local_date_id
			AND  b.local_interval_id = (select max(interval_id) from mart.dim_interval)
			AND b.time_zone_id = @time_zone_id
			AND d.date_date = @source_date_local
		END
	END
	--select @target_date_id_utc, @source_date_id_utc, @target_date_local,@source_date_local, @source_interval_id_utc, @source_interval_local
	
	SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @target_date_local = date_date)
	SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @source_date_local = date_date)


	--prepare INSERT dates and intervals for this time_zone
	--note: Get date and intervals grouped so that we do not get duplicates at DST clock shifts
	INSERT #bridge_time_zone(date_id,time_zone_id,local_date_id,local_interval_id)
	SELECT	min(date_id),	time_zone_id, 	local_date_id,	local_interval_id
	FROM mart.bridge_time_zone WITH(NOLOCK)
	WHERE time_zone_id	= @time_zone_id	
	AND local_date_id BETWEEN @start_date_id AND @end_date_id
	GROUP BY time_zone_id, local_date_id,local_interval_id
	OPTION (RECOMPILE)

	UPDATE temp
	SET interval_id= bt.interval_id
	FROM 
	(SELECT date_id,local_date_id,local_interval_id,interval_id= MIN(interval_id)
	FROM mart.bridge_time_zone WITH(NOLOCK)
	WHERE time_zone_id=@time_zone_id
	GROUP BY date_id,local_date_id,local_interval_id)bt
	INNER JOIN #bridge_time_zone temp
		ON temp.local_interval_id=bt.local_interval_id
		AND temp.date_id=bt.date_id
		AND temp.local_date_id=bt.local_date_id

	--Remove intervals outside scope
	DELETE #bridge_time_zone
	WHERE (date_id < @target_date_id_utc)
	OR (date_id > @source_date_id_utc)

	DELETE  #bridge_time_zone
	WHERE (date_id=@target_date_id_utc AND interval_id < @target_interval_id_utc)
	OR (date_id=@source_date_id_utc	AND interval_id > @source_interval_id_utc)
	-------------
	-- Delete rows last known date_id and interval_id
	-------------
	
	SET NOCOUNT OFF

	IF @source_date_id_utc>@target_date_id_utc
	BEGIN
		SELECT @target_date_id_utc_plus1 = date_id
			FROM mart.dim_date WHERE date_date = (SELECT DATEADD(d, 1, date_date) FROM mart.dim_date WHERE date_id = @target_date_id_utc)
		
		SELECT @source_date_id_utc_minus1 = date_id
			FROM mart.dim_date WHERE date_date = (SELECT DATEADD(d, -1, date_date) FROM mart.dim_date WHERE date_id = @source_date_id_utc)

		--middle dates
		DELETE f
		FROM mart.fact_agent_queue f
		WHERE f.date_id between @target_date_id_utc_plus1 AND @source_date_id_utc_minus1
		AND f.datasource_id = @datasource_id
		OPTION (RECOMPILE)
	
		--maxdate
		DELETE f
		FROM mart.fact_agent_queue f
		WHERE f.date_id=@source_date_id_utc
		AND f.interval_id <= @source_interval_id_utc
		AND f.datasource_id = @datasource_id
	END
	
	--today
	DELETE f
	FROM mart.fact_agent_queue f
	WHERE f.date_id=@target_date_id_utc
	AND f.interval_id >= @target_interval_id_utc
	AND f.datasource_id = @datasource_id
	
	
	--------------
	-- Insert rows
	--------------
	-- Same agent_id can occur on several intervals bound to different queues. 
	-- Important is to get the distinct(min/max) value.
	
	--perpare
	SELECT @sqlstring = 'INSERT INTO mart.fact_agent_queue
		(
		date_id, 
		interval_id, 
		queue_id, 
		acd_login_id, 
		talk_time_s, 
		after_call_work_time_s, 
		answered_calls, 
		transfered_calls, 
		datasource_id, 
		insert_date
		)
	SELECT
		date_id					= bridge.date_id, 
		interval_id				= bridge.interval_id, 
		queue_id				= q.queue_id, 
		acd_login_id			= a.acd_login_id, 
		talk_time_s				= talking_call_dur, 
		after_call_work_time_s	= wrap_up_dur, 
		answered_calls			= answ_call_cnt, 
		transfered_calls		= transfer_out_call_cnt,
		datasource_id			= ' + cast(@datasource_id as varchar(3)) + ',
		insert_date				= getdate()
		FROM
		(SELECT * FROM '+ 
		CASE @internal
			WHEN 0 THEN '	mart.v_agent_logg agg'
			WHEN 1 THEN '	dbo.agent_logg agg'
			ELSE NULL --Fail fast
		END
		+ '
		WHERE (date_from = '''+ CAST(@target_date_local as nvarchar(20))+'''
		AND interval >= '''+ CAST(@target_interval_local as nvarchar(20))+'''
		) OR (date_from BETWEEN '''+ CAST(DATEADD(D,1,@target_date_local) as nvarchar(20))+'''
		 AND '''+ CAST(@source_date_local as nvarchar(20))+'''
		)
		) stg
	INNER JOIN
		mart.dim_date		d
	ON
		stg.date_from	= d.date_date
	INNER JOIN
		#agg_queue_ids q
		ON
			q.queue_agg_id = stg.queue
	INNER JOIN
		#agg_acdlogin_ids a
	ON 
		a.acd_login_agg_id = stg.agent_id
	INNER JOIN
		#bridge_time_zone bridge
		ON
			d.date_id		= bridge.local_date_id		AND
			stg.interval	= bridge.local_interval_id 
	 '

	--Exec
	EXEC sp_executesql @sqlstring
	SET NOCOUNT ON
	
	--finally
	--update with last logged interval
	UPDATE [mart].[etl_job_intraday_settings]
	SET
		target_date		= @source_date_local,
		target_interval	= @source_interval_local
	WHERE datasource_id = @datasource_id
	AND detail_id = @mart_detail_id

END

GO


