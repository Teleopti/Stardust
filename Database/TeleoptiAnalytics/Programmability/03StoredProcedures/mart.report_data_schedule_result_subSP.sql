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
-- 2013-07-10 backed out of #23621
-- 2013-07-10 Fix #24119, same as #23621
-- =============================================
CREATE PROCEDURE [mart].[report_data_schedule_result_subSP] 
@date_from_id int,
@date_to_id int,
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

CREATE TABLE #agent_queue_statistics_subSP
	(
	date_id int,
	interval_id int,
	acd_login_id int,
	person_code uniqueidentifier,
	person_id int,
	answered_calls int,		
	talk_time_s decimal(20,2),
	after_call_work_time_s decimal(20,2),
	ready_time_s int
	)

CREATE TABLE #agent_statistics_subSP
	(
	date_id int,
	interval_id int,
	acd_login_id int,
	person_code uniqueidentifier,
	ready_time_s int
	)

CREATE TABLE #fact_schedule_deviation_subSP (
	shift_startdate_local_id int,
	shift_startdate_id INT,
	shift_startinterval_id int,
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
	shift_startdate_local_id int,
	shift_startdate_id INT,
	shift_startinterval_id int,
	schedule_date_id int,
	interval_id int,
	person_id int,
	scheduled_ready_time_m int,
	scheduled_time_m int,
	scheduled_contract_time_m int
	)

CREATE TABLE #person (
	person_id int,
	valid_from_date_id_local int,
	valid_to_date_id_local int
	)

CREATE TABLE #acd_login (
	acd_login_id int,
	person_code uniqueidentifier
	)
---------
--DECLARE
---------           
DECLARE @hide_time_zone bit

---------
--INIT
---------
--Check if time zone will be hidden (if only one exist then hide) */
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0

--get distinct person_id and acd_login_id
INSERT INTO #acd_login
SELECT DISTINCT
	acd_login_id,
	person_code
FROM #person_acd_subSP

INSERT INTO #person
SELECT DISTINCT person_id, valid_from_date_id_local,valid_to_date_id_local FROM #person_acd_subSP

--Create local table from: mart.fact_schedule_deviation(do a summary in case of overlapping shifts)
INSERT INTO #fact_schedule_deviation_subSP(shift_startdate_local_id,shift_startdate_id,shift_startinterval_id,date_id,interval_id,person_id,deviation_schedule_ready_s,deviation_schedule_s,deviation_contract_s,ready_time_s,is_logged_in,contract_time_s)
SELECT 
	fsd.shift_startdate_local_id,
	fsd.shift_startdate_id,
	fsd.shift_startinterval_id,
	fsd.date_id,
	fsd.interval_id,
	fsd.person_id,
	SUM(fsd.deviation_schedule_ready_s),
	SUM(fsd.deviation_schedule_s),
	SUM(fsd.deviation_contract_s),
	SUM(fsd.ready_time_s),
	MAX(CAST(fsd.is_logged_in AS INT)),
	SUM(fsd.contract_time_s)
FROM mart.fact_schedule_deviation fsd
INNER JOIN #person a
	ON fsd.person_id = a.person_id
	AND fsd.shift_startdate_local_id between a.valid_from_date_id_local AND a.valid_to_date_id_local
WHERE fsd.shift_startdate_local_id BETWEEN @date_from_id -1 AND @date_to_id +1
AND fsd.date_id BETWEEN @date_from_id AND @date_to_id	
GROUP BY fsd.shift_startdate_local_id,fsd.shift_startdate_id,fsd.shift_startinterval_id,fsd.date_id,fsd.person_id,fsd.interval_id

--Get agent statistics
INSERT INTO #agent_queue_statistics_subSP (date_id,interval_id,acd_login_id,person_code,answered_calls,talk_time_s,after_call_work_time_s)
SELECT	faq.date_id,
		faq.interval_id,
		faq.acd_login_id,
		acd1.person_code,
		SUM(faq.answered_calls),
		SUM(faq.talk_time_s),
		SUM(faq.after_call_work_time_s)
FROM mart.fact_agent_queue faq
INNER JOIN #acd_login acd1
	ON acd1.acd_login_id = faq.acd_login_id
INNER JOIN #acd_login acd2
	ON acd2.acd_login_id = acd1.acd_login_id
	AND acd2.person_code = acd1.person_code
WHERE faq.date_id BETWEEN @date_from_id AND @date_to_id		
AND EXISTS(SELECT 1 FROM #person_acd_subSP p where p.acd_login_id = faq.acd_login_id AND(faq.date_id > p.valid_from_date_id AND faq.date_id < p.valid_to_date_id_maxDate)
		OR (faq.date_id = p.valid_from_date_id AND faq.interval_id >= p.valid_from_interval_id)
		OR (faq.date_id = p.valid_to_date_id_maxdate AND faq.interval_id <= p.valid_to_interval_id_maxdate))
