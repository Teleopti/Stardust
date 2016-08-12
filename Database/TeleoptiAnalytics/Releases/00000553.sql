
-- Find duplicate rows to be removed
SELECT dsc.shift_category_code, shift_category_id AS old_id, new_id 
INTO #duplicates
FROM [mart].[dim_shift_category] dsc
JOIN (SELECT shift_category_code, MIN(shift_category_id) AS new_id
  FROM [mart].[dim_shift_category]
  GROUP BY shift_category_code HAVING COUNT(shift_category_code) > 1) err ON dsc.shift_category_code = err.shift_category_code
WHERE shift_category_id <> new_id

-- Change FK links in fact_schedule
UPDATE fs 
SET fs.shift_category_id = dup.new_id
FROM [mart].[fact_schedule] fs
JOIN #duplicates dup ON fs.shift_category_id = dup.old_id

-- Change FK links in fact_schedule_convert
UPDATE fsc
SET fsc.shift_category_id = dup.new_id
FROM [mart].[fact_schedule_convert] fsc
JOIN #duplicates dup ON fsc.shift_category_id = dup.old_id

-- Change FK links in fact_schedule_preference
UPDATE fsp 
SET fsp.shift_category_id = dup.new_id
FROM [mart].[fact_schedule_preference] fsp
JOIN #duplicates dup ON fsp.shift_category_id = dup.old_id

-- Change FK links in fact_schedule_day_count
UPDATE fsdc 
SET fsdc.shift_category_id = dup.new_id
FROM [mart].[fact_schedule_day_count] fsdc
JOIN #duplicates dup ON fsdc.shift_category_id = dup.old_id

-- Remove the duplicate rows that are no longer connected to anything
DELETE FROM [mart].[dim_shift_category]
WHERE shift_category_id IN (SELECT old_id FROM #duplicates)

DROP TABLE #duplicates

-- Update Not Defined Row
UPDATE [mart].[dim_shift_category]
SET shift_category_code = '00000000-0000-0000-0000-000000000000'
WHERE shift_category_id = -1

-- Update any possible null rows
UPDATE [mart].[dim_shift_category]
SET shift_category_code = NEWID()
WHERE shift_category_code = NULL

DROP INDEX [mart].[dim_shift_category].[IX_shift_category_code]

ALTER TABLE [mart].[dim_shift_category] ALTER COLUMN [shift_category_code] UNIQUEIDENTIFIER NOT NULL
ALTER TABLE [mart].[dim_shift_category] ADD CONSTRAINT AK_shift_category_code UNIQUE (shift_category_code)


CREATE NONCLUSTERED INDEX [IX_shift_category_code] ON [mart].[dim_shift_category]
(
    [shift_category_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90)


GO