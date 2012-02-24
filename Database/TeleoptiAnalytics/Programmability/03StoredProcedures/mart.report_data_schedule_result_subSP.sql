IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_schedule_result_subSP]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_schedule_result_subSP]
GO

--exec mart.report_data_agent_schedule_result_subSP @date_from='2009-02-03 00:00:00',@date_to='2009-02-03 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_id=NULL,@group_page_agent_set=NULL,@site_id=N'1',@team_id=N'5',@agent_set=N'-1,260,206,275,192,203,279,257,201,195,207',@adherence_id=N'1',@time_zone_id=N'2',@person_code='18037D35-73D5-4211-A309-9B5E015B2B5C',@report_id=12,@language_id=1053

-- =============================================
-- Author:		DJ
-- Create date: 2011-01-21
-- Update Date: 
-- Description:	SubSP used by Agent Metrics Report + Team Metrics
-- ToDo: consider using temp table to store #fact_scedule_deviation
------------------------------------------------
-- Change Log
-- Date			Author	Description
------------------------------------------------
-- 2011-03-17	DJ		#14092
-- 2011-04-12	DJ		#14477
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- =============================================
CREATE PROCEDURE [mart].[report_data_schedule_result_subSP] 
@date_from datetime,
@date_to datetime,
@interval_from int,--mellan vilka tider
@interval_to int,
@adherence_id int,
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@scenario_id int,
@language_id int

AS
SET NOCOUNT ON

--Create temporary tables
CREATE TABLE #bridge_time_zone_subSP
	(
	local_date_id int,
	local_interval_id int,
	date_id int,
	interval_id int,
	date_date_local smalldatetime
	)

CREATE TABLE #agent_queue_statistics_subSP
	(
	date_id int,
	date_date smalldatetime,
	interval_id int,
	acd_login_id int,
	person_id int,
	answered_calls int,		
	talk_time_s decimal(20,2),
	after_call_work_time_s decimal(20,2),
	ready_time_s int
	)

CREATE TABLE #agent_statistics_subSP
	(
	date_id int,
	date_date smalldatetime,
	interval_id int,
	acd_login_id int,
	person_id int,
	ready_time_s int
	)

CREATE TABLE #fact_schedule_deviation_subSP (
	date_id INT,
	person_id INT,
	interval_id INT,
	deviation_schedule_ready_s INT,
	deviation_schedule_s INT,
	deviation_contract_s INT,
	ready_time_s INT,
	is_logged_in INT,
	contract_time_s INT
	)
	
CREATE TABLE #fact_schedule_subSP
	(
	schedule_date_id int,
	date_date smalldatetime,
	interval_id int,
	person_id int,
	scheduled_ready_time_m int,
	scheduled_time_m int,
	scheduled_contract_time_m int
	)

CREATE TABLE #person (
	person_id int
	)

---------
--DECLARE
---------           
DECLARE @date TABLE (date_from_id INT, date_to_id INT)	 
DECLARE @hide_time_zone bit

---------
--INIT
---------
--Check if time zone will be hidden (if only one exist then hide) */
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0

--get person_id only into #person
INSERT INTO #person
SELECT DISTINCT person_id FROM #person_acd_subSP

--Get needed dates and intervals from bridge time zone into temp table
INSERT INTO #bridge_time_zone_subSP
SELECT 
	local_date_id		= d.date_id,
	local_interval_id	= i.interval_id,
	date_id				= b.date_id,
	interval_id			= b.interval_id,
	date_date_local		= d.date_date
FROM mart.bridge_time_zone b
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND @interval_to
WHERE b.time_zone_id = @time_zone_id

--Get the min/max UTC date_id
INSERT INTO @date (date_from_id,date_to_id)
	SELECT MIN(b.date_id),MAX(b.date_id)
	FROM #bridge_time_zone_subSP b
	INNER JOIN mart.dim_date d 
		ON b.local_date_id = d.date_id
	WHERE	d.date_date	between @date_from AND @date_to


--Create local table from: mart.fact_schedule_deviation
INSERT INTO #fact_schedule_deviation_subSP
SELECT 
	d.date_id,
	fsd.person_id,
	i.interval_id,
	deviation_schedule_ready_s,
	deviation_schedule_s,
	deviation_contract_s,
	ready_time_s,
	is_logged_in,
	contract_time_s
