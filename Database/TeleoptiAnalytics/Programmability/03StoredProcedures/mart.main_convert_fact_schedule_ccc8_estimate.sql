IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[main_convert_fact_schedule_ccc8_estimate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[main_convert_fact_schedule_ccc8_estimate]
GO

CREATE PROCEDURE [mart].[main_convert_fact_schedule_ccc8_estimate] 
AS

--counting rows in fact_schedule_old and give estimate of time
declare @row_count bigint
declare @bytes_per_row int
declare @rows_per_minute int
declare @estimated_minutes int
declare @estimated_mb int
declare @estimated_gb int

set @rows_per_minute = 500000
set @bytes_per_row = 500.0

select @row_count = PS.row_count FROM sys.dm_db_partition_stats 
					AS PS INNER JOIN
					sys.objects AS OBJ
					 ON PS.object_id = OBJ.object_id
					INNER JOIN
					sys.indexes AS IDX
					ON PS.object_id = IDX.object_id
					AND PS.index_id = IDX.index_id
					WHERE IDX.is_primary_key = 1
					AND OBJ.name = 'fact_schedule_old'

select @estimated_minutes = @row_count/@rows_per_minute
select @estimated_mb = @row_count*@bytes_per_row/1024/1024
select @estimated_gb = @estimated_mb/1024

PRINT 'The number of rows to migrate: ' + cast(@row_count as nvarchar(10))
PRINT 'The estimated time in minutes to migrate fact_schedule: ' + cast(@estimated_minutes as nvarchar(10)) + ' minutes.'
PRINT '  Based on Teleopti I/O Pre-reqs: Avg io write stall < 5 ms, avg read io stall < 20 ms)'
PRINT 'The estimated space needed for the implicit sort in tempdb: ' + 
 case  
	 when @estimated_gb < 1 then cast(@estimated_mb as nvarchar(10)) + ' Mb.'
	 else cast(@estimated_gb as nvarchar(10)) + ' Gb.'
 end