GROUP BY faq.date_id, faq.interval_id, faq.acd_login_id, acd1.person_code

--Get the ready time from mart.fact_agent 
INSERT INTO #agent_statistics_subSP
SELECT	faq.date_id,
		faq.interval_id,
		faq.acd_login_id,
		acd1.person_code,
		SUM(ISNULL(faq.ready_time_s,0))
FROM mart.fact_agent faq
INNER JOIN #acd_login acd1
	ON acd1.acd_login_id = faq.acd_login_id
INNER JOIN #acd_login acd2
	ON acd2.acd_login_id = acd1.acd_login_id
	AND acd2.person_code = acd1.person_code
WHERE faq.date_id BETWEEN @date_from_id AND @date_to_id
AND EXISTS(SELECT 1 FROM #person_acd_subSP p where p.acd_login_id = faq.acd_login_id AND(faq.date_id > p.valid_from_date_id AND faq.date_id < p.valid_to_date_id_maxDate)
		OR (faq.date_id = p.valid_from_date_id AND faq.interval_id >= p.valid_from_interval_id)
		OR (faq.date_id = p.valid_to_date_id_maxdate AND faq.interval_id <= p.valid_to_interval_id_maxdate))
GROUP BY faq.date_id, faq.interval_id, faq.acd_login_id, acd1.person_code

UPDATE #agent_queue_statistics_subSP
SET ready_time_s = a.ready_time_s
FROM #agent_queue_statistics_subSP aqs
INNER JOIN #agent_statistics_subSP a ON aqs.date_id = a.date_id AND aqs.interval_id = a.interval_id AND aqs.acd_login_id = a.acd_login_id and aqs.person_code = a.person_code

INSERT INTO #agent_queue_statistics_subSP (date_id,interval_id,acd_login_id,person_code,ready_time_s)
SELECT a.date_id,a.interval_id,a.acd_login_id,a.person_code,ISNULL(a.ready_time_s,0)
FROM #agent_statistics_subSP a
WHERE NOT EXISTS (SELECT 1 FROM #agent_queue_statistics_subSP aqs
					WHERE aqs.date_id = a.date_id AND aqs.interval_id = a.interval_id AND aqs.acd_login_id = a.acd_login_id)

UPDATE #agent_queue_statistics_subSP
SET person_id	= acd.person_id --potential bug: may result in random update on person_id if multiple person_id share same acd_login_id
FROM #agent_queue_statistics_subSP ags
INNER JOIN #person_acd_subSP acd
	ON ags.acd_login_id = acd.acd_login_id
	AND ags.person_code = acd.person_code
	AND (
		(ags.date_id > acd.valid_from_date_id AND ags.date_id < acd.valid_to_date_id_maxDate)
		OR (ags.date_id = acd.valid_from_date_id AND ags.interval_id >= acd.valid_from_interval_id)
		OR (ags.date_id = acd.valid_to_date_id_maxdate AND ags.interval_id <= acd.valid_to_interval_id_maxdate)
	)

--Get agent schedule
INSERT INTO #fact_schedule_subSP(shift_startdate_local_id,shift_startdate_id,shift_startinterval_id, schedule_date_id, interval_id, person_id, scheduled_time_m, scheduled_ready_time_m, scheduled_contract_time_m)
SELECT	fs.shift_startdate_local_id,
		fs.shift_startdate_id,
		fs.shift_startinterval_id,
		fs.schedule_date_id, 
		fs.interval_id, 
		fs.person_id, 
		SUM(fs.scheduled_time_m), 
		SUM(fs.scheduled_ready_time_m), 
		SUM(fs.scheduled_contract_time_m)
FROM mart.fact_schedule fs WITH (NOLOCK)
INNER JOIN #person p
	ON fs.person_id=p.person_id 
	AND shift_startdate_local_id between p.valid_from_date_id_local AND p.valid_to_date_id_local
WHERE fs.shift_startdate_local_id BETWEEN  @date_from_id -1 AND @date_to_id +1
AND fs.schedule_date_id BETWEEN @date_from_id AND @date_to_id
AND fs.scenario_id = @scenario_id
GROUP BY fs.shift_startdate_local_id,fs.shift_startdate_id,fs.shift_startinterval_id,fs.schedule_date_id, fs.interval_id, fs.person_id

--Prepare result
INSERT #pre_result_subSP(date_id,interval_id,person_id,answered_calls,talk_time_s,after_call_work_time_s,ready_time_s, handling_time_s)
SELECT	date_id,
		interval_id, 
		person_id,
		answered_calls, 
		talk_time_s, 
		after_call_work_time_s, 
		ready_time_s,
		ISNULL(talk_time_s,0) + ISNULL(after_call_work_time_s,0)
