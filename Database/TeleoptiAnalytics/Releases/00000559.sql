-- Find duplicate rows to be removed
SELECT dgp.group_id, group_page_id AS old_id, new_id 
INTO #duplicate_group_page
FROM [mart].[dim_group_page] dgp
JOIN (SELECT group_id, MIN(group_page_id) AS new_id
  FROM [mart].[dim_group_page]
  GROUP BY group_id HAVING COUNT(group_id) > 1) err ON dgp.group_id = err.group_id
WHERE group_page_id <> new_id

--
DELETE FROM [mart].[bridge_group_page_person]
WHERE group_page_id IN (SELECT old_id FROM #duplicate_group_page)

DELETE FROM [mart].[dim_group_page]
WHERE group_page_id IN (SELECT old_id FROM #duplicate_group_page)



IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME ='AK_group_id')
BEGIN
	ALTER TABLE [mart].[dim_group_page] ADD CONSTRAINT AK_group_id UNIQUE (group_id)
END



