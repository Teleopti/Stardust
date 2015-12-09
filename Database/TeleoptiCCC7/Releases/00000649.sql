DELETE m FROM dbo.RtaMap m INNER JOIN dbo.BusinessUnit b ON m.BusinessUnit = b.Id WHERE b.IsDeleted = 1
DELETE m FROM dbo.RtaMap m INNER JOIN dbo.Activity a ON m.Activity = a.Id WHERE a.IsDeleted = 1
DELETE m FROM dbo.RtaMap m INNER JOIN dbo.Person p ON m.UpdatedBy = p.Id WHERE p.IsDeleted = 1
DELETE m FROM dbo.RtaMap m INNER JOIN dbo.RtaRule r ON m.RtaRule = r.Id WHERE r.IsDeleted = 1
GO
ALTER TABLE dbo.RtaRule
	DROP COLUMN IsDeleted
GO