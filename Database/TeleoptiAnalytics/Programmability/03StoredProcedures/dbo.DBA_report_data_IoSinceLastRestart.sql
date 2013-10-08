IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DBA_report_data_IoSinceLastRestart]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DBA_report_data_IoSinceLastRestart]
GO

create proc dbo.DBA_report_data_IoSinceLastRestart
as
SET NOCOUNT ON

DECLARE @sqlserver_start_time datetime
SELECT
	@sqlserver_start_time=sqlserver_start_time
FROM sys.dm_os_sys_info


SELECT
	@sqlserver_start_time as sqlserver_start_time,
	interval_ms as interval_ms,
	SUBSTRING(mf.physical_name,0,4) AS DriveLetter,
	DB_NAME(fs.database_id) AS [Database Name],
	mf.physical_name,
	io_stall_read_ms,
	num_of_reads,
	CAST(io_stall_read_ms/(1.0 + num_of_reads) AS NUMERIC(10,1)) AS [avg_read_stall_ms],
	io_stall_write_ms,
	num_of_writes,
	CAST(io_stall_write_ms/(1.0+num_of_writes) AS NUMERIC(10,1)) AS [avg_write_stall_ms],
	io_stall_read_ms + io_stall_write_ms AS [io_stalls],
	num_of_reads + num_of_writes AS [total_io],
	CAST((io_stall_read_ms + io_stall_write_ms)/(1.0 + num_of_reads + num_of_writes) AS NUMERIC(10,1)) AS [avg_io_stall_ms],
	CAST(1000*num_of_reads/interval_ms AS NUMERIC(10,1)) AS avg_read_iop,
	CAST(1000*num_of_writes/interval_ms AS NUMERIC(10,1)) AS avg_write_iop,
	CAST(1000*(num_of_reads+num_of_writes)/interval_ms AS NUMERIC(10,1)) AS avg_iop
FROM [dbo].[DBA_VirtualFileStatsCurrent] AS fs
INNER JOIN sys.master_files AS mf
ON fs.database_id = mf.database_id
AND fs.[file_id] = mf.[file_id]
ORDER BY avg_io_stall_ms DESC OPTION (RECOMPILE);
go
