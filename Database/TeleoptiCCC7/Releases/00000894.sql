
insert into ApplicationFunctionInRole(
			[ApplicationRole]
           ,[ApplicationFunction]
           ,[InsertedOn])
	select 
		ApplicationRole,
		'EFE028F5-F1C0-477C-9A98-7EF73CD09296',
		getdate()
	from ApplicationFunctionInRole 
	where ApplicationFunction = '2DD8E2CD-645D-475B-8DFE-1219FEC66CFA'
