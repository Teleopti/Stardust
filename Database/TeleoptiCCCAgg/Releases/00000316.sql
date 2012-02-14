/* 
Trunk initiated: 
2011-01-31 
11:14
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2011-01-03
--Desc: Rename all PKs  
----------------  
DECLARE @tablename sysname
DECLARE @pkname sysname
DECLARE @newpkname sysname
DECLARE @schema sysname
DECLARE @DynamicSQL nvarchar(4000)

DECLARE cur CURSOR FOR
SELECT 
s2.name AS tablename, 
s1.name AS PKname,
s3.name
FROM sys.objects s1
INNER JOIN sys.objects s2 ON s2.object_id = s1.parent_object_id
inner join sys.schemas s3
on s1.schema_id = s3.schema_id
WHERE OBJECTPROPERTY(s1.object_id, N'IsPrimaryKey') = 1
AND s1.name <> 'PK_'+s2.name
ORDER BY s1.name;
OPEN cur;
	FETCH NEXT FROM cur INTO @tablename, @pkname, @schema;
	WHILE @@FETCH_STATUS = 0
	BEGIN
	SET @newpkname = 'PK_' + @tablename
	SELECT @DynamicSQL = 'sp_rename N''[' + @schema + '].[' + @tablename + '].['+ @pkname +']'', N'''+ @newpkname +''', N''INDEX'''

	PRINT @pkname + ' > ' + @newpkname
	EXEC sp_executesql @DynamicSQL 

FETCH NEXT FROM cur INTO @tablename, @pkname, @schema;
END
CLOSE cur;
DEALLOCATE cur;
GO

----------------  
--Name: David Jonsson
--Date: 2011-01-03
--Desc: re-design agent_logg if needed!
--Desc: PK-design bug found on some customers + SalesDemo
----------------
DECLARE @SchemaName sysname
DECLARE @TableName sysname
DECLARE @isPK int
DECLARE @Key1 nvarchar (128)
DECLARE @Key2 nvarchar (128)
DECLARE @Key3 nvarchar (128)
DECLARE @Key4 nvarchar (128)

SET @SchemaName='dbo'
SET @TableName='agent_logg'
SET @isPK=1
SET @Key1='date_from'
SET @Key2='agent_id'
SET @Key3='queue'
SET @Key4='interval'

IF
(
	SELECT COUNT(*)
	FROM sys.indexes as si 
	LEFT JOIN sys.objects as so on so.object_id=si.object_id 
	WHERE schema_name(schema_id) = @SchemaName
	AND si.object_id = object_id(@TableName)
	AND si.type = @isPK
	AND INDEX_COL(schema_name(schema_id)+'.'+OBJECT_NAME(si.object_id),index_id,1) = @Key1
	AND INDEX_COL(schema_name(schema_id)+'.'+OBJECT_NAME(si.object_id),index_id,2) = @Key2
	AND INDEX_COL(schema_name(schema_id)+'.'+OBJECT_NAME(si.object_id),index_id,3) = @Key3
	AND INDEX_COL(schema_name(schema_id)+'.'+OBJECT_NAME(si.object_id),index_id,4) = @Key4
	AND INDEX_COL(schema_name(schema_id)+'.'+OBJECT_NAME(si.object_id),index_id,5) IS NULL
) <> 1
BEGIN
	--Drop wrong PK
	IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[agent_logg]') AND name = N'PK_agent_logg')
	ALTER TABLE [dbo].[agent_logg] DROP CONSTRAINT [PK_agent_logg]

	--Add correct PK
	ALTER TABLE [dbo].[agent_logg] ADD  CONSTRAINT [PK_agent_logg] PRIMARY KEY CLUSTERED 
	(
		[date_from] ASC,
		[agent_id] ASC,
		[queue] ASC,
		[interval] ASC
	) ON [PRIMARY]
END


 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (316,'7.1.316') 
