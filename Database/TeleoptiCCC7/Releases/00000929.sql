--#48215
--get potential wrong schema
declare @table varchar(1000) = 'BusinessProcessOutsourcer'
declare @newschema varchar(1000) = 'dbo'
declare @sql varchar(8000)
declare @oldschema varchar(1000)
select  @oldschema = s.name
from sys.schemas s
inner join sys.objects so
	on so.schema_id = s.schema_id
where so.[name] = @table

--transfer to 'dbo' if it ended up on the wrong schema
if (@newschema <> @oldschema)
begin
	set @sql = 'alter schema ' + @newschema + ' transfer ' + @oldschema + '.' + @table
	exec(@sql)
end


ALTER TABLE dbo.BusinessProcessOutsourcer --add dbo
	ALTER COLUMN [Source] nvarchar(100) not null
