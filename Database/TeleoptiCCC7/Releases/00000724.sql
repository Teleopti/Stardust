DROP TABLE dbo.QueuedAbsenceRequest
GO
CREATE TABLE dbo.QueuedAbsenceRequest
	(
	Id uniqueidentifier NOT NULL,
	PersonRequest uniqueidentifier NOT NULL,
	Created datetime NOT NULL,
	StartDateTime datetime NOT NULL,
	EndDateTime datetime NOT NULL,
	BusinessUnit uniqueidentifier NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL
	)
GO
ALTER TABLE dbo.QueuedAbsenceRequest ADD CONSTRAINT
	PK_QueuedAbsenceRequest PRIMARY KEY CLUSTERED 
	(
	Id
	)

GO