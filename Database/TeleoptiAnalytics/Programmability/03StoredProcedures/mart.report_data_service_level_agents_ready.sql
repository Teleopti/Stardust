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
-- Description:	Used by report Service level and Agents Ready
-- =============================================
--exec report_data_service_level_agents_ready @skill_set=N'2',@workload_set=N'1',@interval_type=N'1',@date_from='2006-01-04 00:00:00:000',@date_to='2006-01-04 00:00:00:000',@interval_from=N'0',@interval_to=N'287',@sl_calc_id=3,@time_zone_id=81, @person_code='7EB3E1AE-3DF5-4558-81CF-9AB100258670',@report_id=9,@language_id=1053

CREATE PROCEDURE [mart].[report_data_service_level_agents_ready] 
@skill_set nvarchar(max),
@workload_set nvarchar(max),
@interval_type int, --anger vilket intervall rapporten ska grupperas pÃ¥ : lÃ¤gsta intervall/halvtimme/timme/dag/vecka/mÃ¥nad
@date_from datetime,
@date_to datetime,
@interval_from int,--mellan vilka tider
@interval_to int,
@sl_calc_id int,
@time_zone_id int,
@person_code uniqueidentifier,
@report_id int,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON


/*Split string of skill id:s*/
CREATE TABLE #skills(id int)
INSERT INTO #skills
SELECT * FROM mart.SplitStringInt(@skill_set)

/*Split string of workload id:s*/
CREATE TABLE #workloads(id int)
INSERT INTO #workloads
SELECT * FROM mart.SplitStringInt(@workload_set)

/*GET NUMBER OF MINUTES PER INTERVAL*/
/*DECLARE @minutes int
SET @minutes= (SELECT 1440/COUNT(*) FROM dim_interval)
/*GET NUMBER OF INTERVALS PER DAY*/
DECLARE @intervals int
SET @intervals=(SELECT COUNT(*) FROM mart.dim_interval)
*/

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0


/*Get */
CREATE TABLE #queue( period nvarchar(30),
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
INSERT #queue(period,calls_offered,calls_answered,calls_answ_after_sl,calls_answ_within_sl,calls_abnd,calls_abnd_within_sl,answ_rate,abnd_rate,service_level,service_level_numerator,service_level_denominator)
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
	CASE WHEN sum(answered_calls+abandoned_calls)=0 THEN 0
		ELSE
		(sum(fq.answered_calls)/sum(answered_calls+abandoned_calls))
		END AS 'answ_rate',
	CASE WHEN sum(answered_calls+abandoned_calls)=0 THEN 0
		ELSE
		(sum(fq.abandoned_calls)/sum(answered_calls+abandoned_calls))
		END AS 'abnd_rate',
		0 as 'service_level',
		0 as 'service_level_numerator',
		0 as'service_level_denominator'
FROM mart.fact_queue fq
INNER JOIN 
	mart.dim_queue dq 
	ON dq.queue_id=fq.queue_id
INNER JOIN 
	mart.bridge_queue_workload bq ON
	bq.queue_id=dq.queue_id
INNER JOIN 
	mart.bridge_time_zone b
	ON	fq.interval_id= b.interval_id
	AND fq.date_id= b.date_id
INNER JOIN 
	mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN 
	mart.dim_interval i
	ON b.local_interval_id = i.interval_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND i.interval_id BETWEEN @interval_from AND  @interval_to
AND b.time_zone_id = @time_zone_id
AND bq.skill_id IN (select id from #skills)
AND bq.workload_id IN (SELECT id from #workloads)
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

--SELECT * FROM #queue
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
CREATE TABLE #scheduled_agents( period nvarchar(30),
				scheduled_agents_ready decimal(18,3))

INSERT #scheduled_agents(period,scheduled_agents_ready)
SELECT	CASE @interval_type 
			WHEN 1 THEN i.interval_name
			WHEN 2 THEN i.halfhour_name
			WHEN 3 THEN i.hour_name
			WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--mÃ¥ste vara enligt den kultur som anvÃ¤nds
			WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
			WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
			WHEN 7 THEN d.weekday_resource_key
		END AS 'period',
		COUNT(DISTINCT dp.person_code)	'scheduled_agents_ready'
FROM mart.fact_schedule fs
INNER JOIN [mart].[DimPersonLocalized](@date_from, @date_to) dpl
	ON dpl.person_id = fs.person_id
INNER JOIN mart.dim_person dp 
	ON dp.person_id = dpl.person_id
INNER JOIN mart.bridge_skillset_skill bs 
	ON dp.skillset_id = bs.skillset_id
INNER JOIN mart.dim_workload dw 
	ON dw.skill_id = bs.skill_id
INNER JOIN mart.bridge_time_zone b
	ON	fs.interval_id = b.interval_id
	AND fs.schedule_date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
INNER JOIN mart.dim_scenario s
           ON s.scenario_id = fs.scenario_id AND s.default_scenario = 1
WHERE fs.scheduled_ready_time_m>0
	AND d.date_date BETWEEN @date_from AND @date_to
	AND i.interval_id BETWEEN @interval_from AND @interval_to
	AND b.time_zone_id = @time_zone_id
	AND dw.skill_id IN (select id from #skills)
	AND dw.workload_id IN (select id from #workloads)
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
CREATE TABLE #ready_agents( period nvarchar(30),
				agents_ready decimal(18,3))

INSERT #ready_agents(period,agents_ready)
SELECT	CASE @interval_type 
			WHEN 1 THEN i.interval_name
			WHEN 2 THEN i.halfhour_name
			WHEN 3 THEN i.hour_name
			WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--mÃ¥ste vara enligt den kultur som anvÃ¤nds
			WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
			WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
			WHEN 7 THEN d.weekday_resource_key
		END AS 'period',
		COUNT (DISTINCT dp.person_code) 'agents_ready'
FROM mart.fact_agent fa
INNER JOIN mart.bridge_acd_login_person bap 
	ON fa.acd_login_id=bap.acd_login_id
INNER JOIN [mart].[DimPersonLocalized](@date_from, @date_to) dpl
	ON dpl.person_id = bap.person_id
INNER JOIN mart.dim_person dp 
	ON dp.person_id = dpl.person_id
INNER JOIN mart.bridge_skillset_skill bs 
	ON dp.skillset_id = bs.skillset_id
INNER JOIN mart.dim_workload dw 
	ON dw.skill_id = bs.skill_id
INNER JOIN mart.bridge_time_zone b
	ON	fa.interval_id = b.interval_id
	AND fa.date_id = b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
WHERE fa.ready_time_s>0
AND d.date_date BETWEEN @date_from AND @date_to
AND i.interval_id BETWEEN @interval_from AND @interval_to
AND b.time_zone_id = @time_zone_id
AND dw.skill_id IN (SELECT id FROM #skills)
AND dw.workload_id IN (SELECT id FROM #workloads)
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


CREATE TABLE #result(period nvarchar(30),
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
				weekday_number int
				)
INSERT #result(period,hide_time_zone,interval_type)
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
		@interval_type as interval_type
FROM  mart.bridge_time_zone b 
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND i.interval_id BETWEEN @interval_from AND @interval_to
AND b.time_zone_id = @time_zone_id
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
	weekday_number
FROM #result
ORDER BY weekday_number,period

GO

