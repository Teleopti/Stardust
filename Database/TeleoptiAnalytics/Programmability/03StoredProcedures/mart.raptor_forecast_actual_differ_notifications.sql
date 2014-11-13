IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[raptor_forecast_actual_differ_notifications]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[raptor_forecast_actual_differ_notifications]
GO


CREATE PROCEDURE [mart].[raptor_forecast_actual_differ_notifications]
@date_from datetime = NULL, --convert(varchar(8),getdate(),112),
@interval_from int = NULL,
@interval_to int = NULL

AS

declare @scenario_id int
declare @time_zone_id int
declare @Fcalls decimal(28,4), @Ccalls decimal(28,4), @Kvot decimal(28,4)
declare @intmin nvarchar(30), @intmax nvarchar(30) , @Perc decimal(28,2)
declare @TheSubject nvarchar(200), @TheBody nvarchar(2000)

select @scenario_id = scenario_id from dim_scenario where default_scenario = 1
select @time_zone_id = time_zone_id from mart.dim_time_zone where time_zone_code = 'W. Europe Standard Time'

CREATE TABLE #output(Receiver nvarchar(200), [Subject] nvarchar(500), Body nvarchar(1000))

SET NOCOUNT ON
if @date_from is NULL
begin
	if datepart(hour,getdate()) not between 10 and 16
	begin
	SELECT * FROM #output
		return
	end
	select @date_from = convert(nvarchar(8),getdate(),112)
	select @interval_to = interval_id
		from mart.dim_interval
		where datepart(hour,interval_end) = datepart(hour,getdate())
		and datepart(minute,interval_end)/15 = datepart(minute,getdate())/15
	select @interval_from = @interval_to - 3
end





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

 
/*Split string of skill id:s*/
INSERT INTO #skills
SELECT skill_id FROM mart.dim_skill where is_deleted = 0

/*Split string of workload id:s*/
INSERT INTO #workloads
SELECT workload_id FROM mart.dim_workload inner join mart.dim_skill on mart.dim_workload.skill_id =  mart.dim_skill.skill_id where mart.dim_skill.is_deleted = 0 and mart.dim_workload.is_deleted = 0

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
WHERE d.date_date BETWEEN @date_from AND @date_from
AND i.interval_id BETWEEN @interval_from AND @interval_to
AND b.time_zone_id = @time_zone_id
AND bqw.skill_id IN (select id from #skills)
AND bqw.workload_id IN (select id from #workloads)
ORDER BY i.interval_name
		
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
		1
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
WHERE d.date_date BETWEEN @date_from AND @date_from
AND i.interval_id BETWEEN @interval_from AND @interval_to
AND b.time_zone_id = @time_zone_id
AND fw.skill_id IN (select id from #skills)
AND fw.workload_id IN (select id from #workloads)
AND fw.scenario_id=@scenario_id 
ORDER BY i.interval_name


/* GROUP RESULT DATA */
SELECT 	@intmin = min(interval_name),
		@intmax = max(interval_name),
		@Fcalls = sum(isnull(forecasted_calls,0)),
		@Ccalls = sum(isnull(calculated_calls,0))
FROM #pre_result

IF @Fcalls = 0
BEGIN
	SELECT * FROM #output
	RETURN 
END

select @Kvot = @Ccalls / @Fcalls
select @Perc = ((@Ccalls - @Fcalls) / @Fcalls)*100

if @Kvot >= 1.3
begin
set @TheSubject = 'Det ringer mer idag! Faktiskt '+convert(nvarchar(max),isnull(@Kvot,0))+' gånger mer än prognos.'
set @TheBody = 'Det blir '+convert(nvarchar(max),isnull(@Perc,0))+' % mer än prognos.
Verkliga samtal = '+convert(nvarchar(max),isnull(@Ccalls,0))+'
Prognos samtal = '+convert(nvarchar(max),isnull(@Fcalls,0))+'
Genererat för '+convert(nvarchar(10),@date_from,120)+' - '+isnull(@intmin,'NO DATA')+' - '+isnull(@intmax,'FOUND')+'
Powered by Teleopti'

	insert into #output SELECT 'ola@teleopti.com', @TheSubject, @TheBody
end
else if @Kvot <= 0.7
begin
set @TheSubject = 'Det ringer lite mindre idag! Faktiskt '+convert(nvarchar(max),isnull(@Kvot,0))+' gånger mindre än prognos.'
set @TheBody = 'Det blir '+convert(nvarchar(max),isnull(@Perc,0))+' % mindre än prognos.
Verkliga samtal = '+convert(nvarchar(max),isnull(@Ccalls,0))+'
Prognos samtal = '+convert(nvarchar(max),isnull(@Fcalls,0))+'
Genererat för '+convert(nvarchar(8),@date_from,112)+' - '+isnull(@intmin,'NO DATA')+' - '+isnull(@intmax,'FOUND')+'
Powered by Teleopti'

	insert into #output SELECT 'ola@teleopti.com', @TheSubject, @TheBody
end

select * from #output
