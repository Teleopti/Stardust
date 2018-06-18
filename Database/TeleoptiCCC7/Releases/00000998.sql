
DROP INDEX CIX_PersonId ON ReadModel.ExternalLogon
GO

ALTER TABLE ReadModel.ExternalLogon 
ADD [Id] [int] IDENTITY(1,1) NOT NULL 
CONSTRAINT [PK_ExternalLogon] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
GO
