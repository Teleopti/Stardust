IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_service_level_agents_ready]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_service_level_agents_ready]
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-06-03
-- Last Updated:20090414
-- 20090414 Added collate database_default KJ
-- 20090302 Excluded timezone UTC from time_zone check KJ
-- 20090211 Added new mart schema KJ
-- 20090115 Added @interval_type 7 weekday KJ
-- 200811231 Added fields
-- 20081009 Bug fix Date, Week and Month Formats changed KJ
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- 2012-09-17 Investigating Bug, CleanUp, SpeedUp
-- Description:	Used by report Service level and Agents Ready
-- =============================================
/*
set statistics time on
set statistics io on

exec mart.report_data_service_level_agents_ready @skill_set=N'18,169,79,8',@workload_set=N'18,187,68,8',@interval_type=4,@date_from='2016-11-07',@date_to='2016-11-07',
@interval_from=0,@interval_to=287,@sl_calc_id=3,@time_zone_id=2, @person_code='490C31C7-8C30-4CF6-86B0-9E1559B682D5',@report_id='AE758403-C16B-40B0-B6B2-E8F6043B6E04',@language_id=1053,
@business_unit_code='1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B'
*/
CREATE PROCEDURE [mart].[report_data_service_level_agents_ready] 
@skill_set nvarchar(max),
@workload_set nvarchar(max),
@interval_type int, --Group by this period type: interval/30 min/hour/day/week/month
@date_from datetime,
@date_to datetime,
@interval_from int,--intraday period
@interval_to int,
@sl_calc_id int,
@time_zone_id int,
@person_code uniqueidentifier, --Not used!
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON

--temp tables
CREATE TABLE #result(
	period nvarchar(30),
	calls_offered int,
	calls_answered int,
	calls_answ_after_sl int,
	calls_answ_within_sl int,
	calls_abnd int,
	scheduled_agents_ready decimal(18,3),
	agents_ready decimal(18,3),
	calls_offered_per_scheduled_agent decimal(18,3),
	calls_offered_per_ready_agent decimal(18,3),
	abnd_rate decimal(18,3),
	answ_rate decimal(18,3),
	service_level decimal(18,3),
	hide_time_zone bit,
	interval_type int,
	service_level_numerator decimal(18,3), --täljare
	service_level_denominator decimal(18,3), --nämnare
	weekday_number int,
	[date] smalldatetime
	)
CREATE TABLE #ready_agents(
	period nvarchar(30),
	agents_ready decimal(18,3)
	)
CREATE TABLE #bridge_time_zone(
	date_id int NOT NULL,
	interval_id smallint NOT NULL,
	local_date_id int NULL,
	local_interval_id smallint NULL
	)
CREATE TABLE #scheduled_agents_prepare(
	shift_startdate_local_id int,
	schedule_date_id int,
	interval_id smallint,
	scenario_id int,
	person_id int,
	person_code uniqueidentifier,
	skillset_id int,
	scheduled_ready_time_m int
	)
CREATE TABLE #scheduled_agents( 
	period nvarchar(30),
	scheduled_agents_ready decimal(18,3)
	)
CREATE TABLE #skills(id int)
CREATE TABLE #workloads(id int)
CREATE TABLE #queue( 
	period nvarchar(30),
	calls_offered int,
	calls_answered int,
	calls_answ_after_sl int,
	calls_answ_within_sl int,
	calls_abnd int,
	calls_abnd_within_sl int,
	abnd_rate decimal(18,3),
	answ_rate decimal(18,3),
	service_level decimal(18,3),
	service_level_numerator decimal(18,3), --täljare
	service_level_denominator decimal(18,3) --nämnare				
	)

--declare
DECLARE @hide_time_zone bit, @default_scenario_id int, @business_unit_id int, @date_from_id int, @date_to_id int,@date_from_id_minus1 int, @date_to_id_plus1 int

SET @hide_time_zone = 0

SELECT @business_unit_id = business_unit_id FROM mart.dim_business_unit WHERE business_unit_code = @business_unit_code

SELECT @default_scenario_id = scenario_id FROM mart.dim_scenario WHERE business_unit_id = @business_unit_id AND default_scenario = 1

SELECT @date_from_id = date_id FROM mart.dim_date WHERE date_date = @date_from
SELECT @date_to_id = date_id FROM mart.dim_date WHERE date_date = @date_to
SELECT @date_from_id_minus1 = date_id from mart.dim_date where date_date = DATEADD(d,-1,@date_from)--make sure get correct date if gaps in dim_date
SELECT @date_to_id_plus1 = date_id from mart.dim_date where date_date = DATEADD(d,1,@date_to) --make sure get correct date if gaps in dim_date
--SELECT @date_from_id,@date_to_id,@date_from_id_minus1,@date_to_id_plus1

/*Split string of skill id:s*/
INSERT INTO #skills
SELECT * FROM mart.SplitStringInt(@skill_set)

/*Split string of workload id:s*/
INSERT INTO #workloads
SELECT * FROM mart.SplitStringInt(@workload_set)

/* Check if time zone will be hidden (if only one exist then hide) */
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
SET @hide_time_zone = 1

