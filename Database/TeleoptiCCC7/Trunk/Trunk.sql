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

