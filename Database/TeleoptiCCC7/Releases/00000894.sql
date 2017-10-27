
insert into ApplicationFunctionInRole(
			[ApplicationRole]
           ,[ApplicationFunction]
           ,[InsertedOn])
	select 
		afr.ApplicationRole,
		(select Id from ApplicationFunction where ForeignId = '0148'),
		getdate()
	from ApplicationFunctionInRole  afr
	inner join ApplicationFunction old
		on old.Id = afr.ApplicationFunction
	where old.ForeignId = '0059'
