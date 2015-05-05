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
(@QueueList					varchar(max),		
@DateFrom					smalldatetime,
@DateTo						smalldatetime,
@SkillCode					uniqueidentifier,
@MidnightBreakDifference	int
)
AS

BEGIN
	SET NOCOUNT ON;

	DECLARE @mindate as smalldatetime
	DECLARE @time_zone_id as int

	SET @mindate = CAST('19000101' as smalldatetime)
	SELECT @time_zone_id = time_zone_id FROM mart.dim_skill WHERE skill_code = @SkillCode

	
	DECLARE @TempList table
	(
		QueueID int
	)

	CREATE TABLE #PreResult(
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NULL,
	[interval_start] [smalldatetime] NULL,
	[StatOfferedTasks] [decimal](24, 5) NULL,
	[StatAnsweredTasks] [decimal](24, 5) NULL,
	[StatTaskTimeSeconds] [decimal](38, 14) NOT NULL,
	[StatAfterTaskTimeSeconds] [decimal](38, 14) NOT NULL
	)

	INSERT INTO @TempList
		SELECT * FROM mart.SplitStringInt(@QueueList)



	--prepare pre-result
	INSERT INTO #PreResult
	SELECT
		d.date_id,
		i.interval_id,
		i.interval_start,
		fq.offered_calls as StatOfferedTasks,
		fq.answered_calls as StatAnsweredTasks,
		ISNULL(fq.talk_time_s, 0) AS StatTaskTimeSeconds,
		ISNULL(fq.after_call_work_s, 0) AS StatAfterTaskTimeSeconds
	FROM
		mart.fact_queue fq WITH (NOLOCK)
	INNER JOIN	mart.dim_date d
		ON fq.date_id = d.date_id
		AND d.date_date BETWEEN DATEADD(day, -1, @DateFrom) and DATEADD(day, 2, @DateTo) --limit the join but include +/- 1 day in order to cover time zone stuff
	INNER JOIN	mart.dim_interval i WITH (NOLOCK)
		ON fq.interval_id = i.interval_id 
	INNER JOIN	@TempList q
		ON fq.queue_id = q.QueueID
	WHERE (DATEADD(mi, DATEDIFF(mi, @mindate,i.interval_start), d.date_date) BETWEEN DATEADD(day, -1, @DateFrom) and DATEADD(day, 2, @DateTo))
	
	
	SELECT 
		CONVERT(DATE, DATEADD(mi, DATEDIFF(mi, @mindate, i.interval_start) - @MidnightBreakDifference, d.date_date)) as Interval,
		--r.date_id, r.interval_id,
		SUM(r.StatAnsweredTasks) as StatAnsweredTasks,
		SUM(r.StatOfferedTasks) as StatOfferedTasks,
		CASE SUM(r.StatAnsweredTasks)
			WHEN 0 
			THEN SUM(StatTaskTimeSeconds)
			ELSE ISNULL(SUM(StatTaskTimeSeconds)/SUM(r.StatAnsweredTasks), 0)
		END AS StatAverageTaskTimeSeconds,
		CASE SUM(r.StatAnsweredTasks)
			WHEN 0 
			THEN SUM(StatAfterTaskTimeSeconds)
			ELSE ISNULL(SUM(StatAfterTaskTimeSeconds)/SUM(r.StatAnsweredTasks), 0)
		END AS StatAverageAfterTaskTimeSeconds
	FROM 
		#PreResult r
	INNER JOIN mart.bridge_time_zone b 
		ON b.time_zone_id = @time_zone_id
			AND b.date_id = r.date_id 
			AND b.interval_id = r.interval_id
	INNER JOIN mart.dim_interval i 
		ON i.interval_id = b.local_interval_id
	INNER JOIN mart.dim_date d 
		ON d.date_id = b.local_date_id
	WHERE
		CONVERT(DATE, DATEADD(mi, DATEDIFF(mi, @mindate, i.interval_start) - @MidnightBreakDifference, d.date_date)) BETWEEN @DateFrom AND @DateTo
	GROUP BY 
		CONVERT(DATE, DATEADD(mi, DATEDIFF(mi, @mindate, i.interval_start) - @MidnightBreakDifference, d.date_date))--,r.date_id, r.interval_id
END
GO