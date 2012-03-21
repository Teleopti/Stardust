--==============================================
--http://www.brentozar.com/blitz/
--https://sqlserverperformance.wordpress.com/2011/06/30/july-2011-version-of-sql-server-2008-diagnostic-queries/
--==============================================

--------
--Bad Maintanance in our DB?
-- => EDIT!
USE TeleoptiCCC7
-------
-- Do they ahve _any_ maint? => Check our biggest table

select index_level, page_count,avg_fragmentation_in_percent, avg_page_space_used_in_percent, si.name
from sys.dm_db_index_physical_stats(db_id(),object_id('dbo.mainShiftActivityLayer'),null,null,'detailed') ps
inner join sys.indexes si
	on ps.object_id = si.object_id
	and ps.index_id = si.index_id
-- What does this mean?
-- Focus on leave level of the data pages => index_level = 0
-- If the avg_fragmentation_in_percent value is > 5% Then we start suffer from fragmentation
-- reorganize the index when "avg_fragmentation_in_percent" is > 5% but < 30%
-- rebuild the index when "avg_fragmentation_in_percent" > 30% 
-- Rebuild = more thorough defragmentation
 
 
--Old school
DBCC SHOWCONTIG('mainShiftActivityLayer')

-- Scan Density [Best Count:Actual Count]
-- Below than 90% is considered harming
-- Below than 80% is bad
--revert to master
use master
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
--Check for non existing trans log backups for full db's! (Never OR Not since last FULL)
--------
SELECT D.[name] AS [database_name], D.[recovery_model_desc] , BS1.[last_log_backup_date] 
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
WHERE 
D.[recovery_model_desc] <> 'SIMPLE' 
AND BS1.[last_log_backup_date] IS NULL OR BS1.[last_log_backup_date] < BS2.[last_data_backup_date] 
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
'ONDEMAND_TASK_QUEUE', 'BROKER_EVENTHANDLER', 'SLEEP_BPOOL_FLUSH'))
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
-- PLE is a good measurement of memory pressure.
-- Higher PLE is better. Watch the trend, not the absolute value.
SELECT cntr_value AS [Page Life Expectancy]
FROM sys.dm_os_performance_counters
WHERE
	(
	[object_name] = N'SQLServer:Buffer Manager'  --default instance
	OR
	[object_name] = N'MSSQL$' + SUBSTRING(@@SERVERNAME,(CHARINDEX('\',@@SERVERNAME,0)+1),200) + ':Buffer Manager'  --named instance
	)
AND counter_name = N'Page life expectancy' OPTION (RECOMPILE);

--Q: ms or seconds?


--Which database is using most Memory?
SELECT DB_NAME(database_id) AS [Database Name],
COUNT(*) * 8/1024.0 AS [Cached Size (MB)]
FROM sys.dm_os_buffer_descriptors
WHERE database_id > 4 -- system databases
AND database_id <> 32767 -- ResourceDB
GROUP BY DB_NAME(database_id)
ORDER BY [Cached Size (MB)] DESC OPTION (RECOMPILE);

-------
--DISK STATS
-------
--Calculates average stalls per read, per write, and per total input/output for each database file. 
--avg_io_stall_ms and should stay below 40ms!
SELECT DB_NAME(fs.database_id) AS [Database Name], mf.physical_name, io_stall_read_ms, num_of_reads,
CAST(io_stall_read_ms/(1.0 + num_of_reads) AS NUMERIC(10,1)) AS [avg_read_stall_ms],io_stall_write_ms, 
num_of_writes,CAST(io_stall_write_ms/(1.0+num_of_writes) AS NUMERIC(10,1)) AS [avg_write_stall_ms],
io_stall_read_ms + io_stall_write_ms AS [io_stalls], num_of_reads + num_of_writes AS [total_io],
CAST((io_stall_read_ms + io_stall_write_ms)/(1.0 + num_of_reads + num_of_writes) AS NUMERIC(10,1)) 
AS [avg_io_stall_ms]
FROM sys.dm_io_virtual_file_stats(null,null) AS fs
INNER JOIN sys.master_files AS mf
ON fs.database_id = mf.database_id
AND fs.[file_id] = mf.[file_id]
ORDER BY avg_io_stall_ms DESC OPTION (RECOMPILE);