FROM mart.fact_schedule_deviation fsd
INNER JOIN #person a
	ON fsd.person_id = a.person_id
INNER JOIN #bridge_time_zone_subSP b
	ON	fsd.interval_id= b.interval_id
	AND fsd.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id

--Get agent statistics
INSERT INTO #agent_queue_statistics_subSP (date_id,date_date,interval_id,person_id,acd_login_id,answered_calls,talk_time_s,after_call_work_time_s)
SELECT	d.date_id,
		d.date_date 'date_date',
		i.interval_id,
		-1,
		faq.acd_login_id,
		SUM(faq.answered_calls),
		SUM(faq.talk_time_s),
		SUM(faq.after_call_work_time_s)
FROM mart.fact_agent_queue faq
INNER JOIN #bridge_time_zone_subSP b
	ON	faq.interval_id= b.interval_id
	AND faq.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND @interval_to
INNER JOIN #person_acd_subSP acd
	ON acd.acd_login_id = faq.acd_login_id
INNER JOIN mart.DimPersonLocalized(@date_from, @date_to) dpl
	ON acd.person_id = dpl.person_id
	AND d.date_date between dpl.valid_from_date_local and dpl.valid_to_date_local
GROUP BY d.date_id, d.date_date, i.interval_id, faq.acd_login_id

--Get the ready time from mart.fact_agent 
INSERT INTO #agent_statistics_subSP
SELECT	d.date_id,
		d.date_date 'date_date',
		i.interval_id,
		faq.acd_login_id,
		-1,
		SUM(ISNULL(faq.ready_time_s,0))
FROM mart.fact_agent faq
INNER JOIN #bridge_time_zone_subSP b
	ON	faq.interval_id= b.interval_id
	AND faq.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND @interval_to
INNER JOIN #person_acd_subSP acd
	ON acd.acd_login_id = faq.acd_login_id
INNER JOIN mart.DimPersonLocalized(@date_from, @date_to) dpl
	ON acd.person_id = dpl.person_id
	AND d.date_date between dpl.valid_from_date_local and dpl.valid_to_date_local
GROUP BY d.date_id, d.date_date, i.interval_id, faq.acd_login_id

UPDATE #agent_queue_statistics_subSP
SET ready_time_s = a.ready_time_s
FROM #agent_queue_statistics_subSP aqs
INNER JOIN #agent_statistics_subSP a ON aqs.date_id = a.date_id AND aqs.interval_id = a.interval_id AND aqs.acd_login_id = a.acd_login_id

INSERT INTO #agent_queue_statistics_subSP (date_id,date_date,interval_id,acd_login_id,person_id,ready_time_s)
SELECT a.date_id,a.date_date,a.interval_id,a.acd_login_id,a.person_id,ISNULL(a.ready_time_s,0)
FROM #agent_statistics_subSP a
WHERE NOT EXISTS (SELECT 1 FROM #agent_queue_statistics_subSP aqs
					WHERE aqs.date_id = a.date_id AND aqs.interval_id = a.interval_id AND aqs.acd_login_id = a.acd_login_id)

--Update #agent_queue_statistics_subSP with person information
UPDATE #agent_queue_statistics_subSP
SET person_id	= dpl.person_id
FROM #agent_queue_statistics_subSP ags
INNER JOIN #person_acd_subSP acd
	ON ags.acd_login_id = acd.acd_login_id
INNER JOIN mart.DimPersonLocalized(@date_from, @date_to) dpl
	ON acd.person_id = dpl.person_id
	AND date_date between dpl.valid_from_date_local and dpl.valid_to_date_local

--Get agent schedule
INSERT INTO #fact_schedule_subSP(schedule_date_id, date_date, interval_id, person_id, scheduled_time_m, scheduled_ready_time_m, scheduled_contract_time_m)
SELECT	d.date_id, 
		d.date_date 'date_date',
		i.interval_id, 
		fs.person_id, 
		SUM(fs.scheduled_time_m), 
		SUM(fs.scheduled_ready_time_m), 
		SUM(fs.scheduled_contract_time_m)
FROM mart.fact_schedule fs
INNER JOIN #person a
	ON fs.person_id = a.person_id
INNER JOIN #bridge_time_zone_subSP b
	ON	fs.interval_id= b.interval_id
	AND fs.schedule_date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
