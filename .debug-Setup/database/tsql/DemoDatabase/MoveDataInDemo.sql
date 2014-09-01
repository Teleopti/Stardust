/*
:SETVAR TELEOPTICCC Training_TeleoptiCCC7
:SETVAR TELEOPTIANALYTICS Training_TeleoptiAnalytics
:SETVAR TELEOPTIAGG Training_TeleoptiCCCAgg
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
--Add agent statistics from Agg 4 week data
----------------
declare @TemplateEndDate datetime
declare @TemplateStartDate datetime
set @TemplateEndDate = '2013-03-03'
set @TemplateStartDate = '2013-02-04'

delete from $(TELEOPTIAGG).dbo.agent_logg where date_from > @TemplateEndDate --unwanted days after template period

declare @MondayLastWeek datetime
declare @datediffMondayTwoWeeksAgo int
declare @PeriodsToAdd int
declare @TemplateLength int
declare @AddDays int

set @PeriodsToAdd = 0
select @TemplateLength=datediff(DD,@TemplateStartDate,@TemplateEndDate)+1

--get monday of last week
SELECT @MondayLastWeek=DATEADD(wk, DATEDIFF(wk,7,GETDATE()), 0)

WHILE @PeriodsToAdd < 9 --9x4 = 41 weeks
BEGIN
	--Days to add for next
	select @AddDays=datediff(DAY,@TemplateStartDate,@MondayLastWeek)+@TemplateLength*@PeriodsToAdd

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

--Add agent stat to mart directly -10 t0 +20
declare @start_date smalldatetime
declare @end_date smalldatetime
select @start_date=DATEADD(dd, -10, DATEDIFF(dd, 0, GETDATE()))
select @end_date=DATEADD(dd, 20, DATEDIFF(dd, 0, GETDATE()))
exec $(TELEOPTIANALYTICS).mart.etl_fact_agent_load @start_date,@end_date,-2
exec $(TELEOPTIANALYTICS).mart.etl_fact_agent_queue_load @start_date,@end_date,-2

--get all queue data
select @start_date='2000-01-01'
select @end_date='2020-12-31'
exec $(TELEOPTIANALYTICS).mart.etl_fact_queue_load @start_date,@end_date,-2
