/****** Object:  StoredProcedure [mart].[report_data_queue_stat_raw]    Script Date: 10/13/2008 20:44:23 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[report_data_queue_stat_raw]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[report_data_queue_stat_raw]
GO
/****** Object:  StoredProcedure [mart].[report_data_queue_stat_raw]    Script Date: 10/13/2008 20:44:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		KJ
-- Create date: 2008-08-06
-- Last Updated:2009-03-02
--				2009-03-02 Excluded timezone UTC from time_zone check KJ
--				2009-02-11 ADDED NEW MART SCHEMA KJ
--				2009-01-26 Added isnull check on values KJ
--				2008-10-13 Fixed bug with duplicate rows KJ
--				2008-08-27 Removed transferred_calls, added columns overflow_out_calls,overflow_in_calls KJ
--				2012-02-15 Changed to uniqueidentifier as report_id - Ola
-- Description:	Used by report Queue Statistics
-- =============================================
--EXEC report_data_queue_stat_raw @skill_set='1,2,3,4',@workload_set='0,4,5,1', @queue_set='0,1,2,3,4',@date_from='2006-01-01',@date_to='2006-01-10',@interval_from=0,@interval_to=287,@time_zone_id=81,@report_id=11,@language_id=1053
--exec report_data_queue_stat_raw @skill_set=N'4,1,3,2,0,5',@workload_set=N'4,1,5,0,2,3',@queue_set=N'4,0,1,2,3,5,6,7',@date_from='2006-01-01 00:00:00:000',@date_to='2006-01-10 00:00:00:000',@interval_from=N'0',@interval_to=N'287',@time_zone_id=N'81',@person_code='89AD580C-F402-4C8E-9F68-9AF000DB0F47',@report_id=14,@language_id=1053
--exec report_data_queue_stat_raw @skill_set=N'4,1,3,2,0,5',@workload_set=N'4,1,5,0,2,3',@queue_set=N'4,0,1,2,3,5,6,7',@date_from='2006-01-01 00:00:00:000',@date_to='2006-01-15 00:00:00:000',@interval_from=N'0',@interval_to=N'287',@time_zone_id=N'81',@person_code='89AD580C-F402-4C8E-9F68-9AF000DB0F47',@report_id=14,@language_id=1053

--select * from fact_queue where queue_id=0

CREATE PROCEDURE [mart].[report_data_queue_stat_raw] 
@skill_set nvarchar(max),
@workload_set nvarchar(max),
@queue_set nvarchar(max),
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

--DECLARE @time_zone_id INT
--SET @time_zone_id=81 --OBS! HÅRDKODAT

/*SNABBA UPP PROCEDUR, HÄMTA RÄTT TIMEZONE FÖRST OCH JOINA PÅ TEMPTABELL SEDAN*/
/*SELECT * 
INTO #bridge_time_zone
FROM bridge_time_zone
WHERE time_zone_id=@time_zone_id*/

CREATE TABLE #skills(id int)
INSERT INTO #skills
SELECT * FROM mart.SplitStringInt(@skill_set)

CREATE TABLE #workloads(id int)
INSERT INTO #workloads
SELECT * FROM mart.SplitStringInt(@workload_set)

/*Split string of queue id:s*/
CREATE TABLE #queues(id int)
INSERT INTO #queues
SELECT * FROM mart.SplitStringInt(@queue_set)

/* Check if time zone will be hidden (if only one exist then hide) */
DECLARE @hide_time_zone bit
IF (SELECT COUNT(*) FROM mart.dim_time_zone tz WHERE tz.time_zone_code<>'UTC') < 2
	SET @hide_time_zone = 1
ELSE
	SET @hide_time_zone = 0



SELECT DISTINCT	d.date_date,
		i.interval_name,
		dq.queue_name,
		convert(int,isnull(offered_calls,0))offered_calls, 
		convert(int,isnull(answered_calls,0))answered_calls, 
		convert(int,isnull(answered_calls_within_SL,0))answered_calls_within_SL, 
		convert(int,isnull(abandoned_calls,0))abandoned_calls, 
		convert(int,isnull(abandoned_calls_within_SL,0))abandoned_calls_within_SL, 
		convert(int,isnull(abandoned_short_calls,0))abandoned_short_calls, 
		convert(int,isnull(overflow_out_calls,0))overflow_out_calls,
		convert(int,isnull(overflow_in_calls,0))overflow_in_calls,
		convert(decimal(18,2),isnull(talk_time_s,0))talk_time_s, 
		convert(decimal(18,2),isnull(after_call_work_s,0))after_call_work_s, 
		convert(decimal(18,2),isnull(handle_time_s,0))handle_time_s, 
		convert(decimal(18,2),isnull(speed_of_answer_s,0))speed_of_answer_s, 
		convert(decimal(18,2),isnull(time_to_abandon_s,0))time_to_abandon_s, 
		convert(decimal(18,2),isnull(longest_delay_in_queue_answered_s,0))longest_delay_in_queue_answered_s,
		convert(decimal(18,2), isnull(longest_delay_in_queue_abandoned_s,0))longest_delay_in_queue_abandoned_s,
		@hide_time_zone as hide_time_zone
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
AND fq.queue_id in (select id from #queues)
AND bq.skill_id IN (select id from #skills)
AND bq.workload_id IN (SELECT id from #workloads)
ORDER BY d.date_date,i.interval_name,dq.queue_name
GO