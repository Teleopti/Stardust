
-- Find duplicate rows to be removed
SELECT ds.skillset_code, skillset_id AS old_id, new_id 
INTO #duplicates
FROM [mart].[dim_skillset] ds
JOIN (SELECT skillset_code, MIN(skillset_id) AS new_id
  FROM [mart].[dim_skillset]
  GROUP BY skillset_code HAVING COUNT(skillset_code) > 1) err ON ds.skillset_code = err.skillset_code
WHERE skillset_id <> new_id

-- Change FK links in bridge_skillset_skill
UPDATE bss 
SET bss.skillset_id = dup.new_id
FROM [mart].[bridge_skillset_skill] bss
JOIN #duplicates dup ON bss.skillset_id = dup.old_id

-- Change FK links in dim_person
UPDATE dp
SET dp.skillset_id = dup.new_id
FROM [mart].[dim_person] dp
JOIN #duplicates dup ON dp.skillset_id = dup.old_id

-- Remove the duplicate rows that are no longer connected to anything
DELETE FROM [mart].[dim_skillset]
WHERE skillset_id IN (SELECT old_id FROM #duplicates)

DROP TABLE #duplicates

IF NOT EXISTS(SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'dim_skillset' AND COLUMN_NAME = 'skillset_code_hash')
BEGIN
	ALTER TABLE [mart].[dim_skillset] 
	ADD [skillset_code_hash] AS CONVERT(NVARCHAR(32),HashBytes('MD5', skillset_code), 2);
END

IF NOT EXISTS(SELECT * 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='AK_skillset_code_hash')
BEGIN
	ALTER TABLE [mart].[dim_skillset] ADD CONSTRAINT AK_skillset_code_hash UNIQUE (skillset_code_hash)
	--ALTER TABLE [mart].[dim_skillset] DROP CONSTRAINT AK_skillset_code
END
GO


