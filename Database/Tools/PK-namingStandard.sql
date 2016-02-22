SET NOCOUNT ON

CREATE TABLE #badPkName(
	[tablename] [sysname] NOT NULL,
	[PKname] [sysname] NOT NULL,
	[schemaName] [sysname] NOT NULL
)

INSERT INTO #badPkName
	SELECT 
	s2.name AS tablename, 
	s1.name AS PKname,
	s3.name AS schemaName
	FROM sys.objects s1
	INNER JOIN sys.objects s2 ON s2.object_id = s1.parent_object_id
	inner join sys.schemas s3
	on s1.schema_id = s3.schema_id
	WHERE OBJECTPROPERTY(s1.object_id, N'IsPrimaryKey') = 1
	AND s1.name <> 'PK_'+s2.name
	AND s3.name <> N'HangFire'  --Exclude 'Hangfire'-schema
	AND s3.name <> N'SignalR'  --Exclude 'SignalR'-schema

DECLARE @tablename sysname
DECLARE @pkname sysname
DECLARE @schema sysname
	DECLARE @ErrorMessage varchar(4000)
	SET @ErrorMessage = '----------------'
	
	DECLARE cur CURSOR FOR
	SELECT
		tablename,
		PKname,
		schemaName		
	FROM #badPkName
	ORDER BY tablename;
	OPEN cur;
		FETCH NEXT FROM cur INTO @tablename, @pkname, @schema;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @ErrorMessage = @ErrorMessage + char(13)+ char(10) + 'Table: [' + @schema + '].[' +@tablename +'] has a badly named PK-key: [' + @pkname + ']'
			+ char(13)+ char(10) + 'please consider:'
			+ char(13)+ char(10) + 'EXEC sp_rename N''[' + @schema + '].[' +@tablename +'].[' + @pkname + ']'', N''PK_' + @tablename + ''', N''INDEX'''
			+ char(13)+ char(10) + '----------------'
			FETCH NEXT FROM cur INTO @tablename, @pkname, @schema;
		END
		CLOSE cur;
		DEALLOCATE cur;

	--Check if we have PKs with wrong name
	IF EXISTS (
		SELECT 1 FROM #badPkName
	)
	BEGIN
		-- Return an error with state 127 since it will abort SQLCMD
		RAISERROR (@ErrorMessage, 16, 127)
	END
