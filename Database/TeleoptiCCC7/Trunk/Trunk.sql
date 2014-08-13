--Name: Xinfeng, Chundan
--Date: 2014-08-11  
--Desc: Add new column for agent badges
----------------  
ALTER TABLE [dbo].[AgentBadge] ADD [LastCalculatedDate] [datetime] NULL
GO
UPDATE [dbo].[AgentBadge]
SET [LastCalculatedDate] = '2000-01-01'
GO
ALTER TABLE [dbo].[AgentBadge] ALTER COLUMN [LastCalculatedDate] [datetime] NOT NULL
GO

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'CalculationTime' AND [object_id] = OBJECT_ID(N'AgentBadgeThresholdSettings'))
BEGIN
    ALTER TABLE [dbo].[AgentBadgeThresholdSettings] DROP COLUMN [CalculationTime]
END
GO


----------------  
--Name: Mingdi
--Date: 2014-08-13  
--Desc: Add new column for filter dayoff
----------------  

ALTER TABLE [ReadModel].[PersonScheduleDay] ADD [IsDayOff] [bit] NULL

GO
