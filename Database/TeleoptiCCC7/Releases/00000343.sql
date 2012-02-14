
/*
   2011-11-15
   User: Ola
   Database: new table, ScheduleTag
*/

/****** Object:  Table [dbo].[ScheduleTag]    Script Date: 11/16/2011 12:03:35 ******/

CREATE TABLE [dbo].[ScheduleTag](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Description] [nvarchar](15) NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL,
	[IsDeleted] [bit] NOT NULL
)

ALTER TABLE [dbo].[ScheduleTag] ADD  CONSTRAINT [PK_ScheduleTag] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)

GO

ALTER TABLE [dbo].[ScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_ScheduleTag_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[ScheduleTag] CHECK CONSTRAINT [FK_ScheduleTag_BusinessUnit]
GO

ALTER TABLE [dbo].[ScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_ScheduleTag_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[ScheduleTag] CHECK CONSTRAINT [FK_ScheduleTag_Person_CreatedBy]
GO

ALTER TABLE [dbo].[ScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_ScheduleTag_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[ScheduleTag] CHECK CONSTRAINT [FK_ScheduleTag_Person_UpdatedBy]
GO

/*
   2011-11-21
   User: CS
   Database: new table, AgentDayScheduleTag
*/

/****** Object:  Table [dbo].[AgentDayScheduleTag]    Script Date: 11/21/2011 13:22:25 ******/


CREATE TABLE [dbo].[AgentDayScheduleTag](
	[Id] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[UpdatedBy] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Person] [uniqueidentifier] NOT NULL,
	[TagDate] [datetime] NOT NULL,
	[ScheduleTag] [uniqueidentifier] NOT NULL,
	[Scenario] [uniqueidentifier] NOT NULL,
	[BusinessUnit] [uniqueidentifier] NOT NULL
	)

ALTER TABLE [dbo].[AgentDayScheduleTag] ADD  CONSTRAINT [PK_AgentDayScheduleTag] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)

GO

ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_BusinessUnit] FOREIGN KEY([BusinessUnit])
REFERENCES [dbo].[BusinessUnit] ([Id])
GO

ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_BusinessUnit]
GO

ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_Person_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_Person_CreatedBy]
GO

ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_Person_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_Person_UpdatedBy]
GO

ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_Person] FOREIGN KEY([Person])
REFERENCES [dbo].[Person] ([Id])
GO

ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_Person]
GO

ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_Scenario] FOREIGN KEY([Scenario])
REFERENCES [dbo].[Scenario] ([Id])
GO

ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_Scenario]
GO

ALTER TABLE [dbo].[AgentDayScheduleTag]  WITH CHECK ADD  CONSTRAINT [FK_AgentDayScheduleTag_ScheduleTag] FOREIGN KEY([ScheduleTag])
REFERENCES [dbo].[ScheduleTag] ([Id])
GO

ALTER TABLE [dbo].[AgentDayScheduleTag] CHECK CONSTRAINT [FK_AgentDayScheduleTag_ScheduleTag]
GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (343,'7.1.343') 
