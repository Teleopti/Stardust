UPDATE mart.dim_group_page
SET group_is_custom = 1
WHERE group_page_name_resource_key is null
AND group_is_custom = 0