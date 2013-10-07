IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DBA_report_data_IO_By_Interval]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DBA_report_data_IO_By_Interval]
GO

CREATE PROC dbo.DBA_report_data_IO_By_Interval
@DateFrom	DATETIME = '1900-01-01',
@DateTo	DATETIME = '2059-12-31'
AS

CREATE TABLE #tempIntervals(
	[StartOfInterval] datetime NULL,
	[EndOfInterval] datetime NULL
)

--All intervals IO stats
INSERT INTO #tempIntervals
select 
	dateadd(ms,-interval_ms,RecordedDateTime) as StartOfInterval,
	RecordedDateTime as EndOfInterval
from dbo.DBA_VirtualFileStatsHistory
where FirstMeasureFromStart=0
and RecordedDateTime between @DateFrom AND @DateTo
group by RecordedDateTime,interval_ms

--Get @DurationInSecs for the select period
DECLARE @minInterval datetime
DECLARE @maxInterval datetime
DECLARE @DurationInSecs int

SELECT TOP 1 @minInterval=StartOfInterval FROM #tempIntervals WHERE @DateFrom < EndOfInterval ORDER BY StartOfInterval ASC
SELECT @maxInterval =
CASE 
	WHEN max(EndOfInterval) > @DateTo THEN @DateTo
	ELSE max(EndOfInterval)
END
FROM #tempIntervals 
SELECT @DurationInSecs=datediff(ss,@minInterval,@maxInterval)

--Sum full period
SELECT
	@minInterval as StartOfInterval,
	@maxInterval as EndOfInterval,
	DB_NAME(vio.database_id) AS [Database],
        mf.name AS [Logical name],
        mf.type_desc AS [Type],
		CONVERT(DEC(14,2), (sum(vio.num_of_reads) + sum(vio.num_of_writes))/(sum(interval_ms)/1000.00)) AS [Total IOPS],
        CONVERT(DEC(14,2), sum(vio.io_stall_read_ms)/CASE sum(vio.num_of_reads) WHEN 0 THEN 1 ELSE sum(vio.num_of_reads) END) AS [Ave read speed (ms)],
        CONVERT(DEC(14,2), sum(vio.io_stall_write_ms) / CASE sum(vio.num_of_writes) WHEN 0 THEN 1 ELSE sum(vio.num_of_writes) END) AS [Ave write speed (ms)],
        CONVERT(DEC(14,2), (sum(vio.num_of_bytes_read) / 1048576.0) / (sum(interval_ms)/1000.00)) AS [Tot MB read/sec],
        CONVERT(DEC(14,2), (sum(vio.num_of_bytes_written)/1048576.0) / (sum(interval_ms)/1000.00)) AS [Tot MB written/sec],
        sum(num_of_reads) AS [No of reads over period],
        sum(vio.num_of_writes) AS [No of writes over period],
        CONVERT(DEC(14,2), (sum(vio.num_of_reads)) / (sum(interval_ms)/1000.00)) AS [Read IOPS],
        CONVERT(DEC(14,2), (sum(vio.num_of_writes)) / (sum(interval_ms)/1000.00)) AS [Write IOPS],
        --CONVERT(DEC(14,2), sum(vio.num_of_bytes_read) / CASE sum(vio.num_of_reads) WHEN 0 THEN 1 ELSE sum(vio.num_of_reads) END) AS [Ave read size (bytes)],
        --CONVERT(DEC(14,2), sum(vio.num_of_bytes_written) / CASE sum(vio.num_of_writes) WHEN 0 THEN 1 ELSE sum(vio.num_of_writes) END) AS [Ave write size (bytes)]
        --CONVERT(DEC(14,2), (sum(vio.num_of_bytes_read)) / 1048576.0) AS [Tot MB read over period],
        --CONVERT(DEC(14,2), (sum(vio.num_of_bytes_written))/1048576.0) AS [Tot MB written over period],
        mf.physical_name AS [Physical file name]
FROM DBA_VirtualFileStatsHistory vio,
        sys.master_files mf
WHERE mf.database_id = vio.database_id AND
        mf.file_id = vio.file_id AND
        ((vio.num_of_bytes_read) + (vio.num_of_bytes_written)) > 0
and FirstMeasureFromStart=0
and RecordedDateTime between @minInterval AND @maxInterval
GROUP BY DB_NAME(vio.database_id),mf.name,mf.type_desc,mf.physical_name
ORDER BY sum((vio.num_of_bytes_read) + (vio.num_of_bytes_written)) DESC

