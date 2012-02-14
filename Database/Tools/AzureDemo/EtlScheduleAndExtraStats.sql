-- Create a schedule for the main job that runs one minute aftere this script is run
DECLARE @schedule_id INT
DECLARE @minutes_of_day INT
DECLARE @job_start_time_string NVARCHAR(100)
DECLARE @run_period_start SMALLDATETIME
DECLARE @run_period_end SMALLDATETIME
DECLARE @relative_period_start INT
DECLARE @relative_period_end INT
DECLARE @nightlyName nvarchar(150)
DECLARE @intradayName nvarchar(150)

SET @run_period_start = '20110601'
SET @run_period_end = '20111231'
SET @relative_period_start = DATEDIFF(d, GETDATE(), @run_period_start)
SET @relative_period_end = DATEDIFF(d, GETDATE(), @run_period_end)
SET @minutes_of_day = DATEDIFF(mi, CONVERT(INT, CONVERT(FLOAT, GETDATE())), CONVERT(FLOAT, GETDATE()))
-- Main job should start minut after this script is run
SET @minutes_of_day = @minutes_of_day + 2

SET @job_start_time_string = LEFT(CONVERT(NVARCHAR(8), DATEADD(mi, 1, GETDATE()), 108), 5)
SET @job_start_time_string = 'Occurs every day at ' + @job_start_time_string + '. Using the log data source ''<All>''.'
SET @nightlyName = N'Nightly'
SET @intradayName = N'Intraday'

--NIGHTLY
exec mart.etl_job_save_schedule @schedule_id=-1,@schedule_name=@nightlyName,@enabled=1,@schedule_type=0,@occurs_daily_at=0,@occurs_every_minute=0,@recurring_starttime=0,@recurring_endtime=0,@etl_job_name=@nightlyName,@etl_relative_period_start=0,@etl_relative_period_end=365,@etl_datasource_id=-2,@description=N'Occurs every day at 00:00. Using the log data source ''< All >''.'
select @schedule_id = schedule_id FROM  Mart.[etl_job_schedule]
where schedule_name=@nightlyName

exec mart.etl_job_save_schedule_period @schedule_id=@schedule_id,@job_name=N'Initial',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@schedule_id,@job_name=N'Schedule',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@schedule_id,@job_name=N'Forecast',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@schedule_id,@job_name=N'Agent Statistics',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@schedule_id,@job_name=N'Queue Statistics',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end

--INTRADAY
exec mart.etl_job_save_schedule @schedule_id=-1,@schedule_name=@intradayName,@enabled=1,@schedule_type=1,@occurs_daily_at=0,@occurs_every_minute=15,@recurring_starttime=60,@recurring_endtime=1380,@etl_job_name=@intradayName,@etl_relative_period_start=0,@etl_relative_period_end=365,@etl_datasource_id=-2,@description=N'Occurs every day every 15 minute(s) between 01:00 and 23:00. Using the log data source ''< All >''.'
select @schedule_id = schedule_id FROM  Mart.[etl_job_schedule]
where schedule_name=@intradayName

exec mart.etl_job_save_schedule_period @schedule_id=@schedule_id,@job_name=N'Initial',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@schedule_id,@job_name=N'Schedule',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@schedule_id,@job_name=N'Forecast',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@schedule_id,@job_name=N'Agent Statistics',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@schedule_id,@job_name=N'Queue Statistics',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
