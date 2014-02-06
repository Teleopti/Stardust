/****** Object:  StoredProcedure [mart].[report_data_queue_stat_abnd]    Script Date: 10/09/2008 13:45:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_queue_stat_abnd]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_queue_stat_abnd]
GO
/****** Object:  StoredProcedure [mart].[report_data_queue_stat_abnd]    Script Date: 10/09/2008 13:45:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-05-29
-- Last Updated:20090414
-- 20090414 added COLLATE database_default for joins with strings KJ
-- 20090302 Excluded timezone UTC from time_zone check KJ
-- 20090211 Added new mart schema KJ
-- 20090115 Added @interval_type 7 weekday KJ
-- 20081009 Bug fix Date, Week and Month Formats changed KJ
-- 20080708 Added new id:s for period_type
-- 20080808 Added parameter time_zone_id and added connection to bridge_queue_workload KJ
-- 2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	Used by report Queue Statistics -  Abandoned Calls
-- =============================================
--exec report_data_queue_stat_abnd @skill_set=N'0,1,2',@workload_set=N'0',@interval_type=N'6',@date_from='2006-01-01 00:00:00:000',@date_to='2006-01-10 00:00:00:000',@interval_from=N'0',@interval_to=N'287',@time_zone_id='1',@person_code='79C90699-C7C0-4D17-ADBF-9B2F0012C85A',@report_id=8,@language_id=1053

CREATE PROCEDURE [mart].[report_data_queue_stat_abnd] 
@skill_set nvarchar(max),
@workload_set nvarchar(max),
@interval_type int, --anger vilket intervall rapporten ska grupperas pa : lagsta intervall/halvtimme/timme/dag/vecka/manad/veckodag
@date_from datetime,
@date_to datetime,
@interval_from int,--mellan vilka tider
@interval_to int,
@time_zone_id int,
@person_code uniqueidentifier ,
@report_id uniqueidentifier,
@language_id int,
@business_unit_code uniqueidentifier
AS
SET NOCOUNT ON

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0


CREATE TABLE #skills(id int)
INSERT INTO #skills
SELECT * FROM mart.SplitStringInt(@skill_set)

CREATE TABLE #workloads(id int)
INSERT INTO #workloads
SELECT * FROM mart.SplitStringInt(@workload_set)

CREATE TABLE #result(period nvarchar(30),
					calls_offered int,
					calls_answ int,
					calls_abnd int,
					short_calls_abnd int,
					sum_answer_time int,
					sum_abnd_time int,
					answer_rate decimal(18,3),
					abnd_rate decimal(18,3),
					abnd_rate_excl_short_calls decimal(19,3),
					average_speed_answer decimal(19,3),
					average_time_to_Abnd decimal(19,3),
					longest_delay_answered_calls decimal(19,3),
					longest_delay_abnd_calls decimal(19,3),
					hide_time_zone bit,
					interval_type int,
					weekday_number int,
					[date] smalldatetime
)

INSERT #result(period,calls_offered,calls_answ,calls_abnd,short_calls_abnd,sum_answer_time,sum_abnd_time,answer_rate,abnd_rate,abnd_rate_excl_short_calls,average_speed_answer,average_time_to_Abnd,longest_delay_answered_calls,longest_delay_abnd_calls,hide_time_zone,interval_type,[date])
SELECT CASE @interval_type 
		WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--formatted in report to correct culture
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
		END AS 'period',
		convert(int,sum(fq.offered_calls)) 'calls_offered',
		convert(int,sum(fq.answered_calls)) 'calls_answ',
		convert(int,sum(fq.abandoned_calls)) 'calls_abnd',
		convert(int,sum(fq.abandoned_short_calls))'short_calls_abnd',
		convert(int,sum(fq.speed_of_answer_s)) 'sum_answer_time',
		convert(int,sum(fq.time_to_abandon_s)) 'sum_abnd_time',
		CASE WHEN sum(fq.offered_calls)=0 THEN 0
		ELSE
		(sum(fq.answered_calls)/convert(decimal(19,3),sum(fq.offered_calls)))
		END AS 'answer_rate',
		CASE WHEN sum(fq.offered_calls)=0 THEN 0
		ELSE
		(sum(fq.abandoned_calls)/convert(decimal(19,3),sum(fq.offered_calls)))
		END AS 'abnd_rate',
		CASE WHEN sum(fq.offered_calls)=0 THEN 0
		ELSE
		((sum(fq.abandoned_calls)-sum(fq.abandoned_short_calls))/convert(decimal(19,3),sum(fq.offered_calls)))
		END AS 'abnd_rate_excl_short_calls',
		CASE WHEN sum(fq.answered_calls) =0 THEN 0
		ELSE
		convert(decimal(19,3),sum(fq.speed_of_answer_s)/convert(decimal(19,3),sum(fq.answered_calls)))
		END AS 'average_speed_answer',
		CASE WHEN sum(fq.abandoned_calls) =0 THEN 0
		ELSE
		convert(decimal(19,3),sum(fq.time_to_abandon_s)/convert(decimal(19,3),sum(fq.abandoned_calls)))
		END AS 'average_time_to_Abnd',
		convert(decimal(19,3),max(fq.longest_delay_in_queue_answered_s)) 'longest_delay_answered_calls',
		convert(decimal(19,3),max(fq.longest_delay_in_queue_abandoned_s)) 'longest_delay_abnd_calls',
		@hide_time_zone as hide_time_zone,
		@interval_type as interval_type,
		min(d.date_date) as [date]
FROM 
	mart.fact_queue fq
INNER JOIN 
	mart.dim_queue dq 
ON dq.queue_id=fq.queue_id
INNER JOIN 
	mart.bridge_queue_workload bqw
	ON dq.queue_id=bqw.queue_id
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
INNER JOIN 
	mart.dim_skill sk ON
sk.skill_id=bqw.skill_id
WHERE d.date_date BETWEEN @date_from AND @date_to
AND i.interval_id BETWEEN @interval_from AND @interval_to
AND b.time_zone_id = @time_zone_id
AND bqw.skill_id IN (select id from #skills)
AND bqw.workload_id IN (SELECT id from #workloads)
GROUP BY
	CASE @interval_type 
	WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--formatted in report to correct culture
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
	END
ORDER BY 
		CASE @interval_type 
		WHEN 1 THEN i.interval_name
		WHEN 2 THEN i.halfhour_name
		WHEN 3 THEN i.hour_name
		WHEN 4 THEN LEFT(convert(varchar(30),d.date_date,120),10)--formatted in report to correct culture
		WHEN 5 THEN convert(varchar(10),left(d.year_week,4) + '-' + right(d.year_week,2))
		WHEN 6 THEN convert(varchar(10),left(d.year_month,4) + '-' + right(d.year_month,2))
		WHEN 7 THEN d.weekday_resource_key
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

SELECT *
FROM #result
ORDER BY weekday_number,period
GO