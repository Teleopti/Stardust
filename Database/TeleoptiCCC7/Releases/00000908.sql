
ALTER TABLE ReadModel.AgentState ADD
	TextFilter AS ISNULL(FirstName, '')  + ' ' 
	+ ISNULL(LastName, '') + ' ' 
	+ ISNULL(EmploymentNumber, '') + ' ' 
	+ ISNULL(RuleName, '')  + ' ' 
	+ ISNULL(StateName, '')  + ' ' 
	+ ISNULL(Activity, '')  + ' ' 
	+ ISNULL(SiteName, '') + ' ' 
	+ ISNULL(TeamName, '') PERSISTED 
GO

