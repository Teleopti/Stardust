IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[RTA].[tr_ActualAgentState_update]'))
DROP TRIGGER [RTA].[tr_ActualAgentState_update]
GO

CREATE TRIGGER RTA.tr_ActualAgentState_update
ON RTA.ActualAgentState
AFTER UPDATE 
AS

/*
--====================
--How to check/fire the trigger only on specific column changes
--====================
In our case we always update all columns so their is no need for this (yet).
To check whether any one of columns 2, 3 or 4 has been updated:
IF (COLUMNS_UPDATED() & 14) > 0

To see if all of columns 2, 3, and 4 are updated.:
IF (COLUMNS_UPDATED() & 14) = 14

the Bitmask: power(2,(2-1))+power(2,(3-1))+power(2,(4-1)) = 14
or in t-sql:
--
select sum(power(2,(column_id-1)))
from sys.all_columns
where object_name(object_id)='ActualAgentState'
and name in ('State')  --(aka StateGroup)
--
*/

--IF (COLUMNS_UPDATED() & 8) > 0
BEGIN
	INSERT INTO [stage].[stg_agent_state]
	SELECT
		person_code		= d.PersonId,
		state_code		= d.StateCode,
		state_group_code= d.StateId,
		StateStart		= d.StateStart,
--		interval		= cast(dateadd(MINUTE,(DATEPART(HOUR,d.StateStart)*60+DATEPART(MINUTE,d.StateStart)),'1900-01-01') as smalldatetime),
		time_in_state_s = datediff(ss,d.StateStart,i.StateStart)
	FROM DELETED d
	INNER JOIN INSERTED i
		ON d.StateStart<>i.StateStart  --Only if time have changed
END
GO
