/* 
Trunk initiated: 
2010-06-23 
09:01
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 

----------------  
--Name: RobinK
--Date: 2010-07-05  
--Desc: Added columns for calculation of statistics from queue (could be merged when creating release script instead of doing abandoned separatly)
----------------
ALTER TABLE [dbo].[Workload] ADD [OfferedTasks] [float] NULL
ALTER TABLE [dbo].[Workload] ADD [OverflowIn] [float] NULL
ALTER TABLE [dbo].[Workload] ADD [OverflowOut] [float] NULL
ALTER TABLE [dbo].[Workload] ADD [AbandonedShort] [float] NULL
ALTER TABLE [dbo].[Workload] ADD [AbandonedWithinServiceLevel] [float] NULL
ALTER TABLE [dbo].[Workload] ADD [AbandonedAfterServiceLevel] [float] NULL
GO

UPDATE [dbo].[Workload] SET OfferedTasks = 1,OverflowIn = 1,OverflowOut = -1,AbandonedShort = 0,AbandonedWithinServiceLevel = 1,AbandonedAfterServiceLevel = 1
GO

ALTER TABLE [dbo].[Workload] ALTER COLUMN [OfferedTasks] [float] NOT NULL
ALTER TABLE [dbo].[Workload] ALTER COLUMN [OverflowIn] [float] NOT NULL
ALTER TABLE [dbo].[Workload] ALTER COLUMN [OverflowOut] [float] NOT NULL
ALTER TABLE [dbo].[Workload] ALTER COLUMN [AbandonedShort] [float] NOT NULL
ALTER TABLE [dbo].[Workload] ALTER COLUMN [AbandonedWithinServiceLevel] [float] NOT NULL
ALTER TABLE [dbo].[Workload] ALTER COLUMN [AbandonedAfterServiceLevel] [float] NOT NULL
GO

ALTER TABLE [dbo].[Workload] ADD [Abandoned] [float] NULL
GO

UPDATE [dbo].[Workload] SET Abandoned = -1
GO

ALTER TABLE [dbo].[Workload] ALTER COLUMN [Abandoned] [float] NOT NULL
GO

ALTER TABLE  [dbo].[SkillDataPeriod] ADD [Efficiency] [float] NULL
GO
UPDATE [dbo].[SkillDataPeriod] SET [Efficiency] = 1
GO
ALTER TABLE  [dbo].[SkillDataPeriod] ALTER COLUMN [Efficiency] [float] NOT NULL
GO

ALTER TABLE  [dbo].[TemplateSkillDataPeriod] ADD [Efficiency] [float] NULL
GO
UPDATE [dbo].[TemplateSkillDataPeriod] SET [Efficiency] = 1
GO
ALTER TABLE  [dbo].[TemplateSkillDataPeriod] ALTER COLUMN [Efficiency] [float] NOT NULL
GO

----------------  
--Name: RobinK
--Date: 2010-07-06
--Desc: Added column to keep reference to a scorecard from a team
----------------
ALTER TABLE [dbo].[Team] ADD [Scorecard] [uniqueidentifier] NULL
GO

ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK_Team_Scorecard] FOREIGN KEY([Scorecard])
REFERENCES [dbo].[Scorecard] ([Id])
GO

ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK_Team_Scorecard]
GO

 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (289,'7.1.289') 
