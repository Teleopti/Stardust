CREATE CLUSTERED INDEX IX_AgentState_DataSourceIdUserCode
    ON [dbo].[AgentState] (DataSourceId, UserCode);   
GO

CREATE INDEX IX_AgentState_PersonId
    ON [dbo].[AgentState] (PersonId);   
GO
