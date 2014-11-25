----------------  
--Name: Xianwei Shen
--Date: 2014-11-20
--Desc: Add new table for adherence details
---------------- 
DROP TABLE [ReadModel].[AdherenceDetails];
GO
CREATE TABLE [ReadModel].[AdherenceDetails](
	[PersonId] [uniqueidentifier] NOT NULL,
	[BelongsToDate] [smalldatetime] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[StartTime] [datetime] NULL,
	[ActualStartTime] [datetime] NULL,
	[LastStateChangedTime] [datetime] NULL,
	[IsInAdherence] [bit] NULL,
	[TimeInAdherence] [bigint] NULL,
	[TimeOutOfAdherence] [bigint] NULL,
	[ActivityHasEnded] [bit] NULL
)
GO
CREATE UNIQUE CLUSTERED INDEX [UCI_AdherenceDetails] ON [ReadModel].[AdherenceDetails]
(
	[PersonId] ASC,
	[BelongsToDate] ASC,
	[StartTime] ASC
)
GO

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

----------------  
--Name: David Jonsson
--Desc: Fixing build error
---------------- 
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonDayOff_old]') AND name = N'PK_PersonDayOff_old')
	EXEC dbo.sp_rename @objname = N'[dbo].[PersonDayOff_old].[PK_PersonDayOff]', @newname = N'PK_PersonDayOff_old', @objtype = N'INDEX'
GO
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Auditing].[PersonDayOff_AUD_old]') AND name = N'PK_PersonDayOff_AUD')
	EXEC dbo.sp_rename @objname = N'[Auditing].[PersonDayOff_AUD_old].[PK_PersonDayOff_AUD]',@newname =  N'PK_PersonDayOff_AUD_old', @objtype = N'INDEX'
GO