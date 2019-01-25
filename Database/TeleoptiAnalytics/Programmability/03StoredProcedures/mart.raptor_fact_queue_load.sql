IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_fact_queue_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_fact_queue_load]
GO

/****** Object:  StoredProcedure [mart].[raptor_fact_queue_load]    Script Date: 07/04/2012 15:28:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<KJ
-- Create date: <2009-02-02>
-- Update date: 2009-02-13 Added new mart schema
-- Description:	<File Import - Loads data to fact_queue from stg_queue.>
--				This procedure is for TeleoptiAnalytics database, NOT same procedure as in TeleoptiCCC database(even though same name). Handles timezones.
-----------------------------------------------------------------------
-- when			who		what
-- 2011-10-26	DavidJ	#16688
-- =============================================

--exec [mart].[raptor_fact_queue_load] 

CREATE PROCEDURE [mart].[raptor_fact_queue_load] 
AS
BEGIN
--DECLARE
DECLARE @datasource_id smallint
DECLARE @log_object_name nvarchar(100)

--INIT
SET @datasource_id = 1
SET @log_object_name  = 'Teleopti CCC - File import'
DECLARE @start_date_id	INT
DECLARE @end_date_id	INT

DECLARE @max_date smalldatetime
DECLARE @min_date smalldatetime

--temp tables used
CREATE TABLE #queues(
	[queue_id] [int] NOT NULL
)
CREATE TABLE #stg_queue(
	[date] [datetime] NOT NULL,
	[interval] [nvarchar](50) NOT NULL,
	[queue_code] [int] NULL,
	[queue_name] [nvarchar](100) NOT NULL,
	[offd_direct_call_cnt] [int] NULL,
	[overflow_in_call_cnt] [int] NULL,
	[aband_call_cnt] [int] NULL,
	[overflow_out_call_cnt] [int] NULL,
	[answ_call_cnt] [int] NULL,
	[queued_and_answ_call_dur] [int] NULL,
	[queued_and_aband_call_dur] [int] NULL,
	[talking_call_dur] [int] NULL,
	[wrap_up_dur] [int] NULL,
	[queued_answ_longest_que_dur] [int] NULL,
	[queued_aband_longest_que_dur] [int] NULL,
	[avg_avail_member_cnt] [int] NULL,
	[ans_servicelevel_cnt] [int] NULL,
	[wait_dur] [int] NULL,
	[aband_short_call_cnt] [int] NULL,
	[aband_within_sl_cnt] [int] NULL,
)


--ANALYZE AND UPDATE DATA IN TEMPORARY TABLE
INSERT INTO #stg_queue
SELECT * FROM mart.v_stg_queue


UPDATE #stg_queue
SET queue_code = d.queue_original_id
FROM mart.dim_queue d
INNER JOIN #stg_queue stg
	ON stg.queue_name collate database_default = d.queue_name
AND d.datasource_id = @datasource_id  
WHERE (stg.queue_code is null OR stg.queue_code='')

ALTER TABLE  #stg_queue ADD interval_id smallint
ALTER TABLE #stg_queue ADD [datetime_utc] [datetime] null

UPDATE #stg_queue
SET interval_id= i.interval_id,
	datetime_utc=DATEADD(mi, DATEPART(mi,interval_start)+(DATEPART(hh,interval_start)*60), stg.date)
FROM mart.dim_interval i
INNER JOIN #stg_queue stg ON stg.interval collate database_default =LEFT(i.interval_name,5)

SELECT  
	@min_date= min(stg.datetime_utc) ,
	@max_date= max(stg.datetime_utc)
FROM
	#stg_queue stg

--AF:Speed up the delete by preparing the queues
INSERT INTO #queues
SELECT queue_id
FROM mart.dim_queue dq
INNER JOIN #stg_queue stg
	ON dq.queue_original_id = stg.queue_code
WHERE dq.datasource_id = @datasource_id 

-- Delete rows for the queues imported from file
DELETE mart.fact_queue FROM mart.fact_queue q 
		inner join mart.dim_interval i on i.interval_id=q.interval_id 
		inner join mart.dim_date d on d.date_id=q.date_id
WHERE DATEADD(mi, DATEPART(mi,interval_start)+(DATEPART(hh,interval_start)*60), d.date_date) BETWEEN  
@min_date AND @max_date AND q.datasource_id = @datasource_id
and exists (select 1 from #queues WHERE q.queue_id = #queues.queue_id)

INSERT INTO mart.fact_queue
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
	date_id						= d.date_id, 
	interval_id					= stg.interval_id, 
	queue_id					= q.queue_id, 
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
	datasource_id				= q.datasource_id, 
	insert_date					= getdate()
FROM
	(SELECT * FROM #stg_queue WHERE datetime_utc between @min_date and @max_date) stg
JOIN
	mart.dim_date		d
ON
	stg.date	= d.date_date
JOIN
	mart.dim_queue		q
ON
	q.queue_original_id= stg.queue_code
	AND q.datasource_id = @datasource_id

END