SELECT		
	t.name as 'Table Name',	
	c.name as 'Field Name',	
	isnull(convert(nvarchar(500),ep.value), 'No info') 'Field Description',	
	dt.name as 'Datatype',	
	case dt.name	
		when 'nvarchar' then cast(c.max_length as varchar(10))
		when 'varchar' then cast(c.max_length as varchar(10))
		else 'n/a'
	end as 'Max Length',	
	case c.is_nullable	
		when 1 then 'True'
		else 'False'
	end as 'Is Nullable',	
	dt.user_type_id	
from sys.tables t		
inner join sys.columns c		
	on c.object_id=t.object_id	
inner join sys.schemas s		
	on t.schema_id = s.schema_id	
inner join sys.types dt		
	on dt.user_type_id = c.user_type_id	
left outer join sys.extended_properties ep		
	ON	t.object_id = ep.major_id
	and c.column_id = ep.minor_id	
where s.name = 'mart'		
and (t.name like 'bridge%' or t.name like 'dim%' or t.name like 'fact%')		
order by t.name,c.column_id