DROP TABLE [Tenant].[ExternalApplicationAccess]
GO

CREATE TABLE [Tenant].[ExternalApplicationAccess] (
	[Id] int identity(1,1) NOT NULL,
	PersonId [uniqueidentifier] NOT NULL,
	Hash nvarchar(512) not null,
	Name nvarchar(256) not null,
	CreatedOn DateTime not null
)
alter table [Tenant].[ExternalApplicationAccess] add constraint PK_ExternalApplicationAccess primary key clustered
(
 Id asc
)
GO




