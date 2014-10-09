--Name: Chundan
--Date: 2014-09-22  
--Desc: Add new column for agent badges threshold settings table: badgeType settings
----------------  
IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'AnsweredCallsBadgeTypeSelected' 
		AND [object_id] = OBJECT_ID(N'dbo.AgentBadgeThresholdSettings'))
BEGIN
    -- Column Exists
ALTER TABLE [dbo].[AgentBadgeThresholdSettings] DROP COLUMN [AnsweredCallsBadgeTypeSelected]
ALTER TABLE [dbo].[AgentBadgeThresholdSettings] DROP COLUMN [AHTBadgeTypeSelected]
ALTER TABLE [dbo].[AgentBadgeThresholdSettings] DROP COLUMN [AdherenceBadgeTypeSelected]
END

ALTER TABLE [dbo].[AgentBadgeThresholdSettings] ADD [AnsweredCallsBadgeTypeSelected] [bit] NULL
GO
UPDATE [dbo].[AgentBadgeThresholdSettings]
SET [AnsweredCallsBadgeTypeSelected] = 0
GO
ALTER TABLE [dbo].[AgentBadgeThresholdSettings] ALTER COLUMN [AnsweredCallsBadgeTypeSelected] [bit] NOT NULL
GO

ALTER TABLE [dbo].[AgentBadgeThresholdSettings] ADD [AHTBadgeTypeSelected] [bit] NULL
GO
UPDATE [dbo].[AgentBadgeThresholdSettings]
SET [AHTBadgeTypeSelected] = 0
GO
ALTER TABLE [dbo].[AgentBadgeThresholdSettings] ALTER COLUMN [AHTBadgeTypeSelected] [bit] NOT NULL
GO

ALTER TABLE [dbo].[AgentBadgeThresholdSettings] ADD [AdherenceBadgeTypeSelected] [bit] NULL
GO
UPDATE [dbo].[AgentBadgeThresholdSettings]
SET [AdherenceBadgeTypeSelected] = 0
GO
ALTER TABLE [dbo].[AgentBadgeThresholdSettings] ALTER COLUMN [AdherenceBadgeTypeSelected] [bit] NOT NULL
GO
