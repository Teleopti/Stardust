USE [TeleoptiAnalytics-root]
GO


--missing table description
SELECT
	t.name as 'Table Name',	
	'EXEC sys.sp_addextendedproperty @name=N''Teleopti.CRA.Description'',@level0type=N''SCHEMA'',@level0name=N''mart'',@level1type=N''TABLE'', @level1name=N'''+ t.name +''',@value=N''Add you description here'''
from sys.tables t		
inner join sys.schemas s		
	on t.schema_id = s.schema_id	
left outer join fn_listextendedproperty (NULL, 'schema', 'mart', 'table', default, NULL, NULL) ep
	ON	t.name = ep.objname collate database_default
where s.name = 'mart'		
and (t.name like 'bridge%' or t.name like 'dim%' or t.name like 'fact%')		
and (ep.value is null or ep.value = '')
order by t.name




--missing column description
SELECT		
	t.name as 'Table Name',	
	c.name as 'Field Name',
	'EXEC sp_addextendedproperty @name = N''Teleopti.CRA.Description'', @level0type = N''Schema'', @level0name =mart,@level1type = N''Table'',@level2type = N''Column'',@level1name ='+ t.name +',@level2name ='+ c.name +',@value ='''''
		/*,	
	isnull(convert(nvarchar(500),ep.value), 'No info') 'Field Description',	
	dt.name as 'Datatype',	
	case dt.name	
		when 'nvarchar' then cast(c.max_length as varchar(10))
		when 'varchar' then cast(c.max_length as varchar(10))
		else ''
	end as 'Max Length',	
	case c.is_nullable	
		when 1 then 'True'
		else 'False'
	end as 'Is Nullable'*/
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
and (convert(nvarchar(500),ep.value) is null or convert(nvarchar(500),ep.value)='')
--and convert(nvarchar(500),ep.value) is null
order by t.name,c.column_id

--

	