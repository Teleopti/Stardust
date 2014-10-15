IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_fact_queue_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_fact_queue_load]
GO


-- =============================================
-- Author:		ChLu
-- Create date: 2008-02-18
-- Update date: 2009-02-11
--				2009-02-11 New mart schema KJ
--				2009-02-09 Stage moved to mart db, removed view KJ
--				2009-01-26 Added isnull check on values from agg KJ
--				2008-08-27 Removed column Transferred Calls and added columns overflow_out_calls,overflow_in_calls KJ
--				2008-05-26 Added columns longest_delay_in_queue_answered_s and longest_delay_in_queue_abandoned_s KJ
--				2011-01-28 Performance DJ
-- Description:	Loads fact_queue. Obs! Interval = dim_interval_id.
-- =============================================
--exec [mart].[etl_fact_queue_load] '2014-10-01' ,'2014-10-01',-2

CREATE PROCEDURE [mart].[etl_fact_queue_load] 
@start_date smalldatetime,
@end_date smalldatetime,
@datasource_id int,
@is_delayed_job bit = 0

AS
DECLARE @internal bit
DECLARE @sqlstring nvarchar(4000)
SET @sqlstring = ''

--Execute one delayed jobs, if any
if (@datasource_id=-2 AND @is_delayed_job=0) --called from ETL
	EXEC mart.etl_execute_delayed_job @stored_procedure='mart.etl_fact_queue_load'

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
		EXEC [mart].[etl_fact_queue_load] @start_date, @end_date, @datasource_id
		FETCH NEXT FROM DataSouceCursor INTO @datasource_id
	END
	CLOSE DataSouceCursor
	DEALLOCATE DataSouceCursor
END

ELSE  --Single datasource_id
BEGIN
	--is this an internal or external datasource?
	SELECT @internal = internal FROM mart.sys_datasource WHERE datasource_id = @datasource_id

	--declare
	CREATE TABLE #bridge_time_zone(date_id int,interval_id int,time_zone_id int,local_date_id int,local_interval_id int)
	CREATE TABLE #agg_queue_ids (queue_agg_id int, mart_queue_id int)

	DECLARE @UtcNow as smalldatetime
	DECLARE @start_date_id	INT
	DECLARE @end_date_id	INT
	DECLARE @max_date smalldatetime
	DECLARE @min_date smalldatetime
	DECLARE @time_zone_id smallint

	--init
	SELECT @UtcNow=CAST(getutcdate() as smalldatetime)
	SELECT 
		@time_zone_id = ds.time_zone_id
	FROM
		Mart.sys_datasource ds
	WHERE 
		ds.datasource_id= @datasource_id

	SET	@start_date = convert(smalldatetime,floor(convert(decimal(18,4),@start_date )))
	SET @end_date	= convert(smalldatetime,floor(convert(decimal(18,4),@end_date )))

	SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
	SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)

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

	UPDATE #bridge_time_zone
	SET interval_id= bt.interval_id
	FROM 
	(SELECT date_id,local_date_id,local_interval_id,interval_id= MIN(interval_id)
	FROM mart.bridge_time_zone
	WHERE time_zone_id=@time_zone_id
	GROUP BY date_id,local_date_id,local_interval_id)bt
	INNER JOIN #bridge_time_zone temp ON temp.local_interval_id=bt.local_interval_id
	AND temp.date_id=bt.date_id
	AND temp.local_date_id=bt.local_date_id
	
	-------------
	-- Delete rows
	-------------
	--DELETE FROM mart.fact_queue  WHERE local_date_id between @start_date_id AND @end_date_id and datasource_id = @datasource_id 
	DELETE  f
	FROM mart.fact_queue f 
	INNER JOIN #bridge_time_zone b 
		ON b.date_id=f.date_id AND b.interval_id=f.interval_id
	WHERE EXISTS (select 1 from #agg_queue_ids q where q.mart_queue_id = f.queue_id)

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
		offered_calls				= ISNULL(offd_direct_call_cnt,0), 
		answered_calls				= ISNULL(answ_call_cnt,0), 
		answered_calls_within_SL	= ISNULL(ans_servicelevel_cnt,0), 
		abandoned_calls				= ISNULL(aband_call_cnt,0), 
		abandoned_calls_within_SL	= ISNULL(aband_within_sl_cnt,0), 
		abandoned_short_calls		= ISNULL(aband_short_call_cnt,0), 
		overflow_out_calls			= ISNULL(overflow_out_call_cnt,0),
		overflow_in_calls			= ISNULL(overflow_in_call_cnt,0), 
		talk_time_s					= ISNULL(talking_call_dur,0), 
		after_call_work_s			= ISNULL(wrap_up_dur,0), 
		handle_time_s				= ISNULL(talking_call_dur,0)+ISNULL(wrap_up_dur,0), 
		speed_of_answer_s			= ISNULL(queued_and_answ_call_dur,0), 
		time_to_abandon_s			= ISNULL(queued_and_aband_call_dur,0), 
		longest_delay_in_queue_answered_s = ISNULL(queued_answ_longest_que_dur,0),
		longest_delay_in_queue_abandoned_s = ISNULL(queued_aband_longest_que_dur,0),
		datasource_id				= ' + cast(@datasource_id as varchar(3)) + ', 
		insert_date					= getdate()
	FROM
		(SELECT * FROM '+ 
		CASE @internal
			WHEN 0 THEN '	mart.v_queue_logg agg'
			WHEN 1 THEN '	dbo.queue_logg agg'
			ELSE NULL --Fail fast
		END
		+ ' WHERE date_from between '''+ CAST(@start_date as nvarchar(20))+''' and '''+ CAST(@end_date as nvarchar(20))+'''
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
	JOIN
	 #bridge_time_zone bridge
	ON
		d.date_id		= bridge.local_date_id		AND
		stg.interval	= bridge.local_interval_id'

	--Exec
	EXEC sp_executesql @sqlstring

	drop table #bridge_time_zone

END

GO


