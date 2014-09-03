/*
:SETVAR TELEOPTICCC Training_TeleoptiCCC7
:SETVAR TELEOPTIANALYTICS Training_TeleoptiAnalytics
:SETVAR TELEOPTIAGG Training_TeleoptiCCCAgg
:SETVAR TELEOPTICCC main_DemoSales_TeleoptiCCC7
:SETVAR TELEOPTIANALYTICS main_DemoSales_TeleoptiAnalytics
:SETVAR TELEOPTIAGG main_DemoSales_TeleoptiCCCAgg
*/

SET NOCOUNT ON
----------------
--#29068 - Delete PersonalSettings, but keep Intraday settings
----------------
delete from $(TELEOPTICCC).dbo.PersonalSettingData
where [key] not in (
	'IntradaySettings'
	)

----------------
-- NOTE the Pre Req: Restore of Agg and Analytics databases!
----------------

----------------
--Move agent and Queue stat one year forward, monday map to monday.
----------------
declare @DaysToAdd int
set @DaysToAdd = 365

update $(TELEOPTIAGG).dbo.queue_logg
set date_from = dateadd(day,@DaysToAdd,date_from)
update $(TELEOPTIAGG).dbo.agent_logg
set date_from = dateadd(day,@DaysToAdd,date_from)

GO

----------------
--Copy agent statistics from Agg template - 4 week data
----------------
declare @TemplateEndDate datetime
declare @TemplateStartDate datetime
declare @MondayThreeWeeksAgo datetime
declare @PeriodsToAdd int
declare @TemplateLength int
declare @AddDays int

set @TemplateStartDate = '2014-02-03'
set @TemplateEndDate = '2014-03-02'
set @PeriodsToAdd = 0
select @TemplateLength=datediff(DD,@TemplateStartDate,@TemplateEndDate)+1

--get monday of last week
SELECT @MondayThreeWeeksAgo=DATEADD(wk, DATEDIFF(wk,21,GETDATE()), 0)

WHILE @PeriodsToAdd < 9 --9x4 = 41 weeks
BEGIN
	--Days to add for next
	select @AddDays=datediff(DAY,@TemplateStartDate,@MondayThreeWeeksAgo)+@TemplateLength*@PeriodsToAdd

	insert into $(TELEOPTIAGG).dbo.agent_logg
	select
	queue,
	dateadd(day,@AddDays,date_from),
	interval, agent_id, agent_name, avail_dur, tot_work_dur, talking_call_dur, pause_dur, wait_dur, wrap_up_dur, answ_call_cnt, direct_out_call_cnt, direct_out_call_dur, direct_in_call_cnt, direct_in_call_dur, transfer_out_call_cnt, admin_dur
	from $(TELEOPTIAGG).dbo.agent_logg
	where date_from <= @TemplateEndDate
	order by 2

	select @PeriodsToAdd = @PeriodsToAdd + 1
END
GO

----------------
--load all agent,queue,agent_queue from Agg int Mart
----------------
declare @start_date smalldatetime
declare @end_date smalldatetime

SELECT 
	@start_date=isnull(min(date_date), '1999-12-31'),
	@end_date=isnull(max(date_date), '1999-12-31')
FROM $(TELEOPTIANALYTICS).mart.dim_date
WHERE 
	date_id > -1

exec $(TELEOPTIANALYTICS).mart.etl_fact_agent_load @start_date,@end_date,-2
exec $(TELEOPTIANALYTICS).mart.etl_fact_agent_queue_load @start_date,@end_date,-2
exec $(TELEOPTIANALYTICS).mart.etl_fact_queue_load @start_date,@end_date,-2
GO

----------------
--create + copy deviation date from template - 4 week data
----------------
declare @TemplateEndDate datetime
declare @TemplateStartDate datetime
declare @MondayThreeWeeksAgo datetime
declare @PeriodsToAdd int
declare @TemplateLength int
declare @AddDays int

set @TemplateStartDate = '2014-02-03'
set @TemplateEndDate = '2014-03-02'

--create
exec $(TELEOPTIANALYTICS).mart.etl_fact_schedule_deviation_load @TemplateStartDate,@TemplateEndDate,'928DD0BC-BF40-412E-B970-9B5E015AADEA',0,0

--copy
set @PeriodsToAdd = 0
select @TemplateLength=datediff(DD,@TemplateStartDate,@TemplateEndDate)+1

--get monday of last week
SELECT @MondayThreeWeeksAgo=DATEADD(wk, DATEDIFF(wk,21,GETDATE()), 0)

