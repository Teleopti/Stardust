
DROP INDEX IDX_RuleMappings ON ReadModel.RuleMappings
GO

ALTER TABLE ReadModel.RuleMappings ALTER COLUMN BusinessUnitId [uniqueidentifier] NOT NULL
GO
ALTER TABLE ReadModel.RuleMappings ALTER COLUMN StateCode [nvarchar](300) NOT NULL
GO
ALTER TABLE ReadModel.RuleMappings ALTER COLUMN ActivityId [uniqueidentifier] NOT NULL
GO

ALTER TABLE ReadModel.RuleMappings 
ADD CONSTRAINT [PK_RuleMappings] PRIMARY KEY CLUSTERED 
(
	[BusinessUnitId],
	[StateCode],
	[ActivityId]
)
GO
