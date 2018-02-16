/* AF 2018-02-16: All new installs will have personal data pseudonymized after 3 months to support GDPR laws */
/* This will set it to 10 years for existing installs */
if not exists (select 1 from PurgeSetting where [key] = 'MonthsToKeepPersonalData')
	insert into PurgeSetting ([Key], [Value]) values ('MonthsToKeepPersonalData', 120)
