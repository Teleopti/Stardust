
-- Remove ALL team, it was no longer being added in 2009 but some customers have it
delete from [mart].[dim_team]
where team_id = -2

-- Set Not Defined to have empty guid to allow unique constraint later
update [mart].[dim_team]
set team_code = '00000000-0000-0000-0000-000000000000'
where team_id = -1

-- Find duplicate rows to be removed
SELECT dt.team_code, team_id AS old_id, new_id 
INTO #duplicates
FROM [mart].[dim_team] dt
JOIN (SELECT team_code, MIN(team_id) AS new_id
  FROM [mart].[dim_team]
  GROUP BY team_code HAVING COUNT(team_code) > 1) err ON dt.team_code = err.team_code
WHERE team_id <> new_id

-- Change FK links in bridge_acd_login_person
UPDATE balp 
SET balp.team_id = dup.new_id
FROM [mart].[bridge_acd_login_person] balp
JOIN #duplicates dup ON balp.team_id = dup.old_id

-- Change FK links in dim_person
UPDATE dp
SET dp.team_id = dup.new_id
FROM [mart].[dim_person] dp
JOIN #duplicates dup ON dp.team_id = dup.old_id

-- Change FK links in fact_kpi_targets_team
DELETE FROM [mart].[fact_kpi_targets_team]
WHERE team_id IN (SELECT old_id FROM #duplicates)

-- Change FK links in permission_report
DELETE FROM [mart].[permission_report]
WHERE team_id IN (SELECT old_id FROM #duplicates)

-- Remove the duplicate rows that are no longer connected to anything
DELETE FROM [mart].[dim_team]
WHERE team_id IN (SELECT old_id FROM #duplicates)

DROP TABLE #duplicates

IF NOT EXISTS(SELECT * 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='AK_team_code')
BEGIN
	ALTER TABLE [mart].[dim_team] ADD CONSTRAINT AK_team_code UNIQUE (team_code)
END
GO


