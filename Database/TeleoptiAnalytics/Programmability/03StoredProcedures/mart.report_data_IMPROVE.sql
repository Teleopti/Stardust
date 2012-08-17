IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_IMPROVE]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_IMPROVE]
GO

-- =============================================
-- Author:		<KJ>
-- Create date: <2009-01-12>
-- Update date: <2009-04-14>
-- 20090414 added collate database_default on joins with strings KJ
-- 20090211 Added new mart schema KJ
-- 20090302 Excluded timezone UTC from time_zone check KJ	
-- 2010-11-01 #11055, refactor of mart.fact_schedule_deviation KJ
-- 2011-01-27 #12810, major refactor of Adherence and person_code DJ+ME
-- 2011-09-27 #16079, Division by Zero
-- 2012-01-09 Passed BU to ReportAgents
--	2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	<IMPROVE report>
-- =============================================
-- Todo: remove scenario from input??
-- Todo: Currently we get all skills, regardless of person/team. => we get to much queue stats!
-- =============================================
--exec mart.report_data_IMPROVE @scenario_id=N'0',@skill_set=N'2,7,3',@workload_set=N'4,8,9,2',@interval_type=N'4',@date_from='2009-02-03 00:00:00',@date_to='2009-02-03 00:00:00',@interval_from=N'0',@interval_to=N'95',@group_page_code=N'd5ae2a10-2e17-4b3c-816c-1a0e81cd767c',@group_page_group_set=NULL,@site_id=N'1',@team_set=N'5',@adherence_id=N'1',@sl_calc_id=N'1',@time_zone_id=N'2',@person_code='18037D35-73D5-4211-A309-9B5E015B2B5C',@report_id=3,@language_id=1053

CREATE PROCEDURE [mart].[report_data_IMPROVE]
@scenario_id int,
@skill_set nvarchar(max),
@workload_set nvarchar(max),
@interval_type int, --anger vilket intervall rapporten ska grupperas pa : lagsta intervall/halvtimme/timme/dag/vecka/manad
@date_from datetime,
@date_to datetime,
@interval_from int,--mellan vilka tider
@interval_to int,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@site_id int,
@team_set nvarchar(max),
@adherence_id int,--1,2 eller 3 fran adherence_calculation tabellen
@sl_calc_id int,
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
BEGIN
           
SET NOCOUNT ON;

--Create Temp tables used
CREATE TABLE #workloads(id int)
CREATE TABLE #skills(id int)
CREATE TABLE #queues(queue_id int)
CREATE TABLE #rights_agents (right_id int)
CREATE TABLE #rights_teams (right_id int)

CREATE TABLE #person_acd_subSP
	(
	person_id int,
	acd_login_id int
	)

/*
CREATE TABLE #schedule_deviation(
			period nvarchar(30),
			deviation_s decimal(18,4)
			)
*/
			
CREATE TABLE #pre_result(
	date_id int,
	interval_id int,
	scheduled_paid_time_s int,
	calls_offered int,
	calls_answered int,
	calls_answ_within_sl int,
	calls_abnd_within_sl int,
	talk_time_s int,
	after_call_work_time_s int,
	scheduled_ready_time_m int,
	scheduled_time_m int,
	scheduled_contract_time_m int,
	adherence_calc_s decimal(18,3),
	deviation_s decimal(18,3)
	)
CREATE TABLE #fact_schedule
	(
	schedule_date_id int,
	date smalldatetime,
	interval_id int,
	person_id int,
	scheduled_ready_time_m int,
	scheduled_time_m int,
	scheduled_contract_time_m int,
	scheduled_paid_time_m int
	)
        
CREATE TABLE #result(
           period nvarchar(30),
           scheduled_paid_time_s int,
           handling_time_s int,
           handling_time_per_scheduled_paid_time  decimal(18,3),
           calls_offered int,
           calls_answered int,
           calls_answ_within_sl int,
           calls_abnd_within_sl int,
           service_level decimal(18,3),
           service_level_numerator decimal(18,3), --täljare
           service_level_denominator decimal(18,3), --nämnare    
           adherence_calc_s decimal(18,3),
           deviation_s decimal(18,3),
           adherence decimal(18,3),
           hide_time_zone bit,
           interval_type int,
           weekday_number int
           )
                                            
