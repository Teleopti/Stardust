IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[main_convert_fact_schedule_ccc8_run]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[main_convert_fact_schedule_ccc8_run]
GO
--[mart].[main_convert_fact_schedule_ccc8_estimate]
--[mart].[main_convert_fact_schedule_ccc8_run] @start_date_id=4927,@end_date_id=4957

CREATE PROCEDURE [mart].[main_convert_fact_schedule_ccc8_run]
@start_date_id int,
@end_date_id int,
@is_delayed_job int = 1
AS

--TRUNCATE TABLE [mart].[fact_schedule]
DECLARE @io_statistics_string nvarchar(2000)
DECLARE @nl Char(2)
DECLARE @tempdb_time_Start datetime
DECLARE @tempdb_num_of_bytes_written bigint
DECLARE @tempdb_num_of_bytes_read bigint
DECLARE @tempdb_io_stall_read_ms bigint
DECLARE @tempdb_io_stall_write_ms bigint
DECLARE @tempdb_num_of_writes bigint
DECLARE @tempdb_num_of_reads bigint

DECLARE @mart_time_Start datetime
DECLARE @mart_num_of_bytes_written bigint
DECLARE @mart_num_of_bytes_read bigint
DECLARE @mart_io_stall_read_ms bigint
DECLARE @mart_io_stall_write_ms bigint
DECLARE @mart_num_of_writes bigint
DECLARE @mart_num_of_reads bigint

DECLARE @martfileid int

DECLARE @IntervalLengthMinutes smallint
DECLARE @IntervalPerDay smallint 
DECLARE @min_date_id int

SET @nl  = char(13) + char(10)
--check tempdb before
IF  (SELECT [dbo].[IsAzureDB] ()) <> 1 AND @is_delayed_job=0
BEGIN
	SELECT @martfileid=fileid
	FROM sys.sysfiles
	WHERE name='TeleoptiAnalytics_Mart'

	SELECT @martfileid=fileid
	FROM sys.sysfiles
	WHERE name='TeleoptiAnalytics_Mart'

	--Mart stats
	SELECT
		@mart_time_Start=getdate(),
		@mart_num_of_bytes_written=num_of_bytes_written,
		@mart_num_of_bytes_read=num_of_bytes_read,
		@mart_io_stall_write_ms=io_stall_write_ms,
		@mart_io_stall_read_ms=io_stall_read_ms,
		@mart_num_of_reads=num_of_reads,
		@mart_num_of_writes=num_of_writes
	FROM sys.dm_io_virtual_file_stats(DB_ID(DB_NAME()), @martfileid)	

	--tempdb stats
	SELECT
		@tempdb_time_Start=getdate(),
		@tempdb_num_of_bytes_written=num_of_bytes_written,
		@tempdb_num_of_bytes_read=num_of_bytes_read,
		@tempdb_io_stall_write_ms=io_stall_write_ms,
		@tempdb_io_stall_read_ms=io_stall_read_ms,
		@tempdb_num_of_reads=num_of_reads,
		@tempdb_num_of_writes=num_of_writes
	FROM sys.dm_io_virtual_file_stats(DB_ID('tempdb'), 1)
END

--INSERT DATA FROM OLD FACT_SCHEDULE
SELECT
	@IntervalLengthMinutes=value,
	@IntervalPerDay=1440/value
FROM mart.sys_configuration
WHERE [key]='IntervalLengthMinutes'

INSERT [mart].[fact_schedule] WITH(TABLOCK)
SELECT
shift_startdate_local_id		= btz.local_date_id,
schedule_date_id				= f.schedule_date_id,
person_id						= f.person_id,
interval_id						= f.interval_id,
activity_starttime				= f.activity_starttime,
scenario_id						= f.scenario_id,
activity_id						= f.activity_id,
absence_id						= f.absence_id,
activity_startdate_id			= f.activity_startdate_id,
activity_enddate_id				= f.activity_enddate_id,
activity_endtime				= f.activity_endtime,
shift_startdate_id				= f.shift_startdate_id,
shift_starttime					= f.shift_starttime,
shift_enddate_id				= f.shift_enddate_id,
shift_endtime					= f.shift_endtime,
shift_startinterval_id			= f.shift_startinterval_id,
shift_endinterval_id			= case (datepart(day,shift_starttime)-datepart(day,shift_endtime))
									when 0
										then shift_startinterval_id+datediff(mi,shift_starttime,shift_endtime)/@IntervalLengthMinutes
										else shift_startinterval_id+datediff(mi,shift_starttime,shift_endtime)/@IntervalLengthMinutes-@IntervalPerDay
									end,
