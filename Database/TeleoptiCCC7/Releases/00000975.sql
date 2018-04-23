-- Fix bug #75599 String too long when run ETL task - schedule
ALTER TABLE dbo.PersonGroupBase ALTER COLUMN Name NVARCHAR(255) NOT NULL
