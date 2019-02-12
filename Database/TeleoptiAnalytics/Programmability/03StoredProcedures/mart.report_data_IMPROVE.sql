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
-- 2013-07-10 backed out of #23621
-- 2013-07-10 Fix #24119, same as #23621
-- 2014-02-14 Fix #26978 Incorrect handling/paid time when duplicate acd logins
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
@interval_type int, --group by: [interval/half hour/hour/day/week/month] : [1-7]
@date_from datetime,
@date_to datetime,
@interval_from int,--Time converted to intervals
@interval_to int,
@group_page_code uniqueidentifier,
@group_page_group_set nvarchar(max),
@site_id int,
@team_set nvarchar(max),
@adherence_id int,--1,2 or 3 lookup from adherence_calculation table
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
	person_code uniqueidentifier,
	team_id int, 
	acd_login_id int, 
	valid_from_date_id int,
	valid_from_interval_id int,
	valid_to_date_id_maxDate int,
	valid_to_interval_id_maxdate int,
	valid_from_date_id_local int,
	valid_to_date_id_local int
	)
			
CREATE TABLE #pre_result(
	date_id int,
	interval_id int,
	date_date smalldatetime,
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
	[shift_startdate_local_id][int] NOT NULL,
	schedule_date_id int,
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
           service_level_numerator decimal(18,3), --numerator
           service_level_denominator decimal(18,3), --denumerator    
           adherence_calc_s decimal(18,3),
           deviation_s decimal(18,3),
           adherence decimal(18,6),
           hide_time_zone bit,
           interval_type int,
           weekday_number int,
           [date] smalldatetime
           )
                                            
CREATE TABLE #bridge_time_zone
	(
	local_date_id int not null,
	local_interval_id int not null,
	date_id int not null,
	interval_id int not null,
	date_date_local smalldatetime not null
	)
	
--utc raw data	
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
DECLARE @hide_time_zone bit
DECLARE @intervals_length_m INT
DECLARE @intervals_per_day INT
DECLARE @group_page_agent_code uniqueidentifier
DECLARE @agent_person_code uniqueidentifier
DECLARE @date_from_id int
DECLARE @date_to_id int

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
EXEC mart.report_get_AgentsMultipleTeams @date_from, @date_to, @group_page_code, @group_page_group_set, @group_page_agent_code, @site_id, @team_set, @agent_person_code, @person_code, @report_id, @business_unit_code

INSERT INTO #person_acd_subSP
SELECT
	person_id	= a.right_id,
	person_code	= p.person_code,
	team_id		= p.team_id,
	acd_login_id= acd.acd_login_id,
	valid_from_date_id =p.valid_from_date_id,
	valid_from_interval_id =p.valid_from_interval_id,
	valid_to_date_id_maxDate =p.valid_to_date_id_maxDate,
	valid_to_interval_id_maxdate =p.valid_to_interval_id_maxdate,
	valid_from_date_id_local=p.valid_from_date_id_local,
	valid_to_date_id_local=p.valid_to_date_id_local
FROM #rights_agents a
INNER JOIN mart.dim_person p WITH (NOLOCK)
	on p.person_id = a.right_id
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

