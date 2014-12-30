IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IndexMaintenance_Azure_v11]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[IndexMaintenance_Azure_v11]
GO

CREATE PROCEDURE [dbo].[IndexMaintenance_Azure_v11]
AS
DECLARE @TableName varchar(255)
DECLARE @SchemaName varchar(255)
DECLARE @DatabaseName  varchar(128)
DECLARE @CurrentCommandOutput int
DECLARE @CurrentCommand nvarchar(max)
DECLARE @CurrentComment nvarchar(max)
DECLARE @Error int
DECLARE @ReturnCode int
DECLARE @EndMessage nvarchar(max)
DECLARE @DatabaseMessage nvarchar(max)
DECLARE @ErrorMessage nvarchar(max)
DECLARE @CurrentExtendedInfo xml
DECLARE @db_id int
DECLARE @object_id int
DECLARE @indexName sysname

SET @DatabaseName = db_name()
SET @db_id = db_id()

SET @CurrentCommand = ''
SET @Error = 0
SET @ReturnCode = 0

DECLARE @avg_fragmentation_in_percent smallint
SET @avg_fragmentation_in_percent = 10

-- DROP TABLE #dm_db_index_physical_stats_tmp
CREATE TABLE #dm_db_index_physical_stats_tmp(
	[database_id] [smallint] NULL,
	[object_id] [int] NULL,
	[index_id] [int] NULL,
	[partition_number] [int] NULL,
	[index_type_desc] [nvarchar](60) NULL,
	[alloc_unit_type_desc] [nvarchar](60) NULL,
	[index_depth] [tinyint] NULL,
	[index_level] [tinyint] NULL,
	[avg_fragmentation_in_percent] [float] NULL,
	[fragment_count] [bigint] NULL,
	[avg_fragment_size_in_pages] [float] NULL,
	[page_count] [bigint] NULL,
	[avg_page_space_used_in_percent] [float] NULL,
	[record_count] [bigint] NULL,
	[ghost_record_count] [bigint] NULL,
	[version_ghost_record_count] [bigint] NULL,
	[min_record_size_in_bytes] [int] NULL,
	[max_record_size_in_bytes] [int] NULL,
	[avg_record_size_in_bytes] [float] NULL,
	[forwarded_record_count] [bigint] NULL,
	[compressed_page_count] [bigint] NULL
)

--get frag figures for all indexes we are interested in
INSERT INTO #dm_db_index_physical_stats_tmp
SELECT database_id, object_id, index_id, partition_number, index_type_desc, alloc_unit_type_desc, index_depth, index_level, avg_fragmentation_in_percent, fragment_count, avg_fragment_size_in_pages, page_count, avg_page_space_used_in_percent, record_count, ghost_record_count, version_ghost_record_count, min_record_size_in_bytes, max_record_size_in_bytes, avg_record_size_in_bytes, forwarded_record_count, compressed_page_count
FROM sys.dm_db_index_physical_stats(@db_id, null, null, null, 'LIMITED') ips
WHERE avg_fragmentation_in_percent > @avg_fragmentation_in_percent --threshold
AND alloc_unit_type_desc = 'IN_ROW_DATA'
AND index_level = 0 --leaf level
AND index_id <> 0 --no heaps
AND page_count > 100 --at least 100 x 8kb in size

--Get tables with fragmentation on table or its index worse than 10%
DECLARE TableCursor CURSOR FOR
(
	SELECT 
		sc.name AS SchemaName,
		OBJECT_NAME(ps.object_id) AS TableName,
		i.name
	 FROM sys.dm_db_partition_stats ps
	 INNER JOIN sys.objects so
		ON so.object_id = ps.object_id
	 INNER JOIN sys.schemas sc
		on so.schema_id = sc.schema_id
	 INNER JOIN sys.indexes i
		ON ps.object_id = i.object_id
		AND ps.index_id = i.index_id
	 INNER JOIN #dm_db_index_physical_stats_tmp ips
		ON ips.object_id = so.object_id
		AND ips.index_id = i.index_id
		AND ips.object_id = i.object_id
)

--Rebuild
OPEN TableCursor
FETCH NEXT FROM TableCursor INTO @SchemaName,@TableName,@indexName
WHILE @@FETCH_STATUS = 0
 
BEGIN
	SET @CurrentCommand = 'ALTER INDEX ' + @indexName +' ON [' + @SchemaName + '].[' + @TableName + '] REBUILD'
	SET @CurrentComment = 'ALTER INDEX ' + @indexName +' ON [' + @SchemaName + '].[' + @TableName + '] REBUILD'
	

    EXECUTE @CurrentCommandOutput = [dbo].[CommandExecute]
			@Command = @CurrentCommand,
			@CommandType = 'ALTER_INDEX',
			@Mode = 2,
			@Comment = @CurrentComment,
			@DatabaseName = @DatabaseName,
			@SchemaName = @SchemaName,
			@ObjectName = @TableName,
			@ObjectType = null,
			@IndexName = null,
			@IndexType = null,
			@PartitionNumber = null,
			@ExtendedInfo = null,
			@LogToTable = 'Y',
			@Execute = 'Y'

    SET @Error = @@ERROR
    IF @Error <> 0 SET @CurrentCommandOutput = @Error
    IF @CurrentCommandOutput <> 0 SET @ReturnCode = @CurrentCommandOutput + @ReturnCode

	FETCH NEXT FROM TableCursor INTO  @SchemaName,@TableName,@indexName
END
 
CLOSE TableCursor
DEALLOCATE TableCursor

  ----------------------------------------------------------------------------------------------------
  --// Log completing information                                                                 //--
  ----------------------------------------------------------------------------------------------------

  Logging:
  SET @EndMessage = 'Date and time: ' + CONVERT(nvarchar,GETDATE(),120)
  SET @EndMessage = REPLACE(@EndMessage,'%','%%')
  RAISERROR(@EndMessage,10,1) WITH NOWAIT

  IF @ReturnCode <> 0
  BEGIN
    RETURN @ReturnCode
  END
  ----------------------------------------------------------------------------------------------------
  GO