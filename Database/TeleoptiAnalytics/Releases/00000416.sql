----------------  
--Name: Roger Kratz
--Desc: Adding a log table for installation
---------------- 
CREATE TABLE [dbo].[DatabaseVersion_InstallLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CodeVersion] [varchar](255) NOT NULL,
	[DatabaseVersion] [int] NOT NULL,
	[InstalledBy] [varchar](255) NOT NULL CONSTRAINT [DF_DatabaseVersion_InstallLog_InstalledBy] DEFAULT (suser_sname()),
	[InstalledDate] [datetime] NOT NULL CONSTRAINT [DF_DatabaseVersion_InstallLog_InstalledDate] DEFAULT (getdate())
)
alter table dbo.DatabaseVersion_InstallLog add constraint
	PK_DatabaseVersion_InstallLog PRIMARY KEY CLUSTERED(
	Id
)
ALTER TABLE [dbo].[DatabaseVersion_InstallLog]  WITH CHECK ADD  CONSTRAINT [FK_DatabaseVersion_InstallLog_DatabaseVersion] FOREIGN KEY([DatabaseVersion])
REFERENCES [dbo].[DatabaseVersion] ([BuildNumber])
ALTER TABLE [dbo].[DatabaseVersion_InstallLog] CHECK CONSTRAINT [FK_DatabaseVersion_InstallLog_DatabaseVersion]
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (416,'8.1.416') 
