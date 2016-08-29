ALTER TABLE [dbo].[AgentState] ADD [DataSourceId] int NULL
ALTER TABLE [dbo].[AgentState] ADD [UserCode] varchar(50) NULL
ALTER TABLE [dbo].[AgentState] DROP CONSTRAINT PK_AgentState
GO

INSERT INTO [ReadModel].[KeyValueStore] ([Key], [Value]) VALUES ('PersonAssociationChangedPublishTrigger', 'True')
GO
