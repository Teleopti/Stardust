DELETE dbo.RtaState 
FROM dbo.RtaState s
INNER JOIN dbo.RtaStateGroup g
ON g.Id = s.Parent
WHERE g.IsDeleted = 1

DELETE FROM dbo.RtaStateGroup
WHERE IsDeleted = 1

ALTER TABLE dbo.RtaStateGroup
DROP COLUMN IsDeleted
