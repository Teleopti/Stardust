DELETE FROM dbo.RtaStateGroup
WHERE IsDeleted = 1

ALTER TABLE dbo.RtaStateGroup
DROP COLUMN IsDeleted
