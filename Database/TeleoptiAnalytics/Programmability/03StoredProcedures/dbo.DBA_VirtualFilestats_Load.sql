IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DBA_VirtualFilestats_Load]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DBA_VirtualFilestats_Load]
GO

-- =============================================
-- Author:		DJ, TM
-- Create date: 2013-10-07
-- Update date: 
--				yyyy-mm-dd Some comment
-- Description:	fetches the file stat for database files in the instance
-- Example call: EXEC [dbo].[DBA_VirtualFilestats_Load] @Duration='00:00:10',@IntervalInSeconds=1
-- =============================================
CREATE PROC [dbo].[DBA_VirtualFilestats_Load]
@Duration          DATETIME = '23:59:59', --run for 24 hours
@IntervalInSeconds INT = 900 --Samle every 15 min
WITH EXECUTE AS OWNER
AS
SET NOCOUNT ON

--if Azure, bail out
IF (dbo.IsAzureDB() = 1)
BEGIN
	PRINT 'We are in Azure, we cannot run this procedure on this edition of SQL Server'
	RETURN 0
END

--if OWNER lack permissions, try CALLER
IF (SELECT has_perms_by_name(null, null, 'VIEW SERVER STATE')) = 0
BEGIN
	EXECUTE AS CALLER
	--if we still lack permissions, abort
	IF (SELECT has_perms_by_name(null, null, 'VIEW SERVER STATE')) = 0
	BEGIN
		PRINT 'This login lack permissions to view server state'
		RETURN 0
	END
END

--check current activity
IF EXISTS (SELECT 1 FROM dbo.DBA_VirtualFileStats)
BEGIN
	PRINT 'another batch is already running, please wait.'
	RETURN 0
END

DECLARE
@StopTime                 DATETIME,
@LastRecordedDateTime     DATETIME,
@CurrentDateTime          DATETIME,
@ErrorNumber              INT,
@NumberOfRows             INT,
@ErrorMessageText         NVARCHAR(4000),
@CurrentServerName        VARCHAR(255),
@DifferenceInMilliSeconds BIGINT

SELECT @CurrentServerName = Cast(Serverproperty('servername') AS VARCHAR(255))
SET @DifferenceInMilliSeconds = Datediff(ms, CONVERT(DATETIME, '00:00:00', 8), @Duration)
SELECT @StopTime = Dateadd(ms, @DifferenceInMilliSeconds, Getdate())

--Loop over time
WHILE Getdate() <= @StopTime
BEGIN
SELECT @LastRecordedDateTime = @CurrentDateTime
SELECT @CurrentDateTime = Getdate()

INSERT INTO dbo.DBA_VirtualFileStats
(
ServerName,
database_id,
[file_id],
DatabaseName,
PhysicalName,
num_of_reads,
num_of_reads_from_start,
num_of_writes,
num_of_writes_from_start,
num_of_bytes_read,
num_of_bytes_read_from_start,
num_of_bytes_written,
num_of_bytes_written_from_start,
io_stall,
io_stall_from_start,
io_stall_read_ms,
io_stall_read_ms_from_start,
io_stall_write_ms,
io_stall_write_ms_from_start,
RecordedDateTime,
interval_ms,
FirstMeasureFromStart
)
SELECT
@CurrentServerName,
mf.database_id,
mf.[file_id],
db_name(vfs.database_id),
mf.physical_name,
vfs.num_of_reads - dbaf.num_of_reads_from_start,
vfs.num_of_reads,
vfs.num_of_writes - dbaf.num_of_writes_from_start,
vfs.num_of_writes,
vfs.num_of_bytes_read - dbaf.num_of_bytes_read_from_start,
vfs.num_of_bytes_read,
vfs.num_of_bytes_written - dbaf.num_of_bytes_written_from_start,
vfs.num_of_bytes_written,
vfs.io_stall - dbaf.io_stall_from_start,
vfs.io_stall,
vfs.io_stall_read_ms - dbaf.io_stall_read_ms_from_start,
vfs.io_stall_read_ms,
vfs.io_stall_write_ms - dbaf.io_stall_write_ms_from_start,
vfs.io_stall_write_ms,
@CurrentDateTime,
CASE
	WHEN @LastRecordedDateTime IS NULL THEN NULL
	ELSE Datediff(ms, dbaf.RecordedDateTime, @CurrentDateTime)
END																	AS interval_ms,
CASE
	WHEN @LastRecordedDateTime IS NULL THEN 1
	ELSE 0
END																	AS FirstMeasureFromStart
FROM sys.Dm_io_virtual_file_stats(null,null) vfs
LEFT JOIN dbo.DBA_VirtualFileStats dbaf
	ON	db_name(vfs.database_id) = dbaf.DatabaseName
	AND vfs.[file_id] = dbaf.file_id
LEFT JOIN sys.master_files mf
	ON	mf.database_id		= vfs.database_id
	AND	mf.file_id			= vfs.file_id
WHERE (
	@LastRecordedDateTime IS NULL --1st round
	OR dbaf.RecordedDateTime = @LastRecordedDateTime --2nd to n round
	)

SELECT
@ErrorNumber = @@ERROR,
@NumberOfRows = @@ROWCOUNT
 
IF @ErrorNumber != 0
BEGIN
	SET @ErrorMessageText = 'Error ' + CONVERT(VARCHAR(10), @ErrorNumber) + ' failed to insert file stats data!'
	RAISERROR (@ErrorMessageText,16,1)
	RETURN @ErrorNumber
END

WAITFOR DELAY @IntervalInSeconds
        
-- setting the start and end datetime for the batch.
END

--save data to history
INSERT INTO dbo.DBA_VirtualFileStatsHistory
SELECT * FROM dbo.DBA_VirtualFileStats;
TRUNCATE TABLE dbo.DBA_VirtualFileStats;

GO