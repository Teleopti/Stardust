create schema [Tenant] authorization [dbo]
go

CREATE TABLE [Tenant].[Security](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DateTimeUtc] [datetime] NOT NULL CONSTRAINT [DF_Security_Time]  DEFAULT (getutcdate()),
	[Result] [nvarchar](100) NOT NULL,
	[UserCredentials] [nchar](100) NOT NULL,
	[Provider] [nvarchar](100) NOT NULL,
	[Client] [nvarchar](1000) NULL,
	[ClientIp] [nvarchar](100) NOT NULL,
	[PersonId] [uniqueidentifier] NULL)

alter table [Tenant].[Security]
add constraint PK_Security
primary key clustered
(
  [Id] asc
)

go