WHERE fs.scenario_id = @scenario_id
GROUP BY d.date_id, d.date_date, i.interval_id, fs.person_id
ORDER BY d.date_id, i.interval_id, fs.person_id

--Prepare result
INSERT #pre_result_subSP(date_id,interval_id,date_date,person_id,answered_calls,talk_time_s,after_call_work_time_s,ready_time_s)
SELECT	date_id,
		interval_id, 
		date_date, 
		person_id,
		answered_calls, 
		talk_time_s, 
		after_call_work_time_s, 
		ready_time_s
FROM #agent_queue_statistics_subSP

INSERT #pre_result_subSP(date_id,interval_id,date_date,person_id,scheduled_ready_time_m,scheduled_time_m,scheduled_contract_time_m)
SELECT	schedule_date_id,
		interval_id,
		date_date,
		person_id,
		scheduled_ready_time_m,
		scheduled_time_m,
		scheduled_contract_time_m
FROM #fact_schedule_subSP

--Add deviation and adherance figures
INSERT #pre_result_subSP(date_id,interval_id,date_date,person_id,adherence_calc_s,deviation_s)
SELECT	fs.schedule_date_id,
		fs.interval_id,
		fs.date_date,
		fsd.person_id,
		adherence_calc_s =
		CASE @adherence_id 
			WHEN 1 THEN isnull(fs.scheduled_ready_time_m*60,0)
			WHEN 2 THEN isnull(fs.scheduled_time_m*60,0)
			WHEN 3 THEN isnull(fsd.contract_time_s,0)
		END,
		deviation_s		=
		CASE @adherence_id 
			WHEN 1 THEN fsd.deviation_schedule_ready_s
			WHEN 2 THEN fsd.deviation_schedule_s
			WHEN 3 THEN fsd.deviation_contract_s
		END
FROM #fact_schedule_deviation_subSP fsd
INNER JOIN #fact_schedule_subSP fs --only inlclude scheduled intervals
	ON fsd.date_id = fs.schedule_date_id
	AND fsd.interval_id = fs.interval_id
	AND fsd.person_id = fs.person_id

--if @adherence_id=2 we include intervals that have ready_time but no scheduled time. Fake scheduled_time_m = Interval Lenght
IF @adherence_id=2
BEGIN
	DECLARE @intervals_length_m INT
	DECLARE @intervals_per_day INT
	SELECT @intervals_per_day = COUNT(interval_id) FROM mart.dim_interval
	SELECT @intervals_length_m = 1440/@intervals_per_day

	INSERT #pre_result_subSP(date_id,interval_id,date_date,person_id,adherence_calc_s,deviation_s)
	SELECT	d.date_id,
			i.interval_id,
			d.date_date,
			fsd.person_id,
			@intervals_length_m*60,
			deviation_s		=
			CASE @adherence_id 
				WHEN 1 THEN fsd.deviation_schedule_ready_s
				WHEN 2 THEN fsd.deviation_schedule_s
				WHEN 3 THEN fsd.deviation_contract_s
			END
	FROM #fact_schedule_deviation_subSP fsd
	INNER JOIN #bridge_time_zone_subSP b
		ON	fsd.interval_id= b.interval_id
		AND fsd.date_id= b.date_id
	INNER JOIN mart.dim_date d 
		ON b.local_date_id = d.date_id
		AND d.date_date BETWEEN @date_from AND @date_to
	INNER JOIN mart.dim_interval i
		ON b.local_interval_id = i.interval_id
		AND i.interval_id BETWEEN @interval_from AND @interval_to
	INNER JOIN #person a
		ON a.person_id = fsd.person_id
	WHERE NOT EXISTS (SELECT 1 FROM #fact_schedule_subSP fs WHERE fsd.person_id=fs.person_id	AND fsd.date_id=fs.schedule_date_id
		AND fsd.interval_id=fs.interval_id)
END

UPDATE #pre_result_subSP
SET
	team_id			 = p.team_id,
	handling_time_s	 = ISNULL(talk_time_s,0) + ISNULL(after_call_work_time_s,0)
FROM #pre_result_subSP r
INNER JOIN mart.dim_person p
	ON p.person_id=r.person_id
INNER JOIN mart.dim_interval i
	ON i.interval_id = r.interval_id

RETURN 0
GO