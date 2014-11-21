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