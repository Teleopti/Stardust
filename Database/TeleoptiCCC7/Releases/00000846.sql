
update ReadModel.AgentState set IsDeleted = 0 where IsDeleted is null

DROP INDEX [IX_IsDeleted] ON [ReadModel].[AgentState]
GO

alter table ReadModel.AgentState alter column IsDeleted bit not null
go

CREATE NONCLUSTERED INDEX [IX_IsDeleted] ON [ReadModel].[AgentState]
(
	[IsDeleted] ASC
)
GO