WHILE @PeriodsToAdd < 9 --9x4 = 41 weeks
BEGIN
	--Days to add for next
	select @AddDays=datediff(DAY,@TemplateStartDate,@MondayThreeWeeksAgo)+@TemplateLength*@PeriodsToAdd

	insert into $(TELEOPTIANALYTICS).mart.fact_schedule_deviation
	select
	f.date_id+@AddDays,
	f.interval_id, f.person_id, f.scheduled_ready_time_s, f.ready_time_s, f.contract_time_s, f.deviation_schedule_s, f.deviation_schedule_ready_s, f.deviation_contract_s, f.business_unit_id, f.datasource_id, f.insert_date, f.update_date, f.is_logged_in,
	f.shift_startdate_id+@AddDays,
	f.shift_startinterval_id
	from $(TELEOPTIANALYTICS).mart.fact_schedule_deviation f
	inner join $(TELEOPTIANALYTICS).mart.dim_date d
		on d.date_id = f.date_id
	where d.date_date <= @TemplateEndDate

	insert into $(TELEOPTIANALYTICS).mart.fact_schedule
	select
	f.schedule_date_id+@AddDays,
	f.person_id, f.interval_id,
	dateadd(dd,@AddDays,f.activity_starttime),
	f.scenario_id, f.activity_id, f.absence_id,
	f.activity_startdate_id+@AddDays,
	f.activity_enddate_id+@AddDays,
	dateadd(dd,@AddDays,f.activity_endtime),
	f.shift_startdate_id+@AddDays,
	dateadd(dd,@AddDays,f.shift_starttime),
	f.shift_enddate_id+@AddDays,
	dateadd(dd,@AddDays,f.shift_endtime),
	f.shift_startinterval_id, f.shift_category_id, f.shift_length_id, f.scheduled_time_m, f.scheduled_time_absence_m, f.scheduled_time_activity_m, f.scheduled_contract_time_m, f.scheduled_contract_time_activity_m, f.scheduled_contract_time_absence_m, f.scheduled_work_time_m, f.scheduled_work_time_activity_m, f.scheduled_work_time_absence_m, f.scheduled_over_time_m, f.scheduled_ready_time_m, f.scheduled_paid_time_m, f.scheduled_paid_time_activity_m, f.scheduled_paid_time_absence_m, f.last_publish, f.business_unit_id, f.datasource_id, f.insert_date, f.update_date, f.datasource_update_date, f.overtime_id
	
	from $(TELEOPTIANALYTICS).mart.fact_schedule f
	inner join $(TELEOPTIANALYTICS).mart.dim_date d
		on d.date_id = f.schedule_date_id
	where d.date_date <= @TemplateEndDate

	select @PeriodsToAdd = @PeriodsToAdd + 1
END
GO

----------------
--Add one ETL job - Nightly -10/+20
----------------
--Delete All old schedules
EXEC $(TELEOPTIANALYTICS).mart.etl_job_delete_schedule_All

--add a Nightly job that executes in 5 min
-- Create a schedule for the main job that runs one minute aftere this script is run
DECLARE @main_job_schedule_id INT, @minutes_of_day INT, @job_start_time_string NVARCHAR(100)
DECLARE @relative_period_start INT, @relative_period_end INT
DECLARE @delayJobByMin int
SET @delayJobByMin = 25 --Gives the user some time to apply the license before Nightly job is executed

SET @relative_period_start = -10
SET @relative_period_end = 20
SET @minutes_of_day = DATEDIFF(mi, CONVERT(INT, CONVERT(FLOAT, GETDATE())), CONVERT(FLOAT, GETDATE()))
SET @minutes_of_day = @minutes_of_day + @delayJobByMin

SET @job_start_time_string = LEFT(CONVERT(NVARCHAR(8), DATEADD(mi, @delayJobByMin, GETDATE()), 108), 5)
SET @job_start_time_string = 'Occurs every day at ' + @job_start_time_string + '. Using the log data source ''<All>''.'

CREATE TABLE #new_schedule(id INT)
INSERT INTO #new_schedule
	exec $(TELEOPTIANALYTICS).mart.etl_job_save_schedule @schedule_id=-1,@schedule_name=N'My Main Job',@enabled=1,@schedule_type=0,@occurs_daily_at=@minutes_of_day,@occurs_every_minute=0,@recurring_starttime=0,@recurring_endtime=0,@etl_job_name=N'Nightly',@etl_relative_period_start=0,@etl_relative_period_end=365,@etl_datasource_id=-2,@description=@job_start_time_string

SELECT @main_job_schedule_id = id FROM #new_schedule
DROP TABLE #new_schedule

exec $(TELEOPTIANALYTICS).mart.etl_job_save_schedule_period @schedule_id=@main_job_schedule_id,@job_name=N'Initial',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec $(TELEOPTIANALYTICS).mart.etl_job_save_schedule_period @schedule_id=@main_job_schedule_id,@job_name=N'Schedule',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec $(TELEOPTIANALYTICS).mart.etl_job_save_schedule_period @schedule_id=@main_job_schedule_id,@job_name=N'Forecast',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec $(TELEOPTIANALYTICS).mart.etl_job_save_schedule_period @schedule_id=@main_job_schedule_id,@job_name=N'Agent Statistics',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec $(TELEOPTIANALYTICS).mart.etl_job_save_schedule_period @schedule_id=@main_job_schedule_id,@job_name=N'Queue Statistics',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
GO