

-- Set All and Not Defined to have empty and emptyish guids to allow unique constraint later
update [mart].[dim_site]
set site_code = '00000000-0000-0000-0000-000000000000'
where site_id = -2

update [mart].[dim_site]
set site_code = '00000000-0000-0000-0000-000000000001'
where site_id = -1


-- Find duplicate rows to be removed
SELECT ds.site_code, site_id AS old_id, new_id 
INTO #duplicates
FROM [mart].[dim_site] ds
JOIN (SELECT site_code, MIN(site_id) AS new_id
  FROM [mart].[dim_site]
  GROUP BY site_code HAVING COUNT(site_code) > 1) err ON ds.site_code = err.site_code
WHERE site_id <> new_id

-- Change FK links in dim_team
UPDATE dt 
SET dt.site_id = dup.new_id
FROM [mart].[dim_team] dt
JOIN #duplicates dup ON dt.site_id = dup.old_id

-- Change FK links in dim_person
UPDATE dp
SET dp.site_id = dup.new_id
FROM [mart].[dim_person] dp
JOIN #duplicates dup ON dp.site_id = dup.old_id

-- Remove the duplicate rows that are no longer connected to anything
DELETE FROM [mart].[dim_site]
WHERE site_id IN (SELECT old_id FROM #duplicates)

DROP TABLE #duplicates

IF NOT EXISTS(SELECT * 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='AK_site_code')
BEGIN
	ALTER TABLE [mart].[dim_site] ADD CONSTRAINT AK_site_code UNIQUE (site_code)
END
GO


