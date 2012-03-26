SET NOCOUNT ON
set quoted_identifier on
set arithabort off
set numeric_roundabort off
set ansi_warnings on
set ansi_padding on
set ansi_nulls on
set concat_null_yields_null on

CREATE TABLE #statements(SchemaName nvarchar(100), TableName nvarchar(200), Level int, IsLeafLevel int, SQLStatement nvarchar(2000));
--create table #TablesToFlush (SQLStatement nvarchar(4000));
--insert into #TablesToFlush 
--drop table  #TablesToFlush 
with fk_tables as (
     select    s1.schema_id as from_schema_id
     ,        o1.object_id as from_table_id
     ,        s2.schema_id as to_schema_id
     ,        o2.object_id as to_table_id
     from    sys.foreign_keys fk    
     inner    join sys.objects o1    
     on        fk.parent_object_id = o1.object_id    
     inner    join sys.schemas s1    
     on        o1.schema_id = s1.schema_id    
     inner    join sys.objects o2    
     on        fk.referenced_object_id = o2.object_id    
     inner    join sys.schemas s2    
     on        o2.schema_id = s2.schema_id    
     /*For the purposes of finding dependency hierarchy       
         we're not worried about self-referencing tables*/
     where    not    (    s1.schema_id = s2.schema_id
             and        o1.object_id = o2.object_id)
 )
 ,ordered_tables AS 
(        SELECT    s.schema_id as [schema_id]
         ,        t.object_id as table_id
         ,        0 AS Level    
         FROM    (    select    *                
                     from    sys.tables                 
                     where    name <> 'sysdiagrams') t    
         INNER    JOIN sys.schemas s    
         on        t.schema_id = s.schema_id    
         LEFT    OUTER JOIN fk_tables fk    
         ON        s.schema_id = fk.from_schema_id    
         AND        t.object_id = fk.from_table_id
         WHERE    fk.from_schema_id IS NULL
         UNION    ALL
         SELECT    fk.from_schema_id
         ,        fk.from_table_id
         ,        ot.Level + 1    
         FROM    fk_tables fk    
         INNER    JOIN ordered_tables ot    
         ON        fk.to_schema_id = ot.[schema_id]
         AND        fk.to_table_id = ot.table_id
 )
insert into #statements
select distinct
schema_name(ot.[schema_id]) as SchemaName,
object_name(ot.table_id) as TableName,
ot.Level,
CASE WHEN fk2.referenced_object_id IS NULL THEN 1 ELSE 0 END as [IsLeafLevel],
CASE WHEN fk2.referenced_object_id IS NULL THEN 'TRUNCATE TABLE ['+schema_name(ot.[schema_id])+'].['+object_name(ot.table_id)+']' ELSE 'DELETE ['+schema_name(ot.[schema_id])+'].['+object_name(ot.table_id)+']' END as [SQLstatement]

 from    ordered_tables ot
 inner    join (
         select    [schema_id]
         ,        table_id
         ,        MAX(Level) maxLevel        
         from    ordered_tables        
         group    by [schema_id],table_id
         ) mx
 on        ot.[schema_id]= mx.[schema_id]
 and        ot.table_id = mx.table_id
 and        mx.maxLevel = ot.Level
inner    join sys.schemas s1    
on        ot.[schema_id] = s1.schema_id 
left outer join sys.foreign_keys fk2
ON fk2.referenced_object_id = ot.table_id
order by schema_name(ot.[schema_id]),ot.[Level] desc,object_name(ot.table_id);

DECLARE @SQLStatement nvarchar(500)

DECLARE TableCursor CURSOR FOR  
select SQLStatement FROM #statements
order by SchemaName, Level Desc, TableName

OPEN TableCursor    
FETCH NEXT FROM TableCursor  INTO @SQLStatement  

WHILE @@FETCH_STATUS = 0   
BEGIN   
       EXEC sp_executesql @SQLStatement
       FETCH NEXT FROM TableCursor INTO @SQLStatement   
END   

CLOSE TableCursor   
DEALLOCATE TableCursor


DROP TABLE #statements
