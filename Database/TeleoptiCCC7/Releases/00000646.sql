--#36226, already added to customer, so IF EXISTS
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Person_WorkflowControlSet' AND object_id = OBJECT_ID('person'))
BEGIN
	CREATE NONCLUSTERED INDEX IX_Person_WorkflowControlSet
	ON [dbo].[Person] ([WorkflowControlSet])
	INCLUDE ([Id],[FirstName],[LastName])
END
