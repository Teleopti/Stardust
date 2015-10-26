IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[IntradaySkillStatistics]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[IntradaySkillStatistics]
GO
-- =============================================
-- Author:		Ola
-- Create date: 2015-11-22
-- Description:	Returns statistics per skill and interval
-- ---------------------------------------------
-- ChangeLog:
-- Date			Author	Description
-- 2015-11-26   Ola		Get timezone from the skill, and remove the midnightbreakoffset, needs to be handled on client id needed, it is not in the mart

-- =============================================
-- EXEC [mart].[IntradaySkillStatistics] '2011-04-14 00:00','2011-04-14 00:00'

CREATE PROCEDURE [mart].[IntradaySkillStatistics] 
(@DateFrom					smalldatetime,
@DateTo						smalldatetime
)
AS

BEGIN
	SET NOCOUNT ON;

	DECLARE @mindate as smalldatetime
	DECLARE @time_zone_id as int

	SET @mindate = CAST('19000101' as smalldatetime)
	
	CREATE TABLE #PreResult(
	skill_code uniqueidentifier NOT NULL,
	skill_name varchar(100),
	skill_timezone_id int,
	[date_id] [int] NOT NULL,
	[interval_id] [smallint] NULL,
	[interval_start] [smalldatetime] NULL,
	[StatOfferedTasks] [decimal](24, 5) NULL,
	[StatAnsweredTasks] [decimal](24, 5) NULL,
	[StatTaskTimeSeconds] [decimal](38, 14) NOT NULL,
	[StatAfterTaskTimeSeconds] [decimal](38, 14) NOT NULL,
	[StatAbandonedTasks] [decimal](24, 5) NULL,
	[StatAbandonedShortTasks] [decimal](18, 0) NULL,
	[StatAbandonedTasksWithinSL] [decimal](24, 5) NULL,
	[StatOverflowInTasks] [decimal](24, 5) NULL,
	[StatOverflowOutTasks] [decimal](24, 5) NULL
	)

	--prepare pre-result
	INSERT INTO #PreResult
	SELECT
		s.skill_code,
		s.skill_name,
		s.time_zone_id,
		d.date_id,
		i.interval_id,
		i.interval_start,
		fq.offered_calls as StatOfferedTasks,
		fq.answered_calls as StatAnsweredTasks,
		ISNULL(fq.talk_time_s, 0) AS StatTaskTimeSeconds,
		ISNULL(fq.after_call_work_s, 0) AS StatAfterTaskTimeSeconds,
		fq.abandoned_calls as StatAbandonedTasks, 
		fq.abandoned_short_calls as StatAbandonedShortTasks, 
		fq.abandoned_calls_within_SL as StatAbandonedTasksWithinSL, 
		fq.overflow_in_calls as StatOverflowInTasks,
		fq.overflow_out_calls as StatOverflowOutTasks
	FROM
		mart.fact_queue fq WITH (NOLOCK)
	INNER JOIN	mart.dim_date d
		ON fq.date_id = d.date_id
		AND d.date_date BETWEEN DATEADD(day, -1, @DateFrom) and DATEADD(day, 2, @DateTo) --limit the join but include +/- 1 day in order to cover time zone stuff
	INNER JOIN	mart.dim_interval i WITH (NOLOCK)
		ON fq.interval_id = i.interval_id 
	INNER JOIN [mart].[bridge_queue_workload] qw ON fq.queue_id = qw.queue_id
	INNER JOIN dim_skill s ON s.skill_id = qw.skill_id
	WHERE (DATEADD(mi, DATEDIFF(mi, @mindate,i.interval_start), d.date_date) BETWEEN DATEADD(day, -1, @DateFrom) and DATEADD(day, 2, @DateTo))

	SELECT 
		skill_code AS SkillId,
		skill_name AS SkillName,
		DATEADD(mi, DATEDIFF(mi, @mindate, i.interval_start) , d.date_date) as Interval,
		--r.date_id, r.interval_id,
		SUM(r.StatAnsweredTasks) as StatAnsweredTasks,
		SUM(r.StatOfferedTasks) as StatOfferedTasks,
		SUM(r.StatAbandonedTasks) as StatAbandonedTasks,
		SUM(r.StatAbandonedShortTasks) as StatAbandonedShortTasks,
		SUM(r.StatAbandonedTasksWithinSL) as StatAbandonedTasksWithinSL,
		SUM(r.StatOverflowOutTasks) as StatOverflowOutTasks,
		SUM(r.StatOverflowInTasks) as StatOverflowInTasks,
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
		ON b.time_zone_id = skill_timezone_id
			AND b.date_id = r.date_id 
			AND b.interval_id = r.interval_id
	INNER JOIN mart.dim_interval i 
		ON i.interval_id = b.local_interval_id
	INNER JOIN mart.dim_date d 
		ON d.date_id = b.local_date_id
	WHERE
		CONVERT(DATE, DATEADD(mi, DATEDIFF(mi, @mindate, i.interval_start) , d.date_date)) BETWEEN @DateFrom AND @DateTo
	GROUP BY 
		skill_code, skill_name, DATEADD(mi, DATEDIFF(mi, @mindate, i.interval_start) , d.date_date)--,r.date_id, r.interval_id
END
