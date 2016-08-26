  --remove old "Notes" group page where group_code was emptyguid '00000000-0000-0000-0000-000000000000'
 SELECT group_page_id into #old_notes
 FROM mart.dim_group_page
 WHERE group_code='00000000-0000-0000-0000-000000000000'

 DELETE FROM mart.bridge_group_page_person 
 WHERE group_page_id in (SELECT group_page_id FROM #old_notes)

 DELETE FROM mart.dim_group_page 
 WHERE group_page_id in (SELECT group_page_id FROM #old_notes)
 
DROP TABLE #old_notes

SELECT group_page_name_resource_key, business_unit_id,COUNT(DISTINCT group_page_code) AS no_of_codes INTO #group_page_codes
FROM mart.dim_group_page
WHERE group_page_name_resource_key is not null
GROUP BY group_page_name_resource_key, business_unit_id HAVING COUNT(DISTINCT group_page_code)>1
IF @@ROWCOUNT>0
BEGIN
	--Make sure all non-custom group_pages have same group_page_code within business_unit, reset all group_page_codes
	 SELECT group_page_name_resource_key, business_unit_id, newid() AS new_id INTO #temp
	 FROM mart.dim_group_page
	 WHERE group_page_name_resource_key is not null
	 GROUP BY group_page_name_resource_key, business_unit_id

	 UPDATE mart.dim_group_page
	 SET group_page_code= new_id
	 FROM #temp
	 INNER JOIN mart.dim_group_page gp 
	 ON gp.group_page_name_resource_key=#temp.group_page_name_resource_key 
	 AND gp.business_unit_id=#temp.business_unit_id
	 WHERE gp.group_page_name_resource_key is not null

	 DROP TABLE #temp
END
DROP TABLE #group_page_codes
