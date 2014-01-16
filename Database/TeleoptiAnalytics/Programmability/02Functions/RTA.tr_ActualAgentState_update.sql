IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[RTA].[tr_ActualAgentState_update]'))
DROP TRIGGER [RTA].[tr_ActualAgentState_update]
GO

CREATE TRIGGER RTA.tr_ActualAgentState_update
ON RTA.ActualAgentState
AFTER UPDATE 
AS

/*
To enable timeInState report please execute:
ENABLE TRIGGER RTA.tr_ActualAgentState_update ON RTA.ActualAgentState;
*/

--IF (COLUMNS_UPDATED() & 8) > 0
BEGIN
	INSERT INTO [RTA].[ActualAgentState_History](StateStart, person_code, time_in_state_s, state_group_code, days_cross_midnight)
	SELECT
		StateStart		= d.StateStart,
		person_code		= d.PersonId,
		time_in_state_s = datediff(ss,d.StateStart,i.StateStart),
		state_group_code= d.StateId,
		days_cross_midnight = datediff(dd, 0, i.StateStart) - datediff(dd, 0, d.StateStart)
	FROM DELETED d
	INNER JOIN INSERTED i
		ON d.StateStart<>i.StateStart  --Only if time have changed
END
GO
DISABLE TRIGGER RTA.tr_ActualAgentState_update ON RTA.ActualAgentState;
