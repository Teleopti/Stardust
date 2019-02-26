--==============================================
--http://www.brentozar.com/blitz/
--https://sqlserverperformance.wordpress.com/2011/06/30/july-2011-version-of-sql-server-2008-diagnostic-queries/
--==============================================

--------
--Bad Maintanance in our DB?
-------
-- ===== Last REBUILD =====
-- Do they have any regular maintenance of indexes?
-- When did the last REBUILD happen?
-- modify_date reflects the last "rebuild"-date, (since we don't make any other ALTER on tables)
-- http://msdn.microsoft.com/en-us/library/ms190324.aspx
-- "Date the object was last modified by using an ALTER statement. If the object is a table or a view, modify_date also changes when a clustered index on the table or view is created or altered."
-- note: "reorganize" the table/index will not(!) shoe in the "modify_date" value, so we can't tell in that case :-(

Select schemaname =sch.name, objectname=object_name(i.object_id),indexname=i.name, i.index_id, o.create_date, o.modify_date
from sys.indexes i, sys.objects o
inner join sys.schemas sch
	on sch.schema_id = o.schema_id
where objectproperty(o.object_id,'IsUserTable') = 1
and i.index_id NOT IN (
	select s.index_id
	from sys.dm_db_index_usage_stats s
	where s.object_id=i.object_id and
	i.index_id=s.index_id and
	database_id = db_id()
	)
and o.object_id = i.object_id
order by modify_date desc

-- ===== fragmented tables =====
-- Bad maintanance will give you high values over time.
-- But heavy insert/delete activites (RTA, stage tables) will also result in fragmented tables. Not much we can do about that
-- For each of the Teleopti databases run this query and save the result
-- Note: this query is in "SAMPLED" mode, still it will read alot from the database. Handle with care.
select object_name(ps.object_id) as table_name,index_level, page_count,avg_fragmentation_in_percent, avg_page_space_used_in_percent, si.name
from sys.dm_db_index_physical_stats(db_id(),object_id(''),null,null,'SAMPLED') ps
inner join sys.indexes si
	on ps.object_id = si.object_id
	and ps.index_id = si.index_id
where page_count > 100 -- 100 * 8kb = ~ 800Kb
order by avg_fragmentation_in_percent desc

-- What does this mean?
/*
==============================================================================================
index_level	page_count			avg_fragmentation_in_percent	avg_page_space_used_in_percent	name
==============================================================================================
0	44849	59,1027670628108	75,3869532987398				PK_stg_schedule						<-- super critical, the table is all messed up, Use REBUILD INDEX
0	184526	14,347571615924		94,1472448727452				PK_fact_schedule					<-- bad, Use REORGINIZE
0	33964	9,85160758450124	98,1105880899432				IX_fact_schedule_Shift_startdate_id	<-- bad, Use REORGINIZE
0	28945	4,20452582484021	99,7906473931307				IX_person_scenario_schdate_interval	<-- ok from here and below
0	35143	1,78698460575364	99,5648505065481				PK_stg_time_zone_bridge
0	5012	1,23703112529928	97,7257474672597				PK_stg_schedule_forecast_skill
0	8626	0,64920009274287	99,8003829997529				IX_bridge_time_zone_local_date_id_local_interval_id
0	14224	0,47103487064117	99,864096861873					PK_bridge_time_zone
0	1622	0,30826140567201	91,8131578947368				PK_dim_person
==============================================================================================
*/

-- Focus on leave level of the data pages => index_level = 0
-- http://msdn.microsoft.com/en-us/library/ms189858.aspx
-- If the avg_fragmentation_in_percent value is > 5% Then we start suffer from fragmentation
-- reorganize the index when "avg_fragmentation_in_percent" is > 5% but < 30%
-- rebuild the index when "avg_fragmentation_in_percent" > 30% 
-- Rebuild = more thorough defragmentation

-- ===== rebuild a table and it indexes ===== 
-- Ok, this is not our business ...
-- Rebuilding index consumes a lot of I/O and transaction log space. This is not Teleopti stuff. Hand over to customer IT.
-- .. but this is how you REBUILD a table (the clustered index inself) plus all other (non-clustered index) on that specific table
ALTER INDEX ALL ON mart.fact_schedule_forecast_skill reorganize --if avg_fragmentation_in_percent value > 5% and < = 30%
ALTER INDEX ALL ON mart.fact_schedule_forecast_skill rebuild --if avg_fragmentation_in_percent value > 30%

---------
-- queries access the master database or msdb
----------
USE master
GO

---------
-- File Names and Paths for TempDB and all user databases in instance 
----------
SELECT DB_NAME([database_id])AS [Database Name], 
       [file_id], name, physical_name, type_desc, state_desc, 
       CONVERT( bigint, size/128.0) AS [Total Size in MB]
FROM sys.master_files
WHERE [database_id] > 4 
AND [database_id] <> 32767
OR [database_id] = 2
ORDER BY DB_NAME([database_id]) OPTION (RECOMPILE);

-- Things to look at:
-- Are data files and log files on different drives?
-- Is everything on the C: drive?
-- Is TempDB on dedicated drives?
-- Are there multiple data files?

--------
--Show all database with last FULL vs. LOG Backup
--------
SELECT
	D.[name] AS [database_name],
	D.[recovery_model_desc],
	BS2.last_data_backup_date 'last full backup',
	BS1.last_log_backup_date 'last log backup',
	CASE 
	WHEN (CASE WHEN cast(BS2.last_data_backup_date as nvarchar(20)) IS NULL THEN 1 ELSE 0 END)
		* (CASE WHEN D.recovery_model_desc='FULL' THEN 1 ELSE 0 END)
		= 1 THEN 'WARNING! - FULL Backup missing'
	WHEN (CASE WHEN cast(BS1.last_log_backup_date as nvarchar(20)) IS NULL THEN 1 ELSE 0 END)
		* (CASE WHEN D.recovery_model_desc='FULL' THEN 1 ELSE 0 END)
		= 1 THEN 'WARNING! - LOG Backup missing'
	ELSE ''
	END AS 'backup issue'

FROM 
sys.databases D LEFT JOIN  
( 
   SELECT BS.[database_name],  
   MAX(BS.[backup_finish_date]) AS [last_log_backup_date]  
   FROM msdb.dbo.backupset BS  
   WHERE BS.type = 'L'  
   GROUP BY BS.[database_name]  
   ) BS1  
ON D.[name] = BS1.[database_name] 
LEFT JOIN  
( 
   SELECT BS.[database_name],  
   MAX(BS.[backup_finish_date]) AS [last_data_backup_date]  
   FROM msdb.dbo.backupset BS  
   WHERE BS.type = 'D'  
   GROUP BY BS.[database_name]  
) BS2  
ON D.[name] = BS2.[database_name] 
WHERE D.[name] NOT IN ('model','tempdb')
ORDER BY D.[name];



----------
-- Recovery model, log reuse wait description, log file size, log usage size and compatibility level for all databases on instance
----------
SELECT db.[name] AS [Database Name], db.recovery_model_desc AS [Recovery Model], 
db.log_reuse_wait_desc AS [Log Reuse Wait Description], 
ls.cntr_value AS [Log Size (KB)], lu.cntr_value AS [Log Used (KB)],
CAST(CAST(lu.cntr_value AS FLOAT) / CAST(ls.cntr_value AS FLOAT)AS DECIMAL(18,2)) * 100 AS [Log Used %], 
db.[compatibility_level] AS [DB Compatibility Level], 
db.page_verify_option_desc AS [Page Verify Option], db.is_auto_create_stats_on, db.is_auto_update_stats_on,
db.is_auto_update_stats_async_on, db.is_parameterization_forced, 
db.snapshot_isolation_state_desc, db.is_read_committed_snapshot_on
FROM sys.databases AS db
INNER JOIN sys.dm_os_performance_counters AS lu 
ON db.name = lu.instance_name
INNER JOIN sys.dm_os_performance_counters AS ls 
ON db.name = ls.instance_name
WHERE lu.counter_name LIKE N'Log File(s) Used Size (KB)%' 
AND ls.counter_name LIKE N'Log File(s) Size (KB)%'
AND ls.cntr_value > 0 OPTION (RECOMPILE);

-- Things to look at:
-- How many databases are on the instance?
-- What recovery models are they using?
-- What is the log reuse wait description?
-- How full are the transaction logs ?
-- What compatibility level are they on?

----------
-- TOP 10 most expensive queries
----------
Declare @nl Char(2)  Set @nl  = char(13) + char(10)
Declare @tab Char(1) Set @tab = char(9)

--by Duration
SELECT TOP 10
	[Avg. MultiCore/CPU time(sec)] = qs.total_worker_time / 1000000 / qs.execution_count,
	[Total MultiCore/CPU time(sec)] = qs.total_worker_time / 1000000,
	[Avg. Elapsed Time(sec)] = qs.total_elapsed_time / 1000000 / qs.execution_count,
	[Total Elapsed Time(sec)] = qs.total_elapsed_time / 1000000,
	qs.execution_count,
	[Avg. I/O] = (total_logical_reads + total_logical_writes) / qs.execution_count,
	[Total I/O] = total_logical_reads + total_logical_writes,
	Query = replace(replace(SUBSTRING(qt.[text], (qs.statement_start_offset / 2) + 1,
		(
			(
				CASE qs.statement_end_offset
					WHEN -1 THEN DATALENGTH(qt.[text])
					ELSE qs.statement_end_offset
				END - qs.statement_start_offset
			) / 2
		) + 1
	),@tab,' '),@nl,' '), 
	Batch = replace(replace(qt.[text],@tab,' '),@nl,' '),
	[DB] = DB_NAME(qt.[dbid]),
	qs.last_execution_time
--	,qp.query_plan --Excluded in order to "Copy With Headers" to Excel. Might be of greate interest when looking into performance issues!
FROM sys.dm_exec_query_stats AS qs
CROSS APPLY sys.dm_exec_sql_text(qs.[sql_handle]) AS qt
CROSS APPLY sys.dm_exec_query_plan(qs.plan_handle) AS qp
where qs.execution_count > 5	--more than 5 occurences
ORDER BY [Total Elapsed Time(sec)] DESC

--by CPU
SELECT TOP 10
	[Avg. MultiCore/CPU time(sec)] = qs.total_worker_time / 1000000 / qs.execution_count,
	[Total MultiCore/CPU time(sec)] = qs.total_worker_time / 1000000,
	[Avg. Elapsed Time(sec)] = qs.total_elapsed_time / 1000000 / qs.execution_count,
	[Total Elapsed Time(sec)] = qs.total_elapsed_time / 1000000,
	qs.execution_count,
	[Avg. I/O] = (total_logical_reads + total_logical_writes) / qs.execution_count,
	[Total I/O] = total_logical_reads + total_logical_writes,
	Query = replace(replace(SUBSTRING(qt.[text], (qs.statement_start_offset / 2) + 1,
		(
			(
				CASE qs.statement_end_offset
					WHEN -1 THEN DATALENGTH(qt.[text])
					ELSE qs.statement_end_offset
				END - qs.statement_start_offset
			) / 2
		) + 1
	),@tab,' '),@nl,' '), 
	Batch = replace(replace(qt.[text],@tab,' '),@nl,' '),
	[DB] = DB_NAME(qt.[dbid]),
	qs.last_execution_time
--	,qp.query_plan --Excluded in order to "Copy With Headers" to Excel. Might be of greate interest when looking into performance issues!
FROM sys.dm_exec_query_stats AS qs
CROSS APPLY sys.dm_exec_sql_text(qs.[sql_handle]) AS qt
CROSS APPLY sys.dm_exec_query_plan(qs.plan_handle) AS qp
where qs.execution_count > 5	--more than 5 occurences
ORDER BY [Total MultiCore/CPU time(sec)] DESC

--by  logical I/O (RAM)
SELECT TOP 10
	[Avg. MultiCore/CPU time(sec)] = qs.total_worker_time / 1000000 / qs.execution_count,
	[Total MultiCore/CPU time(sec)] = qs.total_worker_time / 1000000,
	[Avg. Elapsed Time(sec)] = qs.total_elapsed_time / 1000000 / qs.execution_count,
	[Total Elapsed Time(sec)] = qs.total_elapsed_time / 1000000,
	qs.execution_count,
	[Avg. I/O] = (total_logical_reads + total_logical_writes) / qs.execution_count,
	[Total I/O] = total_logical_reads + total_logical_writes,
	Query = replace(replace(SUBSTRING(qt.[text], (qs.statement_start_offset / 2) + 1,
		(
			(
				CASE qs.statement_end_offset
					WHEN -1 THEN DATALENGTH(qt.[text])
					ELSE qs.statement_end_offset
				END - qs.statement_start_offset
			) / 2
		) + 1
	),@tab,' '),@nl,' '), 
	Batch = replace(replace(qt.[text],@tab,' '),@nl,' '),
	[DB] = DB_NAME(qt.[dbid]),
	qs.last_execution_time
--	,qp.query_plan --Excluded in order to "Copy With Headers" to Excel. Might be of greate interest when looking into performance issues!
FROM sys.dm_exec_query_stats AS qs
CROSS APPLY sys.dm_exec_sql_text(qs.[sql_handle]) AS qt
CROSS APPLY sys.dm_exec_query_plan(qs.plan_handle) AS qp
where qs.execution_count > 5	--more than 5 occurences
ORDER BY [Total I/O] DESC

--------
-- waitstats
--------
WITH Waits AS
(SELECT wait_type, wait_time_ms / 1000. AS wait_time_s,
100. * wait_time_ms / SUM(wait_time_ms) OVER() AS pct,
ROW_NUMBER() OVER(ORDER BY wait_time_ms DESC) AS rn
FROM sys.dm_os_wait_stats
WHERE wait_type NOT IN ('CLR_SEMAPHORE','LAZYWRITER_SLEEP','RESOURCE_QUEUE','SLEEP_TASK',
'SLEEP_SYSTEMTASK','SQLTRACE_BUFFER_FLUSH','WAITFOR', 'LOGMGR_QUEUE','CHECKPOINT_QUEUE',
'REQUEST_FOR_DEADLOCK_SEARCH','XE_TIMER_EVENT','BROKER_TO_FLUSH','BROKER_TASK_STOP','CLR_MANUAL_EVENT',
'CLR_AUTO_EVENT','DISPATCHER_QUEUE_SEMAPHORE', 'FT_IFTS_SCHEDULER_IDLE_WAIT',
'XE_DISPATCHER_WAIT', 'XE_DISPATCHER_JOIN', 'SQLTRACE_INCREMENTAL_FLUSH_SLEEP',
'ONDEMAND_TASK_QUEUE', 'BROKER_EVENTHANDLER', 'SLEEP_BPOOL_FLUSH','HADR_FILESTREAM_IOMGR_IOCOMPLETION','DIRTY_PAGE_POLL'))
SELECT W1.wait_type, 
CAST(W1.wait_time_s AS DECIMAL(12, 2)) AS wait_time_s,
CAST(W1.pct AS DECIMAL(12, 2)) AS pct,
CAST(SUM(W2.pct) AS DECIMAL(12, 2)) AS running_pct
FROM Waits AS W1
INNER JOIN Waits AS W2
ON W2.rn <= W1.rn
GROUP BY W1.rn, W1.wait_type, W1.wait_time_s, W1.pct
HAVING SUM(W2.pct) - W1.pct < 99 OPTION (RECOMPILE); -- percentage threshold

--Q: Difference between: pct vs. running_pct?

-- Isolate top waits for server instance since last restart or statistics clear
-- i.e. what is MSSQLSERVER spending time doing
-- See: http://msdn.microsoft.com/en-us/library/ms179984.aspx
-- If you want fresh stats issue DBCC SQLPERF('sys.dm_os_wait_stats', CLEAR) to reset the stats


--Q: When did last reset take place?
-------
--CPU STATS
-------
-- Get CPU utilization by database
-- Just to see if there are other non-Teleopti databases using CPU
WITH DB_CPU_Stats
AS
(SELECT DatabaseID, DB_Name(DatabaseID) AS [DatabaseName], SUM(total_worker_time) AS [CPU_Time_Ms]
 FROM sys.dm_exec_query_stats AS qs
 CROSS APPLY (SELECT CONVERT(int, value) AS [DatabaseID] 
              FROM sys.dm_exec_plan_attributes(qs.plan_handle)
              WHERE attribute = N'dbid') AS F_DB
 GROUP BY DatabaseID)
SELECT ROW_NUMBER() OVER(ORDER BY [CPU_Time_Ms] DESC) AS [row_num],
       DatabaseName, [CPU_Time_Ms], 
       CAST([CPU_Time_Ms] * 1.0 / SUM([CPU_Time_Ms]) OVER() * 100.0 AS DECIMAL(5, 2)) AS [CPUPercent]
FROM DB_CPU_Stats
WHERE DatabaseID > 4 -- system databases
AND DatabaseID <> 32767 -- ResourceDB
ORDER BY row_num OPTION (RECOMPILE);

-- Signal Waits for instance
-- Signal Waits above 10-15% is usually a sign of CPU pressure
SELECT CAST(100.0 * SUM(signal_wait_time_ms) / SUM (wait_time_ms) AS NUMERIC(20,2)) 
AS [%signal (cpu) waits],
CAST(100.0 * SUM(wait_time_ms - signal_wait_time_ms) / SUM (wait_time_ms) AS NUMERIC(20,2)) 
AS [%resource waits]
FROM sys.dm_os_wait_stats OPTION (RECOMPILE);

---------
--MEMORY STATS
---------
-- Page Life Expectancy (PLE) value for default instance
-- It represents the number of seconds data stays in memory at current client load
-- PLE is a good measurement of memory pressure.
-- Higher PLE is better. Watch the trend, not the absolute value.
SELECT cntr_value AS [Page Life Expectancy (sec)]
FROM sys.dm_os_performance_counters
WHERE
	(
	[object_name] = N'SQLServer:Buffer Manager'  --default instance
	OR
	[object_name] = N'MSSQL$' + SUBSTRING(@@SERVERNAME,(CHARINDEX('\',@@SERVERNAME,0)+1),200) + ':Buffer Manager'  --named instance
	)
AND counter_name = N'Page life expectancy' OPTION (RECOMPILE);



--Which database is using most Memory?
SELECT DB_NAME(database_id) AS [Database Name],
COUNT(*) * 8/1024.0 AS [Cached Size (MB)]
FROM sys.dm_os_buffer_descriptors
WHERE database_id > 4 -- system databases
AND database_id <> 32767 -- ResourceDB
GROUP BY DB_NAME(database_id)
ORDER BY [Cached Size (MB)] DESC OPTION (RECOMPILE);

-------
--DISK STATS - historic
-------
--Calculates average stalls per read, per write, and per total input/output for each database file. 
--Our System requirements states "latency"
--[avg write latency (ms)] < 5ms (Ideally < 1ms)
--[avg read latency (ms)] < 15ms
--ETL-Nighly + DB-maint + DB-backups + other massvive SAN I/O might skew figures over night.
--please compare with the "now"-script 30-60 min (below) during office hours to get a better picture

declare @DurationInSecs int
select @DurationInSecs=datediff(ss,sqlserver_start_time, GETDATE()) FROM sys.dm_os_sys_info --SQL Server starttime

SELECT 
mf.name AS [Logical name],
--read  figures
CONVERT(DEC(14,2), fs.num_of_bytes_read/1024/1024/@DurationInSecs) as [Mb read/sec],
CONVERT(DEC(14,2), fs.num_of_reads/@DurationInSecs) as [read IOPS],
CASE fs.num_of_reads
	WHEN 0 THEN 0 ELSE fs.io_stall_read_ms/fs.num_of_reads
END as [avg read latency (ms)],
	
--write figures
CONVERT(DEC(14,2), fs.num_of_bytes_written/1024/1024/@DurationInSecs) as [Mb written/sec],
CONVERT(DEC(14,2), fs.num_of_writes/@DurationInSecs) as [write IOPS],
CASE fs.num_of_writes
	WHEN 0 THEN 0 ELSE fs.io_stall_write_ms/fs.num_of_writes
END as [avg write latency (ms)],

DB_NAME(fs.database_id) AS [Database Name], mf.physical_name, io_stall_read_ms, num_of_reads,
io_stall_write_ms, 
num_of_writes,
io_stall_read_ms + io_stall_write_ms AS [io_stalls],
num_of_reads + num_of_writes AS [total_io]
FROM sys.dm_io_virtual_file_stats(null,null) AS fs
INNER JOIN sys.master_files AS mf
ON fs.database_id = mf.database_id
AND fs.[file_id] = mf.[file_id]
ORDER BY num_of_writes DESC OPTION (RECOMPILE);
GO


-------
--DISK STATS - now
-------
/*
Calculates and present the I/O figures summed up over all disks, grouped by drive letter, grouped by file.
It will not cost any overhead I/O on the server to run this script.
Default is 30 min. But the longer the better (to catch ETL and CTI I/O)

Our System requirements states "latency"
[avg write latency (ms)] < 5ms (Ideally < 1ms)
[avg read latency (ms)] < 15ms

NOTE: The latency figures must(!) stay under these values. If they don't - we are in a situation where we probably should stop supporting the customer.
Ask the customer to upgrade their storage or add more RAM to mitigate the problem.

However please make sure we don't saturate (overload) the disks!
e.g you get something like:
All Drives	0.20	OK	146.29	Warning!	18	Warning!

In that case Teleopti have some piece of code that put too much I/O load on the server. If the disk load is greater than its capability to satisfy the latency requirements above it's a Teleopti problem! Find the most expensive I/O query and report it to PS Tech R&D.

These are the values used to detect such a condition:
[Kbyte read/sec/agent] < 7.5
[Kbyte written/sec/agent] < 2.5
[IOPS/agent] < 1.0

*/
--> Run in your App database, in order to fetch current number of agents
SET NOCOUNT ON
DECLARE @WaitTime datetime
DECLARE @Agents int
SET @WaitTime = '00:30:00'

--current threshold for I/O
DECLARE @readOverloadPerAgent decimal(2,1) = 7.5
DECLARE @writeOverloadPerAgent decimal(2,1) = 2.5
DECLARE @IOPSOverloadPerAgent decimal(2,1) = 1.0
DECLARE @OK as varchar(5) = 'OK'
DECLARE @Warning as varchar(20) = 'Warning!'
SET @Agents = (
	SELECT COUNT(*)
	from dbo.personPeriod pp 
	inner join dbo.person p
	on p.Id = pp.Parent
	where getdate() between pp.StartDate and pp.EndDate
	and p.isDeleted = 0
)

IF OBJECT_ID('tempdb..##lastResult') IS NOT NULL DROP TABLE ##lastResult

DECLARE @IOStats TABLE (
        [database_id] [smallint] NOT NULL,
        [file_id] [smallint] NOT NULL,
        [num_of_reads] [bigint] NOT NULL,
        [num_of_bytes_read] [bigint] NOT NULL,
        [io_stall_read_ms] [bigint] NOT NULL,
        [num_of_writes] [bigint] NOT NULL,
        [num_of_bytes_written] [bigint] NOT NULL,
        [io_stall_write_ms] [bigint] NOT NULL)
INSERT INTO @IOStats
SELECT
	database_id,
	vio.file_id,
	num_of_reads,
	num_of_bytes_read,
	io_stall_read_ms,
	num_of_writes,
	num_of_bytes_written,
	io_stall_write_ms
FROM sys.dm_io_virtual_file_stats (NULL, NULL) vio
DECLARE @StartTime datetime, @DurationInSecs int
SET @StartTime = GETDATE()
WAITFOR DELAY @WaitTime
SET @DurationInSecs = DATEDIFF(ss, @startTime, GETDATE())

SELECT
	mf.name AS [Logical name],

	--read  figures
	CONVERT(DEC(14,2), ((vio.num_of_bytes_read - old.num_of_bytes_read) / 1048576.0) / @DurationInSecs) AS [Mb read/sec],
	CONVERT(DEC(14,2), (vio.num_of_reads - old.num_of_reads) / (@DurationInSecs * 1.00)) AS [reads IOPS],
	(vio.io_stall_read_ms - old.io_stall_read_ms) / CASE (vio.num_of_reads-old.num_of_reads) WHEN 0 THEN 1 ELSE vio.num_of_reads-old.num_of_reads END AS [avg read latency (ms)],

	--write figures
	CONVERT(DEC(14,2), ((vio.num_of_bytes_written - old.num_of_bytes_written)/1024/1024) / @DurationInSecs) AS [Mb written/sec],
	CONVERT(DEC(14,2), (vio.num_of_writes - old.num_of_writes) / (@DurationInSecs * 1.00)) AS [write IOPS],
	(vio.io_stall_write_ms - old.io_stall_write_ms) / CASE (vio.num_of_writes-old.num_of_writes) WHEN 0 THEN 1 ELSE vio.num_of_writes-old.num_of_writes END AS [avg write latency (ms)],

	--extra info
	(vio.io_stall_read_ms - old.io_stall_read_ms) AS [Tot read stall ms],
	vio.num_of_reads - old.num_of_reads AS [No of reads over period],
	CONVERT(DEC(14,2), (vio.num_of_bytes_read - old.num_of_bytes_read)/1024/1024) AS [Tot MB read over period],
	(vio.num_of_bytes_read - old.num_of_bytes_read) / CASE (vio.num_of_reads-old.num_of_reads) WHEN 0 THEN 1 ELSE vio.num_of_reads-old.num_of_reads END AS [avg read size (bytes)],
	(vio.io_stall_write_ms - old.io_stall_write_ms) AS [Tot write stall ms],
	vio.num_of_writes - old.num_of_writes AS [No of writes over period],
	CONVERT(DEC(14,2), (vio.num_of_bytes_written - old.num_of_bytes_written)/1024/1024) AS [Tot MB written over period],
	(vio.num_of_bytes_written-old.num_of_bytes_written) / CASE (vio.num_of_writes-old.num_of_writes) WHEN 0 THEN 1 ELSE vio.num_of_writes-old.num_of_writes END AS [avg write size (bytes)],
	mf.physical_name AS [Physical file name],
	size_on_disk_bytes/1048576 AS [File size on disk (MB)],
	DB_NAME(vio.database_id) AS [Database],
	mf.type_desc AS [Type]
INTO ##lastResult
FROM sys.dm_io_virtual_file_stats (NULL, NULL) vio,
        sys.master_files mf,
        @IOStats old
WHERE mf.database_id = vio.database_id AND
        mf.file_id = vio.file_id AND
        old.database_id = vio.database_id AND
        old.file_id = vio.file_id AND
        ((vio.num_of_bytes_read - old.num_of_bytes_read) + (vio.num_of_bytes_written - old.num_of_bytes_written)) > 0
ORDER BY ((vio.num_of_bytes_read - old.num_of_bytes_read) + (vio.num_of_bytes_written - old.num_of_bytes_written)) DESC

--overloaded?
SELECT
	'All Drives' as [Drive letter],
	CONVERT(DEC(14,2), sum([Mb read/sec])*1024/@Agents) as [kbytes read/sec/agent],
	CASE WHEN CONVERT(DEC(14,2), sum([Mb read/sec])*1024/@Agents) > @readOverloadPerAgent THEN @Warning
		ELSE @OK
	END as [Teleopti reads overload],
	CONVERT(DEC(14,2), sum([Mb written/sec])*1024/@Agents) as [kbytes written/sec/agent],
	CASE WHEN CONVERT(DEC(14,2), sum([Mb written/sec])*1024/@Agents) > @writeOverloadPerAgent THEN @Warning
		ELSE @OK
	END as [Teleopti write overload],
	sum([No of reads over period] + [No of writes over period])/@DurationInSecs/@Agents as [IOPS/agent],
	CASE WHEN sum([No of reads over period] + [No of writes over period])/@DurationInSecs/@Agents > @IOPSOverloadPerAgent THEN @Warning
		ELSE @OK
	END as [Teleopti IOPS overload]
FROM ##lastResult

--over all drives
SELECT
	'All Drives' as [Drive letter],
	CONVERT(DEC(14,2), sum([Tot MB read over period])/@DurationInSecs) as [Mb read/sec],
	CONVERT(DEC(14,2), sum([No of reads over period])/@DurationInSecs) as [read IOPS],
	CASE sum([No of reads over period])
		WHEN 0 THEN 0 ELSE sum([Tot read stall ms])/sum([No of reads over period])
	END as [avg read latency (ms)],

	CONVERT(DEC(14,2), sum([Tot MB written over period])/@DurationInSecs) as [Mb written/sec],
	CONVERT(DEC(14,2), sum([No of writes over period])/@DurationInSecs) as [write IOPS],
	CASE sum([No of writes over period])
		WHEN 0 THEN 0 ELSE sum([Tot write stall ms])/sum([No of writes over period])
	END as [avg write latency (ms)]
FROM ##lastResult

--by drive
SELECT
	SUBSTRING([Physical file name],1,2) as [Drive letter],
	CONVERT(DEC(14,2), sum([Tot MB read over period])/@DurationInSecs) as [Mb read/sec],
	CONVERT(DEC(14,2), sum([No of reads over period])/@DurationInSecs) as [read IOPS],
	CASE sum([No of reads over period])
		WHEN 0 THEN 0 ELSE sum([Tot read stall ms])/sum([No of reads over period])
	END as [Avg read latency (ms)],

	CONVERT(DEC(14,2), sum([Tot MB written over period])/@DurationInSecs) as [Mb written/sec],
	CONVERT(DEC(14,2), sum([No of writes over period])/@DurationInSecs) as [write IOPS],
	CASE sum([No of writes over period])
		WHEN 0 THEN 0 ELSE sum([Tot write stall ms])/sum([No of writes over period])
	END as [avg write latency (ms)]
FROM ##lastResult
GROUP BY SUBSTRING([Physical file name],1,2)

--by file
SELECT * FROM ##lastResult


-------
-- DISK STATS - Every week (bugs exists) Please report!
-------
-- We now call a Teleopti Stored Procedure in Analytics to collect and save the data.
-- EXEC [dbo].[DBA_VirtualFilestats_Load]
-- Then query the statistics via => select * from dbo.DBA_VirtualFileStatsHistory

--settings for data collection
DECLARE @Duration datetime
DECLARE @IntervalInSeconds int
DECLARE @DateFrom datetime

SET @Duration='02:00:00' --2 hour. default 24 hours in SP
SET @IntervalInSeconds=300 --every 5 minutes

--collect data
EXEC [dbo].[DBA_VirtualFilestats_Load] @Duration=@Duration,@IntervalInSeconds=@IntervalInSeconds

--view all data
--settings for data collection
EXEC [dbo].DBA_report_data_IO_By_Interval @DateFrom='2001-12-31',@DateTo='2059-12-31'

--view all result
--EXEC [dbo].DBA_report_data_IO_By_Interval @DateFrom='1900-01-01',@DateTo='2059-12-31'

/*
The result will show four result set:
1) sum of IO stats grouped by hour
One line per hour, over all IO stats
note: this output depends on that we run the "collect data" for 24 consecutive hours

2) sum of IO stats grouped database file
One line for each file with any file activity

3) top 3 worst interval by Total IOPS
Shows the worst interval, worst file

4) top 3 worst interval by "Total write latency"
Shows the worst interval, worst file

From Teleopti WFM Pre-reqs:
SQL Server storage MUST be configured properly for high IO performance.
Under sustained load "Ave read speed (ms)" must be <=5ms and "Ave read speed (ms)"  <=20ms. 
To achieve this there must at least be separate physical disks (and controllers) for 
system files, database files, temp db and database log files. 
RAID-10 or equivalent can be used for improved reliability of database server (this is  not a requirement).
Useful reading:
http://searchsqlserver.techtarget.com/tip/0,289483,sid87_gci1262122,00.html
http://msdn.microsoft.com/en-us/library/cc966412.aspx
If SAN storage is used it MUST be configured correct for high IO performance. Useful reading:
http://www.brentozar.com/sql/sql-server-san-best-practices/
*/

-------
-- DISK STATS - Add job
-------
-- This job will collect IO stats every tuesday (00:00), 24 hours, every 15 minutes
USE [msdb]
GO
IF EXISTS (select 1 from sys.databases where name='TeleoptiAnalytics')
BEGIN
	DECLARE @server_name nvarchar(30)
	SELECT @server_name = CAST(SERVERPROPERTY('ServerName') AS nvarchar(30))

	DECLARE @job_name sysname
	SET @job_name=N'Teleopti_VirtualFileStatsHistory'

	--Drop existing Job
	IF  EXISTS (SELECT job_id FROM msdb.dbo.sysjobs_view WHERE name = @job_name)
	EXEC msdb.dbo.sp_delete_job @job_name=@job_name, @delete_unused_schedule=1

	--Add Job
	EXEC  msdb.dbo.sp_add_job @job_name=@job_name, 
			@enabled=1, @notify_level_eventlog=0, @notify_level_email=2, @notify_level_netsend=2, @notify_level_page=2, @delete_level=0, 
			@description=N'Collects IO statistics from [sys].[Dm_io_virtual_file_stats]', @category_name=N'[Uncategorized (Local)]', @owner_login_name=N'sa'

	EXEC msdb.dbo.sp_add_jobserver @job_name=@job_name, @server_name = @server_name

	EXEC msdb.dbo.sp_add_jobstep @job_name=@job_name, @step_name=N'Start collect', 
			@step_id=1, @cmdexec_success_code=0, @on_success_action=1, @on_fail_action=2, @retry_attempts=0, @retry_interval=0, @os_run_priority=0,
			@subsystem=N'TSQL', @command=N'EXEC TeleoptiAnalytics.[dbo].[DBA_VirtualFilestats_Load]', @database_name=N'master', @flags=0

	EXEC msdb.dbo.sp_update_job @job_name=@job_name, 
			@enabled=1, @start_step_id=1, @notify_level_eventlog=0, @notify_level_email=2, @notify_level_netsend=2, @notify_level_page=2, @delete_level=0, 
			@description=N'Collects IO statistics from [sys].[Dm_io_virtual_file_stats]', @category_name=N'[Uncategorized (Local)]', @owner_login_name=N'sa'

	EXEC msdb.dbo.sp_add_jobschedule @job_name=@job_name, @name=N'Once a week', 
			@enabled=1,@freq_type=8, @freq_interval=4, @freq_subday_type=1, @freq_subday_interval=0, @freq_relative_interval=0, @freq_recurrence_factor=1, 
			@active_start_date=20131009, @active_end_date=99991231, @active_start_time=0, @active_end_time=235959
END
ELSE
PRINT 'Sorry, I cannot find the TeleoptiAnalytics database. Please edit the script to point to an existing TeleoptiAnalytics database.'