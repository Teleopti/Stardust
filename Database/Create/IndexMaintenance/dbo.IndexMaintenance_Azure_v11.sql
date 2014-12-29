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


SET @DatabaseName = db_name()
SET @CurrentCommand = ''
SET @Error = 0
SET @ReturnCode = 0


DECLARE @avg_fragmentation_in_percent smallint
SET @avg_fragmentation_in_percent = 10

--Get tables with fragmentation on table or its index worse than 10%
DECLARE TableCursor CURSOR FOR
(
	SELECT
		sc.name AS SchemaName,
		OBJECT_NAME(ps.object_id) AS TableName
	 FROM sys.dm_db_partition_stats ps
	 INNER JOIN sys.objects so
		ON so.object_id = ps.object_id
	 INNER JOIN sys.schemas sc
		on so.schema_id = sc.schema_id
	 INNER JOIN sys.indexes i
	 ON ps.object_id = i.object_id
	 AND ps.index_id = i.index_id
	 CROSS APPLY sys.dm_db_index_physical_stats(DB_ID(), ps.object_id, ps.index_id, null, 'LIMITED') ips
	 GROUP BY
		sc.name,
		OBJECT_NAME(ps.object_id)
	 HAVING max(ips.avg_fragmentation_in_percent) > @avg_fragmentation_in_percent

)

--Rebuild
OPEN TableCursor
FETCH NEXT FROM TableCursor INTO @SchemaName,@TableName
WHILE @@FETCH_STATUS = 0
 
BEGIN
	SET @CurrentCommand = 'ALTER INDEX ALL ON [' + @SchemaName + '].[' + @TableName + '] REBUILD'
	SET @CurrentComment = 'ALTER INDEX ALL ON [' + @SchemaName + '].[' + @TableName + '] REBUILD'

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

	FETCH NEXT FROM TableCursor INTO @SchemaName,@TableName
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