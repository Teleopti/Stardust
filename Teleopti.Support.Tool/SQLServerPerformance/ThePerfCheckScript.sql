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
where page_count > 1000 -- 1000 * 8kb = ~ 7,8Mb
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
ALTER INDEX ALL ON mart.fact_schedule_forecast_skill reorganize

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
-- io_stall_write_ms, should stay below 5ms. Idealy keep it close to 1ms
-- io_stall_read_ms,  should stay below 10ms
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

-------
--DISK STATS - now
-------
--Here we do a delta of the above disk stat select, same figures apply
-- io_stall_write_ms, should stay < 5ms. Idealy < 1ms
-- io_stall_read_ms,  should stay < 10ms
SET NOCOUNT ON
DECLARE @WaitTime datetime
SET @WaitTime = '01:00:00' -- 1 hour wait and check result.
						   -- Edit time value as you please, but make sure to include as much time so that you collect 
						   -- data from normal system usage (load from User, ETL, Agg, etc.)
						   -- "CopyWithHeaders" to Excel for future. Compare later when new disk setup or other improvments.

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
        SELECT database_id,
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
SELECT @@SERVERNAME as [ServerInstance],
		DB_NAME(vio.database_id) AS [Database],
        mf.name AS [Logical name],
        mf.type_desc AS [Type],
                (vio.io_stall_read_ms - old.io_stall_read_ms) / CASE (vio.num_of_reads-old.num_of_reads) WHEN 0 THEN 1 ELSE vio.num_of_reads-old.num_of_reads END AS [Ave read speed (ms)],
        vio.num_of_reads - old.num_of_reads AS [No of reads over period],
        CONVERT(DEC(14,2), (vio.num_of_reads - old.num_of_reads) / (@DurationInSecs * 1.00)) AS [No of reads/sec],
        CONVERT(DEC(14,2), (vio.num_of_bytes_read - old.num_of_bytes_read) / 1048576.0) AS [Tot MB read over period],
        CONVERT(DEC(14,2), ((vio.num_of_bytes_read - old.num_of_bytes_read) / 1048576.0) / @DurationInSecs) AS [Tot MB read/sec],
        (vio.num_of_bytes_read - old.num_of_bytes_read) / CASE (vio.num_of_reads-old.num_of_reads) WHEN 0 THEN 1 ELSE vio.num_of_reads-old.num_of_reads END AS [Ave read size (bytes)],
                (vio.io_stall_write_ms - old.io_stall_write_ms) / CASE (vio.num_of_writes-old.num_of_writes) WHEN 0 THEN 1 ELSE vio.num_of_writes-old.num_of_writes END AS [Ave write speed (ms)],
        vio.num_of_writes - old.num_of_writes AS [No of writes over period],
        CONVERT(DEC(14,2), (vio.num_of_writes - old.num_of_writes) / (@DurationInSecs * 1.00)) AS [No of writes/sec],
        CONVERT(DEC(14,2), (vio.num_of_bytes_written - old.num_of_bytes_written)/1048576.0) AS [Tot MB written over period],
        CONVERT(DEC(14,2), ((vio.num_of_bytes_written - old.num_of_bytes_written)/1048576.0) / @DurationInSecs) AS [Tot MB written/sec],
        (vio.num_of_bytes_written-old.num_of_bytes_written) / CASE (vio.num_of_writes-old.num_of_writes) WHEN 0 THEN 1 ELSE vio.num_of_writes-old.num_of_writes END AS [Ave write size (bytes)],
        mf.physical_name AS [Physical file name],
        size_on_disk_bytes/1048576 AS [File size on disk (MB)]
FROM sys.dm_io_virtual_file_stats (NULL, NULL) vio,
        sys.master_files mf,
        @IOStats old
WHERE mf.database_id = vio.database_id AND
        mf.file_id = vio.file_id AND
        old.database_id = vio.database_id AND
        old.file_id = vio.file_id AND
        ((vio.num_of_bytes_read - old.num_of_bytes_read) + (vio.num_of_bytes_written - old.num_of_bytes_written)) > 0
ORDER BY ((vio.num_of_bytes_read - old.num_of_bytes_read) + (vio.num_of_bytes_written - old.num_of_bytes_written)) DESC
GO