CREATE TABLE #bridge_time_zone
	(
	local_date_id int,
	local_interval_id int,
	date_id int,
	interval_id int
	)
	
/*
CREATE TABLE #fact_schedule_deviation (
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
*/

--Local datetime data	
CREATE TABLE #pre_result_subSP
	(
	date_id int,
	date_date smalldatetime,
	interval_id int,
	acd_login_id int,
	person_id int,
	team_id int,
	answered_calls int DEFAULT 0,		
	talk_time_s decimal(20,2),
	after_call_work_time_s decimal(20,2),
	handling_time_s decimal(20,2),
	ready_time_s int DEFAULT 0,
	scheduled_ready_time_m int DEFAULT 0,
	scheduled_time_m int,
	scheduled_contract_time_m int,
	deviation_s decimal(18,3),
	adherence_calc_s decimal(18,3)	
	)


---------
--DECLARE
---------           
DECLARE @date TABLE (date_from_id INT, date_to_id INT)	 
DECLARE @hide_time_zone bit
DECLARE @intervals_length_m INT
DECLARE @intervals_per_day INT
DECLARE @group_page_agent_code uniqueidentifier
DECLARE @agent_person_code uniqueidentifier

---------
--INIT
---------
SET @group_page_agent_code = '00000000-0000-0000-0000-000000000002'
SET @agent_person_code = '00000000-0000-0000-0000-000000000002'

--Check if time zone will be hidden (if only one exist then hide)
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
           SET @hide_time_zone = 1
ELSE
           SET @hide_time_zone = 0
--init
--SELECT @scenario_id = scenario_id FROM mart.dim_scenario WHERE default_scenario = 1
SET @intervals_length_m = 1
SELECT @intervals_per_day = COUNT(interval_id) FROM mart.dim_interval
IF @intervals_per_day > 0 SELECT @intervals_length_m = 1440/@intervals_per_day

--Get all agents/persons that user has permission to see in given period
INSERT INTO #rights_agents
SELECT * FROM mart.ReportAgentsMultipleTeams(@date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_person_code, @person_code, @report_id, @business_unit_code)

--Join the ResultSets above as:
--a) teams allowed = #rights_teams
--b) agent allowed = #rights_agents
--c) selected = #agents
INSERT INTO #person_acd_subSP
SELECT a.right_id, acd.acd_login_id
FROM #rights_agents a
LEFT JOIN mart.bridge_acd_login_person acd
	ON acd.person_id = a.right_id

/*Split string of skill id:s*/
INSERT INTO #skills
SELECT * FROM mart.SplitStringInt(@skill_set)

/*Split string of workload id:s*/
INSERT INTO #workloads
SELECT * FROM mart.SplitStringInt(@workload_set)

