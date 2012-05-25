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
-- 2009-02-11	KJ		Added new mart schema
-- 2009-11-28	DJ		Switch interface to use queue_id (instead of queue_original_id)
-- 2010-02-04	DJ		Add WITH (NOLOCK)
-- 2011-06-13	RobinK	Changed from date list to date range (functionality wasn't used from code)
-- 2011-07-13	DJ		re-factor and do sum() in sql server to save bandwith (x number of queues)	
-- 2011-09-22	RobinK	Resolved an issue when doing sum of average times.
-- =============================================
-- EXEC [mart].[raptor_statistics_load] '1,2,3,4,5','2009-02-10 08:00','2009-02-13 08:00'

CREATE PROCEDURE [mart].[raptor_statistics_load] 
(@QueueList		varchar(max),		
@DateFrom		smalldatetime,
@DateTo			smalldatetime
)
AS

CREATE TABLE #PreResult(
	[date_date] [smalldatetime] NOT NULL,
	[interval_start] [smalldatetime] NULL,
	[StatOfferedTasks] [decimal](24, 5) NULL,
	[StatAbandonedTasks] [decimal](24, 5) NULL,
	[StatAbandonedShortTasks] [decimal](18, 0) NULL,
	[StatAbandonedTasksWithinSL] [decimal](24, 5) NULL,
	[StatAnsweredTasks] [decimal](24, 5) NULL,
	[StatAnsweredTasksWithinSL] [decimal](24, 5) NULL,
	[StatOverflowOutTasks] [decimal](24, 5) NULL,
	[StatOverflowInTasks] [decimal](24, 5) NULL,
	[StatTaskTimeSeconds] [decimal](38, 14) NOT NULL,
	[StatAfterTaskTimeSeconds] [decimal](38, 14) NOT NULL,
	[StatQueueTimeSeconds] [decimal](38, 14) NOT NULL,
	[StatHandleTimeSeconds] [decimal](38, 14) NOT NULL,
	[StatTimeToAbandonSeconds] [decimal](38, 14) NOT NULL,
	[StatTimeLongestInQueueAnsweredSeconds] [decimal](38, 14) NOT NULL,
	[StatTimeLongestInQueueAbandonedSeconds] [decimal](38, 14) NOT NULL
)

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

	--prepare pre-result
	INSERT INTO #PreResult
	SELECT	
		d.date_date,
		i.interval_start,
		ql.offered_calls as StatOfferedTasks,
		ql.abandoned_calls as StatAbandonedTasks, 
		ql.abandoned_short_calls as StatAbandonedShortTasks, 
		ql.abandoned_calls_within_SL as StatAbandonedTasksWithinSL, 
		ql.answered_calls as StatAnsweredTasks,
		ql.answered_calls_within_SL as StatAnsweredTasksWithinSL,
		ql.overflow_out_calls as StatOverflowOutTasks,
		ql.overflow_in_calls as StatOverflowInTasks,
		ISNULL(ql.talk_time_s, 0) AS StatTaskTimeSeconds,
		ISNULL(ql.after_call_work_s, 0) AS StatAfterTaskTimeSeconds,
		ISNULL(ql.speed_of_answer_s, 0) AS StatQueueTimeSeconds,
		ISNULL(ql.handle_time_s, 0) AS StatHandleTimeSeconds,
		ISNULL(ql.time_to_abandon_s, 0) AS StatTimeToAbandonSeconds,
		ISNULL(ql.longest_delay_in_queue_answered_s, 0) AS StatTimeLongestInQueueAnsweredSeconds,
		ISNULL(ql.longest_delay_in_queue_abandoned_s, 0) AS StatTimeLongestInQueueAbandonedSeconds
	FROM		mart.fact_queue ql WITH (NOLOCK)
	INNER JOIN	mart.dim_date d
		ON ql.date_id = d.date_id
		AND d.date_date BETWEEN dateadd(day,-1,@DateFrom) and dateadd(day,1,@DateTo) --limit the join but include +/- 1 day in order to cover time zone stuff
	INNER JOIN	mart.dim_interval i WITH (NOLOCK)
		ON ql.interval_id = i.interval_id 
	INNER JOIN	@TempList q
		ON ql.queue_id = q.QueueID
	WHERE (DATEADD(mi, DATEDIFF(mi,@mindate,i.interval_start), d.date_date) BETWEEN @DateFrom and @DateTo)

	--Return to client
	SELECT
	DATEADD(mi, DATEDIFF(mi,@mindate,interval_start), date_date) as Interval,
	sum([StatOfferedTasks]) as StatOfferedTasks,
	sum([StatAbandonedTasks]) as StatAbandonedTasks,
	sum([StatAbandonedShortTasks]) as StatAbandonedShortTasks,
	sum([StatAbandonedTasksWithinSL]) as StatAbandonedTasksWithinSL,
	sum([StatAnsweredTasks]) as StatAnsweredTasks,
	sum([StatAnsweredTasksWithinSL]) as StatAnsweredTasksWithinSL,
	sum([StatOverflowOutTasks]) as StatOverflowOutTasks,
	sum([StatOverflowInTasks]) as StatOverflowInTasks,
	CASE sum(StatAnsweredTasks)
			WHEN 0 
			THEN sum([StatTaskTimeSeconds])
			ELSE ISNULL(sum([StatTaskTimeSeconds])/sum(StatAnsweredTasks), 0)
		END AS StatAverageTaskTimeSeconds,
		CASE sum(StatAnsweredTasks)
			WHEN 0 
			THEN sum([StatAfterTaskTimeSeconds])
			ELSE ISNULL(sum([StatAfterTaskTimeSeconds])/sum(StatAnsweredTasks), 0)
		END AS StatAverageAfterTaskTimeSeconds,
		CASE sum(StatAnsweredTasks)
			WHEN 0 
			THEN sum([StatQueueTimeSeconds])
			ELSE ISNULL(sum([StatQueueTimeSeconds])/sum(StatAnsweredTasks), 0)
		END AS StatAverageQueueTimeSeconds,
		CASE sum(StatAnsweredTasks)
			WHEN 0 
			THEN sum([StatHandleTimeSeconds])
			ELSE ISNULL(sum([StatHandleTimeSeconds])/sum(StatAnsweredTasks), 0)
		END AS StatAverageHandleTimeSeconds,
		CASE sum(StatAnsweredTasks)
			WHEN 0 
			THEN sum([StatTimeToAbandonSeconds])
			ELSE ISNULL(sum([StatTimeToAbandonSeconds])/sum(StatAnsweredTasks), 0)
		END AS StatAverageTimeToAbandonSeconds,
		CASE sum(StatAnsweredTasks)
			WHEN 0 
			THEN sum([StatTimeLongestInQueueAnsweredSeconds])
			ELSE ISNULL(sum([StatTimeLongestInQueueAnsweredSeconds])/sum(StatAnsweredTasks), 0)
		END AS StatAverageTimeLongestInQueueAnsweredSeconds,
		CASE sum(StatAnsweredTasks)
			WHEN 0 
			THEN sum([StatTimeLongestInQueueAbandonedSeconds])
			ELSE ISNULL(sum([StatTimeLongestInQueueAbandonedSeconds])/sum(StatAnsweredTasks), 0)
		END AS StatAverageTimeLongestInQueueAbandonedSeconds
	FROM #PreResult
	GROUP BY DATEADD(mi, DATEDIFF(mi,@mindate,interval_start), date_date)

END
GO