shift_category_id				= f.shift_category_id,
shift_length_id					= f.shift_length_id,
scheduled_time_m				= f.scheduled_time_m,
scheduled_time_absence_m		= f.scheduled_time_absence_m,
scheduled_time_activity_m		= f.scheduled_time_activity_m,
scheduled_contract_time_m		= f.scheduled_contract_time_m,
scheduled_contract_time_activity_m	= f.scheduled_contract_time_activity_m,
scheduled_contract_time_absence_m	= f.scheduled_contract_time_absence_m,
scheduled_work_time_m			= f.scheduled_work_time_m,
scheduled_work_time_activity_m		= f.scheduled_work_time_activity_m,
scheduled_work_time_absence_m		= f.scheduled_work_time_absence_m,
scheduled_over_time_m			= f.scheduled_over_time_m,
scheduled_ready_time_m			= f.scheduled_ready_time_m,
scheduled_paid_time_m			= f.scheduled_paid_time_m,
scheduled_paid_time_activity_m	= f.scheduled_paid_time_activity_m,
scheduled_paid_time_absence_m	= f.scheduled_paid_time_absence_m,
business_unit_id				= f.business_unit_id,
datasource_id					= f.datasource_id,
insert_date						= f.insert_date,
update_date						= f.update_date,
datasource_update_date			= f.datasource_update_date,
overtime_id						= f.overtime_id
FROM [mart].[fact_schedule_old] f
INNER JOIN mart.bridge_time_zone btz 
	ON f.shift_startdate_id=btz.date_id 
	AND f.shift_startinterval_id=btz.interval_id
INNER JOIN mart.dim_person dp
	ON f.person_id=dp.person_id
	AND btz.time_zone_id=dp.time_zone_id
WHERE f.schedule_date_id between @start_date_id and @end_date_id
AND NOT EXISTS(	SELECT * FROM mart.fact_schedule_old old 
				INNER JOIN mart.fact_schedule new
				ON new.schedule_date_id=old.schedule_date_id
				AND new.person_id=old.person_id
				AND new.interval_id=old.interval_id
				AND new.activity_starttime=old.activity_starttime
				AND new.scenario_id=old.scenario_id
				WHERE new.schedule_date_id between @start_date_id and @end_date_id)

OPTION (MAXDOP 1);

--check tempdb after
IF  (SELECT [dbo].[IsAzureDB] ()) <> 1 AND @is_delayed_job=0
BEGIN
	PRINT 'tempdb I/O stats:'
	SELECT @io_statistics_string =
		'seconds for fact_schedule insert: ' + cast(datediff(SS,@tempdb_time_Start,getdate()) as varchar(10)) + @nl +
		'num_of_reads: ' + cast(num_of_reads-@tempdb_num_of_reads as varchar(20))+@nl +
		'num_of_bytes_read: ' + cast(num_of_bytes_read-@tempdb_num_of_bytes_read as varchar(20))+@nl +
		'io_stall_read_ms: ' + cast(io_stall_read_ms-@tempdb_io_stall_read_ms as varchar(20))+@nl +
		'avg_io_stall_read_ms: ' + cast(cast((io_stall_read_ms-@tempdb_io_stall_read_ms)/(1.0+num_of_reads-@tempdb_num_of_reads) as numeric(10,1)) as varchar(20)) +@nl +
		'num_of_writes: ' + cast(num_of_writes-@tempdb_num_of_writes as varchar(20)) +@nl +
		'num_of_bytes_written: ' + cast(num_of_bytes_written-@tempdb_num_of_bytes_written as varchar(20)) + @nl +
		'io_stall_write_ms: ' + cast(io_stall_write_ms-@tempdb_io_stall_write_ms as varchar(20)) + @nl +
		'avg_io_stall_write_ms: ' + cast(cast((io_stall_write_ms-@tempdb_io_stall_write_ms)/(1.0+num_of_writes-@tempdb_num_of_writes) as numeric(10,1)) as varchar(20)) + @nl
	FROM sys.dm_io_virtual_file_stats(DB_ID('tempdb'), 1)
	PRINT @io_statistics_string
	PRINT '----'
	PRINT 'Mart I/O stats:'
	SELECT @io_statistics_string =
		'seconds for fact_schedule insert: ' + cast(datediff(SS,@mart_time_Start,getdate()) as varchar(10)) + @nl +
		'num_of_reads: ' + cast(num_of_reads-@mart_num_of_reads as varchar(20))+@nl +
		'num_of_bytes_read: ' + cast(num_of_bytes_read-@mart_num_of_bytes_read as varchar(20))+@nl +
		'io_stall_read_ms: ' + cast(io_stall_read_ms-@mart_io_stall_read_ms as varchar(20))+@nl +
		'avg_io_stall_read_ms: ' + cast(cast((io_stall_read_ms-@mart_io_stall_read_ms)/(1.0+num_of_reads-@mart_num_of_reads) as numeric(10,1)) as varchar(20)) +@nl +
		'num_of_writes: ' + cast(num_of_writes-@mart_num_of_writes as varchar(20)) +@nl +
		'num_of_bytes_written: ' + cast(num_of_bytes_written-@mart_num_of_bytes_written as varchar(20)) + @nl +
		'io_stall_write_ms: ' + cast(io_stall_write_ms-@mart_io_stall_write_ms as varchar(20)) + @nl +
		'avg_io_stall_write_ms: ' + cast(cast((io_stall_write_ms-@mart_io_stall_write_ms)/(1.0+num_of_writes-@mart_num_of_writes) as numeric(10,1)) as varchar(20)) + @nl
	FROM sys.dm_io_virtual_file_stats(DB_ID(DB_NAME()), @martfileid)
	PRINT @io_statistics_string
END
