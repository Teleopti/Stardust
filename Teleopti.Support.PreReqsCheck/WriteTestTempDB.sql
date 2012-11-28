SET NOCOUNT ON

--declares
DECLARE @rows int
DECLARE @toKeep int
DECLARE @i int
DECLARE @j int
DECLARE @StartTime datetime
DECLARE @DurationInMilisecs int

DECLARE @IOStats TABLE (
        [database_id] [smallint] NOT NULL,
        [file_id] [smallint] NOT NULL,
        [num_of_reads] [bigint] NOT NULL,
        [num_of_bytes_read] [bigint] NOT NULL,
        [io_stall_read_ms] [bigint] NOT NULL,
        [num_of_writes] [bigint] NOT NULL,
        [num_of_bytes_written] [bigint] NOT NULL,
        [io_stall_write_ms] [bigint] NOT NULL)
        
--spt_values (corresponding to master.dbo.spt_values)
CREATE TABLE #numbers(intCol int NOT NULL)

--Read and write storage (physical in tempdb)
CREATE TABLE #NetworkRunnerRead (
	x int NOT NULL
	,x2 int NOT NULL
	,y char(10) NOT NULL DEFAULT ('')
	,z char(10) NOT NULL DEFAULT('')
	)

CREATE UNIQUE CLUSTERED INDEX IX_NetworkRunnerRead ON #NetworkRunnerRead (x)

--init
SET @j = 2500
SET @i = 0

--init
SELECT @rows = 100000 --100.000 will return ~63 Mb of data
SELECT @toKeep = @rows*2/3

--messure IOstall during this procedure
SET @StartTime = GETDATE()

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

--fill up the temp table with data
WHILE @i < @j
BEGIN            
	INSERT #numbers (intCol)            
	VALUES (@i)
	SET @i = @i + 1
CONTINUE            
END

--insert dummy values
INSERT #NetworkRunnerRead  (x, x2)
SELECT TOP(@rows)
ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS r
,ROW_NUMBER() OVER (ORDER BY (SELECT 1)) % 10 AS s
FROM #numbers a CROSS JOIN #numbers b

--Create some potential page splits
ALTER TABLE #NetworkRunnerRead ALTER COLUMN y char(892)
ALTER TABLE #NetworkRunnerRead ALTER COLUMN z char(100)

--Put some more dummy data into the table
UPDATE #NetworkRunnerRead
SET y=NEWID(),z=NEWID()

--fragment the table
DELETE TOP(@rows - @toKeep) FROM #NetworkRunnerRead WHERE x2 IN(2, 4, 6, 8)

--Result Set #1
--return number of bytes
SELECT CAST(SUM(DATALENGTH(x)+DATALENGTH(x2)+DATALENGTH(y)+DATALENGTH(z)) AS INT) AS NumberOfBytes
FROM #NetworkRunnerRead 

--Result Set #2
--return all rows tp client
SELECT * FROM #NetworkRunnerRead

SET @DurationInMilisecs = DATEDIFF(ms, @startTime, GETDATE())

--Result Set #3
SELECT 
        /*
        mf.name AS [Logical name],
		(vio.io_stall_read_ms - old.io_stall_read_ms) / CASE (vio.num_of_reads-old.num_of_reads) WHEN 0 THEN 1 ELSE vio.num_of_reads-old.num_of_reads END AS [Avg read speed (ms)],
        vio.num_of_reads - old.num_of_reads AS [No of reads over period],
        CONVERT(DEC(14,2), (vio.num_of_reads - old.num_of_reads) * 1000.00 / (@DurationInMilisecs)) AS [No of reads/sec],
        CONVERT(DEC(14,2), (vio.num_of_bytes_read - old.num_of_bytes_read) / 1048576.0) AS [Tot MB read over period],
        CONVERT(DEC(14,2), ((vio.num_of_bytes_read - old.num_of_bytes_read) / 1048576.0) * 1000.00 / @DurationInMilisecs) AS [Tot MB read/sec],
        (vio.num_of_bytes_read - old.num_of_bytes_read) / CASE (vio.num_of_reads-old.num_of_reads) WHEN 0 THEN 1 ELSE vio.num_of_reads-old.num_of_reads END AS [Avg read size (bytes)],
        */
        (vio.io_stall_write_ms - old.io_stall_write_ms) / CASE (vio.num_of_writes-old.num_of_writes) WHEN 0 THEN 1 ELSE vio.num_of_writes-old.num_of_writes END AS [Avg write speed (ms)],
        vio.num_of_writes - old.num_of_writes AS [No of writes over period],
        CONVERT(DEC(14,2), (vio.num_of_writes - old.num_of_writes) * 1000.00/ (@DurationInMilisecs)) AS [No of writes/sec],
        CONVERT(DEC(14,2), (vio.num_of_bytes_written - old.num_of_bytes_written)/1048576.0) AS [Tot MB written over period],
        CONVERT(DEC(14,2), ((vio.num_of_bytes_written - old.num_of_bytes_written)/1048576.0) * 1000.00/ @DurationInMilisecs) AS [Tot MB written/sec],
        (vio.num_of_bytes_written-old.num_of_bytes_written) / CASE (vio.num_of_writes-old.num_of_writes) WHEN 0 THEN 1 ELSE vio.num_of_writes-old.num_of_writes END AS [Avg write size (bytes)],
        mf.physical_name AS [Physical file name],
        size_on_disk_bytes/1048576 AS [File size on disk (MB)]
FROM sys.dm_io_virtual_file_stats (NULL, NULL) vio,
        sys.master_files mf,
        @IOStats old
WHERE DB_NAME(vio.database_id) = 'tempdb'
	AND mf.database_id = vio.database_id
	AND mf.file_id = vio.file_id
	AND old.database_id = vio.database_id
	AND old.file_id = vio.file_id
	AND mf.name='templog'