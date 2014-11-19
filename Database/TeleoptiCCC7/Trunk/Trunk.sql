----------------  
--Name: Xianwei Shen
--Date: 2014-11-19
--Desc: Add new table for adherence details
---------------- 
IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[ReadModel].[AdherenceDetails]') AND type in (N'U'))
BEGIN
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
END
GO