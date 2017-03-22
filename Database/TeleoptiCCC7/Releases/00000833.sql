BEGIN TRANSACTION
GO
CREATE TABLE dbo.JobResultArtifact
	(
	Id uniqueidentifier NOT NULL,
	Parent uniqueidentifier NOT NULL,
	Owner uniqueidentifier NOT NULL,
	Name nvarchar(MAX) NOT NULL,
	Category tinyint NOT NULL,
	[Content] varbinary(MAX) NOT NULL,
	CreateTime datetime NOT NULL
CONSTRAINT [PK_JobResultArtifact] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]
GO
ALTER TABLE dbo.JobResultArtifact ADD CONSTRAINT
	FK_JobResultArtifact_JobResult FOREIGN KEY
	(
	Parent
	) REFERENCES dbo.JobResult
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.JobResultArtifact ADD CONSTRAINT
	FK_JobResultArtifact_Person FOREIGN KEY
	(
	Owner
	) REFERENCES dbo.Person
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

COMMIT
