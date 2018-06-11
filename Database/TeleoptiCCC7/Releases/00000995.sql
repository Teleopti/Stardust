ALTER TABLE dbo.AgentState ADD
	LateForWork bit NOT NULL CONSTRAINT DF_AgentState_LateForWork DEFAULT 0
GO
