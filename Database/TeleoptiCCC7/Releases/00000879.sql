ALTER TABLE dbo.PersonGroupBase
	DROP COLUMN ShortName

ALTER TABLE dbo.PersonGroupBase
  ALTER COLUMN Name NVARCHAR(100) NOT NULL
