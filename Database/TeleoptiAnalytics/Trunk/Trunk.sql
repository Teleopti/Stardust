
IF NOT EXISTS (SELECT * FROM sys.columns WHERE (Name = N'BatchId' OR Name = N'OriginalDataSourceId') AND OBJECT_ID = OBJECT_ID(N'[RTA].[ActualAgentState]'))