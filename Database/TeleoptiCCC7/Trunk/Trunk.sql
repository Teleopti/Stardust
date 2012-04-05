
/*
Make sure that all persons in the database has write protection info. This is created automatically in the domain.
*/

INSERT INTO [dbo].[PersonWriteProtectionInfo] (Id,CreatedBy,UpdatedBy,CreatedOn,UpdatedOn,PersonWriteProtectedDate) SELECT p.Id,p.CreatedBy,p.UpdatedBy,p.CreatedOn,p.UpdatedOn,null FROM [dbo].[Person] p WHERE p.Id NOT IN (SELECT id FROM [dbo].[PersonWriteProtectionInfo])
GO

/*
We have changed the lowest possible resolution to one hour to avoid issues with daylight savings.
*/

UPDATE dbo.Skill SET DefaultResolution = 60 WHERE DefaultResolution > 60
GO

-- =============================================
-- Author:		Ola
-- Create date: 2012-02-28
-- Description:	New LicenseStatus table
-- =============================================
CREATE TABLE [dbo].[LicenseStatus](
	[Id] [uniqueidentifier] NOT NULL,
	[XmlString] [nvarchar](4000) NOT NULL,
	CONSTRAINT PK_LicenseStatus PRIMARY KEY CLUSTERED (Id))
----------------  
--Name: David Jonsson
--Date: 2012-03-23
--Desc: #18738 - very slow to fetch request from MyTimeWeb
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ShiftTradeSwapDetail]') AND name = N'IX_ShiftTradeSwapDetail_Parent')
CREATE NONCLUSTERED INDEX IX_ShiftTradeSwapDetail_Parent
ON [dbo].[ShiftTradeSwapDetail] ([Parent])
INCLUDE ([PersonFrom],[PersonTo])
GO

----------------  
--Name: AndersF
--Date: 2012-03-30
--Desc: #18789 - Performance: The person day off query is now one of the most resource intense things on sql
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonDayOff]') AND name = N'IX_PersonDayOff_Scenario_BU_Anchor')
CREATE NONCLUSTERED INDEX [IX_PersonDayOff_Scenario_BU_Anchor]
ON [dbo].[PersonDayOff] ([Scenario],[BusinessUnit],[Anchor])
INCLUDE ([Id],[Version],[CreatedBy],[UpdatedBy],[CreatedOn],[UpdatedOn],[Person],[TargetLength],[Flexibility],[Name],[ShortName],[DisplayColor],[PayrollCode])
GO
--Name: Ola H
--Date: 2012-03-29
--Desc: Moving [ApplicationAuthenticationInfo] AND [WindowsAuthenticationInfo]
-- from Person to own tables
---------------- 
CREATE TABLE [dbo].[ApplicationAuthenticationInfo](
	[Person] [uniqueidentifier] NOT NULL,
	[ApplicationLogOnName] [nvarchar](50) NOT NULL,
	[Password] [nvarchar](50) NULL,
CONSTRAINT [PK_ApplicationAuthenticationInfo] PRIMARY KEY CLUSTERED 
(
	[Person] ASC
)  ,
CONSTRAINT [UQ_ApplicationLogOnName] UNIQUE NONCLUSTERED 
(
	[ApplicationLogOnName] ASC
))
GO

ALTER TABLE [dbo].[ApplicationAuthenticationInfo]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationAuthenticationInfo_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO
ALTER TABLE [dbo].[ApplicationAuthenticationInfo] CHECK CONSTRAINT [FK_ApplicationAuthenticationInfo_Person]
GO

CREATE TABLE [dbo].[WindowsAuthenticationInfo](
	[Person] [uniqueidentifier] NOT NULL,
	[WindowsLogOnName] [nvarchar](50) NOT NULL,
	[DomainName] [nvarchar](50) NOT NULL,
CONSTRAINT [PK_WindowsAuthenticationInfo] PRIMARY KEY CLUSTERED 
(
	[Person] ASC
),
CONSTRAINT [UQ_LogonNameDomain]UNIQUE NONCLUSTERED 
(
	[WindowsLogOnName] ASC,
	[DomainName] ASC
))

GO

ALTER TABLE [dbo].[WindowsAuthenticationInfo]  WITH CHECK ADD  CONSTRAINT [FK_WindowsAuthenticationInfo_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[WindowsAuthenticationInfo] CHECK CONSTRAINT [FK_WindowsAuthenticationInfo_Person]
GO

INSERT INTO [ApplicationAuthenticationInfo]
SELECT Id, [ApplicationLogOnName],[Password] From Person
WHERE [ApplicationLogOnName] IS NOT NULL
AND [ApplicationLogOnName] <> ''

INSERT INTO [WindowsAuthenticationInfo]
SELECT Id, [WindowsLogOnName],[DomainName] From Person
WHERE [WindowsLogOnName] IS NOT NULL
AND [DomainName] IS NOT NULL 
AND [WindowsLogOnName] <> ''
AND [DomainName] <> ''

GO
ALTER TABLE dbo.Person
	DROP CONSTRAINT UQ_Person_PartOfUnique
GO
ALTER TABLE dbo.Person
	DROP COLUMN PartOfUnique, WindowsLogOnName, DomainName, ApplicationLogOnName, [Password]

GO

-- =============================================
-- Author:		Talha
-- Create date: 2012-3-05
-- Description:	save forecasts file into database
-- =============================================
CREATE TABLE [dbo].[ForecastFile](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[FileName] [nvarchar](255) NULL,
	[FileContent] [varbinary](MAX) NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_ForecastFile]PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[ForecastFile]  WITH CHECK ADD  CONSTRAINT [FK_ForecastFile_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
ALTER TABLE [dbo].[ForecastFile] CHECK CONSTRAINT [FK_ForecastFile_BusinessUnit]
ALTER TABLE [dbo].[ForecastFile]  WITH CHECK ADD  CONSTRAINT [FK_ForecastFile_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[ForecastFile] CHECK CONSTRAINT [FK_ForecastFile_Person_CreatedBy]
ALTER TABLE [dbo].[ForecastFile]  WITH CHECK ADD  CONSTRAINT [FK_ForecastFile_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[ForecastFile] CHECK CONSTRAINT [FK_ForecastFile_Person_UpdatedBy]
GO
