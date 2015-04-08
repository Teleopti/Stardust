IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_queue_statistics_load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_queue_statistics_load]
GO
-- =============================================
-- Author:		Jonas, Kunning
-- Create date: 2015-04-07
-- Description:	Return the queue workload on hourly basis
-- ---------------------------------------------
-- ChangeLog:
-- Date			Author	Description
-- ---------------------------------------------

-- =============================================
-- EXEC [mart].[raptor_queue_statistics_load] '1,2,3,4,5','2009-02-10 08:00','2009-02-13 08:00'

CREATE PROCEDURE [mart].[raptor_queue_statistics_load] 
(@QueueList		varchar(max),		
@DateFrom		smalldatetime,
@DateTo			smalldatetime
)
AS

CREATE TABLE #PreResult(
	[date_date] [smalldatetime] NOT NULL,
	[interval_start] [smalldatetime] NULL,
	[StatOfferedTasks] [decimal](24, 5) NULL,
	[StatAnsweredTasks] [decimal](24, 5) NULL,
	[StatTaskTimeSeconds] [decimal](38, 14) NOT NULL,
	[StatAfterTaskTimeSeconds] [decimal](38, 14) NOT NULL
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
		ql.answered_calls as StatAnsweredTasks,
		ISNULL(ql.talk_time_s, 0) AS StatTaskTimeSeconds,
		ISNULL(ql.after_call_work_s, 0) AS StatAfterTaskTimeSeconds
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
	DATEADD(hh, DATEDIFF(hh,@mindate,interval_start), date_date) as Interval,
	sum([StatAnsweredTasks]) as StatAnsweredTasks,
	sum([StatOfferedTasks]) as StatOfferedTasks,
	CASE sum(StatAnsweredTasks)
			WHEN 0 
			THEN sum([StatTaskTimeSeconds])
			ELSE ISNULL(sum([StatTaskTimeSeconds])/sum(StatAnsweredTasks), 0)
		END AS StatAverageTaskTimeSeconds,
		CASE sum(StatAnsweredTasks)
			WHEN 0 
			THEN sum([StatAfterTaskTimeSeconds])
			ELSE ISNULL(sum([StatAfterTaskTimeSeconds])/sum(StatAnsweredTasks), 0)
		END AS StatAverageAfterTaskTimeSeconds
	FROM #PreResult
	GROUP BY DATEADD(hh, DATEDIFF(hh,@mindate,interval_start), date_date)

END
GO