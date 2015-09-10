IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_queue_load_intraday]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_queue_load_intraday]
GO

--exec mart.etl_fact_queue_load_intraday @datasource_id=-2

CREATE PROCEDURE [mart].[etl_fact_queue_load_intraday]
@datasource_id int,
@is_delayed_job bit = 0

AS
SET NOCOUNT ON
DECLARE @sqlstring nvarchar(4000)
SET @sqlstring = ''
DECLARE @detail_id int
SET @detail_id = 1 --Queue data

--Execute one delayed jobs, if any
if (@datasource_id=-2 AND @is_delayed_job=0) --called from ETLo
	EXEC mart.etl_execute_delayed_job @stored_procedure='mart.etl_fact_queue_load'

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
	AND dsd.detail_id=@detail_id
	
	OPEN DataSouceCursor

	FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	WHILE @@FETCH_STATUS = 0
	BEGIN
			EXEC [mart].[etl_fact_queue_load_intraday] @datasource_id
			FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	END
	CLOSE DataSouceCursor
	DEALLOCATE DataSouceCursor
END

ELSE  --Single datasource_id
	BEGIN
	--declare
	CREATE TABLE #bridge_time_zone(date_id int,interval_id int,time_zone_id int,local_date_id int,local_interval_id int)
	CREATE TABLE #agg_queue_ids (queue_agg_id int, mart_queue_id int)

	DECLARE @UtcNow as smalldatetime
	DECLARE @time_zone_id smallint
	DECLARE @internal bit

	DECLARE @start_date_id	INT
	DECLARE @end_date_id	INT

	declare @source_date_id_utc int
	declare @source_interval_id_utc int
	declare @source_date_local smalldatetime
	declare @source_interval_local int
	
	declare @target_date_id_utc int
	declare @target_interval_id_utc smallint
	declare @target_date_local smalldatetime
	declare @target_date_local_id int
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
	AND detail_id = @detail_id

	--if any "go back number of intervals"
	SELECT
		@target_date_local		= date_from,
		@target_interval_local	= interval_id
	FROM [mart].[SubtractInterval](@target_date_local,@target_interval_local,@intervals_back)

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

	SELECT
		@target_date_id_utc = b.date_id,
		@target_interval_id_utc = b.interval_id
	FROM  mart.dim_date d
	INNER JOIN mart.bridge_time_zone b
		ON d.date_id = b.local_date_id
		AND b.time_zone_id=@time_zone_id
		AND b.local_interval_id=@target_interval_local
	WHERE d.date_date=@target_date_local

	--if missing intervals in bridge_time_zone, probably DST hour
	IF @target_date_id_utc IS NULL OR @target_interval_id_utc IS NULL
	BEGIN
		--try and go back some more
		SELECT
		@target_date_local		= date_from,
		@target_interval_local	= interval_id
		FROM [mart].[SubtractInterval](dateadd(dd,-1,@target_date_local),@target_interval_local,@intervals_back)
		
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

