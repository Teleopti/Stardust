CREATE TABLE dbo.QueuedAbsenceRequest
	(
	PersonRequest uniqueidentifier NOT NULL,
	Created datetime NOT NULL,
	StartDateTime datetime NOT NULL,
	EndDateTime datetime NOT NULL,
	BusinessUnit uniqueidentifier NOT NULL
	)
GO
ALTER TABLE dbo.QueuedAbsenceRequest ADD CONSTRAINT
	PK_QueuedAbsenceRequest PRIMARY KEY CLUSTERED 
	(
	PersonRequest
	)

GO