--All files, All intervals
SELECT
		dateadd(ms,-interval_ms,RecordedDateTime) as StartOfInterval,
		RecordedDateTime as EndOfInterval,
		DB_NAME(vio.database_id) AS [Database],
        mf.name AS [Logical name],
        mf.type_desc AS [Type],
		CONVERT(DEC(14,2), (sum(vio.num_of_reads) + sum(vio.num_of_writes))/(sum(interval_ms)/1000.00)) AS [Total IOPS],
        CONVERT(DEC(14,2), sum(vio.io_stall_read_ms)/CASE sum(vio.num_of_reads) WHEN 0 THEN 1 ELSE sum(vio.num_of_reads) END) AS [Ave read speed (ms)],
        CONVERT(DEC(14,2), sum(vio.io_stall_write_ms) / CASE sum(vio.num_of_writes) WHEN 0 THEN 1 ELSE sum(vio.num_of_writes) END) AS [Ave write speed (ms)],
        CONVERT(DEC(14,2), (sum(vio.num_of_bytes_read) / 1048576.0) / (sum(interval_ms)/1000.00)) AS [Tot MB read/sec],
        CONVERT(DEC(14,2), (sum(vio.num_of_bytes_written)/1048576.0) / (sum(interval_ms)/1000.00)) AS [Tot MB written/sec],
        sum(num_of_reads) AS [No of reads over period],
        sum(vio.num_of_writes) AS [No of writes over period],
        CONVERT(DEC(14,2), (sum(vio.num_of_reads)) / (sum(interval_ms)/1000.00)) AS [Read IOPS],
        CONVERT(DEC(14,2), (sum(vio.num_of_writes)) / (sum(interval_ms)/1000.00)) AS [Write IOPS],
        --CONVERT(DEC(14,2), sum(vio.num_of_bytes_read) / CASE sum(vio.num_of_reads) WHEN 0 THEN 1 ELSE sum(vio.num_of_reads) END) AS [Ave read size (bytes)],
        --CONVERT(DEC(14,2), sum(vio.num_of_bytes_written) / CASE sum(vio.num_of_writes) WHEN 0 THEN 1 ELSE sum(vio.num_of_writes) END) AS [Ave write size (bytes)]
        --CONVERT(DEC(14,2), (sum(vio.num_of_bytes_read)) / 1048576.0) AS [Tot MB read over period],
        --CONVERT(DEC(14,2), (sum(vio.num_of_bytes_written))/1048576.0) AS [Tot MB written over period],
        mf.physical_name AS [Physical file name]
FROM DBA_VirtualFileStatsHistory vio,
        sys.master_files mf
WHERE mf.database_id = vio.database_id AND
        mf.file_id = vio.file_id AND
        ((vio.num_of_bytes_read) + (vio.num_of_bytes_written)) > 0
and FirstMeasureFromStart=0
and RecordedDateTime between @minInterval AND @maxInterval
GROUP BY DB_NAME(vio.database_id),mf.name,mf.type_desc,mf.physical_name,RecordedDateTime,interval_ms
ORDER BY dateadd(ms,-interval_ms,RecordedDateTime) ASC,sum((vio.num_of_bytes_read) + (vio.num_of_bytes_written)) DESC

--worst interval
SELECT TOP 1
		dateadd(ms,-interval_ms,RecordedDateTime) as StartOfInterval,
		RecordedDateTime as EndOfInterval,
		CONVERT(DEC(14,2), (sum(vio.num_of_reads) + sum(vio.num_of_writes))/(sum(interval_ms)/1000.00)) AS [Total IOPS],
        CONVERT(DEC(14,2), sum(vio.io_stall_read_ms)/CASE sum(vio.num_of_reads) WHEN 0 THEN 1 ELSE sum(vio.num_of_reads) END) AS [Ave read speed (ms)],
        CONVERT(DEC(14,2), sum(vio.io_stall_write_ms) / CASE sum(vio.num_of_writes) WHEN 0 THEN 1 ELSE sum(vio.num_of_writes) END) AS [Ave write speed (ms)],
        CONVERT(DEC(14,2), (sum(vio.num_of_bytes_read) / 1048576.0) / (sum(interval_ms)/1000.00)) AS [Tot MB read/sec],
        CONVERT(DEC(14,2), (sum(vio.num_of_bytes_written)/1048576.0) / (sum(interval_ms)/1000.00)) AS [Tot MB written/sec],
        sum(num_of_reads) AS [No of reads over period],
        sum(vio.num_of_writes) AS [No of writes over period],
        CONVERT(DEC(14,2), (sum(vio.num_of_reads)) / (sum(interval_ms)/1000.00)) AS [Read IOPS],
        CONVERT(DEC(14,2), (sum(vio.num_of_writes)) / (sum(interval_ms)/1000.00)) AS [Write IOPS]
        --CONVERT(DEC(14,2), sum(vio.num_of_bytes_read) / CASE sum(vio.num_of_reads) WHEN 0 THEN 1 ELSE sum(vio.num_of_reads) END) AS [Ave read size (bytes)],
        --CONVERT(DEC(14,2), sum(vio.num_of_bytes_written) / CASE sum(vio.num_of_writes) WHEN 0 THEN 1 ELSE sum(vio.num_of_writes) END) AS [Ave write size (bytes)]
        --CONVERT(DEC(14,2), (sum(vio.num_of_bytes_read)) / 1048576.0) AS [Tot MB read over period],
        --CONVERT(DEC(14,2), (sum(vio.num_of_bytes_written))/1048576.0) AS [Tot MB written over period],
FROM DBA_VirtualFileStatsHistory vio,
        sys.master_files mf
WHERE mf.database_id = vio.database_id AND
        mf.file_id = vio.file_id AND
        ((vio.num_of_bytes_read) + (vio.num_of_bytes_written)) > 0
and FirstMeasureFromStart=0
and RecordedDateTime between @minInterval AND @maxInterval
GROUP BY RecordedDateTime,interval_ms
ORDER BY sum((vio.num_of_reads) + (vio.num_of_writes)) DESC

go