--Get the min/max UTC date_id. Expand UTC -1 +1 when fetching fact_tables (subSP
SELECT 
	@date_from_id=MIN(b.date_id),
	@date_to_id=MAX(b.date_id)
FROM #bridge_time_zone b
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
WHERE	d.date_date	between @date_from AND @date_to

SELECT
	@date_from_id = date_id 
	FROM mart.dim_date
	WHERE date_date = (
		SELECT DATEADD(d, -1, date_date)
		FROM mart.dim_date
		WHERE date_id = @date_from_id)

SELECT
	@date_to_id = date_id 
	FROM mart.dim_date
	WHERE date_date = (
		SELECT DATEADD(d, 1, date_date)
		FROM mart.dim_date
		WHERE date_id = @date_to_id)

--Create UTC table from: mart.fact_schedule
INSERT INTO #fact_schedule(shift_startdate_local_id,schedule_date_id, interval_id, person_id, scheduled_time_m, scheduled_ready_time_m, scheduled_contract_time_m, scheduled_paid_time_m)
SELECT	fs.shift_startdate_local_id,
		fs.schedule_date_id, 
		fs.interval_id, 
		fs.person_id, 
		fs.scheduled_time_m, 
		fs.scheduled_ready_time_m, 
		fs.scheduled_contract_time_m,
		fs.scheduled_paid_time_m
FROM mart.fact_schedule fs WITH (NOLOCK)
INNER JOIN #person_acd_subSP p
	ON p.person_id=fs.person_id
	AND fs.shift_startdate_local_id BETWEEN p.valid_from_date_id_local AND p.valid_to_date_id_local
WHERE fs.shift_startdate_local_id between @date_from_id and @date_to_id	--we use the expanded utc (-1,+1) from above when filtering on shift_startdate_local_id
AND fs.scenario_id=@scenario_id

--This SP will insert Adherence data into table: #pre_result_subSP
EXEC [mart].[report_data_schedule_result_subSP]
	@date_from_id	= @date_from_id,
	@date_to_id		= @date_to_id,
	@interval_from	= @interval_from,
	@interval_to	= @interval_to,
	@adherence_id	= @adherence_id,
	@time_zone_id	= @time_zone_id,
	@person_code	= @person_code,
	@report_id		= @report_id,
	@scenario_id	= @scenario_id,	
	@language_id	= @language_id

--Now group/sum #pre_result_subSP into #pre_result
INSERT INTO #pre_result(date_id,interval_id,adherence_calc_s,deviation_s, talk_time_s,after_call_work_time_s)
SELECT date_id,interval_id,SUM(adherence_calc_s),SUM(deviation_s), sum(talk_time_s),sum(after_call_work_time_s)
FROM #pre_result_subSP
GROUP BY date_id, interval_id

--Sum agent schedule, local datetime
INSERT INTO #pre_result (date_id,interval_id,scheduled_time_m,scheduled_contract_time_m,scheduled_ready_time_m,scheduled_paid_time_s)
SELECT
	fs.schedule_date_id, --utc date
	fs.interval_id, --utc interval
	sum(fs.scheduled_time_m),
	sum(fs.scheduled_contract_time_m),
	sum(fs.scheduled_ready_time_m),
	sum(fs.scheduled_paid_time_m * 60) --we want seconds here
FROM #fact_schedule fs
GROUP BY fs.schedule_date_id, fs.interval_id --Group by utc date_id,interval

-- fact_queue, utc datetime
INSERT INTO #pre_result (date_id,interval_id,calls_offered,calls_answered,calls_answ_within_sl,calls_abnd_within_sl)
SELECT 
	fq.date_id, --utc date
	fq.interval_id, --utc interval
	sum(fq.offered_calls),
	sum(fq.answered_calls),
	sum(fq.answered_calls_within_SL),
	sum(fq.abandoned_calls_within_SL)
FROM mart.fact_queue fq
INNER JOIN #queues q
	ON fq.queue_id = q.queue_id
WHERE fq.date_id between @date_from_id and @date_to_id
GROUP BY fq.date_id, fq.interval_id --Group by utc date_id,interval

--convert to local date
--switch utc date to local date
update #pre_result
set
	date_id		= btz.local_date_id,
	interval_id = btz.local_interval_id,
	date_date	= btz.date_date_local
from #bridge_time_zone btz
inner join #pre_result me
	on me.date_id = btz.date_id
	and me.interval_id = btz.interval_id

--delete data outside local dates
DELETE FROM #pre_result
WHERE date_date < @date_from
OR date_date > @date_to
OR date_date IS NULL

--Arrange the #result table, Local datetime table
INSERT INTO #result(period,scheduled_paid_time_s,handling_time_s,handling_time_per_scheduled_paid_time,calls_offered,calls_answered,calls_answ_within_sl,calls_abnd_within_sl,service_level,service_level_numerator,service_level_denominator,adherence_calc_s,deviation_s,adherence,hide_time_zone,interval_type,weekday_number,[date])
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
		ISNULL(SUM(pr.adherence_calc_s),0) as 'adherence_calc_s',		
		ISNULL(SUM(pr.deviation_s),0) as 'deviation_s',
		CASE
			WHEN ISNULL(SUM(pr.adherence_calc_s),0) = 0 THEN 1
			WHEN ISNULL(SUM(pr.deviation_s + pr.adherence_calc_s),0) = 0 THEN -1
			ELSE (SUM(pr.adherence_calc_s) - ISNULL(SUM(pr.deviation_s),0))/ SUM(pr.adherence_calc_s)
		END as 'adherence',
		@hide_time_zone as hide_time_zone,
		@interval_type as interval_type,
		0,
		min(d.date_date) as [date]
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
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--According to current culture
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
	END

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
SELECT * FROM #result ORDER BY weekday_number, period

END


GO


