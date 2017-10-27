ALTER TABLE [ReadModel].[AgentState]
ADD TextFilter nvarchar(max);

GO

CREATE TRIGGER ReadModelAgentState ON [ReadModel].[AgentState]
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

GO
