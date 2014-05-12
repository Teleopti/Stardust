--Name: Ola, Jonas, Kunning
--Date: 2014-04-29
--Desc: Add the authentication info table to prepare for single sign on
----------------  
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Removed')
EXEC sp_executesql N'CREATE SCHEMA [Removed] AUTHORIZATION [dbo]'
GO

CREATE TABLE [dbo].[AuthenticationInfo](
	[Person] [uniqueidentifier] NOT NULL,
	[Identity] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_AuthenticationInfo] PRIMARY KEY NONCLUSTERED 
(
	[Person] ASC
),
 CONSTRAINT [UQ_Identity] UNIQUE CLUSTERED 
(
	[Identity] ASC
)
)

ALTER TABLE [dbo].[AuthenticationInfo] WITH CHECK ADD CONSTRAINT [FK_AuthenticationInfo_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[AuthenticationInfo] CHECK CONSTRAINT [FK_AuthenticationInfo_Person]

INSERT INTO dbo.[AuthenticationInfo] 
SELECT Person, DomainName+'\'+WindowsLogOnName
FROM dbo.WindowsAuthenticationInfo

ALTER SCHEMA Removed 
TRANSFER [dbo].[WindowsAuthenticationInfo]

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (393,'7.5.393') 
