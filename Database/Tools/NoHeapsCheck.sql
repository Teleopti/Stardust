SET NOCOUNT ON

DECLARE @tablename sysname
DECLARE @schemaname sysname
DECLARE @ErrorMessage varchar(4000)
SET @ErrorMessage = ''
	
	DECLARE cur CURSOR FOR
	SELECT
		s.name as schemaName,
		object_name(o.object_id) as tableName
	FROM sys.indexes i 
	INNER JOIN sys.objects o ON i.object_id = o.object_id
	INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
	INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
	LEFT OUTER JOIN sys.dm_db_index_usage_stats ius ON i.object_id = ius.object_id AND i.index_id = ius.index_id
	WHERE i.type_desc = 'HEAP'
	AND object_name(o.object_id) <> 'sysfiles1'
	AND OBJECT_NAME(s.schema_id) <> 'sys'
	ORDER BY s.name,object_name(o.object_id);
	
	OPEN cur;
		FETCH NEXT FROM cur INTO @schemaname, @tablename;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @ErrorMessage = @ErrorMessage + char(13)+ char(10) + 'Table: [' + @schemaname + '].[' +@tablename +'] does not have a clustered index defined. This is needed for Azure compability'

			FETCH NEXT FROM cur INTO @schemaname, @tablename;
		END
		CLOSE cur;
		DEALLOCATE cur;

	--Check if we have PKs with wrong name
	IF EXISTS (
		SELECT 1
		FROM sys.indexes i 
		INNER JOIN sys.objects o ON i.object_id = o.object_id
		INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
		INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
		LEFT OUTER JOIN sys.dm_db_index_usage_stats ius ON i.object_id = ius.object_id AND i.index_id = ius.index_id
		WHERE i.type_desc = 'HEAP'
		AND object_name(o.object_id) <> 'sysfiles1'
	)
	BEGIN
		-- Return an error with state 127 since it will abort SQLCMD
		RAISERROR (@ErrorMessage, 16, 127)
	END