--We will use this table a few times
INSERT INTO #bridge_time_zone
SELECT 
	b.date_id,
	b.interval_id,
	b.local_date_id,
	b.local_interval_id
FROM mart.bridge_time_zone b
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
	AND b.time_zone_id = @time_zone_id

/*Get queue info*/
INSERT #queue(
	period,
	calls_offered,
	calls_answered,
	calls_answ_after_sl,
	calls_answ_within_sl,
	calls_abnd,
	calls_abnd_within_sl,
	answ_rate,
	abnd_rate,
	service_level,
	service_level_numerator,
	service_level_denominator)
SELECT 
	CASE @interval_type 
		WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
	END AS 'period',
		sum(offered_calls),
		sum(answered_calls),
		sum(answered_calls)-sum(answered_calls_within_SL),
		sum(answered_calls_within_SL),
		sum(abandoned_calls),
		sum(abandoned_calls_within_SL),
	CASE WHEN sum(fq.offered_calls)=0 THEN 0
		ELSE
		(sum(fq.answered_calls)/convert(decimal(19,3),sum(fq.offered_calls)))
		END AS 'answ_rate',
	CASE WHEN sum(fq.offered_calls)=0 THEN 0
		ELSE
		(sum(fq.abandoned_calls)/convert(decimal(19,3),sum(fq.offered_calls)))
		END AS 'abnd_rate',
		0 as 'service_level',
		0 as 'service_level_numerator',
		0 as'service_level_denominator'
FROM mart.fact_queue fq
INNER JOIN 
	mart.dim_queue dq 
	ON dq.queue_id=fq.queue_id
INNER JOIN 
	mart.bridge_queue_workload bq
	ON bq.queue_id=dq.queue_id
	AND bq.workload_id IN (SELECT id from #workloads)
INNER JOIN 
	#bridge_time_zone b
	ON	fq.interval_id= b.interval_id
	AND fq.date_id= b.date_id
INNER JOIN 
	mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN 
	mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND  @interval_to
GROUP BY
	CASE @interval_type 
	WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
	END

ORDER BY 
		CASE @interval_type 
		WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
		END


/*Service Level Calculations*/
IF @sl_calc_id=1
BEGIN
	UPDATE #queue
	set service_level=calls_answ_within_sl/convert(float,calls_offered),
		service_level_numerator=calls_answ_within_sl,
		service_level_denominator=convert(float,calls_offered)
	WHERE calls_offered>0
END
IF @sl_calc_id=2
BEGIN
	UPDATE #queue
	set service_level=(calls_answ_within_sl+calls_abnd_within_sl)/convert(float,calls_offered),
		service_level_numerator=(calls_answ_within_sl+calls_abnd_within_sl),
		service_level_denominator=convert(float,calls_offered)
	WHERE calls_offered>0
END
IF @sl_calc_id=3
BEGIN
	UPDATE #queue
	set service_level=calls_answ_within_sl/convert(float,calls_answered),
		service_level_numerator=calls_answ_within_sl,
		service_level_denominator=convert(float,calls_answered)
	WHERE calls_answered>0
END


/*
Fix unreasonable values when:
a) We have over 100% because calls answered this interval was pegged as offered in previous interval.
b) We have calls answered but none offered. Best if this is displayed as 100%.

CAREFUL! How are totals calculated in the report? They use numerator and denominator! OK let's ignore that for now and assume
that when we pull enough intervals the totals will be OK anyway.
*/
UPDATE #queue
	set service_level = 1
WHERE service_level > 1

UPDATE #queue
	set service_level = 1
WHERE	service_level = 0
AND		calls_answ_within_sl > 0

/*Scheduled Agents Ready with selected skill(not necessarily working on that skill)*/
--This is common customer misstake: "I get more <Scheduled Agents Ready> in the report,
--than if I open the same skill and compare it to <Scheduled Agents> in the Scheduler?!"
--ByDesign:
--1) fact_schedule does not hold any info on how the Scheduler distribution over skills; i.e the forecast/need for each skill
--2) dim person does NOT hold any direct skill-info, but skill_set info.
--Thus for every person holding a Skillset that includes the selected Skill, we get a count=1 in fact_schedule
--Result: The report will show you: "How many agents would have been able to work on the particular skill, if necessary"
INSERT INTO #scheduled_agents_prepare (
	shift_startdate_local_id,
	schedule_date_id,
	interval_id,
	scenario_id,
	person_id,
	person_code,
	skillset_id,
	scheduled_ready_time_m)
SELECT 
	fs.shift_startdate_local_id, 
	fs.schedule_date_id,
	fs.interval_id, 
	fs.scenario_id, 
	fs.person_id, 
	dp.person_code,
	dp.skillset_id,
	fs.scheduled_ready_time_m
FROM mart.fact_schedule fs WITH (NOLOCK)
INNER JOIN mart.dim_person dp  WITH (NOLOCK)
	ON dp.person_id = fs.person_id
	AND fs.shift_startdate_local_id between dp.valid_from_date_id_local and dp.valid_to_date_id_local
