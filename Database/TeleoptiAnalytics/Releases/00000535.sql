ALTER TABLE RTA.ActualAgentState ADD
	IsInAlarm bit NOT NULL CONSTRAINT DF_ActualAgentState_IsInAlarm DEFAULT 1
GO