FROM #agent_queue_statistics_subSP

INSERT #pre_result_subSP(date_id,interval_id,person_id,scheduled_ready_time_m,scheduled_time_m,scheduled_contract_time_m)
SELECT	schedule_date_id,
		interval_id,
		person_id,
		scheduled_ready_time_m,
		scheduled_time_m,
		scheduled_contract_time_m
FROM #fact_schedule_subSP

--Add deviation and adherance figures
INSERT #pre_result_subSP(date_id,interval_id,person_id,adherence_calc_s,deviation_s)
SELECT	fs.schedule_date_id,
		fs.interval_id,
		fs.person_id,
		adherence_calc_s =
		CASE @adherence_id
			WHEN 1 THEN isnull(fs.scheduled_ready_time_m*60,0)
			WHEN 2 THEN isnull(fs.scheduled_time_m*60,0)
			WHEN 3 THEN isnull(fsd.contract_time_s,0)
		END,
		deviation_s		=
		CASE @adherence_id
			WHEN 1 THEN isnull(fsd.deviation_schedule_ready_s, 0)
			WHEN 2 THEN isnull(fsd.deviation_schedule_s, 0)
			WHEN 3 THEN isnull(fsd.deviation_contract_s, 0)
		END
	FROM #person a
	INNER JOIN #fact_schedule_subSP fs
	ON a.person_id = fs.person_id
	AND fs.shift_startdate_local_id BETWEEN a.valid_from_date_id_local AND a.valid_to_date_id_local
	LEFT JOIN #fact_schedule_deviation_subSP fsd
		ON fsd.person_id=fs.person_id
		AND fsd.date_id=fs.schedule_date_id
		AND fsd.interval_id=fs.interval_id
		AND fsd.shift_startdate_local_id=fs.shift_startdate_local_id
	INNER JOIN mart.bridge_time_zone b1 WITH (NOLOCK)
		ON	fs.shift_startinterval_id= b1.local_interval_id
		AND fs.shift_startdate_id=b1.local_date_id
	INNER JOIN #bridge_time_zone b2
		ON	fs.interval_id= b2.interval_id
		AND fs.schedule_date_id= b2.date_id
	INNER JOIN mart.dim_interval i
		ON b2.local_interval_id = i.interval_id			
	INNER JOIN mart.dim_date d WITH (NOLOCK)
		ON b2.local_date_id = d.date_id
	AND b1.time_zone_id = @time_zone_id

--include intervals that have ready_time but no scheduled time. 
INSERT #pre_result_subSP(date_id,interval_id,person_id,adherence_calc_s,deviation_s)
SELECT	fsd.date_id,
		fsd.interval_id,
		fsd.person_id,
		adherence_calc_s =
		CASE @adherence_id 
			WHEN 1 THEN 0
			WHEN 2 THEN 0
			WHEN 3 THEN isnull(fsd.contract_time_s,0)
		END,
		deviation_s		=
		CASE @adherence_id
			WHEN 1 THEN isnull(fsd.deviation_schedule_ready_s, 0)
			WHEN 2 THEN isnull(fsd.deviation_schedule_s, 0)
			WHEN 3 THEN isnull(fsd.deviation_contract_s, 0)
		END
FROM #person a
LEFT JOIN #fact_schedule_deviation_subSP fsd
	ON a.person_id = fsd.person_id
	AND fsd.shift_startdate_local_id BETWEEN a.valid_from_date_id_local AND a.valid_to_date_id_local
INNER JOIN #bridge_time_zone b1
	ON	fsd.shift_startinterval_id= b1.interval_id
	AND fsd.shift_startdate_id=b1.date_id
INNER JOIN bridge_time_zone b2 WITH (NOLOCK)
	ON	fsd.interval_id= b2.interval_id
	AND fsd.date_id= b2.date_id
INNER JOIN mart.dim_interval i WITH (NOLOCK)
	ON b2.local_interval_id = i.interval_id			
INNER JOIN mart.dim_date d WITH (NOLOCK)
	ON b2.local_date_id = d.date_id
AND b2.time_zone_id=@time_zone_id
WHERE fsd.date_id BETWEEN @date_from_id AND @date_to_id				
AND NOT EXISTS (SELECT 1 FROM #fact_schedule_subSP fs WHERE fsd.person_id=fs.person_id	AND fsd.date_id=fs.schedule_date_id
				AND fsd.interval_id=fs.interval_id)

UPDATE #pre_result_subSP
SET
	team_id			 = p.team_id
FROM #pre_result_subSP r
INNER JOIN #person_acd_subSP p
	ON p.person_id=r.person_id

RETURN 0
GO