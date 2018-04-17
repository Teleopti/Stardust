
EXEC sp_rename 'ReadModel.AgentState.IsDeleted', 'HasAssociation', 'COLUMN';  
GO

UPDATE [ReadModel].[AgentState]
SET HasAssociation = (CASE HasAssociation WHEN 1 THEN 0 ELSE 1 END)
GO





ALTER TABLE [ReadModel].[AgentState]
ADD [HasName] [bit] NULL;
GO

UPDATE [ReadModel].[AgentState]
SET [HasName] = 0  WHERE [HasName] IS NULL
GO

ALTER TABLE [ReadModel].[AgentState]
ALTER COLUMN [HasName] [bit] NOT NULL
GO

MERGE INTO ReadModel.AgentState AS T
USING
(
	SELECT 
		Id, 
		FirstName, 
		LastName, 
		EmploymentNumber
	FROM 
		dbo.Person
) AS S (
	PersonId,
	FirstName,
	LastName,
	EmploymentNumber
) ON
	T.PersonId = S.PersonId
WHEN NOT MATCHED THEN
INSERT
(
	PersonId,
	FirstName,
	LastName,
	EmploymentNumber,
	HasAssociation,
	HasName
)
VALUES (
	S.PersonId,
	S.FirstName,
	S.LastName,
	S.EmploymentNumber,
	0,
	1
)
WHEN MATCHED THEN
UPDATE SET
	FirstName = S.FirstName,
	LastName = S.LastName,
	EmploymentNumber = S.EmploymentNumber,
	HasName = 1
;
GO

DELETE PersonAssociationCheckSum
GO

UPDATE ReadModel.KeyValueStore SET Value = 'True' WHERE [Key] = 'PersonAssociationChangedPublishTrigger'
GO

