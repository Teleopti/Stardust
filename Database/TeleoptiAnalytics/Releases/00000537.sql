ALTER TABLE RTA.ActualAgentState ADD
	AlarmColor int NULL
GO
UPDATE RTA.ActualAgentState SET AlarmColor = Color
GO
