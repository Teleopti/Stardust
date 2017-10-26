
insert into ApplicationFunctionInRole(
			[ApplicationRole]
           ,[ApplicationFunction]
           ,[InsertedOn])
	select 
		afr.ApplicationRole,
		af.Id,
		getdate()
	from ApplicationFunctionInRole  afr
	inner join ApplicationFunction af
	on af.Id = afr.ApplicationFunction
	where af.ForeignId = '0059'
