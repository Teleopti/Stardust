IF EXISTS (SELECT * FROM sys.objects WHERE [object_id] = OBJECT_ID(N'ReadModelAgentState')
               AND [type] = 'TR')
BEGIN
      DROP TRIGGER ReadModelAgentState;
END;
GO

ALTER TABLE ReadModel.AgentState DROP COLUMN TextFilter
GO

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