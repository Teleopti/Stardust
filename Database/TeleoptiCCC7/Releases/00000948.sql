CREATE TABLE [Tenant].[ExternalApplicationAccess] (
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
	PersonId [uniqueidentifier] NOT NULL,
	Hash nvarchar(512) not null,
	Name nvarchar(256) not null,
	CreatedOn DateTime not null
)
GO