--If Agg is way ahead of Mart limit fetch to 5 days.
	IF @source_date_id_utc>@target_date_id_utc
	BEGIN
		IF (@source_date_id_utc-@target_date_id_utc > 5)
		BEGIN
			SELECT 'Agg is way ahead of Mart limit fetch to 5 days'
			SET @source_date_local = DATEADD(DAY,5,@target_date_local)
		END
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

	SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @target_date_local = date_date)
	SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @source_date_local = date_date)

	INSERT INTO #agg_queue_ids
	SELECT
		queue_agg_id,
		queue_id
	FROM mart.dim_queue
	WHERE datasource_id = @datasource_id

	--prepare dates and intervals for this time_zone
	--note: Get date and intervals grouped so that we do not get duplicates at DST clock shifts
	INSERT #bridge_time_zone(date_id,time_zone_id,local_date_id,local_interval_id)
	SELECT	min(date_id),	time_zone_id, 	local_date_id,	local_interval_id
	FROM mart.bridge_time_zone 
	WHERE time_zone_id	= @time_zone_id	
	AND local_date_id BETWEEN @start_date_id AND @end_date_id
	GROUP BY time_zone_id, local_date_id,local_interval_id
	OPTION(RECOMPILE)

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

	DELETE #bridge_time_zone
	WHERE date_id < @target_date_id_utc
	and interval_id < @target_interval_id_utc

	-------------
	-- Delete rows last known date_id and interval_id
	-------------
	SET NOCOUNT OFF
	IF @source_date_id_utc>@target_date_id_utc
	BEGIN
		--MIDDLE DATES
		DELETE f
		FROM mart.fact_queue f
		WHERE f.date_id between @target_date_id_utc + 1 AND @source_date_id_utc-1
		AND EXISTS (select 1 from #agg_queue_ids q where q.mart_queue_id = f.queue_id)
		OPTION(RECOMPILE)

		--maxdate
		DELETE f
		FROM mart.fact_queue f
		WHERE EXISTS (select 1 from #agg_queue_ids q where q.mart_queue_id = f.queue_id)
		AND f.date_id=@source_date_id_utc
		AND f.interval_id <= @source_interval_id_utc
	 END

	--TODAY
	DELETE f
	FROM mart.fact_queue f
	WHERE EXISTS (select 1 from #agg_queue_ids q where q.mart_queue_id = f.queue_id)
	AND f.date_id=@target_date_id_utc
	AND f.interval_id >= @target_interval_id_utc

	

	--	AND f.date_id>@target_date_id_utc

	-------------
	-- Insert rows
	-------------
	--perpare
	SELECT @sqlstring = 'INSERT INTO mart.fact_queue
		(
		date_id, 
		interval_id, 
		queue_id, 
		offered_calls, 
		answered_calls, 
		answered_calls_within_SL, 
		abandoned_calls, 
		abandoned_calls_within_SL, 
		abandoned_short_calls, 
		overflow_out_calls,
		overflow_in_calls,
		talk_time_s, 
		after_call_work_s, 
		handle_time_s, 
		speed_of_answer_s, 
		time_to_abandon_s, 
		longest_delay_in_queue_answered_s,
		longest_delay_in_queue_abandoned_s,
		datasource_id, 
		insert_date
		)
		SELECT
		date_id						= bridge.date_id, 
		interval_id					= bridge.interval_id, 
		queue_id					= q.mart_queue_id, 
		offered_calls				= ISNULL(stg.offd_direct_call_cnt,0), 
		answered_calls				= ISNULL(stg.answ_call_cnt,0), 
		answered_calls_within_SL	= ISNULL(stg.ans_servicelevel_cnt,0), 
		abandoned_calls				= ISNULL(stg.aband_call_cnt,0), 
		abandoned_calls_within_SL	= ISNULL(stg.aband_within_sl_cnt,0), 
		abandoned_short_calls		= ISNULL(stg.aband_short_call_cnt,0), 
		overflow_out_calls			= ISNULL(stg.overflow_out_call_cnt,0),
		overflow_in_calls			= ISNULL(stg.overflow_in_call_cnt,0), 
		talk_time_s					= ISNULL(stg.talking_call_dur,0), 
		after_call_work_s			= ISNULL(stg.wrap_up_dur,0), 
		handle_time_s				= ISNULL(stg.talking_call_dur,0)+ISNULL(wrap_up_dur,0), 
		speed_of_answer_s			= ISNULL(stg.queued_and_answ_call_dur,0), 
		time_to_abandon_s			= ISNULL(stg.queued_and_aband_call_dur,0), 
		longest_delay_in_queue_answered_s = ISNULL(stg.queued_answ_longest_que_dur,0),
		longest_delay_in_queue_abandoned_s = ISNULL(stg.queued_aband_longest_que_dur,0),
		datasource_id				= ' + cast(@datasource_id as varchar(3)) + ', 
		insert_date					= getdate()
		FROM
		(
		SELECT
			agg.*
			FROM '+ 
		CASE @internal
			WHEN 0 THEN '	mart.v_queue_logg agg'
			WHEN 1 THEN '	dbo.queue_logg agg'
			ELSE NULL --Fail fast
		END
		+ '
		WHERE (date_from = '''+ CAST(@target_date_local as nvarchar(20))+'''
		AND interval >= '''+ CAST(@target_interval_local as nvarchar(20))+'''
		) OR (date_from BETWEEN '''+ CAST(DATEADD(D,1,@target_date_local) as nvarchar(20))+'''
		 AND '''+ CAST(@source_date_local as nvarchar(20))+'''
		)
		AND EXISTS (SELECT 1 FROM #agg_queue_ids tmp WHERE agg.queue=tmp.queue_agg_id)
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
	 #bridge_time_zone bridge
	ON
		d.date_id		= bridge.local_date_id		AND
		stg.interval	= bridge.local_interval_id'

	--Exec
	EXEC sp_executesql @sqlstring
	SET NOCOUNT ON

	drop table #bridge_time_zone

	--finally
	--update with last logged interval
	update [mart].[etl_job_intraday_settings]
	set
		target_date		= @source_date_local,
		target_interval	= @source_interval_local
	WHERE datasource_id = @datasource_id
	AND detail_id = @detail_id
END
GO