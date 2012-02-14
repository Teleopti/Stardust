IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_statistics_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_statistics_load]
GO
-- =============================================
-- Author:		Unknown
-- Create date: 2008-xx-xx
-- Description:	Return the queue workload used in "prepare workload"
-- ---------------------------------------------
-- ChangeLog:
-- Date			Author	Description
-- ---------------------------------------------
-- 2008-12-02	DJ		Use existing functions to split input strings
-- 2009-11-28	DJ		Switch interface to use queue_id (instead of queue_original_id)'
-- 2011-06-30	DJ		Fix changed interface (#15566)
-- =============================================
CREATE PROCEDURE [mart].[raptor_statistics_load] 
(@QueueList		varchar(max),		
@DateFrom		smalldatetime,
@DateTo			smalldatetime
)
AS

--Create system mindate
DECLARE @mindate as smalldatetime
SELECT @mindate=CAST('19000101' as smalldatetime)

BEGIN
	SET NOCOUNT ON;
	--Declares
	DECLARE @TempList table
	(
	QueueID int
	)

	--Init
	INSERT INTO @TempList
	SELECT * FROM mart.SplitStringInt(@QueueList)

	--Return result set to client
	SELECT	
		DATEADD(mi, DATEDIFF(mi,@mindate,i.interval_start), d.date_date) as Interval, 
		ql.offered_calls as StatOfferedTasks,
		ql.abandoned_calls as StatAbandonedTasks, 
		ql.abandoned_short_calls as StatAbandonedShortTasks, 
		ql.abandoned_calls_within_SL as StatAbandonedTasksWithinSL, 
		ql.answered_calls as StatAnsweredTasks,
		ql.answered_calls_within_SL as StatAnsweredTasksWithinSL,
		ql.overflow_out_calls as StatOverflowOutTasks,
		ql.overflow_in_calls as StatOverflowInTasks,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.talk_time_s, 0)
			ELSE ISNULL(ql.talk_time_s/ql.answered_calls, 0)
		END AS StatAverageTaskTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.after_call_work_s, 0)
			ELSE ISNULL(ql.after_call_work_s/ql.answered_calls, 0)
		END AS StatAverageAfterTaskTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.speed_of_answer_s, 0)
			ELSE ISNULL(ql.speed_of_answer_s/ql.answered_calls, 0)
		END AS StatAverageQueueTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.handle_time_s, 0)
			ELSE ISNULL(ql.handle_time_s/ql.answered_calls, 0)
		END AS StatAverageHandleTimeSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.time_to_abandon_s, 0)
			ELSE ISNULL(ql.time_to_abandon_s/ql.answered_calls, 0)
		END AS StatAverageTimeToAbandonSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.longest_delay_in_queue_answered_s, 0)
			ELSE ISNULL(ql.longest_delay_in_queue_answered_s/ql.answered_calls, 0)
		END AS StatAverageTimeLongestInQueueAnsweredSeconds,
		CASE ql.answered_calls
			WHEN 0 
			THEN ISNULL(ql.longest_delay_in_queue_abandoned_s, 0)
			ELSE ISNULL(ql.longest_delay_in_queue_abandoned_s/ql.answered_calls, 0)
		END AS StatAverageTimeLongestInQueueAbandonedSeconds
	FROM		mart.fact_queue ql WITH (NOLOCK)
	INNER JOIN	mart.dim_date d
		ON ql.date_id = d.date_id 
	INNER JOIN	mart.dim_interval i WITH (NOLOCK)
		ON ql.interval_id = i.interval_id 
	INNER JOIN	mart.dim_queue q  WITH (NOLOCK)
		ON ql.queue_id = q.queue_id 
	WHERE q.queue_id IN (SELECT QueueID FROM @TempList)
	AND (DATEADD(mi, DATEDIFF(mi,@mindate,i.interval_start), d.date_date) BETWEEN @DateFrom and @DateTo)


END
GO