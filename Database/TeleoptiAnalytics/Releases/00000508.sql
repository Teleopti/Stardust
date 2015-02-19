
UPDATE RTA.ActualAgentState 
SET [ScheduledId]=NULL
WHERE [ScheduledId] = '00000000-0000-0000-0000-000000000000'

UPDATE RTA.ActualAgentState 
SET [ScheduledNextId]=NULL
WHERE [ScheduledNextId] = '00000000-0000-0000-0000-000000000000'

UPDATE RTA.ActualAgentState 
SET [AlarmId]=NULL
WHERE [AlarmId] = '00000000-0000-0000-0000-000000000000'

UPDATE RTA.ActualAgentState 
SET [StateId]=NULL
WHERE [StateId] = '00000000-0000-0000-0000-000000000000'

UPDATE RTA.ActualAgentState 
SET [StateId]=NULL
WHERE [StateId] = '00000000-0000-0000-0000-000000000000'

UPDATE RTA.ActualAgentState 
SET [StateStart]=NULL
WHERE [StateStart] = '1900-01-01 00:00:000'

UPDATE RTA.ActualAgentState 
SET [NextStart]=NULL
WHERE [NextStart] = '1900-01-01 00:00:000'

UPDATE RTA.ActualAgentState 
SET [Color]=NULL
WHERE [Color] = 0

UPDATE RTA.ActualAgentState 
SET [AlarmStart]=NULL
WHERE [AlarmStart] = '1900-01-01 00:00:000'

UPDATE RTA.ActualAgentState 
SET [BusinessUnitId]=NULL
WHERE [BusinessUnitId] = '00000000-0000-0000-0000-000000000000'

UPDATE RTA.ActualAgentState 
SET [TeamId]=NULL
WHERE [TeamId] = '00000000-0000-0000-0000-000000000000'

UPDATE RTA.ActualAgentState 
SET [SiteId]=NULL
WHERE [SiteId] = '00000000-0000-0000-0000-000000000000'
