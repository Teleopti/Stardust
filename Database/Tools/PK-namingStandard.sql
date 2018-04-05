SET NOCOUNT ON

IF OBJECT_ID('tempdb..#badPkName') IS NULL 
BEGIN
  
            CREATE TABLE #badPkName(
                         [tablename] [sysname] NOT NULL,
                         [PKname] [sysname] NOT NULL,
                         [schemaName] [sysname] NOT NULL
)
END
DELETE FROM #badPkName

IF OBJECT_ID('tempdb..#resulttablos') IS NOT NULL 
BEGIN
            DROP TABLE #resulttablos
END

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

DECLARE @CRLF nvarchar(MAX)
SELECT @CRLF = char(10)
DECLARE @tablename sysname
DECLARE @pkname sysname
DECLARE @schema sysname

            select
                         @CRLF + 'Table: [' + b.schemaName + '].[' + b.tablename +'] has a badly named PK-key: [' + b.PKname + ']' + @CRLF + 'please consider:' + @CRLF + 'EXEC sp_rename N''[' + b.schemaName + '].[' + b.tablename +'].[' + b.PKname + ']'', N''PK_' + b.tablename + ''', N''INDEX''' as 'resultline'
            into #resulttablos
            from #badPkName as b

            SELECT * FROM #resulttablos