WHERE shift_startdate_local_id between @date_from_id_minus1 and @date_to_id_plus1
	AND fs.scenario_id = @default_scenario_id
	AND fs.scheduled_ready_time_m > 0

INSERT #scheduled_agents(period,scheduled_agents_ready)
SELECT	CASE @interval_type 
			WHEN 1 THEN i.interval_name
			WHEN 2 THEN i.halfhour_name
			WHEN 3 THEN i.hour_name
			WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
			WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
			WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
			WHEN 7 THEN d.weekday_resource_key
		END AS 'period',
		COUNT(DISTINCT fs.person_code)	'scheduled_agents_ready'
FROM #scheduled_agents_prepare fs WITH (NOLOCK)
INNER JOIN mart.bridge_skillset_skill bs 
	ON fs.skillset_id = bs.skillset_id
INNER JOIN #skills s
	ON bs.skill_id = s.id
INNER JOIN #bridge_time_zone b
	ON fs.interval_id = b.interval_id
	AND fs.schedule_date_id = b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND @interval_to
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


/*Scheduled Agents Ready with selected skill(not necessarily working on that skill)*/
INSERT #ready_agents(period,agents_ready)
SELECT	CASE @interval_type 
			WHEN 1 THEN i.interval_name
			WHEN 2 THEN i.halfhour_name
			WHEN 3 THEN i.hour_name
			WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
			WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
			WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
			WHEN 7 THEN d.weekday_resource_key
		END AS 'period',
		COUNT (DISTINCT dp.person_code) 'agents_ready'
FROM mart.fact_agent fa
INNER JOIN mart.bridge_acd_login_person bap 
	ON fa.acd_login_id=bap.acd_login_id
INNER JOIN mart.dim_person dp  WITH (NOLOCK)
	ON dp.person_id = bap.person_id
INNER JOIN mart.bridge_skillset_skill bs 
	ON dp.skillset_id = bs.skillset_id
INNER JOIN mart.dim_workload dw 
	ON dw.skill_id = bs.skill_id
	AND dw.skill_id IN (SELECT id FROM #skills)
	AND dw.workload_id IN (SELECT id FROM #workloads)
INNER JOIN #bridge_time_zone b
	ON	fa.interval_id = b.interval_id
	AND fa.date_id = b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND @interval_to
WHERE fa.ready_time_s>0
AND d.date_id between dp.valid_from_date_id_local and valid_to_date_id_local
GROUP BY
	CASE @interval_type 
	WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
	END

INSERT #result(period,hide_time_zone,interval_type,[date])
SELECT	CASE @interval_type 
			WHEN 1 THEN i.interval_name
			WHEN 2 THEN i.halfhour_name
			WHEN 3 THEN i.hour_name
			WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
			WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
			WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
			WHEN 7 THEN d.weekday_resource_key
		END AS 'period',
		@hide_time_zone as hide_time_zone,
		@interval_type as interval_type,
		min(d.date_date) as [date]
FROM  #bridge_time_zone b 
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
	AND d.date_date BETWEEN @date_from AND @date_to
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
	AND i.interval_id BETWEEN @interval_from AND @interval_to
GROUP BY
	CASE @interval_type 
	WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
	END


UPDATE #result
SET calls_offered =#queue.calls_offered,
	calls_answered=#queue.calls_answered ,
	calls_answ_after_sl= #queue.calls_answ_after_sl,
	calls_answ_within_sl =#queue.calls_answ_within_sl,
	calls_abnd =#queue.calls_abnd,
	abnd_rate=#queue.abnd_rate,
	answ_rate=#queue.answ_rate,
	service_level=#queue.service_level,
	service_level_numerator = #queue.service_level_numerator,
	service_level_denominator = #queue.service_level_denominator
FROM #queue 
INNER JOIN #result 
	ON #result.period=#queue.period

UPDATE #result
SET scheduled_agents_ready=#scheduled_agents.scheduled_agents_ready
FROM #scheduled_agents
INNER JOIN #result 
	ON #result.period=#scheduled_agents.period

UPDATE #result
SET agents_ready=#ready_agents.agents_ready
FROM #ready_agents
INNER JOIN #result 
	ON #result.period=#ready_agents.period


UPDATE #result
SET calls_offered_per_scheduled_agent=calls_offered/scheduled_agents_ready
WHERE scheduled_agents_ready>0


UPDATE #result
SET calls_offered_per_ready_agent=calls_offered/agents_ready
WHERE agents_ready>0

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


SELECT 
	period,
	calls_offered,
	calls_answered,
	calls_answ_after_sl,
	calls_answ_within_sl,
	calls_abnd,
	ISNULL(scheduled_agents_ready,0) AS scheduled_agents_ready,
	ISNULL(agents_ready,0) AS agents_ready,
	ISNULL(calls_offered_per_scheduled_agent,0) AS calls_offered_per_scheduled_agent,
	ISNULL(calls_offered_per_ready_agent,0) AS calls_offered_per_ready_agent,
	abnd_rate,
	answ_rate,
	service_level,
	hide_time_zone,
	interval_type,
	service_level_numerator, --täljare
	service_level_denominator, --nämnare
	weekday_number,
	[date]
FROM #result
ORDER BY weekday_number,period

GO

