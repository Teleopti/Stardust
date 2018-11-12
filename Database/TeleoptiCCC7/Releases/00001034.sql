if not exists (select 1 from PurgeSetting where [key] = 'MonthsToKeepAudit')
	insert into PurgeSetting ([Key], [Value]) values ('MonthsToKeepAudit', 3)