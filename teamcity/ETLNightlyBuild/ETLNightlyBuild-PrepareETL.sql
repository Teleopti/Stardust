--:SETVAR TeleoptiAnalytics TELEOPTI729_EtlAndAzure_DemoSales_TeleoptiAnalytics

USE [$(TeleoptiAnalytics)]
SET NOCOUNT ON

--Insert ETL properties
DELETE [mart].[sys_configuration]
exec mart.sys_configuration_save @key=N'Culture',@value=1053
exec mart.sys_configuration_save @key=N'IntervalLengthMinutes',@value=15
exec mart.sys_configuration_save @key=N'TimeZoneCode',@value=N'W. Europe Standard Time'
GO

--Add SP that checks for ETL-errors
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_nightlyBuild_CheckError]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_nightlyBuild_CheckError]
GO

CREATE PROCEDURE [mart].[etl_nightlyBuild_CheckError] 
@DelayLength char(8)= '00:00:02'
AS
DECLARE @ReturnInfo varchar(255)
IF ISDATE('2000-01-01 ' + @DelayLength + '.000') = 0
    BEGIN
        SELECT @ReturnInfo = 'Invalid time ' + @DelayLength 
        + ',hh:mm:ss, submitted.';
        PRINT @ReturnInfo 
        RETURN(1)
    END
BEGIN

	--Wait until we get a row in table [mart].[etl_job_execution]. e.g. the ETL scheduled job is finished
	WHILE NOT EXISTS (SELECT [job_execution_id] FROM [mart].[etl_job_execution])
	BEGIN
		WAITFOR DELAY @DelayLength
	END

	--Check for any errors in the job
	IF EXISTS (SELECT jobstep_error_id FROM [mart].[etl_jobstep_error])
	BEGIN
		-- Return an error with state 127 since it will abort SQLCMD
		DECLARE @ErrorMessage varchar(8000)
		SELECT top 1 @ErrorMessage = CAST(error_exception_message as varchar(1000)) + CAST(error_exception_stacktrace as varchar(7000)) FROM [mart].[etl_jobstep_error] order by jobstep_error_id desc
		RAISERROR (@ErrorMessage, 16, 127)
	END

END;
GO

--Delete All old schedules
EXEC mart.etl_job_delete_schedule_All

--Empty the datamart
EXEC mart.etl_data_mart_delete

-- Create a schedule for the main job that runs one minute aftere this script is run
DECLARE @main_job_schedule_id INT, @minutes_of_day INT, @job_start_time_string NVARCHAR(100)
DECLARE @run_period_start SMALLDATETIME, @run_period_end SMALLDATETIME
DECLARE @relative_period_start INT, @relative_period_end INT
DECLARE @delay int
SET @delay=1 --default 1 minute

SET @run_period_start = '20130201'
SET @run_period_end = '20130228'
SET @relative_period_start = DATEDIFF(d, GETDATE(), @run_period_start)
SET @relative_period_end = DATEDIFF(d, GETDATE(), @run_period_end)
SET @minutes_of_day = DATEDIFF(mi, CONVERT(INT, CONVERT(FLOAT, GETDATE())), CONVERT(FLOAT, GETDATE()))
-- Main job should start minut after this script is run

IF (DATEDIFF(ss, CONVERT(INT, CONVERT(FLOAT, GETDATE())), CONVERT(FLOAT, GETDATE())) % 60) > 40 --we passed the 40 seconds mark within this current minut
	SET @delay= @delay +1 --add another minute

SET @minutes_of_day = @minutes_of_day + @delay

SET @job_start_time_string = LEFT(CONVERT(NVARCHAR(8), DATEADD(mi, @delay, GETDATE()), 108), 5)
SET @job_start_time_string = 'Occurs every day at ' + @job_start_time_string + '. Using the log data source ''<All>''.'

CREATE TABLE #new_schedule(id INT)
INSERT INTO #new_schedule
	exec mart.etl_job_save_schedule @schedule_id=-1,@schedule_name=N'My Main Job',@enabled=1,@schedule_type=0,@occurs_daily_at=@minutes_of_day,@occurs_every_minute=0,@recurring_starttime=0,@recurring_endtime=0,@etl_job_name=N'Nightly',@etl_relative_period_start=0,@etl_relative_period_end=365,@etl_datasource_id=-2,@description=@job_start_time_string

SELECT @main_job_schedule_id = id FROM #new_schedule
DROP TABLE #new_schedule

exec mart.etl_job_save_schedule_period @schedule_id=@main_job_schedule_id,@job_name=N'Initial',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@main_job_schedule_id,@job_name=N'Schedule',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@main_job_schedule_id,@job_name=N'Forecast',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@main_job_schedule_id,@job_name=N'Agent Statistics',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end
exec mart.etl_job_save_schedule_period @schedule_id=@main_job_schedule_id,@job_name=N'Queue Statistics',@relative_period_start=@relative_period_start,@relative_period_end=@relative_period_end