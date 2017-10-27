IF EXISTS (SELECT * FROM sys.objects WHERE [object_id] = OBJECT_ID(N'[ReadModel].[tr_AgentState]')
               AND [type] = 'TR')
BEGIN
      DROP TRIGGER [ReadModel].[tr_AgentState];
END;
GO

CREATE TRIGGER ReadModel.tr_AgentState ON [ReadModel].[AgentState]
FOR INSERT, UPDATE
AS
SET NOCOUNT ON;

DECLARE @PersonId AS uniqueidentifier

SELECT @PersonId = PersonId FROM inserted 

UPDATE 
[ReadModel].[AgentState] 
SET TextFilter = 
	ISNULL(FirstName, '')  + ' ' 
	+ ISNULL(LastName, '') + ' ' 
	+ ISNULL(EmploymentNumber, '') + ' ' 
	+ ISNULL(RuleName, '')  + ' ' 
	+ ISNULL(StateName, '')  + ' ' 
	+ ISNULL(Activity, '')  + ' ' 
	+ ISNULL(SiteName, '') + ' ' 
	+ ISNULL(TeamName, '') 
WHERE 
PersonId = @PersonId