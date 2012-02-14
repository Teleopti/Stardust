CREATE PROC sp_deleteAllData
AS
Set NoCount ON

Declare @tableName varchar(200)
Declare @tableOwner varchar(100)

set @tableName = ''
set @tableOwner = ''

/*
Step 1: Disable all constraints
*/

exec sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL' 
exec sp_MSforeachtable 'ALTER TABLE ? DISABLE TRIGGER ALL' 

/*
Step 2: Delete the data for all child tables & those which has no relations
*/

While exists
( 
select T.table_name from INFORMATION_SCHEMA.TABLES T
left outer join INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
on T.table_name=TC.table_name where (TC.constraint_Type ='Foreign Key'
or TC.constraint_Type is NULL) and 
T.table_name not in ('dtproperties','sysconstraints','syssegments')
and Table_type='BASE TABLE' and T.table_name > @TableName
)


Begin
           Select top 1 @tableOwner=T.table_schema,@tableName=T.table_name from INFORMATION_SCHEMA.TABLES T
           left outer join INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
           on T.table_name=TC.table_name where (TC.constraint_Type ='Foreign Key'
           or TC.constraint_Type is NULL) and 
           T.table_name not in ('dtproperties','sysconstraints','syssegments')
           and Table_type='BASE TABLE' and T.table_name > @TableName
           order by t.table_name


           --Delete the table
           Exec('DELETE FROM '+ @tableOwner + '.' + @tableName)

           checkpoint
End

/*
Step 3: Delete the data for all Parent tables
*/

set @TableName=''
set @tableOwner=''

While exists
( 
select T.table_name from INFORMATION_SCHEMA.TABLES T
left outer join INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
on T.table_name=TC.table_name where TC.constraint_Type ='Primary Key'
and T.table_name <>'dtproperties'and Table_type='BASE TABLE' 
and T.table_name > @TableName 
)

Begin
           Select top 1 @tableOwner=T.table_schema,@tableName=T.table_name from INFORMATION_SCHEMA.TABLES T
           left outer join INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
           on T.table_name=TC.table_name where TC.constraint_Type ='Primary Key'
           and T.table_name <>'dtproperties'and Table_type='BASE TABLE' 
           and T.table_name > @TableName
           order by t.table_name

           --Delete the table
           Exec('DELETE FROM '+ @tableOwner + '.' + @tableName)

           checkpoint

End

/*
Step 4: Enable all constraints
*/

exec sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL' 
exec sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL' 

Set NoCount Off
