----------------  
--Name: Xianwei Shen
--Date: 2014-11-19
--Desc: Add new table for adherence details
---------------- 
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
)
GO
CREATE CLUSTERED INDEX [PK_AdherenceDetails] ON [ReadModel].[AdherenceDetails]
(
	[PersonId] ASC,
	[BelongsToDate] ASC
)
--Added temporary clustered key
--No unique key exists yet
GO


GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (413,'8.1.413') 
