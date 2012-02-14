--PK
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

	--PRINT @DynamicSQL 
	EXEC sp_executesql @DynamicSQL 

FETCH NEXT FROM cur INTO @tablename, @pkname, @schema;
END
CLOSE cur;
DEALLOCATE cur;

--FK
DECLARE @old sysname, @new sysname

DECLARE rename_cursor CURSOR FOR 
WITH AllFKs AS (
  SELECT DISTINCT fkc.constraint_object_id, fkc.parent_object_id, fkc.referenced_object_id, MIN(fkc.parent_column_id) parent_column_id
    FROM sys.foreign_key_columns fkc
	GROUP BY fkc.constraint_object_id, fkc.parent_object_id, fkc.referenced_object_id
),
Relations AS (
  SELECT FK.name AS Relation, P.name AS Parent, ss.name as MySchema, R.name AS Referenced, ROW_NUMBER() OVER(PARTITION BY P.Name, R.Name ORDER BY P.Name, R.Name, AllFKs.parent_column_id) Prog
    FROM AllFKs INNER JOIN
         sys.objects AS FK ON FK.object_id = AllFKs.constraint_object_id INNER JOIN
         sys.objects AS P ON AllFKs.parent_object_id = P.object_id INNER JOIN
         sys.objects AS R ON AllFKs.referenced_object_id = R.object_id
	INNER JOIN sys.schemas ss
		ON P.schema_id = ss.schema_id

),
Transforms AS (
SELECT MySchema AS MySchema, Parent as Parent, Relation AS OldName, 'FK_'+Parent+'_'+Referenced+
       CASE
        WHEN EXISTS(SELECT * FROM Relations R2 WHERE Prog>1 AND R2.Parent=R1.Parent AND R2.Referenced=R1.Referenced ) THEN CAST(Prog AS NVARCHAR(2))
        ELSE ''
       END NewName
  FROM Relations R1
)
SELECT * FROM Transforms
WHERE NewName NOT IN (SELECT OldName FROM Transforms)

OPEN rename_cursor

FETCH NEXT FROM rename_cursor 
INTO @schema, @tablename, @old, @new

WHILE @@FETCH_STATUS = 0
BEGIN

	SELECT @DynamicSQL = 'sp_rename N''[' + @schema + '].['+ @old +']'', N'''+ @new +''', N''OBJECT'''
	--PRINT @DynamicSQL 
	EXEC sp_executesql @DynamicSQL 

FETCH NEXT FROM rename_cursor 
INTO @schema, @tablename, @old, @new
END
CLOSE rename_cursor
DEALLOCATE rename_cursor
