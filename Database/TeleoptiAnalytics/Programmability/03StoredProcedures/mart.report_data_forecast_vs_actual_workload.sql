IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_forecast_vs_actual_workload]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_forecast_vs_actual_workload]
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-06-09
-- Last Updated:20090414
-- 20090414 Added collate database_default KJ
-- 20090302 Excluded timezone UTC from time_zone check KJ
-- 20090211 Added new mart schema KJ
-- 20090115 Added @interval_type 7 weekday KJ
-- 20081008 Bug fix Date, Week and Month Formats changed KJ
-- 20080708 Added new id:s for period_type
-- 20080808 Added parameter time_zone_id and added connection to bridge_queue_workload KJ
-- 20100705 Added real calculation of calculated calls
-- 20110622 Azure fix DJ
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	Used by report Forecast vs Actual Workload
-- OBS! TODO: @time_zone_id, dateformat, check calculations with week and month selections,calculated calls not in db
-- =============================================
--exec report_data_forecast_vs_actual_workload @scenario_id=N'0', @skill_set=N'4',@workload_set=N'5',@interval_type=N'7',@date_from='2006-01-15 00:00:00:000',@date_to='2006-02-10 00:00:00:000',@interval_from=N'96',@interval_to=N'143',@person_code='10bbde88-ffc3-4f55-8396-9ab60024b7a9',@report_id=10,=1053
--exec mart.report_data_forecast_vs_actual_workload @scenario_id=N'0',@skill_set=N'0,3',@workload_set=N'5,0',@interval_type=N'5',@date_from='2006-01-01 00:00:00:000',@date_to='2006-01-14 00:00:00:000',@interval_from=N'0',@interval_to=N'287',@time_zone_id=N'1',@person_code='79C90699-C7C0-4D17-ADBF-9B2F0012C85A',@report_id=10,@language_id=1053,@business_unit_code='79C90699-C7C0-4D17-ADBF-9B2F0012C85A'

CREATE PROCEDURE [mart].[report_data_forecast_vs_actual_workload] 
@scenario_id int,
@skill_set nvarchar(max),
@workload_set nvarchar(max),
@interval_type int, --anger vilket intervall rapporten ska grupperas pa : lagsta intervall/halvtimme/timme/dag/vecka/manad/veckodag
@date_from datetime,
@date_to datetime,
@interval_from int,--mellan vilka tider
@interval_to int,
@time_zone_id int,
@person_code uniqueidentifier,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
--SET NOCOUNT ON
CREATE TABLE #skills(id int)
CREATE TABLE #workloads(id int)
CREATE TABLE #pre_result(
	date_id int,
	date smalldatetime,
	year_month int,
	year_week nvarchar(6),
	weekday_resource_key nvarchar(100),
	interval_id int,
	interval_name nvarchar(20),
	halfhour_name nvarchar(50),
	hour_name nvarchar(50),
	forecasted_calls decimal(28,4),
	calculated_calls decimal(28,4),
	offered_calls int,
	answered_calls int,
	talk_time_s int,
	acw_s int,
	forecast_talk_time_s int,
	forecast_acw_s int,
	hide_time_zone bit,
	interval_type int,
	weekday_number int)

CREATE TABLE #result(
	period nvarchar(30),
	forecasted_calls decimal(28,4),
	calculated_calls decimal(28,4),
	offered_calls int,
	answered_calls int,
	sum_talk_time_s int,
	sum_acw_s int,
	sum_forecast_talk_time_s int,
	sum_forecast_acw_s int,
	avg_forecasted_talk_time decimal(18,3),
	avg_actual_talk_time decimal(18,3),
	avg_forecasted_acw decimal(18,3),
	avg_actual_acw decimal(18,3),
	hide_time_zone bit,
	interval_type int,
	weekday_number int,
	[date] smalldatetime)
 
/*Split string of skill id:s*/
INSERT INTO #skills
SELECT * FROM mart.SplitStringInt(@skill_set)

/*Split string of workload id:s*/
INSERT INTO #workloads
SELECT * FROM mart.SplitStringInt(@workload_set)

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0

/* GET QUEUE DATA */
INSERT INTO #pre_result (date_id,date,year_month,year_week,weekday_resource_key,
						interval_id,interval_name,halfhour_name,hour_name,
						calculated_calls, offered_calls,answered_calls,talk_time_s,acw_s)
SELECT d.date_id,
		d.date_date as date,
		d.year_month,
		d.year_week,
		d.weekday_resource_key,
		i.interval_id,
		i.interval_name,
		i.halfhour_name,
		i.hour_name,
		mart.CalculateQueueStatistics(w.percentage_offered,
										w.percentage_overflow_in,
										w.percentage_overflow_out,
										w.percentage_abandoned,
										w.percentage_abandoned_short,
										w.percentage_abandoned_within_service_level,
										w.percentage_abandoned_after_service_level,
										fq.offered_calls,
										fq.abandoned_calls,
										fq.abandoned_calls_within_SL,
										fq.abandoned_short_calls,
										fq.overflow_out_calls,
										fq.overflow_in_calls),
		fq.offered_calls,
		fq.answered_calls,
		fq.talk_time_s,
		fq.after_call_work_s
FROM mart.fact_queue fq
INNER JOIN mart.bridge_queue_workload bqw
	ON fq.queue_id=bqw.queue_id
INNER JOIN mart.bridge_time_zone b
	ON	fq.interval_id= b.interval_id
	AND fq.date_id= b.date_id
INNER JOIN mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN mart.dim_interval i
	ON b.local_interval_id = i.interval_id	
INNER JOIN mart.dim_workload w
	ON bqw.workload_id = w.workload_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND i.interval_id BETWEEN @interval_from AND @interval_to
