IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[fact_queue_export]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[fact_queue_export]
GO

-- EXEC [mart].[fact_queue_export] '16','2012-04-20','2015-05-01', 'Eastern Standard Time'

CREATE PROCEDURE [mart].[fact_queue_export] 
(@QueueList					varchar(max),		
@DateFrom					smalldatetime,
@DateTo						smalldatetime,
@TimeZoneCode				nvarchar(50)
)
AS

BEGIN
	SET NOCOUNT ON;

	DECLARE @mindate as smalldatetime
	DECLARE @time_zone_id as int
	DECLARE @intervalLength as int

	SET @mindate = CAST('19000101' as smalldatetime)

	SELECT @time_zone_id = time_zone_id 
	FROM mart.dim_time_zone 
	WHERE time_zone_code = @TimeZoneCode
	
	SELECT @intervalLength = value 
	FROM mart.sys_configuration 
	WHERE [key]='IntervalLengthMinutes'

	
	DECLARE @TempList table
	(
		QueueID int
	)

	INSERT INTO @TempList
		SELECT * FROM mart.SplitStringInt(@QueueList)
			
	CREATE TABLE #PreResult(
		interval int,
		[date] nvarchar(8),
		[time] nvarchar(5),
		[queue] int,
		queue_name nvarchar(30),
		offd_direct_call_cnt int,
		overflow_in_call_cnt int,
		aband_call_cnt int,
		overflow_out_call_cnt int,
		answ_call_cnt int,
		queued_and_answ_call_dur int,
		queued_and_aband_call_dur int,
		talking_call_dur int,
		wrap_up_dur int,
		queued_answ_longest_que_dur int,
		queued_aband_longest_que_dur int,
		avg_avail_member_cnt int,
		ans_servicelevel_cnt int,
		wait_dur int,
		aband_short_call_cnt int,
		aband_within_sl_cnt int
	)
	

	INSERT INTO #PreResult
	SELECT
		@intervalLength as interval,
		CONVERT(nvarchar(8), d.date_date, 112) as [date],
		CONVERT(char(5), i.interval_start, 108) as [time],
		33333 as queue,
		'Import Queue 3' as queue_name,
		ISNULL(fq.offered_calls, 0) as offd_direct_call_cnt,
		ISNULL(fq.overflow_in_calls, 0) as overflow_in_call_cnt,
		ISNULL(fq.abandoned_calls, 0) as aband_call_cnt,
		ISNULL(fq.overflow_out_calls, 0) as overflow_out_call_cnt,
		ISNULL(fq.answered_calls, 0) as answ_call_cnt,
		ISNULL(fq.speed_of_answer_s, 0) as queued_and_answ_call_dur,
		ISNULL(fq.time_to_abandon_s, 0) as queued_and_aband_call_dur,
		ISNULL(fq.talk_time_s, 0) as talking_call_dur,
		ISNULL(fq.after_call_work_s, 0) as wrap_up_dur,
		ISNULL(fq.longest_delay_in_queue_answered_s, 0) as queued_answ_longest_que_dur,
		ISNULL(fq.longest_delay_in_queue_abandoned_s, 0) as queued_aband_longest_que_dur,
		0 as avg_avail_member_cnt,
		ISNULL(fq.answered_calls_within_SL, 0) as ans_servicelevel_cnt,
		0 as wait_dur,
		ISNULL(fq.abandoned_short_calls, 0) as aband_short_call_cnt,
		ISNULL(fq.abandoned_calls_within_SL, 0) as aband_within_sl_cnt
	FROM
		mart.fact_queue fq WITH (NOLOCK)
	INNER JOIN mart.bridge_time_zone b 
		ON b.time_zone_id = @time_zone_id
			AND b.date_id = fq.date_id 
			AND b.interval_id = fq.interval_id
	INNER JOIN mart.dim_interval i 
		ON i.interval_id = b.local_interval_id
	INNER JOIN mart.dim_date d 
		ON d.date_id = b.local_date_id
			AND d.date_date BETWEEN DATEADD(day, -1, @DateFrom) and DATEADD(day, 2, @DateTo) --limit the join but include +/- 1 day in order to cover time zone stuff
	INNER JOIN	@TempList q
		ON fq.queue_id = q.QueueID
	WHERE 
		(DATEADD(mi, DATEDIFF(mi, @mindate,i.interval_start), d.date_date) BETWEEN DATEADD(day, -1, @DateFrom) and DATEADD(day, 2, @DateTo))
	

	SELECT
		interval,
		[date],
		[time],
		[queue],
		queue_name,
		SUM(offd_direct_call_cnt),
		SUM(overflow_in_call_cnt),
		SUM(aband_call_cnt),
		SUM(overflow_out_call_cnt),
		SUM(answ_call_cnt),
		SUM(queued_and_answ_call_dur),
		SUM(queued_and_aband_call_dur),
		SUM(talking_call_dur),
		SUM(wrap_up_dur),
		SUM(queued_answ_longest_que_dur),
		SUM(queued_aband_longest_que_dur),
		SUM(avg_avail_member_cnt),
		SUM(ans_servicelevel_cnt),
		SUM(wait_dur),
		SUM(aband_short_call_cnt),
		SUM(aband_within_sl_cnt)
	FROM 
		#PreResult
	GROUP BY
		interval,
		[date],
		[time],
		[queue],
		queue_name
	ORDER BY
		interval,
		[date],
		[time],
		[queue],
		queue_name
END
GO