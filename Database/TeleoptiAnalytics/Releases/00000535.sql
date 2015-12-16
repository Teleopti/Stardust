ALTER TABLE RTA.ActualAgentState ADD
	IsRuleAlarm bit NOT NULL CONSTRAINT DF_ActualAgentState_IsRuleAlarm DEFAULT 1
GO