AND b.time_zone_id = @time_zone_id
AND bqw.skill_id IN (select id from #skills)
AND bqw.workload_id IN (select id from #workloads)
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


/* GET FORECAST DATA */
INSERT INTO #pre_result(date_id,date,year_month,year_week,weekday_resource_key,
					interval_id,interval_name,halfhour_name,hour_name,
					forecasted_calls,forecast_talk_time_s,forecast_acw_s,hide_time_zone,interval_type)
SELECT	d.date_id,
		d.date_date as date,
		d.year_month,
		d.year_week,
		d.weekday_resource_key,
		i.interval_id,
		i.interval_name,
		i.halfhour_name,
		i.hour_name,
		fw.forecasted_calls,
		forecasted_talk_time_s,
		forecasted_after_call_work_s,
		@hide_time_zone as hide_time_zone,
		@interval_type
FROM  
	mart.fact_forecast_workload fw
INNER JOIN 
	mart.bridge_time_zone b
	ON	fw.interval_id= b.interval_id
	AND fw.date_id= b.date_id
INNER JOIN 
	mart.dim_date d 
	ON b.local_date_id = d.date_id
INNER JOIN 
	mart.dim_interval i
	ON b.local_interval_id = i.interval_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND i.interval_id BETWEEN @interval_from AND @interval_to
AND b.time_zone_id = @time_zone_id
AND fw.skill_id IN (select id from #skills)
AND fw.workload_id IN (select id from #workloads)
AND fw.scenario_id=@scenario_id 
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


/* GROUP RESULT DATA */
INSERT INTO #result (period,
				forecasted_calls,
				calculated_calls,
				offered_calls,
				answered_calls,
				sum_talk_time_s,
				sum_acw_s,
				sum_forecast_talk_time_s,
				sum_forecast_acw_s,
				avg_forecasted_talk_time,
				avg_actual_talk_time,
				avg_forecasted_acw,
				avg_actual_acw,
				hide_time_zone,
				interval_type,
				[date])
SELECT CASE @interval_type 
		WHEN 1 THEN interval_name
		WHEN 2 THEN halfhour_name
		WHEN 3 THEN hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),date,120),10)--mÃ¥ste vara enligt den kultur som anvÃ¤nds
		WHEN 5 THEN convert(varchar(10),left(year_week,4) + '-' + right(year_week,2))
		WHEN 6 THEN convert(varchar(10),left(year_month,4) + '-' + right(year_month,2))
		WHEN 7 THEN weekday_resource_key
		END AS 'period',
		sum(isnull(forecasted_calls,0)) 'forecasted_calls',
		sum(isnull(calculated_calls,0)) 'calculated_calls',
		sum(isnull(offered_calls,0)) 'offered_calls',
		sum(isnull(answered_calls,0)) 'answered_calls',
		sum(isnull(talk_time_s,0)) 'sum_talk_time_s',
		sum(isnull(acw_s,0)) 'sum_acw_s',
		sum(isnull(forecast_talk_time_s,0)) 'sum_forecast_talk_time_s',
		sum(isnull(forecast_acw_s,0)) 'sum_forecast_acw_s',
		CASE WHEN sum(forecasted_calls) <=0 THEN 0
		ELSE
		convert(decimal(19,2),sum(forecast_talk_time_s)/sum(forecasted_calls) )
		END AS 'avg_forecasted_talk_time',
		CASE WHEN sum(answered_calls) <=0 THEN 0
		ELSE
		convert(decimal(19,2),sum(talk_time_s)/sum(answered_calls) )
		END AS 'avg_actual_talk_time',
		CASE WHEN sum(forecasted_calls) <=0 THEN 0
		ELSE
		convert(decimal(19,2),sum(forecast_acw_s)/sum(forecasted_calls) )
		END AS 'avg_forecasted_acw'	,
		CASE WHEN sum(answered_calls) <=0 THEN 0
		ELSE
		convert(decimal(19,2),sum(acw_s)/sum(answered_calls) )
		END AS 'avg_actual_acw',
		@hide_time_zone as hide_time_zone,
		@interval_type,
		min([date]) as [date]
FROM #pre_result
GROUP BY
	CASE @interval_type 
	WHEN 1 THEN interval_name
		WHEN 2 THEN halfhour_name
		WHEN 3 THEN hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),date,120),10)--mÃ¥ste vara enligt den kultur som anvÃ¤nds
		WHEN 5 THEN convert(varchar(10),left(year_week,4) + '-' + right(year_week,2))
		WHEN 6 THEN convert(varchar(10),left(year_month,4) + '-' + right(year_month,2))
		WHEN 7 THEN weekday_resource_key
	END
ORDER BY 
		CASE @interval_type 
		WHEN 1 THEN interval_name
		WHEN 2 THEN halfhour_name
		WHEN 3 THEN hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),date,120),10)--mÃ¥ste vara enligt den kultur som anvÃ¤nds
		WHEN 5 THEN convert(varchar(10),left(year_week,4) + '-' + right(year_week,2))
		WHEN 6 THEN convert(varchar(10),left(year_month,4) + '-' + right(year_month,2))
		WHEN 7 THEN weekday_resource_key
		END



IF @interval_type=7
BEGIN
	UPDATE #result
	SET weekday_number=d.weekday_number
	FROM mart.dim_date d INNER JOIN #result r ON d.weekday_resource_key=r.period COLLATE database_default
	
	UPDATE #result
	SET period=term_language
	FROM mart.language_translation l 
	INNER JOIN #result r ON l.term_english=substring(r.period,13,len(r.period)) COLLATE database_default
	AND l.language_id=@language_id
END


SELECT * FROM #result order by weekday_number, period

GO