/* Get the queues matching selected skills and workloads */
INSERT INTO #queues (queue_id)
SELECT DISTINCT bqw.queue_id
FROM mart.bridge_queue_workload bqw
WHERE skill_id IN (SELECT id FROM #skills)
	AND workload_id IN (SELECT id FROM #workloads)

--Get needed dates and intervals from bridge time zone into temp table
INSERT INTO #bridge_time_zone
SELECT 
	local_date_id		= d.date_id,
	local_interval_id	= i.interval_id,
	date_id				= b.date_id,
	interval_id			= b.interval_id
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
	FROM #bridge_time_zone b
	INNER JOIN mart.dim_date d 
		ON b.local_date_id = d.date_id
	WHERE	d.date_date	between @date_from AND @date_to

--Create UTC table from: mart.fact_schedule_deviation
/*
INSERT INTO #fact_schedule_deviation
SELECT 
	fsd.date_id,
	fsd.person_id,
	fsd.interval_id,
	deviation_schedule_ready_s,
	deviation_schedule_s,
	deviation_contract_s,
	ready_time_s,
	is_logged_in,
	contract_time_s
FROM mart.fact_schedule_deviation fsd
INNER JOIN #person_acd_subSP a
	ON fsd.person_id = a.person_id
INNER JOIN #bridge_time_zone b
	ON	fsd.interval_id= b.interval_id
	AND fsd.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
*/

--Create UTC table from: mart.fact_schedule
INSERT INTO #fact_schedule(schedule_date_id, interval_id, person_id, scheduled_time_m, scheduled_ready_time_m, scheduled_contract_time_m, scheduled_paid_time_m)
SELECT fs.schedule_date_id, 
		fs.interval_id, 
		fs.person_id, 
		fs.scheduled_time_m, 
		fs.scheduled_ready_time_m, 
		fs.scheduled_contract_time_m,
		fs.scheduled_paid_time_m
FROM mart.fact_schedule fs
INNER JOIN #person_acd_subSP a
	ON fs.person_id = a.person_id
INNER JOIN #bridge_time_zone b
	ON	fs.interval_id= b.interval_id
	AND fs.schedule_date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
AND fs.scenario_id=@scenario_id


--This SP will insert Adherence data into table: #pre_result_subSP
EXEC [mart].[report_data_schedule_result_subSP]
	@date_from		= @date_from,
	@date_to		= @date_to,
	@interval_from	= @interval_from,
	@interval_to	= @interval_to,
	@adherence_id	= @adherence_id,
	@time_zone_id	= @time_zone_id,
	@person_code	= @person_code,
	@report_id		= @report_id,
	@scenario_id	= @scenario_id,	
	@language_id	= @language_id

--Now group/sum #pre_result_subSP into #pre_result
INSERT INTO #pre_result(date_id,interval_id,adherence_calc_s,deviation_s)
SELECT date_id,interval_id,SUM(adherence_calc_s),SUM(deviation_s)
FROM #pre_result_subSP
GROUP BY date_id, interval_id

/*
--Add [adherence_calc_m] if we are outside scheduled time
IF @adherence_id=2
BEGIN

	-------------
	--adherence_cals_s
	-------------
	--fake schedule_time_s for all intervals _outside_ the shift.
	--Set fake = @interval_length. Later used as adherance_cals_s
	--#pre_result = Local datetime table
	
	SELECT
		date_id				= d.date_id,
		interval_id			= i.interval_id,
		adherence_calc_s	= sum(@intervals_length_m)  --one row per person x interval_length
		FROM #fact_schedule_deviation fsd
	INNER JOIN #bridge_time_zone b
		ON	fsd.interval_id = b.interval_id
		AND fsd.date_id = b.date_id
	INNER JOIN mart.dim_date d 
		ON b.local_date_id = d.date_id
		AND d.date_date BETWEEN @date_from AND @date_to
	INNER JOIN mart.dim_interval i
		ON b.local_interval_id = i.interval_id
		AND i.interval_id BETWEEN @interval_from AND @interval_to
	WHERE NOT EXISTS
		--do the compare on UTC
		(SELECT 1 FROM #fact_schedule fs
		WHERE fsd.person_id=fs.person_id
		AND fsd.date_id=fs.schedule_date_id
		AND fsd.interval_id=fs.interval_id
		AND fsd.person_id = fs.person_id)
	GROUP BY d.date_id, i.interval_id --Group by Local date_id,interval		

	-------------
	--deviation_s
	-------------
	INSERT #schedule_deviation(period,deviation_s)  --Local datetime table
	SELECT	CASE @interval_type 
			WHEN 1 THEN i.interval_name  --d. and i. are local dates/intervals!
			WHEN 2 THEN i.halfhour_name
			WHEN 3 THEN i.hour_name		
			WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
			WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
			WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
			WHEN 7 THEN d.weekday_resource_key
			END AS 'period',	
			CASE @adherence_id 
				WHEN 1 THEN SUM(ISNULL(deviation_schedule_ready_s,0))
				WHEN 2 THEN SUM(ISNULL(deviation_schedule_s,0)) 
				WHEN 3 THEN SUM(ISNULL(deviation_contract_s,0))
			END AS 'deviation_s'
	FROM #fact_schedule_deviation fsd --UTC
	INNER JOIN #bridge_time_zone b
		ON	fsd.interval_id= b.interval_id
		AND fsd.date_id= b.date_id
	INNER JOIN mart.dim_date d 
		ON b.local_date_id = d.date_id
		AND d.date_date BETWEEN @date_from AND @date_to
	INNER JOIN mart.dim_interval i
		ON b.local_interval_id = i.interval_id
		AND i.interval_id BETWEEN @interval_from AND @interval_to
	WHERE NOT EXISTS
		--do the compare on UTC
		(SELECT 1 FROM #fact_schedule fs
		WHERE fsd.person_id=fs.person_id
		AND fsd.date_id=fs.schedule_date_id
		AND fsd.interval_id=fs.interval_id
		AND fsd.person_id = fs.person_id)
	GROUP BY
		CASE @interval_type 
		WHEN 1 THEN i.interval_name
			WHEN 2 THEN i.halfhour_name
			WHEN 3 THEN i.hour_name
			WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--mÃ¥ste vara enligt den kultur som anvÃ¤nds
			WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
			WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
			WHEN 7 THEN d.weekday_resource_key
		END
	ORDER BY 
			CASE @interval_type 
			WHEN 1 THEN i.interval_name
			WHEN 2 THEN i.halfhour_name
			WHEN 3 THEN i.hour_name
			WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--mÃ¥ste vara enligt den kultur som anvÃ¤nds
			WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
			WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
			WHEN 7 THEN d.weekday_resource_key
			END
END

-------------
--adherence_cals_s
-------------
--inside shift
--#pre_result = Local datetime table
INSERT INTO #pre_result(date_id,interval_id,adherence_calc_s)
SELECT
	date_id				= d.date_id,
	interval_id			= i.interval_id,
	CASE @adherence_id 
		WHEN 1 THEN sum(isnull(fs.scheduled_ready_time_m,0))*60
		WHEN 2 THEN sum(isnull(fs.scheduled_time_m,0))*60
		WHEN 3 THEN sum(isnull(fs.scheduled_contract_time_m,0))*60
	END AS 'adherence_calc_s'
	FROM #fact_schedule_deviation fsd
LEFT JOIN #fact_schedule fs
	ON fsd.person_id=fs.person_id
	AND fsd.date_id=fs.schedule_date_id
	AND fsd.interval_id=fs.interval_id
INNER JOIN #bridge_time_zone b
	ON	fsd.interval_id = b.interval_id
	AND fsd.date_id = b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND @interval_to
GROUP BY d.date_id, i.interval_id --Group by Local date_id,interval		
*/

--Sum agent schedule, local datetime
INSERT INTO #pre_result (date_id,interval_id,scheduled_time_m,scheduled_contract_time_m,scheduled_ready_time_m,scheduled_paid_time_s)
SELECT
	d.date_id, --local date
	i.interval_id, --local interval
	sum(fs.scheduled_time_m),
	sum(fs.scheduled_contract_time_m),
	sum(fs.scheduled_ready_time_m),
	sum(fs.scheduled_paid_time_m * 60) --we want seconds here
FROM #fact_schedule fs
	INNER JOIN #bridge_time_zone b
		ON	fs.interval_id= b.interval_id
		AND fs.schedule_date_id= b.date_id
	INNER JOIN mart.dim_date d 
		ON b.local_date_id = d.date_id
	INNER JOIN mart.dim_interval i
		ON b.local_interval_id = i.interval_id
		AND i.interval_id BETWEEN @interval_from AND @interval_to				
GROUP BY d.date_id, i.interval_id --Group by Local date_id,interval

-- fact_agent_queue, local datetime
INSERT INTO #pre_result (date_id,interval_id,talk_time_s,after_call_work_time_s)
SELECT 
	d.date_id, --local date
	i.interval_id, --local interval
	sum(faq.talk_time_s),
	sum(faq.after_call_work_time_s)
FROM mart.fact_agent_queue faq
INNER JOIN #person_acd_subSP al
	ON faq.acd_login_id = al.acd_login_id
INNER JOIN #bridge_time_zone b
	ON	faq.interval_id = b.interval_id
	AND faq.date_id = b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND @interval_to
GROUP BY d.date_id, i.interval_id --Group by Local date_id,interval

-- fact_queue, local datetime
INSERT INTO #pre_result (date_id,interval_id,calls_offered,calls_answered,calls_answ_within_sl,calls_abnd_within_sl)
SELECT 
	d.date_id, --local date
	i.interval_id, --local interval
	sum(fq.offered_calls),
	sum(fq.answered_calls),
	sum(fq.answered_calls_within_SL),
	sum(fq.abandoned_calls_within_SL)
FROM mart.fact_queue fq
INNER JOIN #queues q
	ON fq.queue_id = q.queue_id
INNER JOIN #bridge_time_zone b
	ON	fq.interval_id = b.interval_id
	AND fq.date_id = b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND @interval_to
GROUP BY d.date_id, i.interval_id --Group by Local date_id,interval

/*
SELECT 	ISNULL(SUM(pr.adherence_calc_s),0) as 'adherence_calc_s',		
		ISNULL(SUM(pr.deviation_s),0) as 'deviation_s',
		CASE
			WHEN ISNULL(SUM(pr.adherence_calc_s),0) = 0 THEN 1
			ELSE (SUM(pr.adherence_calc_s) - ISNULL(SUM(pr.deviation_s),0))/ SUM(pr.adherence_calc_s)
		END as 'adherence'
 FROm #pre_result pr
 GROUP BY date_id,interval_id
 
RETURN 0
*/

--Arrange the #result table, Local datetime table
INSERT INTO #result(period,scheduled_paid_time_s,handling_time_s,handling_time_per_scheduled_paid_time,calls_offered,calls_answered,calls_answ_within_sl,calls_abnd_within_sl,service_level,service_level_numerator,service_level_denominator,adherence_calc_s,deviation_s,adherence,hide_time_zone,interval_type,weekday_number)
SELECT	CASE @interval_type 
			WHEN 1 THEN i.interval_name
			WHEN 2 THEN i.halfhour_name
			WHEN 3 THEN i.hour_name
			WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
			WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
			WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
			WHEN 7 THEN d.weekday_resource_key
		END AS 'period',
		ISNULL(sum(pr.scheduled_paid_time_s),0) AS 'scheduled_paid_time_s',
		sum(pr.talk_time_s) + sum(pr.after_call_work_time_s) AS 'handling_time_s',
		CASE WHEN sum(pr.scheduled_paid_time_s)<=0 THEN 0
			ELSE (sum(pr.talk_time_s) + sum(pr.after_call_work_time_s))/CONVERT(float,sum(pr.scheduled_paid_time_s)) 
		END AS 'handling_time_per_scheduled_paid_time',
		sum(pr.calls_offered),
		sum(pr.calls_answered),
		sum(pr.calls_answ_within_sl),
		sum(pr.calls_abnd_within_sl),
		0 as 'service_level',
		0 as 'service_level_numerator',
		0 as'service_level_denominator',
		/*
		CASE @adherence_id 
			WHEN 1 THEN SUM(ISNULL(pr.scheduled_ready_time_m,0)*60)
			WHEN 2 THEN SUM(ISNULL(ISNULL(pr.adherence_calc_s, pr.scheduled_time_m),0)*60) --If we have no [scheduled_time_m] go for [adherence_calc_outside_shift_m]
			WHEN 3 THEN SUM(ISNULL(pr.scheduled_contract_time_m,0)*60)
		END AS 'adherence_calc_s', */
		ISNULL(SUM(pr.adherence_calc_s),0) as 'adherence_calc_s',		
		ISNULL(SUM(pr.deviation_s),0) as 'deviation_s',
		CASE
			WHEN ISNULL(SUM(pr.adherence_calc_s),0) = 0 THEN 1
			WHEN ISNULL(SUM(pr.deviation_s + pr.adherence_calc_s),0) = 0 THEN -1
			ELSE (SUM(pr.adherence_calc_s) - ISNULL(SUM(pr.deviation_s),0))/ SUM(pr.adherence_calc_s)
		END as 'adherence',
		@hide_time_zone as hide_time_zone,
		@interval_type as interval_type,
		0
FROM #pre_result pr
INNER JOIN mart.dim_date d 
	ON pr.date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON pr.interval_id = i.interval_id
GROUP BY
	CASE @interval_type 
	WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--mÃ¥ste vara enligt den kultur som anvÃ¤nds
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
	END
ORDER BY 
		CASE @interval_type 
		WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--mÃ¥ste vara enligt den kultur som anvÃ¤nds
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
		END

/* Adherence calculations */
/*
INSERT #schedule_deviation(period,deviation_s)  --Local datetime
SELECT	CASE @interval_type 
		WHEN 1 THEN i.interval_name  --d. and i. are local dates/intervals
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name		
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
		END AS 'period',	
		CASE @adherence_id 
			WHEN 1 THEN  SUM(ISNULL(deviation_schedule_ready_s,0))
			WHEN 2 THEN  sum(ISNULL(deviation_schedule_s,0)) 
			WHEN 3 THEN sum(ISNULL(deviation_contract_s,0))
		END AS 'deviation_s'
FROM #pre_result fsd --UTC
INNER JOIN #bridge_time_zone b
	ON	fsd.interval_id= b.interval_id
	AND fsd.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND @interval_to
WHERE EXISTS
		--do the compare on UTC
		(SELECT 1 FROM #fact_schedule fs
		WHERE fsd.person_id=fs.person_id
		AND fsd.date_id=fs.schedule_date_id
		AND fsd.interval_id=fs.interval_id
		AND fsd.person_id = fs.person_id)
GROUP BY
	CASE @interval_type 
	WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--mÃ¥ste vara enligt den kultur som anvÃ¤nds
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
	END
ORDER BY 
		CASE @interval_type 
		WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--mÃ¥ste vara enligt den kultur som anvÃ¤nds
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
		END

--SELECT * FROM #result

--Adherence calc
UPDATE #result
SET adherence =
	CASE
		WHEN ISNULL(r.adherence_calc_s,0) = 0 THEN 1
		ELSE (r.adherence_calc_s - ISNULL(d.deviation_s,0))/ r.adherence_calc_s
	END,
	deviation_s = ISNULL(d.deviation_s,0)
FROM #schedule_deviation d
RIGHT OUTER JOIN #result r 
ON r.period=d.period 
--SELECT * FROM #result r 
*/

/*Service Level Calculations*/
IF @sl_calc_id=1
	BEGIN
		UPDATE #result
		SET service_level=calls_answ_within_sl/convert(float,calls_offered),
			service_level_numerator=calls_answ_within_sl,
			service_level_denominator=convert(float,calls_offered)
		WHERE calls_offered>0
	END

IF @sl_calc_id=2
	BEGIN
		UPDATE #result
		set service_level=(calls_answ_within_sl+calls_abnd_within_sl)/convert(float,calls_offered),
			service_level_numerator=(calls_answ_within_sl+calls_abnd_within_sl),
			service_level_denominator=convert(float,calls_offered)
		WHERE calls_offered>0
	END

IF @sl_calc_id=3
	BEGIN
		UPDATE #result
		set service_level=calls_answ_within_sl/convert(float,calls_answered),
			service_level_numerator=calls_answ_within_sl,
			service_level_denominator=convert(float,calls_answered)
		WHERE calls_answered>0
	END

IF @interval_type=7
	BEGIN
		UPDATE #result
		SET weekday_number=d.weekday_number
		FROM mart.dim_date d INNER JOIN #result r ON d.weekday_resource_key=r.period collate database_default
		
		UPDATE #result
		SET period=term_language
		FROM mart.language_translation l 
		INNER JOIN #result r ON l.term_english=substring(r.period,13,len(r.period)) collate database_default
		AND l.language_id=@language_id
	END
	
--Return data to report
SELECT * FROM #result ORDER BY weekday_number

END


GO


