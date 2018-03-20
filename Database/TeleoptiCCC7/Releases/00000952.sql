/* AF 2018-03-20: All new installs will have personal data pseudonymized after 3 months to support GDPR laws (00000384.sql)*/
/* Existing installs will get 120 months from this script*/
if not exists (select 1 from PurgeSetting where [key] = 'MonthsToKeepPersonalData')
	insert into PurgeSetting ([Key], [Value]) values ('MonthsToKeepPersonalData', 120)
