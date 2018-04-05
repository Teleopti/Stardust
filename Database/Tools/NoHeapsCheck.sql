SET NOCOUNT ON

DECLARE @tablename sysname
DECLARE @schemaname sysname
DECLARE @ErrorMessage varchar(4000)
SET @ErrorMessage = ''

IF OBJECT_ID('tempdb..#bajsmacka') IS NOT NULL 
BEGIN
            DROP TABLE #bajsmacka
END
            
SELECT 'Table: [' + s.name + '].[' +object_name(o.object_id) +'] does not have a clustered index defined. This is needed for Azure compability' AS 'resultline'
INTO #bajsmacka
FROM sys.indexes i 
            INNER JOIN sys.objects o ON i.object_id = o.object_id
            INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
            INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
            LEFT OUTER JOIN sys.dm_db_index_usage_stats ius ON i.object_id = ius.object_id AND i.index_id = ius.index_id
            WHERE i.type_desc = 'HEAP'
            AND object_name(o.object_id) <> 'sysfiles1'
            AND OBJECT_NAME(s.schema_id) <> 'sys'
            ORDER BY s.name,object_name(o.object_id);

SELECT * FROM #bajsmacka
