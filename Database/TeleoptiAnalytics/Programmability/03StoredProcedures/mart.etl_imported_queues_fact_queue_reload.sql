IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_imported_queues_fact_queue_reload]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_imported_queues_fact_queue_reload]
GO

/****** Object:  StoredProcedure [mart].[etl_imported_queues_fact_queue_reload]    Script Date: 07/04/2012 15:28:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<KJ>
-- Create date: <2015-05-18>
-- Description:	<Reloads data from imported files to fact_queue from fact_queue_old>
-----------------------------------------------------------------------
CREATE PROCEDURE [mart].[etl_imported_queues_fact_queue_reload] 
@start_date smalldatetime,
@end_date smalldatetime,
@is_delayed_job int
AS
BEGIN
SET NOCOUNT ON
--DECLARE
DECLARE @datasource_id smallint
DECLARE @log_object_name nvarchar(100)

--INIT
SET @datasource_id = 1
SET @log_object_name  = 'Teleopti CCC - File import'
DECLARE @start_date_id	INT
DECLARE @end_date_id	INT
SET @start_date_id	=	(SELECT date_id FROM dim_date WHERE @start_date = date_date)
SET @end_date_id	=	(SELECT date_id FROM dim_date WHERE @end_date = date_date)


--temp tables used
CREATE TABLE #queues(
	[queue_id] [int] NOT NULL
)


--AF:Speed up the delete by preparing the queues
INSERT #queues
SELECT queue_id 
FROM mart.dim_queue q 
WHERE log_object_name=@log_object_name
and datasource_id=@datasource_id

SET NOCOUNT OFF
-- Delete rows for the queues imported from file
DELETE q
FROM mart.fact_queue q 
WHERE q.datasource_id = @datasource_id
AND EXISTS (SELECT 1 FROM #queues WHERE q.queue_id = #queues.queue_id)
AND date_id between @start_date_id AND @end_date_id 

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
	date_id						= old.date_id, 
	interval_id					= old.interval_id, 
	queue_id					= old.queue_id, 
	offered_calls				= old.offered_calls, 
	answered_calls				= old.answered_calls, 
	answered_calls_within_SL	= old.answered_calls_within_SL, 
	abandoned_calls				= old.abandoned_calls, 
	abandoned_calls_within_SL	= old.abandoned_calls_within_SL, 
	abandoned_short_calls		= old.abandoned_short_calls, 
	overflow_out_calls			= old.overflow_out_calls,
	overflow_in_calls			= old.overflow_in_calls, 
	talk_time_s					= old.talk_time_s, 
	after_call_work_s			= old.after_call_work_s, 
	handle_time_s				= old.handle_time_s, 
	speed_of_answer_s			= old.speed_of_answer_s, 
	time_to_abandon_s			= old.time_to_abandon_s, 
	longest_delay_in_queue_answered_s = old.longest_delay_in_queue_answered_s,
	longest_delay_in_queue_abandoned_s = old.longest_delay_in_queue_abandoned_s,
	datasource_id				= old.datasource_id, 
	insert_date					= getdate()
FROM
	mart.fact_queue_old old
WHERE date_id between @start_date_id AND @end_date_id
AND EXISTS (SELECT 1 FROM #queues WHERE old.queue_id = #queues.queue_id)

END